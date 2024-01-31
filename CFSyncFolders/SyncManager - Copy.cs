using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CFSyncFolders
{
    /// <summary>
    /// Performs one-way sync'ing of a source folder to a destination folder. The destination folder is made to look identical
    /// to the source folder.
    /// </summary>
    internal class SyncManager
    {
        private int _countFilesNew = 0;
        private int _countFilesUpdated = 0 ;
        private int _countFilesDeleted = 0;
        private int _countFoldersChecked = 0;
        private int _countFilesChecked = 0;
        private bool _cancelled = false;
        private int _folderLevel = 0;
        private string _logFile = "";

        public delegate void DisplayStatus(string status);
        public event DisplayStatus OnDisplayStatus; 

        public SyncManager(string logFile)
        {
            // Initialize log file
            _logFile = logFile;
            if (!String.IsNullOrEmpty(_logFile))
            {
                string folder = Path.GetDirectoryName(logFile);
                Directory.CreateDirectory(folder);
                if (!File.Exists(_logFile))
                {
                    WriteLogHeaders();
                }
            }
        }

        public string LogFile
        {
            get { return _logFile; }
        }
        
        public bool Cancelled
        {
            get { return _cancelled; }
            set { _cancelled = value; }
        }

        public int CountFilesNew
        {
            get { return _countFilesNew; }
        }

        public int CountFilesUpdated
        {
            get { return _countFilesUpdated; }
        }

        public int CountFilesDeleted
        {
            get { return _countFilesDeleted; }
        }

        public int CountFoldersChecked
        {
            get { return _countFoldersChecked; }
        }

        public int CountFilesChecked
        {
            get { return _countFilesChecked; }
        }

        private void DoDisplayStatus(string status)
        {
            if (OnDisplayStatus != null)
            {
                OnDisplayStatus(status);
            }
        }

        /// <summary>
        /// Returns the string with placeholders replaced
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetStringWithPlaceholdersReplaced(string input)
        {
            string output = input.Replace("{date}", DateTime.Now.ToString("yyyy-MM-dd"));
            output = output.Replace("{month}", DateTime.Now.Month.ToString());
            output = output.Replace("{day}", DateTime.Now.Day.ToString());
            output = output.Replace("{year}", DateTime.Now.Year.ToString());     
            output = output.Replace("{user_name}", Environment.UserName);
            output = output.Replace("{machine_name}", Environment.MachineName);

            // Handle placeholder for determining drive letter for drive with specific volume label. This is so that we support
            // backing up to removable drives where the drive letter might change.
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
            return output;
        }

        public List<SyncFoldersOptions> LoadSyncFoldersOptionsList()
        {
            List<SyncFoldersOptions> optionsList = new List<SyncFoldersOptions>();
            int count = 0;
            while (true)
            {
                count++;
                string key1 = string.Format("Folders.{0}.Folder1", count);
                string key2 = string.Format("Folders.{0}.Folder2", count);
                if (System.Configuration.ConfigurationSettings.AppSettings.Get(key1) != null)
                {
                    // Load folder pair, replace placeholders
                    SyncFoldersOptions options = new SyncFoldersOptions()
                    {
                        Folder1 = SyncManager.GetStringWithPlaceholdersReplaced(System.Configuration.ConfigurationSettings.AppSettings.Get(key1)),
                        Folder2 = SyncManager.GetStringWithPlaceholdersReplaced(System.Configuration.ConfigurationSettings.AppSettings.Get(key2)),
                        IncludeFileExtensionList = new string[0],
                        KeepFileProperties = false,
                        KeepDeletedItems = false
                    };
                    optionsList.Add(options);
                }
                else
                {
                    break;
                }
            }
            return optionsList;
        }

        /// <summary>
        /// Syncs folder 2 (destination) to match folder 1 (source). Files that exist in folder 2 are only copied if 
        /// different to folder 1.
        /// </summary>
        /// <param name="folder1">Source folder</param>
        /// <param name="folder2">Destination folder</param>
        public void SyncFolders(List<SyncFoldersOptions> syncFoldersOptionsList)
        {
            _cancelled = false;
            _folderLevel = 0;
            _countFilesNew = 0;
            _countFilesUpdated = 0;
            _countFilesDeleted = 0;
            _countFoldersChecked = 0;
            _countFilesChecked = 0;

            // Check if we can sync all of these folders
            foreach (SyncFoldersOptions syncFoldersOptions in syncFoldersOptionsList)
            {
                // Restrict destination drives, prevents accidental trashing of files if config set up incorrectly
                string driveLetter = GetDriveLetter(syncFoldersOptions.Folder2);
                if (Array.IndexOf(new string[] { @"C:" }, driveLetter) != -1)
                {
                    throw new Exception("This drive cannot be used as the destination");
                }
            }

            // Sync folders
            foreach (SyncFoldersOptions syncFoldersOptions in syncFoldersOptionsList)
            {
                string driveLetter = GetDriveLetter(syncFoldersOptions.Folder2);
                DriveInfo driveInfo = new DriveInfo(driveLetter);
                if (driveInfo.IsReady)      // Do nothing if drive not plugged in
                {
                    _folderLevel = 0;
                    WriteLog("SYNC_START", syncFoldersOptions.Folder1, syncFoldersOptions.Folder2, "");
                    SyncFoldersInternal(syncFoldersOptions.Folder1, syncFoldersOptions.Folder2, syncFoldersOptions.IncludeFileExtensionList, syncFoldersOptions.KeepFileProperties, syncFoldersOptions.KeepDeletedItems);
                    WriteLog("SYNC_END", syncFoldersOptions.Folder1, syncFoldersOptions.Folder2, "");
                }
            }
        }

        public static string GetDriveLetter(string folder)
        {
            string driveLetter = folder.Substring(0, 2);
            return driveLetter;
        }

        /// <summary>
        /// Determines if the file can be processed based on it's extension
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="includeFileExtensionList"></param>
        /// <returns></returns>
        //private bool IsCanProcessFileExtension(string filename, string[] includeFileExtensionList)
        //{
        //    bool canProcess = includeFileExtensionList.Length == 0;

        //    if (!canProcess)
        //    {                
        //        string extension = Path.GetExtension(filename);
        //        foreach (string currentExtension in includeFileExtensionList)
        //        {
        //            if (currentExtension.Equals(extension, StringComparison.InvariantCultureIgnoreCase))
        //            {
        //                canProcess = true;
        //                break;
        //            }
        //        }
        //    }
        //    return canProcess;
        //}

        private bool IsCanProcessFileExtension(FileInfo fileInfo, string[] includeFileExtensionList)
        {
            bool canProcess = includeFileExtensionList.Length == 0;

            if (!canProcess)
            {
                string extension = fileInfo.Extension;
                foreach (string currentExtension in includeFileExtensionList)
                {
                    if (currentExtension.Equals(extension, StringComparison.InvariantCultureIgnoreCase))
                    {
                        canProcess = true;
                        break;
                    }
                }
            }
            return canProcess;
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
        /// Syncs folder1 to folder2, only folder2 is modified
        /// </summary>
        /// <param name="folder1">Source folder</param>
        /// <param name="folder2">Destination folder (Folder that is modified)</param>
        /// <param name="includeFileExtensionList">Specific extensions to include</param>
        /// <param name="keepFileProperties">Whether to retain file properties (Timestamps etc)</param>
        /// <param name="keepDeletedItems">Whether to keep items in folder2 when they're deleted from folder1</param>
        private void SyncFoldersInternal(string folder1, string folder2, string[] includeFileExtensionList, bool keepFileProperties, bool keepDeletedItems)
        {
            if (_cancelled)
            {
                return;
            }

            // Check input parameters
            if (String.IsNullOrEmpty(folder1) || String.IsNullOrEmpty(folder2))
            {
                throw new ArgumentException("Invalid source or destination folders");
            }
            else if (folder1.Equals(folder2, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException("Source and destination folders are the same");
            }
            else if (!Directory.Exists(folder1))
            {
                throw new ArgumentException("Source folder does not exist");
            }

            _folderLevel++;

            int itemsProcessedCount = 0;
            bool isDisplayStatus = (_folderLevel < 7);  // Limit status updates for lower level folders
            if (isDisplayStatus)      
            {
                DoDisplayStatus(string.Format("Checking {0}", folder1));
            }

            // Create folder 2 if not exists
            if (!Directory.Exists(folder2))
            {
                Directory.CreateDirectory(folder2);
                WriteLog("FOLDER_NEW", folder1, folder2, "");
            }

            DirectoryInfo directoryInfo1 = new DirectoryInfo(folder1);
            DirectoryInfo directoryInfo2 = new DirectoryInfo(folder2);

            // Get file list
            FileInfo[] fileInfos1 = directoryInfo1.GetFiles();
            FileInfo[] fileInfos2 = directoryInfo2.GetFiles();

            // Copy changes/new files from folder 1
            if (!_cancelled)
            {
                foreach(FileInfo fileInfo1 in fileInfos1)
                {
                    string file1 = Path.Combine(folder1, fileInfo1.Name);
                    if (IsCanProcessFileExtension(fileInfo1, includeFileExtensionList))
                    {
                        _countFilesChecked++;
                        FileInfo fileInfo2 = GetFileInfo(fileInfos2, fileInfo1.Name);

                        string file2 = Path.Combine(folder2, fileInfo1.Name); 
                        if (fileInfo2 != null)
                        {
                            // File exists in folder 2, copy if different
                            if (!IsFilesTheSame(fileInfo1, fileInfo2))
                            {
                                string logFileData = "[1:" + GetFileInfoForLog(fileInfo1) + "] [2:" + GetFileInfoForLog(fileInfo2) + "]";
                                CopyFile(file1, file2, keepFileProperties, fileInfo1);
                                _countFilesUpdated++;
                                WriteLog("FILE_UPDATED", file1, file2, logFileData);
                            }                           
                        }
                        else
                        {
                            // Copy new file
                            string logFileData = "[1:" + GetFileInfoForLog(fileInfo1) + "]";
                            CopyFile(file1, file2, keepFileProperties, fileInfo1);
                            _countFilesNew++;
                            WriteLog("FILE_NEW", file1, file2, logFileData);
                        }
                    }

                    itemsProcessedCount++;
                    if (itemsProcessedCount % 100 == 0)
                    {
                        System.Threading.Thread.Sleep(1);                   
                    }

                    if (_cancelled)
                    {
                        break;
                    }
                }
            }

            System.Threading.Thread.Sleep(1);

            // Delete folder 2 files not in folder 1
            if (!_cancelled && !keepDeletedItems)
            {
                //foreach (string file2 in Directory.GetFiles(folder2))
                foreach(FileInfo fileInfo2 in fileInfos2)
                {                 
                    if (IsCanProcessFileExtension(fileInfo2, includeFileExtensionList))
                    {
                        FileInfo fileInfo1 = GetFileInfo(fileInfos1, fileInfo2.Name);
                        if (fileInfo1 == null)
                        {
                            // File no longer in folder 1, delete it from folder 2
                            string logFileData = "[2:" + GetFileInfoForLog(fileInfo2) + "]";
                            string file2 = Path.Combine(folder2, fileInfo2.Name);
                            File.Delete(file2);
                            _countFilesDeleted++;
                            WriteLog("FILE_DELETED", "", file2, logFileData);                           
                        }
                        if (_cancelled)
                        {
                            break;
                        }
                    }

                    itemsProcessedCount++;
                    if (itemsProcessedCount % 100 == 0)
                    {
                        System.Threading.Thread.Sleep(1);
                    }
                }
            }

            //System.Windows.Forms.Application.DoEvents();

            // Delete folder 2 sub-folders not in folder 1
            if (!_cancelled && !keepDeletedItems)
            {
                foreach (string subFolder2 in Directory.GetDirectories(folder2))
                {
                    string subFolder2Folder = GetLastDirectoryName(subFolder2);
                    string subFolder1 = string.Format(@"{0}\{1}", folder1, subFolder2Folder);
                    if (!Directory.Exists(subFolder1))
                    {
                        // Sub-folder no longer in folder 1, delete from folder 2
                        Directory.Delete(string.Format(@"{0}\{1}", folder2, subFolder2Folder), true);
                        WriteLog("FOLDER_DELETED", "", string.Format(@"{0}\{1}", folder2, subFolder2Folder), "");
                    }
                    itemsProcessedCount++;
                    if (itemsProcessedCount % 100 == 0)
                    {
                        System.Threading.Thread.Sleep(1);
                    }
                    if (_cancelled)
                    {
                        break;
                    }
                }
            }

            System.Threading.Thread.Sleep(1);

            // Sync sub-folders
            if (!_cancelled)
            {
                foreach (string subFolder1 in Directory.GetDirectories(folder1))
                {
                    string subFolder1Folder = GetLastDirectoryName(subFolder1);
                    string subFolder2 = string.Format(@"{0}\{1}", folder2, subFolder1Folder);
                    SyncFoldersInternal(subFolder1, subFolder2, includeFileExtensionList, keepFileProperties, keepDeletedItems);
                    itemsProcessedCount++;
                    if (itemsProcessedCount % 50 == 0)
                    {
                        System.Threading.Thread.Sleep(1);                     
                    }
                }
            }

            _countFoldersChecked++;
            _folderLevel--;
            if (isDisplayStatus)
            {
                DoDisplayStatus(string.Format("Checked {0}", folder1));
            }
        }

        private static string GetFileInfoForLog(FileInfo fileInfo)
        {         
            return string.Format("Len={0}, Cr={1}, Mod={2}", fileInfo.Length, fileInfo.CreationTime, fileInfo.LastWriteTime);
        }

        private void WriteLogHeaders()
        {
            if (!String.IsNullOrEmpty(_logFile))
            {
                Char delimiter = (Char)9;
                using (StreamWriter writer = new StreamWriter(_logFile, true))
                {
                    writer.WriteLine(string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}", delimiter, "Time", "Action", "Item1", "Item2", "Data"));
                    writer.Flush();
                    writer.Close();
                }
            }
        }

        private void WriteLog(string action, string item1, string item2, string data)
        {
            if (!String.IsNullOrEmpty(_logFile))
            {
                Char delimiter = (Char)9;
                using (StreamWriter writer = new StreamWriter(_logFile, true))
                {
                    writer.WriteLine(string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}", delimiter, DateTime.Now, action, item1, item2, data));
                    writer.Flush();
                    writer.Close();
                }
            }
        }

        private string GetLastDirectoryName(string path)
        {
            string[] folderList = path.Split(Path.DirectorySeparatorChar);
            return folderList[folderList.Length - 1];            
        }

        private static void CopyFile(string file1, string file2, bool keepFileProperties, FileInfo fileInfo1)
        {
            File.Copy(file1, file2, true);
            if (keepFileProperties)
            {                
                File.SetCreationTime(file2, fileInfo1.CreationTime);
                File.SetLastAccessTime(file2, fileInfo1.LastAccessTime);
                File.SetLastWriteTime(file2, fileInfo1.LastWriteTime);
                File.SetAttributes(file2, fileInfo1.Attributes);                               
            }
        }

        /// <summary>
        /// Checks if the files are the same by checking properties (size, timestamp etc), doesn't check contents. To handle
        /// differences in timestamps even when file hasn't changed then allow a tolerance.
        /// </summary>
        /// <param name="file1"></param>
        /// <param name="file2"></param>
        /// <returns></returns>
        //private bool IsFilesTheSame(string file1, string file2)
        //{
        //    bool changed = true;
        //    FileInfo fileInfo1 = new FileInfo(file1);
        //    FileInfo fileInfo2 = new FileInfo(file2);

        //    TimeSpan span = fileInfo1.LastWriteTimeUtc - fileInfo2.LastWriteTimeUtc;            
        //    if (fileInfo1.Length == fileInfo2.Length && Math.Abs(span.TotalMilliseconds) <= 5000 && fileInfo1.Attributes == fileInfo2.Attributes && fileInfo1.IsReadOnly == fileInfo2.IsReadOnly)
        //    {
        //        changed = false;
        //    }            
        //    return !changed;
        //}

        /// <summary>
        /// Returns whether files are the same
        /// </summary>
        /// <param name="fileInfo1"></param>
        /// <param name="fileInfo2"></param>
        /// <returns></returns>
        private bool IsFilesTheSame(FileInfo fileInfo1, FileInfo fileInfo2)
        {
            bool changed = true;                 
            TimeSpan span = fileInfo1.LastWriteTimeUtc - fileInfo2.LastWriteTimeUtc;
            if (fileInfo1.Length == fileInfo2.Length && Math.Abs(span.TotalMilliseconds) <= 5000 && fileInfo1.Attributes == fileInfo2.Attributes && fileInfo1.IsReadOnly == fileInfo2.IsReadOnly)
            {
                changed = false;
            }
            return !changed;
        }

        //private bool IsFileContentsTheSame(string file1, string file2)
        //{
        //    int file1byte;
        //    int file2byte;
        //    FileStream fs1 = null;
        //    FileStream fs2 = null;
        //    bool isSame = false;

        //    try
        //    {
        //        // Determine if the same file was referenced two times.
        //        if (file1 == file2)
        //        {
        //            // Return true to indicate that the files are the same.
        //            isSame = true;
        //        }
        //        else
        //        {
        //            // Open the two files.
        //            fs1 = new FileStream(file1, FileMode.Open);
        //            fs2 = new FileStream(file2, FileMode.Open);

        //            // Check the file sizes. If they are not the same, the files 
        //            // are not the same.
        //            if (fs1.Length != fs2.Length)
        //            {
        //                isSame = false;
        //            }
        //            else
        //            {
        //                // Read and compare a byte from each file until either a non-matching set of bytes is found or until the end of
        //                // file1 is reached.
        //                do
        //                {
        //                    // Read one byte from each file.
        //                    file1byte = fs1.ReadByte();
        //                    file2byte = fs2.ReadByte();
        //                }
        //                while ((file1byte == file2byte) && (file1byte != -1));
                     
        //                // Return the success of the comparison. "file1byte" is equal to "file2byte" at this point only if the files are 
        //                // the same.
        //                isSame = ((file1byte - file2byte) == 0);
        //            }
        //        }
        //    }
        //    catch (Exception exception)
        //    {

        //    }
        //    finally
        //    {
        //        if (fs1 != null)
        //        {
        //            fs1.Close();
        //        }
        //        if (fs2 != null)
        //        {
        //            fs2.Close();
        //        }
        //    }
        //    return isSame;
        //}
    }
}
