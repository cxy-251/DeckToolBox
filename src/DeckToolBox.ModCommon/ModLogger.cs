using System;

namespace DeckToolBox.ModCommon
{
    /// <summary>
    /// 统一的跨平台 Mod 日志辅助类
    /// 支持标准控制台输出与前缀格式化，兼容 Linux 终端与 Wine 调试控制台
    /// </summary>
    public static class ModLogger
    {
        public static string Prefix { get; set; } = "[DeckToolBox]";

        public static void Info(string message)
        {
            Console.WriteLine($"{Prefix} [INFO] {message}");
        }

        public static void Warn(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{Prefix} [WARN] {message}");
            Console.ResetColor();
        }

        public static void Error(string message, Exception? ex = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{Prefix} [ERROR] {message}");
            if (ex != null)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ResetColor();
        }
    }
}
