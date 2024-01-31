using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace CFSyncFolders
{
    public partial class MainForm : Form
    {
        private Mutex _uiMutex = new Mutex();
        private SyncManager _syncManager = null;
        private BackgroundWorker _worker = null;
        private DateTime _timeMouseOverNotify = DateTime.Now.AddYears(-1);
        private bool _isTaskTray = false;
        private System.Timers.Timer _timer = null;
        private bool _syncing = false;
        private string _logFile;
        //private string _logFile = "";     
        private List<string> _autoSyncConfigurationDescriptions = new List<string>();
        private DateTime _timeLastUpdateStatus = DateTime.MinValue;
        private Dictionary<string, DateTime> _timeLastGridUpdate = new Dictionary<string, DateTime>();
        
        public MainForm()
        {
            InitializeComponent();

            Control.CheckForIllegalCrossThreadCalls = false;        
            
            // Allow single instance only
            if (System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                Environment.Exit(0);
            }

            try
            {
                // Get path to executable
                string currentFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string dataFolder = System.Configuration.ConfigurationSettings.AppSettings.Get("DataFolder").ToString();
                if (dataFolder.Equals("{default}"))
                {
                    dataFolder = Path.Combine(currentFolder, "Configuration");
                }

                //_logFile = string.Format(@"{0}\Logs\Log-{1}-{2}.txt", currentFolder, DateTime.Now.Month, DateTime.Now.Year);
                //string configurationFolder = System.IO.Path.Combine(currentFolder, "Configuration");
                //dataFolder = @"C:\Data\Applications\CFSyncFolders\Configuration";   // Debugging

                _logFile = string.Format(@"{0}\Logs\{1}.txt", currentFolder, "{date}");
                _syncManager = new SyncManager(new LogFile(_logFile), dataFolder);
                //_syncManager.OnDisplayStatus += _syncManager_OnDisplayStatus;
                //_syncManager.OnFolderChecked += _syncManager_OnFolderChecked;
                //_syncManager.OnSyncFolderProgress += _syncManager_OnSyncFolderProgress;

                // Handle OnDisplayStatus
                _syncManager.OnDisplayStatus += delegate (string status)
                {
                    this.Invoke((Action)delegate 
                    {
                        try
                        {
                            DisplayStatus(status);
                        }
                        catch { };  // Ignore
                    });                    
                };

                // Handle OnSyncFolderProgress
                _syncManager.OnSyncFolderProgress += delegate (SyncManager.ProgressTypes progressType, SyncFoldersOptions syncFolderOptions, string folder1, FolderStatistics folderStatistics, int folderLevel)
                {
                    this.Invoke((Action)delegate
                    {
                        try
                        {
                            EventOnSyncFolderProgress(progressType, syncFolderOptions, folder1, folderStatistics, folderLevel);
                        }
                        catch { };  // Ignore
                    });
                };

                var syncConfigurations = _syncManager.GetSyncConfigurations();
                
                // Load auto-sync configs
                foreach (string arg in Environment.GetCommandLineArgs())
                {
                    if (arg.ToLower().StartsWith("/configs="))
                    {
                        var elements = arg.Split('=');
                        _autoSyncConfigurationDescriptions.AddRange(elements[1].Split(','));
                        foreach (var syncConfigurationDescription in _autoSyncConfigurationDescriptions)
                        {
                            var syncConfiguration = syncConfigurations.FirstOrDefault(sc => sc.Description.ToLower() == syncConfigurationDescription.ToLower());
                            if (syncConfiguration == null)
                            {
                                throw new Exception(string.Format("Sync configuration {0} does not exist", syncConfigurationDescription));
                            }
                        }
                    }
                }

                // Initialise folder grid          
                //_autoSyncConfigurationDescription = Environment.MachineName;            
                string defaultSyncDescription = _autoSyncConfigurationDescriptions.Count == 0 ? Environment.MachineName : _autoSyncConfigurationDescriptions.First();
                var defaultSyncConfiguration = _syncManager.GetSyncConfiguration(defaultSyncDescription);
               
                RefreshSyncConfigurations(defaultSyncConfiguration.ID);
               
                if (Environment.GetCommandLineArgs().Contains("/Tray"))    // Run continuously in system tray
                {
                    if (!_autoSyncConfigurationDescriptions.Any())
                    {
                        throw new Exception("No sync configurations specified");
                    }

                    // Determine if auto sync is required
                    bool isAutoSync = Environment.GetCommandLineArgs().Contains("/AutoSync");                  

                    _isTaskTray = true;
                    niNotify.Icon = SystemIcons.Application;    // new Icon(SystemIcons.Application);  //, 40, 40);
                    niNotify.Text = "Sync Folders - Idle";
                    WindowState = FormWindowState.Minimized;
                    tsbSync.Enabled = !isAutoSync;
                    tsbAddConfig.Visible = !isAutoSync;
                    tsbEditConfig.Visible = !isAutoSync;

                    _timer = new System.Timers.Timer();
                    _timer.Elapsed += _timer_Elapsed;
                    _timer.Interval = 10000 * 1;    // Run soon after launch
                    _timer.Enabled = isAutoSync;
                }
                else if (Environment.GetCommandLineArgs().Contains("/Silent"))    // Run and shut down (Called from Windows scheduler)
                {
                    tsbAddConfig.Visible = false;
                    tsbEditConfig.Visible = false;

                    foreach (var syncConfigurationDescription in _autoSyncConfigurationDescriptions)
                    {
                        bool isLast = syncConfigurationDescription == _autoSyncConfigurationDescriptions.Last();
                        RunSync(false, syncConfigurationDescription, true, false, isLast);

                        // If not last then wait for completion
                        while (!isLast && _syncing)
                        {
                            System.Threading.Thread.Sleep(200);
                            Application.DoEvents();
                        }
                    }
                }
            }
            catch(System.Exception exception)
            {
                MessageBox.Show(string.Format("Error starting application: {0}: {1}", exception.Message, exception.StackTrace), "Error");
                throw;
            }
        }

        private string GetLogFile()
        {     
            string logFile = _logFile.Replace("{date}", string.Format("{0}-{1}", DateTime.Now.Month, DateTime.Now.Year));
            //string logFile = string.Format(@"{ 0}\Logs\Log-{1}-{2}.txt", currentFolder, DateTime.Now.Month, DateTime.Now.Year);
            return logFile;
        }

        private void EventOnSyncFolderProgress(SyncManager.ProgressTypes progressType, SyncFoldersOptions syncFolderOptions, 
                                                string folder1, FolderStatistics folderStatistics, int folderLevel)
        {
            DateTime currentTime = DateTime.UtcNow;

            // Update main status
            TimeSpan timeSinceLastUpdateStatus = currentTime - _timeLastUpdateStatus;
            if (folderLevel == 1)
            {
                tssFolders.Text = string.Format("{0} {1}", (progressType == SyncManager.ProgressTypes.CompletedFolder ? "Synchronised" : "Synchronising"), syncFolderOptions.Folder1Resolved);
                statusStrip1.Refresh();
                switch (progressType)
                {
                    case SyncManager.ProgressTypes.StartingFolder: DisplayMessage(string.Format("Sychronising {0}", syncFolderOptions.Folder1Resolved)); break;
                    case SyncManager.ProgressTypes.CompletedFolder: DisplayMessage(string.Format("Synchronised {0}", syncFolderOptions.Folder1Resolved)); break;
                }
                _timeLastUpdateStatus = currentTime;
            }

            //if (folderLevel > 4)    // Don't update grid for lower level folders
            // {
            //    return;
            //}

            // No need to check, Sync Manager only raises this event periodically
            // Check if time to update grid, always update for top level folder
            bool updateGrid = true;
            /*
            TimeSpan timeSinceLastUpdate = currentTime - _timeLastGridUpdate[syncFolderOptions.Folder1Resolved];
            if (timeSinceLastUpdate.TotalMilliseconds >= 500 || folderLevel == 1)
            {
                updateGrid = true;
            }
            */

            // Update grid
            if (updateGrid)
            {
                bool waited = false;
                _timeLastGridUpdate[syncFolderOptions.Folder1Resolved] = currentTime;
                try
                {
                    waited = _uiMutex.WaitOne();                    
                    foreach (DataGridViewRow row in dgvFolders.Rows)
                    {
                        if (row.Cells["Folder"].Value != null && row.Cells["Folder"].Value.ToString() == syncFolderOptions.Folder1Resolved)
                        {
                            try
                            {
                                row.Cells["Last Completed"].Value = syncFolderOptions.TimeLastCompleted.ToString("dd/MM/yy HH:mm");

                                DateTime next = syncFolderOptions.TimeLastCompleted.AddSeconds(syncFolderOptions.FrequencySeconds);
                                row.Cells["Next"].Value = next.ToString("dd/MM/yy HH:mm");

                                row.Cells["New Files"].Value = folderStatistics.CountFilesNew;
                                row.Cells["Updated Files"].Value = folderStatistics.CountFilesUpdated;
                                row.Cells["Deleted Files"].Value = folderStatistics.CountFilesDeleted;
                                if (folderLevel == 1 && progressType == SyncManager.ProgressTypes.CompletedFolder)   // Completed
                                {
                                    int errorCount = folderStatistics.CountFileErrors + folderStatistics.CountFolderErrors;
                                    row.Cells["Status"].Value = errorCount == 0 ? "Completed" : string.Format("Completed ({0} errors)", errorCount);
                                }
                                else
                                {
                                    row.Cells["Status"].Value = folder1;
                                }
                                dgvFolders.InvalidateRow(row.Index);    // Repaint
                            }
                            catch
                            {
                                throw;
                            }
                            break;
                        }
                    }

                    // Force UI update
                    dgvFolders.Update();
                    dgvFolders.Refresh();
                }
                catch
                {
                    // Ignore error
                }
                finally
                {
                    if (waited)
                    {
                        _uiMutex.ReleaseMutex();
                    }
                }
            }
        }
       
        private void InitialiseFolderGrid(SyncConfiguration syncConfiguration)
        {
            _timeLastGridUpdate.Clear();
            
            syncConfiguration.FoldersOptions.ForEach(syncFolderOptions => _timeLastGridUpdate.Add(syncFolderOptions.Folder1Resolved, DateTime.MinValue));

            dgvFolders.Rows.Clear();
            dgvFolders.Columns.Clear();

            // Set columns
            int columnIndex = dgvFolders.Columns.Add("Folder", "Folder");
            dgvFolders.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            columnIndex = dgvFolders.Columns.Add("Last Completed", "Last Completed");
            dgvFolders.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvFolders.Columns[columnIndex].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            columnIndex = dgvFolders.Columns.Add("Next", "Next");
            dgvFolders.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvFolders.Columns[columnIndex].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            columnIndex = dgvFolders.Columns.Add("New Files", "New Files");
            dgvFolders.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvFolders.Columns[columnIndex].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            columnIndex = dgvFolders.Columns.Add("Updated Files", "Updated Files");
            dgvFolders.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvFolders.Columns[columnIndex].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            columnIndex = dgvFolders.Columns.Add("Deleted Files", "Deleted Files");
            dgvFolders.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvFolders.Columns[columnIndex].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            columnIndex = dgvFolders.Columns.Add("Status", "Status");
            dgvFolders.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            // Add rows
            foreach (SyncFoldersOptions syncFoldersOptions in syncConfiguration.FoldersOptions.OrderBy(o => o.Folder1Resolved))
            {
                using (DataGridViewRow row = new DataGridViewRow())
                {
                    using (DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell())
                    {
                        cell.Value = syncFoldersOptions.Folder1Resolved;
                        row.Cells.Add(cell);
                    }
                    using (DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell())
                    {
                        cell.Value = syncFoldersOptions.TimeLastCompleted.Year < 1900 ? "n/a" : syncFoldersOptions.TimeLastCompleted.ToString("dd/MM/yy HH:mm");
                        row.Cells.Add(cell);
                    }
                    using (DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell())
                    {
                        if (syncFoldersOptions.TimeLastCompleted.Year <= 1900)
                        {
                            cell.Value = "n/a";
                        }
                        else
                        {
                            DateTime next = syncFoldersOptions.TimeLastCompleted.AddSeconds(syncFoldersOptions.FrequencySeconds);
                            cell.Value = next.ToString("dd/MM/yy HH:mm");                            
                        }
                        row.Cells.Add(cell);
                    }
                    using (DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell())
                    {
                        cell.Value = 0;
                        row.Cells.Add(cell);
                    }
                    using (DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell())
                    {
                        cell.Value = 0;
                        row.Cells.Add(cell);
                    }
                    using (DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell())
                    {
                        cell.Value = 0;
                        row.Cells.Add(cell);
                    }
                    using (DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell())
                    {
                        cell.Value = syncFoldersOptions.Enabled ? "Pending" : "Disabled";
                        row.Cells.Add(cell);
                    }
                    dgvFolders.Rows.Add(row);
                }
            }
            dgvFolders.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _timer.Enabled = false;
                DisplayMessage("Starting periodic checks");

                if (!_syncing)    // Do nothing if sync in progress
                {
                    // Check each sync config
                    foreach (string syncConfigurationDescription in _autoSyncConfigurationDescriptions)
                    {                        
                        // Run sync
                        RunSync(false, syncConfigurationDescription, true, false, false);

                        if (_syncing)   // Only one sync at a time
                        {
                            break;
                        }
                    }
                }
            }
            finally
            {
                _timer.Interval = 60000;   // Every min
                _timer.Enabled = true;

                DisplayMessage("Completed periodic checks");
            }
        }

        //void _syncManager_OnDisplayStatus(string status)
        //{
        //    DisplayStatus(status);           
        //}       

        /// <summary>
        /// Runs the sync. Only need to sync if overdue or if being forced
        /// </summary>
        /// <param name="interactive"></param>
        /// <param name="syncConfigurationDescription"></param>
        /// <param name="ignoreLastStartTime"></param>
        /// <param name="oneTime"></param>
        private void RunSync(bool interactive, string syncConfigurationDescription, bool setInUI, bool ignoreLastStartTime, bool oneTime)
        {
            this.Visible = interactive;

            DisplayMessage(string.Format("Checking if sync needed for {0}", syncConfigurationDescription));
                       
            // Load SyncConfigurarion
            var syncConfiguration = _syncManager.GetSyncConfiguration(syncConfigurationDescription);            
            
            // Check if there's any sync'ing to do
            if (syncConfiguration.GetFoldersThatNeedSync(ignoreLastStartTime).Any())
            {
                // Check if we can sync. Removable drive may not be plugged in, other removable drive may be plugged
                // in but it doesn't have the verification file.
                string checkCanSyncMessage = _syncManager.CheckCanSyncFolders(syncConfiguration, ignoreLastStartTime,
                                    FileRepositoryFactory.GetFolder1FileRepository(),
                                    FileRepositoryFactory.GetFolder2FileRepository());
                                
                // Sync if we can
                if (String.IsNullOrEmpty(checkCanSyncMessage))
                {
                    if (setInUI)
                    {
                        tscbConfiguration.ComboBox.SelectedValue = syncConfiguration.ID;
                    }

                    DisplayMessage(string.Format("Starting sync of {0}", syncConfigurationDescription));

                    _worker = new BackgroundWorker();
                    _syncing = true;    // Set sync busy

                    // Define worker action
                    _worker.DoWork += new DoWorkEventHandler(
                    delegate (object o, DoWorkEventArgs args)
                    {
                        BackgroundWorker worker = o as BackgroundWorker;
                        tsbSync.Text = "Cancel";
                        tsbSync.Enabled = interactive;
                        niNotify.Text = "Sync Folders - Busy";
                        tscbConfiguration.Enabled = false;    // Prevent change
                        _syncManager.SyncFolders(syncConfiguration, ignoreLastStartTime,
                                        FileRepositoryFactory.GetFolder1FileRepository(),
                                        FileRepositoryFactory.GetFolder2FileRepository());
                    });

                    // Define worker completion action
                    _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                    delegate (object o, RunWorkerCompletedEventArgs args)
                    {                        
                        tsbSync.Text = "Sync";
                        tsbSync.Enabled = interactive;
                        niNotify.Text = "Sync Folders - Idle";
                        tssFolders.Text = "Ready";
                        tscbConfiguration.Enabled = true;
                        DisplayStatus(_syncManager.Cancelled ? "Cancelled" : "Completed");
                        DisplayMessage("Completed sync");
                        if (args.Error != null)
                        {
                            DisplayMessage(string.Format("Sync error: {0} ({1})", args.Error.Message, args.Error.StackTrace));
                        }
                        _syncing = false;    // Set sync complete
                        if (oneTime)
                        {
                            Environment.Exit(0);
                        }
                    });

                    _worker.RunWorkerAsync();
                }
                else
                {
                    DisplayMessage(string.Format("Cannot sync {0}: {1}", syncConfigurationDescription, checkCanSyncMessage));
                    if (interactive)
                    {
                        MessageBox.Show(string.Format("Cannot sync {0}: {1}", syncConfigurationDescription, checkCanSyncMessage), "Cannot Sync");
                    }
                }
            }
            else
            {
                DisplayMessage(string.Format("Sync not required for ", syncConfigurationDescription));
            }
        }
        
        public void DisplayStatus(string status)
        {
            tssFolders.Text = " " + status;
            statusStrip1.Refresh();
            statusStrip1.Update();
        }

        private void DisplayMessage(string message)
        {
            while (lstMessage.Items.Count > 1000)
            {
                lstMessage.Items.RemoveAt(0);
            }
            lstMessage.Items.Add(string.Format("{0} : {1}", DateTime.UtcNow.ToString(), message));
        }

        private void cboSourceFolder_SelectedIndexChanged(object sender, EventArgs e)
        {
            //cboDestinationFolder.SelectedIndex = cboSourceFolder.SelectedIndex;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (_isTaskTray && FormWindowState.Minimized == WindowState)
            {
                Hide();
            }
        }

        private void niNotify_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_isTaskTray)
            {
                Show();
                WindowState = FormWindowState.Normal;
            }
        }

        private void niNotify_MouseMove(object sender, MouseEventArgs e)
        {
            //int test = 100;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If closing form with task tray then prompt user
            if (e.CloseReason == CloseReason.UserClosing && _isTaskTray)
            {
                if (MessageBox.Show("Close application?", "Close", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            niNotify.Dispose();
        }

        private void btnViewLog_Click(object sender, EventArgs e)
        {
            string logFile = GetLogFile();
            if (System.IO.File.Exists(logFile))
            {
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.FileName = logFile;
                System.Diagnostics.Process.Start(startInfo);
            }
            else
            {
                MessageBox.Show("Log file does not exist", "View Log");
            }
        }

        private void tsbSync_Click(object sender, EventArgs e)
        {
            switch (tsbSync.Text)
            {
                case "Sync":
                    var syncConfiguration = (SyncConfiguration)tscbConfiguration.SelectedItem;
                    if (MessageBox.Show(string.Format("Do you want to sync {0}?", syncConfiguration.Description), "Sync", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        RunSync(true, syncConfiguration.Description, false, true, false);
                    }
                    break;
                case "Cancel":
                    _syncManager.Cancelled = true;
                    break;
            }
        }

        private void tsbViewLog_Click(object sender, EventArgs e)
        {
            string logFile = GetLogFile();
            if (System.IO.File.Exists(logFile))
            {
                CFUtilities.IOUtilities.OpenFileWithDefaultApplication(logFile);                
            }
            else
            {
                MessageBox.Show("Log file does not exist", "View Log");
            }
        }

        private void tscbConfiguration_SelectedIndexChanged(object sender, EventArgs e)
        {            
            SelectSyncConfiguration((Guid)tscbConfiguration.ComboBox.SelectedValue);
        }

        private void SelectSyncConfiguration(Guid id)
        {
            SyncConfiguration syncConfiguration = _syncManager.GetSyncConfiguration(id);
            syncConfiguration.SetResolvedFolders(DateTime.UtcNow);
            InitialiseFolderGrid(syncConfiguration);                   
        }

        private void tsbEditConfig_Click(object sender, EventArgs e)
        {
            Guid syncConfigurationId = (Guid)tscbConfiguration.ComboBox.SelectedValue;
            SyncConfiguration syncConfiguration = _syncManager.GetSyncConfiguration(syncConfigurationId);
            SyncConfigurationForm syncConfigurationForm = new SyncConfigurationForm(syncConfiguration);
            if (syncConfigurationForm.ShowDialog() == DialogResult.OK)
            {            
                _syncManager.UpdateConfiguration(syncConfiguration);

                // Refresh
                SelectSyncConfiguration(syncConfigurationId);   
            }
        }

        private void tsbAddConfig_Click(object sender, EventArgs e)
        {
            SyncConfiguration syncConfiguration = new SyncConfiguration()
            {
                ID = Guid.NewGuid(),
                VerificationFile = "MyFile.verify",
                Description = "[New]",
                FoldersOptions = new List<SyncFoldersOptions>()
            };

            SyncConfigurationForm syncConfigurationForm = new SyncConfigurationForm(syncConfiguration);
            if (syncConfigurationForm.ShowDialog() == DialogResult.OK)
            {
                _syncManager.AddConfiguration(syncConfiguration);

                // Refresh list of sync configs, display first
                RefreshSyncConfigurations(syncConfiguration.ID);
               
            }



        }

        private void RefreshSyncConfigurations(Guid defaultSyncConfigurationId)
        {
            List<SyncConfiguration> syncConfigurations = _syncManager.GetSyncConfigurations();

            //defaultSyncConfiguration.SetResolvedFolders(DateTime.UtcNow);
            tscbConfiguration.ComboBox.DisplayMember = nameof(SyncConfiguration.Description);
            tscbConfiguration.ComboBox.ValueMember = nameof(SyncConfiguration.ID);
            tscbConfiguration.ComboBox.DataSource = syncConfigurations;
            tscbConfiguration.ComboBox.SelectedValue = defaultSyncConfigurationId;
        }
    }
}
