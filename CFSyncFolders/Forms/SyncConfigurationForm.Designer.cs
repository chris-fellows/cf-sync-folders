namespace CFSyncFolders.Forms
{
    partial class SyncConfigurationForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SyncConfigurationForm));
            this.dgvFolder = new System.Windows.Forms.DataGridView();
            this.txtVerificationFile = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbSave = new System.Windows.Forms.ToolStripButton();
            this.tsbCancel = new System.Windows.Forms.ToolStripButton();
            this.tsbAddFolder = new System.Windows.Forms.ToolStripButton();
            this.txtMachine = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnLocalMachine = new System.Windows.Forms.Button();
            this.btnAnyMachine = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFolder)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvFolder
            // 
            this.dgvFolder.AllowUserToAddRows = false;
            this.dgvFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvFolder.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFolder.Location = new System.Drawing.Point(12, 105);
            this.dgvFolder.MultiSelect = false;
            this.dgvFolder.Name = "dgvFolder";
            this.dgvFolder.ReadOnly = true;
            this.dgvFolder.RowHeadersVisible = false;
            this.dgvFolder.Size = new System.Drawing.Size(913, 376);
            this.dgvFolder.TabIndex = 9;
            this.dgvFolder.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // txtVerificationFile
            // 
            this.txtVerificationFile.Location = new System.Drawing.Point(96, 53);
            this.txtVerificationFile.Name = "txtVerificationFile";
            this.txtVerificationFile.Size = new System.Drawing.Size(490, 20);
            this.txtVerificationFile.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Verification file:";
            // 
            // txtDescription
            // 
            this.txtDescription.Location = new System.Drawing.Point(96, 28);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(490, 20);
            this.txtDescription.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Description:";
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbSave,
            this.tsbCancel,
            this.tsbAddFolder});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(937, 25);
            this.toolStrip1.TabIndex = 10;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // tsbSave
            // 
            this.tsbSave.Image = ((System.Drawing.Image)(resources.GetObject("tsbSave.Image")));
            this.tsbSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSave.Name = "tsbSave";
            this.tsbSave.Size = new System.Drawing.Size(51, 22);
            this.tsbSave.Text = "Save";
            this.tsbSave.Click += new System.EventHandler(this.tsbSave_Click);
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
            // tsbAddFolder
            // 
            this.tsbAddFolder.Image = ((System.Drawing.Image)(resources.GetObject("tsbAddFolder.Image")));
            this.tsbAddFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAddFolder.Name = "tsbAddFolder";
            this.tsbAddFolder.Size = new System.Drawing.Size(85, 22);
            this.tsbAddFolder.Text = "Add Folder";
            this.tsbAddFolder.Click += new System.EventHandler(this.tsbAddFolder_Click);
            // 
            // txtMachine
            // 
            this.txtMachine.Location = new System.Drawing.Point(96, 79);
            this.txtMachine.Name = "txtMachine";
            this.txtMachine.Size = new System.Drawing.Size(163, 20);
            this.txtMachine.TabIndex = 12;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(51, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Machine:";
            // 
            // btnLocalMachine
            // 
            this.btnLocalMachine.Location = new System.Drawing.Point(266, 79);
            this.btnLocalMachine.Name = "btnLocalMachine";
            this.btnLocalMachine.Size = new System.Drawing.Size(92, 23);
            this.btnLocalMachine.TabIndex = 13;
            this.btnLocalMachine.Text = "This Machine";
            this.btnLocalMachine.UseVisualStyleBackColor = true;
            this.btnLocalMachine.Click += new System.EventHandler(this.btnLocalMachine_Click);
            // 
            // btnAnyMachine
            // 
            this.btnAnyMachine.Location = new System.Drawing.Point(364, 79);
            this.btnAnyMachine.Name = "btnAnyMachine";
            this.btnAnyMachine.Size = new System.Drawing.Size(92, 23);
            this.btnAnyMachine.TabIndex = 14;
            this.btnAnyMachine.Text = "Any Machine";
            this.btnAnyMachine.UseVisualStyleBackColor = true;
            this.btnAnyMachine.Click += new System.EventHandler(this.btnAnyMachine_Click);
            // 
            // SyncConfigurationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(937, 493);
            this.Controls.Add(this.btnAnyMachine);
            this.Controls.Add(this.btnLocalMachine);
            this.Controls.Add(this.txtMachine);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.dgvFolder);
            this.Controls.Add(this.txtVerificationFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SyncConfigurationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sync Configuration";
            ((System.ComponentModel.ISupportInitialize)(this.dgvFolder)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvFolder;
        private System.Windows.Forms.TextBox txtVerificationFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbSave;
        private System.Windows.Forms.ToolStripButton tsbAddFolder;
        private System.Windows.Forms.ToolStripButton tsbCancel;
        private System.Windows.Forms.TextBox txtMachine;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnLocalMachine;
        private System.Windows.Forms.Button btnAnyMachine;
    }
}