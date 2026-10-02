using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TamilDivineName.Paratext95
{
    public static class PublicationRenderer
    {
        private static readonly Regex VerseRegex =
            new Regex(@"\\v\s+(?<v>[0-9]+[a-z]?(?:-[0-9]+[a-z]?)?)\s+(?<text>.*?)(?=(?:\\v\s+[0-9])|\z)",
                      RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex NdRegex =
            new Regex(@"\\nd\s+(?<name>.*?)\\nd\*", RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex TamilWordAtEnd =
            new Regex(@"(?<word>[\u0B80-\u0BFF]+)(?<gap>\s*)$", RegexOptions.Compiled);

        public static string RenderChapter(
            string projectShortName,
            string bookCode,
            int chapter,
            string usfm,
            DivineNameRegistry registry,
            string profile)
        {
            List<OccurrenceRecord> records = registry.Occurrences
                .Where(x => x.BookCode == bookCode && x.Chapter == chapter)
                .ToList();

            return VerseRegex.Replace(usfm, delegate(Match verseMatch)
            {
                string verse = verseMatch.Groups["v"].Value;
                string raw = verseMatch.Groups["text"].Value;

                List<OccurrenceRecord> verseRecords = records
                    .Where(x => x.Verse == verse)
                    .OrderByDescending(x => x.Ordinal)
                    .ToList();

                foreach (OccurrenceRecord record in verseRecords)
                    raw = RenderOne(projectShortName, raw, record, profile);

                string prefix = verseMatch.Value.Substring(0, verseMatch.Groups["text"].Index - verseMatch.Index);
                return prefix + raw;
            });
        }

        private static string RenderOne(
            string projectShortName,
            string verseRaw,
            OccurrenceRecord record,
            string profile)
        {
            ProfileRendering target = record.GetProfile(profile);
            if (!target.Approved)
                throw new InvalidOperationException(record.Key + ": " + profile + " is not approved.");
            if (record.Stale)
                throw new InvalidOperationException(record.Key + ": approval is stale.");

            MatchCollection nds = NdRegex.Matches(verseRaw);
            if (record.Ordinal < 1 || record.Ordinal > nds.Count)
                throw new InvalidOperationException(record.Key + ": cannot locate \\nd occurrence by ordinal.");

            Match m = nds[record.Ordinal - 1];
            Group name = m.Groups["name"];

            string currentBase = name.Value;
            string currentRightJoin;
            if (TextUtil.EndsWithJoin(currentBase.Trim(), out currentRightJoin))
                currentBase = currentBase.Trim().Substring(0, currentBase.Trim().Length - currentRightJoin.Length);
            else
                currentRightJoin = "";

            string beforeOccurrence = verseRaw.Substring(0, m.Index);
            string afterOccurrence = verseRaw.Substring(m.Index + m.Length);

            Match prev = TamilWordAtEnd.Match(beforeOccurrence);
            if (prev.Success)
            {
                string previousRaw = prev.Groups["word"].Value;
                string sourceJoin = record.SourceLeftJoin ?? "";
                string previousBase = TextUtil.RemoveJoin(previousRaw, sourceJoin);
                string newPrevious = previousBase + (target.LeftJoin ?? "");

                beforeOccurrence =
                    beforeOccurrence.Substring(0, prev.Groups["word"].Index) +
                    newPrevious +
                    prev.Groups["gap"].Value;
            }

            string newName = (target.Surface ?? "") + (target.RightJoin ?? "");
            string newOccurrence = "\\nd " + newName + "\\nd*";

            return beforeOccurrence + newOccurrence + afterOccurrence;
        }
    }
}
