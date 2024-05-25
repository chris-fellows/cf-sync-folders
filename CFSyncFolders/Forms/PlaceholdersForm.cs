using CFSyncFolders.Interfaces;
using CFSyncFolders.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CFSyncFolders.Forms
{
    /// <summary>
    /// Displays list of placeholders
    /// </summary>
    public partial class PlaceholdersForm : Form
    {
        private readonly IPlaceholderService _placeholderService;
        public PlaceholdersForm()
        {
            InitializeComponent();
        }

        public PlaceholdersForm(IPlaceholderService placeholderService)
        {
            InitializeComponent();

            ModelToView(_placeholderService.GetAll());
        }

        private void ModelToView(IReadOnlyList<Placeholder> placeholders)
        {
            dgvPlaceholders.Rows.Clear();
            dgvPlaceholders.Columns.Clear();
            var columnIndex = dgvPlaceholders.Columns.Add("Name", "Name");
            columnIndex = dgvPlaceholders.Columns.Add("Description", "Description");

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
        }
    }
}
