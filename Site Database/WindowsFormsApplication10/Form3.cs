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
    public partial class FrmEditStn : Form
    {
        private SQLiteConnection sql_con = new SQLiteConnection(ConfigurationManager.ConnectionStrings["WindowsFormsApplication10.Properties.Settings.sql_con"].ConnectionString);
        private SQLiteCommand sql_cmd;
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

            dr["StnAddress"] = this.txtIPAdd.Text;
            dr["StationCode"] = this.TxtStnCode.Text;
            //SQLiteCommandBuilder sqlcmdb = new SQLiteCommandBuilder(DAMain);
            //DAMain.Update(DTMain);
            //DAMain.DeleteCommand;
            //MessageBox.Show(dr["StationCode"].ToString());
            //SQLiteCommand cmd = new SQLiteCommand("Update main set StnAddress = @StnAdd where StationCode = @StnCode", sql_con);
            SQLiteCommand cmd = new SQLiteCommand("Update main set StnAddress = '" + txtIPAdd.Text + "' where StationCode = '" + TxtStnCode.Text + "'", sql_con);

            //MessageBox.Show(cmd.CommandText.ToString());

            //cmd.Parameters.Add("@StnAdd",SqlDbType.VarChar,255,this.txtIPAdd.ToString());
            //cmd.Parameters.Add("@StnAdd", DbType.String, 255, "StnAddress");
            //SQLiteParameter parameter = cmd.Parameters.Add("@StnCode", DbType.String,10,"StationCode");

            //parameter.SourceVersion = DataRowVersion.Original;
            cmd.ExecuteNonQuery();
            DTMain.AcceptChanges();
            DAMain.UpdateCommand = cmd;
            DAMain.Update(DTMain);

            sql_con.Close();

            MessageBox.Show("Station Information Updated Successfully.");
            Close();
        }

        private void FrmEditStn_Load(object sender, EventArgs e)
        {
            //MessageBox.Show(rowIndex.ToString());
            LoadData();
        }

        public void LoadData()
        {
            DataRow dr = DTMain.Rows[rowIndex];
            //MessageBox.Show(dr["StationCode"].ToString());
            this.TxtStnCode.Text = dr["StationCode"].ToString();
            this.txtIPAdd.Text = dr["StnAddress"].ToString();
            this.txtUserName.Text = dr["UserName"].ToString();
            this.txtPassword.Text =dr["Password"].ToString();
            //DTMain.Rows.Add(dr);

            //SQLiteCommandBuilder sqlcmdb = new SQLiteCommandBuilder(da);
            //da.Update(dt);

            //MessageBox.Show("Station/Site added to the list.");
            //Close();
        }
    }
}
