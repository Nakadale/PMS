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
using Ionic.Zip;
using Ionic.Zlib;
using Shared_Folder_Login;


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

        bool CancelProg = false;
        List<FTPFiles> ftpfiles = new List<FTPFiles>(); // for storing of file list coming from the site


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

        private void Grid2_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right) 
            {
                this.ContextMenuStrip = contextMenuStrip1;
                contextMenuStrip1.Show();
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
                this.textBox8.Text = reader["File_Location"].ToString();
                this.textBox7.Text = reader["User_ID"].ToString();
                this.textBox6.Text = reader["Password"].ToString();
            }
            sql_con.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
            LoadDesti();
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
            DateTime DT = DTPCheck.Value;

            lock (_object)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + StrDate + "_Logs.txt", true))
                {

                    foreach (FTPFiles i in ftpfiles)
                    {
                        string FileLoc = this.textBox8.Text + DT.Year.ToString() + @"\" + DT.Month.ToString("00") + @"\" +
                        DT.Subtract(TimeSpan.FromDays(1)).Day.ToString("00") + @"\" + Grid.Rows[currRow].Cells[0].Value.ToString() 
                        + @"\" + i.FileName.Substring(0, 12).ToString() + ".zip";

                        //File exist code is in the if statement
                        if (File.Exists(@FileLoc) == false)
                        {
                            Grid2.Rows.Add(i.FileName.Substring(0, 12).ToString(),"","","", i.FileBytes, Grid.Rows[currRow].Cells[1].Value.ToString(), Grid.Rows[currRow].Cells[2].Value.ToString(), Grid.Rows[currRow].Cells[3].Value.ToString(), currRow);
                            Grid2.Update();
                        }
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
                        //UpdateStatusBar("[" + DateTime.Now + "] " + "Connected to Station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + "");

                        string JulianDate = DT.Subtract(TimeSpan.FromDays(1)).DayOfYear.ToString("000");

                        //string JulianDate = DateTime.Now.DayOfYear.ToString("000");
                        //UpdateStatusBar("[" + DateTime.Now + "] " + "Getting File(s) Information");
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
                        if (FileCounter == 0)
                        {
                             MessageBox.Show("Raw files from this date " + DateTime.Now.Subtract(TimeSpan.FromDays(1)).ToShortDateString() + " are all downloaded.");
                        }
                        //UpdateStatusBar("[" + DateTime.Now + "] " + FileCounter + " File(s) Information collected.");
                    }
                    catch (WebException ex)
                    {
                        //MessageBox.Show(ex.Message.ToString());
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            file.WriteLine("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established. Invalid Username or Password");
                            //Update("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established." + "\r\n");
                            MessageBox.Show("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established. Invalid Username or Password");
                        }
                        if (ex.Status == WebExceptionStatus.ConnectFailure)
                        {
                            file.WriteLine("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established.");
                            //Update("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established." + "\r\n");
                            MessageBox.Show("[" + DateTime.Now + "] " + "Connection to station " + Grid.Rows[CurrRow].Cells[0].Value.ToString() + " could not be established.");                            
                            return;
                        }
                    }
                }
            }
        }

        private void Grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            currRow = e.RowIndex;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DoCheckOfAvailableFile(Grid2currRow);
        }


        public void DoCheckOfAvailableFile(object num)
        {
            int row = 0;
            int.TryParse(num.ToString(), out row);
            for (int y = 0; y == row; y++)
            {
                if ((Grid2.Rows[y].Cells[1].Value.ToString() != "") && (Grid2.Rows[y].Cells[2].Value.ToString() != "") && (Grid2.Rows[y].Cells[3].Value.ToString() != ""))
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
                    if (CancelProg == true)
                    {
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
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
                        if (CancelProg == true)
                        {
                            UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
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
            }
            catch (Exception e)
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

            //UpdateGrid2("Downloading File", Grid2RowNumber,2);

            try
            {
                //download code
                if (CancelProg == true)
                {
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                    return;

                }
                FtpWebRequest requestFileDownload = (FtpWebRequest)WebRequest.Create(new Uri(URL + FileName));
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
                        UpdateGrid2("0%", Grid2RowNumber, 1);
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

                responseStream.Close();
                writeStream.Close();

                requestFileDownload = null;
                // end of download code

                if (CancelProg == true)
                {
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                    return;

                }
                //compresses the download raw file
                if (CancelProg == true)
                {
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
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
                UpdateStatusBar("[" + DateTime.Now + "] " + "Compressing File" + "\r\n");
                //logs activity
                //UpdateGrid2("Compressing File", Grid2RowNumber, 2);
                //compression code
                String DirectoryToZip = LocalDirectory + FileName;
                String ZipFileToCreate = LocalDirectory + FileName + ".zip";

                using (ZipFile zip = new ZipFile())
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

                // end of compression code
                if (CancelProg == true)
                {
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
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
                            UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                            return;

                        }
                        //logs activity
                        file.WriteLine("[" + DateTime.Now + "] " + "Deleting File from Temp Folder");
                        //logs activity
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Deleting File from Temp Folder" + "");
                        //delete code/command 
                        File.Delete(LocalDirectory + FileName);
                        //logs activity
                        UpdateStatusBar("[" + DateTime.Now + "] " + "File Deletion Completed" + "");
                        //logs activity
                        file.WriteLine("[" + DateTime.Now + "] " + "File Deletion Completed");
                    }
                }
                //end of deletion code
                if (CancelProg == true)
                {
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                    return;

                }
                //uploads the compress file into the ftp server
                string LocalDirectory1 = @"C:\Temp\" + FileName + ".zip";
                string User = this.textBox7.Text;
                string Pass = this.textBox6.Text;
                string BaseDirectory = this.textBox8.Text;

                string DestinationDirectory = (this.textBox8.Text + DT.Year.ToString()) + @"\" + DT.Month.ToString("00") 
                    + @"\" + DT.Subtract(TimeSpan.FromDays(1)).Day.ToString("00") + 
                    @"\" + Grid.Rows[int.Parse(RowNumber)].Cells[0].Value.ToString() + @"\";  //Local directory where the files will be uploaded/copied.

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
                    UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
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
                            UpdateStatusBar("[" + DateTime.Now + "] " + "Process Stopped");
                            return;

                        }
                        //logs activity
                        file.WriteLine("[" + DateTime.Now + "] " + "Deleting File from Temp Folder");
                        //logs activity
                        UpdateStatusBar("[" + DateTime.Now + "] " + "Deleting File from Temp Folder" + "");
                        //delete code/command 
                        File.Delete(LocalDirectory + FileName + ".zip");
                        //logs activity
                        UpdateStatusBar("[" + DateTime.Now + "] " + "File Deletion Completed" + "");
                        //logs activity
                        file.WriteLine("[" + DateTime.Now + "] " + "File Deletion Completed");
                    }
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
                        Downloadfile(URL, FileName, user, pass, RowNumber, Grid2RowNumber, File_Size);
                        DownCounter++;
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
                UpdateGrid2(((e.BytesTransferred * 100) / e.TotalBytesToTransfer).ToString() + "%", Grid2RowNumber, 2);
                //labelCProg.Text = ((e.BytesTransferred * 100) / e.TotalBytesToTransfer).ToString("0.00%");
            }
        }

        public void UpdateGrid2(string x, int RowNum, int CellNum)
        {
            if (this.Grid2.InvokeRequired)
            {
                this.Grid2.Invoke(
                    new MethodInvoker(
                    delegate() { UpdateGrid2Column2(x, RowNum, CellNum); }));
            }
        }

        public void UpdateGrid2Column2(string x, int RowNum, int CellNum)
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
            Grid2.Rows[RowNum].Cells[CellNum].Value = ((int)percent) + "%";
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

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }


    }
}
