using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DanhBaiTienLen
{
    public partial class frmBegin : Form
    {
        public frmBegin()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            frmMain m = new frmMain();
            m.ShowDialog();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void frmBegin_Load(object sender, EventArgs e)
        {
            this.BackgroundImage = Image.FromFile("Resources\\Bg-Begin-01.jpg");
        }

        private void btnRule_Click(object sender, EventArgs e)
        {
            frmRule r = new frmRule();
            r.ShowDialog();
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            
        }
    }
}
