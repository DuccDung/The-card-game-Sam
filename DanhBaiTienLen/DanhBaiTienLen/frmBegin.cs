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
            StringBuilder output = new StringBuilder();//lưu thông tin
            output.Append("\t  ĐỒ ÁN CUỐI KỲ KĨ THUẬT LẬP TRÌNH");
            output.Append("\r\n                                           \t\t-Lớp chiều thứ 4-");
            output.Append("\r\n                           Giáo viên: Nguyễn Thiên Bảo");
            output.Append("\n\r\n                              GAME ĐÁNH BÀI TIẾN LÊN\n");
            output.Append("\r\n Thành viên:    Lê Tấn Khang                            15110229");
            output.Append("\r\n                          Dương Hồng Phúc                   15110278");
            output.Append("\r\n                          Trần Kim Hoàng                        15110211");
            output.Append("\r\n                          Võ Hoàng Hà                             15110195");
            MessageBox.Show(output.ToString(), "Tác giả", MessageBoxButtons.OK);//show 
        }
    }
}
