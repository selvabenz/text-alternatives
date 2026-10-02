using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace TamilDivineName.Paratext95
{
    public sealed class DivineNameRegistry
    {
        public string SchemaVersion { get; set; }
        public string ProjectShortName { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public List<OccurrenceRecord> Occurrences { get; set; }

        public DivineNameRegistry()
        {
            SchemaVersion = "0.1.0";
            Occurrences = new List<OccurrenceRecord>();
        }
    }

    public sealed class OccurrenceRecord
    {
        public string Key { get; set; }
        public string BookCode { get; set; }
        public int Chapter { get; set; }
        public string Verse { get; set; }
        public int Ordinal { get; set; }

        public string PreviousWord { get; set; }
        public string SourceName { get; set; }
        public string NextWord { get; set; }

        public string GrammarTag { get; set; }
        public string SourceLeftJoin { get; set; }
        public string SourceRightJoin { get; set; }
        public string ContextHash { get; set; }
        public bool Stale { get; set; }
        public string Notes { get; set; }

        public ProfileRendering Karthar { get; set; }
        public ProfileRendering Yahweh { get; set; }
        public ProfileRendering Jehovah { get; set; }

        [ScriptIgnore]
        public int RuntimeNameStart { get; set; }

        [ScriptIgnore]
        public int RuntimeNameLength { get; set; }

        [ScriptIgnore]
        public int RuntimeOccurrenceStart { get; set; }

        public OccurrenceRecord()
        {
            GrammarTag = "OTHER";
            Karthar = new ProfileRendering();
            Yahweh = new ProfileRendering();
            Jehovah = new ProfileRendering();
        }

        public ProfileRendering GetProfile(string profile)
        {
            if (string.Equals(profile, "karthar", StringComparison.OrdinalIgnoreCase)) return Karthar;
            if (string.Equals(profile, "yahweh", StringComparison.OrdinalIgnoreCase)) return Yahweh;
            if (string.Equals(profile, "jehovah", StringComparison.OrdinalIgnoreCase)) return Jehovah;
            throw new ArgumentException("Unknown profile: " + profile);
        }
    }

    public sealed class ProfileRendering
    {
        public string Surface { get; set; }
        public string LeftJoin { get; set; }
        public string RightJoin { get; set; }
        public bool Approved { get; set; }

        public ProfileRendering()
        {
            Surface = "";
            LeftJoin = "";
            RightJoin = "";
            Approved = false;
        }
    }

    public sealed class QaFinding
    {
        public string Key { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return Severity + " | " + Key + " | " + Message;
        }
    }
}
