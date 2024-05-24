using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CFSyncFolders.Interfaces;
using CFSyncFolders.Models;

namespace CFSyncFolders.FileRepository
{
    /// <summary>
    /// Local file repository (local machine or network share)
    /// </summary>
    public class LocalFileRepository : IFileRepository
    {
        public string Name
        {
            get { return "File system"; }
        }

        public void Initialise()
        {
        }

        public bool IsFolderWritable(string folder)
        {
            bool writable = false;
            try
            {
                string driveLetter = GetDriveLetter(folder);
                DriveInfo driveInfo = new DriveInfo(driveLetter);
                writable = driveInfo.IsReady;
            }
            catch { };
            return writable;
        }

        private static string GetDriveLetter(string folder)
        {
            string driveLetter = folder.Substring(0, 2);
            return driveLetter;
        }

        public bool IsFolderAvailable(string folder)
        {       
            // Restrict destination drives, prevents accidental trashing of files if config set up incorrectly
            string driveLetter = GetDriveLetter(folder);
            if (Array.IndexOf(new string[] { @"C:" }, driveLetter) != -1)
            {
                return false;
            }
            return true;
        }

        public List<FileDetails> GetFileDetailsList(string folder)
        {
            List<FileDetails> fileDetailsList = new List<FileDetails>();

            DirectoryInfo directoryInfo = new DirectoryInfo(folder);
            FileInfo[] fileInfos = directoryInfo.GetFiles();
            foreach(FileInfo fileInfo in fileInfos)
            {
                fileDetailsList.Add(GetFileDetails(fileInfo));
            }
            return fileDetailsList;
        }

        private static FileDetails GetFileDetails(FileInfo fileInfo)
        {
            FileDetails fileDetails = new FileDetails()
            {
                Name = fileInfo.Name,               
                Length = fileInfo.Length,
                TimeCreated = fileInfo.CreationTime,
                TimeModified = fileInfo.LastWriteTime,
                TimeAccessed = fileInfo.LastAccessTime,
                Attributes = Convert.ToInt64(fileInfo.Attributes)
                
            };
            return fileDetails;
        }

        public FileDetails GetFileDetails(string filePath)
        {
            return GetFileDetails(new FileInfo(filePath));
        }

        public bool IsFileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public bool IsFolderExists(string folder)
        {
            return Directory.Exists(folder);
        }

        public void DeleteFile(string filePath)
        {
            //ThrowErrorIfWrong("", filePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public void DeleteFolder(string folder)
        {
            //ThrowErrorIfWrong(folder, "");
            if (Directory.Exists(folder))
            {
                DeleteFolderInternal(new DirectoryInfo(folder));
            }
        }

        private void DeleteFolderInternal(DirectoryInfo directoryInfo)
        {
            directoryInfo.Attributes = FileAttributes.Normal;

            // Delete files
            var files = directoryInfo.GetFiles();
            foreach (var file in files)
            {
                file.Attributes = FileAttributes.Normal;
                file.Delete();
            }

            // Delete sub-folders
            var directories = directoryInfo.GetDirectories();
            foreach (var directory in directories)
            {
                DeleteFolderInternal(directory);                
            }

            directoryInfo.Delete(true);
        }

        /// <summary>
        /// Throws error if wrong destination folder. This is a sanity check to prevent potentially modifying
        /// the source folder if the config is set up incorrectly.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="file"></param>
        private void ThrowErrorIfWrong(string folder, string file)
        {
            if (!String.IsNullOrEmpty(folder))
            {
                if (!folder.ToLower().Contains("CHRIS-".ToLower()))       // TODO: Change this
                {
                    throw new Exception("Invalid folder");
                }
            }

            if (!String.IsNullOrEmpty(file))
            {
                if (!file.ToLower().Contains("CHRIS-".ToLower()))
                {
                    throw new Exception("Invalid file");
                }
            }                
        }

        public void WriteFile(string srcFilePath, string dstFilePath, bool copyProperties)
        {
            //ThrowErrorIfWrong(dstFilePath, "");
         
            File.Copy(srcFilePath, dstFilePath, true);
            
            if (copyProperties)
            {
                FileDetails srcFileDetails = GetFileDetails(srcFilePath);
                File.SetCreationTime(dstFilePath, srcFileDetails.TimeCreated);
                File.SetLastAccessTime(dstFilePath, srcFileDetails.TimeAccessed);
                File.SetLastWriteTime(dstFilePath, srcFileDetails.TimeModified);
                File.SetAttributes(dstFilePath, (FileAttributes)srcFileDetails.Attributes);
            }            
        }

        public string PathCombine(string folder, string file)
        {
            return Path.Combine(folder, file);
        }

        public FolderDetails GetFolderDetails(string folder)
        {
            return GetFolderDetails(new DirectoryInfo(folder));
        }

        public List<FolderDetails> GetFolderDetailsList(string folder)
        {
            List<FolderDetails> folderDetailsList = new List<FolderDetails>();          
            foreach(string subFolder in Directory.GetDirectories(folder))
            {
                folderDetailsList.Add(GetFolderDetails(new DirectoryInfo(subFolder)));
            }
            return folderDetailsList;
        }

        private static FolderDetails GetFolderDetails(DirectoryInfo directoryInfo)
        {          
            FolderDetails folderDetails = new FolderDetails()
            {
                Name = directoryInfo.Name,               
                TimeCreated = directoryInfo.CreationTime,
                TimeModified = directoryInfo.LastWriteTime,
                TimeAccessed = directoryInfo.LastAccessTime                               
            };
            return folderDetails;
        }

        public void CreateFolder(string folder)
        {
            Directory.CreateDirectory(folder);
        }
    }
}
