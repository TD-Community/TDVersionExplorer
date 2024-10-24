namespace TDVersionExplorer
{
    partial class FormExplorer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormExplorer));
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.ColumnResult = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnSelect = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ColumnFileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnIcon = new System.Windows.Forms.DataGridViewImageColumn();
            this.ColumnTDVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnVersionCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnFormat = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnBitness = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnEncoding = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnResultTxt = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnObject = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textBoxFolder = new System.Windows.Forms.TextBox();
            this.textBoxDestinationFolder = new System.Windows.Forms.TextBox();
            this.buttonSelectFolder = new System.Windows.Forms.Button();
            this.buttonConvert = new System.Windows.Forms.Button();
            this.checkBoxAllFiles = new System.Windows.Forms.CheckBox();
            this.checkBoxSourceFiles = new System.Windows.Forms.CheckBox();
            this.checkBoxDynalibs = new System.Windows.Forms.CheckBox();
            this.checkBoxQRPs = new System.Windows.Forms.CheckBox();
            this.buttonSelectDestination = new System.Windows.Forms.Button();
            this.checkBoxCustomPath = new System.Windows.Forms.CheckBox();
            this.buttonGetFiles = new System.Windows.Forms.Button();
            this.textBoxFilter = new System.Windows.Forms.TextBox();
            this.comboBoxShowTDVersion = new System.Windows.Forms.ComboBox();
            this.comboBoxDestVersion = new System.Windows.Forms.ComboBox();
            this.labelDestTDVersion = new System.Windows.Forms.Label();
            this.labelDestTextEncoding = new System.Windows.Forms.Label();
            this.comboBoxDestEncoding = new System.Windows.Forms.ComboBox();
            this.checkBoxForceConversion = new System.Windows.Forms.CheckBox();
            this.labelDestFormat = new System.Windows.Forms.Label();
            this.comboBoxDestFormat = new System.Windows.Forms.ComboBox();
            this.checkBoxInclDLLEXE = new System.Windows.Forms.CheckBox();
            this.groupBoxAnalyze = new System.Windows.Forms.GroupBox();
            this.groupBoxFilter = new System.Windows.Forms.GroupBox();
            this.checkBoxTDDllExe = new System.Windows.Forms.CheckBox();
            this.checkBoxTDSampleFiles = new System.Windows.Forms.CheckBox();
            this.checkBoxShowServers = new System.Windows.Forms.CheckBox();
            this.groupBoxDestination = new System.Windows.Forms.GroupBox();
            this.checkBoxRenameExt = new System.Windows.Forms.CheckBox();
            this.groupBoxConvert = new System.Windows.Forms.GroupBox();
            this.comboBoxLogLevel = new System.Windows.Forms.ComboBox();
            this.labelLogLevel = new System.Windows.Forms.Label();
            this.buttonOpenLog = new System.Windows.Forms.Button();
            this.linkLabelFolder = new System.Windows.Forms.LinkLabel();
            this.linkLabelDestination = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.groupBoxAnalyze.SuspendLayout();
            this.groupBoxFilter.SuspendLayout();
            this.groupBoxDestination.SuspendLayout();
            this.groupBoxConvert.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.AllowUserToResizeRows = false;
            this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView.BackgroundColor = System.Drawing.Color.White;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnResult,
            this.ColumnSelect,
            this.ColumnFileName,
            this.ColumnIcon,
            this.ColumnTDVersion,
            this.ColumnVersionCode,
            this.ColumnType,
            this.ColumnFormat,
            this.ColumnBitness,
            this.ColumnEncoding,
            this.ColumnResultTxt,
            this.ColumnObject});
            this.dataGridView.Location = new System.Drawing.Point(12, 139);
            this.dataGridView.MultiSelect = false;
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.ShowEditingIcon = false;
            this.dataGridView.Size = new System.Drawing.Size(1012, 489);
            this.dataGridView.TabIndex = 0;
            this.dataGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView_CellContentClick);
            this.dataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DataGridView_CellValueChanged);
            this.dataGridView.CurrentCellDirtyStateChanged += new System.EventHandler(this.DataGridView_CurrentCellDirtyStateChanged);
            this.dataGridView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataGridView_MouseDown);
            // 
            // ColumnResult
            // 
            this.ColumnResult.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnResult.HeaderText = "";
            this.ColumnResult.Name = "ColumnResult";
            this.ColumnResult.ReadOnly = true;
            this.ColumnResult.Width = 15;
            // 
            // ColumnSelect
            // 
            this.ColumnSelect.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnSelect.FalseValue = "";
            this.ColumnSelect.HeaderText = "";
            this.ColumnSelect.Name = "ColumnSelect";
            this.ColumnSelect.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColumnSelect.Width = 25;
            // 
            // ColumnFileName
            // 
            this.ColumnFileName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnFileName.HeaderText = "File Name";
            this.ColumnFileName.Name = "ColumnFileName";
            this.ColumnFileName.ReadOnly = true;
            this.ColumnFileName.Width = 300;
            // 
            // ColumnIcon
            // 
            this.ColumnIcon.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnIcon.HeaderText = "";
            this.ColumnIcon.Name = "ColumnIcon";
            this.ColumnIcon.ReadOnly = true;
            this.ColumnIcon.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ColumnIcon.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.ColumnIcon.Width = 25;
            // 
            // ColumnTDVersion
            // 
            this.ColumnTDVersion.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnTDVersion.HeaderText = "TD Version";
            this.ColumnTDVersion.Name = "ColumnTDVersion";
            this.ColumnTDVersion.ReadOnly = true;
            // 
            // ColumnVersionCode
            // 
            this.ColumnVersionCode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnVersionCode.HeaderText = "Version code";
            this.ColumnVersionCode.Name = "ColumnVersionCode";
            this.ColumnVersionCode.Width = 80;
            // 
            // ColumnType
            // 
            this.ColumnType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnType.HeaderText = "FileType";
            this.ColumnType.Name = "ColumnType";
            this.ColumnType.ReadOnly = true;
            this.ColumnType.Width = 70;
            // 
            // ColumnFormat
            // 
            this.ColumnFormat.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnFormat.HeaderText = "Outline Format";
            this.ColumnFormat.Name = "ColumnFormat";
            this.ColumnFormat.ReadOnly = true;
            // 
            // ColumnBitness
            // 
            this.ColumnBitness.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnBitness.HeaderText = "Bitness";
            this.ColumnBitness.Name = "ColumnBitness";
            this.ColumnBitness.ReadOnly = true;
            this.ColumnBitness.Width = 50;
            // 
            // ColumnEncoding
            // 
            this.ColumnEncoding.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnEncoding.HeaderText = "Encoding";
            this.ColumnEncoding.Name = "ColumnEncoding";
            this.ColumnEncoding.ReadOnly = true;
            this.ColumnEncoding.Width = 75;
            // 
            // ColumnResultTxt
            // 
            this.ColumnResultTxt.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnResultTxt.HeaderText = "Conversion result";
            this.ColumnResultTxt.Name = "ColumnResultTxt";
            this.ColumnResultTxt.ReadOnly = true;
            this.ColumnResultTxt.Width = 150;
            // 
            // ColumnObject
            // 
            this.ColumnObject.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.ColumnObject.HeaderText = "ColumnObject";
            this.ColumnObject.Name = "ColumnObject";
            this.ColumnObject.ReadOnly = true;
            this.ColumnObject.Visible = false;
            // 
            // textBoxFolder
            // 
            this.textBoxFolder.Location = new System.Drawing.Point(76, 9);
            this.textBoxFolder.Name = "textBoxFolder";
            this.textBoxFolder.Size = new System.Drawing.Size(512, 20);
            this.textBoxFolder.TabIndex = 1;
            this.textBoxFolder.TextChanged += new System.EventHandler(this.TextBoxFolder_TextChanged);
            // 
            // textBoxDestinationFolder
            // 
            this.textBoxDestinationFolder.Location = new System.Drawing.Point(76, 38);
            this.textBoxDestinationFolder.Name = "textBoxDestinationFolder";
            this.textBoxDestinationFolder.Size = new System.Drawing.Size(512, 20);
            this.textBoxDestinationFolder.TabIndex = 2;
            // 
            // buttonSelectFolder
            // 
            this.buttonSelectFolder.Location = new System.Drawing.Point(594, 7);
            this.buttonSelectFolder.Name = "buttonSelectFolder";
            this.buttonSelectFolder.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectFolder.TabIndex = 5;
            this.buttonSelectFolder.Text = "Select";
            this.buttonSelectFolder.UseVisualStyleBackColor = true;
            this.buttonSelectFolder.Click += new System.EventHandler(this.ButtonSelectFolder_Click);
            // 
            // buttonConvert
            // 
            this.buttonConvert.Location = new System.Drawing.Point(18, 20);
            this.buttonConvert.Name = "buttonConvert";
            this.buttonConvert.Size = new System.Drawing.Size(113, 35);
            this.buttonConvert.TabIndex = 6;
            this.buttonConvert.Text = "Convert";
            this.buttonConvert.UseVisualStyleBackColor = true;
            this.buttonConvert.Click += new System.EventHandler(this.ButtonConvert_Click);
            // 
            // checkBoxAllFiles
            // 
            this.checkBoxAllFiles.AutoSize = true;
            this.checkBoxAllFiles.Location = new System.Drawing.Point(16, 22);
            this.checkBoxAllFiles.Name = "checkBoxAllFiles";
            this.checkBoxAllFiles.Size = new System.Drawing.Size(58, 17);
            this.checkBoxAllFiles.TabIndex = 7;
            this.checkBoxAllFiles.Text = "All files";
            this.checkBoxAllFiles.UseVisualStyleBackColor = true;
            this.checkBoxAllFiles.CheckedChanged += new System.EventHandler(this.FilterContent);
            // 
            // checkBoxSourceFiles
            // 
            this.checkBoxSourceFiles.AutoSize = true;
            this.checkBoxSourceFiles.Checked = true;
            this.checkBoxSourceFiles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxSourceFiles.Location = new System.Drawing.Point(184, 22);
            this.checkBoxSourceFiles.Name = "checkBoxSourceFiles";
            this.checkBoxSourceFiles.Size = new System.Drawing.Size(81, 17);
            this.checkBoxSourceFiles.TabIndex = 8;
            this.checkBoxSourceFiles.Text = "TD sources";
            this.checkBoxSourceFiles.UseVisualStyleBackColor = true;
            this.checkBoxSourceFiles.CheckedChanged += new System.EventHandler(this.FilterContent);
            // 
            // checkBoxDynalibs
            // 
            this.checkBoxDynalibs.AutoSize = true;
            this.checkBoxDynalibs.Location = new System.Drawing.Point(275, 22);
            this.checkBoxDynalibs.Name = "checkBoxDynalibs";
            this.checkBoxDynalibs.Size = new System.Drawing.Size(66, 17);
            this.checkBoxDynalibs.TabIndex = 9;
            this.checkBoxDynalibs.Text = "Dynalibs";
            this.checkBoxDynalibs.UseVisualStyleBackColor = true;
            this.checkBoxDynalibs.CheckedChanged += new System.EventHandler(this.FilterContent);
            // 
            // checkBoxQRPs
            // 
            this.checkBoxQRPs.AutoSize = true;
            this.checkBoxQRPs.Location = new System.Drawing.Point(351, 22);
            this.checkBoxQRPs.Name = "checkBoxQRPs";
            this.checkBoxQRPs.Size = new System.Drawing.Size(95, 17);
            this.checkBoxQRPs.TabIndex = 10;
            this.checkBoxQRPs.Text = "Reports (QRP)";
            this.checkBoxQRPs.UseVisualStyleBackColor = true;
            this.checkBoxQRPs.CheckedChanged += new System.EventHandler(this.FilterContent);
            // 
            // buttonSelectDestination
            // 
            this.buttonSelectDestination.Location = new System.Drawing.Point(594, 36);
            this.buttonSelectDestination.Name = "buttonSelectDestination";
            this.buttonSelectDestination.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectDestination.TabIndex = 11;
            this.buttonSelectDestination.Text = "Select";
            this.buttonSelectDestination.UseVisualStyleBackColor = true;
            this.buttonSelectDestination.Click += new System.EventHandler(this.ButtonSelectDestination_Click);
            // 
            // checkBoxCustomPath
            // 
            this.checkBoxCustomPath.AutoSize = true;
            this.checkBoxCustomPath.Location = new System.Drawing.Point(682, 40);
            this.checkBoxCustomPath.Name = "checkBoxCustomPath";
            this.checkBoxCustomPath.Size = new System.Drawing.Size(61, 17);
            this.checkBoxCustomPath.TabIndex = 12;
            this.checkBoxCustomPath.Text = "Custom";
            this.checkBoxCustomPath.UseVisualStyleBackColor = true;
            this.checkBoxCustomPath.CheckedChanged += new System.EventHandler(this.CheckBoxCustomPath_CheckedChanged);
            // 
            // buttonGetFiles
            // 
            this.buttonGetFiles.Location = new System.Drawing.Point(17, 20);
            this.buttonGetFiles.Name = "buttonGetFiles";
            this.buttonGetFiles.Size = new System.Drawing.Size(113, 23);
            this.buttonGetFiles.TabIndex = 13;
            this.buttonGetFiles.Text = "Analyze folder";
            this.buttonGetFiles.UseVisualStyleBackColor = true;
            this.buttonGetFiles.Click += new System.EventHandler(this.ButtonGetFiles_Click);
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Location = new System.Drawing.Point(452, 20);
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.Size = new System.Drawing.Size(131, 20);
            this.textBoxFilter.TabIndex = 14;
            this.textBoxFilter.TextChanged += new System.EventHandler(this.TextBoxFilter_TextChanged);
            // 
            // comboBoxShowTDVersion
            // 
            this.comboBoxShowTDVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxShowTDVersion.FormattingEnabled = true;
            this.comboBoxShowTDVersion.Location = new System.Drawing.Point(594, 20);
            this.comboBoxShowTDVersion.Name = "comboBoxShowTDVersion";
            this.comboBoxShowTDVersion.Size = new System.Drawing.Size(135, 21);
            this.comboBoxShowTDVersion.Sorted = true;
            this.comboBoxShowTDVersion.TabIndex = 15;
            this.comboBoxShowTDVersion.SelectedIndexChanged += new System.EventHandler(this.ComboBoxShowTDVersion_SelectedIndexChanged);
            // 
            // comboBoxDestVersion
            // 
            this.comboBoxDestVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDestVersion.FormattingEnabled = true;
            this.comboBoxDestVersion.Location = new System.Drawing.Point(78, 20);
            this.comboBoxDestVersion.Name = "comboBoxDestVersion";
            this.comboBoxDestVersion.Size = new System.Drawing.Size(120, 21);
            this.comboBoxDestVersion.Sorted = true;
            this.comboBoxDestVersion.TabIndex = 16;
            // 
            // labelDestTDVersion
            // 
            this.labelDestTDVersion.AutoSize = true;
            this.labelDestTDVersion.Location = new System.Drawing.Point(16, 23);
            this.labelDestTDVersion.Name = "labelDestTDVersion";
            this.labelDestTDVersion.Size = new System.Drawing.Size(56, 13);
            this.labelDestTDVersion.TabIndex = 17;
            this.labelDestTDVersion.Text = "Convert to";
            // 
            // labelDestTextEncoding
            // 
            this.labelDestTextEncoding.AutoSize = true;
            this.labelDestTextEncoding.Location = new System.Drawing.Point(428, 23);
            this.labelDestTextEncoding.Name = "labelDestTextEncoding";
            this.labelDestTextEncoding.Size = new System.Drawing.Size(75, 13);
            this.labelDestTextEncoding.TabIndex = 19;
            this.labelDestTextEncoding.Text = "Text encoding";
            // 
            // comboBoxDestEncoding
            // 
            this.comboBoxDestEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDestEncoding.FormattingEnabled = true;
            this.comboBoxDestEncoding.Location = new System.Drawing.Point(509, 20);
            this.comboBoxDestEncoding.Name = "comboBoxDestEncoding";
            this.comboBoxDestEncoding.Size = new System.Drawing.Size(120, 21);
            this.comboBoxDestEncoding.Sorted = true;
            this.comboBoxDestEncoding.TabIndex = 18;
            // 
            // checkBoxForceConversion
            // 
            this.checkBoxForceConversion.AutoSize = true;
            this.checkBoxForceConversion.Location = new System.Drawing.Point(141, 30);
            this.checkBoxForceConversion.Name = "checkBoxForceConversion";
            this.checkBoxForceConversion.Size = new System.Drawing.Size(53, 17);
            this.checkBoxForceConversion.TabIndex = 20;
            this.checkBoxForceConversion.Text = "Force";
            this.checkBoxForceConversion.UseVisualStyleBackColor = true;
            // 
            // labelDestFormat
            // 
            this.labelDestFormat.AutoSize = true;
            this.labelDestFormat.Location = new System.Drawing.Point(212, 23);
            this.labelDestFormat.Name = "labelDestFormat";
            this.labelDestFormat.Size = new System.Drawing.Size(75, 13);
            this.labelDestFormat.TabIndex = 22;
            this.labelDestFormat.Text = "Outline Format";
            // 
            // comboBoxDestFormat
            // 
            this.comboBoxDestFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDestFormat.FormattingEnabled = true;
            this.comboBoxDestFormat.Location = new System.Drawing.Point(293, 20);
            this.comboBoxDestFormat.Name = "comboBoxDestFormat";
            this.comboBoxDestFormat.Size = new System.Drawing.Size(120, 21);
            this.comboBoxDestFormat.Sorted = true;
            this.comboBoxDestFormat.TabIndex = 21;
            // 
            // checkBoxInclDLLEXE
            // 
            this.checkBoxInclDLLEXE.AutoSize = true;
            this.checkBoxInclDLLEXE.Location = new System.Drawing.Point(136, 24);
            this.checkBoxInclDLLEXE.Name = "checkBoxInclDLLEXE";
            this.checkBoxInclDLLEXE.Size = new System.Drawing.Size(95, 17);
            this.checkBoxInclDLLEXE.TabIndex = 27;
            this.checkBoxInclDLLEXE.Text = "Incl. DLL/EXE";
            this.checkBoxInclDLLEXE.UseVisualStyleBackColor = true;
            // 
            // groupBoxAnalyze
            // 
            this.groupBoxAnalyze.Controls.Add(this.checkBoxInclDLLEXE);
            this.groupBoxAnalyze.Controls.Add(this.buttonGetFiles);
            this.groupBoxAnalyze.Location = new System.Drawing.Point(12, 73);
            this.groupBoxAnalyze.Name = "groupBoxAnalyze";
            this.groupBoxAnalyze.Size = new System.Drawing.Size(237, 57);
            this.groupBoxAnalyze.TabIndex = 28;
            this.groupBoxAnalyze.TabStop = false;
            this.groupBoxAnalyze.Text = "Analyze";
            // 
            // groupBoxFilter
            // 
            this.groupBoxFilter.Controls.Add(this.checkBoxTDDllExe);
            this.groupBoxFilter.Controls.Add(this.checkBoxDynalibs);
            this.groupBoxFilter.Controls.Add(this.checkBoxAllFiles);
            this.groupBoxFilter.Controls.Add(this.checkBoxSourceFiles);
            this.groupBoxFilter.Controls.Add(this.checkBoxQRPs);
            this.groupBoxFilter.Controls.Add(this.textBoxFilter);
            this.groupBoxFilter.Controls.Add(this.comboBoxShowTDVersion);
            this.groupBoxFilter.Location = new System.Drawing.Point(264, 74);
            this.groupBoxFilter.Name = "groupBoxFilter";
            this.groupBoxFilter.Size = new System.Drawing.Size(760, 56);
            this.groupBoxFilter.TabIndex = 29;
            this.groupBoxFilter.TabStop = false;
            this.groupBoxFilter.Text = "Show files filter";
            // 
            // checkBoxTDDllExe
            // 
            this.checkBoxTDDllExe.AutoSize = true;
            this.checkBoxTDDllExe.Location = new System.Drawing.Point(84, 22);
            this.checkBoxTDDllExe.Name = "checkBoxTDDllExe";
            this.checkBoxTDDllExe.Size = new System.Drawing.Size(90, 17);
            this.checkBoxTDDllExe.TabIndex = 16;
            this.checkBoxTDDllExe.Text = "TD DLL/EXE";
            this.checkBoxTDDllExe.UseVisualStyleBackColor = true;
            this.checkBoxTDDllExe.CheckedChanged += new System.EventHandler(this.FilterContent);
            // 
            // checkBoxTDSampleFiles
            // 
            this.checkBoxTDSampleFiles.AutoSize = true;
            this.checkBoxTDSampleFiles.Location = new System.Drawing.Point(682, 11);
            this.checkBoxTDSampleFiles.Name = "checkBoxTDSampleFiles";
            this.checkBoxTDSampleFiles.Size = new System.Drawing.Size(98, 17);
            this.checkBoxTDSampleFiles.TabIndex = 30;
            this.checkBoxTDSampleFiles.Text = "TD sample files";
            this.checkBoxTDSampleFiles.UseVisualStyleBackColor = true;
            this.checkBoxTDSampleFiles.CheckedChanged += new System.EventHandler(this.CheckBoxTDSampleFiles_CheckedChanged);
            // 
            // checkBoxShowServers
            // 
            this.checkBoxShowServers.AutoSize = true;
            this.checkBoxShowServers.Location = new System.Drawing.Point(934, 13);
            this.checkBoxShowServers.Name = "checkBoxShowServers";
            this.checkBoxShowServers.Size = new System.Drawing.Size(90, 17);
            this.checkBoxShowServers.TabIndex = 31;
            this.checkBoxShowServers.Text = "Show servers";
            this.checkBoxShowServers.UseVisualStyleBackColor = true;
            this.checkBoxShowServers.Visible = false;
            // 
            // groupBoxDestination
            // 
            this.groupBoxDestination.Controls.Add(this.checkBoxRenameExt);
            this.groupBoxDestination.Controls.Add(this.comboBoxDestFormat);
            this.groupBoxDestination.Controls.Add(this.comboBoxDestVersion);
            this.groupBoxDestination.Controls.Add(this.labelDestTDVersion);
            this.groupBoxDestination.Controls.Add(this.labelDestFormat);
            this.groupBoxDestination.Controls.Add(this.comboBoxDestEncoding);
            this.groupBoxDestination.Controls.Add(this.labelDestTextEncoding);
            this.groupBoxDestination.Location = new System.Drawing.Point(220, 646);
            this.groupBoxDestination.Name = "groupBoxDestination";
            this.groupBoxDestination.Size = new System.Drawing.Size(640, 74);
            this.groupBoxDestination.TabIndex = 32;
            this.groupBoxDestination.TabStop = false;
            this.groupBoxDestination.Text = "Destination";
            // 
            // checkBoxRenameExt
            // 
            this.checkBoxRenameExt.AutoSize = true;
            this.checkBoxRenameExt.Location = new System.Drawing.Point(78, 47);
            this.checkBoxRenameExt.Name = "checkBoxRenameExt";
            this.checkBoxRenameExt.Size = new System.Drawing.Size(130, 17);
            this.checkBoxRenameExt.TabIndex = 35;
            this.checkBoxRenameExt.Text = "Rename file extension";
            this.checkBoxRenameExt.UseVisualStyleBackColor = true;
            // 
            // groupBoxConvert
            // 
            this.groupBoxConvert.Controls.Add(this.buttonConvert);
            this.groupBoxConvert.Controls.Add(this.checkBoxForceConversion);
            this.groupBoxConvert.Location = new System.Drawing.Point(15, 646);
            this.groupBoxConvert.Name = "groupBoxConvert";
            this.groupBoxConvert.Size = new System.Drawing.Size(199, 74);
            this.groupBoxConvert.TabIndex = 33;
            this.groupBoxConvert.TabStop = false;
            this.groupBoxConvert.Text = "Conversion";
            // 
            // comboBoxLogLevel
            // 
            this.comboBoxLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLogLevel.FormattingEnabled = true;
            this.comboBoxLogLevel.Items.AddRange(new object[] {
            "DEBUG",
            "INFO",
            "OFF"});
            this.comboBoxLogLevel.Location = new System.Drawing.Point(934, 646);
            this.comboBoxLogLevel.Name = "comboBoxLogLevel";
            this.comboBoxLogLevel.Size = new System.Drawing.Size(90, 21);
            this.comboBoxLogLevel.Sorted = true;
            this.comboBoxLogLevel.TabIndex = 23;
            // 
            // labelLogLevel
            // 
            this.labelLogLevel.AutoSize = true;
            this.labelLogLevel.Location = new System.Drawing.Point(878, 649);
            this.labelLogLevel.Name = "labelLogLevel";
            this.labelLogLevel.Size = new System.Drawing.Size(50, 13);
            this.labelLogLevel.TabIndex = 34;
            this.labelLogLevel.Text = "Log level";
            // 
            // buttonOpenLog
            // 
            this.buttonOpenLog.Location = new System.Drawing.Point(934, 674);
            this.buttonOpenLog.Name = "buttonOpenLog";
            this.buttonOpenLog.Size = new System.Drawing.Size(90, 23);
            this.buttonOpenLog.TabIndex = 35;
            this.buttonOpenLog.Text = "Open log";
            this.buttonOpenLog.UseVisualStyleBackColor = true;
            this.buttonOpenLog.Click += new System.EventHandler(this.ButtonOpenLog_Click);
            // 
            // linkLabelFolder
            // 
            this.linkLabelFolder.AutoSize = true;
            this.linkLabelFolder.Location = new System.Drawing.Point(12, 12);
            this.linkLabelFolder.Name = "linkLabelFolder";
            this.linkLabelFolder.Size = new System.Drawing.Size(36, 13);
            this.linkLabelFolder.TabIndex = 36;
            this.linkLabelFolder.TabStop = true;
            this.linkLabelFolder.Text = "Folder";
            this.linkLabelFolder.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelFolder_LinkClicked);
            // 
            // linkLabelDestination
            // 
            this.linkLabelDestination.AutoSize = true;
            this.linkLabelDestination.Location = new System.Drawing.Point(12, 41);
            this.linkLabelDestination.Name = "linkLabelDestination";
            this.linkLabelDestination.Size = new System.Drawing.Size(60, 13);
            this.linkLabelDestination.TabIndex = 37;
            this.linkLabelDestination.TabStop = true;
            this.linkLabelDestination.Text = "Destination";
            this.linkLabelDestination.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelDestination_LinkClicked);
            // 
            // FormExplorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1036, 741);
            this.Controls.Add(this.linkLabelDestination);
            this.Controls.Add(this.linkLabelFolder);
            this.Controls.Add(this.buttonOpenLog);
            this.Controls.Add(this.labelLogLevel);
            this.Controls.Add(this.comboBoxLogLevel);
            this.Controls.Add(this.checkBoxShowServers);
            this.Controls.Add(this.checkBoxTDSampleFiles);
            this.Controls.Add(this.checkBoxCustomPath);
            this.Controls.Add(this.buttonSelectDestination);
            this.Controls.Add(this.buttonSelectFolder);
            this.Controls.Add(this.textBoxDestinationFolder);
            this.Controls.Add(this.textBoxFolder);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.groupBoxAnalyze);
            this.Controls.Add(this.groupBoxFilter);
            this.Controls.Add(this.groupBoxDestination);
            this.Controls.Add(this.groupBoxConvert);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormExplorer";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "TD Version Explorer";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.groupBoxAnalyze.ResumeLayout(false);
            this.groupBoxAnalyze.PerformLayout();
            this.groupBoxFilter.ResumeLayout(false);
            this.groupBoxFilter.PerformLayout();
            this.groupBoxDestination.ResumeLayout(false);
            this.groupBoxDestination.PerformLayout();
            this.groupBoxConvert.ResumeLayout(false);
            this.groupBoxConvert.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.TextBox textBoxFolder;
        private System.Windows.Forms.TextBox textBoxDestinationFolder;
        private System.Windows.Forms.Button buttonSelectFolder;
        private System.Windows.Forms.Button buttonConvert;
        private System.Windows.Forms.CheckBox checkBoxAllFiles;
        private System.Windows.Forms.CheckBox checkBoxSourceFiles;
        private System.Windows.Forms.CheckBox checkBoxDynalibs;
        private System.Windows.Forms.CheckBox checkBoxQRPs;
        private System.Windows.Forms.Button buttonSelectDestination;
        private System.Windows.Forms.CheckBox checkBoxCustomPath;
        private System.Windows.Forms.Button buttonGetFiles;
        private System.Windows.Forms.TextBox textBoxFilter;
        private System.Windows.Forms.ComboBox comboBoxShowTDVersion;
        private System.Windows.Forms.ComboBox comboBoxDestVersion;
        private System.Windows.Forms.Label labelDestTDVersion;
        private System.Windows.Forms.Label labelDestTextEncoding;
        private System.Windows.Forms.ComboBox comboBoxDestEncoding;
        private System.Windows.Forms.CheckBox checkBoxForceConversion;
        private System.Windows.Forms.Label labelDestFormat;
        private System.Windows.Forms.ComboBox comboBoxDestFormat;
        private System.Windows.Forms.CheckBox checkBoxInclDLLEXE;
        private System.Windows.Forms.GroupBox groupBoxAnalyze;
        private System.Windows.Forms.GroupBox groupBoxFilter;
        private System.Windows.Forms.CheckBox checkBoxTDSampleFiles;
        private System.Windows.Forms.CheckBox checkBoxShowServers;
        private System.Windows.Forms.GroupBox groupBoxDestination;
        private System.Windows.Forms.GroupBox groupBoxConvert;
        private System.Windows.Forms.ComboBox comboBoxLogLevel;
        private System.Windows.Forms.Label labelLogLevel;
        private System.Windows.Forms.CheckBox checkBoxRenameExt;
        private System.Windows.Forms.Button buttonOpenLog;
        private System.Windows.Forms.CheckBox checkBoxTDDllExe;
        private System.Windows.Forms.LinkLabel linkLabelFolder;
        private System.Windows.Forms.LinkLabel linkLabelDestination;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnResult;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ColumnSelect;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnFileName;
        private System.Windows.Forms.DataGridViewImageColumn ColumnIcon;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTDVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnVersionCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnType;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnFormat;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnBitness;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnEncoding;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnResultTxt;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnObject;
    }
}

