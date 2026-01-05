using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScumChecker.Core.Steam
{
    /// <summary>
    /// Steam API helper.
    /// ВНИМАНИЕ: без WebAPI key Steam НЕ даёт полноценный GetPlayerBans.
    /// Поэтому тут "no-key" вариант: парсим публичную страницу профиля (xml=1).
    /// Это даёт только факт "VACBanned: 0/1" и иногда game bans — если Steam отдаёт.
    /// Если профиль закрыт/есть лимиты — вернём Unknown=true.
    /// </summary>
    public static class SteamApi
    {
        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(12)
        };

        public readonly record struct BanLite(
            bool Unknown,
            bool VacBanned,
            int VacBans,
            int GameBans,
            int? DaysSinceLastBan
        )
        {
            public static BanLite UnknownResult() => new(true, false, 0, 0, null);
        }

        /// <summary>
        /// Проверка VAC без apiKey.
        /// Делается запрос: https://steamcommunity.com/profiles/{steamId64}/?xml=1
        /// И парсятся теги:
        ///   <vacBanned>0|1</vacBanned>
        ///   <numberOfVACBans>int</numberOfVACBans> (может отсутствовать)
        ///   <numberOfGameBans>int</numberOfGameBans> (может отсутствовать)
        ///   <daysSinceLastBan>int</daysSinceLastBan> (может отсутствовать)
        /// </summary>
        public static async Task<BanLite> GetBansNoKeyAsync(string steamId64)
        {
            if (string.IsNullOrWhiteSpace(steamId64))
                return BanLite.UnknownResult();

            // Steam иногда режет без User-Agent — поставим нормальный
            if (!_http.DefaultRequestHeaders.UserAgent.TryParseAdd("ScumChecker/1.0 (+WinForms)"))
            {
                // ignore
            }

            var url = $"https://steamcommunity.com/profiles/{steamId64}/?xml=1";

            string xml;
            try
            {
                xml = await _http.GetStringAsync(url).ConfigureAwait(false);
            }
            catch
            {
                return BanLite.UnknownResult();
            }

            if (string.IsNullOrWhiteSpace(xml))
                return BanLite.UnknownResult();

            // если профиль приватный, Steam часто возвращает:
            // <privacyState>private</privacyState> и может не отдавать теги банов
            // поэтому если не нашли vacBanned — считаем Unknown
            var vacBanned = TryTagBool(xml, "vacBanned");
            if (vacBanned is null)
                return BanLite.UnknownResult();

            var vacCount = TryTagInt(xml, "numberOfVACBans") ?? 0;
            var gameBans = TryTagInt(xml, "numberOfGameBans") ?? 0;
            var days = TryTagInt(xml, "daysSinceLastBan");

            return new BanLite(
                Unknown: false,
                VacBanned: vacBanned.Value,
                VacBans: vacCount,
                GameBans: gameBans,
                DaysSinceLastBan: days
            );
        }

        private static bool? TryTagBool(string xml, string tag)
        {
            var v = TryTagString(xml, tag);
            if (v == null) return null;
            v = v.Trim();
            if (v == "1" || v.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
            if (v == "0" || v.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;
            return null;
        }

        private static int? TryTagInt(string xml, string tag)
        {
            var v = TryTagString(xml, tag);
            if (v == null) return null;
            if (int.TryParse(v.Trim(), out var n)) return n;
            return null;
        }

        private static string? TryTagString(string xml, string tag)
        {
            // простой regex для <tag>value</tag>
            var m = Regex.Match(xml, $@"\<{tag}\>\s*(.*?)\s*\</{tag}\>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (!m.Success) return null;
            return m.Groups[1].Value;
        }
    }
}
