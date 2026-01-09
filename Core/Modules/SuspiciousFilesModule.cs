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
                if (!Directory.Exists(root.Path)) continue;

                foreach (var item in ScanDir(root.Path, 0, ct, root.MaxDepth))
                {
                    yield return item;
                    if (++hits >= MaxHits) yield break;
                }
            }
        }

        private sealed class RootScan
        {
            public RootScan(string path, int maxDepth)
            {
                Path = path;
                MaxDepth = maxDepth;
            }

            public string Path { get; }
            public int MaxDepth { get; }
        }

        private static IEnumerable<RootScan> GetRoots()
        {
            yield return new RootScan(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), MaxDepth);
            yield return new RootScan(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), MaxDepth);
            yield return new RootScan(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 2);

            // ✅ Downloads (нет SpecialFolder.Downloads)
            yield return new RootScan(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads"
            ), MaxDepth);

            yield return new RootScan(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), MaxDepth);
            yield return new RootScan(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), MaxDepth);
            yield return new RootScan(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs"), 3);
            yield return new RootScan(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp"), 2);
            yield return new RootScan(Path.GetTempPath(), 2);

            yield return new RootScan(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 2);
            yield return new RootScan(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), 2);
            yield return new RootScan(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), 2);
        }

        private static IEnumerable<ScanItem> ScanDir(string dir, int depth, CancellationToken ct, int maxDepth)
        {
            if (depth > maxDepth) yield break;
            if (ShouldSkipDirectory(dir)) yield break;

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
                bool isExec = ext is ".exe" or ".dll" or ".sys";
                bool inUserSpace = SuspicionKeywords.IsUserSpacePath(f);
                bool hasSignature = SuspicionKeywords.HasValidDigitalSignature(f);
                bool isCritical = SuspicionKeywords.ContainsCritical($"{name} {f}");
                bool nameSuspicious = !hasSignature
                                      && (isCritical
                                          || SuspicionKeywords.ContainsAny($"{name} {f}", SuspicionKeywords.Generic, SuspicionKeywords.ScumNames));

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
                if (nameSuspicious)
                {
                    // усиливаем риск, если это исполняемое/драйвер и лежит в Temp/AppData
                    var sev = (isExec && inUserSpace) || isCritical ? Severity.High : Severity.Medium;
                    var grp = (sev == Severity.High) ? FindingGroup.HighRisk : FindingGroup.Suspicious;

                    yield return new ScanItem
                    {
                        Severity = sev,
                        Group = grp,
                        Category = "Filesystem",
                        What = isExec ? "Suspicious executable name" : "Suspicious filename",
                        Reason = isExec && inUserSpace
                            ? "Executable/driver-like file in user-space (AppData/Temp) with suspicious keyword"
                            : isCritical
                                ? "Critical keyword detected in filename/path"
                                : "Name contains suspicious keyword (needs manual review)",
                        Recommendation = sev == Severity.High
                            ? "Manual review + ask user. Consider action if confirmed."
                            : "Manual review. Do not ban by this alone.",
                        Details = name,
                        EvidencePath = f
                    };
                }
                else if (isExec && inUserSpace && !hasSignature)
                {
                    var sev = SuspicionKeywords.IsTempPath(f) ? Severity.High : Severity.Medium;
                    yield return new ScanItem
                    {
                        Severity = sev,
                        Group = sev == Severity.High ? FindingGroup.HighRisk : FindingGroup.Suspicious,
                        Category = "Filesystem",
                        What = "Unsigned executable in user-space",
                        Reason = "Unsigned executable/driver-like file running from user profile or temp location",
                        Recommendation = "Manual review. Confirm origin and intent before action.",
                        Details = name,
                        EvidencePath = f
                    };
                }
            }

            foreach (var d in subDirs)
            {
                ct.ThrowIfCancellationRequested();

                if (SuspicionKeywords.IsDirectorySuspicious(d))
                {
                    var name = Path.GetFileName(d);
                    bool isCritical = SuspicionKeywords.ContainsCritical($"{name} {d}");
                    var sev = isCritical ? Severity.High : Severity.Medium;

                    yield return new ScanItem
                    {
                        Severity = sev,
                        Group = sev == Severity.High ? FindingGroup.HighRisk : FindingGroup.Suspicious,
                        Category = "Filesystem",
                        What = "Suspicious folder",
                        Reason = isCritical
                            ? "Critical keyword detected in folder name/path"
                            : "Folder name/path contains suspicious keyword",
                        Recommendation = sev == Severity.High
                            ? "Manual review + ask user. Consider action if confirmed."
                            : "Manual review. Do not ban by this alone.",
                        Details = name,
                        EvidencePath = d
                    };
                }

                foreach (var nested in ScanDir(d, depth + 1, ct, maxDepth))
                    yield return nested;
            }
        }

        private static bool ShouldSkipDirectory(string dir)
        {
            try
            {
                var info = new DirectoryInfo(dir);
                if (info.Attributes.HasFlag(FileAttributes.ReparsePoint)) return true;
                if (info.Attributes.HasFlag(FileAttributes.System)) return true;
            }
            catch
            {
                return true;
            }

            var name = Path.GetFileName(dir.TrimEnd(Path.DirectorySeparatorChar));
            if (string.Equals(name, "$Recycle.Bin", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(name, "System Volume Information", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(name, "Windows", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(name, "WinSxS", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(name, "Program Files", StringComparison.OrdinalIgnoreCase)) return false;
            if (string.Equals(name, "Program Files (x86)", StringComparison.OrdinalIgnoreCase)) return false;

            return false;
        }
    }
}
