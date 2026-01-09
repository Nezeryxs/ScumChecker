using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ScumChecker.Core;

namespace ScumChecker.Core.Modules
{
    public sealed class ProcessesModule : IScanModule
    {
        public string Name => "Processes (name analysis)";

        public IEnumerable<ScanItem> Run(CancellationToken ct)
        {
            Process[] processes;

            try
            {
                processes = Process.GetProcesses();
            }
            catch
            {
                yield break;
            }

            foreach (var p in processes)
            {
                ct.ThrowIfCancellationRequested();

                string name;
                int pid = -1;
                string? path = null;

                try
                {
                    name = p.ProcessName;
                    pid = p.Id;
                }
                catch
                {
                    continue;
                }

                // Что бы не херачило всё подряд
                if (SuspicionKeywords.ContainsAny(name, SuspicionKeywords.DevTools))
                {
                    yield return new ScanItem
                    {
                        Severity = Severity.Low,
                        Group = FindingGroup.DevTools,
                        Category = "Processes",
                        Title = "Dev / reverse engineering tool",
                        Reason = "Process name matches reverse engineering or debugging tool",
                        Recommendation = "Often legitimate. Use as context only.",
                        Details = $"{name} (PID {pid})"
                    };

                    continue;
                }

                try
                {
                    path = p.MainModule?.FileName;
                }
                catch
                {
                    path = null;
                }

                if (!string.IsNullOrWhiteSpace(path))
                {
                    bool isUnsigned = !SuspicionKeywords.HasValidDigitalSignature(path);
                    bool inUserSpace = SuspicionKeywords.IsUserSpacePath(path);
                    if (isUnsigned && inUserSpace)
                    {
                        var sev = SuspicionKeywords.IsTempPath(path) ? Severity.High : Severity.Medium;
                        yield return new ScanItem
                        {
                            Severity = sev,
                            Group = sev == Severity.High ? FindingGroup.HighRisk : FindingGroup.Suspicious,
                            Category = "Processes",
                            Title = "Unsigned process running from user-space",
                            Reason = "Executable without valid signature is running from user profile or temp location",
                            Recommendation = "Manual review. Confirm source and intent before action.",
                            Details = $"{name} (PID {pid}) — {path}"
                        };
                    }
                }

                // для keywords
                if (SuspicionKeywords.ContainsAny(name, SuspicionKeywords.Generic))
                {
                    yield return new ScanItem
                    {
                        Severity = Severity.Medium,
                        Group = FindingGroup.Suspicious,
                        Category = "Processes",
                        Title = "Suspicious process name",
                        Reason = "Process name contains suspicious keyword",
                        Recommendation = "Manual review. Do not ban by this alone.",
                        Details = $"{name} (PID {pid})"
                    };
                }
            }
        }
    }
}
