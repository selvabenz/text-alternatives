using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TamilDivineName.Paratext95
{
    public static class UsfmOccurrenceScanner
    {
        private static readonly Regex VerseRegex =
            new Regex(@"\\v\s+(?<v>[0-9]+[a-z]?(?:-[0-9]+[a-z]?)?)\s+(?<text>.*?)(?=(?:\\v\s+[0-9])|\z)",
                      RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex NdRegex =
            new Regex(@"\\nd\s+(?<name>.*?)\\nd\*", RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex LexicalFallbackRegex =
            new Regex(@"(?<name>கர்த்த(?:ர்|ரை|ருக்கு|ருடைய|ரின்|ரால்|ரிடம்|ரிடத்தில்|ரே|ராகிய|ருக்குள்)[\u0B80-\u0BFF]*|கர்த்தாவே|யாவே[\u0B80-\u0BFF]*|யெகோவா[\u0B80-\u0BFF]*)",
                      RegexOptions.Compiled);

        private static readonly Regex TamilWordRegex =
            new Regex(@"[\u0B80-\u0BFF]+", RegexOptions.Compiled);

        public static List<OccurrenceRecord> ScanChapter(
            string projectShortName,
            string bookCode,
            int chapter,
            string usfm,
            bool lexicalFallback)
        {
            List<OccurrenceRecord> result = new List<OccurrenceRecord>();
            if (string.IsNullOrEmpty(usfm)) return result;

            foreach (Match verseMatch in VerseRegex.Matches(usfm))
            {
                string verse = verseMatch.Groups["v"].Value;
                string verseRaw = verseMatch.Groups["text"].Value;
                List<Match> matches = new List<Match>();

                foreach (Match m in NdRegex.Matches(verseRaw))
                    matches.Add(m);

                if (lexicalFallback && matches.Count == 0)
                {
                    foreach (Match m in LexicalFallbackRegex.Matches(verseRaw))
                        matches.Add(m);
                }

                int ordinal = 0;
                foreach (Match match in matches)
                {
                    ordinal++;

                    Group nameGroup = match.Groups["name"];
                    string rawName = nameGroup.Success ? nameGroup.Value : match.Value;
                    int nameStart = nameGroup.Success ? nameGroup.Index : match.Index;
                    string sourceRightJoin;
                    string baseName = SplitRightJoin(rawName, out sourceRightJoin);

                    string previous = PreviousTamilWord(verseRaw, match.Index);
                    string next = NextTamilWord(verseRaw, match.Index + match.Length);

                    string sourceLeftJoin = InferSourceLeftJoin(previous, baseName);
                    string previousBase = TextUtil.RemoveJoin(previous, sourceLeftJoin);

                    OccurrenceRecord o = new OccurrenceRecord();
                    o.BookCode = bookCode;
                    o.Chapter = chapter;
                    o.Verse = verse;
                    o.Ordinal = ordinal;
                    o.Key = bookCode + "." + chapter + "." + verse + ".YHWH." + ordinal;
                    o.PreviousWord = previousBase;
                    o.SourceName = baseName;
                    o.NextWord = next;
                    o.SourceLeftJoin = sourceLeftJoin;
                    o.SourceRightJoin = sourceRightJoin;
                    o.RuntimeNameStart = nameStart;
                    o.RuntimeNameLength = rawName.Length;
                    o.RuntimeOccurrenceStart = match.Index;

                    SeedSourceProfile(o, baseName, sourceLeftJoin, sourceRightJoin);

                    o.ContextHash = BuildContextHash(projectShortName, o);
                    result.Add(o);
                }
            }

            return result;
        }

        public static string BuildContextHash(string projectShortName, OccurrenceRecord o)
        {
            return TextUtil.Sha256(
                projectShortName + "|" + o.BookCode + "|" + o.Chapter + "|" +
                o.Verse + "|" + o.Ordinal + "|" + o.PreviousWord + "|" +
                o.SourceName + "|" + o.NextWord);
        }

        private static void SeedSourceProfile(
            OccurrenceRecord o,
            string baseName,
            string leftJoin,
            string rightJoin)
        {
            ProfileRendering p = null;
            if (baseName.StartsWith("கர்த்த", StringComparison.Ordinal))
                p = o.Karthar;
            else if (baseName.StartsWith("யாவே", StringComparison.Ordinal))
                p = o.Yahweh;
            else if (baseName.StartsWith("யெகோவா", StringComparison.Ordinal))
                p = o.Jehovah;

            if (p != null)
            {
                p.Surface = baseName;
                p.LeftJoin = leftJoin;
                p.RightJoin = rightJoin;
                // Detection is not editorial approval.
                p.Approved = false;
            }
        }

        private static string SplitRightJoin(string rawName, out string join)
        {
            string s = TextUtil.Normalize((rawName ?? "").Trim());
            if (TextUtil.EndsWithJoin(s, out join))
                return s.Substring(0, s.Length - join.Length);
            join = "";
            return s;
        }

        private static string InferSourceLeftJoin(string previousWord, string name)
        {
            if (string.IsNullOrEmpty(previousWord) || string.IsNullOrEmpty(name)) return "";
            string expected = TextUtil.JoinForInitial(name);
            if (!string.IsNullOrEmpty(expected) &&
                previousWord.EndsWith(expected, StringComparison.Ordinal))
                return expected;
            return "";
        }

        private static string PreviousTamilWord(string raw, int beforeIndex)
        {
            if (beforeIndex <= 0) return "";
            string left = raw.Substring(0, beforeIndex);
            left = Regex.Replace(left, @"\\[A-Za-z0-9+\-]+\*?\s*$", "");
            MatchCollection ms = TamilWordRegex.Matches(left);
            return ms.Count == 0 ? "" : ms[ms.Count - 1].Value;
        }

        private static string NextTamilWord(string raw, int afterIndex)
        {
            if (afterIndex >= raw.Length) return "";
            string right = raw.Substring(afterIndex);
            Match m = TamilWordRegex.Match(right);
            return m.Success ? m.Value : "";
        }
    }
}
