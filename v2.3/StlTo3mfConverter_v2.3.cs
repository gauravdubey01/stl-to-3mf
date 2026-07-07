using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace StlTo3mfConverter
{
    public class MainForm : Form
    {
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWCP_ROUND = 2;
        private const int DWMWCP_ROUND_SMALL = 1;

        private const int CORNER_FORM = 8;
        private const int CORNER_BTN = 5;
        private const int CORNER_TB = 4;
        private const int CORNER_LIST = 4;

        private TextBox txtFolder, txtOutput;
        private Button btnBrowseFolder, btnBrowseOutput, btnResetOutput, btnConvert, btnTheme;
        private ProgressBar progressBar;
        private ListBox lstLog;
        private Label lblStatus, lblFolder, lblOutput;
        private Button btnOpenOutput;
        private PictureBox picKofi;
        private LinkLabel lnkKofi;
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
                _bg = Color.FromArgb(18, 18, 22);
                _ctrlBg = Color.FromArgb(30, 30, 36);
                _text = Color.FromArgb(225, 225, 230);
                _accent = Color.FromArgb(79, 195, 247);
                _accentHover = Color.FromArgb(50, 165, 220);
                _btnBg = Color.FromArgb(45, 45, 52);
                _btnHover = Color.FromArgb(60, 60, 68);
                _listBg = Color.FromArgb(22, 22, 28);
                _statusText = Color.FromArgb(100, 210, 255);
                _headerBg = Color.FromArgb(14, 14, 18);
            }
            else
            {
                _bg = Color.FromArgb(238, 240, 245);
                _ctrlBg = Color.White;
                _text = Color.FromArgb(30, 30, 36);
                _accent = Color.FromArgb(25, 118, 210);
                _accentHover = Color.FromArgb(20, 95, 175);
                _btnBg = Color.FromArgb(230, 232, 237);
                _btnHover = Color.FromArgb(210, 212, 218);
                _listBg = Color.White;
                _statusText = Color.FromArgb(25, 118, 210);
                _headerBg = Color.FromArgb(225, 228, 235);
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

            btnOpenOutput.BackColor = _btnBg; btnOpenOutput.ForeColor = _accent;

            if (lnkKofi != null)
                lnkKofi.LinkColor = _darkMode ? Color.FromArgb(0, 180, 255) : Color.FromArgb(25, 118, 210);

            btnTheme.BackColor = _ctrlBg;
            btnTheme.Text = _darkMode ? "\u263E  Dark" : "\u2600  Light";
            btnTheme.ForeColor = _text;

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

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (Environment.OSVersion.Version.Build >= 22000)
            {
                int attr = DWMWCP_ROUND;
                DwmSetWindowAttribute(Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref attr, sizeof(int));
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            MessageBox.Show(
                "How to use STL to 3MF Batch Converter:\n\n" +
                "1. Click Browse to select a folder containing STL files\n" +
                "2. Optionally set a custom output folder (default: 3mf_output)\n" +
                "3. Click \"Convert All\" to start batch conversion\n" +
                "4. Use \"Open Output\" to view the converted 3MF files\n\n" +
                "Use the \u263E/\u2600 button (top-right) to switch between Dark/Light mode.\n\n" +
                "The app preserves your folder structure in the output.\n" +
                "Supports both ASCII and Binary STL formats.",
                "Welcome",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            var result = MessageBox.Show(
                "Are you sure you want to close?\n\n" +
                "If this tool helped you, consider supporting its development:\n" +
                "https://ko-fi.com/gauravdubeypro\n\n" +
                "Your support keeps this project alive and updated \u2665",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                e.Cancel = true;
        }

        private void RoundCorners(Control ctrl, int radius)
        {
            int w = ctrl.Width;
            int h = ctrl.Height;
            if (w < 1 || h < 1) return;
            using (var path = new GraphicsPath())
            {
                path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                path.AddArc(w - radius * 2 - 1, 0, radius * 2, radius * 2, 270, 90);
                path.AddArc(w - radius * 2 - 1, h - radius * 2 - 1, radius * 2, radius * 2, 0, 90);
                path.AddArc(0, h - radius * 2 - 1, radius * 2, radius * 2, 90, 90);
                path.CloseFigure();
                ctrl.Region = new Region(path);
            }
        }

        private void WireRoundCorners(Control ctrl, int radius)
        {
            RoundCorners(ctrl, radius);
            ctrl.Resize += (s, e) => RoundCorners(ctrl, radius);
        }

        private void InitializeComponent()
        {
            Text = "STL to 3MF Converter v2.5";
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
                RowCount = 8
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
            BuildFooter();

            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));

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
                Text = "\u2699  STL to 3MF Batch Converter  v2.5",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                Location = new Point(0, 8),
                AutoSize = true
            };
            btnTheme = new Button
            {
                Text = "\u263E  Dark",
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                BackColor = Color.FromArgb(45, 45, 52),
                Size = new Size(95, 36),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 12),
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnTheme.Click += (s, e) => { _darkMode = !_darkMode; SetTheme(); ApplyTheme(); };
            pnlHeader.Controls.Add(lblHeader);
            pnlHeader.Controls.Add(btnTheme);
            WireRoundCorners(btnTheme, CORNER_BTN);
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
                FlatAppearance = { BorderSize = 0 },
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            WireHover(btnBrowseFolder);
            WireRoundCorners(btnBrowseFolder, CORNER_BTN);
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
                FlatAppearance = { BorderSize = 0 },
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            WireHover(btnBrowseOutput);
            WireRoundCorners(btnBrowseOutput, CORNER_BTN);
            btnBrowseOutput.Click += BtnBrowseOutput_Click;

            btnResetOutput = new Button
            {
                Text = "\u21BA",
                Width = 32,
                Height = 26,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Font = new Font("Segoe UI", 12),
                Cursor = Cursors.Hand
            };
            var tip = new ToolTip();
            tip.SetToolTip(btnResetOutput, "Reset to default");
            WireHover(btnResetOutput);
            WireRoundCorners(btnResetOutput, CORNER_BTN);
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
            WireRoundCorners(btnConvert, CORNER_BTN);
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
            WireRoundCorners(lstLog, CORNER_LIST);
            mainPanel.Controls.Add(lstLog, 0, 5);
            mainPanel.SetColumnSpan(lstLog, 4);
        }

        private void BuildStatusBar()
        {
            lblStatus = new Label
            {
                Text = "Ready",
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill
            };
            btnOpenOutput = new Button
            {
                Text = "Open Output",
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand,
                Size = new Size(90, 22),
                Enabled = false,
                Margin = new Padding(2, 1, 0, 1)
            };
            WireRoundCorners(btnOpenOutput, CORNER_BTN);
            WireHover(btnOpenOutput);
            btnOpenOutput.Click += (s, e) =>
            {
                string path = txtOutput.Text.Trim();
                if (Directory.Exists(path))
                    Process.Start("explorer.exe", path);
                else
                    MessageBox.Show("Output folder does not exist yet.\nRun a conversion first.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            mainPanel.Controls.Add(lblStatus, 0, 6);
            mainPanel.SetColumnSpan(lblStatus, 3);
            mainPanel.Controls.Add(btnOpenOutput, 3, 6);
        }

        private void BuildFooter()
        {
            var pnlFooter = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 28
            };

            try
            {
                var bytes = Convert.FromBase64String("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");
                using (var ms = new MemoryStream(bytes))
                {
                    picKofi = new PictureBox
                    {
                        Image = Image.FromStream(ms),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Size = new Size(18, 18),
                        Cursor = Cursors.Hand,
                        Location = new Point(pnlFooter.Width - 230, 4)
                    };
                }
            }
            catch
            {
                picKofi = new PictureBox { Size = new Size(18, 18), Visible = false };
            }

            lnkKofi = new LinkLabel
            {
                Text = "Support on Ko-fi",
                Font = new Font("Segoe UI", 8),
                LinkColor = Color.FromArgb(0, 180, 255),
                ActiveLinkColor = Color.FromArgb(100, 210, 255),
                AutoSize = true,
                Cursor = Cursors.Hand
            };

            var lblMadeBy = new Label
            {
                Text = "made by Gaurav Dubey",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                AutoSize = true
            };

            string kofiUrl = "https://ko-fi.com/gauravdubeypro";
            picKofi.Click += (s, e) => Process.Start(kofiUrl);
            lnkKofi.LinkClicked += (s, e) => Process.Start(kofiUrl);

            pnlFooter.Controls.Add(lblMadeBy);
            pnlFooter.Controls.Add(picKofi);
            pnlFooter.Controls.Add(lnkKofi);

            pnlFooter.Resize += (s, e) =>
            {
                lblMadeBy.Left = pnlFooter.Width - 370;
                lblMadeBy.Top = (pnlFooter.Height - lblMadeBy.Height) / 2;
                picKofi.Left = pnlFooter.Width - 210;
                picKofi.Top = (pnlFooter.Height - picKofi.Height) / 2;
                lnkKofi.Left = picKofi.Right + 4;
                lnkKofi.Top = (pnlFooter.Height - lnkKofi.Height) / 2;
            };

            mainPanel.Controls.Add(pnlFooter, 0, 7);
            mainPanel.SetColumnSpan(pnlFooter, 4);
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
                btnOpenOutput.Enabled = Directory.Exists(txtOutput.Text.Trim());
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
            btnOpenOutput.Enabled = enabled && Directory.Exists(txtOutput.Text.Trim());
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
            using (var dlg = new OpenFileDialog())
            {
                dlg.ValidateNames = false;
                dlg.CheckFileExists = false;
                dlg.CheckPathExists = true;
                dlg.Title = description ?? "Select a folder";
                dlg.FileName = "Select";
                if (Directory.Exists(initialDir))
                    dlg.InitialDirectory = initialDir;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    string path = Path.GetDirectoryName(dlg.FileName);
                    if (Directory.Exists(path))
                        return path;
                }
                return null;
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
