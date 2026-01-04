using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using ScumChecker.Core;

namespace ScumChecker.Core.Modules
{
    public sealed class SteamAccountsModule : IScanModule
    {
        public string Name => "Steam (loginusers.vdf)";

        public IEnumerable<ScanItem> Run(CancellationToken ct)
        {
            var path = @"C:\Program Files (x86)\Steam\config\loginusers.vdf";
            if (!File.Exists(path))
                yield break;

            string text = "";
            ScanItem? errorItem = null;

            try
            {
                text = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                errorItem = new ScanItem
                {
                    Severity = Severity.Low,
                    Group = FindingGroup.SystemInfo,
                    Category = "Steam",
                    Title = "Read error",
                    Reason = "Could not read loginusers.vdf",
                    Recommendation = "Run as admin or check permissions.",
                    Details = ex.Message,
                    EvidencePath = path
                };
            }

            if (errorItem != null)
            {
                yield return errorItem;
                yield break;
            }

            // Блоки пользователей: "7656...." { ... }
            foreach (Match m in Regex.Matches(text, "\"(?<id>7656\\d{13,17})\"\\s*\\{(?<body>[\\s\\S]*?)\\}", RegexOptions.Multiline))
            {
                ct.ThrowIfCancellationRequested();

                var id = m.Groups["id"].Value;
                var body = m.Groups["body"].Value;

                string account = GetVdfValue(body, "AccountName");
                string persona = GetVdfValue(body, "PersonaName");
                string mostRecent = GetVdfValue(body, "MostRecent");
                string timestamp = GetVdfValue(body, "Timestamp");

                yield return new ScanItem
                {
                    Severity = Severity.Info,
                    Group = FindingGroup.SystemInfo,
                    Category = "Steam",
                    What = "Steam account",
                    Reason = "Found in Steam loginusers.vdf (local logins history)",
                    Recommendation = "Open profile to review.",
                    Details = $"{persona} ({account}) | MostRecent={mostRecent} | Timestamp={timestamp}",
                    EvidencePath = path,
                    Url = "https://steamcommunity.com/profiles/" + id
                };
            }
        }

        private static string GetVdfValue(string body, string key)
        {
            var mm = Regex.Match(body, $"\"{Regex.Escape(key)}\"\\s*\"(?<v>.*?)\"", RegexOptions.Multiline);
            return mm.Success ? mm.Groups["v"].Value : "";
        }
    }
}
