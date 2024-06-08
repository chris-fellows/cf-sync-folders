using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CFSyncFolders.Models;
using CFUtilities.Interfaces;

namespace CFSyncFolders.Forms
{
    /// <summary>
    /// Form for viewing and modifying sync configuration
    /// </summary>
    public partial class SyncConfigurationForm : Form
    {
        private readonly IPlaceholderService _placeholderService;
        private SyncConfiguration _syncConfigurationOld;    // Update this on form closed
        private SyncConfiguration _syncConfigurationNew;

        public SyncConfigurationForm()
        {
            InitializeComponent();
        }

        public SyncConfigurationForm(IPlaceholderService placeholderService,  SyncConfiguration syncConfiguration)
        {
            InitializeComponent();

            _placeholderService = placeholderService;

            _syncConfigurationOld = syncConfiguration;
            _syncConfigurationNew = (SyncConfiguration)syncConfiguration.Clone();
            ModelToView(_syncConfigurationNew);
        }      

        private void ModelToView(SyncConfiguration syncConfiguration)
        {
            txtDescription.Text = syncConfiguration.Description;
            txtVerificationFile.Text = syncConfiguration.VerificationFile;
            txtMachine.Text = syncConfiguration.Machine;

            dgvFolder.Rows.Clear();
            dgvFolder.Columns.Clear();
            int columnIndex = dgvFolder.Columns.Add("Enabled", "Enabled");
            dgvFolder.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnIndex = dgvFolder.Columns.Add("Source", "Source");
            dgvFolder.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnIndex = dgvFolder.Columns.Add("Destination", "Destination");
            dgvFolder.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnIndex = dgvFolder.Columns.Add("Frequency (Min)", "Frequency (Min)");
            dgvFolder.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnIndex = dgvFolder.Columns.Add("Edit", "Edit");
            dgvFolder.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnIndex = dgvFolder.Columns.Add("Remove", "Remove");
            dgvFolder.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            foreach (SyncFoldersOptions syncFolderOptions in syncConfiguration.FoldersOptions)
            {
                dgvFolder.Rows.Add(AddSyncFoldersOptionsRow(syncFolderOptions));
            }
            dgvFolder.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private void ViewToModel(SyncConfiguration syncConfiguration)
        {
            syncConfiguration.ID = _syncConfigurationOld.ID;
            syncConfiguration.Description = txtDescription.Text;
            syncConfiguration.VerificationFile = txtVerificationFile.Text;
            syncConfiguration.Machine = txtMachine.Text;
            //syncConfiguration.FoldersOptions.Clear();
            syncConfiguration.FoldersOptions = _syncConfigurationNew.FoldersOptions;

            //for (int rowIndex =0; rowIndex < dgvFolder.Rows.Count; rowIndex++)
            //{
            //    syncConfiguration.FoldersOptions.Add(GetSyncFolderOptionsFromGrid(rowIndex));
            //}
        }

        private List<string> ValidateBeforeSave(SyncConfiguration syncConfiguration)
        {
            List<string> messages = new List<string>();

            if (String.IsNullOrEmpty(syncConfiguration.Description))
            {
                messages.Add("Description is invalid or not set");
            }
            if (String.IsNullOrEmpty(syncConfiguration.VerificationFile))
            {
                messages.Add("Verification file is invalid or not set");
            }
            if (syncConfiguration.FoldersOptions.Count == 0)
            {
                messages.Add("No folders have been selected");
            }
            else
            {
                // Check that source folder appears only once
                List<string> sourceFolders = syncConfiguration.FoldersOptions.Select(o => o.Folder1).Distinct().ToList();
                if (sourceFolders.Count != syncConfiguration.FoldersOptions.Count)
                {
                    messages.Add("Source folder appears more than once");
                }
            }

            foreach(SyncFoldersOptions syncFolderOptions in syncConfiguration.FoldersOptions)
            {
                if (String.IsNullOrEmpty(syncFolderOptions.Folder1) || String.IsNullOrEmpty(syncFolderOptions.Folder2))
                {
                    messages.Add("One or more folders is invalid");
                }
            }

            return messages;
        }
   
        private DataGridViewRow AddSyncFoldersOptionsRow(SyncFoldersOptions syncFolderOptions)
        {
            DataGridViewRow row = new DataGridViewRow();

            using (DataGridViewCheckBoxCell cell = new DataGridViewCheckBoxCell())
            {
                cell.Tag = syncFolderOptions;
                cell.Value = syncFolderOptions.Enabled;
                row.Cells.Add(cell);
            }
            using (DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell())
            {
                cell.Value = syncFolderOptions.Folder1;
                row.Cells.Add(cell);
            }
            using (DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell())
            {
                cell.Value = syncFolderOptions.Folder2;
                row.Cells.Add(cell);
            }
            using (DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell())
            {
                cell.Value = (syncFolderOptions.FrequencySeconds / 60).ToString();
                row.Cells.Add(cell);
            }
            using (DataGridViewButtonCell cell = new DataGridViewButtonCell())
            {
                cell.Value = "Edit";                
                row.Cells.Add(cell);
            }
            using (DataGridViewButtonCell cell = new DataGridViewButtonCell())
            {
                cell.Value = "Remove";
                row.Cells.Add(cell);
            }

            return row;
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Rows[e.RowIndex].Cells[e.ColumnIndex] is DataGridViewButtonCell)
            {
                var syncFoldersOptions = (SyncFoldersOptions)senderGrid.Rows[e.RowIndex].Cells[0].Tag;

                var cell = (DataGridViewButtonCell)senderGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];
                switch (cell.Value)
                {
                    case "Edit":
                        EditSyncFoldersOptions(e.RowIndex, syncFoldersOptions);
                        break;
                    case "Remove":                        
                        string sourceFolder = dgvFolder.Rows[e.RowIndex].Cells["Source"].Value.ToString();
                        if (MessageBox.Show(string.Format("Remove {0}?", sourceFolder), "Remove Folder", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            // Remove grid row
                            dgvFolder.Rows.RemoveAt(e.RowIndex);

                            // Remove
                            var item = _syncConfigurationNew.FoldersOptions.FirstOrDefault(i => i.Folder1 == sourceFolder);
                            _syncConfigurationNew.FoldersOptions.Remove(item);
                        }

                        break;
                }
            }
        }

        /// <summary>
        /// Edits sync folders options
        /// </summary>
        /// <param name="gridRowIndex"></param>
        /// <param name="syncFoldersOptions"></param>
        /// <returns></returns>
        private bool EditSyncFoldersOptions(int gridRowIndex, SyncFoldersOptions syncFoldersOptions)
        {
            var form = new FolderOptionsForm(_placeholderService, syncFoldersOptions);
            if (form.ShowDialog() == DialogResult.OK)
            {
                // Apply changes to UI
                dgvFolder.Rows[gridRowIndex].Cells["Source"].Value = syncFoldersOptions.Folder1;
                dgvFolder.Rows[gridRowIndex].Cells["Destination"].Value = syncFoldersOptions.Folder2;
                dgvFolder.Rows[gridRowIndex].Cells["Enabled"].Value = syncFoldersOptions.Enabled;
                dgvFolder.Rows[gridRowIndex].Cells["Frequency (Min)"].Value = syncFoldersOptions.FrequencySeconds / 60;

                return true;
            }
            return false;
        }

        private void tsbAddFolder_Click(object sender, EventArgs e)
        {
            SyncFoldersOptions syncFolderOptions = new SyncFoldersOptions()
            {
                 ID = Guid.NewGuid(),                 
                 Enabled = true,
                 Folder1 = @"C:\Temp",
                 Folder2 = @"{verification_file_drive}\Temp",
                 KeepFileProperties = false,
                 KeepDeletedItems = false,
                 FrequencySeconds = 1 * 3600
            };
            _syncConfigurationNew.FoldersOptions.Add(syncFolderOptions);

            // Add to grid
            dgvFolder.Rows.Add(AddSyncFoldersOptionsRow(syncFolderOptions));

            // Display control for edit
            EditSyncFoldersOptions(dgvFolder.Rows.Count - 1, syncFolderOptions);
        }        

        private void tsbSave_Click(object sender, EventArgs e)
        {
            // Apply changes to working copy
            ViewToModel(_syncConfigurationNew);

            List<string> messages = ValidateBeforeSave(_syncConfigurationNew);
            if (!messages.Any())
            {
                // Apply changes to original
                ViewToModel(_syncConfigurationOld);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(messages[0], "Error");
            }
        }

        private void tsbCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnLocalMachine_Click(object sender, EventArgs e)
        {
            txtMachine.Text = Environment.MachineName;
        }

        //private void tsbRemoveFolder_Click(object sender, EventArgs e)
        //{
        //    if (dgvFolder.SelectedCells.Count > 0)
        //    {
        //        int rowIndex = dgvFolder.SelectedCells[0].RowIndex;                
        //        string sourceFolder = dgvFolder.Rows[rowIndex].Cells["Source"].Value.ToString();
        //        if (MessageBox.Show(string.Format("Remove {0}?", sourceFolder), "Remove Folder", MessageBoxButtons.YesNo) == DialogResult.Yes)
        //        {
        //            dgvFolder.Rows.RemoveAt(rowIndex);
        //        }
        //    }
        //}
    }
}
