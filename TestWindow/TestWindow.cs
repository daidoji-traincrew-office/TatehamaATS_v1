using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrainCrewAPI;

namespace TatehamaATS_v1.TestWindow
{
    public partial class TestWindow : Form {
        private TimeSpan StartTime;
        private bool Run;
        private int YurumegoBrake;
        private int AddBrake;
        private int BeforeBrake;
        private bool isAdd;
        private bool isOpen;
        public TestWindow() {
            TopMost = true;
            InitializeComponent();
            RelocationPrint();
            HidePrint();
        }
        public void UpdataData(TrainCrewStateData tcData) {
            if (tcData.myTrainData.Speed == 0 && tcData.myTrainData.nextUIDistance < 200) {
                if (Run) {
                    Run = false;
                    UpdatePrint(tcData.nowTime.ToTimeSpan(), tcData.myTrainData.nextUIDistance);
                }
            }
            else {
                if (!Run && tcData.myTrainData.Speed != 0) {
                    Run = true;
                    StartTime = tcData.nowTime.ToTimeSpan();
                    HidePrint();
                }

                var nowBrake = tcData.myTrainData.Bnotch;
                if (BeforeBrake > nowBrake) {
                    isAdd = false;
                }
                else if (BeforeBrake < nowBrake) {
                    if (!isAdd) {
                        if (BeforeBrake == 0) {
                            YurumegoBrake++;
                        }
                        else {
                            AddBrake++;
                        }
                    }

                    isAdd = true;
                }

                if (tcData.myTrainData.nextUIDistance > 700 || tcData.myTrainData.speedLimit - tcData.myTrainData.Speed < 5) {
                    YurumegoBrake = 0;
                    AddBrake = 0;
                    isAdd = true;
                }

                BeforeBrake = nowBrake;
            }
        }
        private void UpdatePrint(TimeSpan nowTime, float meter) {
            stabwTime.Text = (nowTime - StartTime).TotalSeconds.ToString("0秒");
            staMeter.Text = meter.ToString("0.0m");
            Yurumego.Text = YurumegoBrake.ToString();
            Add.Text = AddBrake.ToString();
            stabwTime.Visible = true;
            staMeter.Visible = true;
            Yurumego.Visible = true;
            Add.Visible = true;
        }

        private void RelocationPrint() {
            var padding = 12;
            label1.Location = new Point(padding, padding + stabwTime.Size.Height - label1.Size.Height);
            stabwTime.Location = new Point(label1.Location.X + label1.Size.Width + padding, padding);
            var size = stabwTime.Size;
            stabwTime.AutoSize = false;
            stabwTime.Size = size;

            var bottomY = stabwTime.Location.Y + stabwTime.Size.Height + padding;

            label3.Location = new Point(padding, bottomY + stabwTime.Size.Height - label3.Size.Height);
            staMeter.Location = new Point(label3.Location.X + label3.Size.Width + padding, bottomY);
            staMeter.Size = size;

            label2.Location = new Point(stabwTime.Location.X + stabwTime.Size.Width + padding * 2, label1.Location.Y);
            Yurumego.Location = new Point(label2.Location.X + label2.Size.Width + padding, padding);
            size = Yurumego.Size;
            Yurumego.AutoSize = false;
            Yurumego.Size = size;

            label4.Location = new Point(label2.Location.X, label3.Location.Y);
            Add.Location = new Point(Yurumego.Location.X, bottomY);
            Add.Size = size;

            Size = Size + new Size(Add.Location.X + Add.Size.Width + padding, Add.Location.Y + Add.Size.Height + padding) - ClientSize;
        }

        private void HidePrint() {
            stabwTime.Visible = false;
            staMeter.Visible = false;
            Yurumego.Visible = false;
            Add.Visible = false;
        }

        private void TestWindow_FormClosing(object sender, FormClosingEventArgs e) {
            //閉じずに消す
            Hide();
            e.Cancel = true;
        }
    }
}
