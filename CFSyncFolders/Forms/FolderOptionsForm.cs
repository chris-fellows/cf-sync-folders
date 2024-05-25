using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CFSyncFolders.Interfaces;
using CFSyncFolders.Models;

namespace CFSyncFolders.Forms
{
    /// <summary>
    /// Form for viewing and modifying sync folders options
    /// </summary>
    public partial class FolderOptionsForm : Form
    {
        private SyncFoldersOptions _syncFolderOptionsOld;   // Update this on form closed
        private SyncFoldersOptions _syncFolderOptionsNew;
        private readonly IPlaceholderService _placeholderService;

        public FolderOptionsForm()
        {
            InitializeComponent();
        }

        public FolderOptionsForm(IPlaceholderService placeholderService, SyncFoldersOptions syncFoldersOptions)
        {
            InitializeComponent();

            _placeholderService = placeholderService;

            _syncFolderOptionsOld = syncFoldersOptions;
            _syncFolderOptionsNew = (SyncFoldersOptions)_syncFolderOptionsOld.Clone();
            ModelToView(_syncFolderOptionsNew);
        }

        private void ModelToView(SyncFoldersOptions syncFoldersOptions)
        {            
            txtFolder1.Text = syncFoldersOptions.Folder1;
            txtFolder2.Text = syncFoldersOptions.Folder2;
            chkEnabled.Checked = syncFoldersOptions.Enabled;
            chkKeepFileProperties.Checked = syncFoldersOptions.KeepFileProperties;
            chkKeepDeletedFiles.Checked = syncFoldersOptions.KeepDeletedItems;
            nudSyncFrequencyMins.Value = syncFoldersOptions.FrequencySeconds / 60;
        }

        private void ViewToModel(SyncFoldersOptions syncFoldersOptions)
        {
            syncFoldersOptions.Folder1 = txtFolder1.Text;
            syncFoldersOptions.Folder2 = txtFolder2.Text;
            syncFoldersOptions.Enabled = chkEnabled.Checked;
            syncFoldersOptions.KeepFileProperties = chkKeepFileProperties.Checked;
            syncFoldersOptions.KeepDeletedItems = chkKeepDeletedFiles.Checked;
            syncFoldersOptions.FrequencySeconds = (int)(nudSyncFrequencyMins.Value * 60);
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            // Apply changes to working copy
            ViewToModel(_syncFolderOptionsNew);

            var messages = ValidateBeforeSave(_syncFolderOptionsNew);

            if (!messages.Any())
            {
                // Apply changes to original
                ViewToModel(_syncFolderOptionsOld);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(messages[0], "Error");
            }

        }

        private List<string> ValidateBeforeSave(SyncFoldersOptions syncFoldersOptions)
        {
            List<string> messages = new List<string>();

            if (String.IsNullOrEmpty(syncFoldersOptions.Folder1))
            {
                messages.Add("Source folder is invalid or not set");
            }
            if (String.IsNullOrEmpty(syncFoldersOptions.Folder2))
            {
                messages.Add("Destination folder is invalid or not set");
            }
            if (syncFoldersOptions.FrequencySeconds < 1)
            {
                messages.Add("Sync Frequency is invalid");
            }
            
            return messages;
        }

        private void tsbCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            new PlaceholdersForm(_placeholderService).ShowDialog();
        }
    }
}
