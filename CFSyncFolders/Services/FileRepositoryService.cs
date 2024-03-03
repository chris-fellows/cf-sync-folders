using CFSyncFolders.FileRepository;

namespace CFSyncFolders.Services
{
    /// <summary>
    /// Factory for IFileRepository instances
    /// </summary>
    internal class FileRepositoryService
    {      
        public IFileRepository GetFileRepository(string className)
        {            
            switch (className)
            {
                case "AWSS3Repository": return new AWSS3Repository();
                case "GoogleDriveRepository": return new GoogleDriveRepository();
                case "LocalRepository": return new LocalFileRepository();
                case "DropboxRepository": return new DropboxRepository();
                case "OneDriveRepository": return new OneDriveRepository();
                default:
                    throw new System.ApplicationException($"Cannot create file repository {className})");
            }            
        }
    }
}
