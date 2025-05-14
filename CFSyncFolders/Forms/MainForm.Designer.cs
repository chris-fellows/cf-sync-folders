namespace CFSyncFolders.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tssFolders = new System.Windows.Forms.ToolStripStatusLabel();
            this.niNotify = new System.Windows.Forms.NotifyIcon(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.tscbConfiguration = new System.Windows.Forms.ToolStripComboBox();
            this.tsbSync = new System.Windows.Forms.ToolStripButton();
            this.tsbAddConfig = new System.Windows.Forms.ToolStripButton();
            this.tsbEditConfig = new System.Windows.Forms.ToolStripButton();
            this.tsbViewLog = new System.Windows.Forms.ToolStripButton();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.lblSyncConfigMachine = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dgvFolders = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lstMessage = new System.Windows.Forms.ListBox();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFolders)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssFolders});
            this.statusStrip1.Location = new System.Drawing.Point(0, 513);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(907, 22);
            this.statusStrip1.TabIndex = 7;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tssFolders
            // 
            this.tssFolders.Name = "tssFolders";
            this.tssFolders.Size = new System.Drawing.Size(39, 17);
            this.tssFolders.Text = "Ready";
            // 
            // niNotify
            // 
            this.niNotify.Text = "Sync Folders - Idle";
            this.niNotify.Visible = true;
            this.niNotify.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.niNotify_MouseDoubleClick);
            this.niNotify.MouseMove += new System.Windows.Forms.MouseEventHandler(this.niNotify_MouseMove);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.tscbConfiguration,
            this.tsbSync,
            this.tsbAddConfig,
            this.tsbEditConfig,
            this.tsbViewLog});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(907, 25);
            this.toolStrip1.TabIndex = 9;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(84, 22);
            this.toolStripLabel1.Text = "Configuration:";
            // 
            // tscbConfiguration
            // 
            this.tscbConfiguration.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tscbConfiguration.Name = "tscbConfiguration";
            this.tscbConfiguration.Size = new System.Drawing.Size(280, 25);
            this.tscbConfiguration.SelectedIndexChanged += new System.EventHandler(this.tscbConfiguration_SelectedIndexChanged);
            // 
            // tsbSync
            // 
            this.tsbSync.Image = ((System.Drawing.Image)(resources.GetObject("tsbSync.Image")));
            this.tsbSync.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSync.Name = "tsbSync";
            this.tsbSync.Size = new System.Drawing.Size(52, 22);
            this.tsbSync.Text = "Sync";
            this.tsbSync.Click += new System.EventHandler(this.tsbSync_Click);
            // 
            // tsbAddConfig
            // 
            this.tsbAddConfig.Image = ((System.Drawing.Image)(resources.GetObject("tsbAddConfig.Image")));
            this.tsbAddConfig.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAddConfig.Name = "tsbAddConfig";
            this.tsbAddConfig.Size = new System.Drawing.Size(88, 22);
            this.tsbAddConfig.Text = "Add Config";
            this.tsbAddConfig.Click += new System.EventHandler(this.tsbAddConfig_Click);
            // 
            // tsbEditConfig
            // 
            this.tsbEditConfig.Image = ((System.Drawing.Image)(resources.GetObject("tsbEditConfig.Image")));
            this.tsbEditConfig.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbEditConfig.Name = "tsbEditConfig";
            this.tsbEditConfig.Size = new System.Drawing.Size(86, 22);
            this.tsbEditConfig.Text = "Edit Config";
            this.tsbEditConfig.Click += new System.EventHandler(this.tsbEditConfig_Click);
            // 
            // tsbViewLog
            // 
            this.tsbViewLog.Image = ((System.Drawing.Image)(resources.GetObject("tsbViewLog.Image")));
            this.tsbViewLog.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbViewLog.Name = "tsbViewLog";
            this.tsbViewLog.Size = new System.Drawing.Size(75, 22);
            this.tsbViewLog.Text = "View Log";
            this.tsbViewLog.Click += new System.EventHandler(this.tsbViewLog_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 25);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(907, 488);
            this.tabControl1.TabIndex = 11;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lblSyncConfigMachine);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.dgvFolders);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(966, 473);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Folders";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // lblSyncConfigMachine
            // 
            this.lblSyncConfigMachine.AutoSize = true;
            this.lblSyncConfigMachine.Location = new System.Drawing.Point(64, 9);
            this.lblSyncConfigMachine.Name = "lblSyncConfigMachine";
            this.lblSyncConfigMachine.Size = new System.Drawing.Size(25, 13);
            this.lblSyncConfigMachine.TabIndex = 14;
            this.lblSyncConfigMachine.Text = "Any";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Machine:";
            // 
            // dgvFolders
            // 
            this.dgvFolders.AllowUserToAddRows = false;
            this.dgvFolders.AllowUserToDeleteRows = false;
            this.dgvFolders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvFolders.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFolders.Location = new System.Drawing.Point(3, 30);
            this.dgvFolders.Name = "dgvFolders";
            this.dgvFolders.ReadOnly = true;
            this.dgvFolders.RowHeadersVisible = false;
            this.dgvFolders.Size = new System.Drawing.Size(960, 440);
            this.dgvFolders.TabIndex = 11;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lstMessage);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(899, 462);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Messages";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // lstMessage
            // 
            this.lstMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstMessage.FormattingEnabled = true;
            this.lstMessage.Location = new System.Drawing.Point(3, 3);
            this.lstMessage.Name = "lstMessage";
            this.lstMessage.Size = new System.Drawing.Size(893, 456);
            this.lstMessage.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(907, 535);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sync Folders";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFolders)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.NotifyIcon niNotify;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbSync;
        private System.Windows.Forms.ToolStripButton tsbViewLog;
        private System.Windows.Forms.ToolStripComboBox tscbConfiguration;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.DataGridView dgvFolders;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListBox lstMessage;
        private System.Windows.Forms.ToolStripButton tsbEditConfig;
        private System.Windows.Forms.ToolStripButton tsbAddConfig;
        private System.Windows.Forms.ToolStripStatusLabel tssFolders;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblSyncConfigMachine;
    }
}

