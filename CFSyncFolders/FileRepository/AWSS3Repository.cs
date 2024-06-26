﻿using System;
using System.Collections.Generic;
using CFSyncFolders.Interfaces;
using CFSyncFolders.Models;

namespace CFSyncFolders.FileRepository
{
    /// <summary>
    /// File repository for AWS S3 (In progress)
    /// </summary>
    public class AWSS3Repository : IFileRepository
    {
        public string Name
        {
            get { return "AWS S3"; }
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
            throw new NotImplementedException();
        }

        public void DeleteFolder(string folder)
        {
            throw new NotImplementedException();
        }

        public void WriteFile(string srcFilePath, string dstFilePath, bool copyProperties)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}
