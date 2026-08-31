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
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestWindow));
            label1 = new Label();
            label3 = new Label();
            stabwTime = new Label();
            staMeter = new Label();
            label2 = new Label();
            label4 = new Label();
            Add = new Label();
            Yurumego = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 27);
            label1.Name = "label1";
            label1.Size = new Size(77, 12);
            label1.TabIndex = 0;
            label1.Text = "駅間走行時間";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 66);
            label3.Name = "label3";
            label3.Size = new Size(77, 12);
            label3.TabIndex = 0;
            label3.Text = "停止位置誤差";
            // 
            // stabwTime
            // 
            stabwTime.AutoSize = true;
            stabwTime.Font = new Font("ＭＳ ゴシック", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            stabwTime.Location = new Point(101, 12);
            stabwTime.Name = "stabwTime";
            stabwTime.RightToLeft = RightToLeft.No;
            stabwTime.Size = new Size(96, 27);
            stabwTime.TabIndex = 0;
            stabwTime.Text = "9999秒";
            stabwTime.TextAlign = ContentAlignment.MiddleRight;
            // 
            // staMeter
            // 
            staMeter.Font = new Font("ＭＳ ゴシック", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            staMeter.Location = new Point(95, 51);
            staMeter.Name = "staMeter";
            staMeter.RightToLeft = RightToLeft.No;
            staMeter.Size = new Size(96, 27);
            staMeter.TabIndex = 0;
            staMeter.Text = "-99m";
            staMeter.TextAlign = ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(221, 27);
            label2.Name = "label2";
            label2.RightToLeft = RightToLeft.Yes;
            label2.Size = new Size(65, 12);
            label2.TabIndex = 0;
            label2.Text = "弛め後制動";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(221, 66);
            label4.Name = "label4";
            label4.Size = new Size(53, 12);
            label4.TabIndex = 0;
            label4.Text = "追加制動";
            // 
            // Add
            // 
            Add.Font = new Font("ＭＳ ゴシック", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            Add.Location = new Point(298, 51);
            Add.Name = "Add";
            Add.RightToLeft = RightToLeft.No;
            Add.Size = new Size(82, 27);
            Add.TabIndex = 0;
            Add.Text = "1回";
            Add.TextAlign = ContentAlignment.MiddleRight;
            // 
            // Yurumego
            // 
            Yurumego.AutoSize = true;
            Yurumego.Font = new Font("ＭＳ ゴシック", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            Yurumego.Location = new Point(298, 12);
            Yurumego.Name = "Yurumego";
            Yurumego.Size = new Size(82, 27);
            Yurumego.TabIndex = 0;
            Yurumego.Text = "100回";
            Yurumego.TextAlign = ContentAlignment.MiddleRight;
            // 
            // TestWindow
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(398, 107);
            Controls.Add(label4);
            Controls.Add(label2);
            Controls.Add(label3);
            Controls.Add(Yurumego);
            Controls.Add(Add);
            Controls.Add(staMeter);
            Controls.Add(stabwTime);
            Controls.Add(label1);
            Font = new Font("ＭＳ ゴシック", 9F, FontStyle.Regular, GraphicsUnit.Point, 128);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            Name = "TestWindow";
            Text = "試験用 | 館浜ATS - ダイヤ運転会";
            FormClosing += TestWindow_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label3;
        private Label stabwTime;
        private Label staMeter;
        private Label label2;
        private Label label4;
        private Label Add;
        private Label Yurumego;
    }
}