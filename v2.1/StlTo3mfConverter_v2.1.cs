using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StlTo3mfConverter
{
    public class MainForm : Form
    {
        private TextBox txtFolder, txtOutput;
        private Button btnBrowseFolder, btnBrowseOutput, btnResetOutput, btnConvert, btnTheme;
        private ProgressBar progressBar;
        private ListBox lstLog;
        private Label lblStatus, lblFolder, lblOutput;
        private TableLayoutPanel mainPanel;
        private Panel pnlHeader, pnlConvert;
        private BackgroundWorker _worker;
        private DateTime _startTime;
        private bool _darkMode = true;
        private string _iconPath = @"E:\MY COMPANY\WINDOWS PROGRAM\STL TO 3MF CONVERTER\3MF.png";

        private Color _bg, _ctrlBg, _text, _accent, _accentHover, _btnBg, _btnHover, _listBg, _statusText, _headerBg;

        public MainForm()
        {
            SetTheme();
            InitializeComponent();
            LoadIcon();
        }

        private void SetTheme()
        {
            if (_darkMode)
            {
                _bg = Color.FromArgb(28, 28, 28);
                _ctrlBg = Color.FromArgb(45, 45, 48);
                _text = Color.FromArgb(224, 224, 224);
                _accent = Color.FromArgb(0, 120, 212);
                _accentHover = Color.FromArgb(0, 100, 180);
                _btnBg = Color.FromArgb(62, 62, 66);
                _btnHover = Color.FromArgb(80, 80, 85);
                _listBg = Color.FromArgb(30, 30, 30);
                _statusText = Color.FromArgb(80, 180, 255);
                _headerBg = Color.FromArgb(20, 20, 20);
            }
            else
            {
                _bg = Color.FromArgb(240, 240, 240);
                _ctrlBg = Color.White;
                _text = Color.FromArgb(30, 30, 30);
                _accent = Color.FromArgb(0, 120, 212);
                _accentHover = Color.FromArgb(0, 90, 160);
                _btnBg = Color.FromArgb(224, 224, 224);
                _btnHover = Color.FromArgb(200, 200, 200);
                _listBg = Color.White;
                _statusText = Color.FromArgb(0, 100, 180);
                _headerBg = Color.FromArgb(220, 220, 220);
            }
        }

        private void ApplyTheme()
        {
            BackColor = _bg;
            ForeColor = _text;

            mainPanel.BackColor = _bg;
            pnlHeader.BackColor = _headerBg;
            foreach (Control c in pnlHeader.Controls) { c.ForeColor = _darkMode ? Color.White : _text; }

            txtFolder.BackColor = _ctrlBg; txtFolder.ForeColor = _text;
            txtOutput.BackColor = _ctrlBg; txtOutput.ForeColor = _text;

            lblFolder.ForeColor = _text;
            lblOutput.ForeColor = _text;

            lstLog.BackColor = _listBg; lstLog.ForeColor = _text;

            progressBar.ForeColor = _accent;
            progressBar.BackColor = _ctrlBg;

            btnBrowseFolder.BackColor = _btnBg; btnBrowseFolder.ForeColor = _text;
            btnBrowseOutput.BackColor = _btnBg; btnBrowseOutput.ForeColor = _text;
            btnResetOutput.BackColor = _btnBg; btnResetOutput.ForeColor = _text;
            btnConvert.BackColor = _accent; btnConvert.ForeColor = Color.White;

            btnTheme.Text = _darkMode ? "\u263E" : "\u2600";
            btnTheme.ForeColor = _darkMode ? Color.White : Color.Black;

            lblStatus.ForeColor = _statusText;

            Invalidate(true);
        }

        private void LoadIcon()
        {
            if (!File.Exists(_iconPath)) return;
            try
            {
                using (var bmp = new Bitmap(_iconPath))
                {
                    Icon = Icon.FromHandle(bmp.GetHicon());
                }
            }
            catch { }
        }

        private void InitializeComponent()
        {
            Text = "STL to 3MF Converter";
            MinimumSize = new Size(800, 560);
            Size = new Size(900, 640);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            AllowDrop = true;

            mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15, 0, 15, 15),
                ColumnCount = 4,
                RowCount = 7
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            BuildHeader();
            BuildFolderRow();
            BuildOutputRow();
            BuildConvertRow();
            BuildProgressBar();
            BuildLogList();
            BuildStatusBar();

            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));

            Controls.Add(mainPanel);

            txtFolder.TextChanged += OnFolderChanged;
            txtOutput.TextChanged += (s, e) => UpdateState();
            DragEnter += OnDragEnter;
            DragDrop += OnDragDrop;

            ApplyTheme();
            UpdateState();
        }

        private void BuildHeader()
        {
            pnlHeader = new Panel { Dock = DockStyle.Fill, Height = 44 };
            var lblHeader = new Label
            {
                Text = "\u2699  STL to 3MF Converter",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                Location = new Point(0, 8),
                AutoSize = true
            };
            btnTheme = new Button
            {
                Text = "\u263E",
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Size = new Size(40, 36),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 14),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnTheme.Click += (s, e) => { _darkMode = !_darkMode; SetTheme(); ApplyTheme(); };
            pnlHeader.Controls.Add(lblHeader);
            pnlHeader.Controls.Add(btnTheme);
            mainPanel.Controls.Add(pnlHeader, 0, 0);
            mainPanel.SetColumnSpan(pnlHeader, 4);
        }

        private void BuildFolderRow()
        {
            lblFolder = new Label
            {
                Text = "STL Folder:",
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            txtFolder = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9)
            };
            btnBrowseFolder = new Button
            {
                Text = "Browse",
                Width = 80,
                Height = 26,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 1 },
                Font = new Font("Segoe UI", 9)
            };
            WireHover(btnBrowseFolder);
            btnBrowseFolder.Click += BtnBrowseFolder_Click;
            mainPanel.Controls.Add(lblFolder, 0, 1);
            mainPanel.Controls.Add(txtFolder, 1, 1);
            mainPanel.Controls.Add(btnBrowseFolder, 2, 1);
            mainPanel.SetColumnSpan(lblFolder, 1);
            var pad1 = new Panel { Width = 0 }; mainPanel.Controls.Add(pad1, 3, 1);
        }

        private void BuildOutputRow()
        {
            lblOutput = new Label
            {
                Text = "Output:",
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            txtOutput = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9)
            };
            btnBrowseOutput = new Button
            {
                Text = "Browse",
                Width = 80,
                Height = 26,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 1 },
                Font = new Font("Segoe UI", 9)
            };
            WireHover(btnBrowseOutput);
            btnBrowseOutput.Click += BtnBrowseOutput_Click;

            btnResetOutput = new Button
            {
                Text = "\u21BA",
                Width = 32,
                Height = 26,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 1 },
                Font = new Font("Segoe UI", 12)
            };
            var tip = new ToolTip();
            tip.SetToolTip(btnResetOutput, "Reset to default");
            WireHover(btnResetOutput);
            btnResetOutput.Click += (s, e) => ResetOutputPath();

            mainPanel.Controls.Add(lblOutput, 0, 2);
            mainPanel.Controls.Add(txtOutput, 1, 2);
            mainPanel.Controls.Add(btnBrowseOutput, 2, 2);
            mainPanel.Controls.Add(btnResetOutput, 3, 2);
        }

        private void BuildConvertRow()
        {
            pnlConvert = new Panel { Dock = DockStyle.Fill };
            btnConvert = new Button
            {
                Text = "Convert All",
                Height = 40,
                Width = 220,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Enabled = false,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            pnlConvert.Resize += (s, e) => CenterButton();
            pnlConvert.Controls.Add(btnConvert);
            btnConvert.MouseEnter += (s, e) => btnConvert.BackColor = _accentHover;
            btnConvert.MouseLeave += (s, e) => btnConvert.BackColor = _accent;
            btnConvert.Click += BtnConvert_Click;
            mainPanel.Controls.Add(pnlConvert, 0, 3);
            mainPanel.SetColumnSpan(pnlConvert, 4);
        }

        private void BuildProgressBar()
        {
            progressBar = new ProgressBar
            {
                Height = 22,
                Dock = DockStyle.Fill,
                Style = ProgressBarStyle.Continuous,
                Margin = new Padding(0, 6, 0, 4)
            };
            mainPanel.Controls.Add(progressBar, 0, 4);
            mainPanel.SetColumnSpan(progressBar, 4);
        }

        private void BuildLogList()
        {
            lstLog = new ListBox
            {
                Dock = DockStyle.Fill,
                IntegralHeight = false,
                Font = new Font("Consolas", 9),
                Margin = new Padding(0)
            };
            mainPanel.Controls.Add(lstLog, 0, 5);
            mainPanel.SetColumnSpan(lstLog, 4);
        }

        private void BuildStatusBar()
        {
            lblStatus = new Label
            {
                Text = "Ready",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(lblStatus, 0, 6);
            mainPanel.SetColumnSpan(lblStatus, 4);
        }

        private void CenterButton()
        {
            btnConvert.Left = (pnlConvert.Width - btnConvert.Width) / 2;
            btnConvert.Top = (pnlConvert.Height - btnConvert.Height) / 2;
        }

        private void WireHover(Button btn)
        {
            btn.MouseEnter += (s, e) => btn.BackColor = _btnHover;
            btn.MouseLeave += (s, e) => btn.BackColor = _btnBg;
            btn.GotFocus += (s, e) => btn.FlatAppearance.BorderColor = _accent;
        }

        private void UpdateState()
        {
            bool folderExists = Directory.Exists(txtFolder.Text);
            btnConvert.Enabled = folderExists && _worker == null;
            if (folderExists)
            {
                try
                {
                    var count = Directory.GetFiles(txtFolder.Text, "*.stl", SearchOption.AllDirectories).Length;
                    lblStatus.Text = string.Format("{0} STL file(s) found", count);
                }
                catch
                {
                    lblStatus.Text = "Error reading folder";
                }
            }
            else
            {
                lblStatus.Text = txtFolder.Text.Length > 0 ? "Folder does not exist" : "Select a folder with STL files";
            }
        }

        private void ResetOutputPath()
        {
            if (Directory.Exists(txtFolder.Text))
                txtOutput.Text = Path.Combine(txtFolder.Text, "3mf_output");
        }

        private void OnFolderChanged(object sender, EventArgs e)
        {
            UpdateState();
            if (Directory.Exists(txtFolder.Text) && string.IsNullOrWhiteSpace(txtOutput.Text))
                ResetOutputPath();
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Length > 0)
            {
                string path = files[0];
                if (Directory.Exists(path))
                {
                    txtFolder.Text = path;
                    ResetOutputPath();
                }
                else if (File.Exists(path))
                {
                    txtFolder.Text = Path.GetDirectoryName(path);
                    ResetOutputPath();
                }
            }
        }

        private void BtnBrowseFolder_Click(object sender, EventArgs e)
        {
            string path = ModernFolderDialog.Pick("Select STL folder", txtFolder.Text);
            if (path != null)
            {
                txtFolder.Text = path;
                ResetOutputPath();
            }
        }

        private void BtnBrowseOutput_Click(object sender, EventArgs e)
        {
            string path = ModernFolderDialog.Pick("Select output folder", txtOutput.Text);
            if (path != null)
                txtOutput.Text = path;
        }

        private void BtnConvert_Click(object sender, EventArgs e)
        {
            var folder = txtFolder.Text.Trim();
            if (!Directory.Exists(folder))
            {
                MessageBox.Show("Folder does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var outputRoot = txtOutput.Text.Trim();
            if (string.IsNullOrWhiteSpace(outputRoot))
                outputRoot = Path.Combine(folder, "3mf_output");

            var files = Directory.GetFiles(folder, "*.stl", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                lstLog.Items.Add("No STL files found.");
                return;
            }

            SetEnabled(false);
            lstLog.Items.Clear();
            progressBar.Value = 0;

            lstLog.Items.Add(string.Format("Found {0} STL(s). Output: {1}", files.Length, outputRoot));
            lstLog.Items.Add("");

            _startTime = DateTime.Now;
            _worker = new BackgroundWorker { WorkerReportsProgress = true };

            _worker.DoWork += (bw, args) =>
            {
                var worker = (BackgroundWorker)bw;
                int total = files.Length;
                int success = 0, failed = 0;

                for (int i = 0; i < total; i++)
                {
                    string stl = files[i];
                    string stlDir = Path.GetDirectoryName(stl);
                    string rel = stlDir == folder ? "" : stlDir.Substring(folder.Length).TrimStart('\\');
                    string outDir = string.IsNullOrEmpty(rel) ? outputRoot : Path.Combine(outputRoot, rel);
                    string outFile = Path.Combine(outDir, Path.GetFileNameWithoutExtension(stl) + ".3mf");

                    Directory.CreateDirectory(outDir);

                    string logMsg;
                    bool isError = false;

                    try
                    {
                        var parser = new StlParser();
                        var result = parser.Parse(stl);
                        var writer = new ThreeMfWriter();
                        writer.Write(outFile, result.Vertices, result.Triangles);
                        logMsg = string.Format("  -> OK ({0} verts, {1} tris)", result.Vertices.Length, result.Triangles.Length);
                        success++;
                    }
                    catch (Exception ex)
                    {
                        logMsg = string.Format("  -> ERROR: {0}", ex.Message);
                        failed++;
                        isError = true;
                    }

                    var elapsed = DateTime.Now - _startTime;
                    var eta = TimeSpan.FromSeconds(elapsed.TotalSeconds / (i + 1) * (total - i - 1));

                    worker.ReportProgress((i + 1) * 100 / total, new ProgressInfo
                    {
                        Current = i + 1,
                        Total = total,
                        FileName = Path.GetFileName(stl),
                        LogMessage = logMsg,
                        IsError = isError,
                        SuccessCount = success,
                        FailedCount = failed,
                        Eta = eta
                    });
                }

                args.Result = new[] { success, failed };
            };

            _worker.ProgressChanged += (bw, ev) =>
            {
                var info = (ProgressInfo)ev.UserState;
                progressBar.Value = ev.ProgressPercentage;
                lstLog.Items.Add(string.Format("[{0}%] {1}", ev.ProgressPercentage, info.FileName));
                lstLog.Items.Add(info.LogMessage);
                lstLog.TopIndex = lstLog.Items.Count - 1;
                lblStatus.Text = string.Format("File {0}/{1} | {2} | ETA: {3:F0}s",
                    info.Current, info.Total, info.FileName, info.Eta.TotalSeconds);
            };

            _worker.RunWorkerCompleted += (bw, ev) =>
            {
                SetEnabled(true);
                if (ev.Error != null)
                {
                    lstLog.Items.Add(string.Format("FATAL: {0}", ev.Error.Message));
                    lblStatus.Text = "Conversion failed!";
                }
                else
                {
                    var result = (int[])ev.Result;
                    lstLog.Items.Add("");
                    lstLog.Items.Add(string.Format("Done. {0} converted, {1} failed.", result[0], result[1]));
                    lblStatus.Text = string.Format("Done. {0} OK, {1} failed.", result[0], result[1]);
                }
                progressBar.Value = 0;
                _worker.Dispose();
                _worker = null;
            };

            _worker.RunWorkerAsync();
        }

        private void SetEnabled(bool enabled)
        {
            btnConvert.Enabled = enabled;
            btnBrowseFolder.Enabled = enabled;
            btnBrowseOutput.Enabled = enabled;
            btnResetOutput.Enabled = enabled;
            txtFolder.Enabled = enabled;
            txtOutput.Enabled = enabled;
        }

        private class ProgressInfo
        {
            public int Current { get; set; }
            public int Total { get; set; }
            public string FileName { get; set; }
            public string LogMessage { get; set; }
            public bool IsError { get; set; }
            public int SuccessCount { get; set; }
            public int FailedCount { get; set; }
            public TimeSpan Eta { get; set; }
        }
    }

    internal static class ModernFolderDialog
    {
        public static string Pick(string description, string initialDir)
        {
            try
            {
                var guid = new Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7");
                Type type = Type.GetTypeFromCLSID(guid);
                dynamic dlg = Activator.CreateInstance(type);

                try
                {
                    dlg.Title = description ?? "Select a folder";
                    if (!string.IsNullOrEmpty(initialDir) && Directory.Exists(initialDir))
                    {
                        try { dlg.SetDefaultFolder(initialDir); } catch { }
                    }
                    dlg.Options = dlg.Options | 0x20;

                    int hr = (int)dlg.Show(IntPtr.Zero);
                    if (hr == 0)
                    {
                        dynamic item = dlg.GetResult();
                        return item.GetDisplayName(0x80028000);
                    }
                    return null;
                }
                finally
                {
                    Marshal.FinalReleaseComObject(dlg);
                }
            }
            catch
            {
                using (var dlg = new FolderBrowserDialog())
                {
                    if (!string.IsNullOrEmpty(initialDir))
                    {
                        try { dlg.SelectedPath = initialDir; } catch { }
                    }
                    dlg.Description = description ?? "Select a folder";
                    return dlg.ShowDialog() == DialogResult.OK ? dlg.SelectedPath : null;
                }
            }
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
