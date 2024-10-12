using CFUtilities.Logging;
using CFUtilities.Models;
using CFUtilities.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CFSyncFolders.Forms
{
    /// <summary>
    /// View log form. User can select log date
    /// </summary>
    public partial class LogForm : Form
    {
        private readonly ILogger _logger;

        public LogForm()
        {
            InitializeComponent();
        }

        public LogForm(ILogger logger)
        {
            InitializeComponent();

            _logger = logger;

            // Display log date ranges
            var logDate = DateTimeUtilities.GetStartOfDay(DateTimeOffset.UtcNow);
            var items = new List<NameValuePair<Tuple<DateTimeOffset, DateTimeOffset>>>();                        
            foreach (var daysAgo in new[] { 7, 30, 60, 90 })
            {
                items.Add(new NameValuePair<Tuple<DateTimeOffset, DateTimeOffset>>($"Last {daysAgo} days",
                            new Tuple<DateTimeOffset, DateTimeOffset>(logDate.Subtract(TimeSpan.FromDays(daysAgo)), logDate.AddHours(24))));
            }            
            tscbDate.ComboBox.DataSource = items;
            tscbDate.ComboBox.ValueMember = "Value";
            tscbDate.ComboBox.DisplayMember = "Name";
            tscbDate.ComboBox.DataSource = items;
           
            tscbDate.SelectedIndex = 0; // Display current log
        }

        private void tscbDate_SelectedIndexChanged(object sender, EventArgs e)
        {
            ClearLog();

            if (tscbDate.SelectedIndex == -1) return;

            var item = (NameValuePair<Tuple<DateTimeOffset, DateTimeOffset>>)tscbDate.ComboBox.SelectedItem;

            DisplayLog(item.Value.Item1, item.Value.Item2);
        }

        private void ClearLog()
        {  
            // Clear log
            dgvLog.Rows.Clear();
            dgvLog.Columns.Clear();            
        }

        /// <summary>
        /// Displays log for date range
        /// </summary>
        /// <param name="fromDateTime"></param>
        /// <param name="toDateTime"></param>
        private void DisplayLog(DateTimeOffset fromDateTime, DateTimeOffset toDateTime)
        {
            ClearLog();

            int rowCount = 0;
            foreach(var logEntry in _logger.GetByFilter(fromDateTime.DateTime, toDateTime.DateTime))
            {
                rowCount++;

                // Add row headers
                if (rowCount == 1)
                {
                    var columnIndex = dgvLog.Columns.Add("Created", "Created");   // TODO: Set format                                        
                    dgvLog.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    foreach (var key in logEntry.Values.Keys)
                    {
                        columnIndex = dgvLog.Columns.Add(key, key);
                        dgvLog.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    }
                }

                // Add data
                using (var row = new DataGridViewRow())
                {
                    using (var cell = new DataGridViewTextBoxCell())
                    {
                        cell.Value = logEntry.CreatedDateTime;
                        row.Cells.Add(cell);
                    }

                    foreach (var key in logEntry.Values.Keys)
                    {
                        using (var cell = new DataGridViewTextBoxCell())
                        {                           
                            cell.Value = logEntry.Values[key];
                            row.Cells.Add(cell);
                        }
                    }

                    dgvLog.Rows.Add(row);
                }
            }

            // Resize
            dgvLog.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        }

        private void LogForm_Load(object sender, EventArgs e)
        {
     
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
