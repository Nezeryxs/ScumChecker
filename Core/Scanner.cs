using System;
using System.Threading;
using ScumChecker.Core.Modules;
using ScumChecker.Core.Modules;

namespace ScumChecker.Core
{
    public sealed class Scanner
    {
        public event Action<ProgressInfo>? Progress;
        public event Action<string>? Log;
        public event Action<ScanItem>? ItemFound;

        private void EmitProgress(int percent, string stage) =>
            Progress?.Invoke(new ProgressInfo { Percent = Math.Clamp(percent, 0, 100), Stage = stage });

        private void EmitLog(string text) => Log?.Invoke(text);

        private void EmitItem(ScanItem item) => ItemFound?.Invoke(item);

        public ScanResult Run(CancellationToken ct)
        {
            var result = new ScanResult();

            // модули типооооо 
            IScanModule[] modules =
            [
                new HwidModule(),
                new SteamAccountsModule(),
                new ProcessesModule(),
                new SuspiciousFilesModule(),
            ];

            EmitLog($"Modules: {modules.Length}");

            for (int i = 0; i < modules.Length; i++)
            {
                ct.ThrowIfCancellationRequested();

                var m = modules[i];
                var basePct = (int)(i * 100.0 / modules.Length);
                EmitProgress(basePct, m.Name);
                EmitLog($"Running: {m.Name}");

                foreach (var item in m.Run(ct))
                {
                    result.Items.Add(item);
                    EmitItem(item);
                }
            }

            EmitProgress(100, "Done");
            EmitLog("Scan completed.");
            return result;
        }
    }
}
