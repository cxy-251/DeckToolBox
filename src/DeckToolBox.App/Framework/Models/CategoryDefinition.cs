using System.Collections.Generic;

namespace DeckToolBox.App.Framework.Models;

public enum KnowledgeScope
{
    SteamDeckNative,        // 🎮 Steam Deck 本机专精 (SteamOS 3.x 掌机硬件/微合成器/不可变系统)
    UniversalDistros,       // 🐧 广域 Linux 宇宙 (全景包管理与跨发行版生态)
    LinuxKernelInternals    // 🧠 通用内核底石 (跨发行版通用的操作系统底层子系统)
}

public class CategoryDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public KnowledgeScope Scope { get; set; } = KnowledgeScope.SteamDeckNative;
    public List<ModuleDefinition> Modules { get; set; } = new();

    public CategoryDefinition() { }

    public CategoryDefinition(string name, string icon, string description, KnowledgeScope scope, IEnumerable<ModuleDefinition> modules)
    {
        Name = name;
        Icon = icon;
        Description = description;
        Scope = scope;
        Modules.AddRange(modules);
    }
}
