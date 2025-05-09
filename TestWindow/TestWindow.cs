using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrainCrewAPI;

namespace TatehamaATS_v1.TestWindow
{
    public partial class TestWindow : Form
    {
        private TimeSpan StartTime;
        private bool Run;
        private int YurumegoBrake;
        private int AddBrake;
        private int BeforeBrake;
        private bool isAdd;
        public TestWindow()
        {
            TopMost = true;
            InitializeComponent();
            HidePrint();
        }
        public void UpdataData(TrainCrewStateData tcData)
        {
            if (tcData.myTrainData.Speed == 0 && tcData.myTrainData.nextUIDistance < 200)
            {
                if (Run)
                {
                    Run = false;
                    UpdatePrint(tcData.nowTime.ToTimeSpan(), tcData.myTrainData.nextUIDistance);
                }
            }
            else
            {
                if (!Run)
                {
                    Run = true;
                    StartTime = tcData.nowTime.ToTimeSpan();
                    HidePrint();
                }
                var nowBrake = tcData.myTrainData.Bnotch;
                if (BeforeBrake > nowBrake)
                {
                    isAdd = false;
                }
                else if (BeforeBrake < nowBrake)
                {
                    if (!isAdd)
                    {
                        if (BeforeBrake == 0)
                        {
                            YurumegoBrake++;
                        }
                        else
                        {
                            AddBrake++;
                        }
                    }
                    isAdd = true;
                }
                if (tcData.myTrainData.nextUIDistance > 700 || tcData.myTrainData.speedLimit - tcData.myTrainData.Speed < 5)
                {
                    YurumegoBrake = 0;
                    AddBrake = 0;
                    isAdd = true;
                }
                BeforeBrake = nowBrake;
            }
            UpdatePrint(tcData.nowTime.ToTimeSpan(), tcData.myTrainData.nextUIDistance);
        }
        private void UpdatePrint(TimeSpan nowTime, float meter)
        {
            stabwTime.Text = (nowTime - StartTime).TotalSeconds.ToString("0秒");
            staMeter.Text = meter.ToString("0.0m");
            Yurumego.Text = YurumegoBrake.ToString();
            Add.Text = AddBrake.ToString();
            stabwTime.Visible = true;
            staMeter.Visible = true;
            Yurumego.Visible = true;
            Add.Visible = true;
        }
        private void HidePrint()
        {
            stabwTime.Visible = false;
            staMeter.Visible = false;
            Yurumego.Visible = false;
            Add.Visible = false;
        }
    }
}
