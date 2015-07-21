using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PageNet_AutoDownloader

{
    public partial class FrmAddStn : Form
    {
        public static DataSet DS = new DataSet();
        DataTable dt;
        SQLiteDataAdapter da;

        public FrmAddStn(DataTable _dt, SQLiteDataAdapter _da)
        {
            InitializeComponent();
            dt = _dt;
            da = _da;
        }

        public FrmAddStn()
        {
            InitializeComponent();
            
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {

            DataRow dr = dt.NewRow();
            dr["StationCode"] = this.TxtStnCode.Text;
            dr["StnAddress"] = this.txtIPAdd.Text;
            dr["UserName"] = this.txtUserName.Text;
            dr["Password"] = this.txtPassword.Text;
            dt.Rows.Add(dr);

            SQLiteCommandBuilder sqlcmdb = new SQLiteCommandBuilder(da);
            da.Update(dt);

            MessageBox.Show("Station/Site added to the list.");
            Main M = new Main();
            M.LoadData();
            Close();
        }

        private void FrmAddStn_Load(object sender, EventArgs e)
        {
            
        }

    }
}
