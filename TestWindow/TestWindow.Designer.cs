namespace TatehamaATS_v1.TestWindow
{
    partial class TestWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            stabwTime = new Label();
            staMeter = new Label();
            label5 = new Label();
            label6 = new Label();
            Add = new Label();
            Yurumego = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 30);
            label1.Name = "label1";
            label1.Size = new Size(77, 12);
            label1.TabIndex = 0;
            label1.Text = "駅間走行時間";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 86);
            label2.Name = "label2";
            label2.Size = new Size(77, 12);
            label2.TabIndex = 0;
            label2.Text = "停止位置誤差";
            // 
            // stabwTime
            // 
            stabwTime.Font = new Font("ＭＳ ゴシック", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            stabwTime.Location = new Point(95, 9);
            stabwTime.Name = "stabwTime";
            stabwTime.RightToLeft = RightToLeft.No;
            stabwTime.Size = new Size(102, 35);
            stabwTime.TabIndex = 0;
            stabwTime.Text = "999秒";
            stabwTime.TextAlign = ContentAlignment.MiddleRight;
            // 
            // staMeter
            // 
            staMeter.Font = new Font("ＭＳ ゴシック", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            staMeter.Location = new Point(95, 58);
            staMeter.Name = "staMeter";
            staMeter.RightToLeft = RightToLeft.No;
            staMeter.Size = new Size(102, 42);
            staMeter.TabIndex = 0;
            staMeter.Text = "-99m";
            staMeter.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(222, 30);
            label5.Name = "label5";
            label5.RightToLeft = RightToLeft.Yes;
            label5.Size = new Size(65, 12);
            label5.TabIndex = 0;
            label5.Text = "弛め後制動";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(222, 86);
            label6.Name = "label6";
            label6.Size = new Size(53, 12);
            label6.TabIndex = 0;
            label6.Text = "追加制動";
            // 
            // Add
            // 
            Add.Font = new Font("ＭＳ ゴシック", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            Add.Location = new Point(293, 58);
            Add.Name = "Add";
            Add.RightToLeft = RightToLeft.No;
            Add.Size = new Size(90, 42);
            Add.TabIndex = 0;
            Add.Text = "1回";
            Add.TextAlign = ContentAlignment.MiddleRight;
            // 
            // Yurumego
            // 
            Yurumego.Font = new Font("ＭＳ ゴシック", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            Yurumego.Location = new Point(293, 9);
            Yurumego.Name = "Yurumego";
            Yurumego.Size = new Size(90, 35);
            Yurumego.TabIndex = 0;
            Yurumego.Text = "10回";
            Yurumego.TextAlign = ContentAlignment.MiddleRight;
            // 
            // TestWindow
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(398, 107);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label2);
            Controls.Add(Yurumego);
            Controls.Add(Add);
            Controls.Add(staMeter);
            Controls.Add(stabwTime);
            Controls.Add(label1);
            Font = new Font("ＭＳ ゴシック", 9F, FontStyle.Regular, GraphicsUnit.Point, 128);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(3, 2, 3, 2);
            Name = "TestWindow";
            Text = "試験用 | 館浜ATS - ダイヤ運転会";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label stabwTime;
        private Label staMeter;
        private Label label5;
        private Label label6;
        private Label Add;
        private Label Yurumego;
    }
}