//teqc -leica mdb +nav XXX.13n,XXX.13g XXX.m00 > XXX.13o


/*
 * Created by SharpDevelop.
 * User: ghelo
 * Date: 7/23/2013
 * Time: 2:17 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using System.Text.RegularExpressions;

using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;




namespace MDB2RNX
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		
		//public string BUILD = "07.31.2013";
		//public string BUILD = "12.10.2013";
		//public string BUILD = "01.06.2014";
		public string BUILD = "03.07.2014";
		
		
		private bool _TreeViewUpdating = false;
		private string _SourceDirectory = "";
		private string _DestinationDirectory = "";
		
		
		
		public MainForm()
		{
			InitializeComponent();
			
			this.Text = this.Text + " [" + this.BUILD +"]";
			
			
			IniParser parser = new IniParser(Environment.CurrentDirectory+ "\\MDB2RNX.ini");
			string storedArgs = parser.GetSetting("default", "TEQC_ARGUMENT");
			this.txtArguments.Text = (storedArgs == "" ? "-leica mdb +nav {FILENAME}.{YEAR}n,{FILENAME}.{YEAR}g +obs {FILENAME}.{YEAR}o {FILENAME}.m00;" : storedArgs);
			
			
			this.FormClosing += delegate { 
				parser.AddSetting("default","TEQC_ARGUMENT", this.txtArguments.Text);
				parser.SaveSettings(); 
			};
			
		}
		
		
		
		
		
			
		private void BtnBrowseSourceDirectoryClick(object sender, EventArgs e)
		{
			FolderBrowserDialog folder = new FolderBrowserDialog();
			folder.ShowDialog();
			
			this._SourceDirectory = folder.SelectedPath;
			txtRawDirectory.Text = this._SourceDirectory;
			
			if(Directory.Exists(folder.SelectedPath))
		   	{
				ListDirectory(treeViewFiles, folder.SelectedPath);
				treeViewFiles.Nodes[0].Expand();
		   	}
		}
		
		private void BtnBrowseDestinationDirectoryClick(object sender, EventArgs e)
		{
			FolderBrowserDialog folder = new FolderBrowserDialog();
			folder.ShowDialog();
			
			this._DestinationDirectory = folder.SelectedPath;
			txtRinexDirectory.Text = this._DestinationDirectory;

		}
		
		private void BtnConvertClick(object sender, EventArgs e)
		{
			
			//Convert();
			
			ConvertFilesAsync();
			
		}
		
		
		
		private string EvaluateSubstring(string s, string filename)
		{
			string pattern = @"\{FILENAME}\.substring\(\d*,\d*\)";
			Regex regEx = new Regex(pattern);
			Match match = regEx.Match(s);
			foreach(Group group in match.Groups){
				if(group.Value != String.Empty){
					string[] param = group.Value.Split(',');
					int start = Convert.ToInt32(param[0].Replace("{FILENAME}.substring(",""));
					int len = Convert.ToInt32(param[1].Replace(")",""));
					s = s.Replace(group.Value, filename.Substring(start,len));
				}
			}
			
			return s;
		}
		
		
		
		private void ConvertFilesAsync()
		{
			
			BackgroundWorker bw = new BackgroundWorker();
			
			// this allows our worker to report progress during work
			bw.WorkerReportsProgress = true;
			
			// what to do in the background thread
			bw.DoWork += new DoWorkEventHandler(
			delegate(object o, DoWorkEventArgs args)
			{
			    BackgroundWorker b = o as BackgroundWorker;
			
			    ConvertFiles(b);
			
			});
			
			// what to do when progress changed (update the progress bar for example)
			bw.ProgressChanged += new ProgressChangedEventHandler(
			delegate(object o, ProgressChangedEventArgs args)
			{
				string message = string.Format("{0}% Completed", args.ProgressPercentage);
				lblProgress.Text = message;
				System.Diagnostics.Debug.WriteLine(message);
			});
			
			// what to do when worker completes its task (notify the user)
			bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
			delegate(object o, RunWorkerCompletedEventArgs args)
			{
				string message = "Done!";
				lblProgress.Text = message;
				System.Diagnostics.Debug.WriteLine(message);
			});
			
			
			bw.RunWorkerAsync();
			
		}
		
		private void ConvertFiles(BackgroundWorker bw)
		{
			if(this._DestinationDirectory == "" ||
			   this._SourceDirectory == ""){
				MessageBox.Show("Please make sure all directory paths are set.", "You gotta be kidding me", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			
			// get all the files to be converted
			string[] files = GetFilenames(treeViewFiles.Nodes, new List<string>(){});
			
			if(files.Length == 0){
				MessageBox.Show("Make sure there are selected files.", "That's funny", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			
			DateTime now = DateTime.Now;
			string jobDirectory = this._DestinationDirectory + "\\" + String.Format("job-{0:yyyyMMdd}-{0:HHmmss}", now);
			
			Directory.CreateDirectory(jobDirectory);
			
			
			int count = 0;
			
			
			
			foreach(string fileName in files){
				try {
					
					FileInfo file = new FileInfo(fileName);
					
					string year = file.Directory.Parent.Parent.Parent.Name;
					
					int result = 0;
					if(int.TryParse(year, out result)){
						year = year.Substring(2,2);
					} else {
						throw new Exception("Cannot find year folder. Conversion cannot proceed.");
					}
					
					
					string newDirectory = file.Directory.FullName.Replace(this._SourceDirectory, jobDirectory);
					string newFileName = newDirectory + "\\" + file.Name;
					
					
					if(!Directory.Exists(newDirectory))
						Directory.CreateDirectory(newDirectory);
					
					
					// extract zip file
					UnZip(fileName, newDirectory);	
					
					string m00FileName = newFileName.TrimEnd(".zip".ToCharArray());
					
					FileInfo m00File = new FileInfo(m00FileName);
					
		
					RunTEQC(m00FileName, year);
					
					// delete m00 file
					m00File.Delete();
					
					// 
					string baseFilename = m00File.FullName.Replace(m00File.Extension,"");
					
					//string zipFilename = baseFilename + ".zip";
					
					Zip(baseFilename, year);
					count++;
					int percentage = (int)(((double)count/(double)files.Length)*100);
					
					bw.ReportProgress(percentage);
					
					
				} catch(Exception ex){
					MessageBox.Show(ex.Message + "\n" + ex.StackTrace,"Nooooooooooo!!!", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			}
			
			
			if(MessageBox.Show("It SEEMS everyting went fine. Do you want me to open \n " +
			                   "the directory of converted files?","Wohooo!", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				Process proc = new Process();
				proc.StartInfo.FileName = jobDirectory;
				proc.Start();	
			}
			
			
			return;
		
		}
		
		private void CreateZipEntry(string filename, ZipOutputStream zipStream){

			FileInfo fi = new FileInfo(filename);
						
			ZipEntry zipYYo = new ZipEntry(fi.Name);
			
			zipStream.PutNextEntry(zipYYo);
			
			
			
			byte[ ] buffer = new byte[4096];
        	using (FileStream streamReader = File.OpenRead(filename)) {
            	StreamUtils.Copy(streamReader, zipStream, buffer);
            	
        	}
			
		}
		
		private void Zip(string baseFilename, string year) {
			
			FileStream fsOut = File.Create(baseFilename + ".RNX.zip");
			ZipOutputStream zipStream = new ZipOutputStream(fsOut);
			
			zipStream.SetLevel(3); //0-9, 9 being the highest level of compression
		
			
			CreateZipEntry(baseFilename + "." + year + "o", zipStream);
			CreateZipEntry(baseFilename + "." + year + "n", zipStream);
			CreateZipEntry(baseFilename + "." + year + "g", zipStream);
			
			zipStream.CloseEntry();
        	
			zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
			zipStream.Close();
			
		}

		private void UnZip(string zipFileName, string unzipDirectory){
			
			
			using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFileName))) {
			
				ZipEntry theEntry;
				while ((theEntry = s.GetNextEntry()) != null) {
					
					Console.WriteLine(theEntry.Name);
					
					string directoryName = Path.GetDirectoryName(theEntry.Name);
					string fileName      = Path.GetFileName(theEntry.Name);
					
					// create directory
					if ( directoryName.Length > 0 ) {
						Directory.CreateDirectory(unzipDirectory + "\\" + directoryName);
					}
					
					if (fileName != String.Empty) {
						using (FileStream streamWriter = File.Create(unzipDirectory + "\\" + theEntry.Name)) {
						
							int size = 2048;
							byte[] data = new byte[2048];
							while (true) {
								size = s.Read(data, 0, data.Length);
								if (size > 0) {
									streamWriter.Write(data, 0, size);
								} else {
									break;
								}
							}
						}
					}
				}
			}
			
			
		}

		private void RunTEQC(string fileName, string year){
			//teqc -leica mdb +nav XXX.13n,XXX.13g XXX.m00 > XXX.13o
			//teqc -leica mdb +nav PFLO001a.13n,PFLO001a.13g PFLO001a.m00 > PFLO001a.13o
			// -leica mdb +nav {FILENAME}.{YEAR}n,{FILENAME}.{YEAR}g +obs {FILENAME}.{YEAR}o {FILENAME}.m00


			// causing error on PFLO001m.m00
			//fileName = fileName.TrimEnd(".m00".ToCharArray());
			fileName = fileName.Substring(0,fileName.Length-4);
			
			//string executable = System.Configuration.ConfigurationManager.AppSettings["OGR2OGR_EXE"].ToString();
			
			string executable = AppDomain.CurrentDomain.BaseDirectory + "\\teqc.exe";
			
			string args = txtArguments.Text;
			
			
			args = EvaluateSubstring(args, fileName);
			args = args.Replace("{FILENAME}", fileName);
			args = args.Replace("{YEAR}", year);
			
			
			//string args = String.Format("-leica mdb +nav {0}.{1}n,{0}.{1}g +obs  {0}.{1}o {0}.m00 ", fileName, year);
			//string args = String.Format("-leica mdb +nav {0}.13n,{0}.13g +obs  {0}.13o {0}.m00 ", fileName, year);
			
			
			
			// Use ProcessStartInfo class
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.CreateNoWindow = true;
			startInfo.UseShellExecute = false;
			startInfo.FileName = executable;
			startInfo.WindowStyle = ProcessWindowStyle.Hidden;
			startInfo.Arguments = args;

				
			startInfo.RedirectStandardError = true;
			
		
			
			string standardErrorMessage = "";
						
		    // Start the process with the info we specified.
		    // Call WaitForExit and then the using statement will close.
		    using (Process exeProcess = Process.Start(startInfo))
		    {
		    	standardErrorMessage = exeProcess.StandardError.ReadToEnd();
		    	exeProcess.WaitForExit();
		    }
		    if (standardErrorMessage != "" ){
		    	//throw new Exception(standardErrorMessage);
		    }
			
						
	    }
		
				
		
		private string[] GetFilenames(TreeNodeCollection nodes, List<string> fileNames){
			foreach (TreeNode item in nodes)
	        {
				if(item.Checked){
					if(item.Tag != null)
						fileNames.Add(item.Tag.ToString());
		            if (item.Nodes.Count > 0)
		            {
		                GetFilenames(item.Nodes, fileNames);
		            }
				}
	            
	        }
			
			return fileNames.ToArray();
		}

		private void ListDirectory(TreeView treeView, string path)
		{
			treeView.Nodes.Clear();
			var rootDirectoryInfo = new DirectoryInfo(path);
			treeView.Nodes.Add(CreateDirectoryNode(rootDirectoryInfo,"*.zip"));
		}
		
		private static TreeNode CreateDirectoryNode(DirectoryInfo directoryInfo, string filePattern)
		{
			var directoryNode = new TreeNode(directoryInfo.Name);
			foreach (var directory in directoryInfo.GetDirectories())
			    directoryNode.Nodes.Add(CreateDirectoryNode(directory,"*.zip"));
			foreach (var file in directoryInfo.GetFiles(filePattern))
				directoryNode.Nodes.Add(new TreeNode(file.Name){ Tag = file.FullName });
			return directoryNode;
		}

		private void TreeViewFilesAfterCheck(object sender, TreeViewEventArgs e)
		{
			if (_TreeViewUpdating)
				return;
			_TreeViewUpdating = true;
			CheckTreeViewNodeParent(e.Node);
			CheckTreeViewNode(e.Node, e.Node.Checked);	
			_TreeViewUpdating = false;
		}
		
		private void CheckTreeViewNodeParent(TreeNode node)
		{
			if(node.Parent != null)
			{
				node.Parent.Checked = true;
				CheckTreeViewNodeParent(node.Parent);
			}
		}
		
		private void CheckTreeViewNode(TreeNode node, Boolean isChecked)
	    {
				
	        foreach (TreeNode item in node.Nodes)
	        {
	            item.Checked = isChecked;
	
	            if (item.Nodes.Count > 0)
	            {
	                this.CheckTreeViewNode(item, isChecked);
	            }
	        }
	    }

		
		

	}
}


