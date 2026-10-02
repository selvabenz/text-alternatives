using System;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace TamilDivineName.Paratext95
{
    public static class RegistryStore
    {
        private static readonly JavaScriptSerializer Serializer = new JavaScriptSerializer
        {
            MaxJsonLength = int.MaxValue,
            RecursionLimit = 100
        };

        public static string GetRegistryPath(string projectShortName)
        {
            string baseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TamilDivineName",
                "registries");

            Directory.CreateDirectory(baseDir);
            return Path.Combine(baseDir, Sanitize(projectShortName) + ".json");
        }

        public static DivineNameRegistry Load(string projectShortName)
        {
            string path = GetRegistryPath(projectShortName);
            if (!File.Exists(path))
            {
                return new DivineNameRegistry
                {
                    ProjectShortName = projectShortName,
                    UpdatedUtc = DateTime.UtcNow
                };
            }

            string json = File.ReadAllText(path);
            DivineNameRegistry r = Serializer.Deserialize<DivineNameRegistry>(json);
            if (r.Occurrences == null)
                r.Occurrences = new System.Collections.Generic.List<OccurrenceRecord>();
            return r;
        }

        public static void Save(DivineNameRegistry registry)
        {
            registry.UpdatedUtc = DateTime.UtcNow;
            string path = GetRegistryPath(registry.ProjectShortName);
            string json = Serializer.Serialize(registry);
            File.WriteAllText(path, PrettyJson(json));
        }

        public static void MergeScan(DivineNameRegistry registry, System.Collections.Generic.IEnumerable<OccurrenceRecord> scanned)
        {
            foreach (OccurrenceRecord incoming in scanned)
            {
                OccurrenceRecord existing = registry.Occurrences.FirstOrDefault(x => x.Key == incoming.Key);
                if (existing == null)
                {
                    registry.Occurrences.Add(incoming);
                    continue;
                }

                if (!string.Equals(existing.ContextHash, incoming.ContextHash, StringComparison.Ordinal))
                {
                    existing.Stale = true;
                    existing.PreviousWord = incoming.PreviousWord;
                    existing.SourceName = incoming.SourceName;
                    existing.NextWord = incoming.NextWord;
                    existing.SourceLeftJoin = incoming.SourceLeftJoin;
                    existing.SourceRightJoin = incoming.SourceRightJoin;
                    existing.ContextHash = incoming.ContextHash;
                    existing.Karthar.Approved = false;
                    existing.Yahweh.Approved = false;
                    existing.Jehovah.Approved = false;
                }
            }
        }

        private static string Sanitize(string s)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');
            return string.IsNullOrWhiteSpace(s) ? "unknown-project" : s;
        }

        private static string PrettyJson(string json)
        {
            // Keep dependencies minimal. JSON remains machine-readable;
            // insert simple line breaks for easier inspection.
            return json.Replace("},{", "},\r\n{");
        }
    }
}
