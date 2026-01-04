using System.Diagnostics;
using System.Text;
using System.Diagnostics;
using ScumChecker.Core.Tools;
using ScumChecker.Core;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ScumChecker.Core;
using ScumChecker.Core.Tools;


namespace ScumChecker
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource? _cts;
        private readonly List<ScanItem> _allItems = new();

        public Form1()
        {
            InitializeComponent();

            linkGitHub.LinkClicked += (_, __) => OpenUrl("https://github.com/Nezeryxs");
            linkBio.LinkClicked += (_, __) => OpenUrl("https://e-z.bio/nezeryxs");


            // events
            btnScan.Click += btnScan_Click;
            btnCancel.Click += (_, __) => _cts?.Cancel();

            // эти кнопки добавь в designer
            btnCopyReport.Click += (_, __) => CopyReportToClipboard();
            chkInfo.CheckedChanged += (_, __) => ApplyFilters();
            chkLow.CheckedChanged += (_, __) => ApplyFilters();
            chkMedium.CheckedChanged += (_, __) => ApplyFilters();
            chkHigh.CheckedChanged += (_, __) => ApplyFilters();

            // Badge render
            dgvFindings.CellPainting += DgvFindings_CellPainting;
            dgvFindings.CellDoubleClick += DgvFindings_CellDoubleClick;

            // nicer UX
            dgvFindings.RowTemplate.Height = 28;
            dgvFindings.ClearSelection();
        }


        private async void btnScan_Click(object? sender, EventArgs e)
        {
            if (_cts != null) return;

            _allItems.Clear();
            dgvFindings.Rows.Clear();
            txtLog.Clear();
            ResetSummary();

            btnScan.Enabled = false;
            btnCancel.Enabled = true;

            _cts = new CancellationTokenSource();
            var scanner = new Scanner();

            scanner.Log += s => SafeUi(() => txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {s}\r\n"));
            scanner.Progress += p => SafeUi(() =>
            {
                lblStatus.Text = "Status: " + p.Stage;
                progressScan.Value = Math.Clamp(p.Percent, 0, 100);
            });

            scanner.ItemFound += item => SafeUi(() =>
            {
                _allItems.Add(item);
                AddRow(item);
                UpdateSummary();
            });

            try
            {
                await Task.Run(() => scanner.Run(_cts.Token));
            }
            catch (OperationCanceledException)
            {
                SafeUi(() => txtLog.AppendText("Canceled.\r\n"));
            }
            finally
            {
                SafeUi(() =>
                {
                    btnScan.Enabled = true;
                    btnCancel.Enabled = false;
                    lblStatus.Text = "Status: Idle";
                    progressScan.Value = 0;

                    RefreshSteamTab(); // ✅ вот тут
                });

                _cts?.Dispose();
                _cts = null;
            }

        }

        private void AddRow(ScanItem item)
        {
            // Фильтруем сразу при добавлении
            if (!PassSeverityFilter(item.Severity)) return;

            // Важно: колонки должны быть в таком порядке:
            // Severity | Category | What | Why flagged | Recommended action | Details
            int rowIndex = dgvFindings.Rows.Add(
                item.Severity.ToString(),
                item.Category,
                item.What,
                item.Reason,
                item.Recommendation,
                item.Details
            );

            // В row.Tag храним item (для double click)
            dgvFindings.Rows[rowIndex].Tag = item;
        }

        private void ApplyFilters()
        {
            dgvFindings.Rows.Clear();
            foreach (var item in _allItems)
                AddRow(item);
            dgvFindings.ClearSelection();
        }

        private bool PassSeverityFilter(Severity s)
        {
            return s switch
            {
                Severity.Info => chkInfo.Checked,
                Severity.Low => chkLow.Checked,
                Severity.Medium => chkMedium.Checked,
                Severity.High => chkHigh.Checked,
                _ => true
            };
        }

        private void DgvFindings_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvFindings.Rows[e.RowIndex];
            if (row.Tag is not ScanItem item) return;

            // 1) если есть URL (Steam) — открываем профиль
            if (!string.IsNullOrWhiteSpace(item.Url))
            {
                OpenUrl(item.Url!);
                return;
            }

            // 2) если есть EvidencePath — открыть папку / файл
            if (!string.IsNullOrWhiteSpace(item.EvidencePath))
            {
                OpenPath(item.EvidencePath!);
                return;
            }

            // 3) fallback: ничего
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                txtLog.AppendText("Open URL error: " + ex.Message + "\r\n");
            }
        }

        private void OpenPath(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    // выделить файл в проводнике
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"/select,\"{path}\"",
                        UseShellExecute = true
                    });
                }
                else if (Directory.Exists(path))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"\"{path}\"",
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                txtLog.AppendText("Open path error: " + ex.Message + "\r\n");
            }
        }

        // ====== Colored badge in Severity column (0)
        private void DgvFindings_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex != 0) return; // Severity column index

            e.Handled = true;
            e.PaintBackground(e.ClipBounds, true);

            string text = Convert.ToString(e.FormattedValue) ?? "";
            if (string.IsNullOrWhiteSpace(text))
            {
                e.PaintContent(e.ClipBounds);
                return;
            }

            Color badge = text switch
            {
                "High" => Color.FromArgb(220, 60, 60),
                "Medium" => Color.FromArgb(230, 155, 60),
                "Low" => Color.FromArgb(230, 210, 80),
                _ => Color.FromArgb(120, 150, 180) // Info
            };

            var rect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + 5, e.CellBounds.Width - 16, e.CellBounds.Height - 10);
            rect.Width = Math.Min(rect.Width, 90);

            using var brush = new SolidBrush(badge);
            using var textBrush = new SolidBrush(Color.Black);

            // simple rounded-ish look
            e.Graphics.FillRectangle(brush, rect);

            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString(text, e.CellStyle.Font, textBrush, rect, sf);

            // border
            using var pen = new Pen(Color.FromArgb(40, 40, 50));
            e.Graphics.DrawRectangle(pen, rect);
        }

        // ===== Summary + verdict
        private void ResetSummary()
        {
            lblCountHigh.Text = "High: 0";
            lblCountMedium.Text = "Medium: 0";
            lblCountLow.Text = "Low: 0";
            lblCountInfo.Text = "Info: 0";
            lblVerdict.Text = "No data (run Scan)";
        }

        private void UpdateSummary()
        {
            int hi = _allItems.Count(x => x.Severity == Severity.High);
            int me = _allItems.Count(x => x.Severity == Severity.Medium);
            int lo = _allItems.Count(x => x.Severity == Severity.Low);
            int inf = _allItems.Count(x => x.Severity == Severity.Info);

            lblCountHigh.Text = $"High: {hi}";
            lblCountMedium.Text = $"Medium: {me}";
            lblCountLow.Text = $"Low: {lo}";
            lblCountInfo.Text = $"Info: {inf}";

            // админский вердикт
            if (hi > 0) lblVerdict.Text = "High risk indicators found → manual review recommended";
            else if (me > 0) lblVerdict.Text = "Suspicious indicators found → manual review (no instant ban)";
            else lblVerdict.Text = "No high-risk indicators found";
        }

        // ===== Copy report
        private void CopyReportToClipboard()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ScumChecker report");
            sb.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine(lblVerdict.Text);
            sb.AppendLine(lblCountHigh.Text + " | " + lblCountMedium.Text + " | " + lblCountLow.Text + " | " + lblCountInfo.Text);
            sb.AppendLine(new string('-', 80));

            foreach (var it in _allItems.OrderByDescending(x => x.Severity))
            {
                sb.AppendLine($"[{it.Severity}] {it.Category} | {it.What}");
                if (!string.IsNullOrWhiteSpace(it.Reason)) sb.AppendLine($"  Why: {it.Reason}");
                if (!string.IsNullOrWhiteSpace(it.Recommendation)) sb.AppendLine($"  Action: {it.Recommendation}");
                if (!string.IsNullOrWhiteSpace(it.Details)) sb.AppendLine($"  Details: {it.Details}");
                if (!string.IsNullOrWhiteSpace(it.EvidencePath)) sb.AppendLine($"  Path: {it.EvidencePath}");
                if (!string.IsNullOrWhiteSpace(it.Url)) sb.AppendLine($"  URL: {it.Url}");
                sb.AppendLine();
            }

            Clipboard.SetText(sb.ToString());
            txtLog.AppendText("Report copied to clipboard.\r\n");
        }

        private void RefreshSteamTab()
        {
            dgvSteam.Rows.Clear();

            foreach (var it in _allItems)
            {
                if (it.Category != "Steam") continue;
                if (it.What != "Steam account" && it.Title != "Steam account") continue;

                // достаём SteamID из URL
                string steamId = "";
                if (!string.IsNullOrWhiteSpace(it.Url))
                {
                    var idx = it.Url.LastIndexOf('/');
                    steamId = idx >= 0 ? it.Url[(idx + 1)..] : it.Url;
                }

                // Details: persona (account) | MostRecent=.. | Timestamp=..
                string persona = "", account = "", mostRecent = "", timestamp = "";

                var parts = it.Details.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (parts.Length > 0)
                {
                    var left = parts[0]; // "persona (account)"
                    var open = left.LastIndexOf('(');
                    var close = left.LastIndexOf(')');

                    if (open > 0 && close > open)
                    {
                        persona = left.Substring(0, open).Trim();
                        account = left.Substring(open + 1, close - open - 1).Trim();
                    }
                    else
                    {
                        persona = left.Trim();
                    }
                }

                foreach (var p in parts)
                {
                    if (p.StartsWith("MostRecent=", StringComparison.OrdinalIgnoreCase))
                        mostRecent = p["MostRecent=".Length..].Trim();
                    else if (p.StartsWith("Timestamp=", StringComparison.OrdinalIgnoreCase))
                        timestamp = p["Timestamp=".Length..].Trim();
                }

                int r = dgvSteam.Rows.Add(steamId, persona, account, mostRecent, timestamp);
                dgvSteam.Rows[r].Tag = it; // чтобы открыть Url по дабл-клику
            }

            dgvSteam.ClearSelection();
        }


        private void SafeUi(Action a)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke(a);
            else a();
        }
        private List<ToolEntry> _tools = new();

        private void Form1_Load(object sender, EventArgs e)
        {
            // загрузим список tools
            RefreshTools();

            // кнопки
            btnOpenTool.Click += (_, __) => OpenSelectedTool();
            btnLocateTool.Click += (_, __) => LocateSelectedTool();
            btnDownloadTool.Click += (_, __) => DownloadSelectedTool();

            dgvTools.CellDoubleClick += (_, e2) =>
            {
                if (e2.RowIndex >= 0) OpenSelectedTool();
            };
            dgvSteam.CellDoubleClick += (_, e2) =>
            {
                if (e2.RowIndex < 0) return;
                var row = dgvSteam.Rows[e2.RowIndex];
                if (row.Tag is ScanItem it && !string.IsNullOrWhiteSpace(it.Url))
                    OpenUrl(it.Url);
            };

        }

        private void RefreshTools()
        {
            dgvTools.Rows.Clear();
            _tools = ToolsDetector.Detect();

            foreach (var t in _tools)
                dgvTools.Rows.Add(t.Name, t.Status, t.Path);

            dgvTools.ClearSelection();
        }

        private ToolEntry? GetSelectedTool()
        {
            if (dgvTools.CurrentRow == null) return null;
            var name = dgvTools.CurrentRow.Cells[0].Value?.ToString() ?? "";
            return _tools.FirstOrDefault(x => x.Name == name);
        }

        private void OpenSelectedTool()
        {
            var t = GetSelectedTool();
            if (t == null) return;

            if (t.Status != "Found" || string.IsNullOrWhiteSpace(t.Path) || !File.Exists(t.Path))
            {
                MessageBox.Show("Tool not found. Use Download or Locate.", "ScumChecker");
                return;
            }

            Process.Start(new ProcessStartInfo { FileName = t.Path, UseShellExecute = true });
        }

        private void LocateSelectedTool()
        {
            var t = GetSelectedTool();
            if (t == null) return;

            if (!string.IsNullOrWhiteSpace(t.Path) && File.Exists(t.Path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{t.Path}\"",
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show("Path is empty. Download tool or place exe in known folder.", "ScumChecker");
            }
        }

        private void DownloadSelectedTool()
        {
            var t = GetSelectedTool();
            if (t == null) return;

            if (string.IsNullOrWhiteSpace(t.DownloadUrl))
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = t.DownloadUrl,
                UseShellExecute = true
            });
        }

    }
}
