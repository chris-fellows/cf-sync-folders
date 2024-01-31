using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CFSyncFolders
{
    /// <summary>
    /// Interface to file system
    /// </summary>
    public interface IFileRepository
    {
        bool IsFolderAvailable(string folder);
        bool IsFolderWritable(string folder);
        void Initialise();
        string Name { get; }
        List<FileDetails> GetFileDetailsList(string folder);
        FileDetails GetFileDetails(string filePath);
        void DeleteFile(string filePath);
        void DeleteFolder(string folder);
        void WriteFile(string srcFilePath, string dstFilePath, bool copyProperties);
        bool IsFileExists(string filePath);
        bool IsFolderExists(string folder);
        string PathCombine(string folder, string file);

        FolderDetails GetFolderDetails(string folder);
        List<FolderDetails> GetFolderDetailsList(string folder);
        void CreateFolder(string folder);
    }
}
