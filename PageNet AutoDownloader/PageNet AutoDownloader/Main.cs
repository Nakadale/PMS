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


        }

        private void button2_Click(object sender, EventArgs e)
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

        public void LoadDesti()
        {
            sql_con.Open();
            string CommandText = "Select * from DestinationServer";
            SQLiteCommand command = new SQLiteCommand(CommandText, sql_con);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                this.textBox8.Text = reader["File_Location"].ToString();
                this.textBox7.Text = reader["User_ID"].ToString();
                this.textBox6.Text = reader["Password"].ToString();
            }
            sql_con.Close();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //calls LoadData method
            LoadData();

            //loads file server information
            LoadDesti();

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
                            if (JulianDate == varsam[8].Substring(4, 3))
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
                            UpdateGrid2("0%", Grid2RowNumber, 1);
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
                                    UpdateGrid2((FileSize / double.Parse(File_Size) * 100).ToString("00") + "%", Grid2RowNumber, 1);
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

                    if (CancelProg == true)
                    {
                        this.timer2.Enabled = false;
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                        Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");

                        return;

                    }
                    //compresses the download raw file
                    if (CancelProg == true)
                    {
                        this.timer2.Enabled = false;
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                        Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");

                        return;

                    }
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
                    String DirectoryToZip = LocalDirectory + FileName;
                    String ZipFileToCreate = LocalDirectory + FileName + ".zip";

                    using (ZipFile zip = new ZipFile())
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
                            //delete code/command 
                            Update("[" + DateTime.Now + "] " + "Deleting File from Temp Folder" + "\r\n");

                            File.Delete(LocalDirectory + FileName);
                            //logs activity
                            UpdateStatusBar("[" + DateTime.Now + "] " + "File Deletion Completed" + "\r\n");
                            Update("[" + DateTime.Now + "] " + "File Deletion Completed" + "\r\n");

                            //logs activity
                            file.WriteLine("[" + DateTime.Now + "] " + "File Deletion Completed");
                        }
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
                    string User = this.textBox7.Text;
                    string Pass = this.textBox6.Text;
                    string BaseDirectory = this.textBox8.Text;


                    string DestinationDirectory = (this.textBox8.Text + DateTime.Now.Year.ToString()) + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Subtract(TimeSpan.FromDays(1)).Day.ToString("00") + @"\" + Grid.Rows[int.Parse(RowNumber)].Cells[0].Value.ToString() + @"\";  //Local directory where the files will be uploaded/copied.
                    NetworkCredential readCredentials = new NetworkCredential(@User, Pass);
                    using (new NetworkConnection(BaseDirectory, readCredentials))
                    {
                        PageNet_AutoDownloader.CustomFileCopier fc = new PageNet_AutoDownloader.CustomFileCopier(LocalDirectory1, DestinationDirectory + FileName + ".zip");
                        //fc.OnProgressChanged += filecopyprogress;
                        //fc.OnProgressChanged += filecopyprogress(Grid2RowNumber, Grid2RowNumber);
                        fc.OnProgressChanged += (double Persentage, ref bool Cancel) => filecopyprogress(Persentage, Grid2RowNumber, 3);
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
                            File.Delete(LocalDirectory + FileName + ".zip");
                            //logs activity
                            UpdateStatusBar("[" + DateTime.Now + "] " + "File Deletion Completed");
                            Update("[" + DateTime.Now + "] " + "File Deletion Completed\r\n");
                            //logs activity
                            file.WriteLine("[" + DateTime.Now + "] " + "File Deletion Completed");
                        }
                    }
                    // end of deletion code
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
        public void UploadFileStream(string filename, int RowNumber, int Grid2RowNumber)
        {
            string LocalDirectory = @"C:\Temp\" + filename;
            string User = this.textBox7.Text;
            string Pass = this.textBox6.Text;
            string BaseDirectory = this.textBox8.Text;


            string DestinationDirectory = (this.textBox8.Text + DateTime.Now.Year.ToString()) + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Subtract(TimeSpan.FromDays(1)).Day.ToString("00") + @"\" + Grid.Rows[RowNumber].Cells[0].Value.ToString() + @"\";  //Local directory where the files will be uploaded/copied.
            NetworkCredential readCredentials = new NetworkCredential(@User, Pass);
            using (new NetworkConnection(BaseDirectory, readCredentials))
            {
                PageNet_AutoDownloader.CustomFileCopier fc = new PageNet_AutoDownloader.CustomFileCopier(LocalDirectory, DestinationDirectory + filename);
                //fc.OnProgressChanged += filecopyprogress;
                //fc.OnProgressChanged += filecopyprogress(Grid2RowNumber, Grid2RowNumber);
                fc.OnProgressChanged += (double Persentage, ref bool Cancel) => filecopyprogress(Persentage, Grid2RowNumber, 3);
                fc.OnComplete += filecopycomplete;
                fc.Copy();
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
            Grid2.Rows[RowNum].Cells[CellNum].Value = ((int)percent) + "%" ;
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

        public void CompressFile(string path, int Grid2RowNumber)
        {
            //file compress code
            
                if (CancelProg == true)
                {
                    this.timer2.Enabled = false;
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                    Update("[" + DateTime.Now + "] " + "Process Stopped\r\n");

                    return;

                }
                //logs activity
                lock (_object)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {    
                        file.WriteLine("[" + DateTime.Now + "] " + "Compressing File");
                    }
                }
                        //logs activity
                UpdateStatusBar("[" + DateTime.Now + "] " + "Compressing File");
                //logs activity
                //UpdateGrid2("Compressing File", Grid2RowNumber, 2);
                //compression code
                String DirectoryToZip = path;
                String ZipFileToCreate = path + ".zip";

                using (ZipFile zip = new ZipFile())
                {
                    if (CancelProg == true)
                    {
                        this.timer2.Enabled = false;
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
        }

        public void SaveProgress(object sender, SaveProgressEventArgs e, int Grid2RowNumber)
        {
            if (e.EventType == ZipProgressEventType.Saving_BeforeWriteEntry)
            {
            }
            else if (e.EventType == ZipProgressEventType.Saving_EntryBytesRead)
            {
                //UpdateLabelCProg(((e.BytesTransferred * 100) / e.TotalBytesToTransfer).ToString());
                UpdateGrid2(((e.BytesTransferred * 100) / e.TotalBytesToTransfer).ToString() + "%", Grid2RowNumber, 2);
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
        public void FolderChecker(int SelectedRow)
        {
            lock (_object)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                {
                    //checks top level folder
                    if (CheckFolder(this.textBox8.Text) == true)
                    {
                        //does nothing since every folder is created.
                    }
                    else
                    {
                        //creates the folder if non-existent
                        file.WriteLine("[" + DateTime.Now + "] " + "Creating Top Level Folder");
                        Update("[" + DateTime.Now + "] " + "Creating Top Level Folder\r\n");

                        TopCreateFolder(this.textBox8.Text.ToString());
                    }

                    //checks year level folder
                    if (CheckFolder(this.textBox8.Text + DateTime.Now.Year) == true)
                    {
                        //does nothing since every folder is created.
                    }
                    else
                    {
                        //creates the folder if non-existent
                        file.WriteLine("[" + DateTime.Now + "] " + "Creating Year Folder");
                        Update("[" + DateTime.Now + "] " + "Creating Year Folder\r\n");

                        CreateFolder(this.textBox8.Text, DateTime.Now.Year.ToString());
                    }

                    //checks month level folder
                    if (CheckFolder(this.textBox8.Text + DateTime.Now.Year + @"\" + DateTime.Now.Month.ToString("00")) == true)
                    {
                        //does nothing since every folder is created.
                    }
                    else
                    {
                        //creates the folder if non-existent
                        Update("[" + DateTime.Now + "] " + "Creating Month Folder\r\n");

                        file.WriteLine("[" + DateTime.Now + "] " + "Creating Month Folder");
                        CreateFolder((this.textBox8.Text + DateTime.Now.Year.ToString()), DateTime.Now.Month.ToString("00"));
                    }

                    //checks day level folder
                    if (CheckFolder((this.textBox8.Text + DateTime.Now.Year + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Subtract(TimeSpan.FromDays(1)).Day.ToString("00"))) == true)
                    {
                        //does nothing since every folder is created.
                    }
                    else
                    {
                        //creates the folder if non-existent
                        file.WriteLine("[" + DateTime.Now + "] " + "Creating Day Folder");
                        Update("[" + DateTime.Now + "] " + "Creating Day Folder\r\n");
                        CreateFolder((this.textBox8.Text + DateTime.Now.Year.ToString()) + @"\" + DateTime.Now.Month.ToString("00"), DateTime.Now.Subtract(TimeSpan.FromDays(1)).Day.ToString("00"));
                    }

                    //checks station level folder
                    if (CheckFolder(this.textBox8.Text + DateTime.Now.Year + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Subtract(TimeSpan.FromDays(1)).Day.ToString("00") + @"\" + Grid.Rows[SelectedRow].Cells[0].Value.ToString()) == true)
                    {
                        //does nothing since every folder is created.
                    }
                    else
                    {
                        //creates the folder if non-existent
                        file.WriteLine("[" + DateTime.Now + "] " + "Creating Station Folder");
                        Update("[" + DateTime.Now + "] " + "Creating Station Folder\r\n");
                        CreateFolder((this.textBox8.Text + DateTime.Now.Year.ToString()) + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Subtract(TimeSpan.FromDays(1)).Day.ToString("00"), Grid.Rows[SelectedRow].Cells[0].Value.ToString());
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
                FolderChecker(Counter);

                lock (_object)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                    {

                        foreach (FTPFiles i in ftpfiles)
                        {
                            string FileLoc = this.textBox8.Text + DateTime.Now.Year.ToString() + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Subtract(TimeSpan.FromDays(1)).Day.ToString("00") + @"\" + Grid.Rows[Counter].Cells[0].Value.ToString() + @"\" + i.FileName.Substring(0, 12).ToString() + ".zip";
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

            //will initiate downloading of files
            //for (int counter = 0; counter < Grid2.RowCount; counter+=2)
            //{
            //    lock (_object)
            //    {
            //        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "Logs.txt", true))
            //        {
            //            //logs activity
            //            file.WriteLine("[" + DateTime.Now + "] " + "Downloading File: " + Grid2.Rows[counter].Cells[0].Value.ToString());
            //            //logs activity
            //            UpdateStatusBar("[" + DateTime.Now + "] " + "Downloading File: " + Grid2.Rows[counter].Cells[0].Value.ToString() + "");
            //            if (CancelProg == true)
            //            {
            //                this.timer2.Enabled = false;
            //                UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
            //                return;
            //            }
            //        }
            //    }

            //        ParameterizedThreadStart NPT = new ParameterizedThreadStart(DoDownloadFile);
            //        Thread T1 = new Thread(NPT);
            //        T1.Start(counter);

            //        Thread T2 = new Thread(NPT);
            //        T2.Start(counter + 1);
            //}

            if (Grid2.RowCount != 0)
            {
                DoCheckOfAvailableFile(0);
                Thread.Sleep(100);
                DoCheckOfAvailableFile(1);
            }
            UpdateStatusBar("File Comparison Finished");
            Update("File Comparison Finished\r\n");

            UpdateStatusBar("Program will automatically compare files on " + DateTime.Now.AddDays(1).ToShortDateString());
            Update("Program will automatically compare files on " + DateTime.Now.AddDays(1).ToShortDateString() + "\r\n");

            //UpdateGrid2Clear();
        }

        public void DoCheckOfAvailableFile(object num)
        {
            int row = -1;
            int.TryParse(num.ToString(), out row);
            for (int y = 0; y < Grid2.RowCount; y++)
            {
                if ((Grid2.Rows[y].Cells[1].Value.ToString() == "") && (Grid2.Rows[y].Cells[2].Value.ToString() == "") && (Grid2.Rows[y].Cells[3].Value.ToString() == ""))
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
                        Grid2.Rows[counter].Cells[5].Value.ToString(),//url
                        Grid2.Rows[counter].Cells[0].Value.ToString(),//filename
                        Grid2.Rows[counter].Cells[6].Value.ToString(),//user
                        Grid2.Rows[counter].Cells[7].Value.ToString(),//pass
                        Grid2.Rows[counter].Cells[8].Value.ToString(),//rownumbergrid1
                        counter, //rownumbergrid2
                        Grid2.Rows[counter].Cells[4].Value.ToString());//file size;

                counter = GetNextRow(counter);
                if (counter != -1)
                {
                    DoDownloadFile(counter);
                }
                else
                {
                    UpdateStatusBar("File Comparison Finished");
                    Update("File Comparison Finished\r\n");

                    UpdateStatusBar("Program will automatically compare files at " + DTPTimeSched.Value + " on " + DateTime.Now.AddDays(1).ToShortDateString());
                    Update("Program will automatically compare files at " + DTPTimeSched.Value + " on " + DateTime.Now.AddDays(1).ToShortDateString() + "\r\n");

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
                if ((Grid2.Rows[y].Cells[1].Value.ToString() == "") && (Grid2.Rows[y].Cells[2].Value.ToString() == "") && (Grid2.Rows[y].Cells[3].Value.ToString() == ""))
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
            Grid2.Rows.Add(FileName, "", "", "",FileSize,URL,User,Pass,RowNumber);
            Grid2.Update();
        }

        public void UpdateGrid2(string x, int RowNum, int CellNum)
        {
            if (this.Grid2.InvokeRequired)
            {
                this.Grid2.Invoke(
                    new MethodInvoker(
                    delegate() { UpdateGrid2Column2(x, RowNum,CellNum); }));
            }
        }

        public void UpdateGrid2Column2(string x, int RowNum,int CellNum)
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

        private void button7_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Thread {0} started", Thread.CurrentThread.Name);
            //Console.WriteLine("Thread {0} started", Thread.CurrentThread.Name);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start(@AppDomain.CurrentDomain.BaseDirectory + "ManualDownload.exe");
            //System.Diagnostics.Process.Start("mspaint.exe");
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

        private void leicaToRNXConverterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start(@"C:\Users\SherwinAquino\Documents\PMS\PageNet AutoDownloader\PageNet AutoDownloader\MDB2RNX Converter\MDB2RNX.exe");
            //Process p = new Process();
            //p.StartInfo.FileName = @"C:\Users\SherwinAquino\Documents\PMS\PageNet AutoDownloader\PageNet AutoDownloader\MDB2RNX Converter\MDB2RNX.exe";
            //p.StartInfo.UseShellExecute = true;
            ////p.StartInfo.FileName = @"MDB2RNX.exe";
            //p.Start();
            MDB2RNX.MainForm M = new MDB2RNX.MainForm();
            M.ShowDialog();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CancelProg = true;
            Close();
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


    }
}
