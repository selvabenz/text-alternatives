using System;
using System.Collections.Generic;

namespace TamilDivineName.Paratext95
{
    /// <summary>
    /// Deterministic QA/suggestion engine.
    /// It does not publish suggestions. Human approval remains mandatory.
    /// </summary>
    public static class TamilSandhiEngine
    {
        public static string SuggestLeftJoin(
            string previousGrammarTag,
            string selectedNameSurface,
            bool punctuationBoundary,
            bool projectAllowsAccusativeBeforeProperName)
        {
            if (punctuationBoundary) return "";

            if (string.Equals(previousGrammarTag, "ACC", StringComparison.OrdinalIgnoreCase))
            {
                string join = TextUtil.JoinForInitial(selectedNameSurface);
                if (!string.IsNullOrEmpty(join) && projectAllowsAccusativeBeforeProperName)
                    return join;
            }

            return "";
        }

        public static string SuggestRightJoin(
            string divineNameGrammarTag,
            string nextWord,
            bool punctuationBoundary)
        {
            if (punctuationBoundary) return "";

            if (string.Equals(divineNameGrammarTag, "ACC", StringComparison.OrdinalIgnoreCase))
                return TextUtil.JoinForInitial(nextWord);

            // Other rules (e.g. project-approved DAT behavior) intentionally
            // remain manual/configurable in v0.1.
            return "";
        }

        public static List<QaFinding> Check(OccurrenceRecord o, string profileName)
        {
            List<QaFinding> findings = new List<QaFinding>();
            ProfileRendering p = o.GetProfile(profileName);

            if (string.IsNullOrWhiteSpace(p.Surface))
            {
                findings.Add(new QaFinding
                {
                    Key = o.Key,
                    Severity = "ERROR",
                    Message = profileName + ": approved name morphology is missing."
                });
                return findings;
            }

            if (!p.Approved)
            {
                findings.Add(new QaFinding
                {
                    Key = o.Key,
                    Severity = "ERROR",
                    Message = profileName + ": rendering is not editor-approved."
                });
            }

            if (o.Stale)
            {
                findings.Add(new QaFinding
                {
                    Key = o.Key,
                    Severity = "ERROR",
                    Message = "Context changed after approval; re-review this occurrence."
                });
            }

            string suggestedRight = SuggestRightJoin(o.GrammarTag, o.NextWord, false);
            if (!string.IsNullOrEmpty(suggestedRight) &&
                !string.Equals(suggestedRight, p.RightJoin ?? "", StringComparison.Ordinal))
            {
                findings.Add(new QaFinding
                {
                    Key = o.Key,
                    Severity = "WARN",
                    Message = profileName + ": right-boundary suggestion is '" +
                              suggestedRight + "', approved value is '" + (p.RightJoin ?? "") + "'."
                });
            }

            return findings;
        }
    }
}
