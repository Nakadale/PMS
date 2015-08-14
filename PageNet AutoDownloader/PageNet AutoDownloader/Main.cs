using System;
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
using Shared_Folder_Login;
using Ionic.Zip;
using Ionic.Zlib;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using Sample;
namespace PageNet_AutoDownloader
{
    public partial class Main : Form
    {
        static readonly object _object = new object();

        String StrDate = DateTime.Now.Year + "_" + DateTime.Now.Month.ToString("00") + "_" + DateTime.Now.Day.ToString("00");
        bool CancelProg = false;
        List<FTPFiles> ftpfiles = new List<FTPFiles>(); // for storing of file list coming from the site
        int[] DownCounter = new int[25]; //download counter for downloadfile if file failed to download
        int Triggered = 0; // trigger for starting of checking of site and downloading of files for the site. 
        private string _SourceDirectory = "";
        private string _DestinationDirectory = "";
        String File_Location, User_ID, Password_File, File_LocationRNX, User_IDRNX, PasswordRNX;
        String Teqc_Argument;

        // SQLite Connection Parameters

        private SQLiteConnection sql_con = new SQLiteConnection(ConfigurationManager.ConnectionStrings["PageNet_AutoDownloader.Properties.Settings.sql_con"].ConnectionString);
        private SQLiteCommand sql_cmd;
        private SQLiteDataAdapter DBMain;
        private DataSet DSMain = new DataSet();
        private DataTable DTMain = new DataTable();
        //==============================

        public void LoadData()
        {
            // loads all station information from DB.
            try
            {
                sql_con.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

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
                startBotToolStripMenuItem.Enabled = false;
            }
        }

        public Main()
        {
            // standard default form
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Closes/exits the program
            CancelProg = true;
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //minimizes the form.
            WindowState = FormWindowState.Minimized;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // displays the date and time. gets computer time.
            DateTime saveNow = DateTime.Now;
            lblDate.Text = saveNow.ToShortDateString();
            lbldate1.Text = saveNow.Subtract(TimeSpan.FromDays(1)).ToShortDateString();
            //lblDate.Text = saveNow.ToShortDateString();
            lblTime.Text = saveNow.ToShortTimeString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // starts the timer/scheduler
            DTPTimeSched.Enabled = false;
            timer2.Enabled = true;
            //timer3.Enabled = true;
            CancelProg = false;
            stopBotToolStripMenuItem.Enabled = true;
            startBotToolStripMenuItem.Enabled = false;
            button1.Enabled = false; // Start Button
            button2.Enabled = true; // Stop Button
            manualDownloadToolStripMenuItem.Enabled = false;
            settingsToolStripMenuItem.Enabled = false;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to stop the scheduler?", "Stop Scheduler", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.No)
            {
                return;
            }

            if (Triggered == 0)
            {
                DTPTimeSched.Enabled = true;
                stopBotToolStripMenuItem.Enabled = false;
                startBotToolStripMenuItem.Enabled = true;
                button1.Enabled = true; // Start Button
                button2.Enabled = false; // Stop Button
                manualDownloadToolStripMenuItem.Enabled = true;
                settingsToolStripMenuItem.Enabled = true;
                timer2.Enabled = false;

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                {
                    file.WriteLine("[" + DateTime.Now + "] " + "Scheduler Process Ended.");
                }
                Update("[" + DateTime.Now + "] " + "Scheduler Process Ended." + "\r\n");
                UpdateStatusBar("[" + DateTime.Now + "] " + "Scheduler Process Ended." + "\r\n");

                MessageBox.Show("Scheduler Process Ended.");
                Triggered = 0;
                CancelProg = false;
            }
            else
            {
                CancelProg = true;
            }

            UpdateStatusBar("[" + DateTime.Now + "] " + "Cancelling Process. Please wait.");
        }

        public void LoadDesti()
        {
            sql_con.Open();
            string CommandText = "Select * from DestinationServer";
            SQLiteCommand command = new SQLiteCommand(CommandText, sql_con);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                File_Location = reader["File_Location"].ToString();
                User_ID = reader["User_ID"].ToString();
                Password_File = reader["Password"].ToString();

            }

            CommandText = "Select * from DestinationServerRNX";
            command = new SQLiteCommand(CommandText, sql_con);
            reader = command.ExecuteReader();

            while (reader.Read())
            {
                File_LocationRNX = reader["File_Location"].ToString();
                User_IDRNX = reader["User_ID"].ToString();
                PasswordRNX = reader["Password"].ToString();

            }

            CommandText = "Select * from TeqC";
            command = new SQLiteCommand(CommandText, sql_con);
            reader = command.ExecuteReader();

            while (reader.Read())
            {
                Teqc_Argument = reader["TeqC_Argument"].ToString();
            }

            sql_con.Close();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //calls LoadData method
            LoadData();

            //loads file server information
            LoadDesti();

            //DTPTimeSched.Value = DateTime.Now;
            DataGridViewProgressColumn column = new DataGridViewProgressColumn(); //download
            DataGridViewProgressColumn column1 = new DataGridViewProgressColumn(); //convert
            DataGridViewProgressColumn column2 = new DataGridViewProgressColumn(); //compress
            DataGridViewProgressColumn column3 = new DataGridViewProgressColumn(); //upload
            DataGridViewTextBoxColumn column4 = new DataGridViewTextBoxColumn(); //checker whether row is being processed

            column.HeaderText = "Download Progress (%)";

            Grid2.Columns.Add(column);

            column1.HeaderText = "Conversion Progress (%)";

            Grid2.Columns.Add(column1);

            column2.HeaderText = "Compress Progress (%)";

            Grid2.Columns.Add(column2);

            column3.HeaderText = "Upload Progress (%)";

            Grid2.Columns.Add(column3);

            column4.HeaderText = "Started";
            //acolumn4.Visible = false;
            column4.Name = "started";
            Grid2.Columns.Add(column4);

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (CancelProg == true && Triggered == 1)
            {
                DTPTimeSched.Enabled = true;
                stopBotToolStripMenuItem.Enabled = false;
                startBotToolStripMenuItem.Enabled = true;
                button1.Enabled = true; // Start Button
                button2.Enabled = false; // Stop Button
                manualDownloadToolStripMenuItem.Enabled = true;
                settingsToolStripMenuItem.Enabled = true;
                timer2.Enabled = false;
                CancelProg = false;
                lock (_object)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {
                        file.WriteLine("[" + DateTime.Now + "] " + "Scheduler Process Ended.");
                    }
                }
                Update("[" + DateTime.Now + "] " + "Scheduler Process Ended." + "\r\n");
                UpdateStatusBar("[" + DateTime.Now + "] " + "Scheduler Process Ended." + "\r\n");

                MessageBox.Show("Scheduler Process Ended.");
                Triggered = 0;
            }

            // checks time if equivalent to the time set in the schedule and checks Triggered if it was triggered once.
            if (lblTime.Text == DTPTimeSched.Value.ToShortTimeString() && Triggered == 0)
            {
                //tester code for threading
                if (CancelProg == true || timer2.Enabled == false)
                {
                    UpdateClose();
                    return;
                }
                Triggered = 1;
                TSSFile.Text = "Start Automatic File Comparison";
                Thread WorkLoad = new Thread(new ThreadStart(DoWork));
                WorkLoad.Name = "Test";
                WorkLoad.IsBackground = true;
                WorkLoad.Start();
            }
            //Resets 5 minutes before comparison operation again
            if (lblTime.Text == DTPTimeSched.Value.AddMinutes(-5).ToShortTimeString() && Triggered == 1)
            {
                Triggered = 0;
                TxtLogs.Text = "";
                TSStatus.Text = "";
                //Grid2.Rows.Clear();
            }

        }


        // connects to site and try to get all file information
        public void GetDataFromSite(int CurrRow)
        {
            lock (_object)
            {
                try
                {
                    int FileCounter = 0;
                    // Get the object used to communicate with the server.
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(Grid.Rows[CurrRow].Cells[1].Value.ToString()));

                    request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                    request.Proxy = null;
                    request.Timeout = 30000;

                    request.Credentials = new NetworkCredential(Grid.Rows[CurrRow].Cells[2].Value.ToString(), Grid.Rows[CurrRow].Cells[3].Value.ToString());
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                    Stream responseStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(responseStream);
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Connected to Station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + "");
                    Update("[" + DateTime.Now + "] " + "Connected to Station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + "\r\n");

                    string JulianDate = DateTime.Now.Subtract(TimeSpan.FromDays(1)).DayOfYear.ToString("000");

                    //string JulianDate = DateTime.Now.DayOfYear.ToString("000");
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Getting File(s) Information");
                    Update("[" + DateTime.Now + "] " + "Getting File(s) Information\r\n");

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        var varsam = line.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        FTPFiles ftptemp = new FTPFiles();

                        if (varsam[8].Length > 8)
                        {

                            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                            //{
                            //    file.WriteLine("[" + DateTime.Now + "] " + "JulianDate: " + JulianDate + " varsam[8]: " + varsam[8] + " varsam[8].Substring(4, 3)): " + varsam[8].Substring(4, 3) + " varsam[8].Substring(9, 1)):" + varsam[8].Substring(9, 1));
                            //}

                            if ((JulianDate == varsam[8].Substring(4, 3)) && (varsam[8].Substring(9, 1) == "m"))
                            {
                                ftptemp.FileName = varsam[8].ToString();
                                ftptemp.FileBytes = long.Parse(varsam[4].ToString());
                                ftpfiles.Add(ftptemp);
                                FileCounter = FileCounter + 1;
                            }
                        }
                    }
                    UpdateStatusBar("[" + DateTime.Now + "] " + FileCounter + " File(s) Information collected.");
                    Update("[" + DateTime.Now + "] " + FileCounter + " File(s) Information collected.\r\n");

                    responseStream.Close();
                }

                catch (WebException ex)
                {
                    //MessageBox.Show(ex.Message.ToString());
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                        {
                            file.WriteLine("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established. Invalid User Name or Password.");
                        }
                        Update("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established. Invalid User Name or Password." + "\r\n");
                        return;
                    }

                    if (ex.Status == WebExceptionStatus.ConnectFailure)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                        {
                            file.WriteLine("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established.");
                        }
                        Update("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established." + "\r\n");
                        return;
                    }
                }

            }
        }

        //downloads Files that are not present in the Destination Server
        public void Downloadfile(string URL, string FileName, string user, string pass, string RowNumber, int Grid2RowNumber, string File_Size)
        {
            //long FileSize = 0;
            string UserName = user;              //User Name of the FTP server
            string Password = pass;              //Password of the FTP server

            string LocalDirectory = "C:\\Temp\\";  //Local directory where the files will be downloaded

            //UpdateGrid2("Downloading File", Grid2RowNumber,2);

            if (CancelProg == true)
            {
                UpdateClose(); return;
            }
            //download code
            if (File.Exists(URL) == false)
            {
                Download(URL, FileName, user, pass, File_Size, Grid2RowNumber);
            }
            else
            {
                //for retry download

            }

            try
            {
                if (Grid2.Rows[Grid2RowNumber].Cells[7].Value.ToString() == "100")
                {
                    //********************************************************************************************
                    //start conversion protocol
                    //********************************************************************************************

                    UpdateGrid2(Convert.ToInt32(0), Grid2RowNumber, 8);

                    String targetPath = @"C:\Temp\" + FileName.Substring(0, 8) + ".RNX";
                    string sourceFile = System.IO.Path.Combine(@"C:\Temp\", FileName);
                    string destFile = System.IO.Path.Combine(targetPath, FileName);

                    Directory.CreateDirectory(@"C:\Temp\" + FileName.Substring(0, 8) + ".RNX");
                    File.Copy(sourceFile, destFile);
                    //Grid2.Rows[Grid2RowNumber].Cells[7].Value.ToString().Substring(0, 4);


                    //logs activity
                    lock (_object)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                        {
                            file.WriteLine("[" + DateTime.Now + "] " + "Conversion of RAW File to Rinex Started");
                            Update("[" + DateTime.Now + "] " + "Conversion of RAW File to Rinex Started\r\n");

                        }
                    }
                    //logs activity
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Conversion of RAW File to Rinex Started" + "\r\n");

                    RunTEQC(destFile, Grid2.Rows[Grid2RowNumber].Cells[6].Value.ToString().Substring(0, 4));

                    //logs activity
                    lock (_object)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                        {
                            file.WriteLine("[" + DateTime.Now + "] " + "Conversion of RAW File to Rinex Finished");
                            Update("[" + DateTime.Now + "] " + "Conversion of RAW File to Rinex Finished\r\n");

                        }
                    }
                    //logs activity
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Conversion of RAW File to Rinex Finished" + "\r\n");


                    File.Delete(destFile);

                    using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
                    {
                        if (CancelProg == true)
                        {
                            UpdateClose(); return;
                        }
                        String Target = targetPath + @"\" + FileName.Substring(0, 8) + "." + DateTime.Now.Year.ToString();
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Default;
                        //zip.SaveProgress += (object sender, SaveProgressEventArgs e) => SaveProgress(sender, e, Grid2RowNumber);
                        zip.StatusMessageTextWriter = System.Console.Out;
                        zip.AddFile(Target + "g", "");
                        zip.AddFile(Target + "n", "");
                        zip.AddFile(Target + "o", "");
                        zip.Save(LocalDirectory + FileName.Substring(0, 8) + ".RNX.zip");
                    }
                    UpdateGrid2(Convert.ToInt32(100), Grid2RowNumber, 8);
                    //********************************************************************************************
                    // calls the conversion protocol
                    //********************************************************************************************

                    //********************************************************************************************
                    //compresses the download raw file
                    //********************************************************************************************

                    if (CancelProg == true)
                    {
                        UpdateClose(); return;
                    }
                    UpdateGrid2(0, Grid2RowNumber, 9);

                    //logs activity
                    lock (_object)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                        {
                            file.WriteLine("[" + DateTime.Now + "] " + "Compressing File");
                            Update("[" + DateTime.Now + "] " + "Compressing File\r\n");

                        }
                    }
                    //logs activity
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Compressing File" + "\r\n");
                    Update("[" + DateTime.Now + "] " + "Compressing File\r\n");
                    //logs activity
                    //UpdateGrid2("Compressing File", Grid2RowNumber, 2);
                    //compression code
                    UpdateGrid2(0, Grid2RowNumber, 9);

                    String DirectoryToZip = LocalDirectory + FileName;
                    String ZipFileToCreate = LocalDirectory + FileName + ".zip";

                    using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
                    {
                        if (CancelProg == true)
                        {
                            UpdateClose(); return;
                        }
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Default;
                        zip.SaveProgress += (object sender, SaveProgressEventArgs e) => SaveProgress(sender, e, Grid2RowNumber);
                        zip.StatusMessageTextWriter = System.Console.Out;
                        zip.AddFile(DirectoryToZip, ""); // recurses subdirectories
                        zip.Save(ZipFileToCreate);
                    }


                    UpdateGrid2(100, Grid2RowNumber, 9);

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
                    Update("[" + DateTime.Now + "] " + "File Compression Finished" + "\r\n");

                    //********************************************************************************************
                    // end of compression code
                    //********************************************************************************************
                    if (CancelProg == true)
                    {
                        UpdateClose(); return;
                    }
                    //deletes the raw file in the temp folder
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
                            Update("[" + DateTime.Now + "] " + "Deleting File from Temp Folder" + "\r\n");

                            File.Delete(LocalDirectory + FileName);
                            Directory.Delete(LocalDirectory + FileName.Substring(0, 8) + ".RNX", true);
                            //logs activity
                            UpdateStatusBar("[" + DateTime.Now + "] " + "File Deletion Completed" + "\r\n");
                            Update("[" + DateTime.Now + "] " + "File Deletion Completed" + "\r\n");

                            //logs activity
                            file.WriteLine("[" + DateTime.Now + "] " + "File Deletion Completed");
                        }
                    }
                    if (CancelProg == true)
                    {
                        UpdateClose(); return;
                    }
                    
                }

                //end of deletion code
                //Thread.Sleep(500);
                //***************************************************************************************
                //uploads the compress file into the ftp server
                //***************************************************************************************
                if (Grid2.Rows[Grid2RowNumber].Cells[9].Value.ToString() == "100")
                {
                    UploadData(FileName, Grid2RowNumber, RowNumber, Grid2.Rows[Grid2RowNumber].Cells[6].Value.ToString());
                    //check again also
                }
                //***************************************************************************************
                //end of upload code
                //***************************************************************************************

                if (CancelProg == true)
                {
                    UpdateClose(); return;
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
                        Update("[" + DateTime.Now + "] " + "Deleting File from Temp Folder" + "\r\n");

                        //delete code/command 
                        File.Delete(LocalDirectory + FileName + ".zip");
                        File.Delete(LocalDirectory + FileName.Substring(0, 8) + ".RNX.zip");

                        //logs activity
                        UpdateStatusBar("[" + DateTime.Now + "] " + "File Deletion Completed");
                        Update("[" + DateTime.Now + "] " + "File Deletion Completed\r\n");
                        //logs activity
                        file.WriteLine("[" + DateTime.Now + "] " + "File Deletion Completed");
                    }
                }

                if (CancelProg == true)
                {
                    UpdateClose(); return;

                }
                // end of deletion code

                //if (Grid2RowNumber != Grid2.RowCount)
                //{
                //int counter = GetNextRow(Grid2RowNumber);
                //DoCheckOfAvailableFile((counter));
                //}
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Status.ToString());
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    //if the supplied password or username is wrong
                    UpdateStatusBar("[" + DateTime.Now + "] " + "User Name or Password is invalid for site " + Grid.Rows[int.Parse(RowNumber)].Cells[0].Value.ToString());
                    Update("[" + DateTime.Now + "] " + "User Name or Password is invalid for site " + Grid.Rows[int.Parse(RowNumber)].Cells[0].Value.ToString() + "\r\n");

                    if (CancelProg == true)
                    {
                        UpdateClose(); return;
                    }
                }
                if (ex.Status == WebExceptionStatus.ConnectFailure)
                {
                    //if there is no connection to the site
                    UpdateStatusBar("[" + DateTime.Now + "] " + ex.Message.ToString());
                    if (CancelProg == true)
                    {
                        UpdateClose(); return;

                    }
                }
            }

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

        public void DeleteFile(string path)
        {
            lock (_object)
            {
                //file delete code
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                {
                    if (CancelProg == true)
                    {
                        UpdateClose(); return;
                    }
                    //logs activity
                    file.WriteLine("[" + DateTime.Now + "] " + "Deleting File from Temp Folder");
                    //logs activity
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Deleting File from Temp Folder" + "");
                    Update("[" + DateTime.Now + "] " + "Deleting File from Temp Folder" + "\r\n");

                    //delete code/command 
                    File.Delete(path);
                    //logs activity
                    UpdateStatusBar("[" + DateTime.Now + "] " + "File Deletion Completed");
                    Update("[" + DateTime.Now + "] " + "File Deletion Completed\r\n");

                    //logs activity
                    file.WriteLine("[" + DateTime.Now + "] " + "File Deletion Completed");
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

        private void button6_Click(object sender, EventArgs e)
        {
            //tester code for threading
            TSSFile.Text = "Start Automatic File Comparison";
            Thread WorkLoad = new Thread(new ThreadStart(DoWork));
            WorkLoad.Name = "Test";
            WorkLoad.Start();
            //this.DoWork.RunWorkerAsync();
            CancelProg = false;

        }

        //checks for folder existence in the destination/file server
        public void FolderChecker(int SelectedRow, String BaseDir)
        {
            lock (_object)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                {
                    //checks top level folder
                    if (CheckFolder(BaseDir) == true)
                    {
                        //does nothing since every folder is created.
                    }
                    else
                    {
                        //creates the folder if non-existent
                        file.WriteLine("[" + DateTime.Now + "] " + "Creating Top Level Folder");
                        Update("[" + DateTime.Now + "] " + "Creating Top Level Folder\r\n");

                        TopCreateFolder(BaseDir);
                    }

                    //checks year level folder
                    if (CheckFolder(BaseDir + DateTime.Now.Year) == true)
                    {
                        //does nothing since every folder is created.
                    }
                    else
                    {
                        //creates the folder if non-existent
                        file.WriteLine("[" + DateTime.Now + "] " + "Creating Year Folder");
                        Update("[" + DateTime.Now + "] " + "Creating Year Folder\r\n");

                        CreateFolder(BaseDir, DateTime.Now.Year.ToString());
                    }

                    //checks month level folder
                    if (CheckFolder(BaseDir + DateTime.Now.Year + @"\" + DateTime.Now.Month.ToString("00")) == true)
                    {
                        //does nothing since every folder is created.
                    }
                    else
                    {
                        //creates the folder if non-existent
                        Update("[" + DateTime.Now + "] " + "Creating Month Folder\r\n");

                        file.WriteLine("[" + DateTime.Now + "] " + "Creating Month Folder");
                        CreateFolder((BaseDir + DateTime.Now.Year.ToString()), DateTime.Now.Month.ToString("00"));
                    }

                    //checks day level folder
                    if (CheckFolder((BaseDir + DateTime.Now.Year + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Subtract(TimeSpan.FromDays(1)).Day.ToString("00"))) == true)
                    {
                        //does nothing since every folder is created.
                    }
                    else
                    {
                        //creates the folder if non-existent
                        file.WriteLine("[" + DateTime.Now + "] " + "Creating Day Folder");
                        Update("[" + DateTime.Now + "] " + "Creating Day Folder\r\n");
                        CreateFolder((BaseDir + DateTime.Now.Year.ToString()) + @"\" + DateTime.Now.Month.ToString("00"), DateTime.Now.Subtract(TimeSpan.FromDays(1)).Day.ToString("00"));
                    }

                    //checks station level folder
                    if (CheckFolder(BaseDir + DateTime.Now.Year + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Subtract(TimeSpan.FromDays(1)).Day.ToString("00") + @"\" + Grid.Rows[SelectedRow].Cells[0].Value.ToString()) == true)
                    {
                        //does nothing since every folder is created.
                    }
                    else
                    {
                        //creates the folder if non-existent
                        file.WriteLine("[" + DateTime.Now + "] " + "Creating Station Folder");
                        Update("[" + DateTime.Now + "] " + "Creating Station Folder\r\n");
                        CreateFolder((BaseDir + DateTime.Now.Year.ToString()) + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Subtract(TimeSpan.FromDays(1)).Day.ToString("00"), Grid.Rows[SelectedRow].Cells[0].Value.ToString());
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

        //this code does the calling of getting date from site, checking if file exists in the destination server, starts the download method.
        public void DoWork()
        {
            for (int Counter = 0; Counter < Grid.RowCount; Counter++)
            {

                UpdateStatusBar("[" + DateTime.Now + "] " + " Connecting to Station: " + Grid.Rows[Counter].Cells[0].Value.ToString() + "");
                Update("[" + DateTime.Now + "] " + " Connecting to Station: " + Grid.Rows[Counter].Cells[0].Value.ToString() + "\r\n");

                if (CancelProg == true)
                {
                    UpdateClose(); return;

                }
                GetDataFromSite(Counter);

                //checks if folder in the destination server exists. if not exist it will create folders required before continuing.
                if (CancelProg == true)
                {
                    UpdateClose(); return;
                }


                FolderChecker(Counter, File_Location);

                FolderChecker(Counter, File_LocationRNX);

                lock (_object)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {

                        foreach (FTPFiles i in ftpfiles)
                        {
                            string FileLoc = File_Location + DateTime.Now.Year.ToString() + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Subtract(TimeSpan.FromDays(1)).Day.ToString("00") + @"\" + Grid.Rows[Counter].Cells[0].Value.ToString() + @"\" + i.FileName.Substring(0, 12).ToString() + ".zip";
                            //File exist code is in the if statement
                            if (File.Exists(@FileLoc) == false)
                            {
                                //adds filename to File List Grid
                                UpdateGrid(i.FileName.Substring(0, 12).ToString(), i.FileBytes, Grid.Rows[Counter].Cells[1].Value.ToString(), Grid.Rows[Counter].Cells[2].Value.ToString(), Grid.Rows[Counter].Cells[3].Value.ToString(), Counter, (DateTime.Now.Year.ToString() + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Subtract(TimeSpan.FromDays(1)).Day.ToString("00")));
                                //logs activity
                                file.WriteLine("[" + DateTime.Now + "] " + i.FileName.Substring(0, 12).ToString() + " File Does not exist in the destination server!");
                                //logs activity
                                UpdateStatusBar("[" + DateTime.Now + "] " + i.FileName.Substring(0, 12).ToString() + " File Does not exist in the destination server!" + "");
                                Update("[" + DateTime.Now + "] " + i.FileName.Substring(0, 12).ToString() + " File Does not exist in the destination server!" + "\r\n");

                                //logs activity
                                file.WriteLine("[" + DateTime.Now + "] " + "File Size:" + i.FileBytes);
                                //logs activity
                                UpdateStatusBar("[" + DateTime.Now + "] " + "File Size:" + i.FileBytes + "");

                            }
                        }
                    }
                }

                if (Grid2.RowCount == 0)
                {
                    lock (_object)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                        {
                            Update("[" + DateTime.Now + "] All files are downloaded.\r\n");
                            //timer3.Enabled = false;

                            //logs activity
                            file.WriteLine("[" + DateTime.Now + "] All files are downloaded.");
                            //logs activity
                            UpdateStatusBar("[" + DateTime.Now + "] All files are downloaded.");

                        }
                    }
                }

                //clears file list grid and ftpfiles where the file list from one site are stored.
                ftpfiles.Clear();
            }

            if (Grid2.RowCount != 0)
            {
                DoCheckOfAvailableFile(0);
                Thread.Sleep(100);
                DoCheckOfAvailableFile(1);
            }


            if (CancelProg == true)
            {
                // stops the timer/scheduler
                UpdateClose(); return;
            }

        }

        public void DoCheckOfAvailableFile(object num)
        {
            lock (_object)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                {

                    //logs activity
                    file.WriteLine("[" + DateTime.Now + "] " + "Checking file if Process is finished.");
                    //logs activity
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Checking file if Process is finished.");
                    Update("[" + DateTime.Now + "] " + "Checking file if Process is finished." + "\r\n");
                }
            }
            if (CancelProg == true)
            {
                UpdateClose(); return;
            }
            bool checkend = false;// variable if row count reaches end
            int row = -1;
            int.TryParse(num.ToString(), out row);

            if (row == -1)
            {
                checkend = true;
            }
            else
            {
                for (int y = row; y < Grid2.RowCount; y++)
                {
                    if ((Grid2.Rows[y].Cells[7].Value.ToString() == "0") && (Grid2.Rows[y].Cells[8].Value.ToString() == "0") && (Grid2.Rows[y].Cells[9].Value.ToString() == "0") && (Grid2.Rows[y].Cells[10].Value.ToString() == "0") && (Grid2.Rows[y].Cells[11].Value.ToString() == "1"))
                    {
                        row = y;
                        break;
                    }

                }
            }
            if (checkend == false)
            {
                ParameterizedThreadStart NPT = new ParameterizedThreadStart(DoDownloadFile);
                Thread T1 = new Thread(NPT);
                T1.Start(row);
                DownCounter[row] = 0;
            }
            else
            {
                DoCheckOfAvailableFile(0);
            }
        }

        public void DoDownloadFile(object Counter)
        {
            int x = 0;
            int.TryParse(Counter.ToString(), out x);
            if (x == -1)
            {
                x = 0;
            }
            if (CancelProg == true)
            {
                UpdateClose(); return;
            }
            //DownCounter[x] = 0;
            try
            {
                int counter = 0;
                int.TryParse(Counter.ToString(), out counter);

                if (counter == -1)
                {
                    counter = 0;
                }

                if (CancelProg == true)
                {
                    UpdateClose(); return;
                }

                if (Grid2.Rows[counter].Cells[11].Value.ToString() == "1")
                {
                    lock (_object)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                        {
                            //logs activity
                            //file.WriteLine("[" + DateTime.Now + "] " + "Downloading File: " + Grid2.Rows[counter].Cells[0].Value.ToString());
                            //logs activity
                            //UpdateStatusBar("[" + DateTime.Now + "] " + "Downloading File: " + Grid2.Rows[counter].Cells[0].Value.ToString() + "");
                            Update("[" + DateTime.Now + "] " + "Downloading File: " + Grid2.Rows[counter].Cells[0].Value.ToString() + "\r\n");

                        }
                    }
                    if (CancelProg == false && Triggered == 1)
                    {
                        UpdateGrid2(0, counter, 11);

                        Downloadfile(
                                Grid2.Rows[counter].Cells[2].Value.ToString(),//url
                                Grid2.Rows[counter].Cells[0].Value.ToString(),//filename
                                Grid2.Rows[counter].Cells[3].Value.ToString(),//user
                                Grid2.Rows[counter].Cells[4].Value.ToString(),//pass
                                Grid2.Rows[counter].Cells[5].Value.ToString(),//rownumbergrid1
                                counter, //rownumbergrid2
                                Grid2.Rows[counter].Cells[1].Value.ToString());//file size;

                        counter = GetNextRow(counter);
                        Debug.WriteLine(counter);
                        if (counter != -1)
                        {
                            DoDownloadFile(counter);
                        }
                        else
                        {
                            DoDownloadFile(counter);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                if (DownCounter[x] < 3)
                {
                    UpdateStatusBar("[" + DateTime.Now + "] Retrying Download of file " +  Grid2.Rows[x].Cells[0].Value.ToString());

                    Update("[" + DateTime.Now + "] Retrying Download of file " +  Grid2.Rows[x].Cells[0].Value.ToString() + "\r\n");
                    DownCounter[x]++;
                    //ResumeFtpFileDownload((URL + FileName), LocalDirectory, User_ID, Password,Grid2RowNumber);
                    UpdateGrid2(1, x, 11);

                    lock (_object)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                        {
                            //logs activity into Logs.Txt file
                            file.WriteLine("[" + DateTime.Now + "] " + "Retrying Download of file " +  Grid2.Rows[x].Cells[0].Value.ToString());
                        }
                    }
                    DoDownloadFile(x);

                }
                if ((DownCounter[x] > 3))
                {
                    lock (_object)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                        {
                            //logs activity into Logs.Txt file
                            file.WriteLine("[" + DateTime.Now + "] " +  Grid2.Rows[x].Cells[0].Value.ToString() + " will be redownloaded at a later time.");
                        }
                    }
                    //still thinking of what to do with this.
                    UpdateStatusBar("[" + DateTime.Now + "] " +  Grid2.Rows[x].Cells[0].Value.ToString() + " will be redownloaded at a later time.");

                    Update("[" + DateTime.Now + "] " +  Grid2.Rows[x].Cells[0].Value.ToString() + " will be redownloaded at a later time.\r\n");

                    UpdateGrid2(1, x, 11);
                    //Thread.CurrentThread.Abort();
                    int counter = GetNextRow(x+ 1);
                    DoDownloadFile(counter);
                }
            }
        }

        public int GetNextRow(int currrow)
        {
            int row = -1;
            for (int y = currrow; y < Grid2.RowCount; y++)
            {
                if ((Grid2.Rows[y].Cells[11].Value.ToString() == "1"))
                {
                    row = y;

                    break;
                }

            }
            return row;
        }

        //this is for updating file list grid and textbox. this is needed for threading
        public void Update(string x)
        {
            if (this.TxtLogs.InvokeRequired)
            {
                this.TxtLogs.Invoke(
                    new MethodInvoker(
                    delegate() { UpdateTextBox(x); }));
            }
        }

        public void UpdateTextBox(string x)
        {
            this.TxtLogs.Text = this.TxtLogs.Text + x.ToString();
            this.TxtLogs.Focus();
            this.TxtLogs.Select(this.TxtLogs.Text.Length,0);

        }

        public void UpdateGrid(string FileName, long FileSize, string URL, string User, string Pass, int RowNumber, string FileDate)
        {
            if (this.Grid2.InvokeRequired)
            {
                this.Grid2.Invoke(
                    new MethodInvoker(
                    delegate() { UpdateGridAdd(FileName, FileSize, URL, User, Pass, RowNumber, FileDate); }));
            }
        }

        public void UpdateGridAdd(string FileName, long FileSize, string URL, string User, string Pass, int RowNumber, string FileDate)
        {
            Grid2.Rows.Add(FileName, FileSize, URL, User, Pass, RowNumber, FileDate, 0, 0, 0, 0, 1);
            Grid2.Update();
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
        }

        public void UpdateGrid2Clear()
        {
            if (this.Grid2.InvokeRequired)
            {
                this.Grid2.Invoke(
                    new MethodInvoker(
                    delegate() { UpdateGridClear(); }));
            }
        }

        public void UpdateGridClear()
        {
            Grid2.Rows.Clear();
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


        public void UpdateClose()
        {
            if (this.button2.InvokeRequired)
            {
                this.button2.Invoke(
                    new MethodInvoker(
                    delegate() { UpdateStop(); }));
            }
        }

        public void UpdateStop()
        {
            DTPTimeSched.Enabled = true;
            stopBotToolStripMenuItem.Enabled = false;
            startBotToolStripMenuItem.Enabled = true;
            button1.Enabled = true; // Start Button
            button2.Enabled = false; // Stop Button
            manualDownloadToolStripMenuItem.Enabled = true;
            settingsToolStripMenuItem.Enabled = true;
            timer2.Enabled = false;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
            {
                file.WriteLine("[" + DateTime.Now + "] " + "Scheduler Process Ended.");
            }
            Update("[" + DateTime.Now + "] " + "Scheduler Process Ended." + "\r\n");
            UpdateStatusBar("[" + DateTime.Now + "] " + "Scheduler Process Ended." + "\r\n");

            MessageBox.Show("Scheduler Process Ended.");
            Triggered = 0;
            CancelProg = true;

        }

        private void button5_Click(object sender, EventArgs e)
        {
            Grid2.Rows.Clear();
        }

        private void startBotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DTPTimeSched.Enabled = false;
            timer2.Enabled = true;
            CancelProg = false;
            stopBotToolStripMenuItem.Enabled = true;
            startBotToolStripMenuItem.Enabled = false;
            button1.Enabled = false; // Start Button
            button2.Enabled = true; // Stop Button
            manualDownloadToolStripMenuItem.Enabled = false;

        }

        private void stopBotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // stops the timer/scheduler
            DTPTimeSched.Enabled = true;
            timer2.Enabled = false;
            //WorkLoad.Start();
            CancelProg = true;
            stopBotToolStripMenuItem.Enabled = false;
            startBotToolStripMenuItem.Enabled = true;
            button1.Enabled = true; // Start Button
            button2.Enabled = false; // Stop Button
            manualDownloadToolStripMenuItem.Enabled = true;

            UpdateStatusBar("[" + DateTime.Now + "] " + "Cancelling Process. Please wait.");
        }

        private void hideProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void clearFileListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Grid2.Rows.Clear();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CancelProg = true;
            Close();
        }

        private void stationListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StationList StnAdd = new StationList();

            StnAdd.ShowDialog();
        }

        private void changeDestinationFileServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Destination Desti = new Destination();
            Desti.ShowDialog();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to exit the program?", "Exit Program", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                CancelProg = true;
                //Close();
                Application.Exit();
            }
        }

        private void manualDownloadToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            ManualDownload M = new ManualDownload();
            M.ShowDialog();
        }

        private void clearFileListToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Grid2.Rows.Clear();
        }

        private void startBotToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            DTPTimeSched.Enabled = false;
            timer2.Enabled = true;
            CancelProg = false;
            stopBotToolStripMenuItem.Enabled = true;
            startBotToolStripMenuItem.Enabled = false;
            button1.Enabled = false; // Start Button
            button2.Enabled = true; // Stop Button
            manualDownloadToolStripMenuItem.Enabled = false;
        }

        private void stopBotToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to stop the scheduler?", "Stop Scheduler", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.No)
            {
                return;
            }
            // stops the timer/scheduler

            CancelProg = true;

            UpdateStatusBar("[" + DateTime.Now + "] " + "Cancelling Process. Please wait.");
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void changeDestinationForConvertedFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DestinationRNX RNX = new DestinationRNX();
            RNX.ShowDialog();

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

            string args = Teqc_Argument;


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
            using (Process exeProcess = Process.Start(startInfo))
            {
                standardErrorMessage = exeProcess.StandardError.ReadToEnd();
                exeProcess.WaitForExit();
            }
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

        private void setTeqCArgumentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TeqcSet RNX = new TeqcSet();
            RNX.ShowDialog();
        }

        private void Download(string URL, string FileName, string UserName, String Password, string File_Size, int Grid2RowNumber)
        {
            long FileSize = 0;




            string LocalDirectory = "C:\\Temp\\";

            if (CancelProg == true)
            {
                UpdateClose(); return;
            }

            FtpWebRequest requestFileDownload = (FtpWebRequest)WebRequest.Create(URL + FileName);
            requestFileDownload.Credentials = new NetworkCredential(UserName, Password);
            //requestFileDownload.Credentials = new NetworkCredential();
            requestFileDownload.Method = WebRequestMethods.Ftp.DownloadFile;
            requestFileDownload.Timeout = 60000;
            FtpWebResponse responseFileDownload = null;
            Stream responseStream;
            try
            {
                responseFileDownload = (FtpWebResponse)requestFileDownload.GetResponse();

            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    if (DownCounter[Grid2RowNumber] < 3)
                    {
                        //if the connection has a timeout
                        UpdateStatusBar("[" + DateTime.Now + "] " + ex.Message.ToString() + "");
                        UpdateStatusBar("[" + DateTime.Now + "] Retrying Download of file " + FileName);

                        Update("[" + DateTime.Now + "] " + ex.Message.ToString() + "\r\n");
                        Update("[" + DateTime.Now + "] Retrying Download of file " + FileName + "\r\n");

                        DownCounter[Grid2RowNumber]++;
                        //ResumeFtpFileDownload((URL + FileName), LocalDirectory, User_ID, Password,Grid2RowNumber);
                        UpdateGrid2(1, Grid2RowNumber, 11);

                        lock (_object)
                        {
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                            {
                                //logs activity into Logs.Txt file
                                file.WriteLine("[" + DateTime.Now + "] " + ex.Message.ToString());
                                file.WriteLine("[" + DateTime.Now + "] " + "Retrying Download of file " + FileName);

                            }
                        }
                        //int counter = GetNextRow(Grid2RowNumber + 1);
                        //DoCheckOfAvailableFile(counter);
                        Download(URL, FileName, UserName, Password, File_Size, Grid2RowNumber);

                        if (CancelProg == true)
                        {
                            UpdateClose(); return;
                        }
                    }

                    else if ((DownCounter[Grid2RowNumber] > 3))
                    {
                        lock (_object)
                        {
                            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                            {
                                //logs activity into Logs.Txt file
                                file.WriteLine("[" + DateTime.Now + "] " + FileName + " will be redownloaded at a later time.");
                            }
                        }
                        //still thinking of what to do with this.
                        UpdateStatusBar("[" + DateTime.Now + "] " + FileName + " will be redownloaded at a later time.");

                        Update("[" + DateTime.Now + "] " + FileName + " will be redownloaded at a later time.\r\n");
                        
                        UpdateGrid2(1, Grid2RowNumber, 11);
                        //Thread.CurrentThread.Abort();
                        //int counter = GetNextRow(Grid2RowNumber + 1);
                        //DoCheckOfAvailableFile(counter);


                        return;
                    }
                    else
                    {
                        UpdateGrid2(1, Grid2RowNumber, 11);
                        //Thread.CurrentThread.Abort();
                        int counter = GetNextRow(Grid2RowNumber + 1);
                        DoDownloadFile(counter);
                    }
                }
                else
                {
                    //MessageBox.Show(ex.Message.ToString());

                    lock (_object)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                        {
                            //logs activity into Logs.Txt file
                            file.WriteLine("[" + DateTime.Now + "] " + ex.Message.ToString());
                        }
                    }
                    //still thinking of what to do with this.
                    UpdateStatusBar("[" + DateTime.Now + "] " + ex.Message.ToString());

                    Update("[" + DateTime.Now + "] " + ex.Message.ToString());
                }
            }
            responseStream = responseFileDownload.GetResponseStream();
            FileStream writeStream = new FileStream(LocalDirectory + "/" + FileName, FileMode.Create);
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
                    Update("[" + DateTime.Now + "] " + "Downloading File from site to temp folder: " + FileName + " with a File Size of " + File_Size + "\r\n");
                    UpdateGrid2(0, Grid2RowNumber, 7);
                }
            }
            // streams/downloads file from site to temp folder.
            if (CancelProg == true)
            {
                UpdateClose(); return;
            }

            try
            {
                while (bytesRead > 0)
                {
                    if (FileSize != 0)
                    {
                        UpdateGrid2(Convert.ToInt32((FileSize / double.Parse(File_Size) * 100)), Grid2RowNumber, 7);
                        UpdateStatusBar("[" + DateTime.Now + "] " + FileName + " Downloaded: " + String.Format((FileSize / double.Parse(File_Size) * 100).ToString("00"), "0.00") + "%");
                        //Thread.Sleep(1000);
                    }
                    //Thread.Sleep(250);
                    writeStream.Write(buffer, 0, bytesRead);
                    bytesRead = responseStream.Read(buffer, 0, Length);
                    FileSize = FileSize + bytesRead;
                    if (CancelProg == true)
                    {
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                        return;

                    }
                }
            }

            catch (Exception e)
            {
                if (e.HResult.ToString() == "-2146232800")
                {
                    //if (DownCounter[Grid2RowNumber] < 3)
                    //{
                    //    //if the connection has a timeout
                    //    UpdateStatusBar("[" + DateTime.Now + "] Retrying Download of file " + FileName);

                    //    Update("[" + DateTime.Now + "] Retrying Download of file " + FileName + " for the " + (DownCounter[Grid2RowNumber]+1) + "\r\n");
                    //    DownCounter[Grid2RowNumber]++;
                    //    //DownCounter[Grid2RowNumber]++;
                    //    //ResumeFtpFileDownload((URL + FileName), LocalDirectory, User_ID, Password, Grid2RowNumber);
                    //    //Download(URL, FileName, UserName, Password, File_Size, Grid2RowNumber);

                    //    if (CancelProg == true)
                    //    {
                    //        this.timer2.Enabled = false;
                    //        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                    //        Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");

                    //        return;

                    //    }
                    //}
                    //else
                    //{

                    //}
                    UpdateStatusBar("[" + DateTime.Now + "] Download of file " + FileName + " failed.");

                    Update("[" + DateTime.Now + "] Download of file " + FileName + " failed. Moving to next file.\r\n");
                    lock (_object)
                    {
                        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                        {
                            //logs activity into Logs.Txt file
                            file.WriteLine("[" + DateTime.Now + "] Download of file " + FileName + " failed. Moving to next file.");

                        }
                    }
                    responseStream.Close();
                    writeStream.Close();
                    int counter = GetNextRow(Grid2RowNumber + 1);
                    DoDownloadFile(counter);
                }
                else
                {
                    //MessageBox.Show(e.Message.ToString());
                    responseStream.Close();
                    writeStream.Close();
                    int counter = GetNextRow(Grid2RowNumber + 1);
                    //DoCheckOfAvailableFile(counter);
                    DoDownloadFile(counter);
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
            Update("[" + DateTime.Now + "] " + FileName + " Size Downloaded: " + (FileSize + Length) + "\r\n");

            responseStream.Close();
            writeStream.Close();

            requestFileDownload = null;
            // end of download code

        }

        //public void ResumeFtpFileDownload(string sourceUri, string destinationFile, string user, string pass, int Grid2RowNumber)
        //{
        //    FileInfo file = new FileInfo(destinationFile);
        //    FileStream localfileStream;
        //    WebResponse response = null;
        //    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(sourceUri);
        //    request.Credentials = new NetworkCredential(user, pass);
        //    //requestFileDownload.Credentials = new NetworkCredential();
        //    request.Method = WebRequestMethods.Ftp.DownloadFile;
        //    request.Timeout = 5000;
        //    if (file.Exists)
        //    {
        //        request.ContentOffset = file.Length;
        //        localfileStream = new FileStream(destinationFile, FileMode.Append, FileAccess.Write);
        //    }
        //    else
        //    {
        //        localfileStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write);
        //    }
        //    //WebResponse response = request.GetResponse();
        //    try
        //    {
        //        response = request.GetResponse();

        //    }
        //    catch (WebException ex)
        //    {
        //        if (ex.Status == WebExceptionStatus.Timeout)
        //        {
        //            if (DownCounter[Grid2RowNumber] < 3)
        //            {
        //                //if the connection has a timeout
        //                UpdateStatusBar("[" + DateTime.Now + "] " + ex.Message.ToString() + "");
        //                UpdateStatusBar("[" + DateTime.Now + "] Retrying Download of file " + file.Name);

        //                Update("[" + DateTime.Now + "] " + ex.Message.ToString() + "\r\n");
        //                Update("[" + DateTime.Now + "] Retrying Download of file " + file.Name+ " for the " + (DownCounter[Grid2RowNumber] + 1) + "\r\n");
        //                DownCounter[Grid2RowNumber]++;
        //                ResumeFtpFileDownload(sourceUri, destinationFile, user, pass,Grid2RowNumber);
        //                //Download(URL, FileName, UserName, Password, File_Size, Grid2RowNumber);

        //                if (CancelProg == true)
        //                {
        //                    this.timer2.Enabled = false;
        //                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
        //                    Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");
        //                    return;

        //                }
        //            }
        //            else
        //            {
        //                DoCheckOfAvailableFile(Grid2RowNumber + 1);
        //            }
        //        }
        //        else
        //        {
        //            MessageBox.Show(ex.Message.ToString());
        //        }
        //    }
        //    Stream responseStream = response.GetResponseStream();
        //    byte[] buffer = new byte[1024];
        //    int bytesRead = responseStream.Read(buffer, 0, 1024);
        //    try
        //    {
        //        while (bytesRead != 0)
        //        {
        //            localfileStream.Write(buffer, 0, bytesRead);
        //            bytesRead = responseStream.Read(buffer, 0, 1024);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        if (e.HResult.ToString() == "-2146232800")
        //        {
        //            if (DownCounter[Grid2RowNumber] < 3)
        //            {
        //                //if the connection has a timeout
        //                UpdateStatusBar("[" + DateTime.Now + "] Retrying Download of file " + FileName);

        //                Update("[" + DateTime.Now + "] Retrying Download of file " + FileName + " for the " + (DownCounter[Grid2RowNumber] + 1) + "\r\n");
        //                DownCounter[Grid2RowNumber]++;
        //                //DownCounter[Grid2RowNumber]++;
        //                ResumeFtpFileDownload(sourceUri, destinationFile, user, pass, Grid2RowNumber);
        //                //Download(URL, FileName, UserName, Password, File_Size, Grid2RowNumber);

        //                if (CancelProg == true)
        //                {
        //                    this.timer2.Enabled = false;
        //                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
        //                    Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");

        //                    return;

        //                }
        //            }
        //            else
        //            {

        //            }

        //        }
        //        else
        //        {
        //            MessageBox.Show(e.Message.ToString());
        //        }
        //    }


        //    localfileStream.Close();
        //    responseStream.Close();
        //}
        private void UploadData(string FileName, int Grid2RowNumber, string RowNumber, string datedir)
        {
            string LocalDirectory1 = @"C:\Temp\" + FileName + ".zip";
            string LocalDirectory2 = @"C:\Temp\" + FileName.Substring(0, 8) + ".RNX.zip";

            string DestinationDirectory = (File_Location + datedir + @"\" + Grid.Rows[int.Parse(RowNumber)].Cells[0].Value.ToString() + @"\");  //Local directory where the files will be uploaded/copied.
            string DestinationDirectory2 = (File_LocationRNX + datedir + @"\");  //Local directory where the files will be uploaded/copied.

            try
            {
                if (CancelProg == true)
                {
                    UpdateClose(); return;
                }
                lock (_object)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {

                        //logs activity
                        file.WriteLine("[" + DateTime.Now + "] " + "Uploading Files to File Server.");
                        //logs activity
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Uploading Files to File Server.");
                        Update("[" + DateTime.Now + "] " + "Uploading Files to File Server." + "\r\n");
                    }
                }

                if (CancelProg == true)
                {
                    UpdateClose(); return;
                }

                NetworkCredential readCredentials = new NetworkCredential(@User_ID, Password_File);
                using (new NetworkConnection(File_Location, readCredentials))
                {
                    PageNet_AutoDownloader.CustomFileCopier fc = new PageNet_AutoDownloader.CustomFileCopier(LocalDirectory1, DestinationDirectory + FileName + ".zip", @User_ID, Password_File, File_Location);
                    fc.OnProgressChanged += (double Persentage, ref bool Cancel) => filecopyprogress(Persentage, Grid2RowNumber, 10);
                    fc.OnComplete += filecopycomplete;
                    fc.Copy();
                    if (CancelProg == true)
                    {
                        UpdateClose(); return;
                    }
                }

                using (new NetworkConnection(File_Location, readCredentials))
                {
                    PageNet_AutoDownloader.CustomFileCopier fc = new PageNet_AutoDownloader.CustomFileCopier(LocalDirectory2, DestinationDirectory2 + FileName.Substring(0, 8) + ".RNX.zip", @User_ID, Password_File, File_Location);
                    fc.OnProgressChanged += (double Persentage, ref bool Cancel) => filecopyprogress(Persentage, Grid2RowNumber, 10);
                    fc.OnComplete += filecopycomplete;
                    fc.Copy();
                    if (CancelProg == true)
                    {
                        UpdateClose(); return;
                    }
                }

                lock (_object)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {

                        //logs activity
                        file.WriteLine("[" + DateTime.Now + "] " + "Uploading Files to File Server Finished");
                        //logs activity
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Uploading Files to File Server Finished.");
                        Update("[" + DateTime.Now + "] " + "Uploading Files to File Server Finished." + "\r\n");
                    }
                }

            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message.ToString());

                lock (_object)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {

                        //logs activity
                        file.WriteLine("[" + DateTime.Now + "] " + e.Message.ToString());
                        //logs activity
                        UpdateStatusBar("[" + DateTime.Now + "] " + e.Message.ToString());
                        Update("[" + DateTime.Now + "] " + e.Message.ToString() + "\r\n");
                    }
                }
            }
        }

        private void DoEvents()
        {
            throw new NotImplementedException();
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            var dateone = DTPTimeSched.Value;
            var datetwo = DateTime.Now;
            var diff = datetwo.Subtract(dateone);
            //label7.Text = DateTime.Now.ToString();
            //label8.Text = DTPTimeSched.Value.ToString();
            //label5.Text = diff.Hours.ToString();
            //label6.Text = Triggered.ToString();
            if ((diff.Hours == 4) && (Triggered == 0))
            {
                //tester code for threading
                TSSFile.Text = "restarting Automatic File Comparison";
                Thread WorkLoad = new Thread(new ThreadStart(DoWork));
                WorkLoad.Name = "Test";
                WorkLoad.IsBackground = true;
                WorkLoad.Start();
                //this.DoWork.RunWorkerAsync();
                CancelProg = false;
                Triggered = 1;

                //logs activity in the Logs.Txt file.
                lock (_object)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {
                        file.WriteLine("[" + DateTime.Now + "] Restarting File Comparison to check for unfinished downloads");
                    }
                }

                //logs activity into Textbox
                UpdateStatusBar("[" + DateTime.Now + "] Restarting File Comparison to check for unfinished downloads");
                Update("[" + DateTime.Now + "] Restarting File Comparison to check for unfinished downloads\r\n");
            }
        }

    }

}