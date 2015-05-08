using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Configuration;

namespace WindowsFormsApplication10
{
    public partial class StationList : Form
    {
        private SQLiteConnection sql_con = new SQLiteConnection(ConfigurationManager.ConnectionStrings["WindowsFormsApplication10.Properties.Settings.sql_con"].ConnectionString);
        private SQLiteCommand sql_cmd;
        private SQLiteDataAdapter DBMain;
        private DataSet DSMain = new DataSet();
        private DataTable DTMain = new DataTable();

        //sql_con = new SQLiteConnection(ConfigurationManager.ConnectionStrings["WindowsFormsApplication10.Properties.Settings.sql_con"].ConnectionString);


        public StationList()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnAddStn_Click(object sender, EventArgs e)
        {
            FrmAddStn StnAdd = new FrmAddStn(DTMain,DBMain);

            StnAdd.ShowDialog();
        }

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

        private void StationList_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void BtnEditStn_Click(object sender, EventArgs e)
        {
             int rowIndex = Grid.SelectedRows[0].Index;

             FrmEditStn StnEdit = new FrmEditStn(rowIndex,DTMain,DBMain);

             StnEdit.ShowDialog();
        }

        private void Grid_Click(object sender, DataGridViewCellEventArgs e)
        {
            Recno.RecNumber = Grid.SelectedCells[0].Value.ToString();
            //MessageBox.Show(Recno.RecNumber.ToString());

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(Grid.SelectedCells[0].Value.ToString());
            sql_con.Open();

            SQLiteCommand cmd = new SQLiteCommand("Delete From Main where StationCode = '" + Grid.SelectedCells[0].Value.ToString() + "'", sql_con);

            cmd.ExecuteNonQuery();
            DTMain.AcceptChanges();
            DBMain.UpdateCommand = cmd;
            DBMain.Update(DTMain);

            sql_con.Close();

            LoadData();

            MessageBox.Show("Station Information Deleted Successfully.");
            //Close();
        }
    }
}
