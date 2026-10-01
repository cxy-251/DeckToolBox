namespace DeckToolBox.App.Framework.Models;

/// <summary>指令的风险等级，决定界面上的标记颜色。</summary>
public enum Risk
{
    /// <summary>只读查询，不改变任何状态。</summary>
    Read,
    /// <summary>修改当前用户的文件或设置（$HOME、用户级服务、Steam 启动项），可撤销。</summary>
    Change,
    /// <summary>需要 sudo / root，修改系统级配置。</summary>
    Root,
    /// <summary>删除数据或难以撤销的操作，执行前务必看清。</summary>
    Danger
}

/// <summary>
/// 一条可复制的操作指令。
/// Note 讲「怎么用、参数是什么意思」；Outcome 讲「执行后会发生什么」：屏幕上会看到什么、系统里改变了什么。
/// Host 为 null 表示在本机 Konsole 终端执行；否则表示指令应在哪里执行/粘贴（见 <see cref="Hosts"/>）。
/// </summary>
public sealed record CommandStep(string Title, string Command, string Note, Risk Risk)
{
    public string? Host { get; init; }

    public string Outcome { get; init; } = string.Empty;

    /// <summary>为 false 时 <c>--try-read</c> 不自动试跑（需要交互、持续运行或会打开窗口的只读指令）。</summary>
    public bool AutoRun { get; init; } = true;

    /// <summary>指定该指令的执行环境，例如 <c>Step.Read(...).On(Hosts.Ubuntu)</c>。</summary>
    public CommandStep On(string host) => this with { Host = host };

    /// <summary>说明执行后会发生什么，例如 <c>Step.Read(...).Then("列出……")</c>。</summary>
    public CommandStep Then(string outcome) => this with { Outcome = outcome };

    /// <summary>为 true 时表示这是只在练习目录（如 /tmp/xxx-demo、~/linux-lab）内操作的练习步骤，<c>--try-read</c> 会按顺序执行它，让后面的只读步骤有文件可读。</summary>
    public bool IsPractice { get; init; }

    /// <summary>标记为练习步骤，例如 <c>Step.Change("准备练习文件", ...).Practice()</c>；删除练习目录的 Danger 步骤也可以标记。</summary>
    public CommandStep Practice() => this with { IsPractice = true };

    /// <summary>为 true 时表示这条指令的输出本来就含有错误信息（例如演示权限不足、链接失效），<c>--try-read</c> 不把它当作问题。</summary>
    public bool ExpectsError { get; init; }

    /// <summary>标记为「预期会显示错误」的演示步骤。</summary>
    public CommandStep ShowsError() => this with { ExpectsError = true };

    /// <summary>标记为只能手动执行，例如 <c>Step.Read(...).Manual()</c>；内容检查工具不会自动运行它。</summary>
    public CommandStep Manual() => this with { AutoRun = false };
}

/// <summary>内容作者使用的简写工厂：Step.Read / Change / Root / Danger。</summary>
public static class Step
{
    public static CommandStep Read(string title, string command, string note = "") => new(title, command, note, Risk.Read);
    public static CommandStep Change(string title, string command, string note = "") => new(title, command, note, Risk.Change);
    public static CommandStep Root(string title, string command, string note = "") => new(title, command, note, Risk.Root);
    public static CommandStep Danger(string title, string command, string note = "") => new(title, command, note, Risk.Danger);
}
