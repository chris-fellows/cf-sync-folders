using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CFSyncFolders
{
    /// <summary>
    /// Options for sync'ing a pair of folders
    /// </summary>
    [XmlType("SyncFolderOptions")]
    public class SyncFoldersOptions : ICloneable
    {
        /// <summary>
        /// Unique ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// Source folder, may contain placeholders
        /// </summary>
        [XmlAttribute("Folder1")]
        public string Folder1 { get; set; }

        /// <summary>
        /// Source with with placeholders replaced
        /// </summary>
        [XmlIgnore]
        public string Folder1Resolved { get; set; }

        /// <summary>
        /// Destination folder, may contain placeholders
        /// </summary>
        [XmlAttribute("Folder2")]
        public string Folder2 { get; set; }
        
        /// <summary>
        /// Destination folder with placeholders replaced
        /// </summary>
        [XmlIgnore]
        public string Folder2Resolved { get; set; }

        /// <summary>
        /// Whether the sync is currently enabled
        /// </summary>
        [XmlAttribute("Enabled")]
        public bool Enabled { get; set; }
        
        /// <summary>
        /// Time last started
        /// </summary>
        [XmlAttribute("TimeLastStarted")]
        public DateTime TimeLastStarted { get; set; }

        /// <summary>
        /// Time last completed
        /// </summary>
        [XmlAttribute("TimeLastCompleted")]
        public DateTime TimeLastCompleted { get; set; }

        /// <summary>
        /// Which file extensions to include
        /// </summary>
        [XmlArray("IncludeFileExtensionList")]
        [XmlArrayItem("Item")]
        public List<string> IncludeFileExtensionList { get; set; }

        /// <summary>
        /// Which file extensions to exclude
        /// </summary>
        [XmlArray("ExcludeFileExtensionList")]
        [XmlArrayItem("Item")]
        public List<String> ExcludeFileExtensionList { get; set; }

        /// <summary>
        /// Whether to keep file properties
        /// </summary>
        [XmlAttribute("KeepFileProperties")]
        public bool KeepFileProperties { get; set; }

        /// <summary>
        /// Whether to delete destination items no longer in source
        /// </summary>
        [XmlAttribute("KeepDeletedItems")]
        public bool KeepDeletedItems { get; set; }        
        
        /// <summary>
        /// How frequently the folders are sync'd
        /// </summary>
        [XmlAttribute("FrequencySeconds")]
        public int FrequencySeconds { get; set; }

        /// <summary>
        /// Whether a sync is overdue
        /// </summary>
        [XmlAttribute("IsSyncOverdue")]
        public bool IsSyncOverdue
        {
            get
            {                
                return (this.Enabled && this.TimeLastCompleted.AddSeconds(this.FrequencySeconds) < DateTime.UtcNow);                
            }
        }

        public object Clone()
        {
            var copy = new SyncFoldersOptions()
            {
                Enabled = Enabled,
                ExcludeFileExtensionList = ExcludeFileExtensionList,
                Folder1 = Folder1,
                Folder1Resolved = Folder1Resolved,
                Folder2 = Folder2,
                Folder2Resolved = Folder2Resolved,
                FrequencySeconds = FrequencySeconds, 
                ID = ID,
                IncludeFileExtensionList = IncludeFileExtensionList,
                KeepDeletedItems = KeepDeletedItems,
                KeepFileProperties = KeepFileProperties,
                TimeLastCompleted = TimeLastCompleted,
                TimeLastStarted = TimeLastStarted
            };
            return copy;
        }
    }
}
