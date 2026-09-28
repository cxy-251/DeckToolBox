using System;
using DeckToolBox.ModCommon;

namespace UnityBepInExTemplate
{
    /// <summary>
    /// BepInEx 5.x / 6.x 通用 Unity Mod 插件模板骨架
    /// 在开发具体游戏 Mod 时：
    /// 1. 引入游戏目录中的 UnityEngine.dll、Assembly-CSharp.dll、BepInEx.dll 与 0Harmony.dll
    /// 2. 声明 [BepInPlugin(PluginGuid, PluginName, PluginVersion)] 特性
    /// 3. 在 Awake() 中调用 Harmony.PatchAll() 执行拦截与逻辑重写
    /// </summary>
    public class PluginEntry
    {
        public const string PluginGuid = "com.decktoolbox.mod.template";
        public const string PluginName = "UnityBepInExTemplate";
        public const string PluginVersion = "1.0.0";

        public void Awake()
        {
            ModLogger.Prefix = $"[{PluginName}]";
            ModLogger.Info($"插件 {PluginName} v{PluginVersion} 初始化成功 (SteamOS/Linux Runtime)！");
        }
    }
}
