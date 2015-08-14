using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Net;
using Shared_Folder_Login;

namespace PageNet_AutoDownloader
{
        public delegate void ProgressChangeDelegate(double Persentage, ref bool Cancel);
        public delegate void Completedelegate();
       
        class CustomFileCopier
        {
            public CustomFileCopier(string Source, string Dest, string Username, string password, string location)
            {
                this.SourceFilePath = Source;
                this.DestFilePath = Dest;
                this._Username = Username;
                this._Password = password;
                this._Location = location;

                OnProgressChanged += delegate { };
                OnComplete += delegate { };
            }

            public void Copy()
            {
                byte[] buffer = new byte[1024 * 1024]; // 1MB buffer
                bool cancelFlag = false;

                if (File.Exists(@DestFilePath) == true)
                {
                    File.Delete(DestFilePath);
                }
                    using (FileStream source = new FileStream(SourceFilePath, FileMode.Open, FileAccess.Read))
                    {
                        long fileLength = source.Length;
                        using (FileStream dest = new FileStream(DestFilePath, FileMode.CreateNew, FileAccess.Write))
                        {

                            long totalBytes = 0;
                            int currentBlockSize = 0;

                            while ((currentBlockSize = source.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                totalBytes += currentBlockSize;
                                double persentage = (double)totalBytes * 100.0 / fileLength;

                                dest.Write(buffer, 0, currentBlockSize);

                                cancelFlag = false;
                                OnProgressChanged(persentage, ref cancelFlag);

                                if (cancelFlag == true)
                                {
                                    // Delete dest file here
                                    break;
                                }
                            }
                        }
                    }
                OnComplete();
            }

            public string SourceFilePath { get; set; }
            public string DestFilePath { get; set; }
            public string _Username { get; set; }
            public string _Password { get; set; }
            public string _Location { get; set; }
            

            public event ProgressChangeDelegate OnProgressChanged;
            public event Completedelegate OnComplete;
        }

        class Recno
        {
            public static string RecNumber;
        }
}


