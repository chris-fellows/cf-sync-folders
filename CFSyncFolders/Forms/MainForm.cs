using CFSyncFolders.Interfaces;
using CFSyncFolders.Models;
using CFSyncFolders.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace CFSyncFolders.Forms
{
    /// <summary>
    /// Main form
    /// </summary>
    public partial class MainForm : Form
    {
        private Mutex _uiMutex = new Mutex();
        private SyncFoldersService _syncFoldersService = null;
        private readonly IAuditLog _auditLog;
        private readonly IPlaceholderService _placeholderService;
        private readonly ISyncConfigurationService _syncConfigurationService = null;
        private List<string> _autoSyncConfigurationDescriptions = new List<string>();

        private BackgroundWorker _worker = null;
        private DateTime _timeMouseOverNotify = DateTime.Now.AddYears(-1);
        private bool _isTaskTray = false;
        private System.Timers.Timer _timer = null;
        private bool _syncing = false;
        private CancellationTokenSource _syncFoldersCancellationTokenSource = null;
        
        private DateTime _timeLastDeleteLogs = DateTime.MinValue;
        private Dictionary<string, DateTime> _timeLastGridUpdate = new Dictionary<string, DateTime>();
        
        public MainForm(IAuditLog auditLog, IPlaceholderService placeholderService, ISyncConfigurationService syncConfigurationService)                        
        {           
            InitializeComponent();

            Control.CheckForIllegalCrossThreadCalls = false;

            _auditLog = auditLog;
            _placeholderService = placeholderService;
            _syncConfigurationService = syncConfigurationService;

            // Allow single instance only
            if (System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                Environment.Exit(0);
            }

            try
            {
                // Get path to executable
                string currentFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);           
                
                _syncFoldersService = new SyncFoldersService(auditLog, _placeholderService, _syncConfigurationService);              

                // Handle OnDisplayStatus
                _syncFoldersService.OnDisplayStatus += delegate (string status)
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
                _syncFoldersService.OnSyncFolderProgress += delegate (SyncFoldersService.ProgressTypes progressType, SyncFoldersOptions syncFolderOptions, 
                                                    string folder1, FolderStatistics folderStatistics, int folderLevel)
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

                // Get all sync configurations
                var syncConfigurations = _syncConfigurationService.GetAll();                
                
                // Load auto-sync configs
                foreach (string arg in Environment.GetCommandLineArgs())
                {
                    if (arg.ToLower().StartsWith("/configs="))
                    {
                        var elements = arg.Split('=');
                        if (elements[1] == "*")   // All system configs
                        {
                            _autoSyncConfigurationDescriptions.AddRange(syncConfigurations.Select(sc => sc.Description).ToList());
                        }
                        else    // List of sync config descriptions
                        {
                            _autoSyncConfigurationDescriptions.AddRange(elements[1].Split(','));
                            foreach (var syncConfigurationDescription in _autoSyncConfigurationDescriptions)
                            {
                                var syncConfiguration = syncConfigurations.FirstOrDefault(sc =>
                                            sc.Description.Equals(syncConfigurationDescription, StringComparison.InvariantCultureIgnoreCase));
                                if (syncConfiguration == null)
                                {
                                    throw new Exception(string.Format("Sync configuration {0} does not exist", syncConfigurationDescription));
                                }
                            }
                        }
                    }
                }

                // Initialise folder grid                          
                string defaultSyncDescription = _autoSyncConfigurationDescriptions.Count == 0 ? Environment.MachineName : _autoSyncConfigurationDescriptions.First();
                var defaultSyncConfiguration = _syncConfigurationService.GetByFilter((config) => 
                                config.Description.Equals(defaultSyncDescription, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                if (defaultSyncConfiguration == null)   // Default to first sync config
                {
                    defaultSyncConfiguration = syncConfigurations.FirstOrDefault();
                }
               
                RefreshSyncConfigurations(defaultSyncConfiguration.ID);
               
                // Check run mode
                if (Environment.GetCommandLineArgs().Contains("/Tray"))    // Run continuously in system tray
                {
                    RunInTray();                  
                }
                else if (Environment.GetCommandLineArgs().Contains("/Silent"))    // Run and shut down (Called from Windows scheduler)
                {
                    RunSilent();
                }
            }
            catch(System.Exception exception)
            {
                MessageBox.Show(string.Format("Error starting application: {0}: {1}", exception.Message, exception.StackTrace), "Error");
                throw;
            }
        }

        /// <summary>
        /// Runs silently, process terminates after sync complete
        /// </summary>
        private void RunSilent()
        {
            tsbAddConfig.Visible = false;
            tsbEditConfig.Visible = false;

            foreach (var syncConfigurationDescription in _autoSyncConfigurationDescriptions)
            {
                bool isLast = syncConfigurationDescription == _autoSyncConfigurationDescriptions.Last();
                RunFolderSyncAsync(false, syncConfigurationDescription, true, false, isLast);

                // If not last then wait for completion
                while (!isLast && _syncing)
                {
                    System.Threading.Thread.Sleep(200);
                    Application.DoEvents();
                }
            }
        }

        /// <summary>
        /// Runs in system tray
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void RunInTray()
        {
            if (!_autoSyncConfigurationDescriptions.Any())
            {
                throw new Exception("No sync configurations specified");
            }

            // Determine if auto sync is required
            bool isAutoSync = Environment.GetCommandLineArgs().Contains("/AutoSync");

            _isTaskTray = true;
            niNotify.Icon = this.Icon;      // SystemIcons.Application doesn't work
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

        /// <summary>
        /// Handles notification of folder sync progress so that we can updated the UI. Event indicates either starting folder,
        /// completed folder or periodic update while processing folder.
        /// </summary>
        /// <param name="progressType"></param>
        /// <param name="syncFolderOptions"></param>
        /// <param name="folder1"></param>
        /// <param name="folderStatistics"></param>
        /// <param name="folderLevel"></param>
        private void EventOnSyncFolderProgress(SyncFoldersService.ProgressTypes progressType, SyncFoldersOptions syncFolderOptions, 
                                                string folder1, FolderStatistics folderStatistics, int folderLevel)
        {
            DateTime currentTime = DateTime.UtcNow;

            // Update main status
            //TimeSpan timeSinceLastUpdateStatus = currentTime - _timeLastUpdateStatus;
            if (folderLevel == 1)
            {
                tssFolders.Text = string.Format("{0} {1}", (progressType == SyncFoldersService.ProgressTypes.CompletedFolder ? "Synchronised" : "Synchronising"), syncFolderOptions.Folder1Resolved);
                statusStrip1.Refresh();
                switch (progressType)
                {
                    case SyncFoldersService.ProgressTypes.StartingFolder:
                        DisplayMessage(string.Format("Sychronising {0}", syncFolderOptions.Folder1Resolved));
                        break;
                    case SyncFoldersService.ProgressTypes.CompletedFolder:
                        DisplayMessage(string.Format("Synchronised {0}", syncFolderOptions.Folder1Resolved));
                        break;
                }
                //_timeLastUpdateStatus = currentTime;
            }         
            
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
                            if (folderLevel == 1 && progressType == SyncFoldersService.ProgressTypes.CompletedFolder)   // Completed
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
            catch { }       // Ignore error, nothing that we can do
            finally
            {
                if (waited)
                {
                    _uiMutex.ReleaseMutex();
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
                    foreach (var syncConfigurationDescription in _autoSyncConfigurationDescriptions)
                    {
                        // Run sync
                        RunFolderSyncAsync(false, syncConfigurationDescription, true, false, false);

                        if (_syncing)   // Only one sync at a time
                        {
                            break;
                        }
                    }

                    // Clear logs
                    try
                    {
                        var now = DateTime.Now;
                        if (_timeLastDeleteLogs.AddDays(1) < now)
                        {
                            DisplayMessage("Clearing old logs");
                            _timeLastDeleteLogs = now;
                            _auditLog.DeleteBefore(now.Subtract(TimeSpan.FromDays(60)));
                        }
                    }
                    catch (Exception exception)
                    {
                        DisplayMessage($"Error clearing old logs: {exception.Message}");
                    }
                }
            }
            finally
            {
                _timer.Interval = 30000;   // Occasional check
                _timer.Enabled = true;

                DisplayMessage("Completed periodic checks");
            }
        }

        //void _syncManager_OnDisplayStatus(string status)
        //{
        //    DisplayStatus(status);           
        //}       

        /// <summary>
        /// Starts the folder sync running asynchronously.
        /// </summary>
        /// <param name="interactive"></param>
        /// <param name="syncConfigurationDescription"></param>
        /// <param name="ignoreLastStartTime"></param>
        /// <param name="oneTime"></param>
        private void RunFolderSyncAsync(bool interactive, string syncConfigurationDescription, bool setInUI, bool ignoreLastStartTime, bool oneTime)
        {
            this.Visible = interactive;
            _syncFoldersCancellationTokenSource = null;

            DisplayMessage(string.Format("Checking if sync needed for {0}", syncConfigurationDescription));

            // Get file repository service
            var fileRepositoryService = new FileRepositoryService();
            var fileRepository1 = fileRepositoryService.GetFileRepository(System.Configuration.ConfigurationSettings.AppSettings.Get("Folder1.FileRepositoryClass"));
            var fileRepository2 = fileRepositoryService.GetFileRepository(System.Configuration.ConfigurationSettings.AppSettings.Get("Folder2.FileRepositoryClass"));

            // Load SyncConfigurarion                                   
            var syncConfiguration = _syncConfigurationService.GetByFilter((config) => 
                                config.Description.Equals(syncConfigurationDescription, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            // Check if there's any sync'ing to do
            if (syncConfiguration.GetFoldersThatNeedSync(ignoreLastStartTime).Any())
            {
                // Check if we can sync. Removable drive may not be plugged in, other removable drive may be plugged
                // in but it doesn't have the verification file.
                var checkCanSyncMessage = _syncFoldersService.CheckCanSyncFolders(syncConfiguration, ignoreLastStartTime,
                                    fileRepository1, fileRepository2);                                    
                                
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
                    _syncFoldersCancellationTokenSource = new CancellationTokenSource();

                    // Define worker action
                    _worker.DoWork += new DoWorkEventHandler(
                    delegate (object o, DoWorkEventArgs args)
                    {
                        BackgroundWorker worker = o as BackgroundWorker;
                        tsbSync.Text = "Cancel";
                        tsbSync.Enabled = interactive;
                        niNotify.Text = "Sync Folders - Busy";
                        tscbConfiguration.Enabled = false;    // Prevent change
                        _syncFoldersService.SyncFolders(syncConfiguration, ignoreLastStartTime,
                                        fileRepository1, fileRepository2,
                                        _syncFoldersCancellationTokenSource.Token);
                                        
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
                        DisplayStatus(_syncFoldersCancellationTokenSource.IsCancellationRequested ? "Cancelled" : "Completed");
                        DisplayMessage("Completed sync");
                        if (args.Error != null)
                        {
                            DisplayMessage(string.Format("Sync error: {0} ({1})", args.Error.Message, args.Error.StackTrace));
                        }
                        _syncing = false;    // Set sync complete
                        _syncFoldersCancellationTokenSource = null;
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
                DisplayMessage(string.Format("Sync not required for {0}", syncConfigurationDescription));
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
            while (lstMessage.Items.Count > 500)
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
            //string logFile = GetLogFile();
            //if (System.IO.File.Exists(logFile))
            //{
            //    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            //    startInfo.FileName = logFile;
            //    System.Diagnostics.Process.Start(startInfo);
            //}
            //else
            //{
            //    MessageBox.Show("Log file does not exist", "View Log");
            //}
        }

        private void tsbSync_Click(object sender, EventArgs e)
        {
            switch (tsbSync.Text)
            {
                case "Sync":
                    var syncConfiguration = (SyncConfiguration)tscbConfiguration.SelectedItem;
                    if (MessageBox.Show(string.Format("Do you want to sync {0}?", syncConfiguration.Description), "Sync", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        RunFolderSyncAsync(true, syncConfiguration.Description, false, true, false);
                    }
                    break;
                case "Cancel":
                    DisplayMessage("Cancelling folder sync");
                    _syncFoldersCancellationTokenSource.Cancel();                    
                    break;
            }
        }

        private void tsbViewLog_Click(object sender, EventArgs e)
        {
            //string logFile = GetLogFile();
            //if (System.IO.File.Exists(logFile))
            //{
            //    CFUtilities.IOUtilities.OpenFileWithDefaultApplication(logFile);                
            //}
            //else
            //{
            //    MessageBox.Show("Log file does not exist", "View Log");
            //}
        }

        private void tscbConfiguration_SelectedIndexChanged(object sender, EventArgs e)
        {            
            SelectSyncConfiguration((Guid)tscbConfiguration.ComboBox.SelectedValue);
        }

        private void SelectSyncConfiguration(Guid id)
        {
            SyncConfiguration syncConfiguration = _syncConfigurationService.GetByID(id);
            syncConfiguration.SetResolvedFolders(DateTime.UtcNow, _placeholderService);
            InitialiseFolderGrid(syncConfiguration);                   
        }

        private void tsbEditConfig_Click(object sender, EventArgs e)
        {
            Guid syncConfigurationId = (Guid)tscbConfiguration.ComboBox.SelectedValue;
            SyncConfiguration syncConfiguration = _syncConfigurationService.GetByID(syncConfigurationId);
            SyncConfigurationForm syncConfigurationForm = new SyncConfigurationForm(_placeholderService, syncConfiguration);
            if (syncConfigurationForm.ShowDialog() == DialogResult.OK)
            {
                _syncConfigurationService.Update(syncConfiguration);

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

            SyncConfigurationForm syncConfigurationForm = new SyncConfigurationForm(_placeholderService, syncConfiguration);
            if (syncConfigurationForm.ShowDialog() == DialogResult.OK)
            {
                _syncConfigurationService.Insert(syncConfiguration);

                // Refresh list of sync configs, display first
                RefreshSyncConfigurations(syncConfiguration.ID);               
            }
        }

        private void RefreshSyncConfigurations(Guid defaultSyncConfigurationId)
        {
            List<SyncConfiguration> syncConfigurations = _syncConfigurationService.GetAll();

            //defaultSyncConfiguration.SetResolvedFolders(DateTime.UtcNow);
            tscbConfiguration.ComboBox.DisplayMember = nameof(SyncConfiguration.Description);
            tscbConfiguration.ComboBox.ValueMember = nameof(SyncConfiguration.ID);
            tscbConfiguration.ComboBox.DataSource = syncConfigurations;
            tscbConfiguration.ComboBox.SelectedValue = defaultSyncConfigurationId;
        }
    }
}
