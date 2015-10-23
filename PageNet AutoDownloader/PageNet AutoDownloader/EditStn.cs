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

namespace PageNet_AutoDownloader
{
    public partial class FrmEditStn : Form
    {
        private SQLiteConnection sql_con = new SQLiteConnection(@"Data Source=" + Application.StartupPath + @"\StationList.db;Version=3;New=False;Compress=True;");
        private SQLiteDataAdapter DAMain;
        private DataSet DSMain = new DataSet();
        private DataTable DTMain = new DataTable();
        public int rowIndex;

        public FrmEditStn(int _rowIndex, DataTable _dt, SQLiteDataAdapter _da)
        {
            InitializeComponent();
            rowIndex = _rowIndex;
            DTMain = _dt;
            DAMain = _da;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            sql_con.Open();

            DataRow dr = DTMain.Rows[rowIndex];

            dr["FTPAddress"] = this.txtIPAdd.Text;
            dr["StationCode"] = this.TxtStnCode.Text;

            SQLiteCommand cmd = new SQLiteCommand("Update main set FTPAddress = '" + txtIPAdd.Text + "', UserName = '" + txtUserName.Text + "', Password = '" + txtPassword.Text + "' where StationCode = '" + TxtStnCode.Text + "'", sql_con);

            cmd.ExecuteNonQuery();
            DTMain.AcceptChanges();
            DAMain.UpdateCommand = cmd;
            DAMain.Update(DTMain);

            sql_con.Close();

            MessageBox.Show("Station Information Updated Successfully.");
            Main M = new Main();
            M.LoadData();
            Close();
        }

        private void FrmEditStn_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        public void LoadData()
        {
            DataRow dr = DTMain.Rows[rowIndex];

            this.TxtStnCode.Text = dr["StationCode"].ToString();
            this.txtIPAdd.Text = dr["FTPAddress"].ToString();
            this.txtUserName.Text = dr["UserName"].ToString();
            this.txtPassword.Text =dr["Password"].ToString();

        }
    }
}
