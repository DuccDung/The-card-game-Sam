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
    public partial class frmMain : Form
    {
        List<int> listPlayer;  
        List<int> listComputer;
        int[] arrStt;   //mảng trạng thái 1, 0
        List<int> listGO;
        List<int> listComGO;
        List<int> listTable = new List<int>();

        //
        Point[] loc1 = new Point[13] { new Point(33, 46), new Point(94, 46) , new Point(155, 46), new Point(216, 46),
                                       new Point(277, 46),new Point(337, 46),new Point(398, 46),new Point(459, 46),new Point(520, 46),
                                       new Point(581, 46),new Point(641, 46),new Point(702, 46),new Point(763, 46)};
        Point[] loc2 = new Point[13] { new Point(33, 6), new Point(94, 6) , new Point(155, 6), new Point(216, 6),
                                       new Point(277, 6),new Point(337, 6),new Point(398, 6),new Point(459, 6),new Point(520, 6),
                                       new Point(581, 6),new Point(641, 6),new Point(702, 6),new Point(763, 6)};
        public frmMain()
        {
            InitializeComponent();

            prbCoolDown.Step = Const.coolDownStep;
            prbCoolDown.Maximum = Const.coolDownTime;
            prbCoolDown.Value = 0;

            tmrCoolDown.Interval = Const.coolDownInterval;
            tmrComCD.Interval = Const.comInterval;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            this.BackgroundImage = Image.FromFile("Resources\\Bg-02.jpg");
            newGame();
            exception();    //xét ăn trắng
            if (listComputer[0] == 0)   //3 bích máy đánh trc
            {
                comNext();
            }
            else
            { playerNext(); }
            
        }
        public void newGame()
        {
            tmrCoolDown.Stop();
            //
            listPlayer = new List<int>();
            listComputer = new List<int>();
            arrStt = new int[13];
            //đổ giá trị
            Random rnd = new Random();
            int t1 = 0, t2 = 0;
            List<int> lP = new List<int>();
            for (int i = 0; i < 52; i++)
            {
                lP.Add(i);
            }
            for (int i = 0; i < 13; i++)
            {
                t1 = rnd.Next(lP.Count);
                listPlayer.Add(lP[t1]);
                lP.RemoveAt(t1);
            }
            for (int i = 0; i < 13; i++)
            {
                t2 = rnd.Next(lP.Count);
                listComputer.Add(lP[t2]);
                lP.RemoveAt(t2);
            }
            //sắp bài
            listPlayer.Sort();
            listComputer.Sort();
            //đổ ảnh player
            foreach (PictureBox pl in this.pnlPlayer.Controls)
            {
                    for (int i = 0; i < listPlayer.Count; i++)
                    {
                        if (pl.Name == "pl" + i)
                            pl.Image = getImg(listPlayer[i]);
                    }
            }
            //đổ ảnh com
            foreach (Control cC in this.Controls)
            {
                if (cC.Name.StartsWith("c"))
                {
                    PictureBox c = (PictureBox)cC;
                    c.Image = Image.FromFile("Resources\\Z2.png");
                }
            }

            //foreach (Control com in this.Controls)
            //{
            //    if (com.Name.StartsWith("c"))
            //    {
            //        PictureBox c = (PictureBox)com;
            //        for (int j = 0; j < listComputer.Count; j++)
            //        {
            //            if (c.Name == "c" + (j + 1))
            //            {
            //                c.Image = getImg(listComputer[j]);
            //            }
            //        }

            //    }
            //}
        }
        List<int> lstTest;
        public void exception() //ăn trắng (lốc 12 con hoặc 4 con heo hoặc 6 đôi)
        {

            lstTest = new List<int>();
            if (checkLoc(listComputer, 0, 12, ref lstTest) || check4(listComputer, 9, ref lstTest) || SixPairs(listComputer))
            {
                //mở các lá bài lên
                foreach (Control com in this.Controls)
                {
                    if (com.Name.StartsWith("c"))
                    {
                        PictureBox c = (PictureBox)com;
                        for (int j = 0; j < listComputer.Count; j++)
                        {
                            if (c.Name == "c" + (j + 1))
                            {
                                c.Image = getImg(listComputer[j]);
                            }
                        }

                    }
                }
                MessageBox.Show("Máy ăn trắng!! Bạn đã thua!!");
                this.Close();
                Application.Restart();
            }
            if (checkLoc(listPlayer, 0, 12, ref lstTest) || check4(listPlayer, 9, ref lstTest) || SixPairs(listPlayer))
            {
                //mở các lá bài lên
                foreach (Control com in this.Controls)
                {
                    if (com.Name.StartsWith("c"))
                    {
                        PictureBox c = (PictureBox)com;
                        for (int j = 0; j < listComputer.Count; j++)
                        {
                            if (c.Name == "c" + (j + 1))
                            {
                                c.Image = getImg(listComputer[j]);
                            }
                        }

                    }
                }
                MessageBox.Show("Bạn ăn trắng!! Bạn đã thắng!!");
                this.Close();
                Application.Restart();
            }
        }
        public bool SixPairs(List<int> l) //ktra 6 đôi ăn trắng
        {
            for(int i=0; i< 11; i++)
            {
                if (i % 2 == 0)
                {
                    if (getRank(l[i]) != getRank(l[i + 1]))
                        return false;
                }
            }
            return true ;
        }
        Image getImg(int i)
        {
            Image img;
            if (i > 51)
                img = null;
            else
                img = Image.FromFile("Resources\\" + i + ".png");
            return img;
        }   //lấy ảnh lá bài
        private void card_Click(object sender, EventArgs e)
        {
            PictureBox pic = (PictureBox)sender;
            int i = Int32.Parse(pic.Name.Remove(0, 2));
            if (pic.Location == loc1[i])
            {
                pic.Location = loc2[i];
                arrStt[i] = 1;
            }
            else
            {
                pic.Location = loc1[i];
                arrStt[i] = 0;
            }
        }
        //btn đánh
        private void btnGo_Click(object sender, EventArgs e)
        {
            listGO = new List<int>();
            for (int i = 0; i < listPlayer.Count; i++)
            {
                if (arrStt[i] == 1)
                {
                    listGO.Add(listPlayer[i]);
                }
            }
            if (isValid(listGO))
            {
                K(listGO.Count());
                comNext();
            }
        }
        //ktra hợp lệ
        public bool isValid(List<int> a)
        {
            if(listTable.Count()==0)    //bắt đầu chơi hoặc đối thủ bỏ lượt
            {
                switch(a.Count())
                {
                    case 1:
                        return true;
                    case 2:
                        if (isSameRank(a))
                            return true;
                        return false;
                    case 3:
                        if(isSameRank(a))
                            return true;
                        if (isContinuous(a))
                            return true;
                        return false;
                    case 4:
                        if (isContinuous(a))
                            return true;
                        if (isSameRank(a))
                            return true;
                        return false;
                    case 5:
                        if (isContinuous(a))
                            return true;
                        return false;
                    case 6:
                        if (isContinuous(a))
                            return true;
                        if (isConsecutivePairs(a) == 3)
                            return true;
                        return false;
                    case 7:
                        if (isContinuous(a))
                            return true;
                        return false;
                    case 8:
                        if (isContinuous(a))
                            return true;
                        if (isConsecutivePairs(a) == 4)
                            return true;
                        return false;
                    case 9:
                        if (isContinuous(a))
                            return true;
                        return false;
                    case 10:
                        if (isContinuous(a))
                            return true;
                        if (isConsecutivePairs(a) == 5)
                            return true;
                        return false;
                    case 11:
                        if (isContinuous(a))
                            return true;
                        return false;
                    default:
                        return false;
                }
            }
            else   // theo lượt
            {
                switch (a.Count())
                {
                    case 1:
                        if(listTable.Count==1 && a[0]>listTable[0])
                            return true;
                        return false;
                    case 2:
                        if (listTable.Count == 2 && isSameRank(a) && a[1] > listTable[1])
                            return true;
                        return false;
                    case 3:
                        if (listTable.Count == 3)
                        {
                            if (isSameRank(listTable) && isSameRank(a) && a[2] > listTable[2])
                                return true;
                            if (isContinuous(listTable) && isContinuous(a) && a[2] > listTable[2])
                                return true;
                        }
                        return false;
                    case 4:
                        if (listTable.Count == 1 && getRank(listTable[0]) == 12 && isSameRank(a))  //(tứ quý)chặt 1 heo
                            return true;
                        if (listTable.Count == 2 && getRank(listTable[0]) == 12 && isSameRank(a))  //(tứ quý)chặt 2 heo
                            return true;
                        if (listTable.Count == 6 && isConsecutivePairs(listTable) == 3 && isSameRank(a))   //(tứ quý)chặt 3 đôi thông
                            return true;
                        if (listTable.Count == 4)
                        {
                            if (isContinuous(listTable) && isContinuous(a) && a[3] > listTable[3])
                                return true;
                            if (isSameRank(listTable) && isSameRank(a) && a[3] > listTable[3])
                                return true;
                        }
                        return false;
                    case 5:
                        if (listTable.Count==5 && isContinuous(listTable) && isContinuous(a) && a[4] > listTable[4])
                            return true;
                        return false;
                    case 6:
                        if (listTable.Count == 1 && getRank(listTable[0]) == 12 && isConsecutivePairs(a) == 3)  //(3 đôi thông)chặt 1 heo
                            return true;
                        if (listTable.Count == 6)
                        {
                            if (isContinuous(listTable) && isContinuous(a) && a[5] > listTable[5])
                                return true;
                            if (isConsecutivePairs(listTable) == 3 && isConsecutivePairs(a) == 3 && a[5] > listTable[5])
                                return true;
                        }
                        return false;
                    case 7:
                        if (listTable.Count == 7 && isContinuous(a) && isContinuous(listTable) && a[6]> listTable[6])
                            return true;
                        return false;
                    case 8:
                        if (listTable.Count == 1 && getRank(listTable[0]) == 12 && isConsecutivePairs(a) == 4)  //(4 đôi thông)chặt 1 heo
                            return true;
                        if (listTable.Count == 2 && getRank(listTable[0]) == 12 && isConsecutivePairs(a) == 4)  //(4 đôi thông)chặt 2 heo
                            return true;
                        if (listTable.Count == 4 && isSameRank(listTable) && isConsecutivePairs(a) == 4)   //(4 đôi thông)chặt tứ quý
                            return true;
                        if (listTable.Count == 6 && isConsecutivePairs(a) == 3 && isConsecutivePairs(a) == 4)    //(4 đôi thông)chặt 3 đôi thông
                            return true;
                        if (listTable.Count == 8)
                        {
                            if (isContinuous(listTable) && isContinuous(a) && a[7] > listTable[7])
                                return true;
                            if (isConsecutivePairs(listTable) == 4 && isConsecutivePairs(a) == 4 && a[7] > listTable[7])
                                return true;
                        }
                        return false;
                    case 9:
                        if (listTable.Count == 9 && isContinuous(a) && isContinuous(listTable) && a[8] > listTable[8])
                            return true;
                        return false;
                    case 10:
                        if (listTable.Count == 1 && getRank(listTable[0]) == 12 && isConsecutivePairs(a) == 5)  //(5 đôi thông)chặt 1 heo
                            return true;
                        if (listTable.Count == 2 && getRank(listTable[0]) == 12 && isConsecutivePairs(a) == 5)  //(5 đôi thông)chặt 2 heo
                            return true;
                        if (listTable.Count == 4 && isSameRank(listTable) && isConsecutivePairs(a) == 5)   //(5 đôi thông)chặt tứ quý
                            return true;
                        if (listTable.Count == 6 && isConsecutivePairs(listTable) == 3 && isConsecutivePairs(a) == 5)    //(5 đôi thông)chặt 3 đôi thông
                            return true;
                        if (listTable.Count == 8 && isConsecutivePairs(listTable) == 4 && isConsecutivePairs(a) == 5)    //(5 đôi thông)chặt 4 đôi thông
                            return true;
                        if (listTable.Count == 10)
                        {
                            if (isContinuous(listTable) && isContinuous(a) && a[9] > listTable[9])
                                return true;
                            if (isConsecutivePairs(listTable) == 5 && isConsecutivePairs(a) == 5 && a[9] > listTable[9])
                                return true;
                        }
                        return false;
                    case 11:
                        if (listTable.Count == 11 && isContinuous(a) && isContinuous(listTable) && a[10] > listTable[10])
                            return true;
                        return false;
                    default:
                        return false;
                }
            }
        }
        //ktra 3 - 4 - 5 đôi thông
        public int isConsecutivePairs(List<int> l) 
        {
            int k=(int)l.Count()/2;
            for(int i=0;i<(l.Count()-1);i++)
            {
                if (i % 2 == 0)
                {
                    if (getRank(l[i]) != getRank(l[i + 1]))
                        return -1;
                }
                else
                {
                    if (getRank(l[i + 1]) != (getRank(l[i]) + 1))
                        return -1;
                }
            }
            return k;
        }
        //ktra cùng bộ
        public bool isSameRank(List<int> l) 
        {
            for (int i = 0; i < (l.Count() - 1); i++)
                if (getRank(l[i]) != getRank(l[i + 1]))
                    return false;
            return true;
        }
        //ktra lốc liên tục
        public bool isContinuous(List<int> l)   
        {
            if (getRank(l[l.Count - 1]) == 12)
                return false;
            for (int i=0; i<(l.Count()-1);i++)
                if(getRank(l[i+1])!=(getRank(l[i])+1))
                    return false;
            return true ;
        }
        //bộ
        public int getRank(int k)   
        {
            int s = (int)k / 4;
            return s;
        }
        //Người đánh bài
        public void K(int i)
        {
            //reset Table
            removeAll(listTable);
            //chuyển vào list Table
            for (int j = 0; j < listGO.Count; j++)
                listTable.Add(listGO[j]);
            //chuyển ảnh lên Table
            foreach (PictureBox cT in this.pnlTableCards.Controls)
            {
                for (int j = 0; j < i; j++)
                    if (cT.Name == "t" + j)
                    {
                        cT.Visible = true;
                        cT.Image = getImg(listTable[j]);
                    }
                for(int j=i;j<12;j++)
                    if (cT.Name == "t" + j)
                    {
                        cT.Visible = false;
                    }
            }
            //remove giá trị trong listPlayer, reset mảng arrStt
            arrStt = new int[13];
            for (int j = 0; j < listTable.Count; j++)
            {
                listPlayer.Remove(listTable[j]);
            }
            //đổ ảnh player
            foreach (PictureBox pl in this.pnlPlayer.Controls)
            {
                pl.Image = null;
                pl.Enabled = false;
                for (int j = 0; j < listPlayer.Count; j++)
                {
                    if (pl.Name == "pl" + j)
                    {
                        pl.Image = getImg(listPlayer[j]);
                        pl.Enabled = true;
                    }
                }
            }
            //reset location
            foreach (PictureBox loc in this.pnlPlayer.Controls)
            {
                    for (int j = 0; j < 13; j++)
                    {
                        if (loc.Name == "pl" + j)
                            loc.Location = loc1[j];
                    }
            }
            isPlayerWIN();
        }
        public void playerNext()    //lượt người
        {
            tmrComCD.Stop();
            //pnlButton.Visible = true;
            prbCoolDown.Visible = true;
            //pnlPlayer.Enabled = true;
            pnlButton.Enabled = true;
            //prbCoolDown.Enabled = true;

            tmrCoolDown.Start();
            prbCoolDown.Value = 0;
        }
        public void comNext()   //lượt máy
        {
            tmrCoolDown.Stop();
            //pnlPlayer.Enabled = false;
            //pnlButton.Visible = false;
            prbCoolDown.Visible = false;
            pnlButton.Enabled = false;
            //prbCoolDown.Enabled = false;

            tmrComCD.Start();
            prbCoolDown.Value = 0;
            
        }
        private void tmrComCD_Tick(object sender, EventArgs e)
        {
            ssj();
            playerNext();
        }
        private void tmrCoolDown_Tick(object sender, EventArgs e)
        {
            prbCoolDown.PerformStep();

            if (prbCoolDown.Value >= prbCoolDown.Maximum)
            {
                comNext();
            }
        }
        public void removeAll(List<int> l)  //remove all
        {
            for (int i = 0; i < l.Count; i++)
            {
                l.RemoveAt(i);
                i--;
            }
        }
        //Máy đánh
        public void ssj()
        {
            listComGO = new List<int>();
            if (listTable.Count == 0)   //
            {
                firstOff();
            }
            else
            {
                switch (listTable.Count)
                {
                    case 1:
                        for (int i = 0; i < listComputer.Count; i++)
                        {
                            if (check4pairs(listComputer, ref listComGO) && isValid(listComGO))   //4 đôi thông chặt heo
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                            if (check4(listComputer, i, ref listComGO) && isValid(listComGO))   //tứ quý chặt heo
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                            if (check3pairs(listComputer, ref listComGO) && isValid(listComGO))   //3 đôi thông chặt heo
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                            if (listComputer[i] > listTable[0]) //rác
                            {
                                listComGO.Add(listComputer[i]);
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                        }
                        boqua();
                        break;
                    case 2:
                        for (int i = 0; i < listComputer.Count; i++)
                        {
                            if (check4pairs(listComputer, ref listComGO) && isValid(listComGO))   //4 đôi thông ăn đôi heo
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                            if (check4(listComputer, i, ref listComGO) && isValid(listComGO))   //tứ quý chặt đôi heo
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                            if (check2(listComputer, i, ref listComGO) && isValid(listComGO))   //đôi
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                        }
                        boqua();
                        break;
                    case 3:
                        for (int i = 0; i < listComputer.Count; i++)
                        {
                            if (check3(listComputer, i, ref listComGO) && isValid(listComGO))   //3
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                            if (checkLoc(listComputer, i, 3, ref listComGO) && isValid(listComGO))  //lốc 3
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                        }
                        boqua();
                        break;
                    case 4:
                        for (int i = 0; i < listComputer.Count; i++)
                        {
                            if (check4pairs(listComputer, ref listComGO) && isValid(listComGO))   //4 đôi thông ăn tứ quý
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                            if (check4(listComputer, i, ref listComGO) && isValid(listComGO))   //tứ quý ăn tứ quý
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                            if (checkLoc(listComputer, i, 4, ref listComGO) && isValid(listComGO))  //lốc 4
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                        }
                        boqua();
                        break;
                    case 5:
                        for (int i = 0; i < listComputer.Count; i++)
                        {
                            if (checkLoc(listComputer, i, 5, ref listComGO) && isValid(listComGO))  //lốc 5
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                        }
                        boqua();
                        break;
                    case 6:
                        for (int i = 0; i < listComputer.Count; i++)
                        {
                            if (check3pairs(listComputer, ref listComGO) && isValid(listComGO))   //3 đôi thông ăn 3 đôi thông
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                            if (checkLoc(listComputer, i, 6, ref listComGO) && isValid(listComGO))  //lốc 6
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                        }
                        boqua();
                        break;
                    case 7:
                        for (int i = 0; i < listComputer.Count; i++)
                        {
                            if (checkLoc(listComputer, i, 7, ref listComGO) && isValid(listComGO))  //lốc 7
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                        }
                        boqua();
                        break;
                    case 8:
                        for (int i = 0; i < listComputer.Count; i++)
                        {
                            if (check4pairs(listComputer, ref listComGO) && isValid(listComGO))   //4 đôi thông ăn 4 đôi thông
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                            if (checkLoc(listComputer, i, 8, ref listComGO) && isValid(listComGO))  //lốc 8
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                        }
                        boqua();
                        break;
                    case 9:
                        for (int i = 0; i < listComputer.Count; i++)
                        {
                            if (checkLoc(listComputer, i, 9, ref listComGO) && isValid(listComGO))  //lốc 9
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                        }
                        boqua();
                        break;
                    case 10:
                        for (int i = 0; i < listComputer.Count; i++)
                        {
                            if (checkLoc(listComputer, i, 10, ref listComGO) && isValid(listComGO))  //lốc 10
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                        }
                        boqua();
                        break;
                    case 11:
                        for (int i = 0; i < listComputer.Count; i++)
                        {
                            if (checkLoc(listComputer, i, 11, ref listComGO) && isValid(listComGO))  //lốc 11
                            {
                                KK(listComGO);
                                isComWIN();
                                return;
                            }
                            else
                            { removeAll(listComGO); }
                        }
                        boqua();
                        break;
                    default:
                        boqua();
                        break;
                }
            }
            isComWIN();
        }
        //
        List<int> lstcheck2;
        public bool check3pairs(List<int> l, ref List<int> ll)  //3 đôi thông
        {
            lstcheck2 = new List<int>();

            for (int i = 0; i < l.Count-5; i++)
            {
                if (getRank(l[i]) == getRank(l[i + 1]) && getRank(l[i + 2]) == getRank(l[i + 3]) && getRank(l[i + 4]) == getRank(l[i + 5])
                && getRank(l[i + 2]) == getRank(l[i]) + 1 && getRank(l[i + 4]) == getRank(l[i + 2]) + 1)
                {
                    for (int j = i; j < i + 6; j++)
                        ll.Add(l[j]);
                    return true;
                }
                if (i < l.Count-6 && getRank(l[i]) == getRank(l[i + 1]) && getRank(l[i + 2]) == getRank(l[i + 3]) && getRank(l[i + 2]) == getRank(l[i + 4])
                    && getRank(l[i + 5]) == getRank(l[i + 6]) && getRank(l[i + 2]) == getRank(l[i]) + 1 && getRank(l[i + 5]) == getRank(l[i + 2]) + 1)
                {
                    ll.Add(l[i]);
                    ll.Add(l[i + 1]);
                    ll.Add(l[i + 2]);
                    ll.Add(l[i + 3]);
                    ll.Add(l[i + 5]);
                    ll.Add(l[i + 6]);
                    return true;
                }
            }
            return false;
        }
        public bool check4pairs(List<int> l, ref List<int> ll)  //4 đôi thông
        {
            lstcheck2 = new List<int>();
            for (int i = 0; i < l.Count-7; i++)
            {
                if (getRank(l[i]) == getRank(l[i + 1]) && getRank(l[i + 2]) == getRank(l[i + 3]) && getRank(l[i + 4]) == getRank(l[i + 5]) && getRank(l[i + 6]) == getRank(l[i + 7])
                    && getRank(l[i + 2]) == getRank(l[i]) + 1 && getRank(l[i + 4]) == getRank(l[i + 2]) + 1 && getRank(l[i + 6]) == getRank(l[i + 4]) + 1)
                {
                    for (int j = i; j < i + 8; j++)
                        ll.Add(l[j]);
                    return true;
                }
                if (i<l.Count-8 && getRank(l[i]) == getRank(l[i + 1]) && getRank(l[i + 2]) == getRank(l[i + 3]) && getRank(l[i + 2]) == getRank(l[i + 4]) && getRank(l[i + 5]) == getRank(l[i + 6]) && getRank(l[i + 7]) == getRank(l[i + 8])
                    && getRank(l[i + 2]) == getRank(l[i]) + 1 && getRank(l[i + 5]) == getRank(l[i + 2]) + 1 && getRank(l[i + 7]) == getRank(l[i + 5]) + 1)
                {
                    ll.Add(l[i]);
                    ll.Add(l[i + 1]);
                    ll.Add(l[i + 2]);
                    ll.Add(l[i + 3]);
                    ll.Add(l[i + 5]);
                    ll.Add(l[i + 6]);
                    ll.Add(l[i + 7]);
                    ll.Add(l[i + 8]);
                    return true;
                }
                if (i < l.Count-8 && getRank(l[i]) == getRank(l[i + 1]) && getRank(l[i + 2]) == getRank(l[i + 3]) && getRank(l[i + 4]) == getRank(l[i + 5]) && getRank(l[i + 4]) == getRank(l[i + 6]) && getRank(l[i + 7]) == getRank(l[i + 8])
                    && getRank(l[i + 2]) == getRank(l[i]) + 1 && getRank(l[i + 4]) == getRank(l[i + 2]) + 1 && getRank(l[i + 7]) == getRank(l[i + 4]) + 1)
                {
                    ll.Add(l[i]);
                    ll.Add(l[i + 1]);
                    ll.Add(l[i + 2]);
                    ll.Add(l[i + 3]);
                    ll.Add(l[i + 4]);
                    ll.Add(l[i + 5]);
                    ll.Add(l[i + 7]);
                    ll.Add(l[i + 8]);
                    return true;
                }
                if (i < l.Count-9 && getRank(l[i]) == getRank(l[i + 1]) && getRank(l[i + 2]) == getRank(l[i + 3]) && getRank(l[i + 2]) == getRank(l[i + 4])
                    && getRank(l[i + 5]) == getRank(l[i + 6]) && getRank(l[i + 5]) == getRank(l[i + 7]) && getRank(l[i + 8]) == getRank(l[i + 9]) 
                    && getRank(l[i + 2]) == getRank(l[i]) + 1 && getRank(l[i + 5]) == getRank(l[i + 2]) + 1 && getRank(l[i + 8]) == getRank(l[i + 5]) + 1)
                {
                    ll.Add(l[i]);
                    ll.Add(l[i + 1]);
                    ll.Add(l[i + 2]);
                    ll.Add(l[i + 3]);
                    ll.Add(l[i + 5]);
                    ll.Add(l[i + 6]);
                    ll.Add(l[i + 8]);
                    ll.Add(l[i + 9]);
                    return true;
                }
            }
            return false;
        }
        public bool check2(List<int> l, int i, ref List<int> ll)    //đôi
        {
            if(i<(l.Count-1) && getRank(l[i])== getRank(l[i+1]))
            {
                ll.Add(l[i]);
                ll.Add(l[i + 1]);
                return true;
            }
            return false;
        }
        public bool check3(List<int> l, int i, ref List<int> ll)    //ba
        {
            if (i < (l.Count - 2) && getRank(l[i]) == getRank(l[i + 1]) && getRank(l[i]) == getRank(l[i+2]))
            {
                ll.Add(l[i]);
                ll.Add(l[i + 1]);
                ll.Add(l[i + 2]);
                return true;
            }
            return false;
        }
        public bool check4(List<int> l, int i, ref List<int> ll)    //tứ quý
        {
            if (i < (l.Count - 3) && getRank(l[i]) == getRank(l[i + 1]) && getRank(l[i]) == getRank(l[i + 2]) && getRank(l[i]) == getRank(l[i + 3]))
            {
                ll.Add(l[i]);
                ll.Add(l[i + 1]);
                ll.Add(l[i + 2]);
                ll.Add(l[i + 3]);
                return true;
            }
            return false;
        }
        List<int> lstcheck;
        public bool checkLoc(List<int> lst, int i, int loc, ref List<int> ll) //lốc
        {
            int end=0;
            if (i > (lst.Count - loc))
                return false;
            else
            {
                lstcheck = new List<int>();

                int k = i;
                for (int j=0;j<loc; j++)
                {
                    lstcheck.Add(lst[k]);
                    end = lst[k];
                    if (getRank(end) == 12)
                        return false;
                    if (j != (loc - 1) && k == lst.Count - 1)
                        return false;
                    while (k<lst.Count-1)
                    {
                        if (getRank(lst[k]) == getRank(lst[k + 1]))
                        {
                            if (j == (loc-1))
                                end = lst[k + 1];
                            k++;
                        }
                        else
                        {
                            if (getRank(lst[k + 1]) == getRank(lst[k]) + 1)
                            {
                                k++;
                                break;
                            }
                            else
                            {
                                if (j == loc - 1)
                                    break;
                                else
                                    return false;
                            }
                        }
                    }
                }
                for(int j=0;j<(loc-1);j++)
                {
                    ll.Add(lstcheck[j]);
                }
                ll.Add(end);
                return true;
            }
        }
        public void firstOff()  
        {
            if (checkLoc(listComputer, 0, 11, ref listComGO))
                KK(listComGO);
            else
            {
                removeAll(listComGO);
                if (checkLoc(listComputer, 0, 10, ref listComGO))
                    KK(listComGO);
                else
                {
                    removeAll(listComGO);
                    if (checkLoc(listComputer, 0, 9, ref listComGO))
                        KK(listComGO);
                    else
                    {
                        removeAll(listComGO);
                        if (checkLoc(listComputer, 0, 8, ref listComGO))
                            KK(listComGO);
                        else
                        {
                            removeAll(listComGO);
                            if (checkLoc(listComputer, 0, 7, ref listComGO))
                                KK(listComGO);
                            else
                            {
                                removeAll(listComGO);
                                if (checkLoc(listComputer, 0, 6, ref listComGO))
                                    KK(listComGO);
                                else
                                {
                                    removeAll(listComGO);
                                    if (checkLoc(listComputer, 0, 5, ref listComGO))
                                        KK(listComGO);
                                    else
                                    {
                                        removeAll(listComGO);
                                        if (checkLoc(listComputer, 0, 4, ref listComGO))
                                            KK(listComGO);
                                        else
                                        {
                                            removeAll(listComGO);
                                            if (checkLoc(listComputer, 0, 3, ref listComGO))
                                                KK(listComGO);
                                            else
                                            {
                                                removeAll(listComGO);
                                                if (check3(listComputer, 0, ref listComGO))
                                                    KK(listComGO);
                                                else
                                                {
                                                    removeAll(listComGO);
                                                    if (check2(listComputer, 0, ref listComGO))
                                                        KK(listComGO);
                                                    else
                                                    {
                                                        removeAll(listComGO);
                                                        listComGO.Add(listComputer[0]);
                                                        KK(listComGO);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public void boqua()
        {
            //reset Table
            for (int j = 0; j < listTable.Count; j++)
            {
                listTable.RemoveAt(j);
                j--;
            }
        }
        //Máy đánh bài
        public void KK(List<int> lst)   
        {
            //reset Table
            removeAll(listTable);
            //chuyển vào list Table
            for (int j = 0; j < lst.Count; j++)
                listTable.Add(lst[j]);
            //chuyển ảnh lên Table
            foreach (PictureBox cT in this.pnlTableCards.Controls)
            {
                for (int j = 0; j < lst.Count; j++)
                    if (cT.Name == "t" + j)
                    {
                        cT.Visible = true;
                        cT.Image = getImg(listTable[j]);
                    }
                for (int j = lst.Count; j < 12; j++)
                    if (cT.Name == "t" + j)
                    {
                        cT.Visible = false;
                    }
            }
            //remove giá trị trong listComputer
            for (int j = 0; j < listTable.Count; j++)
            {
                listComputer.Remove(listTable[j]);
            }
            //đổ ảnh computer
            foreach (Control com in this.Controls)
            {
                if (com.Name.StartsWith("c"))
                {
                    PictureBox c = (PictureBox)com;
                    c.Image = null;
                    for (int j = 0; j < listComputer.Count; j++)
                    {
                        if (c.Name == "c" + (j + 1))
                        {
                            c.Image = Image.FromFile("Resources\\Z2.png");
                        }
                    }

                }
            }
            //foreach (Control com in this.Controls)
            //{
            //    if (com.Name.StartsWith("c"))
            //    {
            //        PictureBox c = (PictureBox)com;
            //        c.Image = null;
            //        for (int j = 0; j < listComputer.Count; j++)
            //        {
            //            if (c.Name == "c" + (j + 1))
            //            {
            //                c.Image = getImg(listComputer[j]);
            //            }
            //        }
            //    }
            //}

        }
        //btn bỏ qua
        private void btnSkip_Click(object sender, EventArgs e)  //bỏ qua
        {
            comNext();
            //reset Table
            for (int j = 0; j < listTable.Count; j++)
            {
                listTable.RemoveAt(j);
                j--;
            }
        }
        //xét thắng
        public void isComWIN()
        {
            if(listComputer.Count==0)
            {
                tmrCoolDown.Stop();
                tmrComCD.Stop();
                prbCoolDown.Visible = false;

                MessageBox.Show("Bạn đã thua!", "You lose", MessageBoxButtons.OK);
                Application.Restart();
            }
        }
        public void isPlayerWIN()
        {
            if (listPlayer.Count == 0)
            {
                tmrCoolDown.Stop();
                tmrComCD.Stop();
                prbCoolDown.Visible = false;

                //mở bài của máy
                foreach (Control com in this.Controls)
                {
                    if (com.Name.StartsWith("c"))
                    {
                        PictureBox c = (PictureBox)com;
                        c.Image = null;
                        for (int j = 0; j < listComputer.Count; j++)
                        {
                            if (c.Name == "c" + (j + 1))
                            {
                                c.Image = getImg(listComputer[j]);
                            }
                        }
                    }
                }

                MessageBox.Show("Bạn đã thắng!", "You win", MessageBoxButtons.OK);
                this.Close();
                Application.Restart();
            }
        }

    }
}
