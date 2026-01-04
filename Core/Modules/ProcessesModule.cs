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
