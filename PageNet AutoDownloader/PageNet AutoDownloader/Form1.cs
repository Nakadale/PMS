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
using System.Data.SQLite;

namespace PageNet_AutoDownloader
{
    public partial class Form1 : Form
    {
        List<FTPFiles> ftpfiles = new List<FTPFiles>();
        DateTime saveNow = DateTime.Now;

        private SQLiteConnection sql_con = new SQLiteConnection(ConfigurationManager.ConnectionStrings["PageNet_AutoDownloader.Properties.Settings.sql_con"].ConnectionString);
        private SQLiteCommand sql_cmd;
        private SQLiteDataAdapter DBMain;
        private DataSet DSMain = new DataSet();
        private DataTable DTMain = new DataTable();

        public void LoadData()
        {
            sql_con.Open();

            sql_cmd = sql_con.CreateCommand();
            string CommandText = "Select * from Main";
            DBMain = new SQLiteDataAdapter(CommandText, sql_con);
            DSMain.Reset();
            DBMain.Fill(DSMain);
            DTMain = DSMain.Tables[0];
            Grid.DataSource = DTMain;
            sql_con.Close();
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            lblDate.Text = saveNow.ToShortDateString();
            lblTime.Text = saveNow.ToShortTimeString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DTPTimeSched.Enabled = false;
            timer2.Enabled = true;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            DTPTimeSched.Enabled = true;
            timer2.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {

        }



        public void GetDataFromSite(int CurrRow)
        {

            // Get the object used to communicate with the server.
            //FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(this.textBox1.Text));
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(Grid.Rows[CurrRow].Cells[1].Value.ToString()));

            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            request.Proxy = null;

            //request.Credentials = new NetworkCredential(this.textBox2.Text, this.textBox3.Text);
            request.Credentials = new NetworkCredential(Grid.Rows[CurrRow].Cells[2].Value.ToString(),Grid.Rows[CurrRow].Cells[3].Value.ToString());
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            //string List2 = reader.ReadLine();
            //string List2 = reader.ReadToEnd();



            string JulianDate = saveNow.DayOfYear.ToString("000");
                //dateTimePicker1.Value.DayOfYear.ToString("000");
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "WriteLines.txt", true))
            {
                //file.WriteLine("Station Name: " + this.textBox4.Text);
                file.WriteLine("Station Name: " + Grid.Rows[CurrRow].Cells[0].Value.ToString());
                file.WriteLine("Files not found on Data Server: ");

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    var varsam = line.Split(new Char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    //MessageBox.Show(varsam.ToString());
                    FTPFiles ftptemp = new FTPFiles();
                    if (JulianDate == varsam[8].Substring(4, 3))
                    {
                        ftptemp.FileName = varsam[8].ToString();
                        ftptemp.FileBytes = long.Parse(varsam[4].ToString());
                        //ftptemp.FileDate = varsam[6].ToString();
                        ftpfiles.Add(ftptemp);
                    }
                }

            }
            reader.Close();
            response.Close();

        }


        public void Downloadfile(string URL, string FileName)
        {
            //string UserName = this.textBox2.Text;                 //User Name of the FTP server
            //string Password = this.textBox3.Text;              //Password of the FTP server

            string UserName;                 //User Name of the FTP server
            string Password;              //Password of the FTP server

            string LocalDirectory = "C:\\Temp\\";  //Local directory where the files will be downloaded

            try
            {
                FtpWebRequest requestFileDownload = (FtpWebRequest)WebRequest.Create(URL + FileName);
                //requestFileDownload.Credentials = new NetworkCredential(UserName, Password);
                requestFileDownload.Credentials = new NetworkCredential();
                requestFileDownload.Method = WebRequestMethods.Ftp.DownloadFile;
                FtpWebResponse responseFileDownload = (FtpWebResponse)requestFileDownload.GetResponse();
                Stream responseStream = responseFileDownload.GetResponseStream();
                FileStream writeStream = new FileStream(LocalDirectory + "/" + FileName, FileMode.Create);
                int Length = 2048;
                Byte[] buffer = new Byte[Length];
                int bytesRead = responseStream.Read(buffer, 0, Length);
                while (bytesRead > 0)
                {
                    writeStream.Write(buffer, 0, bytesRead);
                    bytesRead = responseStream.Read(buffer, 0, Length);
                }
                responseStream.Close();
                writeStream.Close();
                requestFileDownload = null;

                //compresses the download raw file
                CompressFile(LocalDirectory + FileName);

                //deletes the raw file in the temp folder
                DeleteFile(LocalDirectory + FileName);

                //uploads the compress file into the ftp server
                //Upload(FileName+".zip");
                UploadFile(FileName + ".zip");

                //deletes the zip file in the temp folder
                DeleteFile(LocalDirectory + FileName + ".zip");

            }
            catch (Exception ex)
            {
                //throw ex;
                MessageBox.Show("File Not Found");
                return;
            }

        }

        private void UploadFile(string filename)
        {
            string LocalDirectory = "C:/Temp/";  //Local directory where the files will be downloaded
            FileInfo fileInf = new FileInfo(LocalDirectory + filename);
            string uri = fileInf.Name;


            File.Copy(LocalDirectory + filename, @uri);

        }

        private void Upload(string filename)
        {
            string LocalDirectory = "C:/Temp/";  //Local directory where the files will be downloaded
            FileInfo fileInf = new FileInfo(LocalDirectory + filename);
            //string uri = this.textBox8.Text + fileInf.Name;
            string uri = fileInf.Name;
            FtpWebRequest reqFTP;

            // Create FtpWebRequest object from the Uri provided
            //reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(
            //          this.textBox8.Text + fileInf.Name));
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(fileInf.Name));
            // Provide the WebPermission Credintials
            //reqFTP.Credentials = new NetworkCredential(this.textBox7.Text, this.textBox6.Text);
            reqFTP.Credentials = new NetworkCredential();

            // By default KeepAlive is true, where the control connection is 
            // not closed after a command is executed.
            reqFTP.KeepAlive = false;

            // Specify the command to be executed.
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;

            // Specify the data transfer type.
            reqFTP.UseBinary = true;

            // Notify the server about the size of the uploaded file
            reqFTP.ContentLength = fileInf.Length;

            reqFTP.Proxy = null;
            // The buffer size is set to 2kb
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;

            // Opens a file stream (System.IO.FileStream) to read 
            //the file to be uploaded
            FileStream fs = fileInf.OpenRead();

            try
            {
                // Stream to which the file to be upload is written
                Stream strm = reqFTP.GetRequestStream();

                // Read from the file stream 2kb at a time
                contentLen = fs.Read(buff, 0, buffLength);

                // Till Stream content ends
                while (contentLen != 0)
                {
                    // Write Content from the file stream to the 
                    // FTP Upload Stream
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }

                // Close the file stream and the Request Stream
                strm.Close();
                fs.Close();
            }
            catch (WebException ex)
            {

                MessageBox.Show(ex.Message, "Upload Error");
                Console.Write(ex.Message);
            }
        }


        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public static void CompressFile(string path)
        {
            FileStream sourceFile = File.OpenRead(path);
            FileStream destinationFile = File.Create(path + ".zip");

            byte[] buffer = new byte[sourceFile.Length];
            sourceFile.Read(buffer, 0, buffer.Length);

            using (GZipStream output = new GZipStream(destinationFile,
                CompressionMode.Compress))
            {
                Console.WriteLine("Compressing {0} to {1}.", sourceFile.Name,
                    destinationFile.Name, false);

                output.Write(buffer, 0, buffer.Length);
            }

            // Close the files.
            sourceFile.Close();
            destinationFile.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {

            for (int Counter = 0;Counter < Grid.RowCount;Counter++)
            {
                TxtLogs.Text = TxtLogs.Text + Grid.Rows[Counter].Cells[0].Value.ToString() + "\r\n";
                GetDataFromSite(Counter);
                ListViewItem item1 = new ListViewItem("CH1");
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@AppDomain.CurrentDomain.BaseDirectory + "Logs.txt", true))
                {
                    foreach (FTPFiles i in ftpfiles)
                    {
                        string FileLoc = Grid.Rows[Counter].Cells[1].Value.ToString() + i.FileName.Substring(0, 12).ToString() + ".zip";
                        if (File.Exists(@FileLoc) == false)
                        {
                            Grid2.Rows.Add(i.FileName.Substring(0, 12).ToString(),"","");
                        }
                    }
                    file.WriteLine("=========================================================");

                }

                MessageBox.Show("Ftp Reply Logged into Text File. Beginning file download.");
                for (int counter = 0; counter < Grid2.RowCount; counter++)
                {
                    //Downloadfile(this.textBox1.Text, listBox1.Items[counter].ToString());

                }

                MessageBox.Show("File download complete!");
                ftpfiles.Clear();
            }
        }  
    }
}
