using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CFSyncFolders
{
    public partial class SyncConfigurationForm : Form
    {
        private SyncConfiguration _syncConfiguration;

        public SyncConfigurationForm()
        {
            InitializeComponent();
        }

        public SyncConfigurationForm(SyncConfiguration syncConfiguration)
        {
            InitializeComponent();

            _syncConfiguration = syncConfiguration;
            ModelToView(_syncConfiguration);
        }

        //public void SetParameters(SyncConfiguration syncConfiguration)
        //{
        //    _syncConfiguration = syncConfiguration;

        //    ModelToView(syncConfiguration);
        //}

        private void ModelToView(SyncConfiguration syncConfiguration)
        {
            txtDescription.Text = syncConfiguration.Description;
            txtVerificationFile.Text = syncConfiguration.VerificationFile;

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

            foreach (SyncFoldersOptions syncFolderOptions in syncConfiguration.FoldersOptions)
            {
                dgvFolder.Rows.Add(AddRow(syncFolderOptions));
            }
            dgvFolder.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private void ViewToModel(SyncConfiguration syncConfiguration)
        {
            syncConfiguration.ID = _syncConfiguration.ID;
            syncConfiguration.Description = txtDescription.Text;
            syncConfiguration.VerificationFile = txtVerificationFile.Text;
            syncConfiguration.FoldersOptions.Clear();

            for (int rowIndex =0; rowIndex < dgvFolder.Rows.Count; rowIndex++)
            {
                syncConfiguration.FoldersOptions.Add(GetSyncFolderOptionsFromGrid(rowIndex));
            }


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

        private SyncFoldersOptions GetSyncFolderOptionsFromGrid(int rowIndex)
        {
            SyncFoldersOptions syncFolderOptions = (SyncFoldersOptions)dgvFolder.Rows[rowIndex].Cells[0].Tag;

            DataGridViewCheckBoxCell cell = (DataGridViewCheckBoxCell)dgvFolder.Rows[rowIndex].Cells["Enabled"];
            syncFolderOptions.Enabled = Convert.ToBoolean(cell.Value);
            syncFolderOptions.Folder1 = dgvFolder.Rows[rowIndex].Cells["Source"].Value.ToString();
            syncFolderOptions.Folder2 = dgvFolder.Rows[rowIndex].Cells["Destination"].Value.ToString();         
            syncFolderOptions.FrequencySeconds = Convert.ToInt32(dgvFolder.Rows[rowIndex].Cells["Frequency (Min)"].Value.ToString()) * 60;

            return syncFolderOptions;
        }

        private DataGridViewRow AddRow(SyncFoldersOptions syncFolderOptions)
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

            return row;
        }

        private void tsbAddFolder_Click(object sender, EventArgs e)
        {
            SyncFoldersOptions syncFolderOptions = new SyncFoldersOptions()
            {
                 ID = Guid.NewGuid(),                 
                 Enabled = true,
                 Folder1 = @"C:\Temp",
                 Folder2 = @"{verification_file_drive}\Folder",
                 KeepFileProperties = false,
                 KeepDeletedItems = false,
                 FrequencySeconds = 1 * 3600
            };

            dgvFolder.Rows.Add(AddRow(syncFolderOptions));
        }

        private void tsbSave_Click(object sender, EventArgs e)
        {
            ViewToModel(_syncConfiguration);

            List<string> messages = ValidateBeforeSave(_syncConfiguration);
            if (!messages.Any())
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(messages[0], "Error");
            }
        }

        private void tsbRemoveFolder_Click(object sender, EventArgs e)
        {
            if (dgvFolder.SelectedCells.Count > 0)
            {
                int rowIndex = dgvFolder.SelectedCells[0].RowIndex;                
                string sourceFolder = dgvFolder.Rows[rowIndex].Cells["Source"].Value.ToString();
                if (MessageBox.Show(string.Format("Remove {0}?", sourceFolder), "Remove Folder", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    dgvFolder.Rows.RemoveAt(rowIndex);
                }
            }
        }
    }
}
