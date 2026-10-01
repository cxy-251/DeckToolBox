using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DeckToolBox.App.Framework.Models;

namespace DeckToolBox.App.Framework;

/// <summary>
/// 在本机试跑只读指令：dotnet run -- --try-read [模块Id...] [-v]
/// 每个模块的步骤按顺序在同一个 bash 中执行（cd 与变量会延续），只运行两类步骤：
/// 只读步骤（Risk.Read，本机终端执行，无占位符，未标记 Manual()），以及标记了 Practice() 的练习步骤（只在练习目录内操作）。
/// 其他修改性步骤一律不执行。单个步骤超过时限时结束该模块，报告卡住的步骤。
/// </summary>
public static partial class ReadStepRunner
{
    private static readonly TimeSpan StepTimeout = TimeSpan.FromSeconds(20);
    private const string Marker = "@@DTB@@";

    // 需要用户替换的占位符，如 <AppID>、<卷标>
    [GeneratedRegex(@"<[^<>\s]+>")]
    private static partial Regex PlaceholderPattern();

    // 会打开窗口、等待输入或持续运行的命令，即使是只读的也不适合自动执行
    [GeneratedRegex(@"\b(xdg-open|kde-open|kioclient\s+exec|less|man|vim?|nano|htop|btop|top|watch|evtest|vkcube|vkmark|glxgears|ssh|tmux|gamescope)\b|udevadm\s+monitor|journalctl\b[^|;]*\s-f\b|tail\s+-f|steam\s+steam://|\bping\b(?![^|;]*-c)")]
    private static partial Regex InteractivePattern();

    // 输出中出现这些文字时提示人工确认（命令可能写错，或依赖的文件不存在）
    [GeneratedRegex(@"command not found|未找到命令|No such file|没有那个文件|Permission denied|权限不够|syntax error|语法错误|invalid option|无效的选项|unrecognized option|无法识别的选项|Unknown option|超过 15 个字符")]
    private static partial Regex ErrorTextPattern();

    [GeneratedRegex(Marker + @" (BEGIN|END) (\d+)(?: (\d+))?")]
    private static partial Regex MarkerPattern();

    private sealed class StepResult
    {
        public required CommandStep Step;
        public readonly StringBuilder Output = new();
        public int? ExitCode;
        public bool TimedOut;
    }

    public static int RunCli(IReadOnlyList<KnowledgeScope> scopes, IReadOnlyList<string> args)
    {
        var verbose = args.Contains("-v");
        var ids = args.Where(a => a != "-v").ToHashSet();
        var modules = scopes.SelectMany(s => s.Categories).SelectMany(c => c.Modules)
            .Where(m => ids.Count == 0 || ids.Contains(m.Id)).ToList();
        if (modules.Count == 0)
        {
            Console.WriteLine($"✗ 找不到模块：{string.Join(", ", ids)}");
            return 1;
        }

        int ok = 0, warn = 0, fail = 0, notRun = 0;
        foreach (var m in modules)
        {
            var steps = m.Steps.Where(s => s.IsPractice || IsRunnableRead(s)).ToList();
            notRun += m.Steps.Count(s => s.Risk == Risk.Read && !IsRunnableRead(s));
            if (!steps.Any(IsRunnableRead)) continue;

            var results = RunModule(steps);
            var lines = new List<string>();
            foreach (var r in results)
            {
                var firstError = r.Step.ExpectsError ? null
                    : r.Output.ToString().Split('\n').FirstOrDefault(l => ErrorTextPattern().IsMatch(l));
                var failed = r.TimedOut || r.ExitCode is null
                    || (!r.Step.ExpectsError && r.ExitCode is 2 or 126 or 127 or >= 128);
                var detail = r.TimedOut ? $"超过 {StepTimeout.TotalSeconds:0} 秒未结束"
                    : r.ExitCode is null ? "未执行（前面的步骤超时）" : $"退出码 {r.ExitCode}";
                var kind = r.Step.IsPractice ? "练习步骤 " : "";

                if (failed) { fail++; lines.Add($"  ✗ {kind}{r.Step.Title}：{detail}{(firstError is null ? "" : "　" + Trim(firstError))}"); }
                else if (firstError is not null) { warn++; lines.Add($"  ⚠ {kind}{r.Step.Title}：{Trim(firstError)}"); }
                else
                {
                    ok++;
                    if (verbose) lines.Add($"  ✓ {kind}{r.Step.Title}{(r.ExitCode == 0 ? "" : $"（退出码 {r.ExitCode}）")}");
                }

                if (verbose)
                    lines.AddRange(r.Output.ToString().Split('\n').Where(l => l.Length > 0).Take(8).Select(l => $"      │ {Trim(l)}"));
            }

            if (lines.Count > 0)
            {
                Console.WriteLine($"## {m.Id}  {m.Title}");
                lines.ForEach(Console.WriteLine);
            }
        }

        Console.WriteLine($"试跑 {ok + warn + fail} 条：✓ {ok}  ⚠ {warn}  ✗ {fail}（另有 {notRun} 条只读指令需替换占位符、需交互或不在本机执行，未试跑）");
        Console.WriteLine("⚠ 表示输出中有错误信息，可能与本机状态有关，请人工确认；✗ 表示命令执行失败。修改性步骤只运行标记了 Practice() 的练习步骤。");
        return fail == 0 ? 0 : 1;
    }

    private static bool IsRunnableRead(CommandStep s) =>
        s.Risk == Risk.Read && s.Host is null && s.AutoRun
        && !PlaceholderPattern().IsMatch(s.Command)
        && !InteractivePattern().IsMatch(s.Command);

    private static List<StepResult> RunModule(List<CommandStep> steps)
    {
        var results = steps.Select(s => new StepResult { Step = s }).ToList();

        // 所有步骤写进同一个脚本，用标记行分隔；每步之前回到家目录之外的状态都会延续（与用户依次粘贴执行一致）
        var script = new StringBuilder("exec 2>&1 </dev/null\ncd ~\n");
        for (var i = 0; i < steps.Count; i++)
            script.Append($"printf '\\n{Marker} BEGIN {i}\\n'\n{steps[i].Command}\nprintf '\\n{Marker} END {i} %d\\n' $?\n");

        // .NET 启动的子进程会继承「忽略 SIGPIPE」，管道接 head 时会报「断开的管道」；用 env 恢复默认处理
        var psi = new ProcessStartInfo("env")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var a in new[] { "--default-signal=PIPE", "bash", "-c", script.ToString() }) psi.ArgumentList.Add(a);

        var gate = new object();
        var current = -1;
        var stepStarted = DateTime.UtcNow;

        using var p = new Process { StartInfo = psi };
        void OnLine(string? line)
        {
            if (line is null) return;
            lock (gate)
            {
                var m = MarkerPattern().Match(line);
                if (m.Success)
                {
                    var i = int.Parse(m.Groups[2].Value);
                    if (m.Groups[1].Value == "BEGIN") { current = i; stepStarted = DateTime.UtcNow; }
                    else { results[i].ExitCode = int.Parse(m.Groups[3].Value); current = -1; }
                }
                else if (current >= 0)
                    results[current].Output.AppendLine(line);
            }
        }
        p.OutputDataReceived += (_, e) => OnLine(e.Data);
        p.ErrorDataReceived += (_, e) => OnLine(e.Data);
        p.Start();
        p.StandardInput.Close();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();

        while (!p.WaitForExit(200))
        {
            lock (gate)
            {
                if (current < 0 || DateTime.UtcNow - stepStarted <= StepTimeout) continue;
                results[current].TimedOut = true;
            }
            try { p.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
            break;
        }
        p.WaitForExit();
        Thread.Sleep(50);
        return results;
    }

    private static string Trim(string line)
    {
        line = line.Trim();
        return line.Length <= 100 ? line : line[..100] + "…";
    }
}
