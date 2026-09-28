using System.Collections.Generic;

namespace DeckToolBox.App.Framework.Models;

/// <summary>
/// 顶部视点（专区）。专区 → 分类 → 模块 三级结构全部由 ContentRegistry 以数据声明。
/// </summary>
public sealed record KnowledgeScope(
    string Id,
    string Icon,
    string TabLabel,
    string Title,
    string Tagline,
    IReadOnlyList<CategoryDefinition> Categories);

public sealed record CategoryDefinition(
    string Name,
    string Icon,
    string Description,
    IReadOnlyList<ModuleDefinition> Modules);
