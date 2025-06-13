namespace TatehamaATS_v1.KokuchiWindow
{
    partial class KokuchiWindow
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
            components = new System.ComponentModel.Container();
            KokuchiLED = new PictureBox();
            timer1 = new System.Windows.Forms.Timer(components);
            Transparency = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)KokuchiLED).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Transparency).BeginInit();
            SuspendLayout();
            // 
            // KokuchiLED
            // 
            KokuchiLED.BackgroundImage = ATSDisplay.LEDResource.Null;
            KokuchiLED.Image = KokuchiResource.KokuchiLED_Waku;
            KokuchiLED.Location = new Point(40, 35);
            KokuchiLED.Name = "KokuchiLED";
            KokuchiLED.Size = new Size(289, 97);
            KokuchiLED.TabIndex = 0;
            KokuchiLED.TabStop = false;
            KokuchiLED.Click += KokuchiLED_Click;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 50;
            timer1.Tick += timer1_Tick;
            // 
            // Transparency
            // 
            Transparency.BackgroundImage = ATSDisplay.LEDResource.Null;
            Transparency.Image = KokuchiResource.Kokuchi_Transparency;
            Transparency.Location = new Point(0, 0);
            Transparency.Name = "Transparency";
            Transparency.Size = new Size(369, 167);
            Transparency.TabIndex = 1;
            Transparency.TabStop = false;
            Transparency.Visible = false;
            // 
            // KokuchiWindow
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackgroundImage = KokuchiResource.Kokuchi_Background;
            BackgroundImageLayout = ImageLayout.None;
            ClientSize = new Size(369, 167);
            Controls.Add(Transparency);
            Controls.Add(KokuchiLED);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "KokuchiWindow";
            Text = "運転告知器 | 館浜ATS - ダイヤ運転会";
            FormClosing += KokuchiWindow_FormClosing;
            ((System.ComponentModel.ISupportInitialize)KokuchiLED).EndInit();
            ((System.ComponentModel.ISupportInitialize)Transparency).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox KokuchiLED;
        private System.Windows.Forms.Timer timer1;
        private PictureBox Transparency;
    }
}