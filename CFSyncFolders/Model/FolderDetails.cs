using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CFSyncFolders
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
