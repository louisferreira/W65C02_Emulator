
namespace W65C02S.Simulator
{
    partial class frmMDIMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMDIMain));
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.mnuNewFile = new System.Windows.Forms.ToolStripButton();
            this.mnuOpenFile = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuStartEmulator = new System.Windows.Forms.ToolStripButton();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuNewFile,
            this.mnuOpenFile,
            this.toolStripSeparator1,
            this.mnuStartEmulator});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(1099, 25);
            this.toolStrip.TabIndex = 11;
            this.toolStrip.Text = "toolStrip1";
            // 
            // mnuNewFile
            // 
            this.mnuNewFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.mnuNewFile.Image = ((System.Drawing.Image)(resources.GetObject("mnuNewFile.Image")));
            this.mnuNewFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnuNewFile.Name = "mnuNewFile";
            this.mnuNewFile.Size = new System.Drawing.Size(23, 22);
            this.mnuNewFile.Text = "New File";
            // 
            // mnuOpenFile
            // 
            this.mnuOpenFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.mnuOpenFile.Image = ((System.Drawing.Image)(resources.GetObject("mnuOpenFile.Image")));
            this.mnuOpenFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnuOpenFile.Name = "mnuOpenFile";
            this.mnuOpenFile.Size = new System.Drawing.Size(23, 22);
            this.mnuOpenFile.Text = "Open File";
            this.mnuOpenFile.Click += new System.EventHandler(this.mnuOpenFile_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // mnuStartEmulator
            // 
            this.mnuStartEmulator.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.mnuStartEmulator.Enabled = false;
            this.mnuStartEmulator.Image = ((System.Drawing.Image)(resources.GetObject("mnuStartEmulator.Image")));
            this.mnuStartEmulator.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.mnuStartEmulator.Name = "mnuStartEmulator";
            this.mnuStartEmulator.Size = new System.Drawing.Size(23, 22);
            this.mnuStartEmulator.Text = "Emulator";
            this.mnuStartEmulator.ToolTipText = "Start Emulator";
            this.mnuStartEmulator.Click += new System.EventHandler(this.mnuStartEmulator_Click);
            // 
            // frmMDIMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(51)))), ((int)(((byte)(73)))));
            this.ClientSize = new System.Drawing.Size(1099, 710);
            this.Controls.Add(this.toolStrip);
            this.ForeColor = System.Drawing.Color.White;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MinimumSize = new System.Drawing.Size(960, 650);
            this.Name = "frmMDIMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMDIMain_FormClosing);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton mnuNewFile;
        private System.Windows.Forms.ToolStripButton mnuOpenFile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton mnuStartEmulator;
    }
}

