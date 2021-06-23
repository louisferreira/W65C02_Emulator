
namespace W65C02.IDE
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.pnlMenu = new System.Windows.Forms.Panel();
            this.bynHelp = new FontAwesome.Sharp.IconButton();
            this.btnConfiguration = new FontAwesome.Sharp.IconButton();
            this.btnOpCodeViewer = new FontAwesome.Sharp.IconButton();
            this.btnEmulator = new FontAwesome.Sharp.IconButton();
            this.btnMemoryMonitor = new FontAwesome.Sharp.IconButton();
            this.btnAssembler = new FontAwesome.Sharp.IconButton();
            this.pnlLogo = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pnlStatus = new System.Windows.Forms.Panel();
            this.pnlChildForm = new System.Windows.Forms.Panel();
            this.pnlMenu.SuspendLayout();
            this.pnlLogo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlMenu
            // 
            this.pnlMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(30)))), ((int)(((byte)(68)))));
            this.pnlMenu.Controls.Add(this.bynHelp);
            this.pnlMenu.Controls.Add(this.btnConfiguration);
            this.pnlMenu.Controls.Add(this.btnOpCodeViewer);
            this.pnlMenu.Controls.Add(this.btnEmulator);
            this.pnlMenu.Controls.Add(this.btnMemoryMonitor);
            this.pnlMenu.Controls.Add(this.btnAssembler);
            this.pnlMenu.Controls.Add(this.pnlLogo);
            this.pnlMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlMenu.Location = new System.Drawing.Point(0, 0);
            this.pnlMenu.Name = "pnlMenu";
            this.pnlMenu.Size = new System.Drawing.Size(167, 661);
            this.pnlMenu.TabIndex = 0;
            // 
            // bynHelp
            // 
            this.bynHelp.Dock = System.Windows.Forms.DockStyle.Top;
            this.bynHelp.FlatAppearance.BorderSize = 0;
            this.bynHelp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.bynHelp.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.bynHelp.ForeColor = System.Drawing.Color.Gainsboro;
            this.bynHelp.IconChar = FontAwesome.Sharp.IconChar.Question;
            this.bynHelp.IconColor = System.Drawing.Color.Gainsboro;
            this.bynHelp.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.bynHelp.IconSize = 32;
            this.bynHelp.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bynHelp.Location = new System.Drawing.Point(0, 298);
            this.bynHelp.Name = "bynHelp";
            this.bynHelp.Size = new System.Drawing.Size(167, 40);
            this.bynHelp.TabIndex = 6;
            this.bynHelp.Text = "Help";
            this.bynHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bynHelp.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.bynHelp.UseVisualStyleBackColor = true;
            this.bynHelp.Click += new System.EventHandler(this.iconButton_Click);
            // 
            // btnConfiguration
            // 
            this.btnConfiguration.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnConfiguration.FlatAppearance.BorderSize = 0;
            this.btnConfiguration.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfiguration.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnConfiguration.ForeColor = System.Drawing.Color.Gainsboro;
            this.btnConfiguration.IconChar = FontAwesome.Sharp.IconChar.Toolbox;
            this.btnConfiguration.IconColor = System.Drawing.Color.Gainsboro;
            this.btnConfiguration.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnConfiguration.IconSize = 32;
            this.btnConfiguration.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnConfiguration.Location = new System.Drawing.Point(0, 258);
            this.btnConfiguration.Name = "btnConfiguration";
            this.btnConfiguration.Size = new System.Drawing.Size(167, 40);
            this.btnConfiguration.TabIndex = 5;
            this.btnConfiguration.Text = "Configure";
            this.btnConfiguration.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnConfiguration.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnConfiguration.UseVisualStyleBackColor = true;
            this.btnConfiguration.Click += new System.EventHandler(this.iconButton_Click);
            // 
            // btnOpCodeViewer
            // 
            this.btnOpCodeViewer.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnOpCodeViewer.FlatAppearance.BorderSize = 0;
            this.btnOpCodeViewer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpCodeViewer.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnOpCodeViewer.ForeColor = System.Drawing.Color.Gainsboro;
            this.btnOpCodeViewer.IconChar = FontAwesome.Sharp.IconChar.Search;
            this.btnOpCodeViewer.IconColor = System.Drawing.Color.Gainsboro;
            this.btnOpCodeViewer.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnOpCodeViewer.IconSize = 32;
            this.btnOpCodeViewer.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOpCodeViewer.Location = new System.Drawing.Point(0, 218);
            this.btnOpCodeViewer.Name = "btnOpCodeViewer";
            this.btnOpCodeViewer.Size = new System.Drawing.Size(167, 40);
            this.btnOpCodeViewer.TabIndex = 4;
            this.btnOpCodeViewer.Text = "OpCode Viewer";
            this.btnOpCodeViewer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOpCodeViewer.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnOpCodeViewer.UseVisualStyleBackColor = true;
            this.btnOpCodeViewer.Click += new System.EventHandler(this.iconButton_Click);
            // 
            // btnEmulator
            // 
            this.btnEmulator.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnEmulator.FlatAppearance.BorderSize = 0;
            this.btnEmulator.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEmulator.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnEmulator.ForeColor = System.Drawing.Color.Gainsboro;
            this.btnEmulator.IconChar = FontAwesome.Sharp.IconChar.Running;
            this.btnEmulator.IconColor = System.Drawing.Color.Gainsboro;
            this.btnEmulator.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnEmulator.IconSize = 32;
            this.btnEmulator.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnEmulator.Location = new System.Drawing.Point(0, 178);
            this.btnEmulator.Name = "btnEmulator";
            this.btnEmulator.Size = new System.Drawing.Size(167, 40);
            this.btnEmulator.TabIndex = 3;
            this.btnEmulator.Text = "Simulator";
            this.btnEmulator.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnEmulator.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnEmulator.UseVisualStyleBackColor = true;
            this.btnEmulator.Click += new System.EventHandler(this.iconButton_Click);
            // 
            // btnMemoryMonitor
            // 
            this.btnMemoryMonitor.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnMemoryMonitor.FlatAppearance.BorderSize = 0;
            this.btnMemoryMonitor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMemoryMonitor.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnMemoryMonitor.ForeColor = System.Drawing.Color.Gainsboro;
            this.btnMemoryMonitor.IconChar = FontAwesome.Sharp.IconChar.Memory;
            this.btnMemoryMonitor.IconColor = System.Drawing.Color.Gainsboro;
            this.btnMemoryMonitor.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnMemoryMonitor.IconSize = 32;
            this.btnMemoryMonitor.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMemoryMonitor.Location = new System.Drawing.Point(0, 138);
            this.btnMemoryMonitor.Name = "btnMemoryMonitor";
            this.btnMemoryMonitor.Size = new System.Drawing.Size(167, 40);
            this.btnMemoryMonitor.TabIndex = 2;
            this.btnMemoryMonitor.Text = "Memory Monitor";
            this.btnMemoryMonitor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMemoryMonitor.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnMemoryMonitor.UseVisualStyleBackColor = true;
            this.btnMemoryMonitor.Click += new System.EventHandler(this.iconButton_Click);
            // 
            // btnAssembler
            // 
            this.btnAssembler.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnAssembler.FlatAppearance.BorderSize = 0;
            this.btnAssembler.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAssembler.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.btnAssembler.ForeColor = System.Drawing.Color.Gainsboro;
            this.btnAssembler.IconChar = FontAwesome.Sharp.IconChar.FileCode;
            this.btnAssembler.IconColor = System.Drawing.Color.Gainsboro;
            this.btnAssembler.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.btnAssembler.IconSize = 32;
            this.btnAssembler.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAssembler.Location = new System.Drawing.Point(0, 98);
            this.btnAssembler.Name = "btnAssembler";
            this.btnAssembler.Size = new System.Drawing.Size(167, 40);
            this.btnAssembler.TabIndex = 1;
            this.btnAssembler.Text = "Assembler";
            this.btnAssembler.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAssembler.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAssembler.UseVisualStyleBackColor = true;
            this.btnAssembler.Click += new System.EventHandler(this.iconButton_Click);
            // 
            // pnlLogo
            // 
            this.pnlLogo.Controls.Add(this.pictureBox1);
            this.pnlLogo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLogo.Location = new System.Drawing.Point(0, 0);
            this.pnlLogo.Name = "pnlLogo";
            this.pnlLogo.Size = new System.Drawing.Size(167, 98);
            this.pnlLogo.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(37, 10);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(80, 80);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // pnlStatus
            // 
            this.pnlStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(87)))), ((int)(((byte)(101)))));
            this.pnlStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlStatus.Location = new System.Drawing.Point(167, 607);
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Size = new System.Drawing.Size(767, 54);
            this.pnlStatus.TabIndex = 1;
            // 
            // pnlChildForm
            // 
            this.pnlChildForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlChildForm.Location = new System.Drawing.Point(167, 0);
            this.pnlChildForm.Name = "pnlChildForm";
            this.pnlChildForm.Size = new System.Drawing.Size(767, 607);
            this.pnlChildForm.TabIndex = 2;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(132)))), ((int)(((byte)(131)))), ((int)(((byte)(152)))));
            this.ClientSize = new System.Drawing.Size(934, 661);
            this.Controls.Add(this.pnlChildForm);
            this.Controls.Add(this.pnlStatus);
            this.Controls.Add(this.pnlMenu);
            this.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ForeColor = System.Drawing.Color.Gainsboro;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(950, 650);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "W65C02 Emulator";
            this.pnlMenu.ResumeLayout(false);
            this.pnlLogo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlMenu;
        private System.Windows.Forms.Panel pnlLogo;
        private FontAwesome.Sharp.IconButton btnAssembler;
        private FontAwesome.Sharp.IconButton bynHelp;
        private FontAwesome.Sharp.IconButton btnConfiguration;
        private FontAwesome.Sharp.IconButton btnOpCodeViewer;
        private FontAwesome.Sharp.IconButton btnEmulator;
        private FontAwesome.Sharp.IconButton btnMemoryMonitor;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel pnlStatus;
        private System.Windows.Forms.Panel pnlChildForm;
    }
}

