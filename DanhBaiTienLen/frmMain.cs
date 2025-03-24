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
        private Timer tmrWaitForSam;
        private int waitTime = 10; // Thời gian chờ (giây)

        bool isSamDeclared = false;
        int samPlayer = -1; // Người hô Sâm (-1: chưa ai hô, 0: người chơi, 1: máy)
        int currentPlayer = 0; // Người chơi hiện tại (0: người chơi, 1: máy)
        int lastWinner = -1; // Người thắng ván trước (-1: chưa có, 0: máy, 1: người chơi)

 
        // Các điểm đặt bài
        Point[] loc1 = new Point[10] {
            new Point(33, 46), new Point(94, 46), new Point(155, 46),
            new Point(216, 46), new Point(277, 46), new Point(337, 46),
            new Point(398, 46), new Point(459, 46), new Point(520, 46),
            new Point(581, 46) // Chỉ 10 điểm
        };

        Point[] loc2 = new Point[10] {
            new Point(33, 6), new Point(94, 6), new Point(155, 6),
            new Point(216, 6), new Point(277, 6), new Point(337, 6),
            new Point(398, 6), new Point(459, 6), new Point(520, 6),
            new Point(581, 6) // Chỉ 10 điểm
        };
        public frmMain()
        {
            InitializeComponent();
            // Khởi tạo Timer chờ báo Sâm
            tmrWaitForSam = new Timer();
            tmrWaitForSam.Interval = 1000; // 1 giây
            tmrWaitForSam.Tick += new EventHandler(tmrWaitForSam_Tick);

            prbCoolDown.Step = Const.coolDownStep;
            prbCoolDown.Maximum = Const.coolDownTime;
            prbCoolDown.Value = 0;

            tmrCoolDown.Interval = Const.coolDownInterval;
            tmrComCD.Interval = Const.comInterval;
        }
        public void ResetGame()
        {
            // Dừng các timer
            tmrCoolDown.Stop();
            tmrComCD.Stop(); // Dừng timer của máy
            tmrWaitForSam.Stop(); // Dừng timer chờ báo Sâm

            // Đặt lại các biến
            listPlayer = new List<int>();
            listComputer = new List<int>();
            arrStt = new int[13];
            listTable = new List<int>();
            listGO = new List<int>();
            listComGO = new List<int>();

            // Reset các biến liên quan đến thời gian chờ và trạng thái
            isSamDeclared = false; // Reset trạng thái báo Sâm
            samPlayer = -1; // -1: chưa ai hô Sâm
            currentPlayer = 0; // Người chơi hiện tại (0: người chơi, 1: máy)

            // Hiển thị lại thanh thời gian chờ
            lblWaitTime.Visible = true;
            lblWaitTime.Text = $"Thời gian chờ báo sâm: {waitTime} giây";

            // Bật lại nút "Báo Sâm"
            btnBao.Enabled = true;

            // Khởi tạo lại bài
            newGame();

            // Xác định người đánh đầu tiên dựa trên người thắng ván trước
            if (lastWinner == 1) // Người chơi thắng ván trước
            {
                playerNext(); // Người chơi đánh đầu tiên
            }
            else
            {
                comNext(); // Máy đánh đầu tiên
            }

            // Bắt đầu chờ báo Sâm
            StartWaitForSam();

            // Cập nhật giao diện
            UpdateUI();
        }
        public void UpdateUI()
        {
            // Đổ ảnh player
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

            // Đổ ảnh computer
            foreach (Control com in this.Controls)
            {
                if (com.Name.StartsWith("c"))
                {
                    PictureBox c = (PictureBox)com;
                    c.Image = Image.FromFile("Resources\\Z2.png");
                }
            }

            // Xóa bài trên bàn
            foreach (Control ctrl in this.pnlTableCards.Controls)
            {
                if (ctrl is PictureBox cT)
                {
                    cT.Visible = false;
                }
            }
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

            StartWaitForSam();
        }
        private void StartWaitForSam()
        {
            // Reset thời gian chờ
            waitTime = 10; // 10 giây
            lblWaitTime.Text = $"Thời gian chờ báo sâm: {waitTime} giây";

            // Vô hiệu hóa các nút và tương tác của người chơi, trừ nút "Báo"
            pnlPlayer.Enabled = false; // Vô hiệu hóa panel chứa bài của người chơi
            btnGo.Enabled = false;     // Vô hiệu hóa nút "Đánh"
            btnSkip.Enabled = false;   // Vô hiệu hóa nút "Bỏ qua"

            // Bật nút "Báo Sâm" trong thời gian chờ
            btnBao.Enabled = true;

            // Dừng timer của máy để đảm bảo máy không đánh bài trong thời gian chờ
            tmrComCD.Stop();

            // Bắt đầu Timer chờ báo Sâm
            tmrWaitForSam.Start();
        }
        public void newGame()
        {
            tmrCoolDown.Stop();

            // Khởi tạo danh sách bài
            listPlayer = new List<int>();
            listComputer = new List<int>();
            arrStt = new int[10]; // Chỉ 10 lá bài

            // Đổ giá trị bài ngẫu nhiên
            Random rnd = new Random();
            List<int> lP = new List<int>();
            for (int i = 0; i < 52; i++)
            {
                lP.Add(i);
            }

            // Chia 10 lá cho người chơi
            for (int i = 0; i < 10; i++)
            {
                int t1 = rnd.Next(lP.Count);
                listPlayer.Add(lP[t1]);
                lP.RemoveAt(t1);
            }

            // Chia 10 lá cho máy
            for (int i = 0; i < 10; i++)
            {
                int t2 = rnd.Next(lP.Count);
                listComputer.Add(lP[t2]);
                lP.RemoveAt(t2);
            }

            // Sắp xếp bài
            listPlayer.Sort();
            listComputer.Sort();

            // Hiển thị bài của người chơi
            foreach (PictureBox pl in this.pnlPlayer.Controls)
            {
                for (int i = 0; i < listPlayer.Count; i++)
                {
                    if (pl.Name == "pl" + i)
                        pl.Image = getImg(listPlayer[i]);
                }
            }

            // Hiển thị bài của máy (chỉ hiển thị 10 lá)
            foreach (Control cC in this.Controls)
            {
                if (cC.Name.StartsWith("c"))
                {
                    PictureBox c = (PictureBox)cC;
                    c.Image = Image.FromFile("Resources\\Z2.png"); // Ảnh úp bài
                }
            }
        }
        List<int> lstTest;
        public void exception()
        {
            // Kiểm tra Sảnh rồng
            if (CheckDragon(listPlayer))
            {
                MessageBox.Show("Bạn có Sảnh rồng! Bạn thắng!");
                lastWinner = 1;
                ResetGame();
            }
            if (CheckDragon(listComputer))
            {
                MessageBox.Show("Máy có Sảnh rồng! Bạn thua!");
                lastWinner = 0;
                ResetGame();
            }

            // Kiểm tra Tứ quý 2
            if (CheckQuad2(listPlayer))
            {
                MessageBox.Show("Bạn có Tứ quý 2! Bạn thắng!");
                lastWinner = 1;
                ResetGame();
            }
            if (CheckQuad2(listComputer))
            {
                MessageBox.Show("Máy có Tứ quý 2! Bạn thua!");
                lastWinner = 0;
                ResetGame();
            }

            // Kiểm tra Đồng hoa
            if (CheckSameSuit(listPlayer))
            {
                MessageBox.Show("Bạn có Đồng hoa! Bạn thắng!");
                lastWinner = 1;
                ResetGame();
            }
            if (CheckSameSuit(listComputer))
            {
                MessageBox.Show("Máy có Đồng hoa! Bạn thua!");
                lastWinner = 0;
                ResetGame();
            }

            // Kiểm tra 3 Sám
            if (CheckThreeTriples(listPlayer))
            {
                MessageBox.Show("Bạn có 3 Sám! Bạn thắng!");
                lastWinner = 1;
                ResetGame();
            }
            if (CheckThreeTriples(listComputer))
            {
                MessageBox.Show("Máy có 3 Sám! Bạn thua!");
                lastWinner = 0;
                ResetGame();
            }

            // Kiểm tra 5 Đôi
            if (CheckFivePairs(listPlayer))
            {
                MessageBox.Show("Bạn có 5 Đôi! Bạn thắng!");
                lastWinner = 1;
                ResetGame();
            }
            if (CheckFivePairs(listComputer))
            {
                MessageBox.Show("Máy có 5 Đôi! Bạn thua!");
                lastWinner = 0;
                ResetGame();
            }
        }

        // Kiểm tra Sảnh rồng
        private bool CheckDragon(List<int> hand)
        {
            if (hand.Count < 10) return false;
            for (int i = 0; i < hand.Count - 1; i++)
            {
                if (getRank(hand[i + 1]) != getRank(hand[i]) + 1)
                    return false;
            }
            return true;
        }

        // Kiểm tra Tứ quý 2
        private bool CheckQuad2(List<int> hand)
        {
            int count = 0;
            foreach (int card in hand)
            {
                if (getRank(card) == 12) // Giả sử 2 có rank = 12
                    count++;
            }
            return count >= 4;
        }

        // Kiểm tra Đồng hoa
        private bool CheckSameSuit(List<int> hand)
        {
            if (hand.Count < 10) return false;
            int suit = hand[0] % 4;
            foreach (int card in hand)
            {
                if (card % 4 != suit)
                    return false;
            }
            return true;
        }

        // Kiểm tra 3 Sám
        private bool CheckThreeTriples(List<int> hand)
        {
            int count = 0;
            for (int i = 0; i < hand.Count - 2; i++)
            {
                if (getRank(hand[i]) == getRank(hand[i + 1]) && getRank(hand[i]) == getRank(hand[i + 2]))
                {
                    count++;
                    i += 2;
                }
            }
            return count >= 3;
        }

        // Kiểm tra 5 Đôi
        private bool CheckFivePairs(List<int> hand)
        {
            int count = 0;
            for (int i = 0; i < hand.Count - 1; i++)
            {
                if (getRank(hand[i]) == getRank(hand[i + 1]))
                {
                    count++;
                    i++;
                }
            }
            return count >= 5;
        }

        // Hàm kiểm tra xem một danh sách bài có chứa tứ quý không
        private bool CheckAnyQuad(List<int> hand)
        {
            for (int i = 0; i <= hand.Count - 4; i++)
            {
                List<int> temp = new List<int>();
                if (check4(hand, i, ref temp)) // Gọi hàm check4 để kiểm tra từ vị trí i
                    return true;
            }
            return false;
        }
        public bool SixPairs(List<int> l) //ktra 6 đôi ăn trắng
        {
            for (int i = 0; i < 11; i++)
            {
                if (i % 2 == 0)
                {
                    if (getRank(l[i]) != getRank(l[i + 1]))
                        return false;
                }
            }
            return true;
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
            int i = Int32.Parse(pic.Name.Remove(0, 2)); // Lấy chỉ số của lá bài

            if (pic.Location == loc1[i]) // Nếu lá bài đang ở vị trí ban đầu
            {
                pic.Location = loc2[i]; // Di chuyển lá bài lên
                arrStt[i] = 1; // Đánh dấu lá bài được chọn
            }
            else
            {
                pic.Location = loc1[i]; // Di chuyển lá bài về vị trí ban đầu
                arrStt[i] = 0; // Đánh dấu lá bài không được chọn
            }
        }
        // Xử lý khi người chơi hô Sâm
        private void tmrWaitForSam_Tick(object sender, EventArgs e)
        {
            waitTime--; // Giảm thời gian chờ
            lblWaitTime.Text = $"Thời gian chờ: {waitTime} giây";

            if (waitTime <= 0)
            {
                // Dừng Timer chờ
                tmrWaitForSam.Stop();

                // Ẩn thanh thời gian chờ
                lblWaitTime.Visible = false;

                // Tắt nút "Báo Sâm" khi thời gian chờ kết thúc
                btnBao.Enabled = false;

                // Kích hoạt lại các nút và tương tác của người chơi
                pnlPlayer.Enabled = true; // Kích hoạt panel chứa bài của người chơi
                btnGo.Enabled = true;     // Kích hoạt nút "Đánh"
                btnSkip.Enabled = true;   // Kích hoạt nút "Bỏ qua"

                // Thông báo hết thời gian chờ
                if (!isSamDeclared)
                {
                    MessageBox.Show("Hết thời gian chờ! Trò chơi bắt đầu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // Xác định người đánh đầu tiên
                if (isSamDeclared && samPlayer == 0)
                {
                    playerNext(); // Người chơi đã báo Sâm, được đánh đầu tiên
                }
                else
                {
                    if (lastWinner == 1) // Người chơi thắng ván trước
                        playerNext(); // Người chơi đánh đầu tiên
                    else
                        comNext(); // Máy đánh đầu tiên nếu không ai báo Sâm
                }
            }
        }
        private void btnBao_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem đã có ai hô Sâm chưa
            if (isSamDeclared)
            {
                MessageBox.Show("Bạn đã hô Sâm rồi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Dừng Timer chờ
            tmrWaitForSam.Stop();

            // Ẩn thanh thời gian chờ
            lblWaitTime.Visible = false;

            // Tắt nút "Báo Sâm" khi người chơi báo Sâm
            btnBao.Enabled = false;

            // Đánh dấu là bạn đã hô Sâm
            isSamDeclared = true;
            samPlayer = 0; // 0 là người chơi (bạn)

            // Kiểm tra xem máy có chặn được bài của người chơi không
            if (CheckIfComCanBlock(listPlayer))
            {
                MessageBox.Show("Máy đã chặn được bài của bạn! Bạn thua!", "Thua cuộc", MessageBoxButtons.OK, MessageBoxIcon.Information);
                lastWinner = 0; // Máy thắng
                ResetGame(); // Reset game nếu máy chặn được
                return;
            }

            // Kích hoạt lại các nút và tương tác của người chơi
            pnlPlayer.Enabled = true; // Kích hoạt panel chứa bài của người chơi
            btnGo.Enabled = true;     // Kích hoạt nút "Đánh"
            btnSkip.Enabled = true;   // Kích hoạt nút "Bỏ qua"

            // Thông báo bạn đã hô Sâm
            MessageBox.Show("Bạn đã hô Sâm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Bạn được đánh đầu tiên sau khi hô Sâm
            playerNext();
        }
        private bool CheckIfComCanBlock(List<int> playerHand)
        {
            // Kiểm tra xem máy có bài mạnh hơn để chặn không
            foreach (int card in listComputer)
            {
                if (getRank(card) > getRank(playerHand.Last()))
                {
                    return true; // Máy có bài mạnh hơn để chặn
                }
            }
            return false; // Máy không có bài mạnh hơn để chặn
        }

        public void HandleSamPlay()
        {
            // Chỉ xử lý khi bạn hô Sâm
            if (samPlayer == 0) // 0 là người chơi (bạn)
            {
                listGO = new List<int>();
                for (int i = 0; i < listPlayer.Count; i++)
                {
                    if (arrStt[i] == 1) // Nếu lá bài được chọn
                    {
                        listGO.Add(listPlayer[i]); // Thêm lá bài vào danh sách đánh
                    }
                }

                // Kiểm tra tính hợp lệ của bộ bài
                if (!isValid(listGO))
                {
                    MessageBox.Show("Bộ bài không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Đánh bài
                K(listGO.Count);

                // Nếu bạn đã đánh hết bài, bạn thắng
                if (listPlayer.Count == 0)
                {
                    MessageBox.Show("Bạn đã thắng!", "Chiến thắng", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    lastWinner = 1;
                    ResetGame();
                }
            }
            else
            {
                // Nếu máy chặn được bài của bạn, bạn thua
                listComGO = new List<int>();
                if (CheckAnyQuad(listComputer) || isValid(listComputer))
                {
                    MessageBox.Show("Bạn bị chặn! Bạn đã thua!", "Thua cuộc", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    lastWinner = 0;
                    ResetGame();
                }
                else
                {
                    // Nếu không ai chặn được, tiếp tục ván đấu
                    MessageBox.Show("Không ai chặn được Sâm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lastWinner = 1;
                    ResetGame();
                }
            }
        }
        //btn đánh
        private void btnGo_Click(object sender, EventArgs e)
        {
            listGO = new List<int>();
            for (int i = 0; i < listPlayer.Count; i++)
            {
                if (arrStt[i] == 1) // Nếu lá bài được chọn
                {
                    listGO.Add(listPlayer[i]); // Thêm lá bài vào danh sách đánh
                }
            }

            if (isValid(listGO)) // Kiểm tra tính hợp lệ của bộ bài
            {
                K(listGO.Count); // Đánh bài
                comNext(); // Chuyển lượt cho máy
            }
            else
            {
                MessageBox.Show("Bộ bài không hợp lệ!"); // Thông báo lỗi
            }
        }
        //ktra hợp lệ
        public bool isValid(List<int> a)
        {
            if (a.Count == 0)
                return false;

            // Kiểm tra tứ quý
            if (a.Count == 4 && isSameRank(a))
                return true;

            // Kiểm tra sám cô
            if (a.Count == 3 && isSameRank(a))
                return true;

            if (listTable.Count == 0) // Nếu bàn chơi trống
            {
                switch (a.Count)
                {
                    case 1: // Bài lẻ
                        return true;
                    case 2: // Đôi
                        return isSameRank(a);
                    case 3: // Sám cô hoặc sảnh 3 lá
                        return isSameRank(a) || isContinuous(a);
                    case 4: // Tứ quý hoặc sảnh 4 lá
                        return isSameRank(a) || isContinuous(a);
                    default: // Sảnh >= 3 lá
                        return isContinuous(a);
                }
            }
            else // Nếu bàn chơi đã có bài
            {
                // Kiểm tra số lượng lá bài phải bằng nhau
                if (a.Count != listTable.Count)
                    return false;

                // So sánh giá trị lá bài cao nhất
                if (isSameRank(a) && isSameRank(listTable)) // Nếu cùng là đôi, sám, tứ quý
                {
                    return getRank(a.Last()) > getRank(listTable.Last());
                }
                else if (isContinuous(a) && isContinuous(listTable)) // Nếu cùng là sảnh
                {
                    return getRank(a.Last()) > getRank(listTable.Last());
                }
                else if (a.Count == 4 && isSameRank(a)) // Tứ quý có thể đánh bất kỳ lúc nào
                {
                    return true;
                }

                return false;
            }
        }
        //ktra 3 - 4 - 5 đôi thông
        public int isConsecutivePairs(List<int> l)
        {
            int k = (int)l.Count() / 2;
            for (int i = 0; i < (l.Count() - 1); i++)
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
        public void CalculateScore()
        {
            int playerScore = 0;
            int comScore = 0;

            // Kiểm tra thối 2
            if (listPlayer.Any(card => getRank(card) == 12)) // Giả sử 2 có rank = 12
                playerScore -= 10;

            if (listComputer.Any(card => getRank(card) == 12))
                comScore -= 10;

            // Tính điểm dựa trên số lá còn lại
            playerScore += listPlayer.Count * 2;
            comScore += listComputer.Count * 2;

            // Hiển thị kết quả
            MessageBox.Show($"Điểm: Người chơi {playerScore}, Máy {comScore}");
        }
        //ktra cùng bộ
        public bool isSameRank(List<int> l)
        {
            if (l.Count == 0)
                return false;

            int rank = getRank(l[0]); // Lấy giá trị của lá bài đầu tiên
            for (int i = 1; i < l.Count; i++)
            {
                if (getRank(l[i]) != rank) // So sánh giá trị của các lá bài
                    return false;
            }
            return true;
        }
        //ktra lốc liên tục
        public bool isContinuous(List<int> l)
        {
            if (l.Count < 3) // Sảnh phải có ít nhất 3 lá
                return false;

            for (int i = 0; i < l.Count - 1; i++)
            {
                if (getRank(l[i + 1]) != getRank(l[i]) + 1) // Kiểm tra dãy liên tiếp
                    return false;
            }
            return true;
        }
        //bộ
        public int getRank(int k)
        {
            return k / 4; // Trả về giá trị của lá bài (0-12)
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
            foreach (Control ctrl in this.pnlTableCards.Controls)
            {
                if (ctrl is PictureBox cT)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (cT.Name == "t" + j)
                        {
                            cT.Visible = true;
                            cT.Image = getImg(listTable[j]);
                        }
                    }

                    for (int j = i; j < 12; j++)
                    {
                        if (cT.Name == "t" + j)
                        {
                            cT.Visible = false;
                        }
                    }
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
            // Trong phương thức K()
            foreach (PictureBox loc in this.pnlPlayer.Controls)
            {
                for (int j = 0; j < 10; j++) // 
                {
                    if (loc.Name == "pl" + j)
                        loc.Location = loc1[j];
                }
            }
            isPlayerWIN();
            // Nếu người chơi chưa thắng, chuyển lượt cho máy
            if (listPlayer.Count > 0)
            {
                comNext();
            }
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
            // Kiểm tra xem người chơi đã thắng chưa
            if (listPlayer.Count == 0)
            {
                tmrComCD.Stop();
                return; // Nếu người chơi đã thắng, không cho máy đánh
            }
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

            // Ưu tiên tứ quý
            if (check4(listComputer, 0, ref listComGO) && isValid(listComGO))
            {
                KK(listComGO);
                return;
            }

            // Ưu tiên sám cô
            if (check3(listComputer, 0, ref listComGO) && isValid(listComGO))
            {
                KK(listComGO);
                return;
            }

            if (listTable.Count == 0) // Nếu bàn chơi trống
            {
                firstOff(); // Máy đánh bài đầu tiên
            }
            else
            {
                // Xử lý các trường hợp dựa trên số lượng lá bài trên bàn
                switch (listTable.Count)
                {
                    case 1: // Đánh bài lẻ
                        for (int i = 0; i < listComputer.Count; i++)
                        {
                            if (getRank(listComputer[i]) > getRank(listTable[0])) // So sánh giá trị
                            {
                                listComGO.Add(listComputer[i]);
                                KK(listComGO);
                                return;
                            }
                        }
                        boqua(); // Bỏ qua nếu không có bài lớn hơn
                        break;

                    case 2: // Đánh đôi
                        for (int i = 0; i < listComputer.Count - 1; i++)
                        {
                            if (getRank(listComputer[i]) == getRank(listComputer[i + 1]) && // Kiểm tra đôi
                                getRank(listComputer[i]) > getRank(listTable[0])) // So sánh giá trị
                            {
                                listComGO.Add(listComputer[i]);
                                listComGO.Add(listComputer[i + 1]);
                                KK(listComGO);
                                return;
                            }
                        }
                        boqua(); // Bỏ qua nếu không có đôi lớn hơn
                        break;

                    case 3: // Đánh sám cô hoặc sảnh 3 lá
                        for (int i = 0; i < listComputer.Count - 2; i++)
                        {
                            if (getRank(listComputer[i]) == getRank(listComputer[i + 1]) && // Kiểm tra sám cô
                                getRank(listComputer[i]) == getRank(listComputer[i + 2]) &&
                                getRank(listComputer[i]) > getRank(listTable[0])) // So sánh giá trị
                            {
                                listComGO.Add(listComputer[i]);
                                listComGO.Add(listComputer[i + 1]);
                                listComGO.Add(listComputer[i + 2]);
                                KK(listComGO);
                                return;
                            }
                            else if (isContinuous(listComputer.GetRange(i, 3))) // Kiểm tra sảnh 3 lá
                            {
                                listComGO.Add(listComputer[i]);
                                listComGO.Add(listComputer[i + 1]);
                                listComGO.Add(listComputer[i + 2]);
                                KK(listComGO);
                                return;
                            }
                        }
                        boqua(); // Bỏ qua nếu không có sám cô hoặc sảnh lớn hơn
                        break;

                    case 4: // Đánh tứ quý hoặc sảnh 4 lá
                        for (int i = 0; i < listComputer.Count - 3; i++)
                        {
                            if (getRank(listComputer[i]) == getRank(listComputer[i + 1]) && // Kiểm tra tứ quý
                                getRank(listComputer[i]) == getRank(listComputer[i + 2]) &&
                                getRank(listComputer[i]) == getRank(listComputer[i + 3]) &&
                                getRank(listComputer[i]) > getRank(listTable[0])) // So sánh giá trị
                            {
                                listComGO.Add(listComputer[i]);
                                listComGO.Add(listComputer[i + 1]);
                                listComGO.Add(listComputer[i + 2]);
                                listComGO.Add(listComputer[i + 3]);
                                KK(listComGO);
                                return;
                            }
                            else if (isContinuous(listComputer.GetRange(i, 4))) // Kiểm tra sảnh 4 lá
                            {
                                listComGO.Add(listComputer[i]);
                                listComGO.Add(listComputer[i + 1]);
                                listComGO.Add(listComputer[i + 2]);
                                listComGO.Add(listComputer[i + 3]);
                                KK(listComGO);
                                return;
                            }
                        }
                        boqua(); // Bỏ qua nếu không có tứ quý hoặc sảnh lớn hơn
                        break;

                    case 5: // Đánh sảnh 5 lá
                        for (int i = 0; i < listComputer.Count - 4; i++)
                        {
                            if (isContinuous(listComputer.GetRange(i, 5))) // Kiểm tra sảnh 5 lá
                            {
                                listComGO.AddRange(listComputer.GetRange(i, 5));
                                KK(listComGO);
                                return;
                            }
                        }
                        boqua(); // Bỏ qua nếu không có sảnh 5 lá lớn hơn
                        break;

                    case 6: // Đánh sảnh 6 lá
                        for (int i = 0; i < listComputer.Count - 5; i++)
                        {
                            if (isContinuous(listComputer.GetRange(i, 6))) // Kiểm tra sảnh 6 lá
                            {
                                listComGO.AddRange(listComputer.GetRange(i, 6));
                                KK(listComGO);
                                return;
                            }
                        }
                        boqua(); // Bỏ qua nếu không có sảnh 6 lá lớn hơn
                        break;

                    case 7: // Đánh sảnh 7 lá
                        for (int i = 0; i < listComputer.Count - 6; i++)
                        {
                            if (isContinuous(listComputer.GetRange(i, 7))) // Kiểm tra sảnh 7 lá
                            {
                                listComGO.AddRange(listComputer.GetRange(i, 7));
                                KK(listComGO);
                                return;
                            }
                        }
                        boqua(); // Bỏ qua nếu không có sảnh 7 lá lớn hơn
                        break;

                    case 8: // Đánh sảnh 8 lá
                        for (int i = 0; i < listComputer.Count - 7; i++)
                        {
                            if (isContinuous(listComputer.GetRange(i, 8))) // Kiểm tra sảnh 8 lá
                            {
                                listComGO.AddRange(listComputer.GetRange(i, 8));
                                KK(listComGO);
                                return;
                            }
                        }
                        boqua(); // Bỏ qua nếu không có sảnh 8 lá lớn hơn
                        break;

                    case 9: // Đánh sảnh 9 lá
                        for (int i = 0; i < listComputer.Count - 8; i++)
                        {
                            if (isContinuous(listComputer.GetRange(i, 9))) // Kiểm tra sảnh 9 lá
                            {
                                listComGO.AddRange(listComputer.GetRange(i, 9));
                                KK(listComGO);
                                return;
                            }
                        }
                        boqua(); // Bỏ qua nếu không có sảnh 9 lá lớn hơn
                        break;

                    case 10: // Đánh sảnh 10 lá
                        for (int i = 0; i < listComputer.Count - 9; i++)
                        {
                            if (isContinuous(listComputer.GetRange(i, 10))) // Kiểm tra sảnh 10 lá
                            {
                                listComGO.AddRange(listComputer.GetRange(i, 10));
                                KK(listComGO);
                                return;
                            }
                        }
                        boqua(); // Bỏ qua nếu không có sảnh 10 lá lớn hơn
                        break;

                    default:
                        boqua(); // Bỏ qua nếu không có bộ bài phù hợp
                        break;
                }
            }
            isComWIN(); // Kiểm tra xem máy có thắng không
        }
        //
        List<int> lstcheck2;
        public bool check3pairs(List<int> l, ref List<int> ll)  //3 đôi thông
        {
            lstcheck2 = new List<int>();

            for (int i = 0; i < l.Count - 5; i++)
            {
                if (getRank(l[i]) == getRank(l[i + 1]) && getRank(l[i + 2]) == getRank(l[i + 3]) && getRank(l[i + 4]) == getRank(l[i + 5])
                && getRank(l[i + 2]) == getRank(l[i]) + 1 && getRank(l[i + 4]) == getRank(l[i + 2]) + 1)
                {
                    for (int j = i; j < i + 6; j++)
                        ll.Add(l[j]);
                    return true;
                }
                if (i < l.Count - 6 && getRank(l[i]) == getRank(l[i + 1]) && getRank(l[i + 2]) == getRank(l[i + 3]) && getRank(l[i + 2]) == getRank(l[i + 4])
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
            for (int i = 0; i < l.Count - 7; i++)
            {
                if (getRank(l[i]) == getRank(l[i + 1]) && getRank(l[i + 2]) == getRank(l[i + 3]) && getRank(l[i + 4]) == getRank(l[i + 5]) && getRank(l[i + 6]) == getRank(l[i + 7])
                    && getRank(l[i + 2]) == getRank(l[i]) + 1 && getRank(l[i + 4]) == getRank(l[i + 2]) + 1 && getRank(l[i + 6]) == getRank(l[i + 4]) + 1)
                {
                    for (int j = i; j < i + 8; j++)
                        ll.Add(l[j]);
                    return true;
                }
                if (i < l.Count - 8 && getRank(l[i]) == getRank(l[i + 1]) && getRank(l[i + 2]) == getRank(l[i + 3]) && getRank(l[i + 2]) == getRank(l[i + 4]) && getRank(l[i + 5]) == getRank(l[i + 6]) && getRank(l[i + 7]) == getRank(l[i + 8])
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
                if (i < l.Count - 8 && getRank(l[i]) == getRank(l[i + 1]) && getRank(l[i + 2]) == getRank(l[i + 3]) && getRank(l[i + 4]) == getRank(l[i + 5]) && getRank(l[i + 4]) == getRank(l[i + 6]) && getRank(l[i + 7]) == getRank(l[i + 8])
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
                if (i < l.Count - 9 && getRank(l[i]) == getRank(l[i + 1]) && getRank(l[i + 2]) == getRank(l[i + 3]) && getRank(l[i + 2]) == getRank(l[i + 4])
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
            if (i < (l.Count - 1) && getRank(l[i]) == getRank(l[i + 1]))
            {
                ll.Add(l[i]);
                ll.Add(l[i + 1]);
                return true;
            }
            return false;
        }
        public bool check3(List<int> l, int i, ref List<int> ll)    //ba
        {
            if (i < (l.Count - 2) && getRank(l[i]) == getRank(l[i + 1]) && getRank(l[i]) == getRank(l[i + 2]))
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
            int end = 0;
            if (i > (lst.Count - loc))
                return false;
            else
            {
                lstcheck = new List<int>();

                int k = i;
                for (int j = 0; j < loc; j++)
                {
                    lstcheck.Add(lst[k]);
                    end = lst[k];
                    if (getRank(end) == 12)
                        return false;
                    if (j != (loc - 1) && k == lst.Count - 1)
                        return false;
                    while (k < lst.Count - 1)
                    {
                        if (getRank(lst[k]) == getRank(lst[k + 1]))
                        {
                            if (j == (loc - 1))
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
                for (int j = 0; j < (loc - 1); j++)
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
            // Reset bàn chơi
            removeAll(listTable);

            // Chuyển bài vào bàn chơi
            for (int j = 0; j < lst.Count; j++)
            {
                listTable.Add(lst[j]);
            }

            // Hiển thị bài lên bàn chơi
            foreach (Control ctrl in this.pnlTableCards.Controls)
            {
                if (ctrl is PictureBox cT)
                {
                    for (int j = 0; j < lst.Count; j++)
                    {
                        if (cT.Name == "t" + j)
                        {
                            cT.Visible = true;
                            cT.Image = getImg(listTable[j]);
                        }
                    }
                    for (int j = lst.Count; j < 12; j++)
                    {
                        if (cT.Name == "t" + j)
                        {
                            cT.Visible = false;
                        }
                    }
                }
            }

            // Xóa bài đã đánh khỏi danh sách của máy
            for (int j = 0; j < listTable.Count; j++)
            {
                listComputer.Remove(listTable[j]);
            }

            // Hiển thị bài của máy (chỉ hiển thị 10 lá)
            foreach (Control com in this.Controls)
            {
                if (com.Name.StartsWith("c"))
                {
                    PictureBox c = (PictureBox)com;
                    c.Image = null;
                    for (int j = 0; j < listComputer.Count; j++)
                    {
                        if (j < 10) // Chỉ hiển thị tối đa 10 lá
                        {
                            if (c.Name == "c" + (j + 1))
                                c.Image = Image.FromFile("Resources\\Z2.png"); // Ảnh úp bài
                        }
                    }
                }
            }
            // Kiểm tra xem máy có thắng không
            isComWIN();

            // Nếu máy chưa thắng, chuyển lượt cho người chơi
            if (listComputer.Count > 0)
            {
                playerNext();
            }
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
        public void isPlayerWIN()
        {
            if (listPlayer.Count == 0)
            {
                tmrCoolDown.Stop();
                tmrComCD.Stop();
                prbCoolDown.Visible = false;

                // Hiển thị bài của máy (chỉ hiển thị 10 lá)
                foreach (Control com in this.Controls)
                {
                    if (com.Name.StartsWith("c"))
                    {
                        PictureBox c = (PictureBox)com;
                        c.Image = null;
                        for (int j = 0; j < listComputer.Count; j++)
                        {
                            if (j < 10) // Chỉ hiển thị tối đa 10 lá
                            {
                                if (c.Name == "c" + (j + 1))
                                    c.Image = getImg(listComputer[j]);
                            }
                        }
                    }
                }

                // Thông báo người chơi thắng
                MessageBox.Show("Bạn đã thắng!", "You win", MessageBoxButtons.OK);

                // Cập nhật lastWinner
                lastWinner = 1; // 1: Người chơi thắng

                // Reset game
                ResetGame();
                return;
            }
        }

        public void isComWIN()
        {
            if (listComputer.Count == 0)
            {
                tmrCoolDown.Stop();
                tmrComCD.Stop();
                prbCoolDown.Visible = false;

                MessageBox.Show("Bạn đã thua!", "You lose", MessageBoxButtons.OK);
                lastWinner = 0;
                ResetGame();
                return;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}


