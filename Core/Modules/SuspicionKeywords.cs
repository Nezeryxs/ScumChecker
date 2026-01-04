using System;

namespace ScumChecker.Core.Modules
{
    public static class SuspicionKeywords
    {
        // слова, которые часто встречаются в плохом софте / обходах
        public static readonly string[] Generic =
        [
            "inject", "injector", "injection",
            "bypass", "spoof", "spoofer",
            "mapper", "kdmapper",
            "hook", "hooks",
            "overlay",
            "aim", "aimbot", "trigger", "silent",
            "esp", "wallhack", "radar",
            "macro", "autoclick", "autoclicker",
            "unban", "hwid", "serial"
        ];

        // dev/reverse инструменты (не чит, но может быть “подозрительным контекстом”)
        public static readonly string[] DevTools =
        [
            "dnspy", "ilspy", "x64dbg", "x32dbg", "ollydbg",
            "decompiler", "debugger", "debug"
        ];

        public static bool ContainsAny(string text, string[] list)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            foreach (var k in list)
                if (text.Contains(k, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }
    }
}
