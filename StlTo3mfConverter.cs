using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;

namespace StlTo3mfConverter
{
    public class MainForm : Form
    {
        private TextBox txtFolder;
        private Button btnBrowse;
        private Button btnConvert;
        private ProgressBar progressBar;
        private Label lblStatus;
        private ListBox lstLog;
        private Label lblCount;
        private string _prusaSlicerPath = @"C:\Program Files\Prusa3D\PrusaSlicer\prusa-slicer-console.exe";

        public MainForm()
        {
            Text = "STL to 3MF Converter";
            Size = new System.Drawing.Size(620, 520);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 3,
                RowCount = 5
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var lblFolder = new Label { Text = "STL Folder:", Anchor = AnchorStyles.Left };
            txtFolder = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, Width = 400 };
            btnBrowse = new Button { Text = "Browse...", Width = 80 };
            btnBrowse.Click += BtnBrowse_Click;

            mainPanel.Controls.Add(lblFolder, 0, 0);
            mainPanel.Controls.Add(txtFolder, 1, 0);
            mainPanel.Controls.Add(btnBrowse, 2, 0);

            btnConvert = new Button { Text = "Convert All", Height = 35, Enabled = false };
            btnConvert.Click += BtnConvert_Click;
            btnConvert.Anchor = AnchorStyles.Left;

            lblCount = new Label { Text = "", Anchor = AnchorStyles.Left };
            var countPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Dock = DockStyle.Fill };
            countPanel.Controls.Add(btnConvert);
            countPanel.Controls.Add(lblCount);
            countPanel.Controls.Add(new Label { Text = "   " });
            mainPanel.Controls.Add(countPanel, 0, 1);
            mainPanel.SetColumnSpan(countPanel, 3);

            progressBar = new ProgressBar { Height = 22, Dock = DockStyle.Fill };
            mainPanel.Controls.Add(progressBar, 0, 2);
            mainPanel.SetColumnSpan(progressBar, 3);

            lstLog = new ListBox { Dock = DockStyle.Fill, ItemHeight = 15, Font = new System.Drawing.Font("Consolas", 9) };
            mainPanel.Controls.Add(lstLog, 0, 3);
            mainPanel.SetColumnSpan(lstLog, 3);

            var lblFooter = new Label
            {
                Text = "Requires PrusaSlicer installed at: " + _prusaSlicerPath,
                ForeColor = System.Drawing.Color.Gray,
                Font = new System.Drawing.Font("Segoe UI", 8),
                Anchor = AnchorStyles.Left
            };
            mainPanel.Controls.Add(lblFooter, 0, 4);
            mainPanel.SetColumnSpan(lblFooter, 3);

            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Controls.Add(mainPanel);

            txtFolder.TextChanged += (s, e) => UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            btnConvert.Enabled = Directory.Exists(txtFolder.Text) && File.Exists(_prusaSlicerPath);
            if (Directory.Exists(txtFolder.Text))
            {
                var count = Directory.GetFiles(txtFolder.Text, "*.stl", SearchOption.AllDirectories).Length;
                lblCount.Text = string.Format("{0} STL file(s) found", count);
            }
            else
            {
                lblCount.Text = "";
            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                    txtFolder.Text = dlg.SelectedPath;
            }
        }

        private void BtnConvert_Click(object sender, EventArgs e)
        {
            var folder = txtFolder.Text.Trim();
            if (!Directory.Exists(folder))
            {
                MessageBox.Show("Folder does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!File.Exists(_prusaSlicerPath))
            {
                MessageBox.Show("PrusaSlicer not found at:\n" + _prusaSlicerPath, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnConvert.Enabled = false;
            btnBrowse.Enabled = false;
            lstLog.Items.Clear();
            progressBar.Value = 0;

            var files = Directory.GetFiles(folder, "*.stl", SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                lstLog.Items.Add("No STL files found.");
                btnConvert.Enabled = true;
                btnBrowse.Enabled = true;
                return;
            }

            progressBar.Maximum = files.Length;
            var outputRoot = Path.Combine(folder, "3mf_output");
            int success = 0, failed = 0;

            lstLog.Items.Add(string.Format("Found {0} STL(s). Output: {1}", files.Length, outputRoot));
            lstLog.Items.Add("");

            for (int i = 0; i < files.Length; i++)
            {
                var stl = files[i];
                var pct = (int)((double)(i + 1) / files.Length * 100);
                var stlDir = Path.GetDirectoryName(stl);
                var rel = stlDir == folder ? "" : stlDir.Substring(folder.Length).TrimStart('\\');
                var outDir = string.IsNullOrEmpty(rel) ? outputRoot : Path.Combine(outputRoot, rel);
                var outFile = Path.Combine(outDir, Path.GetFileNameWithoutExtension(stl) + ".3mf");

                Directory.CreateDirectory(outDir);

                lstLog.Items.Add(string.Format("[{0}%] Converting: {1}", pct, Path.GetFileName(stl)));
                lstLog.TopIndex = lstLog.Items.Count - 1;
                Application.DoEvents();

                var psi = new ProcessStartInfo
                {
                    FileName = _prusaSlicerPath,
                    Arguments = string.Format("--export-3mf --dont-arrange --output \"{0}\" \"{1}\"", outFile, stl),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                try
                {
                    using (var proc = Process.Start(psi))
                    {
                        proc.WaitForExit();
                        if (proc.ExitCode == 0)
                        {
                            success++;
                        }
                        else
                        {
                            lstLog.Items.Add(string.Format("  -> FAILED (exit {0})", proc.ExitCode));
                            failed++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    lstLog.Items.Add(string.Format("  -> ERROR: {0}", ex.Message));
                    failed++;
                }

                progressBar.Value = i + 1;
            }

            lstLog.Items.Add("");
            lstLog.Items.Add(string.Format("Done. {0} converted, {1} failed.", success, failed));
            lstLog.TopIndex = lstLog.Items.Count - 1;

            btnConvert.Enabled = true;
            btnBrowse.Enabled = true;
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
