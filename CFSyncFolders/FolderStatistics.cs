using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFSyncFolders
{
    /// <summary>
    /// Statistics for a particular folder
    /// </summary>
    public class FolderStatistics
    {
        public string Folder { get; set; }

        public int CountFilesNew { get; set; }       

        public int CountFilesUpdated { get; set; }     

        public int CountFilesDeleted { get; set; }

        public int CountFoldersChecked { get; set; }     

        public int CountFilesChecked { get; set; }        
        public int CountFileErrors { get; set; }        

        public int CountFolderErrors { get; set; }        
    }
}
