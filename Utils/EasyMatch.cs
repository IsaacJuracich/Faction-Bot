using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace orbitFactionBot.Utils {
    class EasyMatch {
        public string chars;
        public string chars1;
        public EasyMatch(string chars, string chars1) {
            this.chars = chars;
            this.chars1 = chars1;
        }
        public List<string> match(string text, string pattern) {
            List<string> matches = new List<string>();
            var split = text.Split(' ');
            var split2 = pattern.Split(' ');
            var txt = split.Length > split2.Length ? split : split2;
            var pat = txt == split2 ? split : split2;
            var difference = txt.Except(pat).ToArray();
            var difference1 = pat.Except(txt).ToArray();
            foreach (var vara in difference.Zip(difference1, (d, d1) => new { difference = d, difference1 = d1 })) {
                if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text)) break;
                if (Regex.Replace(text, @"\s", "").Length == 0) break;
                if (text.Replace("  ", "").Length == 0) break;
                if (text.StartsWith(" ") || text.Length == 0 || text.Replace(" ", "").Length == 0) break;
                try {
                    matches.Add($"{vara.difference.Replace(chars, "").Replace(chars1, "")}|" + vara.difference.Replace(vara.difference, vara.difference1));
                }
                catch (Exception e) {
                }
            }
            return matches;
        }
        public static bool isMatchValid(string text, EasyMatch matcher) {
            if (!text.Contains(matcher.chars) && !text.Contains(matcher.chars1)) return true;
            return false;
        }
    }
}
