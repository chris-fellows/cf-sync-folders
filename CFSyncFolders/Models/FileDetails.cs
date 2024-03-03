using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CFUtilities;

namespace CFSyncFolders.Models
{
    /// <summary>
    /// File details
    /// </summary>
    public class FileDetails
    {
        public string Name { get; set; }
        public string Folder { get; set; }
        public long Length { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimeModified { get; set; }
        public DateTime TimeAccessed { get; set; }
        public long Attributes { get; set; }

        public string Extension
        {
            get
            {                
                if (Name.Contains("."))
                {
                    string nameReverse = StringUtilities.Reverse(Name);
                    int index = nameReverse.IndexOf('.');
                    return StringUtilities.Reverse(nameReverse.Substring(0, index + 1));
                }
                return "";
            }
        }

    }
}
