using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ScumChecker.Core;

namespace ScumChecker.Core.Modules
{
    public sealed class SuspiciousFilesModule : IScanModule
    {
        public string Name => "Filesystem (scan)";

        private const int MaxDepth = 5;
        private const int MaxHits = 300;

        public IEnumerable<ScanItem> Run(CancellationToken ct)
        {
            var roots = GetRoots();
            int hits = 0;

            foreach (var root in roots)
            {
                ct.ThrowIfCancellationRequested();
                if (!Directory.Exists(root)) continue;

                foreach (var item in ScanDir(root, 0, ct))
                {
                    yield return item;
                    if (++hits >= MaxHits) yield break;
                }
            }
        }

        private static IEnumerable<string> GetRoots()
        {
            yield return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            yield return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // ✅ Downloads (нет SpecialFolder.Downloads)
            yield return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads"
            );

            yield return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            yield return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            yield return Path.GetTempPath();
        }

        private static IEnumerable<ScanItem> ScanDir(string dir, int depth, CancellationToken ct)
        {
            if (depth > MaxDepth) yield break;

            IEnumerable<string> subDirs;
            IEnumerable<string> files;

            try { subDirs = Directory.EnumerateDirectories(dir); }
            catch { yield break; }

            try { files = Directory.EnumerateFiles(dir); }
            catch { files = Array.Empty<string>(); }

            foreach (var f in files)
            {
                ct.ThrowIfCancellationRequested();

                var name = Path.GetFileName(f);
                var ext = Path.GetExtension(f).ToLowerInvariant();

                // devtools (dnSpy и т.п.)
                if (SuspicionKeywords.ContainsAny(name, SuspicionKeywords.DevTools))
                {
                    yield return new ScanItem
                    {
                        Severity = Severity.Low,
                        Group = FindingGroup.DevTools,
                        Category = "Filesystem",
                        What = "Dev / reverse tool file",
                        Reason = "Reverse engineering/debug tooling detected (often legitimate)",
                        Recommendation = "No ban. Use as context only.",
                        Details = name,
                        EvidencePath = f
                    };
                    continue;
                }

                // generic suspicious keywords
                if (SuspicionKeywords.ContainsAny(name, SuspicionKeywords.Generic))
                {
                    // усиливаем риск, если это исполняемое/драйвер и лежит в Temp/AppData
                    bool isExec = ext is ".exe" or ".dll" or ".sys";
                    bool inUserSpace = f.Contains(@"\AppData\", StringComparison.OrdinalIgnoreCase)
                                       || f.Contains(@"\Temp\", StringComparison.OrdinalIgnoreCase);

                    var sev = (isExec && inUserSpace) ? Severity.High : Severity.Medium;
                    var grp = (sev == Severity.High) ? FindingGroup.HighRisk : FindingGroup.Suspicious;

                    yield return new ScanItem
                    {
                        Severity = sev,
                        Group = grp,
                        Category = "Filesystem",
                        What = isExec ? "Suspicious executable name" : "Suspicious filename",
                        Reason = isExec && inUserSpace
                            ? "Executable/driver-like file in user-space (AppData/Temp) with suspicious keyword"
                            : "Name contains suspicious keyword (needs manual review)",
                        Recommendation = sev == Severity.High
                            ? "Manual review + ask user. Consider action if confirmed."
                            : "Manual review. Do not ban by this alone.",
                        Details = name,
                        EvidencePath = f
                    };
                }
            }

            foreach (var d in subDirs)
            {
                ct.ThrowIfCancellationRequested();
                foreach (var nested in ScanDir(d, depth + 1, ct))
                    yield return nested;
            }
        }
    }
}
