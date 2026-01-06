using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using ScumChecker.Core;
using ScumChecker.Core.Tools;
using ScumChecker.Core.Steam;
using ScumChecker.Controls;



namespace ScumChecker
{
    public partial class Form1 : Form
    {

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
            int DWMWCP_ROUND = 2;

            DwmSetWindowAttribute(
                Handle,
                DWMWA_WINDOW_CORNER_PREFERENCE,
                ref DWMWCP_ROUND,
                sizeof(int)
            );
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            ref int attrValue,
            int attrSize
        );

        private CancellationTokenSource? _cts;

        private readonly List<ScanItem> _allItems = new();
        private List<ToolEntry> _tools = new();

        private string _lang = "RU"; // RU / EN

        // ===== Tools tiles UI
        private FlowLayoutPanel? _flowToolsTiles;
        private ToolEntry? _selectedTool;

        private Button btnGitHubFooter = null!;
        private Button btnBioFooter = null!;



        // ===== Tools action cards (bottom)
        private ToolActionCard cardOpenTool = null!;
        private ToolActionCard cardLocateTool = null!;
        private ToolActionCard cardDownloadTool = null!;
        private Label lblToolsSubHint = null!;



        // ===== Steam cards UI (твоя текущая логика)
        private FlowLayoutPanel? _flowSteam;
        private static readonly Size SteamCardSize = new Size(210, 250);
        private static readonly Padding SteamCardMargin = new Padding(12);

        public Form1()
        {
            InitializeComponent();

            button3akrit.Click += (_, __) => Close();
            buttonSvernut.Click += (_, __) => WindowState = FormWindowState.Minimized;

            PanelLogo.MouseDown += Title_MouseDown;
            PanelLogo.MouseMove += Title_MouseMove;
            PanelLogo.MouseUp += Title_MouseUp;

            StartPosition = FormStartPosition.CenterScreen;
            MaximumSize = new Size(1220, 720); // или под дизайн
            MinimumSize = new Size(1200, 700);


            FormBorderStyle = FormBorderStyle.None;
            ControlBox = false;
            MaximizeBox = false;
            MinimizeBox = false;


            flowQuick.SizeChanged += (_, __) => UpdateQuickSectionWidths();

            // Glow sidebar + icons
            UpgradeSidebarButtonsToGlow();

            // top icons
            TryApply("btnScan", () => Properties.Resources.icon_scan, btnScan);
            TryApply("btnCancel", () => Properties.Resources.icon_cancel, btnCancel);
            TryApply("btnCopyReport", () => Properties.Resources.icon_tools, btnCopyReport);

            // bottom tools cards icons (твои)
            // (иконки можно заменить)
            InitToolsActionCardsUi();

            // NAV
            btnNavNative.Click += (_, __) => ShowPage(pageNative, btnNavNative);
            btnNavSteam.Click += (_, __) => ShowPage(pageSteam, btnNavSteam);
            btnNavTools.Click += (_, __) => ShowPage(pageTools, btnNavTools);
            btnNavQuick.Click += (_, __) => ShowPage(pageQuick, btnNavQuick);

            // Links
            //  linkGitHub.LinkClicked += (_, __) => OpenUrl("https://github.com/Nezeryxs");
            // linkBio.LinkClicked += (_, __) => OpenUrl("https://e-z.bio/nezeryxs");

            // Scan events
            btnScan.Click += btnScan_Click;
            btnCancel.Click += (_, __) => _cts?.Cancel();
            btnCopyReport.Click += (_, __) => CopyReportToClipboard();

            // Filters
            chkInfo.CheckedChanged += (_, __) => ApplyFilters();
            chkLow.CheckedChanged += (_, __) => ApplyFilters();
            chkMedium.CheckedChanged += (_, __) => ApplyFilters();
            chkHigh.CheckedChanged += (_, __) => ApplyFilters();

            // DGV native
            dgvFindings.CellPainting += DgvFindings_CellPainting;
            dgvFindings.CellDoubleClick += DgvFindings_CellDoubleClick;
            dgvFindings.RowTemplate.Height = 28;

            // Language dropdown
            if (cmbLang.Items.Count == 0)
                cmbLang.Items.AddRange(new object[] { "RU", "EN" });

            cmbLang.SelectedIndexChanged += (_, __) =>
            {
                var v = cmbLang.SelectedItem?.ToString() ?? "RU";
                SetLanguage(v);
                UpdateToolsActionsState(); // обновит Open/Locate/Download под новый _lang

            };

            // Quick tiles
            InitQuickTiles();
            BuildQuickSections();
            UiPost(UpdateQuickSectionWidths);
            WireQuickActions();

            // Progress fill resize -> recompute
            panelProgressTrack.SizeChanged += (_, __) => SetProgress(_lastProgress);

            // default language
            cmbLang.SelectedItem = "RU";
            SetLanguage("RU");

            // default page
            ShowPage(pageNative, btnNavNative);


            // init
            SetProgress(0);
            ResetSummary();
        }

        // =========================================================
        // FORM LOAD
        // =========================================================

        private bool _dragging;
        private Point _dragStart;

        private void Title_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _dragging = true;
            _dragStart = e.Location;
        }

        private void Title_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_dragging) return;

            var screen = PointToScreen(e.Location);
            Location = new Point(screen.X - _dragStart.X, screen.Y - _dragStart.Y);
        }

        private void Title_MouseUp(object? sender, MouseEventArgs e)
        {
            _dragging = false;
        }




        private void Form1_Load(object sender, EventArgs e)
        {
            // Steam cards host
            BuildSteamCardsUi();

            // Extract bundled tools
            ToolsBundler.EnsureExtracted();
            ToolsDetector.SetBaseDirectory(ToolsBundler.GetProgramsDir());



            // Build tools tiles host
            BuildToolsTilesUi();

            // Load tools list
            RefreshToolsTiles();
        }

        // =========================================================
        // TOOLS UI: tiles (buttons/cards)
        // =========================================================
        private void BuildToolsTilesUi()
        {
            if (panelToolsListHost == null) return;

            panelToolsListHost.Controls.Clear();
            panelToolsListHost.BackColor = Color.FromArgb(12, 12, 18);

            _flowToolsTiles = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = false,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(12, 12, 18)
            };

            panelToolsListHost.Controls.Add(_flowToolsTiles);
        }

        private void RefreshToolsTiles()
        {
            _tools = ToolsDetector.Detect();
            _selectedTool = null;

            if (_flowToolsTiles == null) return;

            SafeUi(() => _flowToolsTiles.SuspendLayout());
            try
            {
                SafeUi(() => _flowToolsTiles.Controls.Clear());

                foreach (var t in _tools)
                {
                    var card = new ToolTileCard
                    {
                        Width = Math.Max(600, panelToolsListHost.Width - 40),
                        Title = GetToolTitle(t.Name),
                        Description = GetToolDescriptionTranslated(t.Name),
                        Icon = GetToolIcon(t.Name),
                        Payload = t
                    };

                    ApplyToolStatusBadge(card, t);

                    card.Clicked += ToolCard_Clicked;
                    card.DoubleClicked += ToolCard_DoubleClicked;

                    _flowToolsTiles.Controls.Add(card);
                }


                // авто-выбор первой (если есть)
                if (_tools.Count > 0)
                {
                    SelectToolFromUi(_tools[0]);
                }
                else
                {
                    UpdateToolsActionsState();
                }
            }
            finally
            {
                SafeUi(() =>
                {
                    _flowToolsTiles.ResumeLayout(true);
                    _flowToolsTiles.PerformLayout();
                });
            }
        }

        private void ToolCard_Clicked(object? sender, EventArgs e)
        {
            if (sender is ToolTileCard card && card.Payload is ToolEntry t)
                SelectToolFromUi(t);
        }

        private void ToolCard_DoubleClicked(object? sender, EventArgs e)
        {
            if (sender is ToolTileCard card && card.Payload is ToolEntry t)
            {
                SelectToolFromUi(t);
                OpenSelectedTool();
            }
        }


        private void ApplyToolStatusBadge(ToolTileCard card, ToolEntry t)
        {
            bool found = t.Status == "Found" && !string.IsNullOrWhiteSpace(t.Path) && File.Exists(t.Path);

            card.StatusText = found ? "Found" : "Not found";
            card.StatusColor = found
                ? Color.FromArgb(120, 230, 140)
                : Color.FromArgb(230, 155, 60);

            // под язык можно сделать:
            if (_lang == "RU")
            {
                card.StatusText = found ? "Найдено" : "Не найдено";
            }
        }

        private void InitQuickTiles()
        {


            // ===== Cheats markets (links)
            btnOpenYougame ??= new ScumChecker.Controls.QuickTileButton();
            btnOpenDragonHack ??= new ScumChecker.Controls.QuickTileButton();
            btnOpenUpGame ??= new ScumChecker.Controls.QuickTileButton();
            btnOpenIndustries ??= new ScumChecker.Controls.QuickTileButton();
            btnOpenCheatrise ??= new ScumChecker.Controls.QuickTileButton();
            btnOpenCyberhack ??= new ScumChecker.Controls.QuickTileButton();
            btnOpenSoftix ??= new ScumChecker.Controls.QuickTileButton();
            btnOpenScumFolder ??= new ScumChecker.Controls.QuickTileButton();

            InitQuickButton(btnOpenYougame, "yougame.biz");
            InitQuickButton(btnOpenDragonHack, "dragon-hack.pro");
            InitQuickButton(btnOpenUpGame, "up-game.pro");
            InitQuickButton(btnOpenIndustries, "industries-cheat.store");
            InitQuickButton(btnOpenCheatrise, "cheatrise.com");
            InitQuickButton(btnOpenCyberhack, "cyberhack.pro");
            InitQuickButton(btnOpenSoftix, "softixcheats.com");

            ApplyButtonIcon(btnOpenRegedit, Properties.Resources.ico_reg, 18);
            ApplyButtonIcon(btnOpenTemp, Properties.Resources.ico_temp, 18);
            ApplyButtonIcon(btnOpenDownloads, Properties.Resources.ico_downloads, 18);
            ApplyButtonIcon(btnOpenWindowsUpdate, Properties.Resources.ico_winupdate, 18);

            ApplyButtonIcon(btnOpenChromeProfile, Properties.Resources.ico_chrome, 18);
            ApplyButtonIcon(btnOpenEdgeProfile, Properties.Resources.ico_edge, 18);
            ApplyButtonIcon(btnOpenFirefoxProfiles, Properties.Resources.ico_firefox, 18);
            ApplyButtonIcon(btnOpenBrowserCache, Properties.Resources.ico_cache, 18);

            ApplyButtonIcon(btnOpenSteamConfig, Properties.Resources.ico_steam, 18);
            ApplyButtonIcon(btnOpenScumFolder, Properties.Resources.ico_scum, 18);

            // для сайтов можно один общий значок
            ApplyButtonIcon(btnOpenYougame, Properties.Resources.ico_shop, 18);
            ApplyButtonIcon(btnOpenDragonHack, Properties.Resources.ico_shop, 18);
            // ...


            InitQuickButton(btnOpenScumFolder, T("Папка игры SCUM", "SCUM game folder"));


            // ===== System
            InitQuickButton(btnOpenRegedit, T("Реестр ПК", "Registry"));
            InitQuickButton(btnOpenPrefetch, T("Prefetch (запуски)", "Prefetch (program runs)"));
            InitQuickButton(btnOpenEventViewer, T("Журналы Windows", "Event Viewer"));
            InitQuickButton(btnOpenTemp, T("Temp", "Temp"));
            InitQuickButton(btnOpenAppData, T("AppData (Roaming)", "AppData (Roaming)"));
            InitQuickButton(btnOpenLocalAppData, T("AppData (Local)", "AppData (Local)"));

            // ===== Browsers
            InitQuickButton(btnOpenChromeProfile, T("Chrome: профиль/история", "Chrome: profile/history"));
            InitQuickButton(btnOpenEdgeProfile, T("Edge: профиль/история", "Edge: profile/history"));
            InitQuickButton(btnOpenFirefoxProfiles, T("Firefox: profiles", "Firefox: profiles"));
            InitQuickButton(btnOpenBrowserCache, T("Browser cache", "Browser cache"));

            // ===== Files
            InitQuickButton(btnOpenDownloads, T("Загрузки", "Downloads"));
            InitQuickButton(btnOpenRecent, T("Недавние файлы", "Recent files"));
            InitQuickButton(btnOpenDesktop, T("Рабочий стол", "Desktop"));
            InitQuickButton(btnOpenSteamConfig, "Steam: loginusers.vdf");
            InitQuickButton(btnOpenWindowsUpdate, T("Обновления Windows", "Windows Update"));

            // System
            ApplyQuickIcon(btnOpenRegedit, Properties.Resources.ico_reg);
            ApplyQuickIcon(btnOpenTemp, Properties.Resources.ico_temp);
            ApplyQuickIcon(btnOpenDownloads, Properties.Resources.ico_downloads);
            ApplyQuickIcon(btnOpenWindowsUpdate, Properties.Resources.ico_winupdate);
            ApplyQuickIcon(btnOpenAppData, Properties.Resources.ico_appdata);
            ApplyQuickIcon(btnOpenLocalAppData, Properties.Resources.ico_appdata);
            ApplyQuickIcon(btnOpenPrefetch, Properties.Resources.ico_prefetch);
            ApplyQuickIcon(btnOpenEventViewer, Properties.Resources.ico_eventlog);

            // Browsers
            ApplyQuickIcon(btnOpenChromeProfile, Properties.Resources.ico_chrome);
            ApplyQuickIcon(btnOpenEdgeProfile, Properties.Resources.ico_edge);
            ApplyQuickIcon(btnOpenFirefoxProfiles, Properties.Resources.ico_firefox);
            ApplyQuickIcon(btnOpenBrowserCache, Properties.Resources.ico_cache);

            // Files
            ApplyQuickIcon(btnOpenRecent, Properties.Resources.ico_recent);
            ApplyQuickIcon(btnOpenDesktop, Properties.Resources.ico_desktop);
            ApplyQuickIcon(btnOpenSteamConfig, Properties.Resources.ico_steam);
            ApplyQuickIcon(btnOpenScumFolder, Properties.Resources.ico_scum);

            // Purchases (одна общая “корзина/магазин”)
            ApplyQuickIcon(btnOpenYougame, Properties.Resources.ico_shop);
            ApplyQuickIcon(btnOpenDragonHack, Properties.Resources.ico_shop);
            ApplyQuickIcon(btnOpenUpGame, Properties.Resources.ico_shop);
            ApplyQuickIcon(btnOpenIndustries, Properties.Resources.ico_shop);
            ApplyQuickIcon(btnOpenCheatrise, Properties.Resources.ico_shop);
            ApplyQuickIcon(btnOpenCyberhack, Properties.Resources.ico_shop);
            ApplyQuickIcon(btnOpenSoftix, Properties.Resources.ico_shop);

        }



        private void SelectToolFromUi(ToolEntry t)
        {
            _selectedTool = t;

            if (_flowToolsTiles != null)
            {
                foreach (Control c in _flowToolsTiles.Controls)
                {
                    if (c is ToolTileCard tc && tc.Payload is ToolEntry te)
                        tc.Selected = (te.Name == t.Name);
                }
            }

            UpdateToolsActionsState();
        }

        private ToolEntry? GetSelectedTool() => _selectedTool;

        private string GetToolDescription(string name)
        {
            // коротко и по делу, как ты хотел
            // (если твой ToolsDetector имена отличаются — подгони кейсы)
            return name.ToLowerInvariant() switch
            {
                "everything" =>
                    T("Мгновенный поиск файлов по диску (по названию).", "Instant file name search across disks."),
                "shellbag" or "shellbags" =>
                    T("История открытых папок/проводника (ShellBags).", "Explorer folder history (ShellBags)."),
                "cacheprogramlist" =>
                    T("Список программ по кешу/артефактам системы.", "Programs list from system cache/artifacts."),
                "executedprogramslist" =>
                    T("Следы запусков программ (Executed programs).", "Evidence of executed programs."),
                "journaltrace" =>
                    T("Следы активности по журналам/артефактам.", "Activity traces from journal/artifacts."),
                "lastactivityview" =>
                    T("Сводка последней активности пользователя.", "Summary of recent user activity."),
                "usbdeview" =>
                    T("История USB устройств (флешки, телефоны и т.д.).", "USB devices history (drives, phones, etc.)."),
                "usbdrivelog" =>
                    T("Логи подключений USB-накопителей (время/инфо).", "USB drive connection logs (time/info)."),
                _ =>
                    T("Утилита для модерации / анализа активности.", "Moderation / activity analysis utility.")
            };
        }

        private Image GetToolIcon(string name)
        {
            var key = NormalizeToolKey(name);

            return key switch
            {
                "everything" => Properties.Resources.everything_ico,

                "cachedprogramslist" or "cacheprogramlist"
                    => Properties.Resources.cacheprogramlist_ico,

                "executedprogramslist"
                    => Properties.Resources.executedprogramslist_ico,

                "journaltrace"
                    => Properties.Resources.JournalTrace_ico,

                "lastactivityview"
                    => Properties.Resources.lastactivityview_ico,

                "usbdeview"
                    => Properties.Resources.usbdeview_ico,

                "usbdrivelog"
                    => Properties.Resources.usbdrivelog_ico,


                _ => Properties.Resources.shellbag_ico
            };
        }

        private static string NormalizeToolKey(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            name = name.ToLowerInvariant().Trim();

            // отрезаем всё в скобках
            int idx = name.IndexOf('(');
            if (idx >= 0)
                name = name[..idx];

            // убираем пробелы / дефисы / подчёркивания
            name = name
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_", "");

            return name;
        }

        private string GetToolTitle(string name)
        {
            var key = NormalizeToolKey(name);

            return key switch
            {
                "everything" => T("Everything — поиск файлов", "Everything — file search"),
                "shellbagsexplorer" or "shellbag" or "shellbags" => T("ShellBags Explorer — история папок", "ShellBags Explorer — folder history"),
                "cachedprogramslist" or "cacheprogramlist" => T("CachedProgramsList — следы установок", "CachedProgramsList — install traces"),
                "executedprogramslist" => T("ExecutedProgramsList — следы запусков", "ExecutedProgramsList — execution traces"),
                "journaltrace" => T("JournalTrace — следы активности", "JournalTrace — activity traces"),
                "lastactivityview" => T("LastActivityView — сводка активности", "LastActivityView — activity summary"),
                "usbdeview" => T("USBDeview — история USB устройств", "USBDeview — USB devices history"),
                "usbdrivelog" => T("USBDriveLog — логи USB накопителей", "USBDriveLog — USB drive logs"),
                _ => name // если неизвестно — показываем как есть
            };
        }

        private string GetToolDescriptionTranslated(string name)
        {
            var key = NormalizeToolKey(name);

            return key switch
            {
                "everything" => T("Мгновенный поиск по имени файлов на диске.", "Instant file-name search across disks."),
                "shellbagsexplorer" or "shellbag" or "shellbags" => T("Показывает, какие папки открывались в проводнике.", "Shows which folders were opened in Explorer."),
                "cachedprogramslist" or "cacheprogramlist" => T("Находит следы установленного/запускаемого софта по артефактам.", "Finds traces of installed/executed software from artifacts."),
                "executedprogramslist" => T("Список запусков программ по системным следам.", "Lists executed programs based on system evidence."),
                "journaltrace" => T("Следы активности по журналам и артефактам Windows.", "Activity traces from Windows journals/artifacts."),
                "lastactivityview" => T("Сводка последней активности пользователя и системы.", "Summary of recent user and system activity."),
                "usbdeview" => T("История подключений USB: флешки, телефоны, устройства.", "USB connection history: drives, phones, devices."),
                "usbdrivelog" => T("Подробные логи подключений USB-накопителей (время/буква).", "Detailed USB drive connection logs (time/letter)."),
                _ => T("Утилита для модерации / анализа активности.", "Moderation / activity analysis utility.")
            };
        }



        // =========================================================
        // TOOLS action cards (bottom)
        // =========================================================
        private void InitToolsActionCardsUi()
        {
            // панель снизу (у тебя она есть)
            panelToolsBottom.Controls.Clear();
            panelToolsBottom.Padding = new Padding(12);
            panelToolsBottom.BackColor = Color.FromArgb(12, 12, 18);

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                WrapContents = false,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            cardOpenTool = new ToolActionCard { Width = 210, Height = 44, Margin = new Padding(0, 0, 12, 0) };
            cardLocateTool = new ToolActionCard { Width = 210, Height = 44, Margin = new Padding(0, 0, 12, 0) };
            cardDownloadTool = new ToolActionCard { Width = 210, Height = 44, Margin = new Padding(0, 0, 12, 0) };

            flow.Controls.Add(cardOpenTool);
            flow.Controls.Add(cardLocateTool);
            flow.Controls.Add(cardDownloadTool);

            lblToolsSubHint = new Label
            {
                AutoSize = true,
                ForeColor = Color.Gainsboro,
                Location = new Point(flow.Right + 16, 22),
                Text = T("Выбрано: -", "Selected: -")
            };

            panelToolsBottom.Controls.Add(flow);
            panelToolsBottom.Controls.Add(lblToolsSubHint);

            // иконки (можешь заменить на свои)
            cardOpenTool.Icon = Properties.Resources.icon_scan;
            cardLocateTool.Icon = Properties.Resources.icon_path;
            cardDownloadTool.Icon = Properties.Resources.icon_download;

            cardOpenTool.Clicked += (_, __) => OpenSelectedTool();
            cardLocateTool.Clicked += (_, __) => LocateSelectedTool();
            cardDownloadTool.Clicked += (_, __) => DownloadSelectedTool();

            UpdateToolsActionsState();
        }

        private void UpdateToolsActionsState()
        {
            var t = GetSelectedTool();

            cardOpenTool.Selected = false;
            cardLocateTool.Selected = false;
            cardDownloadTool.Selected = false;

            // тексты (под язык)
            cardOpenTool.Title = T("Открыть", "Open");
            cardOpenTool.Description = T("Запустить утилиту", "Run selected tool");

            cardLocateTool.Title = T("Показать", "Locate");
            cardLocateTool.Description = T("Открыть папку и выделить", "Open folder and select");

            cardDownloadTool.Title = T("Скачать", "Download");
            cardDownloadTool.Description = T("Открыть сайт утилиты", "Open tool website");

            if (t == null)
            {
                cardOpenTool.Enabled = false;
                cardLocateTool.Enabled = false;
                cardDownloadTool.Enabled = false;
                lblToolsSubHint.Text = T("Выбрано: -", "Selected: -");
                return;
            }

            bool found = t.Status == "Found" && !string.IsNullOrWhiteSpace(t.Path) && File.Exists(t.Path);
            bool hasUrl = !string.IsNullOrWhiteSpace(t.DownloadUrl);

            cardOpenTool.Enabled = found;
            cardLocateTool.Enabled = found;
            cardDownloadTool.Enabled = hasUrl;

            lblToolsSubHint.Text = T(
                $"Выбрано: {t.Name} | {t.Status}",
                $"Selected: {t.Name} | {t.Status}"
            );
        }

        private void OpenSelectedTool()
        {
            // защита от двойного запуска
            if (System.Threading.Interlocked.Exchange(ref _toolLaunchGate, 1) == 1)
                return;

            try
            {
                var t = GetSelectedTool();
                if (t == null) return;

                if (t.Status != "Found" || string.IsNullOrWhiteSpace(t.Path) || !File.Exists(t.Path))
                {
                    MessageBox.Show(T("Утилита не найдена. Используй Download или Locate.",
                                      "Tool not found. Use Download or Locate."), "ScumChecker");
                    return;
                }

                Process.Start(new ProcessStartInfo { FileName = t.Path, UseShellExecute = true });
            }
            finally
            {
                // через небольшой таймаут отпускаем (иначе быстрые клики блокнутся навсегда)
                _ = Task.Delay(350).ContinueWith(_ =>
                    System.Threading.Interlocked.Exchange(ref _toolLaunchGate, 0));
            }
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

            if (page == pageQuick)
            {
                UiPost(UpdateQuickSectionWidths);

            }

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

        private void WireQuickActions()
        {
            // ===== System (добавляем недостающие)
            btnOpenRegedit.Click += (_, __) => OpenUrl("regedit.exe");

            btnOpenTemp.Click += (_, __) =>
                OpenPath(Path.GetTempPath());

            btnOpenDownloads.Click += (_, __) =>
                OpenUrl("shell:downloads");

            btnOpenWindowsUpdate.Click += (_, __) =>
                OpenUrl("ms-settings:windowsupdate");

            btnOpenAppData.Click += (_, __) =>
                OpenPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

            btnOpenLocalAppData.Click += (_, __) =>
                OpenPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

            btnOpenPrefetch.Click += (_, __) =>
                OpenPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch"));

            btnOpenEventViewer.Click += (_, __) => OpenUrl("eventvwr.msc");

            // ===== Browsers
            btnOpenChromeProfile.Click += (_, __) =>
                OpenPath(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Google", "Chrome", "User Data", "Default"));

            btnOpenEdgeProfile.Click += (_, __) =>
                OpenPath(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Microsoft", "Edge", "User Data", "Default"));

            btnOpenFirefoxProfiles.Click += (_, __) =>
                OpenPath(Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Mozilla", "Firefox", "Profiles"));

            btnOpenBrowserCache.Click += (_, __) =>
            {
                var chromeCache = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Google", "Chrome", "User Data", "Default", "Cache");

                var edgeCache = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Microsoft", "Edge", "User Data", "Default", "Cache");

                if (Directory.Exists(chromeCache)) OpenPath(chromeCache);
                else OpenPath(edgeCache);
            };

            // ===== Cheats markets (links)
            btnOpenYougame.Click += (_, __) => OpenUrl("https://yougame.biz/");
            btnOpenDragonHack.Click += (_, __) => OpenUrl("https://dragon-hack.pro/");
            btnOpenUpGame.Click += (_, __) => OpenUrl("https://up-game.pro/");
            btnOpenIndustries.Click += (_, __) => OpenUrl("https://industries-cheat.store/");
            btnOpenCheatrise.Click += (_, __) => OpenUrl("https://cheatrise.com/");
            btnOpenCyberhack.Click += (_, __) => OpenUrl("https://cyberhack.pro/ru");
            btnOpenSoftix.Click += (_, __) => OpenUrl("https://softixcheats.com/");

            // ===== SCUM folder
            btnOpenScumFolder.Click += (_, __) =>
            {
                // 1) Быстрые кандидаты (частые пути)
                var candidates = new List<string>
    {
        @"C:\SteamLibrary\steamapps\common\SCUM",
        @"D:\SteamLibrary\steamapps\common\SCUM",
        @"E:\SteamLibrary\steamapps\common\SCUM",
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common", "SCUM"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Steam", "steamapps", "common", "SCUM"),
    };

                // 2) Подбираем по всем доступным дискам (C, D, E...)
                try
                {
                    foreach (var d in DriveInfo.GetDrives().Where(x => x.IsReady))
                    {
                        // SteamLibrary
                        candidates.Add(Path.Combine(d.RootDirectory.FullName, "SteamLibrary", "steamapps", "common", "SCUM"));
                        // Steam
                        candidates.Add(Path.Combine(d.RootDirectory.FullName, "Steam", "steamapps", "common", "SCUM"));
                        // Часто юзают "Games"
                        candidates.Add(Path.Combine(d.RootDirectory.FullName, "Games", "SteamLibrary", "steamapps", "common", "SCUM"));
                    }
                }
                catch { }

                // убираем дубли
                candidates = candidates
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // 3) Проверяем: есть ли SCUM.exe или папка
                string? found = candidates.FirstOrDefault(p =>
                    Directory.Exists(p) &&
                    (File.Exists(Path.Combine(p, "SCUM.exe")) ||
                     File.Exists(Path.Combine(p, "SCUM", "Binaries", "Win64", "SCUM.exe")) ||   // иногда бывает вложенность
                     true)); // папка уже норм

                if (!string.IsNullOrWhiteSpace(found))
                {
                    OpenPath(found);
                    return;
                }

                // 4) Если не нашли — попробуем открыть "common" (полезно, чтобы чел сам нашёл)
                // Открываем первый существующий steamapps\common, который найдём
                string? common = null;
                foreach (var p in candidates)
                {
                    var commonPath = Path.GetFullPath(Path.Combine(p, "..")); // ...\common
                    if (Directory.Exists(commonPath))
                    {
                        common = commonPath;
                        break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(common))
                {
                    OpenPath(common);
                    return;
                }

                // 5) Фоллбек: предложить выбрать папку вручную
                using var fbd = new FolderBrowserDialog
                {
                    Description = T("Не нашёл папку SCUM автоматически. Выбери папку игры вручную:",
                                    "Couldn't find SCUM automatically. Please select the game folder:"),
                    UseDescriptionForTitle = true,
                    ShowNewFolderButton = false
                };

                if (fbd.ShowDialog(this) == DialogResult.OK && Directory.Exists(fbd.SelectedPath))
                    OpenPath(fbd.SelectedPath);
            };


            // ===== Files
            btnOpenRecent.Click += (_, __) => OpenUrl("shell:recent");

            btnOpenDesktop.Click += (_, __) =>
                OpenPath(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));

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

        private void BuildQuickSections()
        {
            flowQuick.SuspendLayout();
            try
            {
                flowQuick.Controls.Clear();

                flowQuick.WrapContents = false;                 // 🔥 важно: иначе он делает колонки
                flowQuick.FlowDirection = FlowDirection.TopDown; // секции сверху вниз
                flowQuick.Padding = new Padding(14);
                flowQuick.AutoScroll = true;

                //ЕСЛИ Они ПОВТОРЯЮТСЯ НЕ БЕЙТЕ это хелперы типоооо
                flowQuick.AutoScroll = true;
                flowQuick.WrapContents = false;
                flowQuick.FlowDirection = FlowDirection.TopDown;


                flowQuick.Controls.Add(MakeQuickSection(T("Система", "System"),
                    btnOpenRegedit, btnOpenTemp, btnOpenDownloads, btnOpenWindowsUpdate,
                    btnOpenAppData, btnOpenLocalAppData, btnOpenPrefetch, btnOpenEventViewer));

                flowQuick.Controls.Add(MakeQuickSection(T("Браузеры", "Browsers"),
                    btnOpenChromeProfile, btnOpenEdgeProfile, btnOpenFirefoxProfiles, btnOpenBrowserCache));

                flowQuick.Controls.Add(MakeQuickSection(T("Файлы", "Files"),
                    btnOpenRecent, btnOpenDesktop, btnOpenSteamConfig));

                flowQuick.Controls.Add(MakeQuickSection(
                    T("Проверка покупок читов", "Cheat purchases check"),
                    btnOpenYougame,
                    btnOpenDragonHack,
                    btnOpenUpGame,
                    btnOpenIndustries,
                    btnOpenCheatrise,
                    btnOpenCyberhack,
                    btnOpenSoftix,
                    btnOpenScumFolder
                ));

            }
            finally
            {
                flowQuick.ResumeLayout(true);
                flowQuick.PerformLayout();
            }

            UpdateQuickSectionWidths(); // 👇 добавим ниже
        }



        private Control MakeQuickSection(string title, params Button[] buttons)
        {
            var section = new Panel
            {
                AutoSize = false,
                BackColor = Color.FromArgb(12, 12, 18),
                Padding = new Padding(14),
                Margin = new Padding(0, 0, 0, 14)
            };

            var lbl = new Label
            {
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Text = title,
                Location = new Point(14, 12)
            };

            var inner = new FlowLayoutPanel
            {
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = false,
                AutoScroll = false,
                BackColor = Color.Transparent,
                Location = new Point(14, lbl.Bottom + 10),
                Padding = new Padding(0),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };


            foreach (var b in buttons)
                inner.Controls.Add(b);

            section.Controls.Add(lbl);
            section.Controls.Add(inner);

            // сохраняем ссылки для ресайза/пересчёта
            section.Tag = (lbl, inner);

            return section;
        }



        private void FitButtonsToColumns(FlowLayoutPanel inner, int cols)
        {
            if (inner == null || cols < 1) return;

            int available = inner.ClientSize.Width;
            if (available <= 0) return;

            int gap = 12;
            int w = (available - gap * (cols - 1)) / cols;

            // допускаем более узкие кнопки
            w = Math.Max(120, w);

            foreach (Control c in inner.Controls)
            {
                if (c is Button b)
                {
                    b.Width = w;
                    b.Height = 42;
                    b.Margin = new Padding(0, 0, gap, gap);

                    // чтобы длинный текст не ломал лейаут
                    b.AutoEllipsis = true;
                }
            }
        }


        private void RecalcQuickSectionHeight(Panel section)
        {
            if (section.Tag is not ValueTuple<Label, FlowLayoutPanel> tag) return;
            var (lbl, inner) = tag;

            // нижняя точка inner + padding снизу
            section.Height = inner.Bottom + section.Padding.Bottom;
        }



        private void UpdateQuickSectionWidths()
        {
            if (flowQuick == null) return;

            int sectionW = flowQuick.ClientSize.Width
                           - flowQuick.Padding.Horizontal
                           - SystemInformation.VerticalScrollBarWidth
                           - 6;

            sectionW = Math.Max(300, sectionW);

            foreach (Control c in flowQuick.Controls)
            {
                if (c is not Panel section) continue;
                if (section.Tag is not ValueTuple<Label, FlowLayoutPanel> tag) continue;

                var (lbl, inner) = tag;

                section.Width = sectionW;

                int innerW = section.ClientSize.Width - section.Padding.Horizontal;
                inner.Width = Math.Max(200, innerW);

                // ⚙️ тут можно разные колонки для разных секций
                int minBtnW = 130; // или 120, если хочешь 4-5 чаще
                int gap = 12;

                // хотим максимум колонок (по секциям можно разный)
                int maxCols = 4;
                if (lbl.Text.Contains("Проверка покупок") || lbl.Text.Contains("purchases"))
                    maxCols = 5;

                // сколько реально колонок помещается
                int possible = (inner.Width + gap) / (minBtnW + gap);
                int cols = Math.Clamp(possible, 2, maxCols);

                // 💡 если “почти влезает” — всё равно пробуем 4/5 (будет уже, но красиво)
                if (maxCols >= 4 && inner.Width >= (4 * 130 + 3 * gap)) cols = Math.Max(cols, 4);
                if (maxCols >= 5 && inner.Width >= (5 * 120 + 4 * gap)) cols = Math.Max(cols, 5);



                FitButtonsToColumns(inner, cols);

                // важное: после изменения Width у inner нужно форснуть layout, чтобы высота стала правильной
                // 1) сначала форсим ширину
                inner.Width = Math.Max(200, innerW);

                // 2) раскладка + расчёт preferred по текущей ширине
                inner.SuspendLayout();
                FitButtonsToColumns(inner, cols);
                inner.ResumeLayout(true);
                inner.PerformLayout();

                // 🔥 ключ: GetPreferredSize учитывает текущую ширину, PreferredSize иногда врёт
                var pref = inner.GetPreferredSize(new Size(inner.Width, 0));
                inner.Height = pref.Height;

                // 3) обновляем высоту секции
                RecalcQuickSectionHeight(section);

            }

            flowQuick.PerformLayout();
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

            lblToolsTitle.Text = T("Утилиты", "Tools for moderation");
            lblToolsDesc.Text = T("Утилиты для администраторов и модераторов", "Utilities for administrators and moderators");
            lblToolsHint.Text = T("Выбери утилиту → Open / Locate / Download", "Select tool → Open / Locate / Download");

            lblQuickTitle.Text = T("Быстрый доступ", "Quick access");
            lblQuickDesc.Text = T(
                "Быстрый доступ к реестру, временным файлам, загрузкам, обновлениям и другим системным разделам.",
                "Quick access to the registry, temporary files, downloads, updates, and other system locations."
            );

            lblStatus.Text = T("Статус: Ожидание", "Status: Idle");

            UpdateSummary();
            InitQuickTiles();
            BuildQuickSections();         // 🔥 обязательно                         // WireQuickActions();        // можно НЕ вызывать повторно, иначе нащёлкаешь хендлеры
            UiPost(UpdateQuickSectionWidths);



            // обновляем Tools badges под язык
            if (_flowToolsTiles != null)
            {
                foreach (Control c in _flowToolsTiles.Controls)
                {
                    if (c is ToolTileCard tc && tc.Payload is ToolEntry te)
                    {
                        tc.Title = GetToolTitle(te.Name);
                        tc.Description = GetToolDescriptionTranslated(te.Name);
                        ApplyToolStatusBadge(tc, te);
                    }
                }
            }


            // обновим Steam карточки под язык
            if (_flowSteam != null) _ = RefreshSteamCardsAsync();
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

                try { await RefreshSteamCardsAsync(); } catch { }

                _cts?.Dispose();
                _cts = null;
            }
        }

        private int _lastProgress = 0;

        private int _toolLaunchGate = 0;


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
            var hay = $"{item.Title} {item.What} {item.Category} {item.Reason} {item.Details} {item.EvidencePath} {item.Url}";

            bool canEscalate =
                item.Category.Equals("Filesystem", StringComparison.OrdinalIgnoreCase) ||
                item.Category.Equals("Processes", StringComparison.OrdinalIgnoreCase);

            if (!canEscalate) return;

            if (ScumChecker.Core.Modules.SuspicionKeywords.ContainsCritical(hay))
            {
                item.Severity = Severity.High;

                if (string.IsNullOrWhiteSpace(item.Reason))
                    item.Reason = "Critical keyword match";
                else if (!item.Reason.Contains("Critical", StringComparison.OrdinalIgnoreCase))
                    item.Reason = "Critical keyword match | " + item.Reason;

                if (string.IsNullOrWhiteSpace(item.Recommendation))
                    item.Recommendation = "Manual review. Consider as high risk.";
            }
        }

        private void UiPost(Action a)
        {
            if (IsDisposed) return;

            // если хэндл уже есть — можно постить в UI очередь
            if (IsHandleCreated)
            {
                BeginInvoke(a);
                return;
            }

            // иначе дождёмся создания хэндла (один раз)
            EventHandler? h = null;
            h = (_, __) =>
            {
                HandleCreated -= h!;
                if (!IsDisposed) BeginInvoke(a);
            };
            HandleCreated += h;
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

        private bool PassSeverityFilter(Severity s) => s switch
        {
            Severity.Info => chkInfo.Checked,
            Severity.Low => chkLow.Checked,
            Severity.Medium => chkMedium.Checked,
            Severity.High => chkHigh.Checked,
            _ => true
        };

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
                lblVerdict.Text = T("Найдены индикаторы высокого риска → нужна ручная проверка", "High risk indicators found → manual review recommended");
            else if (me > 0)
                lblVerdict.Text = T("Есть подозрительные индикаторы → ручная проверка (без мгновенного бана)", "Suspicious indicators found → manual review (no instant ban)");
            else if (_allItems.Count > 0)
                lblVerdict.Text = T("Высокорисковых индикаторов не найдено", "No high-risk indicators found");
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
        // STEAM CARDS UI
        // =========================================================
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

            card.SetHeader(T("Загрузка…", "Loading…"), null);
            card.ClearRows();
            card.AddRow(T("Получаю данные Steam…", "Fetching Steam data…"), null);

            return card;
        }

        private async Task RefreshSteamCardsAsync()
        {
            if (_flowSteam == null) return;

            SafeUi(() => _flowSteam.SuspendLayout());
            try
            {
                SafeUi(() => _flowSteam.Controls.Clear());

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
                    if (parts.Length > 0) account = parts[0];

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

                var current = accounts.FirstOrDefault(a => a.mostRecent.Equals("true", StringComparison.OrdinalIgnoreCase));
                if (string.IsNullOrWhiteSpace(current.steamId)) current = accounts[0];

                var ordered = new List<(string steamId, string account, string timestamp)>();
                ordered.Add((current.steamId, current.account, current.timestamp));
                ordered.AddRange(accounts.Where(x => x.steamId != current.steamId).Select(x => (x.steamId, x.account, x.timestamp)));

                var cards = new List<(SteamAccountCard card, string id, string acc, string ts)>();
                foreach (var a in ordered)
                {
                    var card = CreateSteamCard();
                    cards.Add((card, a.steamId, a.account, a.timestamp));
                    SafeUi(() => _flowSteam.Controls.Add(card));
                }

                SafeUi(() =>
                {
                    _flowSteam.ResumeLayout(true);
                    _flowSteam.PerformLayout();
                    _flowSteam.SuspendLayout();
                });

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

        private static Bitmap CropTransparent(Image src)
        {
            var bmp = new Bitmap(src);
            int minX = bmp.Width, minY = bmp.Height, maxX = 0, maxY = 0;
            bool any = false;

            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                {
                    var c = bmp.GetPixel(x, y);
                    if (c.A <= 10) continue; // почти прозрачное игнорим
                    any = true;
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }

            if (!any) return bmp;

            var rect = Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
            var cropped = new Bitmap(rect.Width, rect.Height);
            using (var g = Graphics.FromImage(cropped))
            {
                g.DrawImage(bmp, new Rectangle(0, 0, rect.Width, rect.Height), rect, GraphicsUnit.Pixel);
            }
            bmp.Dispose();
            return cropped;
        }

        private void ApplyQuickIcon(Button b, Image icon, int iconSize = 18)
        {
            if (b == null || icon == null) return;

            using var cropped = CropTransparent(icon);
            b.Image = ResizeIcon(cropped, iconSize);

            b.ImageAlign = ContentAlignment.MiddleLeft;
            b.TextAlign = ContentAlignment.MiddleLeft;
            b.TextImageRelation = TextImageRelation.ImageBeforeText;

            // расстояние “иконка → текст”
            b.Padding = new Padding(14, 0, 10, 0);
        }


        private async Task FillOneCardAsync(SteamAccountCard card, string steamId64, string accountName, string timestamp)
        {
            SteamProfileScraper.SteamProfileLite? prof = null;
            try { prof = await SteamProfileScraper.GetProfileLiteAsync(steamId64); } catch { }

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
                catch { }
            }

            SteamApi.BanLite bans;
            try { bans = await SteamApi.GetBansNoKeyAsync(steamId64); }
            catch { bans = SteamApi.BanLite.UnknownResult(); }

            SafeUi(() =>
            {
                var name = prof?.PersonaName ?? steamId64;

                card.SuspendLayout();

                card.SetHeader(name, avatar);
                card.ClearRows();

                bool bannedAny = (!bans.Unknown) && (bans.VacBanned || bans.GameBans > 0);

                card.AddRow($"{T("Блокировка VAC", "Banned")}: {FormatYesNoUnknown(bans.Unknown, bannedAny)}", null);
                card.AddRow($"{T("VAC банов", "VAC bans")}: {(bans.Unknown ? "-" : bans.VacBans.ToString())}", null);
                card.AddRow($"{T("Game bans", "Game bans")}: {(bans.Unknown ? "-" : bans.GameBans.ToString())}", null);
                card.AddRow($"{T("Дней с последнего", "Days since last ban")}: {(bans.DaysSinceLastBan.HasValue ? bans.DaysSinceLastBan.Value.ToString() : "-")}", null);
                card.AddRow($"{T("Аккаунт", "Account")}: {accountName}", null);
                card.AddRow($"{T("Последний вход", "Last logon")}: {timestamp}", null);

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
        // SIDEBAR glow
        // =========================================================
        private void UpgradeSidebarButtonsToGlow()
        {
            btnNavNative = ReplaceWithGlowByName("btnNavNative", () => Properties.Resources.icon_native) ?? btnNavNative;
            btnNavSteam = ReplaceWithGlowByName("btnNavSteam", () => Properties.Resources.icon_steam) ?? btnNavSteam;
            btnNavTools = ReplaceWithGlowByName("btnNavTools", () => Properties.Resources.icon_tools) ?? btnNavTools;
            btnNavQuick = ReplaceWithGlowByName("btnNavQuick", () => Properties.Resources.icon_quick) ?? btnNavQuick;
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

            try { gb.Image = iconGetter(); } catch { }

            parent.Controls.Remove(oldBtn);
            parent.Controls.Add(gb);
            parent.Controls.SetChildIndex(gb, idx);

            return gb;
        }

        private void TryApply(string name, Func<Image> getter, Button b, int size = 18)
        {
            try { ApplyButtonIcon(b, getter(), size); } catch { }
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

        private void SafeUi(Action a)
        {
            if (IsDisposed) return;
            if (InvokeRequired) BeginInvoke(a);
            else a();
        }

        private void Git_button_Click(object sender, EventArgs e)
        {
            OpenUrl("https://e-z.bio/nezeryxs");
        }

        private void ezka_button_Click(object sender, EventArgs e)
        {
            OpenUrl("https://github.com/Nezeryxs");
        }

        private void panelFooter_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
