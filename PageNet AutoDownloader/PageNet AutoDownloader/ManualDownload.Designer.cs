namespace PageNet_AutoDownloader
{
    partial class ManualDownload
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManualDownload));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Grid = new System.Windows.Forms.DataGridView();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.Grid2 = new System.Windows.Forms.DataGridView();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.startToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.TSSFile = new System.Windows.Forms.ToolStripStatusLabel();
            this.TSStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.FileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Progress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Convert = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Compression_Progress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Upload_Progress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FileSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.url = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Password = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RowNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.DTPCheck = new System.Windows.Forms.DateTimePicker();
            this.button4 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Grid2)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Grid);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(402, 318);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Station List";
            // 
            // Grid
            // 
            this.Grid.AllowUserToAddRows = false;
            this.Grid.AllowUserToDeleteRows = false;
            this.Grid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.Grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Grid.Location = new System.Drawing.Point(3, 18);
            this.Grid.Name = "Grid";
            this.Grid.ReadOnly = true;
            this.Grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Grid.Size = new System.Drawing.Size(393, 297);
            this.Grid.TabIndex = 0;
            this.Grid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Grid_CellClick);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.Grid2);
            this.groupBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox5.Location = new System.Drawing.Point(420, 12);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(746, 639);
            this.groupBox5.TabIndex = 10;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "File List";
            // 
            // Grid2
            // 
            this.Grid2.AllowUserToAddRows = false;
            this.Grid2.AllowUserToDeleteRows = false;
            this.Grid2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.Grid2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Grid2.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.FileName,
            this.Progress,
            this.Convert,
            this.Compression_Progress,
            this.Upload_Progress,
            this.FileSize,
            this.url,
            this.UserName,
            this.Password,
            this.RowNum});
            this.Grid2.Location = new System.Drawing.Point(5, 18);
            this.Grid2.Name = "Grid2";
            this.Grid2.ReadOnly = true;
            this.Grid2.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Grid2.ShowCellErrors = false;
            this.Grid2.ShowCellToolTips = false;
            this.Grid2.ShowEditingIcon = false;
            this.Grid2.ShowRowErrors = false;
            this.Grid2.Size = new System.Drawing.Size(735, 615);
            this.Grid2.TabIndex = 0;
            this.Grid2.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Grid2_CellClick);
            this.Grid2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Grid2_MouseClick);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button1.Location = new System.Drawing.Point(12, 438);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(209, 42);
            this.button1.TabIndex = 11;
            this.button1.Text = "Check Data Completeness";
            this.button1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
            this.button2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button2.Location = new System.Drawing.Point(12, 486);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(209, 42);
            this.button2.TabIndex = 12;
            this.button2.Text = "Process";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.Image = ((System.Drawing.Image)(resources.GetObject("button3.Image")));
            this.button3.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button3.Location = new System.Drawing.Point(12, 582);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(209, 42);
            this.button3.TabIndex = 13;
            this.button3.Text = "&Close";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startToolStripMenuItem,
            this.stopToolStripMenuItem,
            this.restartToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(153, 114);
            // 
            // startToolStripMenuItem
            // 
            this.startToolStripMenuItem.Name = "startToolStripMenuItem";
            this.startToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.startToolStripMenuItem.Text = "Download";
            this.startToolStripMenuItem.Click += new System.EventHandler(this.startToolStripMenuItem_Click);
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.stopToolStripMenuItem.Text = "Stop";
            this.stopToolStripMenuItem.Click += new System.EventHandler(this.stopToolStripMenuItem_Click);
            // 
            // restartToolStripMenuItem
            // 
            this.restartToolStripMenuItem.Name = "restartToolStripMenuItem";
            this.restartToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.restartToolStripMenuItem.Text = "Restart";
            this.restartToolStripMenuItem.Visible = false;
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Visible = false;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSSFile,
            this.TSStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 665);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1178, 22);
            this.statusStrip1.TabIndex = 15;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // TSSFile
            // 
            this.TSSFile.Name = "TSSFile";
            this.TSSFile.Size = new System.Drawing.Size(82, 17);
            this.TSSFile.Text = "                         ";
            // 
            // TSStatus
            // 
            this.TSStatus.Name = "TSStatus";
            this.TSStatus.Size = new System.Drawing.Size(127, 17);
            this.TSStatus.Text = "                                        ";
            // 
            // FileName
            // 
            this.FileName.HeaderText = "File";
            this.FileName.Name = "FileName";
            this.FileName.ReadOnly = true;
            this.FileName.Width = 150;
            // 
            // Progress
            // 
            this.Progress.FillWeight = 150F;
            this.Progress.HeaderText = "Download Progress";
            this.Progress.Name = "Progress";
            this.Progress.ReadOnly = true;
            this.Progress.Width = 150;
            // 
            // Convert
            // 
            this.Convert.HeaderText = "Conversion Progress";
            this.Convert.Name = "Convert";
            this.Convert.ReadOnly = true;
            // 
            // Compression_Progress
            // 
            this.Compression_Progress.HeaderText = "Compression Progress";
            this.Compression_Progress.Name = "Compression_Progress";
            this.Compression_Progress.ReadOnly = true;
            // 
            // Upload_Progress
            // 
            this.Upload_Progress.HeaderText = "Upload Progress";
            this.Upload_Progress.Name = "Upload_Progress";
            this.Upload_Progress.ReadOnly = true;
            // 
            // FileSize
            // 
            this.FileSize.HeaderText = "File Size(Bytes)";
            this.FileSize.Name = "FileSize";
            this.FileSize.ReadOnly = true;
            this.FileSize.Visible = false;
            // 
            // url
            // 
            this.url.HeaderText = "URL";
            this.url.Name = "url";
            this.url.ReadOnly = true;
            this.url.Visible = false;
            // 
            // UserName
            // 
            this.UserName.HeaderText = "UserName";
            this.UserName.Name = "UserName";
            this.UserName.ReadOnly = true;
            this.UserName.Visible = false;
            // 
            // Password
            // 
            this.Password.HeaderText = "Password";
            this.Password.Name = "Password";
            this.Password.ReadOnly = true;
            this.Password.Visible = false;
            // 
            // RowNum
            // 
            this.RowNum.HeaderText = "RowNum";
            this.RowNum.Name = "RowNum";
            this.RowNum.ReadOnly = true;
            this.RowNum.Visible = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.DTPCheck);
            this.groupBox2.Location = new System.Drawing.Point(12, 334);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(402, 89);
            this.groupBox2.TabIndex = 19;
            this.groupBox2.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(145, 16);
            this.label1.TabIndex = 20;
            this.label1.Text = "Choose Date to Check:";
            // 
            // DTPCheck
            // 
            this.DTPCheck.Location = new System.Drawing.Point(167, 35);
            this.DTPCheck.Name = "DTPCheck";
            this.DTPCheck.Size = new System.Drawing.Size(200, 20);
            this.DTPCheck.TabIndex = 19;
            // 
            // button4
            // 
            this.button4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button4.Image = ((System.Drawing.Image)(resources.GetObject("button4.Image")));
            this.button4.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button4.Location = new System.Drawing.Point(12, 534);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(209, 42);
            this.button4.TabIndex = 20;
            this.button4.Text = "&Stop Processes";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // ManualDownload
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1178, 687);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ManualDownload";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Download Manually";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
            this.groupBox5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Grid2)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView Grid;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.DataGridView Grid2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem startToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restartToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel TSSFile;
        private System.Windows.Forms.ToolStripStatusLabel TSStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Progress;
        private System.Windows.Forms.DataGridViewTextBoxColumn Convert;
        private System.Windows.Forms.DataGridViewTextBoxColumn Compression_Progress;
        private System.Windows.Forms.DataGridViewTextBoxColumn Upload_Progress;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn url;
        private System.Windows.Forms.DataGridViewTextBoxColumn UserName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Password;
        private System.Windows.Forms.DataGridViewTextBoxColumn RowNum;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker DTPCheck;
        private System.Windows.Forms.Button button4;
    }
}

