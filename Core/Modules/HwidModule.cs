using System.Collections.Generic;
using System.Management;
using System.Threading;

namespace ScumChecker.Core.Modules
{
    public sealed class HwidModule : IScanModule
    {
        public string Name => "HWID (basic)";

        public IEnumerable<ScumChecker.Core.ScanItem> Run(CancellationToken ct)
        {
            string cpu = "";
            string bios = "";
            string disk = "";

            try
            {
                using (var s1 = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                    foreach (var o in s1.Get())
                    {
                        ct.ThrowIfCancellationRequested();
                        cpu = o["ProcessorId"]?.ToString() ?? "";
                        break;
                    }

                using (var s2 = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS"))
                    foreach (var o in s2.Get())
                    {
                        ct.ThrowIfCancellationRequested();
                        bios = o["SerialNumber"]?.ToString() ?? "";
                        break;
                    }

                using (var s3 = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_PhysicalMedia"))
                    foreach (var o in s3.Get())
                    {
                        ct.ThrowIfCancellationRequested();
                        disk = o["SerialNumber"]?.ToString() ?? "";
                        if (!string.IsNullOrWhiteSpace(disk)) break;
                    }
            }
            catch
            {
                // WMI может быть отключен (он тут пофану)
            }

            yield return new ScumChecker.Core.ScanItem
            {
                Severity = ScumChecker.Core.Severity.Info,
                Category = "System",
                Title = "HWID (fingerprint)",
                Details = $"CPU: {cpu} | BIOS: {bios} | DISK: {disk}"
            };
        }
    }
}
