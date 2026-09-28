using System;
using System.IO;

namespace DeckToolBox.ModCommon
{
    /// <summary>
    /// Steam Deck 路径解析助手
    /// 自动处理 Linux 原生路径、MicroSD 卡挂载点与 Wine 虚拟盘符映射
    /// </summary>
    public static class DeckPathResolver
    {
        public static readonly string DefaultHomeGamesDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Games");

        public static readonly string MicroSdMountBase = "/run/media/deck";

        /// <summary>
        /// 探测当前 MicroSD 卡路径（若存在）
        /// </summary>
        public static string? GetPrimaryMicroSdPath()
        {
            if (Directory.Exists(MicroSdMountBase))
            {
                var dirs = Directory.GetDirectories(MicroSdMountBase);
                if (dirs.Length > 0)
                {
                    return dirs[0];
                }
            }
            return null;
        }

        /// <summary>
        /// 获取独立游戏根目录
        /// </summary>
        public static string GetStandaloneGamesDir()
        {
            var path = Path.Combine(DefaultHomeGamesDir, "standalone_games");
            if (Directory.Exists(path)) return path;

            var sd = GetPrimaryMicroSdPath();
            if (sd != null)
            {
                var sdPath = Path.Combine(sd, "standalone_games");
                if (Directory.Exists(sdPath)) return sdPath;
            }

            return path;
        }
    }
}
