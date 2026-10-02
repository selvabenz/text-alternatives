using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Paratext.PluginInterfaces;

namespace TamilDivineName.Paratext95
{
    public sealed class DivineNameControl : EmbeddedPluginControl
    {
        private IProject _project;
        private IVerseRef _verseRef;
        private DivineNameRegistry _registry;

        private readonly Label _projectLabel;
        private readonly DataGridView _grid;
        private readonly TextBox _qa;
        private readonly CheckBox _lexicalFallback;
        private readonly ComboBox _profile;

        public DivineNameControl()
        {
            Dock = DockStyle.Fill;

            FlowLayoutPanel top = new FlowLayoutPanel();
            top.Dock = DockStyle.Top;
            top.AutoSize = true;
            top.WrapContents = true;

            _projectLabel = new Label { AutoSize = true, Padding = new Padding(4, 8, 10, 4) };
            _lexicalFallback = new CheckBox { Text = "Lexical fallback", AutoSize = true, Padding = new Padding(4, 7, 4, 4) };
            _profile = new ComboBox { Width = 110, DropDownStyle = ComboBoxStyle.DropDownList };
            _profile.Items.AddRange(new object[] { "karthar", "yahweh", "jehovah" });
            _profile.SelectedIndex = 0;

            Button scanChapter = new Button { Text = "Scan Chapter", AutoSize = true };
            Button scanAll = new Button { Text = "Scan Whole Bible", AutoSize = true };
            Button save = new Button { Text = "Save Registry", AutoSize = true };
            Button qa = new Button { Text = "Run QA", AutoSize = true };
            Button export = new Button { Text = "Export Profile", AutoSize = true };

            scanChapter.Click += delegate { ScanCurrentChapter(); };
            scanAll.Click += delegate { ScanWholeBible(); };
            save.Click += delegate { SaveGridToRegistry(); RegistryStore.Save(_registry); ShowStatus("Registry saved."); };
            qa.Click += delegate { SaveGridToRegistry(); RunQa(); };
            export.Click += delegate { SaveGridToRegistry(); ExportProfile(); };

            top.Controls.Add(_projectLabel);
            top.Controls.Add(_lexicalFallback);
            top.Controls.Add(new Label { Text = "Profile:", AutoSize = true, Padding = new Padding(4, 8, 0, 4) });
            top.Controls.Add(_profile);
            top.Controls.Add(scanChapter);
            top.Controls.Add(scanAll);
            top.Controls.Add(save);
            top.Controls.Add(qa);
            top.Controls.Add(export);

            _grid = new DataGridView();
            _grid.Dock = DockStyle.Fill;
            _grid.AutoGenerateColumns = false;
            _grid.AllowUserToAddRows = false;
            _grid.RowHeadersVisible = false;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

            AddTextColumn("Key", "Key", true, 150);
            AddTextColumn("Previous", "PreviousWord", true, 90);
            AddTextColumn("Source", "SourceName", true, 100);
            AddTextColumn("Next", "NextWord", true, 100);
            AddTextColumn("Grammar", "GrammarTag", false, 70);

            AddTextColumn("Karthar", "KartharSurface", false, 100);
            AddTextColumn("K-L", "KartharLeft", false, 45);
            AddTextColumn("K-R", "KartharRight", false, 45);
            AddCheckColumn("K ✓", "KartharApproved");

            AddTextColumn("Yahweh", "YahwehSurface", false, 100);
            AddTextColumn("Y-L", "YahwehLeft", false, 45);
            AddTextColumn("Y-R", "YahwehRight", false, 45);
            AddCheckColumn("Y ✓", "YahwehApproved");

            AddTextColumn("Jehovah", "JehovahSurface", false, 100);
            AddTextColumn("J-L", "JehovahLeft", false, 45);
            AddTextColumn("J-R", "JehovahRight", false, 45);
            AddCheckColumn("J ✓", "JehovahApproved");

            AddCheckColumn("Stale", "Stale", true);

            _qa = new TextBox();
            _qa.Dock = DockStyle.Bottom;
            _qa.Multiline = true;
            _qa.ScrollBars = ScrollBars.Both;
            _qa.Height = 145;
            _qa.ReadOnly = true;
            _qa.Font = new Font(FontFamily.GenericMonospace, 9);

            Controls.Add(_grid);
            Controls.Add(_qa);
            Controls.Add(top);
        }

        public override void OnAddedToParent(IPluginChildWindow parent, IWindowPluginHost host, string state)
        {
            parent.SetTitle(TamilDivineNamePlugin.PluginTitle);
            SetProject(parent.CurrentState.Project);
            _verseRef = parent.CurrentState.VerseRef;

            parent.ProjectChanged += delegate(IPluginChildWindow sender, IProject p)
            {
                SetProject(p);
            };

            parent.VerseRefChanged += delegate(IPluginChildWindow sender, IVerseRef oldRef, IVerseRef newRef)
            {
                _verseRef = newRef;
            };
        }

        public override string GetState() { return null; }
        public override void DoLoad(IProgressInfo progressInfo) { }

        private void SetProject(IProject project)
        {
            _project = project;
            if (_project == null)
            {
                _projectLabel.Text = "No project";
                _registry = new DivineNameRegistry();
                RefreshGrid();
                return;
            }

            _projectLabel.Text = "Project: " + _project.ShortName;
            _registry = RegistryStore.Load(_project.ShortName);
            RefreshGrid();
        }

        private void ScanCurrentChapter()
        {
            if (_project == null || _verseRef == null) return;
            string usfm = _project.GetUSFM(_verseRef.BookNum, _verseRef.ChapterNum);
            List<OccurrenceRecord> scan = UsfmOccurrenceScanner.ScanChapter(
                _project.ShortName,
                _verseRef.BookCode,
                _verseRef.ChapterNum,
                usfm,
                _lexicalFallback.Checked);

            RegistryStore.MergeScan(_registry, scan);
            RefreshGrid();
            ShowStatus("Scanned " + _verseRef.BookCode + " " + _verseRef.ChapterNum + ": " + scan.Count + " occurrence(s).");
        }

        private void ScanWholeBible()
        {
            if (_project == null) return;
            int total = 0;

            foreach (var book in _project.AvailableBooks)
            {
                int lastChapter = _project.Versification.GetLastChapter(book.Number);
                for (int chapter = 1; chapter <= lastChapter; chapter++)
                {
                    string usfm = _project.GetUSFM(book.Number, chapter);
                    List<OccurrenceRecord> scan = UsfmOccurrenceScanner.ScanChapter(
                        _project.ShortName,
                        book.Code,
                        chapter,
                        usfm,
                        _lexicalFallback.Checked);
                    RegistryStore.MergeScan(_registry, scan);
                    total += scan.Count;
                    Application.DoEvents();
                }
            }

            RegistryStore.Save(_registry);
            RefreshGrid();
            ShowStatus("Whole-Bible scan complete: " + total + " occurrence(s).");
        }

        private void RefreshGrid()
        {
            _grid.Rows.Clear();
            if (_registry == null) return;

            foreach (OccurrenceRecord o in _registry.Occurrences.OrderBy(x => x.BookCode).ThenBy(x => x.Chapter).ThenBy(x => x.Verse).ThenBy(x => x.Ordinal))
            {
                _grid.Rows.Add(
                    o.Key,
                    o.PreviousWord,
                    o.SourceName,
                    o.NextWord,
                    o.GrammarTag,
                    o.Karthar.Surface,
                    o.Karthar.LeftJoin,
                    o.Karthar.RightJoin,
                    o.Karthar.Approved,
                    o.Yahweh.Surface,
                    o.Yahweh.LeftJoin,
                    o.Yahweh.RightJoin,
                    o.Yahweh.Approved,
                    o.Jehovah.Surface,
                    o.Jehovah.LeftJoin,
                    o.Jehovah.RightJoin,
                    o.Jehovah.Approved,
                    o.Stale);
            }
        }

        private void SaveGridToRegistry()
        {
            if (_registry == null) return;

            foreach (DataGridViewRow row in _grid.Rows)
            {
                string key = Convert.ToString(row.Cells["Key"].Value);
                OccurrenceRecord o = _registry.Occurrences.FirstOrDefault(x => x.Key == key);
                if (o == null) continue;

                o.GrammarTag = Cell(row, "Grammar");
                Apply(row, o.Karthar, "Karthar");
                Apply(row, o.Yahweh, "Yahweh");
                Apply(row, o.Jehovah, "Jehovah");
            }
        }

        private static void Apply(DataGridViewRow row, ProfileRendering p, string prefix)
        {
            p.Surface = Cell(row, prefix + "Surface");
            p.LeftJoin = Cell(row, prefix + "Left");
            p.RightJoin = Cell(row, prefix + "Right");
            p.Approved = BoolCell(row, prefix + "Approved");
        }

        private void RunQa()
        {
            List<string> lines = new List<string>();
            string profile = Convert.ToString(_profile.SelectedItem);

            foreach (OccurrenceRecord o in _registry.Occurrences)
            {
                foreach (QaFinding f in TamilSandhiEngine.Check(o, profile))
                    lines.Add(f.ToString());
            }

            if (lines.Count == 0)
                lines.Add("PASS: no QA findings for " + profile + ".");

            _qa.Lines = lines.ToArray();
        }

        private void ExportProfile()
        {
            if (_project == null) return;

            string profile = Convert.ToString(_profile.SelectedItem);
            RunQa();

            foreach (OccurrenceRecord o in _registry.Occurrences)
            {
                ProfileRendering p = o.GetProfile(profile);
                if (!p.Approved || o.Stale || string.IsNullOrWhiteSpace(p.Surface))
                {
                    MessageBox.Show(
                        "Export blocked. Resolve all unapproved/stale/missing " + profile + " renderings first.\nFirst blocked item: " + o.Key,
                        TamilDivineNamePlugin.PluginTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }

            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Choose publication export folder";
                if (dlg.ShowDialog() != DialogResult.OK) return;

                string outDir = Path.Combine(dlg.SelectedPath, _project.ShortName + "-" + profile);
                Directory.CreateDirectory(outDir);

                foreach (var book in _project.AvailableBooks)
                {
                    int lastChapter = _project.Versification.GetLastChapter(book.Number);
                    List<string> parts = new List<string>();

                    for (int chapter = 1; chapter <= lastChapter; chapter++)
                    {
                        string usfm = _project.GetUSFM(book.Number, chapter);
                        string rendered = PublicationRenderer.RenderChapter(
                            _project.ShortName, book.Code, chapter, usfm, _registry, profile);
                        parts.Add(rendered);
                    }

                    string file = Path.Combine(outDir, book.Number.ToString("00") + book.Code + ".SFM");
                    File.WriteAllText(file, string.Join(Environment.NewLine, parts), new System.Text.UTF8Encoding(false));
                }

                RegistryStore.Save(_registry);
                MessageBox.Show("Publication USFM exported to:\n" + outDir);
            }
        }

        private void ShowStatus(string text)
        {
            _qa.Text = text + Environment.NewLine + "Registry: " +
                       (_project == null ? "" : RegistryStore.GetRegistryPath(_project.ShortName));
        }

        private void AddTextColumn(string header, string name, bool readOnly, int width)
        {
            DataGridViewTextBoxColumn c = new DataGridViewTextBoxColumn();
            c.HeaderText = header;
            c.Name = name;
            c.ReadOnly = readOnly;
            c.Width = width;
            _grid.Columns.Add(c);
        }

        private void AddCheckColumn(string header, string name, bool readOnly = false)
        {
            DataGridViewCheckBoxColumn c = new DataGridViewCheckBoxColumn();
            c.HeaderText = header;
            c.Name = name;
            c.ReadOnly = readOnly;
            c.Width = 45;
            _grid.Columns.Add(c);
        }

        private static string Cell(DataGridViewRow row, string name)
        {
            object v = row.Cells[name].Value;
            return v == null ? "" : Convert.ToString(v);
        }

        private static bool BoolCell(DataGridViewRow row, string name)
        {
            object v = row.Cells[name].Value;
            return v != null && Convert.ToBoolean(v);
        }
    }
}
