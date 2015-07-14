/*
 * Created by SharpDevelop.
 * User: ghelo
 * Date: 7/23/2013
 * Time: 2:17 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace MDB2RNX
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.lblDirectory = new System.Windows.Forms.Label();
			this.txtRawDirectory = new System.Windows.Forms.TextBox();
			this.btnBrowseSourceDirectory = new System.Windows.Forms.Button();
			this.btnBrowseDestinationDirectory = new System.Windows.Forms.Button();
			this.txtRinexDirectory = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.btnConvert = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.treeViewFiles = new System.Windows.Forms.TreeView();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.lblProgress = new System.Windows.Forms.Label();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.txtArguments = new System.Windows.Forms.TextBox();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// lblDirectory
			// 
			this.lblDirectory.AutoSize = true;
			this.lblDirectory.Location = new System.Drawing.Point(12, 19);
			this.lblDirectory.Name = "lblDirectory";
			this.lblDirectory.Size = new System.Drawing.Size(85, 15);
			this.lblDirectory.TabIndex = 0;
			this.lblDirectory.Text = "RAW directory";
			// 
			// txtRawDirectory
			// 
			this.txtRawDirectory.Location = new System.Drawing.Point(103, 16);
			this.txtRawDirectory.Name = "txtRawDirectory";
			this.txtRawDirectory.ReadOnly = true;
			this.txtRawDirectory.Size = new System.Drawing.Size(229, 23);
			this.txtRawDirectory.TabIndex = 1;
			// 
			// btnBrowseSourceDirectory
			// 
			this.btnBrowseSourceDirectory.Location = new System.Drawing.Point(338, 13);
			this.btnBrowseSourceDirectory.Name = "btnBrowseSourceDirectory";
			this.btnBrowseSourceDirectory.Size = new System.Drawing.Size(69, 27);
			this.btnBrowseSourceDirectory.TabIndex = 2;
			this.btnBrowseSourceDirectory.Text = "Browse";
			this.btnBrowseSourceDirectory.UseVisualStyleBackColor = true;
			this.btnBrowseSourceDirectory.Click += new System.EventHandler(this.BtnBrowseSourceDirectoryClick);
			// 
			// btnBrowseDestinationDirectory
			// 
			this.btnBrowseDestinationDirectory.Location = new System.Drawing.Point(338, 15);
			this.btnBrowseDestinationDirectory.Name = "btnBrowseDestinationDirectory";
			this.btnBrowseDestinationDirectory.Size = new System.Drawing.Size(69, 27);
			this.btnBrowseDestinationDirectory.TabIndex = 5;
			this.btnBrowseDestinationDirectory.Text = "Browse";
			this.btnBrowseDestinationDirectory.UseVisualStyleBackColor = true;
			this.btnBrowseDestinationDirectory.Click += new System.EventHandler(this.BtnBrowseDestinationDirectoryClick);
			// 
			// txtRinexDirectory
			// 
			this.txtRinexDirectory.Location = new System.Drawing.Point(97, 18);
			this.txtRinexDirectory.Name = "txtRinexDirectory";
			this.txtRinexDirectory.ReadOnly = true;
			this.txtRinexDirectory.Size = new System.Drawing.Size(235, 23);
			this.txtRinexDirectory.TabIndex = 4;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 22);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(57, 15);
			this.label1.TabIndex = 3;
			this.label1.Text = "Directory";
			// 
			// btnConvert
			// 
			this.btnConvert.AutoSize = true;
			this.btnConvert.Font = new System.Drawing.Font("Comic Sans MS", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnConvert.Location = new System.Drawing.Point(345, 446);
			this.btnConvert.Name = "btnConvert";
			this.btnConvert.Size = new System.Drawing.Size(87, 33);
			this.btnConvert.TabIndex = 6;
			this.btnConvert.Text = "Convert";
			this.btnConvert.UseVisualStyleBackColor = true;
			this.btnConvert.Click += new System.EventHandler(this.BtnConvertClick);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.lblDirectory);
			this.groupBox1.Controls.Add(this.treeViewFiles);
			this.groupBox1.Controls.Add(this.txtRawDirectory);
			this.groupBox1.Controls.Add(this.btnBrowseSourceDirectory);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(420, 271);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Source";
			// 
			// treeViewFiles
			// 
			this.treeViewFiles.CheckBoxes = true;
			this.treeViewFiles.Location = new System.Drawing.Point(12, 45);
			this.treeViewFiles.Name = "treeViewFiles";
			this.treeViewFiles.Size = new System.Drawing.Size(395, 211);
			this.treeViewFiles.TabIndex = 0;
			this.treeViewFiles.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewFilesAfterCheck);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.btnBrowseDestinationDirectory);
			this.groupBox2.Controls.Add(this.txtRinexDirectory);
			this.groupBox2.Location = new System.Drawing.Point(12, 289);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(423, 58);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Destination";
			// 
			// lblProgress
			// 
			this.lblProgress.Location = new System.Drawing.Point(12, 454);
			this.lblProgress.Name = "lblProgress";
			this.lblProgress.Size = new System.Drawing.Size(327, 20);
			this.lblProgress.TabIndex = 7;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.txtArguments);
			this.groupBox3.Location = new System.Drawing.Point(12, 354);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(420, 86);
			this.groupBox3.TabIndex = 8;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "TECQ arguments";
			// 
			// txtArguments
			// 
			this.txtArguments.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtArguments.Location = new System.Drawing.Point(12, 20);
			this.txtArguments.Multiline = true;
			this.txtArguments.Name = "txtArguments";
			this.txtArguments.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtArguments.Size = new System.Drawing.Size(395, 56);
			this.txtArguments.TabIndex = 0;
			this.txtArguments.Text = "-leica mdb +nav {FILENAME}.{YEAR}n,{FILENAME}.{YEAR}g +obs {FILENAME}.{YEAR}o {FI" +
			"LENAME}.m00";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(444, 488);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.lblProgress);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btnConvert);
			this.Font = new System.Drawing.Font("Comic Sans MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "LEICA to RINEX";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.TextBox txtArguments;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label lblProgress;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TreeView treeViewFiles;
		private System.Windows.Forms.Button btnConvert;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtRinexDirectory;
		private System.Windows.Forms.Button btnBrowseDestinationDirectory;
		private System.Windows.Forms.Button btnBrowseSourceDirectory;
		private System.Windows.Forms.TextBox txtRawDirectory;
		private System.Windows.Forms.Label lblDirectory;
	}
}
