using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace TamilDivineName.Paratext95
{
    internal static class TextUtil
    {
        private static readonly string[] JoinSuffixes = { "க்", "ச்", "ட்", "த்", "ப்", "ற்" };

        public static string Normalize(string value)
        {
            return (value ?? "").Normalize(NormalizationForm.FormC);
        }

        public static string Sha256(string value)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value ?? "");
                byte[] hash = sha.ComputeHash(bytes);
                StringBuilder sb = new StringBuilder(hash.Length * 2);
                for (int i = 0; i < hash.Length; i++)
                    sb.Append(hash[i].ToString("x2"));
                return sb.ToString();
            }
        }

        public static string StripNotesAndMarkers(string raw)
        {
            if (raw == null) return "";
            string s = Regex.Replace(raw, @"\\f\b.*?\\f\*", " ", RegexOptions.Singleline);
            s = Regex.Replace(s, @"\\x\b.*?\\x\*", " ", RegexOptions.Singleline);
            s = Regex.Replace(s, @"\\[A-Za-z0-9+\-]+\*?", " ");
            s = Regex.Replace(s, @"\|[^\s\\]+", " ");
            s = Regex.Replace(s, @"\s+", " ");
            return s.Trim();
        }

        public static string FirstTamilLetter(string word)
        {
            string s = Normalize(word);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c >= '\u0B80' && c <= '\u0BFF')
                    return c.ToString();
            }
            return "";
        }

        public static string JoinForInitial(string word)
        {
            string first = FirstTamilLetter(word);
            switch (first)
            {
                case "க": return "க்";
                case "ச": return "ச்";
                case "ட": return "ட்";
                case "த": return "த்";
                case "ப": return "ப்";
                case "ற": return "ற்";
                default: return "";
            }
        }

        public static bool EndsWithJoin(string word, out string join)
        {
            string s = Normalize(word);
            foreach (string item in JoinSuffixes)
            {
                if (s.EndsWith(item, StringComparison.Ordinal))
                {
                    join = item;
                    return true;
                }
            }
            join = "";
            return false;
        }

        public static string RemoveJoin(string word, string join)
        {
            if (string.IsNullOrEmpty(word) || string.IsNullOrEmpty(join)) return word ?? "";
            if (word.EndsWith(join, StringComparison.Ordinal))
                return word.Substring(0, word.Length - join.Length);
            return word;
        }

        public static bool HasHardPunctuationBoundary(string rawBetween)
        {
            if (string.IsNullOrEmpty(rawBetween)) return false;
            return rawBetween.IndexOf(',') >= 0 ||
                   rawBetween.IndexOf(';') >= 0 ||
                   rawBetween.IndexOf(':') >= 0 ||
                   rawBetween.IndexOf('.') >= 0 ||
                   rawBetween.IndexOf('!') >= 0 ||
                   rawBetween.IndexOf('?') >= 0 ||
                   rawBetween.IndexOf('…') >= 0 ||
                   rawBetween.IndexOf('—') >= 0;
        }
    }
}
