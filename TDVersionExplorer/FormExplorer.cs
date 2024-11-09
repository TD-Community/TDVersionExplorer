using log4net;
using log4net.Config;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TDVersionExplorer
{
    public partial class FormExplorer : Form
    {
        private FormProgress progressForm;
        private CancellationTokenSource cancellationTokenSource;
        private readonly Dictionary<Control, bool> controlStates = new Dictionary<Control, bool>();
        private double totalTimeInSeconds = 0;
        private double itemsPerSecond = 0;
        private int itemsConverted = 0;
        private int itemsError = 0;
        private DataGridViewCell clickedCell;
        private DataGridViewRow clickedRow;
        private readonly ContextMenuStrip headerMenu;
        private ContextMenuStrip contextMenu;
        private readonly ContextMenuStrip onlineMenu;
        private readonly ContextMenuStrip adminToolsMenu;

        private List<TDFileEx> TDFiles = new List<TDFileEx>();

        private string SelectedFolderTmp = string.Empty;

        public FormExplorer()
        {
            InitializeComponent();

            // Initialize list of TD versions and which ones are installed and present on the current system
            TDVersionRepository.GetInstalledTDVersions();

            this.StartPosition = FormStartPosition.CenterScreen;

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            this.Text = $"TD Version Explorer {version.Major}.{version.Minor}.{version.Build}";

            SetTooltips();

            // Create a context menu for the checkbox column header
            headerMenu = new ContextMenuStrip();
            headerMenu.Items.Add("Select All", null, (s, e) => AllCheckBoxes(true));
            headerMenu.Items.Add("Deselect All", null, (s, e) => AllCheckBoxes(false));

            onlineMenu = new ContextMenuStrip();
            onlineMenu.Items.Add("TD books", null, (s, e) => OnlineMenuExecute("https://samples.tdcommunity.net/index.php?dir=Misc/TD_Books/"));
            onlineMenu.Items.Add("TD release notes", null, (s, e) => OnlineMenuExecute("https://samples.tdcommunity.net/index.php?dir=Misc/TD_ReleaseNotes/"));
            onlineMenu.Items.Add(new ToolStripSeparator());
            onlineMenu.Items.Add("TD forum", null, (s, e) => OnlineMenuExecute("https://forum.tdcommunity.net/"));

            adminToolsMenu = new ContextMenuStrip();
            adminToolsMenu.Items.Add("Open log", null, (s, e) => OpenLog());
            adminToolsMenu.Items.Add("Open temp folder", null, (s, e) => OpenTempFolder());
            adminToolsMenu.Items.Add("Delete temp folder", null, (s, e) => DeleteTempFolder());
            adminToolsMenu.Items.Add("Terminate helper processes", null, (s, e) => TerminateAllProcesses());
            adminToolsMenu.Items.Add("Export gridview to html", null, (s, e) => ExportGridViewToHtml());

            dataGridView.ColumnHeaderMouseClick += DataGridView_ColumnHeaderMouseClick;

            PopulateTDVersionCombos();
            PopulateDestFormatCombo();
            PopulateDestEncodingCombo();
            comboBoxLogLevel.SelectedIndex = 0;

            // When selected folder and other options are saved to registry, put them back when starting
            ReadWriteRegistry(true);

            ManageDestinationFolder();

            ClearFiles(true);
        }

        private void ReadWriteRegistry(bool read)
        {
            string KeyPath = @"SOFTWARE\TDVersionExplorer";

            try
            {
                if (read)
                {
                    RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyPath);
                    textBoxFolder.Text = (string)key.GetValue("Folder", Environment.CurrentDirectory);
                    object registryValue = key.GetValue("UseCustomDestinationFolder", null);
                    if (registryValue != null)
                    {
                        // Convert the DWORD (int) to a boolean value (0 = false, 1 = true)
                        int dwordValue = (int)registryValue;
                        checkBoxCustomPath.Checked = dwordValue == 1;
                    }
                    if (checkBoxCustomPath.Checked)
                        textBoxDestinationFolder.Text = (string)key.GetValue("CustomDestinationFolder", "");
                    key.Close();
                }
                else
                {
                    if (!checkBoxTDSampleFiles.Checked)
                    {
                        RegistryKey key = Registry.CurrentUser.CreateSubKey(KeyPath);
                        key.SetValue("Folder", textBoxFolder.Text);
                        key.SetValue("UseCustomDestinationFolder", checkBoxCustomPath.Checked ? 1 : 0, RegistryValueKind.DWord);
                        if (checkBoxCustomPath.Checked)
                            key.SetValue("CustomDestinationFolder", textBoxDestinationFolder.Text);
                        key.Close();
                    }
                }
            }
            catch (Exception)
            {
                // Do not bother about saved settings
            }
        }

        private void PopulateTDVersionCombos()
        {
            foreach (KeyValuePair<TDVersion, TDVersionInfo> item in TDVersionRepository.Versions)
            {
                if(item.Key != TDVersion.UNKNOWN)
                {
                    comboBoxShowTDVersion.Items.Add(item.Key.ToString());

                    if(item.Key > TDVersion.TD11 && item.Key != TDVersion.TD750)
                        comboBoxDestVersion.Items.Add(item.Key.ToString());
                }
            }

            comboBoxShowTDVersion.SelectedIndex = comboBoxShowTDVersion.Items.Add("All TD versions");
            comboBoxDestVersion.SelectedIndex = comboBoxDestVersion.Items.Add(TDVersion.KEEP_ORIGINAL.ToString());
        }

        private void PopulateDestFormatCombo()
        {
            foreach (TDOutlineFormat format in Enum.GetValues(typeof(TDOutlineFormat)))
            {
                if (format > 0 && format != TDOutlineFormat.COMPILED)
                {
                    int index = comboBoxDestFormat.Items.Add(format);
                    if (format == TDOutlineFormat.KEEP_ORIGINAL)
                        comboBoxDestFormat.SelectedIndex = index;
                }
            }
        }

        private void PopulateDestEncodingCombo()
        {
            foreach (TDEncoding encoding in Enum.GetValues(typeof(TDEncoding)))
            {
                if (encoding > 0)
                {
                    int index = comboBoxDestEncoding.Items.Add(encoding);
                    if (encoding == TDEncoding.KEEP_ORIGINAL)
                        comboBoxDestEncoding.SelectedIndex = index;
                }
            }
        }

        private void ClearFiles(bool AlsoClearDomain)
        {
            dataGridView.EndEdit();
            dataGridView.Rows.Clear();
            dataGridView.Refresh();
            if(AlsoClearDomain)
                TDFiles.Clear();
            ManageGUI();
        }

        private void ManageGUI()
        {
            groupBoxAnalyze.Text = $"Analyze ({TDFiles.Count})";
            groupBoxFilter.Text = $"Show files filter ({dataGridView.RowCount})";

            int selected = GetCheckedRowCount();

            buttonConvert.Text = $"Convert ({selected})";

            buttonConvert.Enabled = (selected > 0);
        }

        private void SetTooltips()
        {
            System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip
            {
                AutomaticDelay = 200, // Delay before the tooltip appears
                AutoPopDelay = 5000, // Duration the tooltip remains visible
                InitialDelay = 100, // Delay before the tooltip appears for the first time
                ReshowDelay = 100 // Delay before the tooltip appears again
            };
            toolTip.SetToolTip(textBoxFilter, "Enter text to search the grid.");

            toolTip = new System.Windows.Forms.ToolTip
            {
                AutomaticDelay = 200, // Delay before the tooltip appears
                AutoPopDelay = 5000, // Duration the tooltip remains visible
                InitialDelay = 100, // Delay before the tooltip appears for the first time
                ReshowDelay = 100 // Delay before the tooltip appears again
            };
            toolTip.SetToolTip(linkLabelFolder, "Click here to open the folder.");

            toolTip = new System.Windows.Forms.ToolTip
            {
                AutomaticDelay = 200, // Delay before the tooltip appears
                AutoPopDelay = 5000, // Duration the tooltip remains visible
                InitialDelay = 100, // Delay before the tooltip appears for the first time
                ReshowDelay = 100 // Delay before the tooltip appears again
            };
            toolTip.SetToolTip(linkLabelDestination, "Click here to open the destination folder.");

            toolTip = new System.Windows.Forms.ToolTip
            {
                AutomaticDelay = 200, // Delay before the tooltip appears
                AutoPopDelay = 5000, // Duration the tooltip remains visible
                InitialDelay = 100, // Delay before the tooltip appears for the first time
                ReshowDelay = 100 // Delay before the tooltip appears again
            };
            toolTip.SetToolTip(checkBoxForceConversion, "Force conversion on files having the destination attributeds already");

            toolTip = new ToolTip
            {
                AutomaticDelay = 200, // Delay before the tooltip appears
                AutoPopDelay = 10000, // Duration the tooltip remains visible
                InitialDelay = 100, // Delay before the tooltip appears for the first time
                IsBalloon = true,
                ReshowDelay = 100 // Delay before the tooltip appears again
            };
            toolTip.SetToolTip(checkBoxRenameExt, "Use global convention:\nNormal = .app\nText = .apt\n\nLibraries (.apl) keep their extension");

            toolTip = new ToolTip
            {
                AutomaticDelay = 200, // Delay before the tooltip appears
                AutoPopDelay = 5000, // Duration the tooltip remains visible
                InitialDelay = 100, // Delay before the tooltip appears for the first time
                IsBalloon = false,
                ReshowDelay = 100 // Delay before the tooltip appears again
            };
            toolTip.SetToolTip(checkBoxTDSampleFiles, "Predefined list of TD related files");

            toolTip = new ToolTip
            {
                AutomaticDelay = 200, // Delay before the tooltip appears
                AutoPopDelay = 5000, // Duration the tooltip remains visible
                InitialDelay = 100, // Delay before the tooltip appears for the first time
                IsBalloon = false,
                ReshowDelay = 100 // Delay before the tooltip appears again
            };
            toolTip.SetToolTip(checkBoxFullCDKErrors, "Report all CDK errors. Can reduce performance significantly");

            toolTip = new ToolTip
            {
                AutomaticDelay = 200, // Delay before the tooltip appears
                AutoPopDelay = 5000, // Duration the tooltip remains visible
                InitialDelay = 100, // Delay before the tooltip appears for the first time
                IsBalloon = false,
                ReshowDelay = 100 // Delay before the tooltip appears again
            };
            toolTip.SetToolTip(checkBoxInclDLLEXE, "Also analyze exe and dll files. May reduce performance");
        }

        private void PopulateFiles()
        {
            dataGridView.EndEdit();
            ClearFiles(false);
            dataGridView.Columns["ColumnIcon"].DefaultCellStyle.NullValue = null;
            dataGridView.Columns["ColumnResultTxt"].DefaultCellStyle.Font = new Font("Arial", 8F, FontStyle.Bold);

            try
            {
                List<DataGridViewRow> rows = new List<DataGridViewRow>();

                foreach (TDFileEx file in TDFiles)
                {
                    string TDVersionStr = string.Empty;
                    bool populate = false;

                    if ((file.TDOutLineFormat == TDOutlineFormat.TEXT || file.TDOutLineFormat == TDOutlineFormat.TEXTINDENTED) && file.TDVersionInfo.NormalVersion == TDVersion.TD10)
                        TDVersionStr = "TD10_TD11";
                    else
                        TDVersionStr = file.TDVersionInfo.NormalVersion.ToString();

                    if (checkBoxAllFiles.Checked)
                        populate = true;
                    else
                    {
                        if (TDVersionStr == "TD10_TD11" && (comboBoxShowTDVersion.Text =="TD10" || comboBoxShowTDVersion.Text == "TD11"))
                            populate = true;
                        else if (comboBoxShowTDVersion.Text == TDVersionStr)
                            populate = true;

                        if (comboBoxShowTDVersion.Text == "All TD versions" || populate)
                        {
                            if (checkBoxDynalibs.Checked)
                                populate = populate || (file.TDFileType == TDFileType.DYNALIB);
                            if (checkBoxQRPs.Checked)
                                populate = populate || (file.TDFileType == TDFileType.QRP);
                            if (checkBoxSourceFiles.Checked)
                                populate = populate || (file.TDFileType == TDFileType.SOURCE);
                            if (checkBoxTDDllExe.Checked)
                                populate = populate || (file.TDFileType == TDFileType.EXE || file.TDFileType == TDFileType.DLL);
                        }
                    }

                    if (populate && !string.IsNullOrEmpty(textBoxFilter.Text))
                        populate = file.FileName.IndexOf(textBoxFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0;

                    if (populate)
                    {
                        DataGridViewRow row = new DataGridViewRow();
                        row.CreateCells(dataGridView);
                        row.Cells[0].Value = "";
                        row.Cells[1].Value = false;
                        row.Cells[2].Value = file.FileName;
                        if (file.TDVersionInfo.NormalVersion != TDVersion.UNKNOWN)
                        {
                            // Set the TD icons
                            if (file.TDVersionInfo.NormalVersion < TDVersion.TD15)
                                row.Cells[3].Value = Properties.Resources.cbi10_128;
                            else if (file.TDVersionInfo.NormalVersion < TDVersion.TD40)
                                row.Cells[3].Value = Properties.Resources.cbi15_128;
                            else if (file.TDVersionInfo.NormalVersion < TDVersion.TD60)
                                row.Cells[3].Value = Properties.Resources.cbi42_128;
                            else if (file.TDVersionInfo.NormalVersion < TDVersion.TD70)
                                row.Cells[3].Value = Properties.Resources.cbi60_128;
                            else if (file.TDVersionInfo.NormalVersion > TDVersion.TD63)
                                row.Cells[3].Value = Properties.Resources.cbi70_128;
                        }

                        row.Cells[4].Value = TDVersionStr.Equals("UNKNOWN") ? string.Empty : TDVersionStr;
                        if (!string.IsNullOrEmpty(file.TDOutlineVersionStr))
                            row.Cells[5].Value = file.TDOutlineVersionStr;
                        else if (file.TDVersionInfo.NormalVersion > 0)
                            row.Cells[5].Value = (int)file.TDVersionInfo.NormalVersion + " (0x" + ((int)file.TDVersionInfo.NormalVersion).ToString("X") + ")";
                        row.Cells[6].Value = file.TDFileType.ToString().Equals("UNKNOWN") ? string.Empty : file.TDFileType.ToString();
                        row.Cells[7].Value = file.TDOutLineFormat.ToString().Equals("UNKNOWN") ? string.Empty : file.TDOutLineFormat.ToString();
                        row.Cells[8].Value = file.TDBitness.ToString().Equals("UNKNOWN") ? string.Empty : file.TDBitness.ToString();
                        row.Cells[9].Value = file.TDEncoding.ToString().Equals("UNKNOWN") ? string.Empty : file.TDEncoding.ToString();
                        row.Cells[10].Value = string.Empty;
                        row.Cells[11].Value = file;

                        if (!file.CanBeConverted)
                        {
                            // Trick to remove checkbox from cell
                            row.Cells[1] = new DataGridViewTextBoxCell();
                            row.Cells[1].Value = string.Empty;
                            row.Cells[1].ReadOnly = true;
                        }

                        rows.Add(row);
                    }
                }
                dataGridView.Rows.AddRange(rows.ToArray());

                ManageGUI();
                dataGridView.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading files:\n\n{ex.Message}");
            }
        }

        private void PopulateConvertResults()
        {
            dataGridView.EndEdit();
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                TDFileEx file = (TDFileEx)row.Cells["columnObject"].Value;
                Color color = Color.White;
                Color colorTxt = Color.Black;
                string resultTxt = string.Empty;

                if (file.converterResult.resultCode != ConverterResultCode.UNKNOWN )
                {
                    resultTxt = file.converterResult.resultCode.ToString();
                    if (file.converterResult.resultCode == ConverterResultCode.CONVERTED)
                        color = Color.LightGreen;
                    else if (file.converterResult.resultCode == ConverterResultCode.ALREADYPORTED)
                        color = Color.LightBlue;
                    else if (file.converterResult.resultCode == ConverterResultCode.CONVERTED_WITH_ERRORS)
                        color = Color.Salmon;
                    else
                    {
                        color = Color.Red;
                        colorTxt = Color.White;
                    }
                }

                row.Cells["ColumnResult"].Style.BackColor = color;
                row.Cells["ColumnResultTxt"].Style.BackColor = color;
                row.Cells["ColumnResultTxt"].Style.ForeColor = colorTxt;
                row.Cells["ColumnResultTxt"].Value = resultTxt;
            }
        }

        private void DataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (dataGridView.Columns[e.ColumnIndex].Name == "ColumnSelect")
                    headerMenu.Show(Cursor.Position);
            }
        }

        private void AllCheckBoxes(bool ischecked)
        {
            dataGridView.EndEdit();
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!row.Cells["ColumnSelect"].ReadOnly)
                    row.Cells["ColumnSelect"].Value = ischecked;  // Set checkbox to checked
            }
            ManageGUI();
        }

        private void ManageDestinationFolder()
        {
            textBoxDestinationFolder.ReadOnly = !checkBoxCustomPath.Checked;
            buttonSelectDestination.Enabled = checkBoxCustomPath.Checked;

            if (string.IsNullOrEmpty(textBoxFolder.Text))
            {
                if (!checkBoxCustomPath.Checked)
                    textBoxDestinationFolder.Text = string.Empty;
            }
            else
            {
                if (!checkBoxCustomPath.Checked)
                    textBoxDestinationFolder.Text = Path.Combine(textBoxFolder.Text, "CONVERTED");
            }
        }

        private void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView.EndEdit();
            if (dataGridView.Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn && e.RowIndex >= 0)
            {
                DataGridViewCheckBoxCell cell = (DataGridViewCheckBoxCell)dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];

                if (cell.Value == null || cell.Value == "" || !(bool)cell.Value)
                    cell.Value = true;
                else
                    cell.Value = false;

                dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
                dataGridView.InvalidateCell(cell);
                dataGridView.EndEdit();
            }
        }

        private void ButtonSelectFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (string.IsNullOrEmpty( textBoxFolder.Text ))
                    folderBrowserDialog.SelectedPath = Environment.CurrentDirectory;
                else
                    folderBrowserDialog.SelectedPath = textBoxFolder.Text;

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxFolder.Text = folderBrowserDialog.SelectedPath;
                    ClearFiles(true);
                    ManageDestinationFolder();
                }
            }
        }

        private void ButtonSelectDestination_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (string.IsNullOrEmpty(textBoxDestinationFolder.Text))
                    folderBrowserDialog.SelectedPath = Environment.CurrentDirectory;
                else
                    folderBrowserDialog.SelectedPath = textBoxDestinationFolder.Text;

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    textBoxDestinationFolder.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private async void ButtonConvert_Click(object sender, EventArgs e)
        {
            StartProgressWindow(true);
            var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository);
            Logger.DeleteLogFile();
            Logger.SetLogFile();
            Logger.SetLogLevel(comboBoxLogLevel.Text);

            ConvertSettings settings = new ConvertSettings()
            {
                destinationfolder = textBoxDestinationFolder.Text,
                DestVersion = (TDVersion)Enum.Parse(typeof(TDVersion), comboBoxDestVersion.Text),
                DestFormat = (TDOutlineFormat)Enum.Parse(typeof(TDOutlineFormat), comboBoxDestFormat.Text),
                DestEncoding = (TDEncoding)Enum.Parse(typeof(TDEncoding), comboBoxDestEncoding.Text),
                attributes = ConverterAttribs.NONE
            };

            if (checkBoxForceConversion.Checked)
                settings.attributes |= ConverterAttribs.FORCE_CONVERSION;
            if (checkBoxRenameExt.Checked)
                settings.attributes |= ConverterAttribs.RENAME_EXTENSION;
            if (checkBoxShowServers.Checked)
                settings.attributes |= ConverterAttribs.SHOW_SERVERS;
            if (comboBoxLogLevel.Text == "DEBUG")
                settings.attributes |= ConverterAttribs.LOGLEVEL_DEBUG;
            if (checkBoxFullCDKErrors.Checked)
                settings.attributes |= ConverterAttribs.CDK_FULL_ERRORS;

            try
            {
                // Run the folder analysis in the background with the cancellation token
                await Task.Run(() => ExecuteConversion(settings, cancellationTokenSource.Token), cancellationTokenSource.Token);
                PopulateConvertResults();
                MessageBox.Show($"Total processed items : {itemsConverted}\nConversion errors: {itemsError}\n\nTotal time: {totalTimeInSeconds:F1} seconds\nItems per second: {itemsPerSecond:F1}", "Conversion finished", MessageBoxButtons.OK, itemsError > 0 ? MessageBoxIcon.Error : MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                PopulateConvertResults();
                MessageBox.Show("Conversion was cancelled.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred:\n{ex.Message}");
            }
        }

        private void ExecuteConversion(ConvertSettings settings, CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            itemsConverted = 0;
            itemsError = 0;
            totalTimeInSeconds = 0;
            itemsPerSecond = 0;

            int totalrows = GetCheckedRowCount();

            progressForm.Invoke((Action)(() =>
            {
                progressForm.SetMaximum(totalrows);
            }));

            try
            {
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    TDFileEx file = (TDFileEx)row.Cells["columnObject"].Value;
                    file.converterResult.resultCode = ConverterResultCode.UNKNOWN;

                    if (!row.IsNewRow && row.Cells["ColumnSelect"].Value is bool)
                    {
                        bool isChecked = Convert.ToBoolean(row.Cells["ColumnSelect"].Value);

                        if (isChecked)
                        {
                            ConverterParam convertParams = new ConverterParam()
                            {
                                source = file.FileFullPath,
                                destinationfolder = settings.destinationfolder,
                                DestVersion = settings.DestVersion,
                                DestFormat = settings.DestFormat,
                                DestEncoding = settings.DestEncoding,
                                attributes = settings.attributes
                            };

                            progressForm.BeginInvoke((Action)(() =>
                            {
                                progressForm.UpdateProgress(itemsConverted, file.FileName);
                            }));

                            file.StartConvert(convertParams);

                            if (!(file.converterResult.resultCode == ConverterResultCode.CONVERTED || file.converterResult.resultCode == ConverterResultCode.ALREADYPORTED))
                                itemsError += 1;

                            itemsConverted += 1;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                InvokeStopProgressWindow();
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:\n{ex.Message}");
            }

            stopwatch.Stop();

            totalTimeInSeconds = stopwatch.Elapsed.TotalSeconds;
            itemsPerSecond = itemsConverted / totalTimeInSeconds;
            InvokeStopProgressWindow();
        }

        private int GetCheckedRowCount()
        {
            int checkedRowCount = 0;

            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (!row.IsNewRow && row.Cells["ColumnSelect"].Value is bool)
                {
                    if (Convert.ToBoolean(row.Cells["ColumnSelect"].Value))
                        checkedRowCount++;
                }
            }

            return checkedRowCount;
        }

        private async void ButtonGetFiles_Click(object sender, EventArgs e)
        {
            ReadWriteRegistry(false);
            StartProgressWindow(true);

            try
            {
                // Run the folder analysis in the background with the cancellation token
                await Task.Run(() => AnalyzeFolder(textBoxFolder.Text, cancellationTokenSource.Token), cancellationTokenSource.Token);
                PopulateFiles();
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Analysis was cancelled.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred:\n{ex.Message}");
            }
            finally
            {
                StopProgressWindow();
            }
        }

        private void StartProgressWindow(bool show)
        {
            SetControlsEnabled(this, false);

            progressForm = new FormProgress
            {
                StartPosition = FormStartPosition.Manual
            };

            int x = this.Location.X + (this.Width - progressForm.Width) / 2;
            int y = this.Location.Y + (this.Height - progressForm.Height) / 2;

            progressForm.Location = new Point(x, y);

            progressForm.Owner = this;

            cancellationTokenSource = new CancellationTokenSource();

            progressForm.CancelAction = () => cancellationTokenSource.Cancel();

            if (show)
                progressForm.Show();
        }

        private void StopProgressWindow()
        {
            try
            {
                if (progressForm != null)
                {
                    progressForm.Close();
                    progressForm = null;
                }
            }
            catch (Exception)
            {
                //
            }

            SetControlsEnabled(this, true);
        }

        private void InvokeStopProgressWindow()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(StopProgressWindow));
            else
                StopProgressWindow();
        }

        private void SetControlsEnabled(Control parent, bool enabled)
        {
            foreach (Control control in parent.Controls)
            {
                if (enabled)
                {
                    if (controlStates.ContainsKey(control))
                        control.Enabled = true;
                }
                else
                {
                    if (control.Enabled)
                    {
                        if (!controlStates.ContainsKey(control))
                            controlStates.Add(control, control.Enabled);
                        control.Enabled = false;
                    }
                }

                if (control.HasChildren)
                    SetControlsEnabled(control, enabled);
            }

            if (enabled && parent == this)
                controlStates.Clear();
        }

        private void AnalyzeFolder(string folderPath, CancellationToken cancellationToken)
        {
            try
            {
                var files = Directory.EnumerateFiles(folderPath, "*", SearchOption.TopDirectoryOnly)
                             .Where(file => !IsHiddenOrSystem(file) &&
                                            (checkBoxInclDLLEXE.Checked || !IsExecutable(file)));
                int fileCount = files.Count();
                TDFiles = new List<TDFileEx>(fileCount);

                progressForm.Invoke((Action)(() =>
                {
                    progressForm.SetMaximum(fileCount);
                }));

                int count = 0;

                foreach (string file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!checkBoxInclDLLEXE.Checked)
                    {
                        string extension = Path.GetExtension(file).ToLowerInvariant();
                        if (extension == ".exe" || extension == ".dll")
                            continue;
                    }

                    TDFileEx tdfile = new TDFileEx();
                    if (tdfile.AnalyseFile(file))
                        TDFiles.Add(tdfile);

                    count += 1;
                    progressForm.BeginInvoke((Action)(() =>
                    {
                        progressForm.UpdateProgress(count, tdfile.FileName);
                    }));
                }
            }
            catch (OperationCanceledException)
            {
                InvokeStopProgressWindow();
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading files:\n{ex.Message}");
            }
        }

        private bool IsExecutable(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension == ".exe" || extension == ".dll";
        }

        private static bool IsHiddenOrSystem(string filePath)
        {
            var attributes = File.GetAttributes(filePath);
            return (attributes & FileAttributes.Hidden) != 0 ||
                   (attributes & FileAttributes.System) != 0;
        }

        private void FilterContent(object sender, EventArgs e)
        {
            checkBoxDynalibs.Enabled = !checkBoxAllFiles.Checked;
            checkBoxQRPs.Enabled = !checkBoxAllFiles.Checked;
            checkBoxSourceFiles.Enabled = !checkBoxAllFiles.Checked;
            comboBoxShowTDVersion.Enabled = !checkBoxAllFiles.Checked;
            checkBoxTDDllExe.Enabled = !checkBoxAllFiles.Checked;

            PopulateFiles();
        }

        private void ComboBoxShowTDVersion_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateFiles();
        }

        private void TextBoxFilter_TextChanged(object sender, EventArgs e)
        {
            PopulateFiles();
        }

        private void TextBoxFolder_TextChanged(object sender, EventArgs e)
        {
            ClearFiles(true);
            ManageDestinationFolder();
        }

        private void CheckBoxCustomPath_CheckedChanged(object sender, EventArgs e)
        {
            ManageDestinationFolder();
        }

        private void CheckBoxTDSampleFiles_CheckedChanged(object sender, EventArgs e)
        {
            string samplesfolder = AppDomain.CurrentDomain.BaseDirectory + "TDSampleFiles";

            if (Directory.Exists(samplesfolder))
            {
                buttonSelectFolder.Enabled = !checkBoxTDSampleFiles.Checked;
                textBoxFolder.ReadOnly = checkBoxTDSampleFiles.Checked;

                if (checkBoxTDSampleFiles.Checked)
                {
                    SelectedFolderTmp = textBoxFolder.Text;
                    textBoxFolder.Text = samplesfolder;
                }
                else
                    textBoxFolder.Text = SelectedFolderTmp;

                ManageDestinationFolder();
            }
            else
            {
                checkBoxTDSampleFiles.CheckedChanged -= CheckBoxTDSampleFiles_CheckedChanged;
                checkBoxTDSampleFiles.Checked = false;
                checkBoxTDSampleFiles.CheckedChanged += CheckBoxTDSampleFiles_CheckedChanged;

                MessageBox.Show($"TDSamples folder does not exist:\n\n{samplesfolder}\n\nMake sure to copy the TDSampleFiles folder to the application folder.", "TD Samples Folder error");
            }
        }

        private void DataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1 && e.RowIndex >= 0)
            {
                ManageGUI();
            }
        }

        private void DataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView.IsCurrentCellDirty && dataGridView.CurrentCell is DataGridViewCheckBoxCell)
                dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void LinkLabelFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string folderPath = textBoxFolder.Text;

            if (Directory.Exists(folderPath))
                try
                {
                    Process.Start("explorer.exe", folderPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error:\n{ex.Message}", "Open folder error");
                }
            else
                MessageBox.Show($"Folder does not exist:\n\n{folderPath}", "Open folder");
        }

        private void LinkLabelDestination_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string folderPath = textBoxDestinationFolder.Text;

            if (Directory.Exists(folderPath))
                try
                {
                    Process.Start("explorer.exe", folderPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error:\n{ex.Message}", "Open destination folder error");
                }
            else
                MessageBox.Show($"Destination folder does not exist:\n\n{folderPath}", "Open folder");
        }

        private void DataGridView_MouseDown(object sender, MouseEventArgs argEvent)
        {
            if (argEvent.Button == MouseButtons.Right)
            {
                var hitTestInfo = dataGridView.HitTest(argEvent.X, argEvent.Y);

                if (hitTestInfo.Type == DataGridViewHitTestType.Cell)
                {
                    if (hitTestInfo.ColumnIndex == 2)   // filename column
                    {
                        dataGridView.ClearSelection();
                        DataGridViewCell cell = dataGridView[hitTestInfo.ColumnIndex, hitTestInfo.RowIndex];
                        cell.Selected = true;

                        clickedCell = cell;
                        clickedRow = dataGridView.Rows[hitTestInfo.RowIndex];

                        TDFileEx file = (TDFileEx)clickedRow.Cells["columnObject"].Value;

                        int count = TDVersionRepository.GetAvailableTDInstalls(file.TDVersionInfo.NormalVersion, file.TDBitness, out List<InstalledTDVersionInfo> installs);

                        contextMenu = new ContextMenuStrip();
                        contextMenu.Items.Add("Copy filename", null, (s, e) => FileContextMenuExecute(FileContextMenu.FILE_COPY, ""));
                        contextMenu.Items.Add("Open file location", null, (s, e) => FileContextMenuExecute(FileContextMenu.FILE_SHOWLOC, ""));
                        
                        if (file.TDFileType == TDFileType.SOURCE)
                        {
                            contextMenu.Items.Add(new ToolStripSeparator());
                            if (count > 0)
                            {
                                foreach (InstalledTDVersionInfo install in installs)
                                {
                                    contextMenu.Items.Add($"Open using {install.VersionStr} ({install.Bitness})", null, (s, e) => FileContextMenuExecute(FileContextMenu.FILE_OPEN_TD, install));
                                }
                            }
                            else
                            {
                                ToolStripMenuItem unavailableOption = new ToolStripMenuItem($"{file.TDVersionInfo.NormalVersion} IDE not installed")
                                {
                                    Enabled = false
                                };
                                contextMenu.Items.Add(unavailableOption);
                            }
                        }
                        if (!string.IsNullOrEmpty(file.converterResult.errFile))
                        {
                            contextMenu.Items.Add(new ToolStripSeparator());
                            contextMenu.Items.Add($"Open err file", null, (s, e) => FileContextMenuExecute(FileContextMenu.FILE_OPEN_ERR, null));
                        }

                        contextMenu.Show(dataGridView, argEvent.Location);
                    }
                }
            }
        }

        private void FileContextMenuExecute(FileContextMenu param, object value)
        {
            TDFileEx file = (TDFileEx)clickedRow.Cells["columnObject"].Value;

            switch (param)
            {
                case FileContextMenu.FILE_SHOWLOC:
                    try
                    {
                        Process.Start("explorer.exe", $"/select,\"{file.FileFullPath}\"");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening file in Explorer:\n{ex.Message}");
                    }
                    break;
                case FileContextMenu.FILE_COPY:
                    Clipboard.SetText(clickedCell.Value.ToString());
                    break;
                case FileContextMenu.FILE_OPEN_TD:
                    InstalledTDVersionInfo version = (InstalledTDVersionInfo)value;

                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = version.InstallPath,
                        Arguments = $"\"{file.FileFullPath}\"",
                        UseShellExecute = false
                    };

                    string newPath = Path.GetDirectoryName(version.InstallPath);
                    startInfo.EnvironmentVariables["PATH"] = newPath + ";" + Environment.GetEnvironmentVariable("PATH");

                    startInfo.WorkingDirectory = Path.GetDirectoryName(version.InstallPath);

                    try
                    {
                        Process process = Process.Start(startInfo);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening file in TD:\n{ex.Message}");
                    }
                    break;
                case FileContextMenu.FILE_OPEN_ERR:
                    startInfo = new ProcessStartInfo
                    {
                        FileName = "notepad.exe",
                        Arguments = $"\"{file.converterResult.errFile}\"",
                        UseShellExecute = false
                    };

                    try
                    {
                        Process process = Process.Start(startInfo);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening err file:\n{ex.Message}");
                    }
                    break;
                default:
                    break;
            }
        }

        private void OnlineMenuExecute(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file in Explorer:\n{ex.Message}");
            }
        }

        private void ButtonOnlineInfo_Click(object sender, EventArgs argEvent)
        {
            onlineMenu.Show(buttonOnlineInfo, new Point(buttonOnlineInfo.Width / 2, buttonOnlineInfo.Height / 2));
        }

        private void ButtonAdminTools_Click(object sender, EventArgs e)
        {
            adminToolsMenu.Show(buttonAdminTools, new Point(buttonAdminTools.Width / 2, buttonAdminTools.Height / 2));
        }

        private void OpenLog()
        {
            if (!Logger.OpenLogFile())
                MessageBox.Show("Logfile not found!", "Open logfile error");
        }

        private void TerminateAllProcesses()
        {
            TDFileBase.TerminateAllProcesses();
        }

        private void OpenTempFolder()
        {
            try
            {
                if (Directory.Exists(TDFileBase.TempfolderBase))
                    Process.Start("explorer.exe", TDFileBase.TempfolderBase);
                else
                    MessageBox.Show($"Temp folder does not exist:\n\n{TDFileBase.TempfolderBase}", "Open temp folder");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:\n{ex.Message}", "Open temp folder error");
            }
        }

        private void DeleteTempFolder()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                TerminateAllProcesses();
                // Wait a while so all processes are killed.
                Thread.Sleep(1000);

                if (Directory.Exists(TDFileBase.TempfolderBase))
                {
                    Directory.Delete(TDFileBase.TempfolderBase, true);
                    MessageBox.Show($"Temp folder deleted:\n\n{TDFileBase.TempfolderBase}", "Delete temp folder");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:\n{ex.Message}", "Delete temp folder error");
            }
            Cursor.Current = Cursors.Default;
        }

        public void ExportGridViewToHtml()
        {
            string filePath = string.Empty;

            try
            {
                string tempFolder = Path.Combine(Path.GetTempPath(), "TDVersionExplorer");
                Directory.CreateDirectory(tempFolder);
                filePath = Path.Combine(tempFolder, "TDVersionExplorer_Export.html");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating file path: " + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var html = new StringBuilder();
            html.AppendLine("<html><head><style>");
            html.AppendLine("table { width: 100%; border-collapse: collapse; font-family: Arial, sans-serif; }");
            html.AppendLine("th, td { padding: 8px; text-align: left; border: 1px solid #ddd; }");
            html.AppendLine("th { background-color: #4CAF50; color: white; }");
            html.AppendLine("tr:nth-child(even) { background-color: #f2f2f2; }");
            html.AppendLine("tr:hover { background-color: #ddd; }");
            html.AppendLine("</style></head><body><table>");

            try
            {
                html.AppendLine("<tr>");
                for (int i = 0; i < dataGridView.Columns.Count; i++)
                {
                    if (dataGridView.Columns[i].Visible && i != 0 && i != 1 && i != 3)
                        html.AppendFormat("<th>{0}</th>", dataGridView.Columns[i].HeaderText);
                }
                html.AppendLine("</tr>");

                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        html.AppendLine("<tr>");
                        for (int i = 0; i < row.Cells.Count; i++)
                        {
                            if (row.Cells[i].Visible && i != 0 && i != 1 && i != 3)
                                html.AppendFormat("<td>{0}</td>", row.Cells[i].Value?.ToString() ?? string.Empty);
                        }
                        html.AppendLine("</tr>");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while building HTML content: " + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            html.AppendLine("</table></body></html>");

            try
            {
                File.WriteAllText(filePath, html.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving HTML file: " + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                 Process.Start(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening HTML file in browser: " + ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    internal enum FileContextMenu
    {
        FILE_COPY = 1,
        FILE_SHOWLOC = 2,
        FILE_OPEN_TD = 3,
        FILE_OPEN_ERR = 4
    }
}
