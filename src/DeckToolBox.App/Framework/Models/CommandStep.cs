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
/// 一条可复制的操作指令。Host 为 null 表示在本机 Konsole 终端执行；
/// 否则表示指令应在哪里执行/粘贴（见 <see cref="Hosts"/>）。
/// </summary>
public sealed record CommandStep(string Title, string Command, string Note, Risk Risk)
{
    public string? Host { get; init; }

    /// <summary>指定该指令的执行环境，例如 <c>Step.Read(...).On(Hosts.Ubuntu)</c>。</summary>
    public CommandStep On(string host) => this with { Host = host };
}

/// <summary>内容作者使用的简写工厂：Step.Read / Change / Root / Danger。</summary>
public static class Step
{
    public static CommandStep Read(string title, string command, string note = "") => new(title, command, note, Risk.Read);
    public static CommandStep Change(string title, string command, string note = "") => new(title, command, note, Risk.Change);
    public static CommandStep Root(string title, string command, string note = "") => new(title, command, note, Risk.Root);
    public static CommandStep Danger(string title, string command, string note = "") => new(title, command, note, Risk.Danger);
}
