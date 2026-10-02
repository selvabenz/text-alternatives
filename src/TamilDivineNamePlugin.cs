using System;
using System.Collections.Generic;
using Paratext.PluginInterfaces;

namespace TamilDivineName.Paratext95
{
    public sealed class TamilDivineNamePlugin : IParatextWindowPlugin
    {
        public const string PluginTitle = "Tamil Divine Name";

        public string Name { get { return PluginTitle; } }
        public Version Version { get { return new Version(0, 1, 0); } }
        public string VersionString { get { return Version.ToString(); } }
        public string Publisher { get { return "BCS Text Integration"; } }

        public string GetDescription(string locale)
        {
            return "Boundary-aware Tamil YHWH profile manager for Paratext 9.5.";
        }

        public IEnumerable<WindowPluginMenuEntry> PluginMenuEntries
        {
            get
            {
                yield return new WindowPluginMenuEntry(
                    "Tamil Divine Name...",
                    Run,
                    PluginMenuLocation.ScrTextTools);
            }
        }

        public IDataFileMerger GetMerger(IPluginHost host, string dataIdentifier)
        {
            throw new NotImplementedException();
        }

        private static void Run(IWindowPluginHost host, IParatextChildState windowState)
        {
            host.ShowEmbeddedUi(new DivineNameControl(), windowState.Project);
        }
    }
}
