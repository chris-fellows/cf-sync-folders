﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CFSyncFolders
{
    /// <summary>
    /// File repository for Google Drive (In progress)
    /// </summary>
    public class GoogleDriveRepository : IFileRepository
    {
        public string Name
        {
            get { return "Google Drive"; }
        }

        public void Initialise()
        {

        }

        public bool IsFolderAvailable(string folder)
        {
            return false;
        }

        public bool IsFolderWritable(string folder)
        {
            return true;
        }

        public List<FileDetails> GetFileDetailsList(string folder)
        {
            return null;
        }

        public FileDetails GetFileDetails(string filePath)
        {
            return null;
        }

        public bool IsFileExists(string filePath)
        {
            return false;
        }

        public bool IsFolderExists(string folder)
        {
            return false;
        }

        public void DeleteFile(string filePath)
        { 
        
        }

        public void DeleteFolder(string folder)
        {
          
        }

        public void WriteFile(string srcFilePath, string dstFilePath, bool copyProperties)
        {
           
        }

        public string PathCombine(string folder, string file)
        {
            return string.Format("{0}/{1}", folder, file);
        }

        public FolderDetails GetFolderDetails(string folder)
        {
            throw new NotImplementedException();
        }

        public List<FolderDetails> GetFolderDetailsList(string folder)
        {
            return null;
        }

        public void CreateFolder(string folder)
        {
         
        }
    }
}
