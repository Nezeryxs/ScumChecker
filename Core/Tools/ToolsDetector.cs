using System;
using System.Collections.Generic;
using System.IO;

namespace ScumChecker.Core.Tools
{
    public static class ToolsDetector
    {
        public static List<ToolEntry> Detect()
        {
            var list = new List<ToolEntry>();

            list.Add(DetectEverything());
            list.Add(DetectShellBagsExplorer());

            return list;
        }

        private static ToolEntry DetectEverything()
        {
            var t = new ToolEntry
            {
                Name = "Everything (Voidtools)",
                DownloadUrl = "https://www.voidtools.com/downloads/"
            };

            var candidates = new List<string>();

            var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            if (!string.IsNullOrWhiteSpace(pf))
                candidates.Add(Path.Combine(pf, "Everything", "Everything.exe"));

            if (!string.IsNullOrWhiteSpace(pf86))
                candidates.Add(Path.Combine(pf86, "Everything", "Everything.exe"));

            // portable часто лежит в Downloads/Desktop
            candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Everything.exe"));
            candidates.Add(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads", "Everything.exe"));

            foreach (var c in candidates)
            {
                if (File.Exists(c))
                {
                    t.Status = "Found";
                    t.Path = c;
                    return t;
                }
            }

            return t;
        }

        // ShellBags Explorer (часто используют утилиту от Eric Zimmerman)
        private static ToolEntry DetectShellBagsExplorer()
        {
            var t = new ToolEntry
            {
                Name = "ShellBags Explorer",
                DownloadUrl = "https://ericzimmerman.github.io/"
            };

            var candidates = new List<string>();

            var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            // типовые (если юзер положил в папку Tools)
            if (!string.IsNullOrWhiteSpace(pf))
                candidates.Add(Path.Combine(pf, "ShellBagsExplorer", "ShellBagsExplorer.exe"));

            if (!string.IsNullOrWhiteSpace(pf86))
                candidates.Add(Path.Combine(pf86, "ShellBagsExplorer", "ShellBagsExplorer.exe"));

            // portable варианты
            candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "ShellBagsExplorer.exe"));
            candidates.Add(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads", "ShellBagsExplorer.exe"));

            foreach (var c in candidates)
            {
                if (File.Exists(c))
                {
                    t.Status = "Found";
                    t.Path = c;
                    return t;
                }
            }

            return t;
        }
    }
}
