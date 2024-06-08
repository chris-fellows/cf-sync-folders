using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using CFSyncFolders.Interfaces;
using CFSyncFolders.Models;
using CFUtilities.Interfaces;
using System.Threading;
using CFUtilities.Logging;
using CFUtilities.Models;

namespace CFSyncFolders.Services
{
    /// <summary>
    /// Performs one-way sync'ing of a source folder to a destination folder. The destination folder is made to look identical
    /// to the source folder.
    /// </summary>
    internal class SyncFoldersService
    {
        public enum ProgressTypes : byte
        {
            StartingFolder = 0,
            CompletedFolder = 1,
            Periodic = 2
        }

        //private bool _cancelled = false;    
        private readonly ILogger _auditLog;
        private readonly IPlaceholderService _placeholderService;
        private readonly ISyncConfigurationService _syncConfigurationService;        

        private DateTime _lastProgressEvent = DateTime.UtcNow;
        private DateTime _lastStatusEvent = DateTime.UtcNow;
        private DateTime _lastPeriodicProgressEvent = DateTime.UtcNow;
        private DateTime _lastPause = DateTime.UtcNow;

        public delegate void DisplayStatus(string status);
        public event DisplayStatus OnDisplayStatus;     
        
        public delegate void SyncFolderProgress(ProgressTypes progressType, SyncFoldersOptions syncFolderOptions, string currentFolder,
                                FolderStatistics folderStatistics, int folderLevel);
        public event SyncFolderProgress OnSyncFolderProgress;
        
        public SyncFoldersService(ILogger auditLog, IPlaceholderService placeholderService,
                                ISyncConfigurationService syncConfigurationService)
        {                        
            _auditLog = auditLog;
            _placeholderService = placeholderService;
            _syncConfigurationService = syncConfigurationService;            
        }      

        /// <summary>
        /// Periodically pause if necessary, avoid high CPU
        /// </summary>
        private void PauseIfRequired(bool force)
        {
            var now = DateTime.UtcNow;
            if (force || _lastPause.AddMilliseconds(200) <= now)
            {
                System.Threading.Thread.Sleep(1);
                _lastPause = now;
            }
        }

        private static LogEntry CreateLogEntry(string action, string item1, string item1Data, string item2, string item2Data, Exception exeption)
        {
            return new LogEntry()
            {
                Values = new Dictionary<string, object>()
                {
                    { "Action", action },
                    { "Machine", Environment.MachineName },
                    { "Item1", item1 },
                    { "Item1Data", item1Data },
                    { "Item2", item2 },
                    { "Item2Data", item2Data },
                    { "Exception", exeption == null ? "" : exeption.Message }
                }
            };
        }
        
        //public bool Cancelled
        //{
        //    get { return _cancelled; }
        //    set { _cancelled = value; }
        //}
      
        private void DoDisplayStatus(string status, bool force)
        {
            if (OnDisplayStatus != null)
            {
                var frequency = TimeSpan.FromMilliseconds(500);
                if (force || _lastStatusEvent.Add(frequency) <= DateTime.UtcNow)
                {
                    _lastStatusEvent = DateTime.UtcNow;

                    try
                    {
                        OnDisplayStatus(status);
                    }
                    catch { };  // Ignore
                }
            }
        }
    
        /// <summary>
        /// Returns the string with placeholders replaced
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ReplacePlaceholdersInFolder(string input, DateTime date, string verificationFile, IPlaceholderService placeholderService)
        {
            var parameters = new Dictionary<string, object>()
            {
                { "date", date }
            };
            var output = placeholderService.GetWithPlaceholdersReplaced(input, parameters);         

            // Replace verification file drive letter
            const string placeholder1 = "{verification_file_drive}";
            string verificationFileDriveLetter = verificationFile.Substring(0, 2);
            if (!String.IsNullOrEmpty(verificationFile) && input.Contains(placeholder1))
            {
                output = output.Replace(placeholder1, verificationFileDriveLetter);
            }

            /* Not needed. Better to use verification file drive letter that label.
            const string prefix = "{drive_letter_by_label:";
            int startIndex = output.IndexOf(prefix);
            if (startIndex > -1)
            {
                int endIndex = output.IndexOf("}", startIndex + 1);
                string driveVolumeLabel = output.Substring(startIndex + prefix.Length, endIndex - (startIndex + prefix.Length));
                string replace = "";

                DriveInfo[] driveInfoList = System.IO.DriveInfo.GetDrives();
                foreach (DriveInfo driveInfo in driveInfoList)
                {
                    if (driveInfo.IsReady)
                    {
                        if (!String.IsNullOrEmpty(driveVolumeLabel) && driveInfo.VolumeLabel.Equals(driveVolumeLabel, StringComparison.InvariantCultureIgnoreCase))
                        {
                            replace = driveInfo.RootDirectory.Name;                         
                        }
                    }
                }
                if (String.IsNullOrEmpty(replace))
                {
                    throw new ArgumentException(string.Format("Failed to find drive letter for volume label {0}", driveVolumeLabel));
                }
                output = output.Replace(prefix + driveVolumeLabel + "}", replace);
            }
            */
            return output;
        }        

        /// <summary>
        /// Returns drive where verification file is. Throws exception if exists on multiple drives as we may
        /// sync to wrong folder.
        /// </summary>
        /// <param name="verificationFile"></param>
        /// <returns></returns>
        public static string GetVerificationFileDrive(string verificationFile)
        {
            if (!String.IsNullOrEmpty(verificationFile) && !verificationFile.StartsWith("//"))
            {
                var drivesFound = new List<string>();

                var driveInfoList = System.IO.DriveInfo.GetDrives();
                foreach (DriveInfo driveInfo in driveInfoList)
                {
                    // Determine if we check this drive
                    bool isCheckDrive = true;
                    string file = string.Format("{0}{1}", driveInfo.Name, verificationFile);
                    if (verificationFile.Substring(1, 1) == ":")  // Verification drive letter
                    {
                        file = verificationFile;
                        if (!driveInfo.Name.StartsWith(verificationFile.Substring(0, 1)))
                        {
                            isCheckDrive = false;
                        }
                    }

                    if (isCheckDrive && driveInfo.IsReady)
                    {                        
                        if (File.Exists(file))
                        {
                            drivesFound.Add(driveInfo.Name.Substring(0, 2));
                        }
                    }
                }            

                if (drivesFound.Count == 1)
                {
                    return drivesFound.First();
                }
                else if (drivesFound.Count > 1)   // May sync to wrong folder if verification file exists on multiple drives
                {
                    throw new Exception(string.Format("Verification file {0} must only exist on one drive", verificationFile));
                }               
            }
            return null;
        }

        ///// <summary>
        ///// Creates SyncConfiguration in new format (XML file) from old format (Web.config settings)
        ///// </summary>
        ///// <returns></returns>
        //private SyncConfiguration CreateAndSave()
        //{
        //    SyncConfiguration syncConfiguration = new SyncConfiguration()
        //    {
        //        ID = Guid.NewGuid(),                
        //        Description = "Default",
        //        FoldersOptions = LoadSyncFoldersOptionsList()
        //    };
        //    _syncConfigurationRepository.Insert<SyncConfiguration>(syncConfiguration.ID.ToString(), syncConfiguration);

        //    return syncConfiguration;
        //}       

        //private List<SyncFoldersOptions> LoadSyncFoldersOptionsList()
        //{
        //    var optionsList = new List<SyncFoldersOptions>();
        //    int count = 0;
        //    while (true)
        //    {
        //        count++;
        //        string key1 = string.Format("Folders.{0}.Folder1", count);
        //        string key2 = string.Format("Folders.{0}.Folder2", count);
        //        if (System.Configuration.ConfigurationSettings.AppSettings.Get(key1) != null)
        //        {
        //            // Load folder pair, replace placeholders
        //            SyncFoldersOptions options = new SyncFoldersOptions()
        //            {
        //                ID = Guid.NewGuid(),
        //                //Folder1 = SyncManager.GetStringWithPlaceholdersReplaced(System.Configuration.ConfigurationSettings.AppSettings.Get(key1)),
        //                //Folder2 = SyncManager.GetStringWithPlaceholdersReplaced(System.Configuration.ConfigurationSettings.AppSettings.Get(key2)),
        //                Folder1 = System.Configuration.ConfigurationSettings.AppSettings.Get(key1),
        //                Folder2 = System.Configuration.ConfigurationSettings.AppSettings.Get(key2),
        //                Enabled = true,
        //                IncludeFileExtensionList = new List<string>(),
        //                ExcludeFileExtensionList = new List<string>(),
        //                KeepFileProperties = false,
        //                KeepDeletedItems = false,
        //                FrequencySeconds = 86400    // Daily
        //            };
        //            optionsList.Add(options);
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    return optionsList;
        //}

        /// <summary>
        /// Whether the folders can be sync'd for the sync configuration.
        /// 
        /// Possible reasons for rejection:
        /// - Sync config is only for specific machines.
        /// - Source or destination folder is a removable drive that isn't currently connected.       
        /// </summary>
        /// <param name="syncConfiguration"></param>
        /// <param name="ignoreLastStartTime"></param>
        /// <param name="fileRepository1"></param>
        /// <param name="fileRepository2"></param>
        /// <returns></returns>
        public string CheckCanSyncFolders(SyncConfiguration syncConfiguration, bool ignoreLastStartTime,
                                          IFileRepository fileRepository1, IFileRepository fileRepository2)
        {            
            // Replace placeholders in source & destination folders
            syncConfiguration.SetResolvedFolders(DateTime.UtcNow, _placeholderService);

            // Check if machine specific config
            if (!String.IsNullOrEmpty(syncConfiguration.Machine) && 
                !syncConfiguration.Machine.Equals(Environment.MachineName, StringComparison.InvariantCultureIgnoreCase))
            {
                return $"Sync configuration is only valid for machine {syncConfiguration.Machine}";
            }

            // Check if we can sync all of these folders
            foreach (SyncFoldersOptions syncFoldersOptions in syncConfiguration.FoldersOptions)
            {
                // If folder 1 contains placeholder to determine drive letter for verification file then it may not
                // be available.
                if (String.IsNullOrEmpty(syncFoldersOptions.Folder1Resolved))
                {
                    return string.Format("Source folder {0} cannot be found for verification file {1}. It " +
                                    "may be that a removable drive is not currently available",
                                    syncFoldersOptions.Folder1, syncConfiguration.VerificationFile);
                }

                // If folder 2 contains placeholder to determine drive letter for verification file then it may not
                // be available.
                if (String.IsNullOrEmpty(syncFoldersOptions.Folder2Resolved))
                {
                    return string.Format("Destination folder {0} cannot be found for verification file {1}. It " +
                                    "may be that a removable drive is not currently available",
                                    syncFoldersOptions.Folder2, syncConfiguration.VerificationFile);
                }

                // Restrict destination drives, prevents accidental trashing of files if config set up incorrectly
                if (!fileRepository2.IsFolderWritable(syncFoldersOptions.Folder2Resolved))
                {
                    return "This repository cannot be used because it may not be currently available";
                }
            }

            return null;    // Can check
        }

        /// <summary>
        /// Syncs folder 2 (destination) to match folder 1 (source). Files that exist in folder 2 are only copied if 
        /// different to folder 1.
        /// </summary>
        /// <param name="folder1">Source folder</param>
        /// <param name="folder2">Destination folder</param>
        public void SyncFolders(SyncConfiguration syncConfiguration, bool ignoreLastStartTime,
                                IFileRepository fileRepository1, IFileRepository fileRepository2,
                                CancellationToken cancellationToken)
        {                        
            // Replace placeholders in source & destination folders
            syncConfiguration.SetResolvedFolders(DateTime.UtcNow, _placeholderService);

            // Check if we can sync
            var message = CheckCanSyncFolders(syncConfiguration, ignoreLastStartTime, fileRepository1, fileRepository2);
            if (!String.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }                

            //const int maxThreads = 5;
            //Semaphore semaphore = new Semaphore(maxThreads, maxThreads);
            //Mutex mutex = new Mutex();
            //int activeThreads = 0;
            //List<Task> tasks = new List<Task>();

            // Sync folders            
            foreach (SyncFoldersOptions syncFoldersOptions in syncConfiguration.FoldersOptions.Where(fo => fo.Enabled))
            {       
                if (syncFoldersOptions.IsSyncOverdue || ignoreLastStartTime)
                {
                    if (fileRepository2.IsFolderAvailable(syncFoldersOptions.Folder2Resolved))        // Do nothing if destination not available
                    {
                        if (fileRepository1.IsFolderExists(syncFoldersOptions.Folder1Resolved))
                        {
                            // Set started time
                            syncFoldersOptions.TimeLastStarted = DateTime.UtcNow;
                            _syncConfigurationService.Update(syncConfiguration);

                            // Multi-threading disabled, causes issues
                            //var task = Task.Factory.StartNew<int>(() =>
                            //{
                            // Wait for free thread
                            //    while (activeThreads >= maxThreads)
                            //    {
                            //        System.Threading.Thread.Sleep(100);
                            //    }
                            //    mutex.WaitOne();
                            //    activeThreads++;
                            //    mutex.ReleaseMutex();

                            FolderStatistics folderStatistics = new FolderStatistics()
                            {
                                Folder = syncFoldersOptions.Folder1Resolved
                            };
                            int folderLevel = 1;
                            _auditLog.Write(CreateLogEntry("SYNC_START", syncFoldersOptions.Folder1Resolved, "", syncFoldersOptions.Folder2Resolved, "", null));                          
                            SyncFoldersInternal(syncFoldersOptions, syncFoldersOptions,
                                                fileRepository1, fileRepository2,
                                                folderStatistics,
                                                folderLevel,
                                                cancellationToken);
                            _auditLog.Write(CreateLogEntry("SYNC_END", syncFoldersOptions.Folder1Resolved, "", syncFoldersOptions.Folder2Resolved, "", null));                         
                            //    mutex.WaitOne();
                            //    activeThreads--;
                            ////    mutex.ReleaseMutex();
                            //    return 0;
                            //});
                            //tasks.Add(task);

                            // Set completed time
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                syncFoldersOptions.TimeLastCompleted = DateTime.UtcNow;
                                _syncConfigurationService.Update(syncConfiguration);
                            }
                        }
                    }

                    // Save sync status
                    _syncConfigurationService.Update(syncConfiguration);
                }
            
                /*
                if (fileRepository2.IsFolderAvailable(syncFoldersOptions.Folder2))        // Do nothing if destination not available
                {
                    _folderLevel = 0;
                    _log.Write("SYNC_START", syncFoldersOptions.Folder1, "", syncFoldersOptions.Folder2, "", null);                  
                    SyncFoldersInternal(syncFoldersOptions, 
                                        fileRepository1, fileRepository2);
                    _log.Write("SYNC_END", syncFoldersOptions.Folder1, "", syncFoldersOptions.Folder2, "", null);

                }
                */

                //string driveLetter = GetDriveLetter(syncFoldersOptions.Folder2);
                //DriveInfo driveInfo = new DriveInfo(driveLetter);
                //if (driveInfo.IsReady)      // Do nothing if drive not plugged in
                //{
                //    _folderLevel = 0;
                //    WriteLog("SYNC_START", syncFoldersOptions.Folder1, syncFoldersOptions.Folder2, "");
                //    SyncFoldersInternal(syncFoldersOptions.Folder1, fileRepository1,
                //                        syncFoldersOptions.Folder2, fileRepository2,
                //                        syncFoldersOptions.IncludeFileExtensionList, syncFoldersOptions.KeepFileProperties, syncFoldersOptions.KeepDeletedItems);
                //    WriteLog("SYNC_END", syncFoldersOptions.Folder1, syncFoldersOptions.Folder2, "");
                //}
            }

            // Wait for all tasks to complete
            //Task.WaitAll(tasks.ToArray());

            // Bit of a hack to flush the log because we cache items
            //_auditLog.LogAction("", "", "", "", "", null);
        }
     
        //private static bool IsCanProcessFileExtension(FileInfo fileInfo, string[] includeFileExtensionList,
        //                                string[] excludeFileExtensionList)
        //{            
        //    string extension = fileInfo.Extension;

        //    // Check for specific included
        //    if (includeFileExtensionList != null && includeFileExtensionList.Any())   // Include only specific extensions
        //    {                
        //        foreach (string currentExtension in includeFileExtensionList)
        //        {
        //            if (currentExtension.Equals(extension, StringComparison.InvariantCultureIgnoreCase))   //Included
        //            {
        //                return true;
        //            }    
        //        }
        //        return false;
        //    }

        //    if (excludeFileExtensionList != null && excludeFileExtensionList.Any())
        //    {
        //        foreach (string currentExtension in excludeFileExtensionList)
        //        {
        //            if (currentExtension.Equals(extension, StringComparison.InvariantCultureIgnoreCase))    // Excluded
        //            {
        //                return false;
        //            }
        //        }
        //    }
        //    return true;
        //}

        private static bool IsCanProcessFileExtension(FileDetails fileDetails, string[] includeFileExtensionList,
                                        string[] excludeFileExtensionList)
        {
            string extension = fileDetails.Extension;

            // Check for specific included
            if (includeFileExtensionList != null && includeFileExtensionList.Any())   // Include only specific extensions
            {
                foreach (string currentExtension in includeFileExtensionList)
                {
                    if (currentExtension.Equals(extension, StringComparison.InvariantCultureIgnoreCase)) // Included
                    {
                        return true;
                    }
                }
                return false;
            }

            // Check for specific exclude
            if (excludeFileExtensionList != null && excludeFileExtensionList.Any())
            {
                foreach (string currentExtension in excludeFileExtensionList)
                {
                    if (currentExtension.Equals(extension, StringComparison.InvariantCultureIgnoreCase))    // Excluded
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static FileDetails GetFileDetails(List<FileDetails> fileDetailsList, string name)
        {
            return fileDetailsList.Find(item => (item.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)));
        }

        private static FileInfo GetFileInfo(FileInfo[] fileInfos, string name)
        {
            foreach (FileInfo fileInfo in fileInfos)
            {
                if (fileInfo.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return fileInfo;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns whether the folders are the same.
        /// 
        /// This is method is just used for an optimisation so that we don't sync folders if they are the same.
        /// </summary>
        /// <param name="folder1"></param>
        /// <param name="fileRepository1"></param>
        /// <param name="folder2"></param>
        /// <param name="fileRepository2"></param>
        /// <returns></returns>
        private static bool IsFoldersTheSame(string folder1, IFileRepository fileRepository1,
                                        string folder2, IFileRepository fileRepository2)
        {
            if (!fileRepository2.IsFolderExists(folder2))   // Destination folder doesn't exist
            {
                return false;
            }
            if (fileRepository1.IsFolderExists(folder1) && fileRepository2.IsFolderExists(folder2))
            {
                // Compare folder modified timestamps
                FolderDetails folderDetails1 = fileRepository1.GetFolderDetails(folder1);
                FolderDetails folderDetails2 = fileRepository2.GetFolderDetails(folder2);

                if (folderDetails1.TimeModified == folderDetails2.TimeModified)
                {
                    return true;
                }
            }
            return false;
        }

        public static void WriteDebug(string message)
        {
            return;

            //using (StreamWriter writer = new StreamWriter(@"C:\Data\Applications\CFSyncFolders\Debug.txt", true))
            //{
            //    writer.WriteLine(string.Format("{0} {1}", DateTime.UtcNow.ToString(), message));
            //    writer.Flush();
            //    writer.Close();
            //}
        }

        /// <summary>
        /// Syncs folder1 to folder2, only folder2 is modified
        /// </summary>
        /// <param name="folder1">Source folder</param>
        /// <param name="folder2">Destination folder (Folder that is modified)</param>
        /// <param name="includeFileExtensionList">Specific extensions to include</param>
        /// <param name="keepFileProperties">Whether to retain file properties (Timestamps etc)</param>
        /// <param name="keepDeletedItems">Whether to keep items in folder2 when they're deleted from folder1</param>
        private void SyncFoldersInternal(SyncFoldersOptions rootSyncFoldersOptions,
                                         SyncFoldersOptions syncFoldersOptions,
                                         IFileRepository fileRepository1,
                                         IFileRepository fileRepository2,
                                         FolderStatistics folderStatistics,
                                         int folderLevel,
                                         CancellationToken cancellationToken)
        {            
            if (cancellationToken.IsCancellationRequested)
            {
                WriteDebug("Cancelled");
                return;
            }

            WriteDebug(string.Format("Synchronising {0}", syncFoldersOptions.Folder1Resolved));
    
            DoSyncFolderEvent(ProgressTypes.StartingFolder, rootSyncFoldersOptions, syncFoldersOptions.Folder1Resolved, folderStatistics, folderLevel, (folderLevel == 1));            
            
            //string[] includeFileExtensionList = syncFoldersOptions.IncludeFileExtensionList;
            //bool keepFileProperties = syncFoldersOptions.KeepFileProperties;
            //bool keepDeletedItems = syncFoldersOptions.KeepDeletedItems;

            // Check input parameters
            if (String.IsNullOrEmpty(syncFoldersOptions.Folder1Resolved) || String.IsNullOrEmpty(syncFoldersOptions.Folder2Resolved))
            {
                throw new ArgumentException("Invalid source or destination folders");
            }
            else if (syncFoldersOptions.Folder1Resolved.Equals(syncFoldersOptions.Folder2Resolved, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException("Source and destination folders are the same");
            }          
            else if (!fileRepository1.IsFolderExists(syncFoldersOptions.Folder1Resolved))
            {
                throw new ArgumentException("Source folder does not exist");
            }

            //folderLevel++;
            
            int itemsProcessedCount = 0;
            bool isDisplayStatus = (folderLevel < 8);  // Limit status updates for lower level folders
            if (isDisplayStatus)      
            {
                DoDisplayStatus(string.Format("Checking {0}", syncFoldersOptions.Folder1Resolved), false);
            }

            // Create folder 2 if not exists
            if (!fileRepository2.IsFolderExists(syncFoldersOptions.Folder2Resolved))
            {  
                fileRepository2.CreateFolder(syncFoldersOptions.Folder2Resolved);
                _auditLog.Write(CreateLogEntry("FOLDER_NEW", syncFoldersOptions.Folder1Resolved, "", syncFoldersOptions.Folder2Resolved, "", null));
            }

            // Check if we need to sync these folders
            if (!cancellationToken.IsCancellationRequested && 
                    !IsFoldersTheSame(syncFoldersOptions.Folder1Resolved, fileRepository1, syncFoldersOptions.Folder2Resolved, fileRepository2))
            {
                // Get file list
                List<FileDetails> fileDetails1List = fileRepository1.GetFileDetailsList(syncFoldersOptions.Folder1Resolved);
                List<FileDetails> fileDetails2List = fileRepository2.GetFileDetailsList(syncFoldersOptions.Folder2Resolved);

                // Copy changes/new files from folder 1
                if (!cancellationToken.IsCancellationRequested)
                {                    
                    foreach (FileDetails fileDetails1 in fileDetails1List)
                    {
                        PauseIfRequired(false);

                        WriteDebug(string.Format("Checking (1) {0}", fileDetails1.Name));

                        string file1 = fileRepository1.PathCombine(syncFoldersOptions.Folder1Resolved, fileDetails1.Name);
                        if (IsCanProcessFileExtension(fileDetails1, 
                                                            syncFoldersOptions.IncludeFileExtensionList.ToArray(),
                                                            syncFoldersOptions.ExcludeFileExtensionList.ToArray()))
                        {
                            folderStatistics.CountFilesChecked++;
                            FileDetails fileDetails2 = GetFileDetails(fileDetails2List, fileDetails1.Name);

                            string file2 = fileRepository2.PathCombine(syncFoldersOptions.Folder2Resolved, fileDetails1.Name);
                            if (fileDetails2 != null)
                            {
                                // File exists in folder 2, copy if different         
                                if (!IsFilesTheSame(fileDetails1, fileDetails2))
                                {
                                    try
                                    {
                                        CopyFile(fileRepository1, file1, fileDetails1, fileRepository2, file2, fileDetails2, syncFoldersOptions.KeepFileProperties);
                                        folderStatistics.CountFilesUpdated++;
                                        _auditLog.Write(CreateLogEntry("FILE_UPDATED", file1, GetFileDetailsForLog(fileDetails1), file2, GetFileDetailsForLog(fileDetails2), null));
                                    }
                                    catch (System.Exception exception)
                                    {
                                        _auditLog.Write(CreateLogEntry("FILE_UPDATED_ERROR", file1, GetFileDetailsForLog(fileDetails1), file2, GetFileDetailsForLog(fileDetails2), exception));
                                        folderStatistics.CountFileErrors++;
                                    }
                                }
                                PauseIfRequired(false);
                            }
                            else
                            {
                                // File doesn't exist in folder 2, copy new file                            
                                try
                                {
                                    CopyFile(fileRepository1, file1, fileDetails1, fileRepository2, file2, fileDetails2, syncFoldersOptions.KeepFileProperties);
                                    folderStatistics.CountFilesNew++;
                                    _auditLog.Write(CreateLogEntry("FILE_NEW", file1, GetFileDetailsForLog(fileDetails1), file2, "", null));
                                }
                                catch (System.Exception exception)
                                {
                                    _auditLog.Write(CreateLogEntry("FILE_NEW_ERROR", file1, GetFileDetailsForLog(fileDetails1), file2, "", exception));
                                    folderStatistics.CountFileErrors++;
                                }
                                PauseIfRequired(false);
                            }
                        }

                        itemsProcessedCount++;
                        PauseIfRequired(false);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        // Periodic UI update
                        if (_lastPeriodicProgressEvent.AddSeconds(5) <= DateTime.UtcNow)
                        {
                            _lastPeriodicProgressEvent = DateTime.UtcNow;
                            DoSyncFolderEvent(ProgressTypes.Periodic, rootSyncFoldersOptions, syncFoldersOptions.Folder1Resolved, folderStatistics, folderLevel, true);
                        }
                    }
                }

                PauseIfRequired(false);

                // Delete folder 2 files not in folder 1
                if (!cancellationToken.IsCancellationRequested && !syncFoldersOptions.KeepDeletedItems)
                {
                    foreach (FileDetails fileDetails2 in fileDetails2List)
                    {
                        WriteDebug(string.Format("Checking (2) {0}", fileDetails2.Name));

                        PauseIfRequired(false);
                        if (IsCanProcessFileExtension(fileDetails2, 
                                    syncFoldersOptions.IncludeFileExtensionList.ToArray(),
                                    syncFoldersOptions.ExcludeFileExtensionList.ToArray()))
                        {

                            FileDetails fileDetails1 = GetFileDetails(fileDetails1List, fileDetails2.Name);
                            if (fileDetails1 == null)
                            {
                                // File no longer in folder 1, delete it from folder 2                                           
                                string file2 = fileRepository2.PathCombine(syncFoldersOptions.Folder2Resolved, fileDetails2.Name);
                                try
                                {
                                    fileRepository2.DeleteFile(file2);
                                    folderStatistics.CountFilesDeleted++;
                                    _auditLog.Write(CreateLogEntry("FILE_DELETED", "", "", file2, GetFileDetailsForLog(fileDetails2), null));
                                }
                                catch (System.Exception exception)
                                {
                                    _auditLog.Write(CreateLogEntry("FILE_DELETED_ERROR", "", "", file2, GetFileDetailsForLog(fileDetails2), exception));
                                    folderStatistics.CountFileErrors++;
                                }
                                PauseIfRequired(false);
                            }
                            if (cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }
                        }

                        itemsProcessedCount++;
                        PauseIfRequired(false);

                        // Periodic UI update
                        if (_lastPeriodicProgressEvent.AddSeconds(5) <= DateTime.UtcNow)
                        {
                            _lastPeriodicProgressEvent = DateTime.UtcNow;
                            DoSyncFolderEvent(ProgressTypes.Periodic, rootSyncFoldersOptions, syncFoldersOptions.Folder1Resolved, folderStatistics, folderLevel, true);
                        }
                    }
                }

                // Free memory not needed
                fileDetails1List.Clear();
                fileDetails2List.Clear();

                //System.Windows.Forms.Application.DoEvents();

                // Delete folder 2 sub-folders not in folder 1
                if (!cancellationToken.IsCancellationRequested && !syncFoldersOptions.KeepDeletedItems)
                {
                    List<FolderDetails> subFolderDetails2List = fileRepository2.GetFolderDetailsList(syncFoldersOptions.Folder2Resolved);               
                    foreach (FolderDetails subFolderDetails2 in subFolderDetails2List)
                    {
                        WriteDebug(string.Format("Checking (3) {0}", subFolderDetails2.Name));

                        PauseIfRequired(false);
                        string subFolder1 = fileRepository1.PathCombine(syncFoldersOptions.Folder1Resolved, subFolderDetails2.Name);
                        string subFolder2 = fileRepository2.PathCombine(syncFoldersOptions.Folder2Resolved, subFolderDetails2.Name);

                        //if (!Directory.Exists(subFolder1))
                        if (!fileRepository1.IsFolderExists(subFolder1))
                        {
                            try
                            {
                                // Sub-folder no longer in folder 1, delete from folder 2
                                fileRepository2.DeleteFolder(subFolder2);
                                _auditLog.Write(CreateLogEntry("FOLDER_DELETED", "", "", subFolder2, "", null));
                            }
                            catch (System.Exception exception)
                            {
                                folderStatistics.CountFolderErrors++;
                                _auditLog.Write(CreateLogEntry("FOLDER_DELETED_ERROR", "", "", subFolder2, "", exception));
                            }
                        }
                        itemsProcessedCount++;
                        PauseIfRequired(false);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        // Periodic UI update
                        if (_lastPeriodicProgressEvent.AddSeconds(5) <= DateTime.UtcNow)
                        {
                            _lastPeriodicProgressEvent = DateTime.UtcNow;
                            DoSyncFolderEvent(ProgressTypes.Periodic, rootSyncFoldersOptions, syncFoldersOptions.Folder1Resolved, folderStatistics, folderLevel, true);
                        }
                    }
                    subFolderDetails2List.Clear();
                }

                PauseIfRequired(false);

                // Sync sub-folders
                if (!cancellationToken.IsCancellationRequested)
                {
                    List<Task> tasks = new List<Task>();

                    List<FolderDetails> subFolderDetails1List = fileRepository1.GetFolderDetailsList(syncFoldersOptions.Folder1Resolved);

                    foreach (FolderDetails subFolderDetails1 in subFolderDetails1List)
                    {
                        WriteDebug(string.Format("Checking (4) {0}", subFolderDetails1.Name));

                        PauseIfRequired(false);
                        string subFolder1 = fileRepository1.PathCombine(syncFoldersOptions.Folder1Resolved, subFolderDetails1.Name);
                        string subFolder2 = fileRepository2.PathCombine(syncFoldersOptions.Folder2Resolved, subFolderDetails1.Name);

                        SyncFoldersOptions syncFoldersOptionsSub = new SyncFoldersOptions()
                        {
                            Folder1 = subFolder1,
                            Folder1Resolved = subFolder1,
                            Folder2 = subFolder2,
                            Folder2Resolved = subFolder2,
                            IncludeFileExtensionList = syncFoldersOptions.IncludeFileExtensionList,
                            ExcludeFileExtensionList = syncFoldersOptions.ExcludeFileExtensionList,
                            Enabled = true,
                            KeepDeletedItems = syncFoldersOptions.KeepDeletedItems,
                            KeepFileProperties = syncFoldersOptions.KeepFileProperties
                        };

                        try
                        {
                            SyncFoldersInternal(rootSyncFoldersOptions, syncFoldersOptionsSub,
                                            fileRepository1, fileRepository2,
                                            folderStatistics,
                                            folderLevel + 1,
                                            cancellationToken);
                            itemsProcessedCount++;
                            PauseIfRequired(false);
                            
                        }
                        catch (System.Exception exception)
                        {
                            _auditLog.Write(CreateLogEntry("FOLDER_SYNC_ERROR", subFolder1, "", subFolder2, "", exception));
                            folderStatistics.CountFolderErrors++;
                        }

                        // Periodic UI update
                        if (_lastPeriodicProgressEvent.AddSeconds(5) <= DateTime.UtcNow)
                        {
                            _lastPeriodicProgressEvent = DateTime.UtcNow;
                            DoSyncFolderEvent(ProgressTypes.Periodic, rootSyncFoldersOptions, syncFoldersOptions.Folder1Resolved, folderStatistics, folderLevel, true);
                        }
                    }
                    subFolderDetails1List.Clear();
                }
            }

            // If top level then update time completed so that OnSyncFolderProgress has correct value and the UI
            // shows it
            if (folderLevel == 1)
            {
                syncFoldersOptions.TimeLastCompleted = DateTime.UtcNow;
            }

            folderStatistics.CountFoldersChecked++;

            WriteDebug(string.Format("Synchronised {0}", syncFoldersOptions.Folder1Resolved));

            if (isDisplayStatus)
            {
                DoDisplayStatus(string.Format("Checked {0}", syncFoldersOptions.Folder1Resolved), false);
            }           
            DoSyncFolderEvent(ProgressTypes.CompletedFolder, rootSyncFoldersOptions, syncFoldersOptions.Folder1Resolved, folderStatistics, folderLevel, (folderLevel == 1));            
        }

        /// <summary>
        /// Raises OnSyncFolderProgress if time overdue or forced
        /// </summary>
        /// <param name="progressType"></param>
        /// <param name="syncFolderOptions"></param>
        /// <param name="currentFolder"></param>
        /// <param name="folderStatistics"></param>
        /// <param name="folderLevel"></param>
        /// <param name="force"></param>
        private void DoSyncFolderEvent(ProgressTypes progressType, SyncFoldersOptions syncFolderOptions, string currentFolder,                        
                        FolderStatistics folderStatistics, int folderLevel,
                        bool force)
        {
            if (OnSyncFolderProgress != null)
            {
                TimeSpan frequency = TimeSpan.FromMilliseconds(500);
                if (force || _lastProgressEvent.Add(frequency) <= DateTime.UtcNow)
                {
                    _lastProgressEvent = DateTime.UtcNow;

                    try
                    {
                        OnSyncFolderProgress(progressType, syncFolderOptions, currentFolder, folderStatistics, folderLevel);
                    }
                    catch { };
                }
            }
        }
        
        private static string GetFileDetailsForLog(FileDetails fileDetails)
        {
            return string.Format("Len={0}, Cr={1}, Mod={2}", fileDetails.Length, fileDetails.TimeCreated, fileDetails.TimeModified);
        }
    
        /// <summary>
        /// Copies file from source to destination
        /// </summary>
        /// <param name="fileRepository1"></param>
        /// <param name="filePath1"></param>
        /// <param name="fileDetails1"></param>
        /// <param name="fileRepository2"></param>
        /// <param name="filePath2"></param>
        /// <param name="fileDetails2"></param>
        /// <param name="copyProperties"></param>
        private static void CopyFile(IFileRepository fileRepository1, string filePath1, FileDetails fileDetails1,
                              IFileRepository fileRepository2, string filePath2, FileDetails fileDetails2,
                              bool copyProperties)
        {
            fileRepository2.WriteFile(filePath1, filePath2, copyProperties);                
        }    
    
        /// <summary>
        /// Checks whether both files are the same. Just check size and timestamp
        /// </summary>
        /// <param name="fileDetails1"></param>
        /// <param name="fileDetails2"></param>
        /// <returns></returns>
        private static bool IsFilesTheSame(FileDetails fileDetails1, FileDetails fileDetails2)
        {
            bool changed = true;
            TimeSpan span = fileDetails1.TimeModified - fileDetails2.TimeModified;
            //if (fileDetails1.Length == fileDetails2.Length && Math.Abs(span.TotalMilliseconds) <= 5000 && fileDetails1.Attributes == fileDetails2.Attributes)
            if (fileDetails1.Length == fileDetails2.Length && Math.Abs(span.TotalMilliseconds) <= 5000)   // Ignore attributes
            {
                changed = false;
            }
            return !changed;
        }       
    }
}
