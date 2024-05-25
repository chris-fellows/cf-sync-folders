using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CFSyncFolders.Interfaces;
using CFSyncFolders.Services;

namespace CFSyncFolders.Models
{
    /// <summary>
    /// Sync configuration
    /// </summary>
    [XmlType("SyncConfiguration")]
    public class SyncConfiguration : ICloneable
    {
        [XmlAttribute("ID")]
        public Guid ID { get; set; }

        [XmlAttribute("Description")]
        public string Description { get; set; }

        /// <summary>
        /// Defines a file on destination drive that must exist in order for the sync to take place. It can be
        /// used to dynamically determine the destination drive letter at runtime, might be a removable drive.
        /// </summary>   
        [XmlAttribute("VerificationFile")]
        public string VerificationFile { get; set; }

        /// <summary>
        /// Specific machine that config is valid for (null=Any). It is necessary for configs copying local 
        /// machine files (C:\*) to NAS. Each machine needs to only use the config for itself.
        /// </summary>
        public string Machine { get; set; }

        [XmlArray("FoldersOptions")]
        [XmlArrayItem("FolderOptions")]
        public List<SyncFoldersOptions> FoldersOptions = new List<SyncFoldersOptions>();

        public object Clone()
        {
            var copy = new SyncConfiguration()
            {
                ID = ID,
                Description = Description,
                VerificationFile = VerificationFile,
                Machine = Machine,
                FoldersOptions = FoldersOptions == null ? null : FoldersOptions.Select(fo => (SyncFoldersOptions)fo.Clone()).ToList()
            };
            return copy;
        }

        /// <summary>
        /// Returns folders that need a sync, either overdue or being forced to sync
        /// </summary>     
        public List<Guid> GetFoldersThatNeedSync(bool ignoreLastStartTime)
        {
            List<Guid> folderOptionIds = new List<Guid>();
            foreach (var folderOptions in this.FoldersOptions.Where(fo => fo.Enabled))
            {
                if (folderOptions.IsSyncOverdue || ignoreLastStartTime)
                {
                    folderOptionIds.Add(folderOptions.ID);
                }
            }
            return folderOptionIds;
        }

        /// <summary>
        /// Sets the resolved folders, replaces placeholders.       
        /// </summary>
        public void SetResolvedFolders(DateTime date, IPlaceholderService placeholderService)
        {
            string verificationFile = "";   // Full path
            bool isVerificationDriveMissing = false;

            // Find verification file across all drives, not necessary if UNC path is specified  
            if (!String.IsNullOrEmpty(this.VerificationFile) && !this.VerificationFile.StartsWith("\\"))
            {
                string verificationFileDrive = SyncFoldersService.GetVerificationFileDrive(this.VerificationFile);
                if (String.IsNullOrEmpty(verificationFileDrive))  // Can't determine verification file drive, possibly removable drive not connected
                {
                    isVerificationDriveMissing = true;
                }
                else
                {
                    verificationFile = string.Format("{0}{1}", verificationFileDrive, this.VerificationFile);
                }
            }

            foreach(var syncFoldersOptions in this.FoldersOptions)
            {
                if (isVerificationDriveMissing)
                {
                    // Clear folder 1 if we couldn't define drive letter
                    syncFoldersOptions.Folder1Resolved = syncFoldersOptions.Folder1;                    
                    if (syncFoldersOptions.Folder1.Contains("{verification_drive_letter}"))
                    {
                        syncFoldersOptions.Folder1Resolved = null;
                    }

                    // Clear folder 2 if we couldn't define drive letter
                    syncFoldersOptions.Folder2Resolved = syncFoldersOptions.Folder2;
                    if (syncFoldersOptions.Folder2.Contains("{verification_drive_letter}"))
                    {
                        syncFoldersOptions.Folder2Resolved = null;
                    }
                }
                else
                {
                    syncFoldersOptions.Folder1Resolved = SyncFoldersService.ReplacePlaceholdersInFolder(syncFoldersOptions.Folder1, date, verificationFile, placeholderService);
                    syncFoldersOptions.Folder2Resolved = SyncFoldersService.ReplacePlaceholdersInFolder(syncFoldersOptions.Folder2, date, verificationFile, placeholderService);
                }
            }
        }
    }
}
