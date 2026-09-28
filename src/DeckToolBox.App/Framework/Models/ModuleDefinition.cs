using System.Collections.Generic;

namespace DeckToolBox.App.Framework.Models;

public enum ModuleLevel
{
    Basic,        // 入门：日常使用必会
    Intermediate, // 进阶：排障与定制
    Advanced      // 深入：原理与内核机制
}

/// <summary>
/// 一个知识模块，固定四段式：原理 (Why) → 后果与边界 → 操作步骤 → 验证。
/// 文本支持轻量标记：**加粗**、`代码`；以 "• " 或 "- " 开头的行按列表缩进渲染。
/// </summary>
public sealed class ModuleDefinition
{
    /// <summary>全局唯一，kebab-case。</summary>
    public required string Id { get; init; }

    /// <summary>短标题，用于导航栏与页头（建议 ≤ 18 个汉字）。</summary>
    public required string Title { get; init; }

    /// <summary>关键词，导航栏副标题，例如 "hwmon · TDP · 风扇"。</summary>
    public string Keywords { get; init; } = string.Empty;

    public ModuleLevel Level { get; init; } = ModuleLevel.Intermediate;

    public required string Overview { get; init; }

    public IReadOnlyList<string> Why { get; init; } = [];
    public IReadOnlyList<string> Consequences { get; init; } = [];
    public IReadOnlyList<CommandStep> Steps { get; init; } = [];
    public IReadOnlyList<string> Verify { get; init; } = [];
}
