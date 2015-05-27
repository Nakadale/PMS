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

namespace PageNet_AutoDownloader
{
    public partial class Form1 : Form
    {

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

        }
        
        public Form1()
        {
            // standard default form
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Closes/exits the program
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
            lblDate.Text = saveNow.Subtract(TimeSpan.FromDays(1)).ToShortDateString();
            //lblDate.Text = saveNow.ToShortDateString();
            lblTime.Text = saveNow.ToShortTimeString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // starts the timer/scheduler
            DTPTimeSched.Enabled = false;
            timer2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // stops the timer/scheduler
            DTPTimeSched.Enabled = true;
            timer2.Enabled = false;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //calls LoadData method
            LoadData();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            // checks time if equivalent to the time set in the schedule and checks Triggered if it was triggered once.
            if (lblTime.Text == DTPTimeSched.Value.ToShortTimeString() && Triggered == 0)
            {
                TSSFile.Text = "Start Automatic File Comparison";
                Thread WorkLoad = new Thread(new ThreadStart(DoWork));
                WorkLoad.Start();
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
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "Logs.txt", true))
            {

                try
                {
                    int FileCounter=0;
                    // Get the object used to communicate with the server.
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(Grid.Rows[CurrRow].Cells[1].Value.ToString()));

                    request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                    request.Proxy = null;


                    request.Credentials = new NetworkCredential(Grid.Rows[CurrRow].Cells[2].Value.ToString(), Grid.Rows[CurrRow].Cells[3].Value.ToString());
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                    Stream responseStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(responseStream);
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Connected to Station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + "");

                    string JulianDate = DateTime.Now.Subtract(TimeSpan.FromDays(1)).DayOfYear.ToString("000");
                    
                    //string JulianDate = DateTime.Now.DayOfYear.ToString("000");
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Getting File(s) Information");
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
                            FileCounter = FileCounter+1;
                        }
                    }
                    UpdateStatusBar("[" + DateTime.Now + "] " + FileCounter + " File(s) Information collected.");

                }
                catch (WebException ex)
                {
                    //MessageBox.Show(ex.Message.ToString());
                    if (ex.Status == WebExceptionStatus.ConnectFailure)
                    {
                        file.WriteLine("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established.");
                        Update("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established." + "\r\n");
                        return;
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

                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "Logs.txt", true))
                    {
                        //logs activity into Logs.Txt file
                        file.WriteLine("[" + DateTime.Now + "] " + "Downloading File from site to temp folder: " + FileName + " with a File Size of " + File_Size);
                        //logs activity into Textbox
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Downloading File from site to temp folder: " + FileName + " with a File Size of " + File_Size + "");
                        //updates file activity into File List Grid
                        UpdateGrid2("0%", Grid2RowNumber, 1);

                        // streams/downloads file from site to temp folder.
                        while (bytesRead > 0)
                        {
                            if (FileSize != 0)
                            {
                                UpdateGrid2((FileSize / double.Parse(File_Size) * 100).ToString("00") + "%", Grid2RowNumber, 1);
                                UpdateStatusBar("[" + DateTime.Now + "] " + FileName + " Downloaded: " + String.Format((FileSize/double.Parse(File_Size)*100).ToString(),"0.00") + "%");
                                //Thread.Sleep(1000);
                            }
                            writeStream.Write(buffer, 0, bytesRead);
                            bytesRead = responseStream.Read(buffer, 0, Length);
                            FileSize = FileSize + bytesRead;
                        }
                        //logs activity to File List Grid that file download was finished 100%.
                        UpdateGrid2("100%", Grid2RowNumber, 1);
                        //logs activity in the Logs.Txt file.
                        file.WriteLine("[" + DateTime.Now + "] " + FileName + " Size Downloaded: " + (FileSize + Length));
                        //logs activity into Textbox
                        UpdateStatusBar("[" + DateTime.Now + "] " + FileName + " Size Downloaded: " + (FileSize + Length) + "");
                    }
                    responseStream.Close();
                    writeStream.Close();
                    requestFileDownload = null;
                    // end of download code


                    //compresses the download raw file
                    CompressFile(LocalDirectory + FileName, Grid2RowNumber);

                    //deletes the raw file in the temp folder
                    DeleteFile(LocalDirectory + FileName);

                    //uploads the compress file into the ftp server
                    UploadFileStream(FileName + ".zip", int.Parse(RowNumber), Grid2RowNumber);

                    //deletes the zip file in the temp folder
                    DeleteFile(LocalDirectory + FileName + ".zip");

                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.ConnectFailure)
                    {
                        //if there is no connection to the site
                        UpdateStatusBar("[" + DateTime.Now + "] " + ex.Message.ToString() + "\r\n");

                    }
                    if (ex.Status == WebExceptionStatus.Timeout)
                    {
                        if (DownCounter < 3)
                        {
                            //if the connection has a timeout
                            UpdateStatusBar("[" + DateTime.Now + "] " + ex.Message.ToString() + "");
                            UpdateStatusBar("[" + DateTime.Now + "] Retrying Download.");
                            Update("[" + DateTime.Now + "] " + ex.Message.ToString() + "\r\n");
                            Update("[" + DateTime.Now + "] Retrying Download.\r\n");
                            Downloadfile(URL, FileName, user, pass, RowNumber, Grid2RowNumber, File_Size);
                            DownCounter++;
                        }
                    }

                    return;
                }
            
        }
        public void UploadFileStream(string filename, int RowNumber, int Grid2RowNumber)
        {
            string LocalDirectory = @"C:\Temp\" + filename;
            string User = this.textBox7.Text;
            string Pass = this.textBox6.Text;
            string BaseDirectory = this.textBox8.Text;
            
                
            string DestinationDirectory = (this.textBox8.Text + DateTime.Now.Year.ToString()) + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Day.ToString("00") + @"\" + Grid.Rows[RowNumber].Cells[0].Value.ToString() + @"\";  //Local directory where the files will be uploaded/copied.
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

        //uploads file from temp folder to Destination/File Server
        //private void UploadFile(string filename, int RowNumber, int Grid2RowNumber)
        //{
        //    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "Logs.txt", true))
        //    {

        //        //logs activity into Logs.Txt file
        //        file.WriteLine("[" + DateTime.Now + "] " + filename + " is being copied to the Destination Server");
        //        //logs activity into Textbox
        //        //Update("[" + DateTime.Now + "] " + filename + " is being copied to the Destination Server" + "\r\n");
        //        //Updates File Status in File List Grid
        //        //UpdateGrid2("Uploading File to Server", Grid2RowNumber, 2);

        //        string LocalDirectory = "C:/Temp/";  //Local directory where the files will be downloaded
        //        string User = this.textBox7.Text;
        //        string Pass = this.textBox6.Text;

        //        string BaseDirectory = this.textBox8.Text;

        //        string DestinationDirectory = (this.textBox8.Text + DateTime.Now.Year.ToString()) + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Day.ToString("00") + @"\" + Grid.Rows[RowNumber].Cells[0].Value.ToString() + @"\";  //Local directory where the files will be uploaded/copied.
                
        //        FileInfo fileInf = new FileInfo(LocalDirectory + filename);
        //        string uri = fileInf.Name;
        //        //copy code
        //        NetworkCredential readCredentials = new NetworkCredential(@User, Pass);
        //        using (new NetworkConnection(BaseDirectory, readCredentials))
        //        {
        //            File.Copy(LocalDirectory + @uri, DestinationDirectory + filename, true);
        //        }
        //        //logs activity to Logs.Txt
        //        file.WriteLine("[" + DateTime.Now + "] " + "Copying Complete");
        //        //logs activity to Textbox
        //        //Update("[" + DateTime.Now + "] " + "Copying Complete" + "\r\n");
        //    }
        //}
        //old code for uploading. not being used at the moment.
        //private void Upload(string filename)
        //{
        //    string LocalDirectory = "C:/Temp/";  //Local directory where the files will be downloaded
        //    FileInfo fileInf = new FileInfo(LocalDirectory + filename);
        //    //string uri = this.textBox8.Text + fileInf.Name;



        //    string uri = fileInf.Name;
        //    FtpWebRequest reqFTP;

        //    // Create FtpWebRequest object from the Uri provided
        //    //reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(
        //    //          this.textBox8.Text + fileInf.Name));
        //    reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(fileInf.Name));
        //    // Provide the WebPermission Credintials
        //    reqFTP.Credentials = new NetworkCredential(this.textBox7.Text, this.textBox6.Text);
        //    reqFTP.Credentials = new NetworkCredential();

        //    // By default KeepAlive is true, where the control connection is 
        //    // not closed after a command is executed.
        //    reqFTP.KeepAlive = false;

        //    // Specify the command to be executed.
        //    reqFTP.Method = WebRequestMethods.Ftp.UploadFile;

        //    // Specify the data transfer type.
        //    reqFTP.UseBinary = true;

        //    // Notify the server about the size of the uploaded file
        //    reqFTP.ContentLength = fileInf.Length;

        //    reqFTP.Proxy = null;
        //    // The buffer size is set to 2kb
        //    int buffLength = 2048;
        //    byte[] buff = new byte[buffLength];
        //    int contentLen;

        //    // Opens a file stream (System.IO.FileStream) to read 
        //    //the file to be uploaded
        //    FileStream fs = fileInf.OpenRead();

        //    try
        //    {
        //        // Stream to which the file to be upload is written
        //        Stream strm = reqFTP.GetRequestStream();

        //        // Read from the file stream 2kb at a time
        //        contentLen = fs.Read(buff, 0, buffLength);

        //        // Till Stream content ends
        //        while (contentLen != 0)
        //        {
        //            // Write Content from the file stream to the 
        //            // FTP Upload Stream
        //            strm.Write(buff, 0, contentLen);
        //            contentLen = fs.Read(buff, 0, buffLength);
        //        }

        //        // Close the file stream and the Request Stream
        //        strm.Close();
        //        fs.Close();
        //    }
        //    catch (WebException ex)
        //    {

        //        MessageBox.Show(ex.Message, "Upload Error");
        //        Console.Write(ex.Message);
        //    }
        //}

        //private void UploadFile2(string filename, int RowNumber, int Grid2RowNumber)
        //{
        //    try
        //    {
        //        string LocalDirectory = "C:/Temp/";  //Local directory where the files will be downloaded
        //        string User = this.textBox7.Text;
        //        string Pass = this.textBox6.Text;

        //        string BaseDirectory = this.textBox8.Text;
        //        string DestinationDirectory = (this.textBox8.Text + DateTime.Now.Year.ToString()) + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Day.ToString("00") + @"\" + Grid.Rows[RowNumber].Cells[0].Value.ToString() + @"\";  //Local directory where the files will be uploaded/copied.


        //        //Open a FileStream to the source file
        //        FileStream fin = new FileStream(LocalDirectory + filename, FileMode.Open,
        //        FileAccess.Read, FileShare.Read);

        //        //Open a FileStream to the destination file
        //        FileStream fout = new FileStream(DestinationDirectory, FileMode.OpenOrCreate,
        //        FileAccess.Write, FileShare.None);

        //        //Create a byte array to act as a buffer
        //        Byte[] buffer = new Byte[32];
        //        Console.WriteLine("File Copy Started");

        //        //Loop until end of file is not reached

        //        while (fin.Position != fin.Length)
        //        {
        //            //Read from the source file
        //            //The Read method returns the number of bytes read
        //            int n = fin.Read(buffer, 0, buffer.Length);

        //            //Write the contents of the buffer to the destination file
        //            fout.Write(buffer, 0, n);
        //        }

        //        //Flush the contents of the buffer to the file
        //        fout.Flush();

        //        //Close the streams and free the resources
        //        fin.Close();
        //        fout.Close();
        //        Console.WriteLine("File Copy Ended");
        //    }

        //    catch (IOException e)
        //    {
        //        //Catch a IOException
        //        Console.WriteLine("An IOException Occurred :" + e);
        //    }
        //}

        public void DeleteFile(string path)
        {
            //file delete code
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "Logs.txt", true))
            {
                //logs activity
                file.WriteLine("[" + DateTime.Now + "] " + "Deleting File from Temp Folder");
                //logs activity
                UpdateStatusBar("[" + DateTime.Now + "] " + "Deleting File from Temp Folder" + "");
                //delete code/command 
                File.Delete(path);
                //logs activity
                UpdateStatusBar("[" + DateTime.Now + "] " + "File Deletion Completed" + "\r\n");
                //logs activity
                file.WriteLine("[" + DateTime.Now + "] " + "File Deletion Completed");
            }
        }


        //public void CompressFile2(string path, int Grid2RowNumber,string FileName)
        //{
        //    //file compress code
        //    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "Logs.txt", true))
        //    {
        //        string anyString = File.ReadAllText(path);
        //        string temp = Path.GetTempFileName();
        //        File.WriteAllText(temp, anyString);

        //        byte[] b;
        //        using (FileStream f = new FileStream(temp, FileMode.Open))
        //        {
        //            b = new byte[f.Length];
        //            f.Read(b, 0, (int)f.Length);
        //        }

        //        // C.
        //        // Use GZipStream to write compressed bytes to target file.
        //        using (FileStream f2 = new FileStream(FileName, FileMode.Create))
        //        using (GZipStream gz = new GZipStream(f2, CompressionMode.Compress, false))
        //        {
        //            gz.Write(b, 0, b.Length);
        //        }
        //    }
        //}

        public void CompressFile(string path, int Grid2RowNumber)
        {
            //file compress code
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "Logs.txt", true))
            {
                //logs activity
                file.WriteLine("[" + DateTime.Now + "] " + "Compressing File");
                //logs activity
                UpdateStatusBar("[" + DateTime.Now + "] " + "Compressing File" + "\r\n");
                //logs activity
                //UpdateGrid2("Compressing File", Grid2RowNumber, 2);
                //compression code
                String DirectoryToZip = path;
                String ZipFileToCreate = path + ".zip";

                using (ZipFile zip = new ZipFile())
                {
                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Default;
                    zip.SaveProgress += (object sender, SaveProgressEventArgs e) => SaveProgress(sender, e, Grid2RowNumber);                    
                    zip.StatusMessageTextWriter = System.Console.Out;
                    zip.AddFile(DirectoryToZip, ""); // recurses subdirectories
                    zip.Save(ZipFileToCreate);
                }
                //logs activity
                file.WriteLine("[" + DateTime.Now + "] " + "File Compression Finished");
                //logs activity
                UpdateStatusBar("[" + DateTime.Now + "] " + "File Compression Finished" + "");
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
                UpdateGrid2(((e.BytesTransferred * 100) / e.TotalBytesToTransfer).ToString() + "%", Grid2RowNumber, 2);
                //Thread.Sleep(1000);
                //labelCProg.Text = ((e.BytesTransferred * 100) / e.TotalBytesToTransfer).ToString("0.00%");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //tester code for threading
            TSSFile.Text = "Start Automatic File Comparison";
            Thread WorkLoad = new Thread(new ThreadStart(DoWork));
            WorkLoad.Start();
            //this.DoWork.RunWorkerAsync();

        }

        //checks for folder existence in the destination/file server
        public void FolderChecker(int SelectedRow)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "Logs.txt", true))
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
                    file.WriteLine("[" + DateTime.Now + "] " + "Creating Month Folder");
                    CreateFolder((this.textBox8.Text + DateTime.Now.Year.ToString()), DateTime.Now.Month.ToString("00"));
                }

                //checks day level folder
                if (CheckFolder((this.textBox8.Text + DateTime.Now.Year + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Day.ToString("00"))) == true)
                {
                    //does nothing since every folder is created.
                }
                else
                {
                    //creates the folder if non-existent
                    file.WriteLine("[" + DateTime.Now + "] " + "Creating Day Folder");
                    CreateFolder((this.textBox8.Text + DateTime.Now.Year.ToString()) + @"\" + DateTime.Now.Month.ToString("00"), DateTime.Now.Day.ToString("00"));
                }

                //checks station level folder
                if (CheckFolder(this.textBox8.Text + DateTime.Now.Year + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Day.ToString("00") + @"\" + Grid.Rows[SelectedRow].Cells[0].Value.ToString()) == true)
                {
                    //does nothing since every folder is created.
                }
                else
                {
                    //creates the folder if non-existent
                    file.WriteLine("[" + DateTime.Now + "] " + "Creating Station Folder");
                    CreateFolder((this.textBox8.Text + DateTime.Now.Year.ToString()) + @"\" + DateTime.Now.Month.ToString("00") + @"\" + DateTime.Now.Day.ToString("00"), Grid.Rows[SelectedRow].Cells[0].Value.ToString());
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
            //MessageBox.Show("Ginawa ako!");
            for (int Counter = 0; Counter < Grid.RowCount; Counter++)
            {
                //MessageBox.Show(Counter.ToString());
                UpdateStatusBar("[" + DateTime.Now + "] " + " Connecting to Station: " + Grid.Rows[Counter].Cells[0].Value.ToString() + "");
                
                //Update("[" + DateTime.Now + "] " + " Connecting to Station: " + Grid.Rows[Counter].Cells[0].Value.ToString() + "\r\n");
                GetDataFromSite(Counter);

                //checks if folder in the destination server exists. if not exist it will create folders required before continuing.
                FolderChecker(Counter);

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "Logs.txt", true))
                {

                    foreach (FTPFiles i in ftpfiles)
                    {
                        string FileLoc = this.textBox8.Text + DateTime.Now.Year.ToString() + "\"" + DateTime.Now.Month.ToString("00") + "\"" + DateTime.Now.Subtract(TimeSpan.FromDays(1)).Day.ToString("00") + "\"" + Grid.Rows[Counter].Cells[0].Value.ToString() + "\"" + i.FileName.Substring(0, 12).ToString() + ".zip";
                        //File exist code is in the if statement
                        if (File.Exists(@FileLoc) == false)
                        {
                            //adds filename to File List Grid
                            UpdateGrid(i.FileName.Substring(0, 12).ToString(), i.FileBytes, Grid.Rows[Counter].Cells[1].Value.ToString(), Grid.Rows[Counter].Cells[2].Value.ToString(), Grid.Rows[Counter].Cells[3].Value.ToString(), Counter);
                            //logs activity
                            file.WriteLine("[" + DateTime.Now + "] " + i.FileName.Substring(0, 12).ToString() + " File Does not exist in the destination server!");
                            //logs activity
                            UpdateStatusBar("[" + DateTime.Now + "] " + i.FileName.Substring(0, 12).ToString() + " File Does not exist in the destination server!" + "");
                            //logs activity
                            file.WriteLine("[" + DateTime.Now + "] " + "File Size:" + i.FileBytes);
                            //logs activity
                            UpdateStatusBar("[" + DateTime.Now + "] " + "File Size:" + i.FileBytes + "");

                        }
                    }
                }
                //clears file list grid and ftpfiles where the file list from one site are stored.
                ftpfiles.Clear();
            }

            //will initiate downloading of files
            for (int counter = 0; counter < Grid2.RowCount; counter++)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "Logs.txt", true))
                {
                    //logs activity
                    file.WriteLine("[" + DateTime.Now + "] " + "Downloading File: " + Grid2.Rows[counter].Cells[0].Value.ToString());
                    //logs activity
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Downloading File: " + Grid2.Rows[counter].Cells[0].Value.ToString() + "");
                }
                Downloadfile(
                Grid2.Rows[counter].Cells[5].Value.ToString(),//url
                Grid2.Rows[counter].Cells[0].Value.ToString(),//filename
                Grid2.Rows[counter].Cells[6].Value.ToString(),//user
                Grid2.Rows[counter].Cells[7].Value.ToString(),//pass
                Grid2.Rows[counter].Cells[8].Value.ToString(),//rownumbergrid1
                counter, //rownumbergrid2
                Grid2.Rows[counter].Cells[4].Value.ToString());//file size;
                //logs activity
                //UpdateGrid2("Done", counter, 2);

                //UpdateGrid2("", counter, 1);
            }
            UpdateStatusBar("File Comparison Finished");
            //UpdateGrid2Clear();
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


    }
}
