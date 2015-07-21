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
        int DownCounter = 0; //download counter for downloadfile if file failed to download
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
            catch(Exception e)
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
            settingsToolStripMenuItem.Enabled = true;

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

            DataGridViewProgressColumn column = new DataGridViewProgressColumn(); //download
            DataGridViewProgressColumn column1 = new DataGridViewProgressColumn(); //convert
            DataGridViewProgressColumn column2 = new DataGridViewProgressColumn(); //compress
            DataGridViewProgressColumn column3 = new DataGridViewProgressColumn(); //upload

            column.HeaderText = "Download Progress (%)";

            Grid2.Columns.Add(column);

            column1.HeaderText = "Conversion Progress (%)";

            Grid2.Columns.Add(column1);

            column2.HeaderText = "Compress Progress (%)";

            Grid2.Columns.Add(column2);

            column3.HeaderText = "Upload Progress (%)";

            Grid2.Columns.Add(column3);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            // checks time if equivalent to the time set in the schedule and checks Triggered if it was triggered once.
            if (lblTime.Text == DTPTimeSched.Value.ToShortTimeString() && Triggered == 0)
            {
                //tester code for threading
                TSSFile.Text = "Start Automatic File Comparison";
                Thread WorkLoad = new Thread(new ThreadStart(DoWork));
                WorkLoad.Name = "Test";
                WorkLoad.IsBackground = true;
                WorkLoad.Start();
                //this.DoWork.RunWorkerAsync();
                CancelProg = false;
                Triggered = 1;

            }

            //Resets Triggered to 0 when time is equivalent to 12:00 AM
            if (lblTime.Text == "12:00 AM" && Triggered == 1)
            {
                Triggered = 0;
                TxtLogs.Text = "";
                TSStatus.Text = "";
            }

        }


        // connects to site and try to get all file information
        public void GetDataFromSite(int CurrRow)
        {
            lock (_object)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
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
                            if ((JulianDate == varsam[8].Substring(4, 3)) && (varsam[8].Substring(9,1) == "m"))
                            {
                                ftptemp.FileName = varsam[8].ToString();
                                ftptemp.FileBytes = long.Parse(varsam[4].ToString());
                                ftpfiles.Add(ftptemp);
                                FileCounter = FileCounter + 1;
                            }
                        }
                        UpdateStatusBar("[" + DateTime.Now + "] " + FileCounter + " File(s) Information collected.");
                        Update("[" + DateTime.Now + "] " + FileCounter + " File(s) Information collected.\r\n");

                    }
                    catch (WebException ex)
                    {
                        //MessageBox.Show(ex.Message.ToString());
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            file.WriteLine("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established. Invalid User Name or Password.");
                            Update("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established. Invalid User Name or Password." + "\r\n");
                            return;
                        }

                        if (ex.Status == WebExceptionStatus.ConnectFailure)
                        {
                            file.WriteLine("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established.");
                            Update("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established." + "\r\n");
                            return;
                        }
                    }
                }
            }
        }

        //downloads Files that are not present in the Destination Server
        public void Downloadfile(string URL, string FileName,string user, string pass, string RowNumber, int Grid2RowNumber, string File_Size)
        {

                long FileSize = 0;
                string UserName = user;              //User Name of the FTP server
                string Password = pass;              //Password of the FTP server

                string LocalDirectory = "C:\\Temp\\";  //Local directory where the files will be downloaded

                //UpdateGrid2("Downloading File", Grid2RowNumber,2);

                try
                {
                    //download code
                    if (CancelProg == true)
                    {
                        this.timer2.Enabled = false;
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                        Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");
                        return; 

                    }
                    FtpWebRequest requestFileDownload = (FtpWebRequest)WebRequest.Create(URL + FileName);
                    requestFileDownload.Credentials = new NetworkCredential(UserName, Password);
                    //requestFileDownload.Credentials = new NetworkCredential();
                    requestFileDownload.Method = WebRequestMethods.Ftp.DownloadFile;
                    FtpWebResponse responseFileDownload = (FtpWebResponse)requestFileDownload.GetResponse();
                    Stream responseStream = responseFileDownload.GetResponseStream();
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
                            UpdateGrid2(0, Grid2RowNumber, 6);
                        }
                    }
                            // streams/downloads file from site to temp folder.
                            if (CancelProg == true)
                            {
                                this.timer2.Enabled = false;
                                UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                                Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");
                                return;


                            }

                            while (bytesRead > 0)
                            {
                                if (FileSize != 0)
                                {
                                    UpdateGrid2(Convert.ToInt32((FileSize / double.Parse(File_Size) * 100)), Grid2RowNumber, 6);
                                    UpdateStatusBar("[" + DateTime.Now + "] " + FileName + " Downloaded: " + String.Format((FileSize / double.Parse(File_Size) * 100).ToString(), "0.00") + "%");
                                    //Thread.Sleep(1000);
                                }
                                writeStream.Write(buffer, 0, bytesRead);
                                bytesRead = responseStream.Read(buffer, 0, Length);
                                FileSize = FileSize + bytesRead;
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


                    //********************************************************************************************
                    //start conversion protocol
                    //********************************************************************************************
                    UpdateGrid2(Convert.ToInt32(0), Grid2RowNumber, 7);

                    String targetPath = @"C:\Temp\" + FileName.Substring(0, 8) + ".RNX";
                    string sourceFile = System.IO.Path.Combine(@"C:\Temp\", FileName);
                    string destFile = System.IO.Path.Combine(targetPath, FileName);

                    Directory.CreateDirectory(@"C:\Temp\" + FileName.Substring(0, 8) + ".RNX");
                    File.Copy(sourceFile, destFile);

                    RunTEQC(destFile, DateTime.Now.Year.ToString());

                    File.Delete(destFile);

                    using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
                    {
                        if (CancelProg == true)
                        {
                            UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                            return;

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
                    UpdateGrid2(Convert.ToInt32(100), Grid2RowNumber, 7);
                    //********************************************************************************************
                    // calls the conversion protocol
                    //********************************************************************************************

                    //compresses the download raw file
                    if (CancelProg == true)
                    {
                        this.timer2.Enabled = false;
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                        Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");

                        return;

                    }
                    UpdateGrid2(0, Grid2RowNumber, 8);

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
                    UpdateGrid2(0, Grid2RowNumber, 8);

                    String DirectoryToZip = LocalDirectory + FileName;
                    String ZipFileToCreate = LocalDirectory + FileName + ".zip";

                    using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
                    {
                        if (CancelProg == true)
                        {
                            this.timer2.Enabled = false;
                            UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                            Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");
                            return;

                        }
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Default;
                        zip.SaveProgress += (object sender, SaveProgressEventArgs e) => SaveProgress(sender, e, Grid2RowNumber);
                        zip.StatusMessageTextWriter = System.Console.Out;
                        zip.AddFile(DirectoryToZip, ""); // recurses subdirectories
                        zip.Save(ZipFileToCreate);
                    }


                    UpdateGrid2(100, Grid2RowNumber, 8);

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

                    // end of compression code
                    if (CancelProg == true)
                    {
                        this.timer2.Enabled = false;
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                        Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");

                        return;

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
                        this.timer2.Enabled = false;
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                        Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");
                        return;

                    }
                    //end of deletion code
                    if (CancelProg == true)
                    {
                        this.timer2.Enabled = false;
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                        Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");

                        return;

                    }
                    //uploads the compress file into the ftp server
                    string LocalDirectory1 = @"C:\Temp\" + FileName + ".zip";
                    string LocalDirectory2 = @"C:\Temp\" + FileName.Substring(0, 8) + ".RNX.zip";
 
                    string DestinationDirectory = (File_Location + DateTime.Now.Year.ToString()) + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Subtract(TimeSpan.FromDays(1)).Day.ToString("00") + @"\" + Grid.Rows[int.Parse(RowNumber)].Cells[0].Value.ToString() + @"\";  //Local directory where the files will be uploaded/copied.
                    string DestinationDirectory2 = (File_LocationRNX + DateTime.Now.Year.ToString()) + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Subtract(TimeSpan.FromDays(1)).Day.ToString("00") + @"\" + Grid.Rows[int.Parse(RowNumber)].Cells[0].Value.ToString() + @"\";  //Local directory where the files will be uploaded/copied.
                    NetworkCredential readCredentials = new NetworkCredential(@User_ID, Password_File);
                    using (new NetworkConnection(File_Location, readCredentials))
                    {
                        PageNet_AutoDownloader.CustomFileCopier fc = new PageNet_AutoDownloader.CustomFileCopier(LocalDirectory1, DestinationDirectory + FileName + ".zip");
                        fc.OnProgressChanged += (double Persentage, ref bool Cancel) => filecopyprogress(Persentage, Grid2RowNumber, 9);
                        fc.OnComplete += filecopycomplete;
                        fc.Copy();
                    }

                    using (new NetworkConnection(File_Location, readCredentials))
                    {
                        PageNet_AutoDownloader.CustomFileCopier fc = new PageNet_AutoDownloader.CustomFileCopier(LocalDirectory2, DestinationDirectory2 + FileName.Substring(0,8) + ".RNX.zip");
                        fc.OnProgressChanged += (double Persentage, ref bool Cancel) => filecopyprogress(Persentage, Grid2RowNumber, 9);
                        fc.OnComplete += filecopycomplete;
                        fc.Copy();
                    }
                    
                    //end of upload code
                    if (CancelProg == true)
                    {
                        this.timer2.Enabled = false;
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                        Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");

                        return;

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
                            File.Delete(LocalDirectory + FileName.Substring(0,8) + ".RNX.zip");

                            //logs activity
                            UpdateStatusBar("[" + DateTime.Now + "] " + "File Deletion Completed");
                            Update("[" + DateTime.Now + "] " + "File Deletion Completed\r\n");
                            //logs activity
                            file.WriteLine("[" + DateTime.Now + "] " + "File Deletion Completed");
                        }
                    }

                    if (CancelProg == true)
                    {
                        this.timer2.Enabled = false;
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                        Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");
                        return;

                    }
                    // end of deletion code
                    
                    //if (Grid2RowNumber != Grid2.RowCount)
                    //{
                    //    DoCheckOfAvailableFile((Grid2RowNumber+1));
                    //}
                    return;
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ProtocolError)
                    {
                        //if the supplied password or username is wrong
                        UpdateStatusBar("[" + DateTime.Now + "] " + "User Name or Password is invalid for site " +  Grid.Rows[int.Parse(RowNumber)].Cells[0].Value.ToString());
                        Update("[" + DateTime.Now + "] " + "User Name or Password is invalid for site " + Grid.Rows[int.Parse(RowNumber)].Cells[0].Value.ToString() + "\r\n");

                        if (CancelProg == true)
                        {
                            this.timer2.Enabled = false;
                            UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                            Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");
                            return;

                        }
                    }
                    if (ex.Status == WebExceptionStatus.ConnectFailure)
                    {
                        //if there is no connection to the site
                        UpdateStatusBar("[" + DateTime.Now + "] " + ex.Message.ToString());
                        if (CancelProg == true)
                        {
                            this.timer2.Enabled = false;

                            UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                            Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");

                            return;

                        }
                    }
                    if (ex.Status == WebExceptionStatus.Timeout)
                    {
                        if (DownCounter < 3)
                        {
                            //if the connection has a timeout
                            UpdateStatusBar("[" + DateTime.Now + "] " + ex.Message.ToString() + "");
                            UpdateStatusBar("[" + DateTime.Now + "] Retrying Download of file " + FileName);

                            Update("[" + DateTime.Now + "] " + ex.Message.ToString() + "\r\n");
                            Update("[" + DateTime.Now + "] Retrying Download of file " + FileName + "\r\n");
                            Downloadfile(URL, FileName, user, pass, RowNumber, Grid2RowNumber, File_Size);
                            DownCounter++;
                            if (CancelProg == true)
                            {
                                this.timer2.Enabled = false;
                                UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                                Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");

                                return;

                            }
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
                    delegate() { UpdateProgressbar(Persentage, RowNum,CellNum); }));
            }
            //this.textBox1.Text = Persentage.ToString();
        }

        public void filecopycomplete()
        {
        }

        private void UpdateProgressbar(double percent,int RowNum, int CellNum)
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
                        this.timer2.Enabled = false;
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                        Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");
                        return;

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
                    this.timer2.Enabled = false;
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                    Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");
                    return;

                }
                GetDataFromSite(Counter);

                //checks if folder in the destination server exists. if not exist it will create folders required before continuing.
                if (CancelProg == true)
                {
                    this.timer2.Enabled = false;
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                    Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");

                    return;
                }


                FolderChecker(Counter, File_Location);

                FolderChecker(Counter,File_LocationRNX);

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
                                UpdateGrid(i.FileName.Substring(0, 12).ToString(), i.FileBytes, Grid.Rows[Counter].Cells[1].Value.ToString(), Grid.Rows[Counter].Cells[2].Value.ToString(), Grid.Rows[Counter].Cells[3].Value.ToString(), Counter);
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
                //clears file list grid and ftpfiles where the file list from one site are stored.
                ftpfiles.Clear();
            }

            if (Grid2.RowCount != 0)
            {
                DoCheckOfAvailableFile(0);
                Thread.Sleep(1000);
                DoCheckOfAvailableFile(1);
            }
            UpdateStatusBar("File Comparison Finished");
            Update("File Comparison Finished\r\n");

            UpdateStatusBar("Program will automatically compare files on " + DateTime.Now.AddDays(1).ToShortDateString());
            Update("Program will automatically compare files on " + DateTime.Now.AddDays(1).ToShortDateString() + "\r\n");

        }

        public void DoCheckOfAvailableFile(object num)
        {
            int row = -1;
            int.TryParse(num.ToString(), out row);
            for (int y = 0; y < Grid2.RowCount; y++)
            {
                if ((Grid2.Rows[y].Cells[6].Value.ToString() == "0") && (Grid2.Rows[y].Cells[7].Value.ToString() == "0") && (Grid2.Rows[y].Cells[8].Value.ToString() == "0") && (Grid2.Rows[y].Cells[9].Value.ToString() == "0"))
                {
                    row = y;
                    break;
                }
                if (y == Grid2.RowCount)
                {
                    break;
                }
            }
            lock (_object)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "Logs.txt", true))
                {
                    //logs activity
                    file.WriteLine("[" + DateTime.Now + "] " + "Downloading File: " + Grid2.Rows[row].Cells[0].Value.ToString());
                    //logs activity
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Downloading File: " + Grid2.Rows[row].Cells[0].Value.ToString() + "");
                    Update("[" + DateTime.Now + "] " + "Downloading File: " + Grid2.Rows[row].Cells[0].Value.ToString() + "\r\n");

                    if (CancelProg == true)
                    {
                        this.timer2.Enabled = false;
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                        Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");

                        return;
                    }
                }
            }

            ParameterizedThreadStart NPT = new ParameterizedThreadStart(DoDownloadFile);
            Thread T1 = new Thread(NPT);
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
                        Update("[" + DateTime.Now + "] " + "Downloading File: " + Grid2.Rows[counter].Cells[0].Value.ToString() + "\r\n");
                        if (CancelProg == true)
                        {
                            this.timer2.Enabled = false;
                            UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                            Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");

                            return;
                        }
                    }
                }

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
                    Debug.WriteLine(counter);
                    UpdateStatusBar("File Comparison Finished");
                    Update("File Comparison Finished\r\n");

                    UpdateStatusBar("Program will automatically compare files on " + DateTime.Now.AddDays(1).ToShortDateString());
                    Update("Program will automatically compare files on " + DateTime.Now.AddDays(1).ToShortDateString() + "\r\n");

                }
            }
            catch(Exception e)
            {
                //MessageBox.Show(e.ToString());

            }
        }

        public int GetNextRow(int currrow)
        {
            int row = -1;
            for (int y = currrow; y < Grid2.RowCount; y++)
            {
                if ((Grid2.Rows[y].Cells[6].Value.ToString() == "0") && (Grid2.Rows[y].Cells[7].Value.ToString() == "0") && (Grid2.Rows[y].Cells[8].Value.ToString() == "0") && (Grid2.Rows[y].Cells[9].Value.ToString() == "0"))
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

        }

        public void UpdateGrid(string FileName, long FileSize, string URL, string User, string Pass, int RowNumber)
        {
            if (this.Grid2.InvokeRequired)
            {
                this.Grid2.Invoke(
                    new MethodInvoker(
                    delegate() { UpdateGridAdd(FileName,FileSize,URL,User,Pass, RowNumber); }));
            }
        }

        public void UpdateGridAdd(string FileName, long FileSize, string URL, string User, string Pass, int RowNumber)
        {
            Grid2.Rows.Add(FileName,FileSize,URL,User,Pass,RowNumber,0,0,0,0);
            Grid2.Update();
        }

        public void UpdateGrid2(int x, int RowNum, int CellNum)
        {
            if (this.Grid2.InvokeRequired)
            {
                this.Grid2.Invoke(
                    new MethodInvoker(
                    delegate() { UpdateGrid2Column2(x, RowNum,CellNum); }));
            }
        }

        public void UpdateGrid2Column2(int x, int RowNum,int CellNum)
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
    }
}