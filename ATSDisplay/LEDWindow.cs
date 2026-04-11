using TatehamaATS_v1.Exceptions;
using TatehamaATS_v1.KokuchiWindow;
using TatehamaATS_v1.Utils;


namespace TatehamaATS_v1.ATSDisplay
{
    public partial class LEDWindow : Form {
        private Bitmap sourceImage;
        internal event Action LEDTestModePush;

        private Size originSize = new Size(277, 360);
        private Size displaySize = new Size(277, 360);
        private Point panelLocation = new Point(40, 35);
        private Size panelSize = new Size(289, 97);
        private Point l1Location = new Point(40, 35);
        private Point l2Location = new Point(40, 35);
        private Point l3Location = new Point(40, 35);
        private Size ledSize = new Size(289, 97);
        private Point buttonLocation = new Point(219, 331);

        public LEDWindow() {
            InitializeComponent();
            originSize = ClientSize;
            displaySize = originSize;
            panelLocation = panel1.Location;
            panelSize = panel1.Size;
            l1Location = L1.Location;
            l2Location = L2.Location;
            l3Location = L3.Location;
            ledSize = L1.Size;
            buttonLocation = LEDTest.Location;
            var tst_time = DateTimeUtils.GetNowJst();
            if (tst_time.Month == 4 && tst_time.Day == 1)
            {
                sourceImage = LEDResource.ATS_LED2;
            }
            else
            {
                sourceImage = LEDResource.ATS_LED;
            }
            Shown += TopMost_Shown;
        }

        private void TopMost_Shown(Object? sender, EventArgs e) {
            TopLevel = false;
            TopLevel = true;
            TopMost = false;
            TopMost = true;
        }
        /// <summary>
        /// 切り出された画像を引き伸ばし、指定の表示器に表示する
        /// </summary>
        /// <param name="pictureBoxIndex">表示するPictureBoxの番号（1～3）</param>
        /// <param name="imageNumber">表示する画像の番号</param>
        internal void DisplayImage(int pictureBoxIndex, int imageNumber) {
            try {
                Bitmap croppedImage;
                if (0x180 <= imageNumber && imageNumber <= 0x1FF || 0x280 <= imageNumber && imageNumber <= 0x2FF || 0x380 <= imageNumber && imageNumber <= 0x3FF || 0x580 <= imageNumber && imageNumber <= 0x5FF || 0x680 <= imageNumber && imageNumber <= 0x6FF || 0x780 <= imageNumber && imageNumber <= 0x7FF || 0x880 <= imageNumber && imageNumber <= 0x8FF) {
                    croppedImage = GetImageByNumber(351);
                    //コード表示無視
                    int codeC = (imageNumber >> 8) & 0xF;
                    Bitmap codeCImage = GetImageByCodeNumber(codeC);
                    int codeB = (imageNumber >> 4) & 0xF;
                    Bitmap codeBImage = GetImageByCodeNumber(codeB);
                    int codeA = imageNumber & 0xF;
                    Bitmap codeAImage = GetImageByCodeNumber(codeA);

                    using (Graphics g = Graphics.FromImage(croppedImage)) {
                        g.DrawImage(codeAImage, 26, 0, codeAImage.Width, codeAImage.Height);
                        g.DrawImage(codeBImage, 19, 0, codeBImage.Width, codeBImage.Height);
                        g.DrawImage(codeCImage, 12, 0, codeCImage.Width, codeCImage.Height);
                    }
                }
                else {
                    croppedImage = GetImageByNumber(imageNumber);
                }
                PictureBox pictureBox = GetPictureBoxByIndex(pictureBoxIndex);

                Bitmap enlargedImage = EnlargePixelArt(croppedImage);

                pictureBox.BackgroundImage = enlargedImage;
            }
            catch (Exception ex) {
                throw new LEDControlException(3, $"エラーが発生しました: {ex.Message} @DisplayImage", ex);
            }
        }

        /// <summary>
        /// ピクセルアートを6倍に拡大する
        /// </summary>
        /// <param name="original">元の画像</param>
        /// <returns>6倍に拡大された画像</returns>
        private Bitmap EnlargePixelArt(Bitmap original) {
            int newWidth = original.Width * 6;
            int newHeight = original.Height * 6;

            Bitmap enlargedImage = new Bitmap(newWidth + 1, newHeight + 1);
            using (Graphics g = Graphics.FromImage(enlargedImage)) {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                for (int y = 0; y < original.Height; y++) {
                    for (int x = 0; x < original.Width; x++) {
                        Color pixelColor = original.GetPixel(x, y);
                        for (int dy = 0; dy < 6; dy++) {
                            for (int dx = 0; dx < 6; dx++) {
                                enlargedImage.SetPixel(x * 6 + dx, y * 6 + dy, pixelColor);
                            }
                        }
                    }
                }
            }

            return enlargedImage;
        }


        /// <summary>
        /// 指定した番号に基づいて画像を切り出す
        /// </summary>
        /// <param name="number">切り出す画像の番号</param>
        /// <returns>切り出された画像</returns>
        private Bitmap GetImageByNumber(int number) {
            int columns = 8;
            int rows = 32;
            int width = 32;
            int height = 16;
            int margin = 1;

            int colIndex = number / 50;
            int rowIndex = number % 50;

            if (colIndex >= columns || rowIndex >= rows) {
                throw new LEDDisplayNumberAbnormal(3, $"指定された番号{number}が無効です", new ArgumentOutOfRangeException());
            }

            int x = margin + colIndex * (width + margin);
            int y = margin + rowIndex * (height + margin);

            Bitmap croppedImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(croppedImage)) {
                g.DrawImage(sourceImage, new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height), GraphicsUnit.Pixel);
            }

            return croppedImage;
        }

        /// <summary>
        /// 指定した番号に基づいて故障数値画像を切り出す
        /// </summary>
        /// <param name="number">切り出す画像の番号</param>
        /// <returns>切り出された画像</returns>
        private Bitmap GetImageByCodeNumber(int number) {
            int columns = 4;
            int rows = 4;
            int width = 6;
            int height = 16;
            int margin = 1;
            int dx = 236;
            int dy = 34;


            int colIndex = number / 4;
            int rowIndex = number % 4;

            if (colIndex >= columns || rowIndex >= rows) {
                throw new LEDDisplayNumberAbnormal(3, $"指定された番号{number}が無効です", new ArgumentOutOfRangeException());
            }

            int x = dx + margin + colIndex * (width + margin);
            int y = dy + margin + rowIndex * (height + margin);

            Bitmap croppedImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(croppedImage)) {
                g.DrawImage(sourceImage, new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height), GraphicsUnit.Pixel);
            }

            return croppedImage;
        }

        /// <summary>
        /// 指定された番号に対応するPictureBoxを取得する
        /// </summary>
        /// <param name="index">PictureBoxの番号（1～3）</param>
        /// <returns>対応するPictureBox</returns>
        private PictureBox GetPictureBoxByIndex(int index) {
            switch (index) {
                case 1:
                    return L1;
                case 2:
                    return L2;
                case 3:
                    return L3;
                default:
                    throw new LEDControlException(3, $"無効な表示器番号: {index}@GetPictureBoxByIndex", new ArgumentOutOfRangeException());
            }
        }


        /// <summary>
        /// ウィンドウのドラッグ開始位置
        /// </summary>
        private Point dragStartPoint;

        /// <summary>
        /// マウスダウンイベントの処理
        /// </summary>
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                dragStartPoint = e.Location;
            }
        }

        /// <summary>
        /// マウスムーブイベントの処理
        /// </summary>
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                this.Location = new Point(this.Left + e.X - dragStartPoint.X, this.Top + e.Y - dragStartPoint.Y);
            }
        }

        private void LEDTest_Click(object sender, EventArgs e) {
            LEDTestModePush?.Invoke();
        }

        private void LEDWindow_Activated(object sender, EventArgs e) {
            var loc = Location;
            FormBorderStyle = FormBorderStyle.Sizable;
            var borderWidth = Size.Width - ClientSize.Width;
            var borderHeight = Size.Height - ClientSize.Height;
            Location = new Point(loc.X - borderWidth / 2, loc.Y - borderHeight + borderWidth / 2);
        }

        private void LEDWindow_Deactivate(object sender, EventArgs e) {
            var borderWidth = Size.Width - ClientSize.Width;
            var borderHeight = Size.Height - ClientSize.Height;
            FormBorderStyle = FormBorderStyle.None;
            Location = new Point(Location.X + borderWidth / 2, Location.Y + borderHeight - borderWidth / 2);
        }

        private void LEDWindow_ResizeEnd(object sender, EventArgs e) {
            var newWidth = ClientSize.Width;
            var newHeight = ClientSize.Height;
            if(newHeight < 155) {
                newHeight = originSize.Height * newWidth / originSize.Width;
            }
            else if ((float)originSize.Width / originSize.Height < (float)newWidth / newHeight) {
                newWidth = originSize.Width * newHeight / originSize.Height;
            }
            else if ((float)originSize.Width / originSize.Height > (float)newWidth / newHeight) {
                newHeight = originSize.Height * newWidth / originSize.Width;
            }
            displaySize = new Size(newWidth, newHeight);
            Size = new Size(newWidth + Size.Width - ClientSize.Width, newHeight + Size.Height - ClientSize.Height);


            panel1.Location = new Point(panelLocation.X * newHeight / originSize.Height, panelLocation.Y * newHeight / originSize.Height);
            panel1.Size = new Size(panelSize.Width * newHeight / originSize.Height, panelSize.Height * newHeight / originSize.Height);

            L1.Location = new Point(l1Location.X * newHeight / originSize.Height, l1Location.Y * newHeight / originSize.Height);
            L1.Size = new Size(ledSize.Width * newHeight / originSize.Height, ledSize.Height * newHeight / originSize.Height);

            L2.Location = new Point(l2Location.X * newHeight / originSize.Height, l2Location.Y * newHeight / originSize.Height);
            L2.Size = new Size(ledSize.Width * newHeight / originSize.Height, ledSize.Height * newHeight / originSize.Height);

            L3.Location = new Point(l3Location.X * newHeight / originSize.Height, l3Location.Y * newHeight / originSize.Height);
            L3.Size = new Size(ledSize.Width * newHeight / originSize.Height, ledSize.Height * newHeight / originSize.Height);

            LEDTest.Location = new Point(buttonLocation.X * newHeight / originSize.Height, buttonLocation.Y * newHeight / originSize.Height);
        }
    }
}
