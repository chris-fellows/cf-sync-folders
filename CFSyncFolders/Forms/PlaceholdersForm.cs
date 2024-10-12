using CFUtilities.Interfaces;
using CFUtilities.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace CFSyncFolders.Forms
{
    /// <summary>
    /// Displays list of placeholders
    /// </summary>
    public partial class PlaceholdersForm : Form
    {        
        public PlaceholdersForm()
        {
            InitializeComponent();
        }

        public PlaceholdersForm(IPlaceholderService placeholderService)
        {
            InitializeComponent();
            
            ModelToView(placeholderService.GetAll());
        }

        private void ModelToView(IReadOnlyList<Placeholder> placeholders)
        {
            dgvPlaceholders.Rows.Clear();
            dgvPlaceholders.Columns.Clear();
            var columnIndex = dgvPlaceholders.Columns.Add("Name", "Name");
            dgvPlaceholders.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            columnIndex = dgvPlaceholders.Columns.Add("Description", "Description");
            dgvPlaceholders.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            foreach (var placeholder in placeholders.OrderBy(p => p.Name))
            {
                using (var row = new DataGridViewRow())
                {
                    using (var cell = new DataGridViewTextBoxCell())
                    {
                        cell.Value = placeholder.Name;
                        row.Cells.Add(cell);
                    }

                    using (var cell = new DataGridViewTextBoxCell())
                    {
                        cell.Value = placeholder.Description;
                        row.Cells.Add(cell);
                    }

                    dgvPlaceholders.Rows.Add(row);
                }
            }

            dgvPlaceholders.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }
     
        private void tsbCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
