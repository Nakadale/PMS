namespace PageNet_AutoDownloader
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Grid = new System.Windows.Forms.DataGridView();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.TxtLogs = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.lblDate = new System.Windows.Forms.Label();
            this.lblTime = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.DTPTimeSched = new System.Windows.Forms.DateTimePicker();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.TSSFile = new System.Windows.Forms.ToolStripStatusLabel();
            this.TSStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.Grid2 = new System.Windows.Forms.DataGridView();
            this.FileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FileSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.url = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.UserName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Password = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RowNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FileDate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startBotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopBotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.manualDownloadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.clearFileListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideProgramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.leicaToRNXConverterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stationListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeDestinationFileServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeDestinationForConvertedFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setTeqCArgumentsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.lbldate1 = new System.Windows.Forms.Label();
            this.timer3 = new System.Windows.Forms.Timer(this.components);
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Grid2)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button1.Location = new System.Drawing.Point(10, 622);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 44);
            this.button1.TabIndex = 0;
            this.button1.Text = "        &Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button2.Enabled = false;
            this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
            this.button2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button2.Location = new System.Drawing.Point(91, 622);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 44);
            this.button2.TabIndex = 1;
            this.button2.Text = "            St&op";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Grid);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(831, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(402, 318);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Station List";
            // 
            // Grid
            // 
            this.Grid.AllowUserToAddRows = false;
            this.Grid.AllowUserToDeleteRows = false;
            this.Grid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.Grid.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
            this.Grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Grid.Location = new System.Drawing.Point(3, 18);
            this.Grid.Name = "Grid";
            this.Grid.ReadOnly = true;
            this.Grid.Size = new System.Drawing.Size(394, 297);
            this.Grid.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.TxtLogs);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(831, 348);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(402, 318);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Transaction Log";
            // 
            // TxtLogs
            // 
            this.TxtLogs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.TxtLogs.BackColor = System.Drawing.SystemColors.HighlightText;
            this.TxtLogs.Location = new System.Drawing.Point(3, 19);
            this.TxtLogs.Multiline = true;
            this.TxtLogs.Name = "TxtLogs";
            this.TxtLogs.ReadOnly = true;
            this.TxtLogs.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.TxtLogs.Size = new System.Drawing.Size(394, 296);
            this.TxtLogs.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.lblDate);
            this.groupBox3.Controls.Add(this.lblTime);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(12, 27);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(211, 206);
            this.groupBox3.TabIndex = 6;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Date and Time";
            // 
            // lblDate
            // 
            this.lblDate.AutoSize = true;
            this.lblDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDate.Location = new System.Drawing.Point(12, 163);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(63, 25);
            this.lblDate.TabIndex = 3;
            this.lblDate.Text = "Time";
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTime.Location = new System.Drawing.Point(12, 63);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(63, 25);
            this.lblTime.TabIndex = 2;
            this.lblTime.Text = "Time";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 126);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 25);
            this.label2.TabIndex = 1;
            this.label2.Text = "Date";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Time";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.DTPTimeSched);
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.Location = new System.Drawing.Point(12, 309);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(211, 87);
            this.groupBox4.TabIndex = 7;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Download Schedule";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(127, 16);
            this.label4.TabIndex = 2;
            this.label4.Text = "Data Completeness";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(182, 16);
            this.label3.TabIndex = 1;
            this.label3.Text = "Set Schedule for Checking of ";
            // 
            // DTPTimeSched
            // 
            this.DTPTimeSched.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.DTPTimeSched.Location = new System.Drawing.Point(6, 59);
            this.DTPTimeSched.Name = "DTPTimeSched";
            this.DTPTimeSched.ShowUpDown = true;
            this.DTPTimeSched.Size = new System.Drawing.Size(200, 22);
            this.DTPTimeSched.TabIndex = 0;
            this.DTPTimeSched.Value = new System.DateTime(2015, 5, 4, 10, 0, 0, 0);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSSFile,
            this.TSStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 673);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1238, 22);
            this.statusStrip1.TabIndex = 8;
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
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timer2
            // 
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.Grid2);
            this.groupBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox5.Location = new System.Drawing.Point(229, 27);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(598, 639);
            this.groupBox5.TabIndex = 9;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "File List";
            // 
            // Grid2
            // 
            this.Grid2.AllowUserToAddRows = false;
            this.Grid2.AllowUserToDeleteRows = false;
            this.Grid2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.Grid2.BackgroundColor = System.Drawing.SystemColors.ButtonHighlight;
            this.Grid2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Grid2.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.FileName,
            this.FileSize,
            this.url,
            this.UserName,
            this.Password,
            this.RowNum,
            this.FileDate});
            this.Grid2.Location = new System.Drawing.Point(5, 18);
            this.Grid2.Name = "Grid2";
            this.Grid2.ReadOnly = true;
            this.Grid2.ShowCellErrors = false;
            this.Grid2.ShowCellToolTips = false;
            this.Grid2.ShowEditingIcon = false;
            this.Grid2.ShowRowErrors = false;
            this.Grid2.Size = new System.Drawing.Size(589, 618);
            this.Grid2.TabIndex = 0;
            // 
            // FileName
            // 
            this.FileName.HeaderText = "File";
            this.FileName.Name = "FileName";
            this.FileName.ReadOnly = true;
            this.FileName.Width = 150;
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
            // FileDate
            // 
            this.FileDate.HeaderText = "FileDate";
            this.FileDate.Name = "FileDate";
            this.FileDate.ReadOnly = true;
            this.FileDate.Visible = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.exitToolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1238, 24);
            this.menuStrip1.TabIndex = 16;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            this.fileToolStripMenuItem.Visible = false;
            this.fileToolStripMenuItem.Click += new System.EventHandler(this.fileToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(89, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startBotToolStripMenuItem,
            this.stopBotToolStripMenuItem,
            this.toolStripSeparator2,
            this.manualDownloadToolStripMenuItem,
            this.toolStripSeparator3,
            this.clearFileListToolStripMenuItem,
            this.hideProgramToolStripMenuItem,
            this.leicaToRNXConverterToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // startBotToolStripMenuItem
            // 
            this.startBotToolStripMenuItem.Name = "startBotToolStripMenuItem";
            this.startBotToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.startBotToolStripMenuItem.Text = "Start";
            this.startBotToolStripMenuItem.Click += new System.EventHandler(this.startBotToolStripMenuItem_Click_1);
            // 
            // stopBotToolStripMenuItem
            // 
            this.stopBotToolStripMenuItem.Enabled = false;
            this.stopBotToolStripMenuItem.Name = "stopBotToolStripMenuItem";
            this.stopBotToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.stopBotToolStripMenuItem.Text = "Stop";
            this.stopBotToolStripMenuItem.Click += new System.EventHandler(this.stopBotToolStripMenuItem_Click_1);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(193, 6);
            // 
            // manualDownloadToolStripMenuItem
            // 
            this.manualDownloadToolStripMenuItem.Name = "manualDownloadToolStripMenuItem";
            this.manualDownloadToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.manualDownloadToolStripMenuItem.Text = "Manual Download";
            this.manualDownloadToolStripMenuItem.Click += new System.EventHandler(this.manualDownloadToolStripMenuItem_Click_1);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(193, 6);
            // 
            // clearFileListToolStripMenuItem
            // 
            this.clearFileListToolStripMenuItem.Name = "clearFileListToolStripMenuItem";
            this.clearFileListToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.clearFileListToolStripMenuItem.Text = "Clear File List";
            this.clearFileListToolStripMenuItem.Click += new System.EventHandler(this.clearFileListToolStripMenuItem_Click_1);
            // 
            // hideProgramToolStripMenuItem
            // 
            this.hideProgramToolStripMenuItem.Name = "hideProgramToolStripMenuItem";
            this.hideProgramToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.hideProgramToolStripMenuItem.Text = "Hide Program";
            this.hideProgramToolStripMenuItem.Visible = false;
            // 
            // leicaToRNXConverterToolStripMenuItem
            // 
            this.leicaToRNXConverterToolStripMenuItem.Name = "leicaToRNXConverterToolStripMenuItem";
            this.leicaToRNXConverterToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.leicaToRNXConverterToolStripMenuItem.Text = "Leica to RNX Converter";
            this.leicaToRNXConverterToolStripMenuItem.Visible = false;
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.stationListToolStripMenuItem,
            this.changeDestinationFileServerToolStripMenuItem,
            this.changeDestinationForConvertedFilesToolStripMenuItem,
            this.setTeqCArgumentsToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "&Settings";
            // 
            // stationListToolStripMenuItem
            // 
            this.stationListToolStripMenuItem.Name = "stationListToolStripMenuItem";
            this.stationListToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.stationListToolStripMenuItem.Text = "Station &List";
            this.stationListToolStripMenuItem.Click += new System.EventHandler(this.stationListToolStripMenuItem_Click);
            // 
            // changeDestinationFileServerToolStripMenuItem
            // 
            this.changeDestinationFileServerToolStripMenuItem.Name = "changeDestinationFileServerToolStripMenuItem";
            this.changeDestinationFileServerToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.changeDestinationFileServerToolStripMenuItem.Text = "File Server";
            this.changeDestinationFileServerToolStripMenuItem.Click += new System.EventHandler(this.changeDestinationFileServerToolStripMenuItem_Click);
            // 
            // changeDestinationForConvertedFilesToolStripMenuItem
            // 
            this.changeDestinationForConvertedFilesToolStripMenuItem.Name = "changeDestinationForConvertedFilesToolStripMenuItem";
            this.changeDestinationForConvertedFilesToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.changeDestinationForConvertedFilesToolStripMenuItem.Text = "RINEX Server";
            this.changeDestinationForConvertedFilesToolStripMenuItem.Click += new System.EventHandler(this.changeDestinationForConvertedFilesToolStripMenuItem_Click);
            // 
            // setTeqCArgumentsToolStripMenuItem
            // 
            this.setTeqCArgumentsToolStripMenuItem.Name = "setTeqCArgumentsToolStripMenuItem";
            this.setTeqCArgumentsToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.setTeqCArgumentsToolStripMenuItem.Text = "Set TeqC Arguments";
            this.setTeqCArgumentsToolStripMenuItem.Click += new System.EventHandler(this.setTeqCArgumentsToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem1
            // 
            this.exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
            this.exitToolStripMenuItem1.Size = new System.Drawing.Size(37, 20);
            this.exitToolStripMenuItem1.Text = "E&xit";
            this.exitToolStripMenuItem1.Click += new System.EventHandler(this.exitToolStripMenuItem1_Click);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.lbldate1);
            this.groupBox7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox7.Location = new System.Drawing.Point(12, 238);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(211, 65);
            this.groupBox7.TabIndex = 17;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Date to Check";
            // 
            // lbldate1
            // 
            this.lbldate1.AutoSize = true;
            this.lbldate1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbldate1.Location = new System.Drawing.Point(12, 25);
            this.lbldate1.Name = "lbldate1";
            this.lbldate1.Size = new System.Drawing.Size(63, 25);
            this.lbldate1.TabIndex = 4;
            this.lbldate1.Text = "Time";
            // 
            // timer3
            // 
            this.timer3.Tick += new System.EventHandler(this.timer3_Tick);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1238, 695);
            this.Controls.Add(this.groupBox7);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Data Completeness Control Module";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Grid2)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView Grid;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox TxtLogs;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.DateTimePicker DTPTimeSched;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripStatusLabel TSSFile;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.DataGridView Grid2;
        private System.Windows.Forms.ToolStripStatusLabel TSStatus;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stationListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeDestinationFileServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeDestinationForConvertedFilesToolStripMenuItem;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Label lbldate1;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem leicaToRNXConverterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem startBotToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stopBotToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manualDownloadToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem clearFileListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hideProgramToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setTeqCArgumentsToolStripMenuItem;
        private System.Windows.Forms.Timer timer3;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileName;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileSize;
        private System.Windows.Forms.DataGridViewTextBoxColumn url;
        private System.Windows.Forms.DataGridViewTextBoxColumn UserName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Password;
        private System.Windows.Forms.DataGridViewTextBoxColumn RowNum;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileDate;
    }
}

