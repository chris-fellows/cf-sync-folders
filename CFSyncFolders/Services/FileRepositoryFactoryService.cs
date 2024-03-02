namespace CFSyncFolders
{
    /// <summary>
    /// Factory for IFileRepository instances
    /// </summary>
    internal class FileRepositoryFactoryService
    {
        public static IFileRepository GetFolder2FileRepository()
        {
            return GetFileRepository(System.Configuration.ConfigurationSettings.AppSettings.Get("Folder2.FileRepositoryClass"));            
        }

        public static IFileRepository GetFolder1FileRepository()
        {
            return GetFileRepository(System.Configuration.ConfigurationSettings.AppSettings.Get("Folder1.FileRepositoryClass"));
        }

        private static IFileRepository GetFileRepository(string className)
        {            
            switch (className)
            {
                case "GoogleDriveRepository": return new GoogleDriveRepository();
                case "LocalRepository": return new LocalFileRepository();
                case "DropboxRepository": return new DropboxRepository();
                case "OneDriveRepository": return new OneDriveRepository();
            }
            return null;
        }
    }
}
