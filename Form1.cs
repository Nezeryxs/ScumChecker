using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using ScumChecker.Core;
using ScumChecker.Core.Tools;
using ScumChecker.Core.Steam;
using ScumChecker.Controls;

namespace ScumChecker
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource? _cts;
        private readonly List<ScanItem> _allItems = new();
        private List<ToolEntry> _tools = new();

        private string _lang = "RU"; // RU / EN

        // ===== Steam cards UI
        private SteamAccountCard? _cardCurrent;
        private FlowLayoutPanel? _flowOther;

        public Form1()
        {
            InitializeComponent();

            // ✅ Glow на sidebar-кнопках + иконки из ресурсов
            UpgradeSidebarButtonsToGlow();

            // ===== Icons for top scan buttons
            TryApply("btnScan", () => Properties.Resources.icon_scan, btnScan);
            TryApply("btnCancel", () => Properties.Resources.icon_cancel, btnCancel);
            TryApply("btnCopyReport", () => Properties.Resources.icon_tools, btnCopyReport);

            // ===== Icons for tools bottom buttons
            TryApply("btnOpenTool", () => Properties.Resources.icon_scan, btnOpenTool);
            TryApply("btnLocateTool", () => Properties.Resources.icon_path, btnLocateTool);
            TryApply("btnDownloadTool", () => Properties.Resources.icon_download, btnDownloadTool);

            // NAV
            btnNavNative.Click += (_, __) => ShowPage(pageNative, btnNavNative);
            btnNavSteam.Click += (_, __) => ShowPage(pageSteam, btnNavSteam);
            btnNavTools.Click += (_, __) => ShowPage(pageTools, btnNavTools);
            btnNavQuick.Click += (_, __) => ShowPage(pageQuick, btnNavQuick);

            // Links
            linkGitHub.LinkClicked += (_, __) => OpenUrl("https://github.com/Nezeryxs");
            linkBio.LinkClicked += (_, __) => OpenUrl("https://e-z.bio/nezeryxs");

            // Scan events
            btnScan.Click += btnScan_Click;
            btnCancel.Click += (_, __) => _cts?.Cancel();
            btnCopyReport.Click += (_, __) => CopyReportToClipboard();

            // Filters
            chkInfo.CheckedChanged += (_, __) => ApplyFilters();
            chkLow.CheckedChanged += (_, __) => ApplyFilters();
            chkMedium.CheckedChanged += (_, __) => ApplyFilters();
            chkHigh.CheckedChanged += (_, __) => ApplyFilters();

            // DGV visuals (native)
            dgvFindings.CellPainting += DgvFindings_CellPainting;
            dgvFindings.CellDoubleClick += DgvFindings_CellDoubleClick;
            dgvFindings.RowTemplate.Height = 28;

            // Tools dgv
            dgvTools.CellDoubleClick += (_, e2) =>
            {
                if (e2.RowIndex >= 0) OpenSelectedTool();
            };

            // Tools buttons
            btnOpenTool.Click += (_, __) => OpenSelectedTool();
            btnLocateTool.Click += (_, __) => LocateSelectedTool();
            btnDownloadTool.Click += (_, __) => DownloadSelectedTool();

            // Language dropdown
            if (cmbLang.Items.Count == 0)
                cmbLang.Items.AddRange(new object[] { "RU", "EN" });

            cmbLang.SelectedIndexChanged += (_, __) =>
            {
                var v = cmbLang.SelectedItem?.ToString() ?? "RU";
                SetLanguage(v);
            };

            // Quick tiles styling + actions
            InitQuickTiles();
            WireQuickActions();

            // Progress track width changed -> recompute fill
            panelProgressTrack.SizeChanged += (_, __) => SetProgress(GetProgressPercentSafe());

            // Default language UI
            cmbLang.SelectedItem = "RU";
            SetLanguage("RU");

            // Default page
            ShowPage(pageNative, btnNavNative);

            // Fix progress initial
            SetProgress(0);

            // Summary initial
            ResetSummary();
        }

        private void TryApply(string name, Func<Image> getter, Button b, int size = 18)
        {
            try { ApplyButtonIcon(b, getter(), size); }
            catch { /* ignore */ }
        }

        // =========================================================
        // GLOW ICON SIDEBAR
        // =========================================================
        private void UpgradeSidebarButtonsToGlow()
        {
            btnNavNative = ReplaceWithGlowByName("btnNavNative", () => Properties.Resources.icon_native) ?? btnNavNative;
            btnNavSteam = ReplaceWithGlowByName("btnNavSteam", () => Properties.Resources.icon_steam) ?? btnNavSteam;
            btnNavTools = ReplaceWithGlowByName("btnNavTools", () => Properties.Resources.icon_tools) ?? btnNavTools;
            btnNavQuick = ReplaceWithGlowByName("btnNavQuick", () => Properties.Resources.icon_quick) ?? btnNavQuick;
        }

        private static Image ResizeIcon(Image src, int size)
        {
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                float scale = Math.Min((float)size / src.Width, (float)size / src.Height);
                int w = (int)(src.Width * scale);
                int h = (int)(src.Height * scale);
                int x = (size - w) / 2;
                int y = (size - h) / 2;

                g.DrawImage(src, new Rectangle(x, y, w, h));
            }
            return bmp;
        }

        private void ApplyButtonIcon(Button b, Image icon, int iconSize = 18, int leftPad = 12)
        {
            if (b == null || icon == null) return;

            b.AutoSize = false;
            b.Image = ResizeIcon(icon, iconSize);
            b.ImageAlign = ContentAlignment.MiddleLeft;
            b.TextAlign = ContentAlignment.MiddleLeft;
            b.TextImageRelation = TextImageRelation.ImageBeforeText;
            b.Padding = new Padding(leftPad, 0, 10, 0);
        }

        private Button? ReplaceWithGlowByName(string controlName, Func<Image> iconGetter)
        {
            if (panelSidebar == null) return null;

            var found = panelSidebar.Controls.Find(controlName, true).FirstOrDefault();
            if (found is not Button oldBtn) return null;

            var parent = oldBtn.Parent;
            if (parent == null) return null;

            int idx = parent.Controls.GetChildIndex(oldBtn);

            var gb = new GlowIconButton
            {
                Name = oldBtn.Name,
                Text = oldBtn.Text,
                Dock = oldBtn.Dock,
                Size = oldBtn.Size,
                Location = oldBtn.Location,
                Margin = oldBtn.Margin,

                BackColor = oldBtn.BackColor,
                ForeColor = oldBtn.ForeColor,
                Font = oldBtn.Font,
                TabIndex = oldBtn.TabIndex,

                FlatStyle = FlatStyle.Flat,

                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding(12, 0, 10, 0),
            };

            gb.GlowColor = Color.FromArgb(200, 120, 110, 255);
            gb.GlowRadius = 12;
            gb.GlowStrength = 10;
            gb.IconSize = 18;

            gb.FlatAppearance.BorderSize = 1;
            gb.FlatAppearance.BorderColor = oldBtn.FlatAppearance.BorderColor;

            try { gb.Image = iconGetter(); } catch { /* ignore */ }

            parent.Controls.Remove(oldBtn);
            parent.Controls.Add(gb);
            parent.Controls.SetChildIndex(gb, idx);

            return gb;
        }

        // =========================================================
        // FORM LOAD
        // =========================================================
        private void Form1_Load(object sender, EventArgs e)
        {
            // ✅ распаковываем утилиты из exe
            var toolsDir = EnsureBundledProgrammsExtracted();

            // если у тебя есть ToolsDetector — можно сохранить путь
            ToolsDetector.SetBaseDirectory(toolsDir); // если такого метода нет — скажи

            RefreshTools();
            dgvFindings.ClearSelection();
            dgvTools.ClearSelection();

            BuildSteamCardsUi();
        }


        // =========================================================
        // NAV
        // =========================================================
        private void ShowPage(Panel page, Button activeBtn)
        {
            pageNative.Visible = page == pageNative;
            pageSteam.Visible = page == pageSteam;
            pageTools.Visible = page == pageTools;
            pageQuick.Visible = page == pageQuick;

            SetNavActive(btnNavNative, activeBtn == btnNavNative);
            SetNavActive(btnNavSteam, activeBtn == btnNavSteam);
            SetNavActive(btnNavTools, activeBtn == btnNavTools);
            SetNavActive(btnNavQuick, activeBtn == btnNavQuick);
        }

        private void SetNavActive(Button b, bool active)
        {
            if (b is GlowIconButton gi)
            {
                gi.Selected = active;
                gi.FlatAppearance.BorderColor = active ? Color.FromArgb(120, 110, 255) : Color.FromArgb(60, 60, 80);
                gi.BackColor = active ? Color.FromArgb(16, 12, 28) : Color.FromArgb(12, 12, 18);
                gi.ForeColor = active ? Color.White : Color.Gainsboro;
                gi.Invalidate();
                return;
            }

            b.FlatAppearance.BorderColor = active ? Color.FromArgb(120, 110, 255) : Color.FromArgb(60, 60, 80);
            b.BackColor = active ? Color.FromArgb(16, 12, 28) : Color.FromArgb(12, 12, 18);
            b.ForeColor = active ? Color.White : Color.Gainsboro;
        }

        // =========================================================
        // QUICK ACCESS
        // =========================================================
        private void InitQuickTiles()
        {
            flowQuick.WrapContents = true;
            flowQuick.FlowDirection = FlowDirection.LeftToRight;
            flowQuick.Padding = new Padding(14, 14, 14, 14);
            flowQuick.AutoScroll = true;

            InitQuickButton(btnOpenRegedit, "Registry");
            InitQuickButton(btnOpenTemp, "Temp");
            InitQuickButton(btnOpenDownloads, "Downloads");
            InitQuickButton(btnOpenWindowsUpdate, "Windows Update");
            InitQuickButton(btnOpenAppData, "AppData");
            InitQuickButton(btnOpenSteamConfig, "Steam config");
        }

        private void WireQuickActions()
        {
            btnOpenRegedit.Click += (_, __) => OpenUrl("regedit");
            btnOpenTemp.Click += (_, __) => OpenPath(Path.GetTempPath());

            btnOpenDownloads.Click += (_, __) => OpenPath(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"));

            btnOpenWindowsUpdate.Click += (_, __) => OpenUrl("ms-settings:windowsupdate");

            btnOpenAppData.Click += (_, __) =>
            {
                OpenPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            };

            btnOpenSteamConfig.Click += (_, __) =>
            {
                var pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

                var cand = new[]
                {
                    Path.Combine(pf86, "Steam", "config", "loginusers.vdf"),
                    Path.Combine(pf, "Steam", "config", "loginusers.vdf"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Steam", "config", "loginusers.vdf"),
                };

                var found = cand.FirstOrDefault(File.Exists);
                if (!string.IsNullOrWhiteSpace(found)) OpenPath(found);
                else MessageBox.Show(T("Файл loginusers.vdf не найден.", "loginusers.vdf not found."), "ScumChecker");
            };
        }

        // =========================================================
        // LANGUAGE
        // =========================================================
        private void SetLanguage(string lang)
        {
            _lang = (lang == "EN") ? "EN" : "RU";

            btnNavNative.Text = T("Проверка процесса игры", "Game process scan");
            btnNavSteam.Text = T("Проверка аккаунтов", "Accounts check");
            btnNavTools.Text = T("Программы", "Programs");
            btnNavQuick.Text = T("Быстрый доступ", "Quick access");

            btnOpenRegedit.Text = T("Реестр ПК", "Registry");
            btnOpenTemp.Text = T("Temp", "Temp");
            btnOpenDownloads.Text = T("Загрузки", "Downloads");
            btnOpenWindowsUpdate.Text = T("Обновления Windows", "Windows Update");
            btnOpenAppData.Text = T("AppData", "AppData");
            btnOpenSteamConfig.Text = T("Steam: loginusers.vdf", "Steam: loginusers.vdf");

            btnScan.Text = T("Скан", "Scan");
            btnCancel.Text = T("Отмена", "Cancel");
            btnCopyReport.Text = T("Копировать отчёт", "Copy report");

            lblVerdictTitle.Text = T("Вердикт:", "Verdict:");

            lblSteamTitle.Text = T("Steam аккаунты", "Steam accounts");
            lblSteamHint.Text = T(
                "Ниже отображаются Steam-аккаунты, ранее использовавшиеся на этом ПК, с детальной информацией о VAC и игровых блокировках.",
                "Below are the Steam accounts previously used on this PC, with detailed information about VAC and game bans."
            );

            lblToolsTitle.Text = T("Tools for moderation", "Tools for moderation");
            lblToolsDesc.Text = T(
                "Утилиты для администраторов и модераторов",
                "Utilities for administrators and moderators"
            );
            lblToolsHint.Text = T(
                "Выбери утилиту → Open / Locate / Download",
                "Select tool → Open / Locate / Download"
            );

            lblQuickTitle.Text = T("Быстрый доступ", "Quick access");
            lblQuickDesc.Text = T(
                "Быстрый доступ к реестру, временным файлам, загрузкам, обновлениям и другим системным разделам.",
                "Quick access to the registry, temporary files, downloads, updates, and other system locations."
            );


            lblStatus.Text = T("Статус: Ожидание", "Status: Idle");

            colSeverity.HeaderText = "Severity";
            colCategory.HeaderText = "Category";
            colWhat.HeaderText = "What";
            colReason.HeaderText = "Why flagged";
            colAction.HeaderText = "Recommended action";
            colDetails.HeaderText = "Details";

            UpdateSummary();
            // ✅ обновить Steam карточки под новый язык
            if (_flowSteam != null)
            {
                _ = RefreshSteamCardsAsync(); // можно без await
            }
        }

        private string T(string ru, string en) => _lang == "EN" ? en : ru;

        // =========================================================
        // SCAN
        // =========================================================
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
                lblStatus.Text = T("Статус: ", "Status: ") + p.Stage;
                SetProgress(Math.Clamp(p.Percent, 0, 100));
            });

            scanner.ItemFound += item => SafeUi(() =>
            {
                // ✅ авто-эскалация по критичным ключам
                EscalateSeverityIfCritical(item);

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
                SafeUi(() => txtLog.AppendText(T("Отменено.\r\n", "Canceled.\r\n")));
            }
            finally
            {
                SafeUi(() =>
                {
                    btnScan.Enabled = true;
                    btnCancel.Enabled = false;
                    lblStatus.Text = T("Статус: Ожидание", "Status: Idle");
                    SetProgress(0);
                });

                // ОБНОВЛЯЕМ Steam карточки (асинхронно)
                try { await RefreshSteamCardsAsync(); } catch { /* ignore */ }

                _cts?.Dispose();
                _cts = null;
            }
        }

        private int _lastProgress = 0;
        private int GetProgressPercentSafe() => _lastProgress;

        private void SetProgress(int percent)
        {
            percent = Math.Clamp(percent, 0, 100);
            _lastProgress = percent;

            int trackW = panelProgressTrack.Width;
            int fillW = (int)(trackW * (percent / 100f));
            fillW = Math.Max(0, Math.Min(trackW, fillW));
            panelProgressFill.Width = fillW;
        }

        private void EscalateSeverityIfCritical(ScanItem item)
        {
            // собираем “где искать” (и путь, и детали, и what/reason)
            var hay = $"{item.Title} {item.What} {item.Category} {item.Reason} {item.Details} {item.EvidencePath} {item.Url}";

            // критичные слова — только для Filesystem/Processes (чтобы не триггерить Steam строки)
            bool canEscalate =
                item.Category.Equals("Filesystem", StringComparison.OrdinalIgnoreCase) ||
                item.Category.Equals("Processes", StringComparison.OrdinalIgnoreCase);

            if (!canEscalate) return;

            if (ScumChecker.Core.Modules.SuspicionKeywords.ContainsCritical(hay))
            {
                item.Severity = Severity.High;

                // (опционально) усилим текст причины/рекомендации
                if (string.IsNullOrWhiteSpace(item.Reason))
                    item.Reason = "Critical keyword match";
                else if (!item.Reason.Contains("Critical", StringComparison.OrdinalIgnoreCase))
                    item.Reason = "Critical keyword match | " + item.Reason;

                if (string.IsNullOrWhiteSpace(item.Recommendation))
                    item.Recommendation = "Manual review. Consider as high risk.";
            }
        }


        private SteamAccountCard CreateSteamCard()
        {
            var card = new SteamAccountCard
            {
                AutoSize = false,
                Dock = DockStyle.None,
                Margin = SteamCardMargin,

                Size = SteamCardSize,
                MinimumSize = SteamCardSize,
                MaximumSize = SteamCardSize,
            };

            // (опционально) плейсхолдер, чтобы выглядело не пусто до загрузки
            card.SetHeader(T("Загрузка…", "Loading…"), null);
            card.ClearRows();
            card.AddRow(T("Получаю данные Steam…", "Fetching Steam data…"), null);

            return card;
        }



        // =========================================================
        // TABLES / FILTERS
        // =========================================================
        private void AddRow(ScanItem item)
        {
            if (!PassSeverityFilter(item.Severity)) return;

            int rowIndex = dgvFindings.Rows.Add(
                item.Severity.ToString(),
                item.Category,
                item.What,
                item.Reason,
                item.Recommendation,
                item.Details
            );

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

        // =========================================================
        // DOUBLE CLICK ACTIONS
        // =========================================================
        private void DgvFindings_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvFindings.Rows[e.RowIndex];
            if (row.Tag is not ScanItem item) return;

            if (!string.IsNullOrWhiteSpace(item.Url))
            {
                OpenUrl(item.Url!);
                return;
            }

            if (!string.IsNullOrWhiteSpace(item.EvidencePath))
            {
                OpenPath(item.EvidencePath!);
                return;
            }
        }

        // =========================================================
        // BADGE RENDER: Severity
        // =========================================================
        private void DgvFindings_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex != 0) return;

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
                _ => Color.FromArgb(90, 150, 255)
            };

            string shown = text;
            if (_lang == "RU")
            {
                shown = text switch
                {
                    "High" => "Высокий",
                    "Medium" => "Средний",
                    "Low" => "Низкий",
                    "Info" => "Инфо",
                    _ => text
                };
            }

            var rect = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y + 6, e.CellBounds.Width - 16, e.CellBounds.Height - 12);
            rect.Width = Math.Min(rect.Width, 110);

            using var brush = new SolidBrush(badge);
            using var textBrush = new SolidBrush(Color.Black);

            e.Graphics.FillRectangle(brush, rect);

            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString(shown, e.CellStyle.Font, textBrush, rect, sf);

            using var pen = new Pen(Color.FromArgb(40, 40, 50));
            e.Graphics.DrawRectangle(pen, rect);
        }

        // =========================================================
        // SUMMARY + VERDICT
        // =========================================================
        private void ResetSummary()
        {
            lblCountHigh.Text = "High: 0";
            lblCountMedium.Text = "Medium: 0";
            lblCountLow.Text = "Low: 0";
            lblCountInfo.Text = "Info: 0";
            lblVerdict.Text = T("Нет данных (запусти скан)", "No data (run Scan)");
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

            if (hi > 0)
                lblVerdict.Text = T(
                    "Найдены индикаторы высокого риска → нужна ручная проверка",
                    "High risk indicators found → manual review recommended"
                );
            else if (me > 0)
                lblVerdict.Text = T(
                    "Есть подозрительные индикаторы → ручная проверка (без мгновенного бана)",
                    "Suspicious indicators found → manual review (no instant ban)"
                );
            else if (_allItems.Count > 0)
                lblVerdict.Text = T(
                    "Высокорисковых индикаторов не найдено",
                    "No high-risk indicators found"
                );
        }

        // =========================================================
        // COPY REPORT
        // =========================================================
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
            txtLog.AppendText(T("Отчёт скопирован в буфер.\r\n", "Report copied to clipboard.\r\n"));
        }

        // =========================================================
        // STEAM CARDS UI (как на скрине)
        // =========================================================
        private FlowLayoutPanel? _flowSteam;

        // единый размер всех карточек
        // единый размер всех карточек
        private static readonly Size SteamCardSize = new Size(320, 250);
        private static readonly Padding SteamCardMargin = new Padding(12);




        private void BuildSteamCardsUi()
        {
            panelSteamGridHost.Controls.Clear();
            panelSteamGridHost.BackColor = Color.FromArgb(12, 12, 18);

            _flowSteam = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(12, 12, 18)
            };

            panelSteamGridHost.Controls.Add(_flowSteam);
        }

        private async Task RefreshSteamCardsAsync()
        {
            if (_flowSteam == null) return;

            SafeUi(() => _flowSteam.SuspendLayout());
            try
            {
                SafeUi(() => _flowSteam.Controls.Clear());

                // собираем аккаунты
                var accounts = new List<(string steamId, string account, string mostRecent, string timestamp)>();

                foreach (var it in _allItems)
                {
                    if (it.Category != "Steam") continue;
                    if (it.What != "Steam account" && it.Title != "Steam account") continue;

                    string steamId = "";
                    if (!string.IsNullOrWhiteSpace(it.Url))
                    {
                        var url = it.Url.TrimEnd('/');
                        var idx = url.LastIndexOf('/');
                        steamId = idx >= 0 ? url[(idx + 1)..] : url;
                    }
                    if (string.IsNullOrWhiteSpace(steamId)) continue;

                    string account = "";
                    string mostRecent = "";
                    string timestamp = "";

                    var parts = it.Details.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    if (parts.Length > 0)
                        account = parts[0];

                    foreach (var p in parts)
                    {
                        if (p.StartsWith("MostRecent=", StringComparison.OrdinalIgnoreCase))
                            mostRecent = p["MostRecent=".Length..].Trim();
                        else if (p.StartsWith("Timestamp=", StringComparison.OrdinalIgnoreCase))
                            timestamp = p["Timestamp=".Length..].Trim();
                    }

                    accounts.Add((steamId, account, mostRecent, timestamp));
                }

                if (accounts.Count == 0)
                {
                    var empty = CreateSteamCard();
                    empty.SetHeader(T("Аккаунты не найдены", "No accounts found"), null);
                    empty.ClearRows();
                    empty.AddRow(T("Проверь Steam/config/loginusers.vdf", "Check Steam/config/loginusers.vdf"), null);
                    SafeUi(() => _flowSteam.Controls.Add(empty));
                    return;
                }

                // текущий аккаунт
                var current = accounts.FirstOrDefault(a => a.mostRecent.Equals("true", StringComparison.OrdinalIgnoreCase));
                if (string.IsNullOrWhiteSpace(current.steamId))
                    current = accounts[0];

                // порядок: текущий первым, остальные следом
                var ordered = new List<(string steamId, string account, string timestamp)>();
                ordered.Add((current.steamId, current.account, current.timestamp));
                ordered.AddRange(accounts
                    .Where(x => x.steamId != current.steamId)
                    .Select(x => (x.steamId, x.account, x.timestamp)));

                // 1) СНАЧАЛА создаём и добавляем ВСЕ карточки фиксированного размера
                var cards = new List<(SteamAccountCard card, string id, string acc, string ts)>();
                foreach (var a in ordered)
                {
                    var card = CreateSteamCard();
                    cards.Add((card, a.steamId, a.account, a.timestamp));
                    SafeUi(() => _flowSteam.Controls.Add(card));
                }

                // важный момент: после добавления всех — один layout-pass
                SafeUi(() =>
                {
                    _flowSteam.ResumeLayout(true);
                    _flowSteam.PerformLayout();
                    _flowSteam.SuspendLayout(); // дальше уже не даём ему беситься от обновлений
                });

                // 2) Теперь заполняем: текущий ждём, остальные параллельно
                await FillOneCardAsync(cards[0].card, cards[0].id, cards[0].acc, cards[0].ts);

                for (int i = 1; i < cards.Count; i++)
                {
                    var c = cards[i];
                    _ = FillOneCardAsync(c.card, c.id, c.acc, c.ts);
                }
            }
            finally
            {
                SafeUi(() =>
                {
                    _flowSteam.ResumeLayout(true);
                    _flowSteam.PerformLayout();
                });
            }
        }


        private async Task FillOneCardAsync(SteamAccountCard card, string steamId64, string accountName, string timestamp)
        {
            // 1) Ник + аватар (без ключа)
            SteamProfileScraper.SteamProfileLite? prof = null;
            try { prof = await SteamProfileScraper.GetProfileLiteAsync(steamId64); } catch { /* ignore */ }

            Image? avatar = null;
            if (prof?.AvatarUrl != null)
            {
                try
                {
                    using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                    var bytes = await http.GetByteArrayAsync(prof.AvatarUrl);
                    using var ms = new MemoryStream(bytes);
                    avatar = Image.FromStream(ms);
                }
                catch { /* ignore */ }
            }

            // 2) VAC/Game bans (без ключа) — SteamApi.GetBansNoKeyAsync
            SteamApi.BanLite bans;
            try { bans = await SteamApi.GetBansNoKeyAsync(steamId64); }
            catch { bans = SteamApi.BanLite.UnknownResult(); }

            SafeUi(() =>
            {
                var name = prof?.PersonaName ?? steamId64;

                card.SuspendLayout();

                card.SetHeader(name, avatar);
                card.ClearRows();

                card.AddRow($"{T("Блокировка VAC", "VAC banned")}: {FormatYesNoUnknown(bans.Unknown, bans.VacBanned)}", null);
                card.AddRow($"{T("VAC банов", "VAC bans")}: {(bans.Unknown ? "-" : bans.VacBans.ToString())}", null);
                card.AddRow($"{T("Game bans", "Game bans")}: {(bans.Unknown ? "-" : bans.GameBans.ToString())}", null);
                card.AddRow($"{T("Дней с последнего", "Days since last ban")}: {(bans.DaysSinceLastBan.HasValue ? bans.DaysSinceLastBan.Value.ToString() : "-")}", null);
                card.AddRow($"{T("Аккаунт", "Account")}: {accountName}", null);
                card.AddRow($"{T("Последний вход", "Last logon")}: {timestamp}", null);


                // Быстрый переход в профиль
                card.Cursor = Cursors.Hand;
                card.Click -= Card_Click;
                card.Click += Card_Click;
                card.Tag = steamId64;


                card.ResumeLayout(true);
                card.PerformLayout();
                card.Invalidate();
            });
        }

        private void Card_Click(object? sender, EventArgs e)
        {
            if (sender is Control c && c.Tag is string id && !string.IsNullOrWhiteSpace(id))
                OpenUrl($"https://steamcommunity.com/profiles/{id}/");
        }

        private string FormatYesNoUnknown(bool unknown, bool value)
        {
            if (unknown) return T("Неизвестно", "Unknown");
            return value ? T("Да", "Yes") : T("Нет", "No");
        }


        // =========================================================
        // TOOLS
        // =========================================================
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
                MessageBox.Show(T("Утилита не найдена. Используй Download или Locate.", "Tool not found. Use Download or Locate."), "ScumChecker");
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
                MessageBox.Show(T("Путь пуст. Скачай утилиту или положи exe в известную папку.", "Path is empty. Download tool or place exe in known folder."), "ScumChecker");
            }
        }

        private void DownloadSelectedTool()
        {
            var t = GetSelectedTool();
            if (t == null) return;
            if (string.IsNullOrWhiteSpace(t.DownloadUrl)) return;

            Process.Start(new ProcessStartInfo
            {
                FileName = t.DownloadUrl,
                UseShellExecute = true
            });
        }

        // =========================================================
        // HELPERS
        // =========================================================
        private void OpenUrl(string urlOrProtocol)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = urlOrProtocol,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                txtLog.AppendText(T("Ошибка открытия: ", "Open error: ") + ex.Message + "\r\n");
            }
        }

        private void OpenPath(string path)
        {
            try
            {
                if (File.Exists(path))
                {
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
                else
                {
                    MessageBox.Show(T("Путь не найден: ", "Path not found: ") + path, "ScumChecker");
                }
            }
            catch (Exception ex)
            {
                txtLog.AppendText(T("Ошибка открытия пути: ", "Open path error: ") + ex.Message + "\r\n");
            }
        }
        private static string EnsureBundledProgrammsExtracted()
        {
            var baseDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ScumChecker",
                "programms");

            Directory.CreateDirectory(baseDir);

            var asm = Assembly.GetExecutingAssembly();
            var names = asm.GetManifestResourceNames();

            foreach (var resName in names)
            {
                // ресурсы будут вида "ScumChecker.programms.xxx.yyy.exe"
                if (!resName.Contains(".programms.", StringComparison.OrdinalIgnoreCase))
                    continue;

                // делаем относительный путь внутри programms
                var idx = resName.IndexOf(".programms.", StringComparison.OrdinalIgnoreCase);
                var relative = resName[(idx + ".programms.".Length)..];

                // из-за точек в resName путь будет "folder.exe" -> ок
                // если хочешь подпапки — лучше хранить zip (скажу ниже)
                var outPath = Path.Combine(baseDir, relative);

                if (File.Exists(outPath)) continue;

                using var s = asm.GetManifestResourceStream(resName);
                if (s == null) continue;

                using var fs = new FileStream(outPath, FileMode.Create, FileAccess.Write);
                s.CopyTo(fs);
            }

            return baseDir;
        }

        private void SafeUi(Action a)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke(a);
            else a();
        }
    }
}
