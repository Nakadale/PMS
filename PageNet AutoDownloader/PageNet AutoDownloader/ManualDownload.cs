﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Configuration;
using System.Data.SQLite;  // for connecting to SQLite DB.
using System.Threading; // this is for threading. for multi processor use.
using Ionic.Zip;
using Ionic.Zlib;
using Shared_Folder_Login;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using Sample;

namespace PageNet_AutoDownloader
{
    public partial class ManualDownload : Form
    {
        public ManualDownload()
        {
            InitializeComponent();
        }
        static readonly object _object = new object();
        String StrDate = DateTime.Now.Year + "_" + DateTime.Now.Month.ToString("00") + "_" + DateTime.Now.Day.ToString("00");
        int currRow = 0;
        int Grid2currRow = 0;
        int DownCounter = 0;
        private string _SourceDirectory = "";
        private string _DestinationDirectory = "";
        String File_Location, User_ID, Password_File,File_LocationRNX, User_IDRNX, PasswordRNX;
        String TeqC_Argument;

        bool CancelProg = false;
        List<FTPFiles> ftpfiles = new List<FTPFiles>(); // for storing of file list coming from the site


        private SQLiteConnection sql_con = new SQLiteConnection(@"Data Source=" + Application.StartupPath + @"\StationList.db;Version=3;New=False;Compress=True;");
        private SQLiteCommand sql_cmd;
        private SQLiteDataAdapter DBMain;
        private DataSet DSMain = new DataSet();
        private DataTable DTMain = new DataTable();
        //==============================

        public void LoadData()
        {
            // loads all station information from DB.
            sql_con.Open();

            sql_cmd = sql_con.CreateCommand();
            string CommandText = "Select * from Main";
            DBMain = new SQLiteDataAdapter(CommandText, sql_con);
            DSMain.Reset();
            DBMain.Fill(DSMain);
            DTMain = DSMain.Tables[0];
            Grid.DataSource = DTMain;
            sql_con.Close();
            Grid.Columns[2].Visible = false;
            Grid.Columns[3].Visible = false;
            Grid.Columns[0].HeaderText = "Station Code";
            Grid.Columns[1].HeaderText = "FTP Address";
            Grid.Columns[1].Width = 200;

            if (Grid.RowCount == 0)
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button4.Enabled = false;

            }

        }

        private void Grid2_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right) 
            {
                this.ContextMenuStrip = contextMenuStrip1;
                contextMenuStrip1.Show(Cursor.Position);
            }
        }
        public void LoadDesti()
        {
            sql_con.Open();
            string CommandText = "Select * from DestinationServer";
            SQLiteCommand command = new SQLiteCommand(CommandText, sql_con);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                //this.textBox8.Text = reader["File_Location"].ToString();
                //this.textBox7.Text = reader["User_ID"].ToString();
                //this.textBox6.Text = reader["Password"].ToString();
                File_Location = @reader["File_Location"].ToString();
                User_ID = reader["User_ID"].ToString();
                Password_File = reader["Password"].ToString();

            }

            CommandText = "Select * from DestinationServerRNX";
            command = new SQLiteCommand(CommandText, sql_con);
            reader = command.ExecuteReader();

            while (reader.Read())
            {
                //this.textBox8.Text = reader["File_Location"].ToString();
                //this.textBox7.Text = reader["User_ID"].ToString();
                //this.textBox6.Text = reader["Password"].ToString();
                File_LocationRNX = @reader["File_Location"].ToString();
                User_IDRNX = reader["User_ID"].ToString();
                PasswordRNX = reader["Password"].ToString();

            }

            CommandText = "Select * from TeqC";
            command = new SQLiteCommand(CommandText, sql_con);
            reader = command.ExecuteReader();

            while (reader.Read())
            {
                TeqC_Argument = reader["TeqC_Argument"].ToString();

            }
            sql_con.Close();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
            LoadDesti();

            DataGridViewProgressColumn column = new DataGridViewProgressColumn(); //download
            DataGridViewProgressColumn column1 = new DataGridViewProgressColumn(); //convert
            DataGridViewProgressColumn column2 = new DataGridViewProgressColumn(); //compress
            DataGridViewProgressColumn column3 = new DataGridViewProgressColumn(); //upload

            column.HeaderText = "Download Progress (%)";
            column.Name = "Download";
            Grid2.Columns.Add(column);

            column1.HeaderText = "Conversion Progress (%)";
            column.Name = "Conversion";

            Grid2.Columns.Add(column1);

            column2.HeaderText = "Compress Progress (%)";
            column.Name = "Compress";

            Grid2.Columns.Add(column2);
            
            column3.HeaderText = "Upload Progress (%)";
            column.Name = "Upload";

            Grid2.Columns.Add(column3);


        }

        private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Grid2.Rows.Clear();
            if (currRow == -1)
            {
                currRow = 0;
            }

            GetDataFromSite(currRow);

            lock (_object)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                {

                    foreach (FTPFiles i in ftpfiles)
                    {
                        string FileLoc = File_Location + DTPCheck.Value.Year.ToString() + @"\" + DTPCheck.Value.Month.ToString("00") + @"\" +
                        DTPCheck.Value.Day.ToString("00") + @"\" + Grid.Rows[currRow].Cells[0].Value.ToString() 
                        + @"\" + i.FileName.Substring(0, 12).ToString() + ".zip";

                                                //File exist code is in the if statement
                        if ((File.Exists(FileLoc) == false) && (File.Exists((@"C:\Temp\" + i.FileName.Substring(0, 12).ToString())) == false ))
                        {                            
                            Grid2.Rows.Add(i.FileName.Substring(0, 12).ToString(), i.FileBytes, Grid.Rows[currRow].Cells[1].Value.ToString(), Grid.Rows[currRow].Cells[2].Value.ToString(), Grid.Rows[currRow].Cells[3].Value.ToString(), currRow,0,0,0,0);
                            
                            Grid2.Update();
                        }

                        if ((File.Exists((@"C:\Temp\" + i.FileName.Substring(0, 12).ToString())) == true) && (File.Exists(FileLoc) == false))
                        {
                            Grid2.Rows.Add(i.FileName.Substring(0, 12).ToString(), i.FileBytes, Grid.Rows[currRow].Cells[1].Value.ToString(), Grid.Rows[currRow].Cells[2].Value.ToString(), Grid.Rows[currRow].Cells[3].Value.ToString(), currRow,0,0,0,0);
                            Grid2.Update();

                            FileInfo file1 = new FileInfo((@"C:\Temp\" + i.FileName.Substring(0, 12).ToString()));
                            Grid2.Rows[(Grid2.Rows.Count - 1)].Cells[6].Value = (Convert.ToInt32((Convert.ToDouble(file1.Length) / Convert.ToDouble(i.FileBytes)) * 100));
                            Grid2.Update();

                        }
                    }
                    if (Grid2.RowCount == 0 && ftpfiles.Count != 0)
                    {
                        MessageBox.Show("Raw files from this date " + DTPCheck.Value.ToShortDateString() + " are all downloaded.");
                    }
                }
            }
            //clears file list grid and ftpfiles where the file list from one site are stored.
            ftpfiles.Clear();



        }

        // connects to site and try to get all file information
        public void GetDataFromSite(int CurrRow)
        {
            DateTime DT = DTPCheck.Value;
           
            lock (_object)
            {
                

                    try
                    {
                        int FileCounter = 0;
                        // Get the object used to communicate with the server.
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(Grid.Rows[CurrRow].Cells[1].Value.ToString()));

                        request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                        request.Proxy = null;


                        request.Credentials = new NetworkCredential(Grid.Rows[CurrRow].Cells[2].Value.ToString(), Grid.Rows[CurrRow].Cells[3].Value.ToString());
                        FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                        Stream responseStream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(responseStream);
                        //UpdateStatusBar("[" + DateTime.Now + "] " + "Connected to Station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + "");
                        //DTPCheck.Value.DayOfYear.ToString("000");
                        string JulianDate = DTPCheck.Value.DayOfYear.ToString("000");
                        
                        //string JulianDate = DateTime.Now.DayOfYear.ToString("000");
                        //UpdateStatusBar("[" + DateTime.Now + "] " + "Getting File(s) Information");
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            var varsam = line.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            FTPFiles ftptemp = new FTPFiles();

                            if (varsam[8].Length > 8)
                            {

                                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                                {
                                    file.WriteLine("[" + DateTime.Now + "] " + "JulianDate: " + JulianDate + " varsam[8]: " + varsam[8] + " varsam[8].Substring(4, 3)): " + varsam[8].Substring(4, 3) + " varsam[8].Substring(9, 1)):" + varsam[8].Substring(9, 1));
                                }

                                if ((JulianDate == varsam[8].Substring(4, 3)) && (varsam[8].Substring(9, 1) == "m"))
                                {
                                    ftptemp.FileName = varsam[8].ToString();
                                    ftptemp.FileBytes = long.Parse(varsam[4].ToString());
                                    ftpfiles.Add(ftptemp);
                                    FileCounter = FileCounter + 1;
                                }
                            }
                        }
                        if (FileCounter == 0 )
                        {
                             MessageBox.Show("There are no available Raw files from this date " + DTPCheck.Value.ToShortDateString() + ".");
                        }
                        //UpdateStatusBar("[" + DateTime.Now + "] " + FileCounter + " File(s) Information collected.");
                    }
                    catch (WebException ex)
                    {
                        //MessageBox.Show(ex.Message.ToString());
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                            {
                                file.WriteLine("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established. Invalid Username or Password");
                            }
                            //Update("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established." + "\r\n");
                            MessageBox.Show("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established. Invalid Username or Password");
                        }
                        if (ex.Status == WebExceptionStatus.ConnectFailure)
                        {
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                            {
                                file.WriteLine("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established.");
                            }
                                //Update("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established." + "\r\n");
                            MessageBox.Show("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established.");                            
                            return;
                        }
                   }
                
            }
        }

        private void Grid_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            currRow = e.RowIndex;

        }


        private void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //currRow = e.RowIndex;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DoCheckOfAvailableFile(Grid2currRow);
        }


        public void DoCheckOfAvailableFile(object num)
        {
            if (Grid2.RowCount == 0 )
            {
                return;
            }
            int row = 0;
            int.TryParse(num.ToString(), out row);
            for (int y = 0; y == row; y++)
            {
                if ((Grid2.Rows[y].Cells[6].Value.ToString() != "0") && (Grid2.Rows[y].Cells[7].Value.ToString() != "0") && (Grid2.Rows[y].Cells[8].Value.ToString() != "0") && (Grid2.Rows[y].Cells[9].Value.ToString() != "0"))
                {
                    row = y;
                    break;
                }
                else if ((Convert.ToInt32(Grid2.Rows[y].Cells[6].Value) < 100) || (Convert.ToInt32(Grid2.Rows[y].Cells[9].Value) < 100))
                {
                    row = y;
                    break;
                }
            }
            lock (_object)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                {
                    //logs activity
                    file.WriteLine("[" + DateTime.Now + "] " + "Downloading File: " + Grid2.Rows[row].Cells[0].Value.ToString());
                    //logs activity
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Downloading File: " + Grid2.Rows[row].Cells[0].Value.ToString() + "");
                }

            }
            if (CancelProg == true)
            {
                UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                return;
            }

            ParameterizedThreadStart NPT = new ParameterizedThreadStart(DoDownloadFile);
            Thread T1 = new Thread(NPT);
            T1.IsBackground = true;
            T1.Start(row);

        }

        public void DoDownloadFile(object Counter)
        {
            try
            {
                int counter = 0;
                int.TryParse(Counter.ToString(), out counter);

                lock (_object)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {
                        //logs activity
                        file.WriteLine("[" + DateTime.Now + "] " + "Downloading File: " + Grid2.Rows[counter].Cells[0].Value.ToString());
                        //logs activity
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Downloading File: " + Grid2.Rows[counter].Cells[0].Value.ToString() + "");
                    }
                }

                if (CancelProg == true)
                {
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                    return;
                }
                Downloadfile(
                        Grid2.Rows[counter].Cells["url1"].Value.ToString(),//url
                        Grid2.Rows[counter].Cells["filename1"].Value.ToString(),//filename
                        Grid2.Rows[counter].Cells["UserName1"].Value.ToString(),//user
                        Grid2.Rows[counter].Cells["Password1"].Value.ToString(),//pass
                        Grid2.Rows[counter].Cells["Rownum1"].Value.ToString(),//rownumbergrid1
                        counter, //rownumbergrid2
                        Grid2.Rows[counter].Cells["FileSize1"].Value.ToString());//file size;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());

            }
        }

        public void UpdateStatusBar(string Status)
        {
            if (this.Grid2.InvokeRequired)
            {
                this.Grid2.Invoke(
                    new MethodInvoker(
                    delegate() { UpdateStatusBarText(Status); }));
            }
        }

        public void UpdateStatusBarText(String Status)
        {
            TSSFile.Text = Status;
        }



        //downloads Files that are not present in the Destination Server
        public void Downloadfile(string URL, string FileName, string user, string pass, string RowNumber, int Grid2RowNumber, string File_Size)
        {

            long FileSize = 0;
            string UserName = user;              //User Name of the FTP server
            string Password = pass;              //Password of the FTP server

            string LocalDirectory = "C:\\Temp\\";  //Local directory where the files will be downloaded

            DateTime DT = DTPCheck.Value;
            FileStream writeStream;
            //UpdateGrid2("Downloading File", Grid2RowNumber,2);

            try
            {
                //download code
                if (CancelProg == true)
                {
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                    return;

                }
                FileInfo file1 = new FileInfo(@"C:\Temp\" + FileName);
                FtpWebRequest requestFileDownload = (FtpWebRequest)WebRequest.Create(new Uri(URL + FileName));
                requestFileDownload.Credentials = new NetworkCredential(UserName, Password);
                requestFileDownload.Proxy = null;
                //requestFileDownload.Credentials = new NetworkCredential();
                requestFileDownload.Method = WebRequestMethods.Ftp.DownloadFile;
                //FileStream writeStream = new FileStream(LocalDirectory + "/" + FileName, FileMode.Create);

                if (file1.Exists)
                {
                    requestFileDownload.ContentOffset = file1.Length;
                    writeStream = new FileStream(@"C:\Temp\" + FileName, FileMode.Append, FileAccess.Write);
                }
                else
                {
                    writeStream = new FileStream(@"C:\Temp\" + FileName, FileMode.Create, FileAccess.Write);
                }
                FtpWebResponse responseFileDownload = (FtpWebResponse)requestFileDownload.GetResponse();
                Stream responseStream = responseFileDownload.GetResponseStream();

                int Length = 2048;
                Byte[] buffer = new Byte[Length];
                int bytesRead = responseStream.Read(buffer, 0, Length);
                lock (_object)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {
                        //logs activity into Logs.Txt file
                        file.WriteLine("[" + DateTime.Now + "] " + "Downloading File from site to temp folder: " + FileName + " with a File Size of " + File_Size);
                        //logs activity into Textbox
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Downloading File from site to temp folder: " + FileName + " with a File Size of " + File_Size + "");
                        //updates file activity into File List Grid
                        UpdateGrid2(0, Grid2RowNumber,6);
                    }
                }
                // streams/downloads file from site to temp folder.
                if (CancelProg == true)
                {
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                    return;

                }

                while (bytesRead > 0)
                {
                    if (FileSize != 0)
                    {
                        UpdateGrid2(Convert.ToInt32((FileSize / double.Parse(File_Size) * 100)), Grid2RowNumber, 6);
                        //UpdateStatusBar("[" + DateTime.Now + "] " + FileName + " Downloaded: " + String.Format((FileSize / double.Parse(File_Size) * 100).ToString("00"), "0.00") + "");
                        //Thread.Sleep(1000);
                    }
                    writeStream.Write(buffer, 0, bytesRead);
                    bytesRead = responseStream.Read(buffer, 0, Length);
                    FileSize = FileSize + bytesRead;
                    if (CancelProg == true)
                    {
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                        return;

                    }
                }

                //logs activity in the Logs.Txt file.
                lock (_object)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {
                        file.WriteLine("[" + DateTime.Now + "] " + FileName + " Size Downloaded: " + (FileSize + Length));
                    }
                }

                //logs activity into Textbox
                UpdateStatusBar("[" + DateTime.Now + "] " + FileName + " Size Downloaded: " + (FileSize + Length) + "");

                responseStream.Close();
                writeStream.Close();


                requestFileDownload = null;
                UpdateGrid2(100, Grid2RowNumber, 6);

                //********************************************************************************************
                // end of download code
                //********************************************************************************************

                //********************************************************************************************
                //start conversion protocol
                //********************************************************************************************
                lock (_object)
                {
                    UpdateGrid2(Convert.ToInt32(0), Grid2RowNumber, 7);
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {
                        file.WriteLine("[" + DateTime.Now + "] File Conversion Started.");
                    }
                }
                String targetPath = @"C:\Temp\" + FileName.Substring(0, 8) + ".RNX";
                string sourceFile = System.IO.Path.Combine(@"C:\Temp\", FileName);
                string destFile = System.IO.Path.Combine(targetPath, FileName);

                Directory.CreateDirectory(@"C:\Temp\" + FileName.Substring(0,8) + ".RNX");
                File.Copy(sourceFile, destFile);

                    //Debug.WriteLine("RunTeqC Started.");
                    RunTEQC(destFile, DateTime.Now.Year.ToString());
                    //Debug.WriteLine("RunTeqC Done.");
                File.Delete(destFile);
                    Debug.WriteLine("File Deleted.");
                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
                {
                    if (CancelProg == true)
                    {
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                        return;

                    }
                    String Target = targetPath+ @"\" + FileName.Substring(0,8) + "." + DateTime.Now.Year.ToString();
                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Default;
                    //zip.SaveProgress += (object sender, SaveProgressEventArgs e) => SaveProgress(sender, e, Grid2RowNumber);
                    zip.StatusMessageTextWriter = System.Console.Out;
                    zip.AddFile(Target+"g","");
                    zip.AddFile(Target + "n","");
                    zip.AddFile(Target + "o","");
                    zip.Save(LocalDirectory + FileName.Substring(0, 8) + ".RNX.zip");
                }
                lock (_object)
                {
                    UpdateGrid2(Convert.ToInt32(100), Grid2RowNumber, 7);
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {
                        file.WriteLine("[" + DateTime.Now + "] File Conversion Finished.");
                    }
                }
                //********************************************************************************************
                // calls the conversion protocol
                //********************************************************************************************

                //********************************************************************************************
                //compresses the download raw file.
                //********************************************************************************************
                lock (_object)
                {
                    UpdateGrid2(0, Grid2RowNumber, 8);
                    Thread.Sleep(1000);
                }
                if (CancelProg == true)
                {
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                    return;

                }
                //logs activity
                //MessageBox.Show(Grid2.Rows[0].Cells[7].Value.ToString());
                lock (_object)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {
                        file.WriteLine("[" + DateTime.Now + "] " + "Compressing File");
                    }
                }
                //logs activity
                UpdateStatusBar("[" + DateTime.Now + "] " + "Compressing File" + "\r\n");
                //logs activity
                //UpdateGrid2("Compressing File", Grid2RowNumber, 2);
                //compression code
                String DirectoryToZip = LocalDirectory + FileName;
                String ZipFileToCreate = LocalDirectory + FileName + ".zip";


                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
                {
                    if (CancelProg == true)
                    {
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                        return;

                    }
                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Default;
                    zip.SaveProgress += (object sender, SaveProgressEventArgs e) => SaveProgress(sender, e, Grid2RowNumber);
                    zip.StatusMessageTextWriter = System.Console.Out;
                    zip.AddFile(DirectoryToZip, ""); // recurses subdirectories
                    zip.Save(ZipFileToCreate);
                }
                //logs activity
                lock (_object)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {
                        file.WriteLine("[" + DateTime.Now + "] " + "File Compression Finished");
                    }
                }
                //logs activity
                UpdateStatusBar("[" + DateTime.Now + "] " + "File Compression Finished" + "");
                lock (_object)
                {
                    UpdateGrid2(100, Grid2RowNumber, 8);
                }
                //********************************************************************************************
                // end of compression code
                //********************************************************************************************

                if (CancelProg == true)
                {
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                    return;

                }
                //********************************************************************************************
                //deletes the raw file in the temp folder
                //********************************************************************************************

                lock (_object)
                {
                    //file delete code
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {

                        //logs activity
                        file.WriteLine("[" + DateTime.Now + "] " + "Deleting Raw & RNX File from Temp Folder");
                        //logs activity
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Deleting Raw & RNX File from Temp Folder" + "");
                        //delete code/command 
                        File.Delete(LocalDirectory + FileName);
                        Directory.Delete(LocalDirectory + FileName.Substring(0,8) + ".RNX",true);
                        //logs activity
                        UpdateStatusBar("[" + DateTime.Now + "] " + "File Raw & RNX File Deletion Completed" + "");
                        //logs activity
                        file.WriteLine("[" + DateTime.Now + "] " + "File Raw & RNX File Deletion Completed");
                    }
                }

                if (CancelProg == true)
                {
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                    return;

                }
                //********************************************************************************************
                //end of deletion code
                //********************************************************************************************

                if (CancelProg == true)
                {
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                    return;

                }

                //********************************************************************************************
                //uploads the compress file into the ftp server
                //********************************************************************************************
                lock (_object)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {
                        file.WriteLine("[" + DateTime.Now + "] " + "File Uploading Started.");
                    }
                }
                string LocalDirectory1 = @"C:\Temp\" + FileName + ".zip";
                string LocalDirectory2 = @"C:\Temp\" + FileName.Substring(0,8) + ".RNX.zip";
                //string User = this.textBox7.Text;
                //string Pass = this.textBox6.Text;
                //string BaseDirectory = this.textBox8.Text;

                //checks folders in the File server
                FolderChecker(currRow, File_Location, DTPCheck.Value);
                //checks folders in the RNX File Server
                FolderChecker(currRow, File_LocationRNX, DTPCheck.Value);

                string DestinationDirectory = (File_Location + DTPCheck.Value.Year.ToString()) + @"\" + DTPCheck.Value.Month.ToString("00")
                    + @"\" + DTPCheck.Value.Day.ToString("00") + 
                    @"\" + Grid.Rows[int.Parse(RowNumber)].Cells[0].Value.ToString() + @"\";  //Local directory where the files will be uploaded/copied.

                string DestinationDirectory2 = (File_LocationRNX + DTPCheck.Value.Year.ToString()) + @"\" + DTPCheck.Value.Month.ToString("00")
                    + @"\" + DTPCheck.Value.Day.ToString("00") +
                    @"\" + Grid.Rows[int.Parse(RowNumber)].Cells[0].Value.ToString() + @"\";  //Local directory where the files will be uploaded/copied for the compressed RNX files

                NetworkCredential readCredentials = new NetworkCredential(@User_ID, Password_File);
                using (new NetworkConnection(File_Location.ToString(),readCredentials))
                {
                    PageNet_AutoDownloader.CustomFileCopier fc = new PageNet_AutoDownloader.CustomFileCopier(LocalDirectory1, DestinationDirectory + FileName + ".zip", @User_ID.ToString(), Password_File.ToString() ,File_Location.ToString());
                    //fc.OnProgressChanged += filecopyprogress;
                    //fc.OnProgressChanged += filecopyprogress(Grid2RowNumber, Grid2RowNumber);
                    fc.OnProgressChanged += (double Persentage, ref bool Cancel) => filecopyprogress(Persentage, Grid2RowNumber, 9);
                    fc.OnComplete += filecopycomplete;
                    fc.Copy();
                    if (CancelProg == true)
                    {
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                        return;

                    }
                }

                using (new NetworkConnection(File_LocationRNX, readCredentials))
                {
                    PageNet_AutoDownloader.CustomFileCopier fc = new PageNet_AutoDownloader.CustomFileCopier(LocalDirectory2, DestinationDirectory2 + FileName.Substring(0, 8) + ".RNX.zip", @User_ID.ToString(), Password_File.ToString(), File_Location.ToString());
                    fc.OnProgressChanged += (double Persentage, ref bool Cancel) => filecopyprogress(Persentage, Grid2RowNumber, 9);
                    fc.OnComplete += filecopycomplete;
                    fc.Copy();
                    if (CancelProg == true)
                    {
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                        return;

                    }
                }
                //end of upload code
                if (CancelProg == true)
                {
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                    return;

                }
                lock (_object)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {
                        file.WriteLine("[" + DateTime.Now + "] " + "File Uploading Finished");
                    }
                }
                //deletes the zip file in the temp folder
                lock (_object)
                {
                    //file delete code
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {
  
                        //logs activity
                        file.WriteLine("[" + DateTime.Now + "] " + "Deleting File from Temp Folder");
                        //logs activity
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Deleting File from Temp Folder" + "");
                        //delete code/command 
                        File.Delete(LocalDirectory + FileName + ".zip");
                        File.Delete(LocalDirectory + FileName.Substring(0,8) + ".RNX.zip");

                        //logs activity
                        UpdateStatusBar("[" + DateTime.Now + "] " + "File Deletion Completed" + "");
                        //logs activity
                        file.WriteLine("[" + DateTime.Now + "] " + "File Deletion Completed");
                    }
                }

                if (CancelProg == true)
                {
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                    return;

                }
                // end of deletion code
                return;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ConnectFailure)
                {
                    //if there is no connection to the site
                    UpdateStatusBar("[" + DateTime.Now + "] " + ex.Message.ToString() + "");
                    if (CancelProg == true)
                    {
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                        return;

                    }
                }
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    if (DownCounter < 3)
                    {
                        //if the connection has a timeout
                        UpdateStatusBar("[" + DateTime.Now + "] " + ex.Message.ToString() + "");
                        UpdateStatusBar("[" + DateTime.Now + "] Retrying Download of file " + FileName + "");
                        //Update("[" + DateTime.Now + "] " + ex.Message.ToString() + "\r\n");
                        //Update("[" + DateTime.Now + "] Retrying Download of file " + FileName + "\r\n");
                        DownCounter++;
                        Downloadfile(URL, FileName, user, pass, RowNumber, Grid2RowNumber, File_Size);
                        if (CancelProg == true)
                        {
                            UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                            return;

                        }
                    }
                }
            }

        }


        public void SaveProgress(object sender, SaveProgressEventArgs e, int Grid2RowNumber)
        {
            if (e.EventType == ZipProgressEventType.Saving_BeforeWriteEntry)
            {
            }
            else if (e.EventType == ZipProgressEventType.Saving_EntryBytesRead)
            {
                //UpdateLabelCProg(((e.BytesTransferred * 100) / e.TotalBytesToTransfer).ToString());
                UpdateGrid2(Convert.ToInt32(((e.BytesTransferred * 100) / e.TotalBytesToTransfer)), Grid2RowNumber, 8);
                //labelCProg.Text = ((e.BytesTransferred * 100) / e.TotalBytesToTransfer).ToString("0.00%");
            }
        }

        public void UpdateGrid2(int x, int RowNum, int CellNum)
        {
            if (this.Grid2.InvokeRequired)
            {
                this.Grid2.Invoke(
                    new MethodInvoker(
                    delegate() { UpdateGrid2Column2(x, RowNum, CellNum); }));
            }
        }

        public void UpdateGrid2Column2(int x, int RowNum, int CellNum)
        {
            Grid2.Rows[RowNum].Cells[CellNum].Value = x;
            Grid2.Update();
            Grid2.Refresh();
        }

        public void UpdateGrid3(int x, int RowNum, int CellNum)
        {
            if (this.Grid2.InvokeRequired)
            {
                this.Grid2.Invoke(
                    new MethodInvoker(
                    delegate() { UpdateGrid3Column2(x, RowNum, CellNum); }));
            }
        }

        public void UpdateGrid3Column2(int x, int RowNum, int CellNum)
        {
            Grid2.Rows[RowNum].Cells[CellNum].Value = x;
            Grid2.Update();
        }
        public void UpdateGrid4(int x, int RowNum, int CellNum)
        {
            if (this.Grid2.InvokeRequired)
            {
                this.Grid2.Invoke(
                    new MethodInvoker(
                    delegate() { UpdateGrid4Column2(x, RowNum, CellNum); }));
            }
        }

        public void UpdateGrid4Column2(int x, int RowNum, int CellNum)
        {
            Grid2.Rows[RowNum].Cells[CellNum].Value = x;
            Grid2.Update();
        }

        public void filecopyprogress(double Persentage, int RowNum, int CellNum)
        {

            //return Persentage;
            if (this.Grid2.InvokeRequired)
            {
                this.Grid2.Invoke(
                    new MethodInvoker(
                    delegate() { UpdateProgressbar(Persentage, RowNum, CellNum); }));
            }
            //this.textBox1.Text = Persentage.ToString();
        }

        public void filecopycomplete()
        {
        }

        private void UpdateProgressbar(double percent, int RowNum, int CellNum)
        {
            Grid2.Rows[RowNum].Cells[CellNum].Value = ((int)percent);
            Grid2.Update();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoCheckOfAvailableFile(Grid2currRow);
        }


        private void Grid2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            Grid2currRow = e.RowIndex;
        }

        private void RunTEQC(string fileName, string year)
        {
            //teqc -leica mdb +nav XXX.13n,XXX.13g XXX.m00 > XXX.13o
            //teqc -leica mdb +nav PFLO001a.13n,PFLO001a.13g PFLO001a.m00 > PFLO001a.13o
            // -leica mdb +nav {FILENAME}.{YEAR}n,{FILENAME}.{YEAR}g +obs {FILENAME}.{YEAR}o {FILENAME}.m00


            // causing error on PFLO001m.m00
            //fileName = fileName.TrimEnd(".m00".ToCharArray());
            fileName = fileName.Substring(0, fileName.Length - 4);

            //string executable = System.Configuration.ConfigurationManager.AppSettings["OGR2OGR_EXE"].ToString();

            string executable = AppDomain.CurrentDomain.BaseDirectory + "teqc.exe";

            string args = TeqC_Argument;

            Debug.WriteLine("Setting Up TeqC Argument");

            args = EvaluateSubstring(args, fileName);
            args = args.Replace("{FILENAME}", fileName);
            args = args.Replace("{YEAR}", year);


            //string args = String.Format("-leica mdb +nav {0}.{1}n,{0}.{1}g +obs  {0}.{1}o {0}.m00 ", fileName, year);
            //string args = String.Format("-leica mdb +nav {0}.13n,{0}.13g +obs  {0}.13o {0}.m00 ", fileName, year);



            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = @executable;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = @args;


            startInfo.RedirectStandardError = true;



            string standardErrorMessage = "";

            // Start the process with the info we specified.
            // Call WaitForExit and then the using statement will close.
            Debug.WriteLine("Start RNX Conversion");

            using (Process exeProcess = Process.Start(startInfo))
            {
                standardErrorMessage = exeProcess.StandardError.ReadToEnd();
                exeProcess.WaitForExit();
            }
            Debug.WriteLine("End RNX Conversion");

            if (standardErrorMessage != "")
            {
                //throw new Exception(standardErrorMessage);
            }


        }

        private string EvaluateSubstring(string s, string filename)
        {
            string pattern = @"\{FILENAME}\.substring\(\d*,\d*\)";
            Regex regEx = new Regex(pattern);
            Match match = regEx.Match(s);
            foreach (Group group in match.Groups)
            {
                if (group.Value != String.Empty)
                {
                    string[] param = group.Value.Split(',');
                    int start = System.Convert.ToInt32(param[0].Replace("{FILENAME}.substring(", ""));
                    int len = System.Convert.ToInt32(param[1].Replace(")", ""));
                    s = s.Replace(group.Value, filename.Substring(start, len));
                }
            }

            return s;
        }

        public void FolderChecker(int SelectedRow,String BaseDir,DateTime Date)
        {
            lock (_object)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                {
                    //checks top level folder
                    if (CheckFolder(BaseDir.ToString()) == true)
                    {
                        //does nothing since every folder is created.
                    }
                    else
                    {
                        //creates the folder if non-existent
                        file.WriteLine("[" + DateTime.Now + "] " + "Creating Top Level Folder");
                        //Update("[" + DateTime.Now + "] " + "Creating Top Level Folder\r\n");

                        TopCreateFolder(BaseDir.ToString());
                    }

                    //checks year level folder
                    if (CheckFolder(BaseDir.ToString() + DateTime.Now.Year) == true)
                    {
                        //does nothing since every folder is created.
                    }
                    else
                    {
                        //creates the folder if non-existent
                        file.WriteLine("[" + DateTime.Now + "] " + "Creating Year Folder");
                        //Update("[" + DateTime.Now + "] " + "Creating Year Folder\r\n");

                        CreateFolder(BaseDir.ToString(), Date.Year.ToString());
                    }

                    //checks month level folder
                    if (CheckFolder(BaseDir.ToString() + Date.Year + @"\" + Date.Month.ToString("00")) == true)
                    {
                        //does nothing since every folder is created.
                    }
                    else
                    {
                        //creates the folder if non-existent
                        //Update("[" + DateTime.Now + "] " + "Creating Month Folder\r\n");

                        file.WriteLine("[" + DateTime.Now + "] " + "Creating Month Folder");
                        CreateFolder((BaseDir.ToString() + Date.Year.ToString()), Date.Month.ToString("00"));
                    }

                    //checks day level folder
                    if (CheckFolder((BaseDir.ToString() + Date.Year + @"\" + Date.Month.ToString("00") + @"\" + Date.Day.ToString("00"))) == true)
                    {
                        //does nothing since every folder is created.
                    }
                    else
                    {
                        //creates the folder if non-existent
                        file.WriteLine("[" + DateTime.Now + "] " + "Creating Day Folder");
                        //Update("[" + DateTime.Now + "] " + "Creating Day Folder\r\n");
                        CreateFolder((BaseDir.ToString() + Date.Year.ToString()) + @"\" + Date.Month.ToString("00"), Date.Day.ToString("00"));
                    }

                    //checks station level folder
                    if (CheckFolder(BaseDir.ToString() + Date.Year + @"\" + Date.Month.ToString("00") + @"\" + Date.Day.ToString("00") + @"\" + Grid.Rows[SelectedRow].Cells[0].Value.ToString()) == true)
                    {
                        //does nothing since every folder is created.
                    }
                    else
                    {
                        //creates the folder if non-existent
                        file.WriteLine("[" + DateTime.Now + "] " + "Creating Station Folder");
                        //Update("[" + DateTime.Now + "] " + "Creating Station Folder\r\n");
                        CreateFolder((BaseDir.ToString() + Date.Year.ToString()) + @"\" + Date.Month.ToString("00") + @"\" + Date.Day.ToString("00"), Grid.Rows[SelectedRow].Cells[0].Value.ToString());
                    }
                }
            }
        }

        //this checks if the folder exists
        Boolean CheckFolder(string FolderPath)
        {
            Boolean Checker;
            if (Directory.Exists(FolderPath) == false)
            {
                Checker = false;
            }
            else
            {
                Checker = true;
            }
            return Checker;
        }

        //checks tops most folder if exists
        public static void TopCreateFolder(string path)
        {
            try
            {
                // Specify a name for your top-level folder. 
                string folderName = @path;

                // To create a string that specifies the path to a subfolder under your  
                // top-level folder, add a name for the subfolder to folderName. 
                //string pathString = System.IO.Path.Combine(folderName, "SubFolder");

                System.IO.Directory.CreateDirectory(folderName);
            }
            catch
            {
                return;
            }
        }

        //create folder command are here
        public static void CreateFolder(string path, string FolderName)
        {
            try
            {
                // Specify a name for your top-level folder. 
                string TopLevelFolder = @path;

                // To create a string that specifies the path to a subfolder under your  
                // top-level folder, add a name for the subfolder to folderName. 
                string pathString = System.IO.Path.Combine(TopLevelFolder, FolderName);

                System.IO.Directory.CreateDirectory(pathString);
            }
            catch
            {
                return;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to stop all the current running process(es)?", "Stop Process", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                CancelProg = true;
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CancelProg = true;
        }

        private void Grid2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }


    }
}

namespace Sample
{
    public class DataGridViewProgressColumn : DataGridViewImageColumn
    {
        public DataGridViewProgressColumn()
        {
            CellTemplate = new DataGridViewProgressCell();
        }
    }
}
namespace Sample
{
    class DataGridViewProgressCell : DataGridViewImageCell
    {
        // Used to make custom cell consistent with a DataGridViewImageCell
        static Image emptyImage;
        static DataGridViewProgressCell()
        {
            emptyImage = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }
        public DataGridViewProgressCell()
        {
            this.ValueType = typeof(int);
        }
        // Method required to make the Progress Cell consistent with the default Image Cell. 
        // The default Image Cell assumes an Image as a value, although the value of the Progress Cell is an int.
        protected override object GetFormattedValue(object value,
                            int rowIndex, ref DataGridViewCellStyle cellStyle,
                            TypeConverter valueTypeConverter,
                            TypeConverter formattedValueTypeConverter,
                            DataGridViewDataErrorContexts context)
        {
            return emptyImage;
        }
        protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle clipBounds, System.Drawing.Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            try
            {
                int progressVal = (int)value;
                float percentage = ((float)progressVal / 100.0f); // Need to convert to float before division; otherwise C# returns int which is 0 for anything but 100%.
                Brush backColorBrush = new SolidBrush(cellStyle.BackColor);
                Brush foreColorBrush = new SolidBrush(cellStyle.ForeColor);
                // Draws the cell grid
                base.Paint(g, clipBounds, cellBounds,
                 rowIndex, cellState, value, formattedValue, errorText,
                 cellStyle, advancedBorderStyle, (paintParts & ~DataGridViewPaintParts.ContentForeground));
                if (percentage > 0.0)
                {
                    // Draw the progress bar and the text
                    g.FillRectangle(new SolidBrush(Color.FromArgb(203, 235, 108)), cellBounds.X + 2, cellBounds.Y + 2, Convert.ToInt32((percentage * cellBounds.Width - 4)), cellBounds.Height - 4);
                    g.DrawString(progressVal.ToString() + "%", cellStyle.Font, foreColorBrush, cellBounds.X + (cellBounds.Width / 2) - 5, cellBounds.Y + 2);

                }
                else
                {
                    // draw the text
                    if (this.DataGridView.CurrentRow.Index == rowIndex)
                        g.DrawString(progressVal.ToString() + "%", cellStyle.Font, new SolidBrush(cellStyle.SelectionForeColor), cellBounds.X + 6, cellBounds.Y + 2);
                    else
                        g.DrawString(progressVal.ToString() + "%", cellStyle.Font, foreColorBrush, cellBounds.X + 6, cellBounds.Y + 2);
                }
            }
            catch (Exception e) {
                //MessageBox.Show(e.ToString());
            }

        }
    }
}