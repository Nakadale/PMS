namespace PageNet_AutoDownloader
{
    partial class StationList
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
            this.BtnClose = new System.Windows.Forms.Button();
            this.BtnAddStn = new System.Windows.Forms.Button();
            this.BtnEditStn = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.Grid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
            this.SuspendLayout();
            // 
            // BtnClose
            // 
            this.BtnClose.Location = new System.Drawing.Point(435, 524);
            this.BtnClose.Name = "BtnClose";
            this.BtnClose.Size = new System.Drawing.Size(135, 47);
            this.BtnClose.TabIndex = 9;
            this.BtnClose.Text = "&Close";
            this.BtnClose.UseVisualStyleBackColor = true;
            this.BtnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // BtnAddStn
            // 
            this.BtnAddStn.Location = new System.Drawing.Point(12, 524);
            this.BtnAddStn.Name = "BtnAddStn";
            this.BtnAddStn.Size = new System.Drawing.Size(135, 47);
            this.BtnAddStn.TabIndex = 8;
            this.BtnAddStn.Text = "&Add Station/Site";
            this.BtnAddStn.UseVisualStyleBackColor = true;
            this.BtnAddStn.Click += new System.EventHandler(this.BtnAddStn_Click);
            // 
            // BtnEditStn
            // 
            this.BtnEditStn.Location = new System.Drawing.Point(153, 524);
            this.BtnEditStn.Name = "BtnEditStn";
            this.BtnEditStn.Size = new System.Drawing.Size(135, 47);
            this.BtnEditStn.TabIndex = 7;
            this.BtnEditStn.Text = "&Edit Station/Site Info";
            this.BtnEditStn.UseVisualStyleBackColor = true;
            this.BtnEditStn.Click += new System.EventHandler(this.BtnEditStn_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(294, 524);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(135, 47);
            this.button1.TabIndex = 6;
            this.button1.Text = "&Delete Station/Site";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Grid
            // 
            this.Grid.AllowUserToAddRows = false;
            this.Grid.AllowUserToDeleteRows = false;
            this.Grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Grid.Location = new System.Drawing.Point(12, 12);
            this.Grid.MultiSelect = false;
            this.Grid.Name = "Grid";
            this.Grid.ReadOnly = true;
            this.Grid.RowHeadersVisible = false;
            this.Grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.Grid.Size = new System.Drawing.Size(558, 506);
            this.Grid.TabIndex = 5;
            this.Grid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Grid_Click);
            // 
            // StationList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(582, 580);
            this.Controls.Add(this.BtnClose);
            this.Controls.Add(this.BtnAddStn);
            this.Controls.Add(this.BtnEditStn);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Grid);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "StationList";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PageNet Station/Site List";
            this.Load += new System.EventHandler(this.StationList_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnClose;
        private System.Windows.Forms.Button BtnAddStn;
        private System.Windows.Forms.Button BtnEditStn;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView Grid;
    }
}

