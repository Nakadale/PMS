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
    public partial class TeqcSet : Form
    {
        private SQLiteConnection sql_con = new SQLiteConnection(ConfigurationManager.ConnectionStrings["PageNet_AutoDownloader.Properties.Settings.sql_con"].ConnectionString);
        private SQLiteCommand sql_cmd;
        private SQLiteDataAdapter DBMain;
        private SQLiteDataReader reader;

        public TeqcSet()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            sql_con.Open();
            string CommandText = "Update TeqC set TeqC_Argument='" + this.textBox1.Text + "'";
            SQLiteCommand command = new SQLiteCommand(CommandText, sql_con);
            command.ExecuteNonQuery();
            sql_con.Close();
            MessageBox.Show("Settings Saved");
            Main M = new Main();
            M.LoadDesti();
        }

        private void TeqcSet_Load(object sender, EventArgs e)
        {
            // loads all station information from DB.

            sql_con.Open();
            string CommandText = "Select * from Teqc";
            SQLiteCommand command = new SQLiteCommand(CommandText, sql_con);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                this.textBox1.Text = reader["TeqC_Argument"].ToString();
            }
            sql_con.Close();
        }
    }
}
