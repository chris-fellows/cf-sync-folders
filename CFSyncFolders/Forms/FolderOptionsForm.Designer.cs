namespace CFSyncFolders.Forms
{
    partial class FolderOptionsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FolderOptionsForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.tsbCancel = new System.Windows.Forms.ToolStripButton();
            this.txtFolder1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFolder2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkEnabled = new System.Windows.Forms.CheckBox();
            this.chkKeepFileProperties = new System.Windows.Forms.CheckBox();
            this.chkKeepDeletedFiles = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.nudSyncFrequencyMins = new System.Windows.Forms.NumericUpDown();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSyncFrequencyMins)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbClose,
            this.tsbCancel});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(750, 25);
            this.toolStrip1.TabIndex = 11;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsbClose
            // 
            this.tsbClose.Image = ((System.Drawing.Image)(resources.GetObject("tsbClose.Image")));
            this.tsbClose.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Size = new System.Drawing.Size(56, 22);
            this.tsbClose.Text = "Close";
            this.tsbClose.Click += new System.EventHandler(this.tsbClose_Click);
            // 
            // tsbCancel
            // 
            this.tsbCancel.Image = ((System.Drawing.Image)(resources.GetObject("tsbCancel.Image")));
            this.tsbCancel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCancel.Name = "tsbCancel";
            this.tsbCancel.Size = new System.Drawing.Size(63, 22);
            this.tsbCancel.Text = "Cancel";
            this.tsbCancel.Click += new System.EventHandler(this.tsbCancel_Click);
            // 
            // txtFolder1
            // 
            this.txtFolder1.Location = new System.Drawing.Point(110, 40);
            this.txtFolder1.Name = "txtFolder1";
            this.txtFolder1.Size = new System.Drawing.Size(630, 20);
            this.txtFolder1.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Source folder:";
            // 
            // txtFolder2
            // 
            this.txtFolder2.Location = new System.Drawing.Point(110, 66);
            this.txtFolder2.Name = "txtFolder2";
            this.txtFolder2.Size = new System.Drawing.Size(630, 20);
            this.txtFolder2.TabIndex = 15;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Destination folder:";
            // 
            // chkEnabled
            // 
            this.chkEnabled.AutoSize = true;
            this.chkEnabled.Location = new System.Drawing.Point(110, 92);
            this.chkEnabled.Name = "chkEnabled";
            this.chkEnabled.Size = new System.Drawing.Size(65, 17);
            this.chkEnabled.TabIndex = 16;
            this.chkEnabled.Text = "Enabled";
            this.chkEnabled.UseVisualStyleBackColor = true;
            // 
            // chkKeepFileProperties
            // 
            this.chkKeepFileProperties.AutoSize = true;
            this.chkKeepFileProperties.Location = new System.Drawing.Point(221, 92);
            this.chkKeepFileProperties.Name = "chkKeepFileProperties";
            this.chkKeepFileProperties.Size = new System.Drawing.Size(116, 17);
            this.chkKeepFileProperties.TabIndex = 17;
            this.chkKeepFileProperties.Text = "Keep file properties";
            this.chkKeepFileProperties.UseVisualStyleBackColor = true;
            this.chkKeepFileProperties.Visible = false;
            // 
            // chkKeepDeletedFiles
            // 
            this.chkKeepDeletedFiles.AutoSize = true;
            this.chkKeepDeletedFiles.Location = new System.Drawing.Point(357, 92);
            this.chkKeepDeletedFiles.Name = "chkKeepDeletedFiles";
            this.chkKeepDeletedFiles.Size = new System.Drawing.Size(110, 17);
            this.chkKeepDeletedFiles.TabIndex = 18;
            this.chkKeepDeletedFiles.Text = "Keep deleted files";
            this.chkKeepDeletedFiles.UseVisualStyleBackColor = true;
            this.chkKeepDeletedFiles.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 123);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 13);
            this.label3.TabIndex = 19;
            this.label3.Text = "Sync frequency:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(189, 123);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(28, 13);
            this.label4.TabIndex = 20;
            this.label4.Text = "mins";
            // 
            // nudSyncFrequencyMins
            // 
            this.nudSyncFrequencyMins.Location = new System.Drawing.Point(110, 121);
            this.nudSyncFrequencyMins.Maximum = new decimal(new int[] {
            1410065407,
            2,
            0,
            0});
            this.nudSyncFrequencyMins.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudSyncFrequencyMins.Name = "nudSyncFrequencyMins";
            this.nudSyncFrequencyMins.Size = new System.Drawing.Size(73, 20);
            this.nudSyncFrequencyMins.TabIndex = 21;
            this.nudSyncFrequencyMins.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // FolderOptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 154);
            this.Controls.Add(this.nudSyncFrequencyMins);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chkKeepDeletedFiles);
            this.Controls.Add(this.chkKeepFileProperties);
            this.Controls.Add(this.chkEnabled);
            this.Controls.Add(this.txtFolder2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtFolder1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.toolStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FolderOptionsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Folders Sync Options";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudSyncFrequencyMins)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private System.Windows.Forms.ToolStripButton tsbCancel;
        private System.Windows.Forms.TextBox txtFolder1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFolder2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.CheckBox chkKeepFileProperties;
        private System.Windows.Forms.CheckBox chkKeepDeletedFiles;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown nudSyncFrequencyMins;
    }
}