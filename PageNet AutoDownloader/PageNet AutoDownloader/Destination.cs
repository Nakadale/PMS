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
using System.Configuration;
using System.Data.SQLite;
using System.IO;

namespace PageNet_AutoDownloader
{
    public partial class Destination : Form
    {
        public Destination()
        {
            InitializeComponent();
        }

        // SQLite Connection Parameters
        private SQLiteConnection sql_con = new SQLiteConnection(@"Data Source=" + Application.StartupPath + @"\StationList.db;Version=3;New=False;Compress=True;");
        private SQLiteCommand sql_cmd;
        private SQLiteDataAdapter DBMain;
        private SQLiteDataReader reader;
        //private DataSet DSMain = new DataSet();
        //private DataTable DTMain = new DataTable();

        private void Form1_Load(object sender, EventArgs e)
        {
            // loads all station information from DB.

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

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sql_con.Open();
            string CommandText = "Update DestinationServer set File_Location='" + this.textBox8.Text + "',User_ID='" + this.textBox7.Text + "',Password='" + this.textBox6.Text + "'";
            SQLiteCommand command = new SQLiteCommand(CommandText, sql_con);
            command.ExecuteNonQuery();
            sql_con.Close();
            MessageBox.Show("Settings Saved");
            Main M = new Main();
            M.LoadDesti();
        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }
    }
}
