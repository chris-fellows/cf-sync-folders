using System;

namespace CFSyncFolders.Models
{
    /// <summary>
    /// Folder details
    /// </summary>
    public class FolderDetails
    {
        public string Name { get; set; }

        public DateTime TimeCreated { get; set; }
        public DateTime TimeModified { get; set; }
        public DateTime TimeAccessed { get; set; }
    }
}
