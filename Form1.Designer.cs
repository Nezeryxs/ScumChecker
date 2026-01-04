namespace ScumChecker
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        // Tabs
        private TabControl tabControl1;
        private TabPage tabNative;
        private TabPage tabSteam;
        private TabPage tabTools;

        // Bottom footer
        private Panel panelFooter;
        private LinkLabel linkGitHub;
        private LinkLabel linkBio;
        private Label lblMadeWith;

        // Summary (Native)
        private Panel panelSummary;
        private Label lblVerdictTitle;
        private Label lblVerdict;
        private Label lblCountHigh;
        private Label lblCountMedium;
        private Label lblCountLow;
        private Label lblCountInfo;

        // Top panel (Native)
        private Panel panelTop;
        private Button btnScan;
        private Button btnCancel;
        private Button btnCopyReport;
        private CheckBox chkHigh;
        private CheckBox chkMedium;
        private CheckBox chkLow;
        private CheckBox chkInfo;
        private Label lblStatus;
        private ProgressBar progressScan;

        // Native main area
        private SplitContainer splitNative;
        private DataGridView dgvFindings;
        private TextBox txtLog;

        // Findings columns
        private DataGridViewTextBoxColumn colSeverity;
        private DataGridViewTextBoxColumn colCategory;
        private DataGridViewTextBoxColumn colWhat;
        private DataGridViewTextBoxColumn colReason;
        private DataGridViewTextBoxColumn colAction;
        private DataGridViewTextBoxColumn colDetails;

        // Steam tab
        private Panel panelSteamTop;
        private Label lblSteamHint;
        private DataGridView dgvSteam;
        private DataGridViewTextBoxColumn colSteamId;
        private DataGridViewTextBoxColumn colPersona;
        private DataGridViewTextBoxColumn colAccountName;
        private DataGridViewTextBoxColumn colMostRecent;
        private DataGridViewTextBoxColumn colTimestamp;

        // Tools tab
        private Panel panelToolsTop;
        private Label lblToolsTitle;
        private Label lblToolsDesc;

        private DataGridView dgvTools;
        private Panel panelToolsBottom;
        private Button btnOpenTool;
        private Button btnLocateTool;
        private Button btnDownloadTool;
        private Label lblToolsHint;

        private DataGridViewTextBoxColumn colTool;
        private DataGridViewTextBoxColumn colToolStatus;
        private DataGridViewTextBoxColumn colToolPath;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            tabControl1 = new TabControl();
            tabNative = new TabPage();
            splitNative = new SplitContainer();
            dgvFindings = new DataGridView();
            colSeverity = new DataGridViewTextBoxColumn();
            colCategory = new DataGridViewTextBoxColumn();
            colWhat = new DataGridViewTextBoxColumn();
            colReason = new DataGridViewTextBoxColumn();
            colAction = new DataGridViewTextBoxColumn();
            colDetails = new DataGridViewTextBoxColumn();
            txtLog = new TextBox();
            panelTop = new Panel();
            btnScan = new Button();
            btnCancel = new Button();
            btnCopyReport = new Button();
            chkHigh = new CheckBox();
            chkMedium = new CheckBox();
            chkLow = new CheckBox();
            chkInfo = new CheckBox();
            lblStatus = new Label();
            progressScan = new ProgressBar();
            panelSummary = new Panel();
            lblVerdictTitle = new Label();
            lblVerdict = new Label();
            lblCountHigh = new Label();
            lblCountMedium = new Label();
            lblCountLow = new Label();
            lblCountInfo = new Label();
            tabSteam = new TabPage();
            dgvSteam = new DataGridView();
            colSteamId = new DataGridViewTextBoxColumn();
            colPersona = new DataGridViewTextBoxColumn();
            colAccountName = new DataGridViewTextBoxColumn();
            colMostRecent = new DataGridViewTextBoxColumn();
            colTimestamp = new DataGridViewTextBoxColumn();
            panelSteamTop = new Panel();
            lblSteamHint = new Label();
            tabTools = new TabPage();
            dgvTools = new DataGridView();
            colTool = new DataGridViewTextBoxColumn();
            colToolStatus = new DataGridViewTextBoxColumn();
            colToolPath = new DataGridViewTextBoxColumn();
            panelToolsBottom = new Panel();
            btnOpenTool = new Button();
            btnLocateTool = new Button();
            btnDownloadTool = new Button();
            lblToolsHint = new Label();
            panelToolsTop = new Panel();
            lblToolsTitle = new Label();
            lblToolsDesc = new Label();
            panelFooter = new Panel();
            linkGitHub = new LinkLabel();
            linkBio = new LinkLabel();
            lblMadeWith = new Label();
            tabControl1.SuspendLayout();
            tabNative.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitNative).BeginInit();
            splitNative.Panel1.SuspendLayout();
            splitNative.Panel2.SuspendLayout();
            splitNative.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvFindings).BeginInit();
            panelTop.SuspendLayout();
            panelSummary.SuspendLayout();
            tabSteam.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSteam).BeginInit();
            panelSteamTop.SuspendLayout();
            tabTools.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTools).BeginInit();
            panelToolsBottom.SuspendLayout();
            panelToolsTop.SuspendLayout();
            panelFooter.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabNative);
            tabControl1.Controls.Add(tabSteam);
            tabControl1.Controls.Add(tabTools);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1100, 686);
            tabControl1.TabIndex = 0;
            // 
            // tabNative
            // 
            tabNative.BackgroundImage = Properties.Resources.load;
            tabNative.BackgroundImageLayout = ImageLayout.Stretch;
            tabNative.Controls.Add(splitNative);
            tabNative.Controls.Add(panelTop);
            tabNative.Controls.Add(panelSummary);
            tabNative.Location = new Point(4, 24);
            tabNative.Name = "tabNative";
            tabNative.Padding = new Padding(10);
            tabNative.Size = new Size(1092, 658);
            tabNative.TabIndex = 0;
            tabNative.Text = "Нативная проверка";
            tabNative.UseVisualStyleBackColor = true;
            // 
            // splitNative
            // 
            splitNative.BackColor = Color.FromArgb(18, 18, 24);
            splitNative.Dock = DockStyle.Fill;
            splitNative.Location = new Point(10, 152);
            splitNative.Name = "splitNative";
            splitNative.Orientation = Orientation.Horizontal;
            // 
            // splitNative.Panel1
            // 
            splitNative.Panel1.Controls.Add(dgvFindings);
            // 
            // splitNative.Panel2
            // 
            splitNative.Panel2.Controls.Add(txtLog);
            splitNative.Size = new Size(1072, 496);
            splitNative.SplitterDistance = 352;
            splitNative.TabIndex = 0;
            // 
            // dgvFindings
            // 
            dgvFindings.AllowUserToAddRows = false;
            dgvFindings.AllowUserToDeleteRows = false;
            dgvFindings.AllowUserToResizeRows = false;
            dgvFindings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvFindings.BackgroundColor = Color.FromArgb(12, 12, 18);
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(22, 22, 30);
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = Color.Gainsboro;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgvFindings.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvFindings.Columns.AddRange(new DataGridViewColumn[] { colSeverity, colCategory, colWhat, colReason, colAction, colDetails });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(12, 12, 18);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = Color.Gainsboro;
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(30, 30, 46);
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dgvFindings.DefaultCellStyle = dataGridViewCellStyle2;
            dgvFindings.Dock = DockStyle.Fill;
            dgvFindings.EnableHeadersVisualStyles = false;
            dgvFindings.GridColor = Color.FromArgb(32, 32, 40);
            dgvFindings.Location = new Point(0, 0);
            dgvFindings.MultiSelect = false;
            dgvFindings.Name = "dgvFindings";
            dgvFindings.ReadOnly = true;
            dgvFindings.RowHeadersVisible = false;
            dgvFindings.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFindings.Size = new Size(1072, 352);
            dgvFindings.TabIndex = 0;
            // 
            // colSeverity
            // 
            colSeverity.FillWeight = 10F;
            colSeverity.HeaderText = "Severity";
            colSeverity.Name = "colSeverity";
            colSeverity.ReadOnly = true;
            // 
            // colCategory
            // 
            colCategory.FillWeight = 12F;
            colCategory.HeaderText = "Category";
            colCategory.Name = "colCategory";
            colCategory.ReadOnly = true;
            // 
            // colWhat
            // 
            colWhat.FillWeight = 16F;
            colWhat.HeaderText = "What";
            colWhat.Name = "colWhat";
            colWhat.ReadOnly = true;
            // 
            // colReason
            // 
            colReason.FillWeight = 22F;
            colReason.HeaderText = "Why flagged";
            colReason.Name = "colReason";
            colReason.ReadOnly = true;
            // 
            // colAction
            // 
            colAction.FillWeight = 16F;
            colAction.HeaderText = "Recommended action";
            colAction.Name = "colAction";
            colAction.ReadOnly = true;
            // 
            // colDetails
            // 
            colDetails.FillWeight = 24F;
            colDetails.HeaderText = "Details";
            colDetails.Name = "colDetails";
            colDetails.ReadOnly = true;
            // 
            // txtLog
            // 
            txtLog.BackColor = Color.FromArgb(10, 10, 14);
            txtLog.BorderStyle = BorderStyle.FixedSingle;
            txtLog.Dock = DockStyle.Fill;
            txtLog.ForeColor = Color.Gainsboro;
            txtLog.Location = new Point(0, 0);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(1072, 140);
            txtLog.TabIndex = 0;
            // 
            // panelTop
            // 
            panelTop.BackColor = Color.FromArgb(18, 18, 24);
            panelTop.Controls.Add(btnScan);
            panelTop.Controls.Add(btnCancel);
            panelTop.Controls.Add(btnCopyReport);
            panelTop.Controls.Add(chkHigh);
            panelTop.Controls.Add(chkMedium);
            panelTop.Controls.Add(chkLow);
            panelTop.Controls.Add(chkInfo);
            panelTop.Controls.Add(lblStatus);
            panelTop.Controls.Add(progressScan);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(10, 68);
            panelTop.Name = "panelTop";
            panelTop.Padding = new Padding(12, 12, 12, 10);
            panelTop.Size = new Size(1072, 84);
            panelTop.TabIndex = 1;
            // 
            // btnScan
            // 
            btnScan.FlatStyle = FlatStyle.System;
            btnScan.Location = new Point(12, 12);
            btnScan.Name = "btnScan";
            btnScan.Size = new Size(120, 36);
            btnScan.TabIndex = 0;
            btnScan.Text = "Scan";
            // 
            // btnCancel
            // 
            btnCancel.Enabled = false;
            btnCancel.FlatStyle = FlatStyle.System;
            btnCancel.Location = new Point(140, 12);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(120, 36);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            // 
            // btnCopyReport
            // 
            btnCopyReport.FlatStyle = FlatStyle.System;
            btnCopyReport.Location = new Point(268, 12);
            btnCopyReport.Name = "btnCopyReport";
            btnCopyReport.Size = new Size(120, 36);
            btnCopyReport.TabIndex = 2;
            btnCopyReport.Text = "Copy report";
            // 
            // chkHigh
            // 
            chkHigh.AutoSize = true;
            chkHigh.Checked = true;
            chkHigh.CheckState = CheckState.Checked;
            chkHigh.ForeColor = Color.Gainsboro;
            chkHigh.Location = new Point(410, 14);
            chkHigh.Name = "chkHigh";
            chkHigh.Size = new Size(52, 19);
            chkHigh.TabIndex = 3;
            chkHigh.Text = "High";
            // 
            // chkMedium
            // 
            chkMedium.AutoSize = true;
            chkMedium.Checked = true;
            chkMedium.CheckState = CheckState.Checked;
            chkMedium.ForeColor = Color.Gainsboro;
            chkMedium.Location = new Point(470, 14);
            chkMedium.Name = "chkMedium";
            chkMedium.Size = new Size(71, 19);
            chkMedium.TabIndex = 4;
            chkMedium.Text = "Medium";
            // 
            // chkLow
            // 
            chkLow.AutoSize = true;
            chkLow.Checked = true;
            chkLow.CheckState = CheckState.Checked;
            chkLow.ForeColor = Color.Gainsboro;
            chkLow.Location = new Point(555, 14);
            chkLow.Name = "chkLow";
            chkLow.Size = new Size(48, 19);
            chkLow.TabIndex = 5;
            chkLow.Text = "Low";
            // 
            // chkInfo
            // 
            chkInfo.AutoSize = true;
            chkInfo.Checked = true;
            chkInfo.CheckState = CheckState.Checked;
            chkInfo.ForeColor = Color.Gainsboro;
            chkInfo.Location = new Point(615, 14);
            chkInfo.Name = "chkInfo";
            chkInfo.Size = new Size(47, 19);
            chkInfo.TabIndex = 6;
            chkInfo.Text = "Info";
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.ForeColor = Color.Gainsboro;
            lblStatus.Location = new Point(690, 18);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(64, 15);
            lblStatus.TabIndex = 7;
            lblStatus.Text = "Status: Idle";
            // 
            // progressScan
            // 
            progressScan.Dock = DockStyle.Bottom;
            progressScan.Location = new Point(12, 60);
            progressScan.Name = "progressScan";
            progressScan.Size = new Size(1048, 14);
            progressScan.TabIndex = 8;
            // 
            // panelSummary
            // 
            panelSummary.BackColor = Color.FromArgb(18, 18, 24);
            panelSummary.Controls.Add(lblVerdictTitle);
            panelSummary.Controls.Add(lblVerdict);
            panelSummary.Controls.Add(lblCountHigh);
            panelSummary.Controls.Add(lblCountMedium);
            panelSummary.Controls.Add(lblCountLow);
            panelSummary.Controls.Add(lblCountInfo);
            panelSummary.Dock = DockStyle.Top;
            panelSummary.Location = new Point(10, 10);
            panelSummary.Name = "panelSummary";
            panelSummary.Padding = new Padding(12, 10, 12, 10);
            panelSummary.Size = new Size(1072, 58);
            panelSummary.TabIndex = 2;
            // 
            // lblVerdictTitle
            // 
            lblVerdictTitle.AutoSize = true;
            lblVerdictTitle.ForeColor = Color.Gainsboro;
            lblVerdictTitle.Location = new Point(12, 19);
            lblVerdictTitle.Name = "lblVerdictTitle";
            lblVerdictTitle.Size = new Size(46, 15);
            lblVerdictTitle.TabIndex = 0;
            lblVerdictTitle.Text = "Verdict:";
            // 
            // lblVerdict
            // 
            lblVerdict.AutoSize = true;
            lblVerdict.ForeColor = Color.White;
            lblVerdict.Location = new Point(70, 19);
            lblVerdict.Name = "lblVerdict";
            lblVerdict.Size = new Size(106, 15);
            lblVerdict.TabIndex = 1;
            lblVerdict.Text = "No data (run Scan)";
            // 
            // lblCountHigh
            // 
            lblCountHigh.AutoSize = true;
            lblCountHigh.ForeColor = Color.Gainsboro;
            lblCountHigh.Location = new Point(610, 19);
            lblCountHigh.Name = "lblCountHigh";
            lblCountHigh.Size = new Size(45, 15);
            lblCountHigh.TabIndex = 2;
            lblCountHigh.Text = "High: 0";
            // 
            // lblCountMedium
            // 
            lblCountMedium.AutoSize = true;
            lblCountMedium.ForeColor = Color.Gainsboro;
            lblCountMedium.Location = new Point(690, 19);
            lblCountMedium.Name = "lblCountMedium";
            lblCountMedium.Size = new Size(64, 15);
            lblCountMedium.TabIndex = 3;
            lblCountMedium.Text = "Medium: 0";
            // 
            // lblCountLow
            // 
            lblCountLow.AutoSize = true;
            lblCountLow.ForeColor = Color.Gainsboro;
            lblCountLow.Location = new Point(800, 19);
            lblCountLow.Name = "lblCountLow";
            lblCountLow.Size = new Size(41, 15);
            lblCountLow.TabIndex = 4;
            lblCountLow.Text = "Low: 0";
            // 
            // lblCountInfo
            // 
            lblCountInfo.AutoSize = true;
            lblCountInfo.ForeColor = Color.Gainsboro;
            lblCountInfo.Location = new Point(880, 19);
            lblCountInfo.Name = "lblCountInfo";
            lblCountInfo.Size = new Size(40, 15);
            lblCountInfo.TabIndex = 5;
            lblCountInfo.Text = "Info: 0";
            // 
            // tabSteam
            // 
            tabSteam.BackgroundImage = Properties.Resources.load;
            tabSteam.BackgroundImageLayout = ImageLayout.Stretch;
            tabSteam.Controls.Add(dgvSteam);
            tabSteam.Controls.Add(panelSteamTop);
            tabSteam.Location = new Point(4, 24);
            tabSteam.Name = "tabSteam";
            tabSteam.Padding = new Padding(10);
            tabSteam.Size = new Size(192, 72);
            tabSteam.TabIndex = 1;
            tabSteam.Text = "Steam";
            tabSteam.UseVisualStyleBackColor = true;
            // 
            // dgvSteam
            // 
            dgvSteam.AllowUserToAddRows = false;
            dgvSteam.AllowUserToDeleteRows = false;
            dgvSteam.AllowUserToResizeRows = false;
            dgvSteam.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvSteam.BackgroundColor = Color.FromArgb(12, 12, 18);
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.FromArgb(22, 22, 30);
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.Gainsboro;
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            dgvSteam.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dgvSteam.Columns.AddRange(new DataGridViewColumn[] { colSteamId, colPersona, colAccountName, colMostRecent, colTimestamp });
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.FromArgb(12, 12, 18);
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle4.ForeColor = Color.Gainsboro;
            dataGridViewCellStyle4.SelectionBackColor = Color.FromArgb(30, 30, 46);
            dataGridViewCellStyle4.SelectionForeColor = Color.White;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.False;
            dgvSteam.DefaultCellStyle = dataGridViewCellStyle4;
            dgvSteam.Dock = DockStyle.Fill;
            dgvSteam.EnableHeadersVisualStyles = false;
            dgvSteam.GridColor = Color.FromArgb(32, 32, 40);
            dgvSteam.Location = new Point(10, 68);
            dgvSteam.MultiSelect = false;
            dgvSteam.Name = "dgvSteam";
            dgvSteam.ReadOnly = true;
            dgvSteam.RowHeadersVisible = false;
            dgvSteam.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSteam.Size = new Size(172, 0);
            dgvSteam.TabIndex = 0;
            // 
            // colSteamId
            // 
            colSteamId.FillWeight = 22F;
            colSteamId.HeaderText = "SteamID64";
            colSteamId.Name = "colSteamId";
            colSteamId.ReadOnly = true;
            // 
            // colPersona
            // 
            colPersona.FillWeight = 22F;
            colPersona.HeaderText = "PersonaName";
            colPersona.Name = "colPersona";
            colPersona.ReadOnly = true;
            // 
            // colAccountName
            // 
            colAccountName.FillWeight = 18F;
            colAccountName.HeaderText = "AccountName";
            colAccountName.Name = "colAccountName";
            colAccountName.ReadOnly = true;
            // 
            // colMostRecent
            // 
            colMostRecent.FillWeight = 10F;
            colMostRecent.HeaderText = "MostRecent";
            colMostRecent.Name = "colMostRecent";
            colMostRecent.ReadOnly = true;
            // 
            // colTimestamp
            // 
            colTimestamp.FillWeight = 28F;
            colTimestamp.HeaderText = "Timestamp";
            colTimestamp.Name = "colTimestamp";
            colTimestamp.ReadOnly = true;
            // 
            // panelSteamTop
            // 
            panelSteamTop.BackColor = Color.FromArgb(18, 18, 24);
            panelSteamTop.Controls.Add(lblSteamHint);
            panelSteamTop.Dock = DockStyle.Top;
            panelSteamTop.Location = new Point(10, 10);
            panelSteamTop.Name = "panelSteamTop";
            panelSteamTop.Padding = new Padding(12, 10, 12, 10);
            panelSteamTop.Size = new Size(172, 58);
            panelSteamTop.TabIndex = 1;
            // 
            // lblSteamHint
            // 
            lblSteamHint.AutoSize = true;
            lblSteamHint.ForeColor = Color.Gainsboro;
            lblSteamHint.Location = new Point(12, 19);
            lblSteamHint.Name = "lblSteamHint";
            lblSteamHint.Size = new Size(484, 15);
            lblSteamHint.TabIndex = 0;
            lblSteamHint.Text = "Steam accounts from loginusers.vdf (double-click a row to open steamcommunity profile)";
            // 
            // tabTools
            // 
            tabTools.BackgroundImage = Properties.Resources.load;
            tabTools.BackgroundImageLayout = ImageLayout.Stretch;
            tabTools.Controls.Add(dgvTools);
            tabTools.Controls.Add(panelToolsBottom);
            tabTools.Controls.Add(panelToolsTop);
            tabTools.Location = new Point(4, 24);
            tabTools.Name = "tabTools";
            tabTools.Padding = new Padding(10);
            tabTools.Size = new Size(192, 72);
            tabTools.TabIndex = 2;
            tabTools.Text = "Программы";
            tabTools.UseVisualStyleBackColor = true;
            // 
            // dgvTools
            // 
            dgvTools.AllowUserToAddRows = false;
            dgvTools.AllowUserToDeleteRows = false;
            dgvTools.AllowUserToResizeRows = false;
            dgvTools.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTools.BackgroundColor = Color.FromArgb(12, 12, 18);
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = Color.FromArgb(22, 22, 30);
            dataGridViewCellStyle5.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle5.ForeColor = Color.Gainsboro;
            dataGridViewCellStyle5.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = DataGridViewTriState.True;
            dgvTools.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle5;
            dgvTools.Columns.AddRange(new DataGridViewColumn[] { colTool, colToolStatus, colToolPath });
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = Color.FromArgb(12, 12, 18);
            dataGridViewCellStyle6.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle6.ForeColor = Color.Gainsboro;
            dataGridViewCellStyle6.SelectionBackColor = Color.FromArgb(30, 30, 46);
            dataGridViewCellStyle6.SelectionForeColor = Color.White;
            dataGridViewCellStyle6.WrapMode = DataGridViewTriState.False;
            dgvTools.DefaultCellStyle = dataGridViewCellStyle6;
            dgvTools.Dock = DockStyle.Fill;
            dgvTools.EnableHeadersVisualStyles = false;
            dgvTools.GridColor = Color.FromArgb(32, 32, 40);
            dgvTools.Location = new Point(10, 82);
            dgvTools.MultiSelect = false;
            dgvTools.Name = "dgvTools";
            dgvTools.ReadOnly = true;
            dgvTools.RowHeadersVisible = false;
            dgvTools.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTools.Size = new Size(172, 0);
            dgvTools.TabIndex = 0;
            // 
            // colTool
            // 
            colTool.FillWeight = 22F;
            colTool.HeaderText = "Tool";
            colTool.Name = "colTool";
            colTool.ReadOnly = true;
            // 
            // colToolStatus
            // 
            colToolStatus.FillWeight = 18F;
            colToolStatus.HeaderText = "Status";
            colToolStatus.Name = "colToolStatus";
            colToolStatus.ReadOnly = true;
            // 
            // colToolPath
            // 
            colToolPath.FillWeight = 60F;
            colToolPath.HeaderText = "Path";
            colToolPath.Name = "colToolPath";
            colToolPath.ReadOnly = true;
            // 
            // panelToolsBottom
            // 
            panelToolsBottom.BackColor = Color.FromArgb(18, 18, 24);
            panelToolsBottom.Controls.Add(btnOpenTool);
            panelToolsBottom.Controls.Add(btnLocateTool);
            panelToolsBottom.Controls.Add(btnDownloadTool);
            panelToolsBottom.Controls.Add(lblToolsHint);
            panelToolsBottom.Dock = DockStyle.Bottom;
            panelToolsBottom.Location = new Point(10, -14);
            panelToolsBottom.Name = "panelToolsBottom";
            panelToolsBottom.Padding = new Padding(12, 12, 12, 10);
            panelToolsBottom.Size = new Size(172, 76);
            panelToolsBottom.TabIndex = 1;
            // 
            // btnOpenTool
            // 
            btnOpenTool.FlatStyle = FlatStyle.System;
            btnOpenTool.Location = new Point(12, 12);
            btnOpenTool.Name = "btnOpenTool";
            btnOpenTool.Size = new Size(120, 36);
            btnOpenTool.TabIndex = 0;
            btnOpenTool.Text = "Open";
            // 
            // btnLocateTool
            // 
            btnLocateTool.FlatStyle = FlatStyle.System;
            btnLocateTool.Location = new Point(140, 12);
            btnLocateTool.Name = "btnLocateTool";
            btnLocateTool.Size = new Size(120, 36);
            btnLocateTool.TabIndex = 1;
            btnLocateTool.Text = "Locate";
            // 
            // btnDownloadTool
            // 
            btnDownloadTool.FlatStyle = FlatStyle.System;
            btnDownloadTool.Location = new Point(268, 12);
            btnDownloadTool.Name = "btnDownloadTool";
            btnDownloadTool.Size = new Size(120, 36);
            btnDownloadTool.TabIndex = 2;
            btnDownloadTool.Text = "Download";
            // 
            // lblToolsHint
            // 
            lblToolsHint.AutoSize = true;
            lblToolsHint.ForeColor = Color.Gainsboro;
            lblToolsHint.Location = new Point(410, 20);
            lblToolsHint.Name = "lblToolsHint";
            lblToolsHint.Size = new Size(218, 15);
            lblToolsHint.TabIndex = 3;
            lblToolsHint.Text = "Select tool → Open / Locate / Download";
            // 
            // panelToolsTop
            // 
            panelToolsTop.BackColor = Color.FromArgb(18, 18, 24);
            panelToolsTop.Controls.Add(lblToolsTitle);
            panelToolsTop.Controls.Add(lblToolsDesc);
            panelToolsTop.Dock = DockStyle.Top;
            panelToolsTop.Location = new Point(10, 10);
            panelToolsTop.Name = "panelToolsTop";
            panelToolsTop.Padding = new Padding(12, 10, 12, 10);
            panelToolsTop.Size = new Size(172, 72);
            panelToolsTop.TabIndex = 2;
            // 
            // lblToolsTitle
            // 
            lblToolsTitle.AutoSize = true;
            lblToolsTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblToolsTitle.ForeColor = Color.White;
            lblToolsTitle.Location = new Point(12, 12);
            lblToolsTitle.Name = "lblToolsTitle";
            lblToolsTitle.Size = new Size(157, 20);
            lblToolsTitle.TabIndex = 0;
            lblToolsTitle.Text = "Tools for moderation";
            // 
            // lblToolsDesc
            // 
            lblToolsDesc.AutoSize = true;
            lblToolsDesc.ForeColor = Color.Gainsboro;
            lblToolsDesc.Location = new Point(12, 40);
            lblToolsDesc.Name = "lblToolsDesc";
            lblToolsDesc.Size = new Size(431, 15);
            lblToolsDesc.TabIndex = 1;
            lblToolsDesc.Text = "Detect Everything / ShellBags and open them. Nothing is installed automatically.";
            // 
            // panelFooter
            // 
            panelFooter.BackColor = Color.FromArgb(14, 14, 20);
            panelFooter.Controls.Add(linkGitHub);
            panelFooter.Controls.Add(linkBio);
            panelFooter.Controls.Add(lblMadeWith);
            panelFooter.Dock = DockStyle.Bottom;
            panelFooter.Location = new Point(0, 686);
            panelFooter.Name = "panelFooter";
            panelFooter.Padding = new Padding(12, 8, 12, 8);
            panelFooter.Size = new Size(1100, 34);
            panelFooter.TabIndex = 1;
            // 
            // linkGitHub
            // 
            linkGitHub.ActiveLinkColor = Color.FromArgb(160, 220, 255);
            linkGitHub.AutoSize = true;
            linkGitHub.LinkColor = Color.FromArgb(120, 200, 255);
            linkGitHub.Location = new Point(12, 9);
            linkGitHub.Name = "linkGitHub";
            linkGitHub.Size = new Size(120, 15);
            linkGitHub.TabIndex = 0;
            linkGitHub.TabStop = true;
            linkGitHub.Text = "github.com/Nezeryxs";
            linkGitHub.VisitedLinkColor = Color.FromArgb(120, 200, 255);
            // 
            // linkBio
            // 
            linkBio.ActiveLinkColor = Color.FromArgb(160, 220, 255);
            linkBio.AutoSize = true;
            linkBio.LinkColor = Color.FromArgb(120, 200, 255);
            linkBio.Location = new Point(170, 9);
            linkBio.Name = "linkBio";
            linkBio.Size = new Size(92, 15);
            linkBio.TabIndex = 1;
            linkBio.TabStop = true;
            linkBio.Text = "e-z.bio/nezeryxs";
            linkBio.VisitedLinkColor = Color.FromArgb(120, 200, 255);
            // 
            // lblMadeWith
            // 
            lblMadeWith.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblMadeWith.AutoSize = true;
            lblMadeWith.ForeColor = Color.Gainsboro;
            lblMadeWith.Location = new Point(1880, 9);
            lblMadeWith.Name = "lblMadeWith";
            lblMadeWith.Size = new Size(113, 15);
            lblMadeWith.TabIndex = 2;
            lblMadeWith.Text = "Made with ChatGPT";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            BackgroundImage = Properties.Resources.load;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(1100, 720);
            Controls.Add(tabControl1);
            Controls.Add(panelFooter);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ScumChecker by nezeryxs";
            Load += Form1_Load;
            tabControl1.ResumeLayout(false);
            tabNative.ResumeLayout(false);
            splitNative.Panel1.ResumeLayout(false);
            splitNative.Panel2.ResumeLayout(false);
            splitNative.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitNative).EndInit();
            splitNative.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvFindings).EndInit();
            panelTop.ResumeLayout(false);
            panelTop.PerformLayout();
            panelSummary.ResumeLayout(false);
            panelSummary.PerformLayout();
            tabSteam.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvSteam).EndInit();
            panelSteamTop.ResumeLayout(false);
            panelSteamTop.PerformLayout();
            tabTools.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvTools).EndInit();
            panelToolsBottom.ResumeLayout(false);
            panelToolsBottom.PerformLayout();
            panelToolsTop.ResumeLayout(false);
            panelToolsTop.PerformLayout();
            panelFooter.ResumeLayout(false);
            panelFooter.PerformLayout();
            ResumeLayout(false);
        }
        #endregion
    }
}
