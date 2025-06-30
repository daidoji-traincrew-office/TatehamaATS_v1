﻿namespace TatehamaATS_v1.MainWindow
{
    partial class MainWindow
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
            Image_ATSCut = new PictureBox();
            Image_Reset = new PictureBox();
            Image_Kokuchi = new PictureBox();
            Image_LED = new PictureBox();
            Image_Retsuban = new PictureBox();
            Image_ATSReady = new PictureBox();
            Image_ATSBrakeApply = new PictureBox();
            Image_ATSOpen = new PictureBox();
            Image_Transfer = new PictureBox();
            Image_Network = new PictureBox();
            Image_Kyokan = new PictureBox();
            Image_Relay = new PictureBox();
            Image_TopMostOFF = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)Image_ATSCut).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Image_Reset).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Image_Kokuchi).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Image_LED).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Image_Retsuban).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Image_ATSReady).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Image_ATSBrakeApply).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Image_ATSOpen).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Image_Transfer).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Image_Network).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Image_Kyokan).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Image_Relay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)Image_TopMostOFF).BeginInit();
            SuspendLayout();
            // 
            // Image_ATSCut
            // 
            Image_ATSCut.BackgroundImage = MainResource.ATS_Cut0;
            Image_ATSCut.BackgroundImageLayout = ImageLayout.None;
            Image_ATSCut.Location = new Point(15, 5);
            Image_ATSCut.Name = "Image_ATSCut";
            Image_ATSCut.Size = new Size(80, 95);
            Image_ATSCut.TabIndex = 0;
            Image_ATSCut.TabStop = false;
            Image_ATSCut.MouseDown += Image_ATSCut_MouseDown;
            Image_ATSCut.MouseUp += Image_ATSCut_MouseUp;
            // 
            // Image_Reset
            // 
            Image_Reset.BackgroundImage = MainResource.ATS_Reset0;
            Image_Reset.BackgroundImageLayout = ImageLayout.None;
            Image_Reset.Location = new Point(15, 105);
            Image_Reset.Name = "Image_Reset";
            Image_Reset.Size = new Size(80, 75);
            Image_Reset.TabIndex = 1;
            Image_Reset.TabStop = false;
            // 
            // Image_Kokuchi
            // 
            Image_Kokuchi.BackgroundImage = MainResource.Button_Kokuchi;
            Image_Kokuchi.BackgroundImageLayout = ImageLayout.None;
            Image_Kokuchi.Location = new Point(200, 40);
            Image_Kokuchi.Name = "Image_Kokuchi";
            Image_Kokuchi.Size = new Size(90, 40);
            Image_Kokuchi.TabIndex = 2;
            Image_Kokuchi.TabStop = false;
            Image_Kokuchi.Click += Image_Kokuchi_Click;
            // 
            // Image_LED
            // 
            Image_LED.BackgroundImage = MainResource.Button_LED;
            Image_LED.BackgroundImageLayout = ImageLayout.None;
            Image_LED.Location = new Point(88, 228);
            Image_LED.Name = "Image_LED";
            Image_LED.Size = new Size(44, 64);
            Image_LED.TabIndex = 3;
            Image_LED.TabStop = false;
            Image_LED.Click += Image_LED_Click;
            // 
            // Image_Retsuban
            // 
            Image_Retsuban.BackgroundImage = MainResource.Button_Retsuban;
            Image_Retsuban.BackgroundImageLayout = ImageLayout.None;
            Image_Retsuban.Location = new Point(210, 210);
            Image_Retsuban.Name = "Image_Retsuban";
            Image_Retsuban.Size = new Size(70, 40);
            Image_Retsuban.TabIndex = 4;
            Image_Retsuban.TabStop = false;
            Image_Retsuban.Click += Image_Retsuban_Click;
            // 
            // Image_ATSReady
            // 
            Image_ATSReady.BackgroundImage = MainResource.Lamp_ATS_Ready;
            Image_ATSReady.BackgroundImageLayout = ImageLayout.None;
            Image_ATSReady.Location = new Point(12, 225);
            Image_ATSReady.Name = "Image_ATSReady";
            Image_ATSReady.Size = new Size(36, 10);
            Image_ATSReady.TabIndex = 5;
            Image_ATSReady.TabStop = false;
            Image_ATSReady.Visible = false;
            Image_ATSReady.Click += Image_ATSReady_Click;
            // 
            // Image_ATSBrakeApply
            // 
            Image_ATSBrakeApply.BackgroundImage = MainResource.Lamp_ATS_BrakeApply;
            Image_ATSBrakeApply.BackgroundImageLayout = ImageLayout.None;
            Image_ATSBrakeApply.Location = new Point(12, 237);
            Image_ATSBrakeApply.Name = "Image_ATSBrakeApply";
            Image_ATSBrakeApply.Size = new Size(36, 10);
            Image_ATSBrakeApply.TabIndex = 6;
            Image_ATSBrakeApply.TabStop = false;
            // 
            // Image_ATSOpen
            // 
            Image_ATSOpen.BackgroundImage = MainResource.Lamp_ATS_Open;
            Image_ATSOpen.BackgroundImageLayout = ImageLayout.None;
            Image_ATSOpen.Location = new Point(12, 249);
            Image_ATSOpen.Name = "Image_ATSOpen";
            Image_ATSOpen.Size = new Size(36, 10);
            Image_ATSOpen.TabIndex = 6;
            Image_ATSOpen.TabStop = false;
            Image_ATSOpen.Visible = false;
            // 
            // Image_Transfer
            // 
            Image_Transfer.BackgroundImage = MainResource.Lamp_Transfar_Abnormal;
            Image_Transfer.BackgroundImageLayout = ImageLayout.None;
            Image_Transfer.Location = new Point(12, 273);
            Image_Transfer.Name = "Image_Transfer";
            Image_Transfer.Size = new Size(36, 10);
            Image_Transfer.TabIndex = 6;
            Image_Transfer.TabStop = false;
            // 
            // Image_Network
            // 
            Image_Network.BackgroundImage = MainResource.Lamp_Server_Abnormal;
            Image_Network.BackgroundImageLayout = ImageLayout.None;
            Image_Network.Location = new Point(12, 285);
            Image_Network.Name = "Image_Network";
            Image_Network.Size = new Size(36, 10);
            Image_Network.TabIndex = 6;
            Image_Network.TabStop = false;
            // 
            // Image_Kyokan
            // 
            Image_Kyokan.BackColor = Color.Transparent;
            Image_Kyokan.BackgroundImage = MainResource.Kyokan;
            Image_Kyokan.BackgroundImageLayout = ImageLayout.None;
            Image_Kyokan.Location = new Point(0, 0);
            Image_Kyokan.Name = "Image_Kyokan";
            Image_Kyokan.Size = new Size(300, 300);
            Image_Kyokan.TabIndex = 7;
            Image_Kyokan.TabStop = false;
            Image_Kyokan.Visible = false;
            // 
            // Image_Relay
            // 
            Image_Relay.BackgroundImage = MainResource.Lamp_Relay_Abnormal;
            Image_Relay.BackgroundImageLayout = ImageLayout.None;
            Image_Relay.Location = new Point(12, 261);
            Image_Relay.Name = "Image_Relay";
            Image_Relay.Size = new Size(36, 10);
            Image_Relay.TabIndex = 8;
            Image_Relay.TabStop = false;
            // 
            // Image_TopMostOFF
            // 
            Image_TopMostOFF.BackgroundImage = MainResource.TopMostOFF;
            Image_TopMostOFF.BackgroundImageLayout = ImageLayout.None;
            Image_TopMostOFF.Location = new Point(245, 285);
            Image_TopMostOFF.Name = "Image_TopMostOFF";
            Image_TopMostOFF.Size = new Size(50, 10);
            Image_TopMostOFF.TabIndex = 9;
            Image_TopMostOFF.TabStop = false;
            Image_TopMostOFF.Click += Image_TopMostOFF_Click;
            // 
            // MainWindow
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackgroundImage = MainResource.Main_Background;
            BackgroundImageLayout = ImageLayout.None;
            ClientSize = new Size(300, 300);
            Controls.Add(Image_Kyokan);
            Controls.Add(Image_Reset);
            Controls.Add(Image_TopMostOFF);
            Controls.Add(Image_Relay);
            Controls.Add(Image_Retsuban);
            Controls.Add(Image_LED);
            Controls.Add(Image_Kokuchi);
            Controls.Add(Image_Network);
            Controls.Add(Image_Transfer);
            Controls.Add(Image_ATSOpen);
            Controls.Add(Image_ATSBrakeApply);
            Controls.Add(Image_ATSReady);
            Controls.Add(Image_ATSCut);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximumSize = new Size(316, 339);
            MinimumSize = new Size(316, 339);
            Name = "MainWindow";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "主画面 | 館浜ATS - ダイヤ運転会";
            ((System.ComponentModel.ISupportInitialize)Image_ATSCut).EndInit();
            ((System.ComponentModel.ISupportInitialize)Image_Reset).EndInit();
            ((System.ComponentModel.ISupportInitialize)Image_Kokuchi).EndInit();
            ((System.ComponentModel.ISupportInitialize)Image_LED).EndInit();
            ((System.ComponentModel.ISupportInitialize)Image_Retsuban).EndInit();
            ((System.ComponentModel.ISupportInitialize)Image_ATSReady).EndInit();
            ((System.ComponentModel.ISupportInitialize)Image_ATSBrakeApply).EndInit();
            ((System.ComponentModel.ISupportInitialize)Image_ATSOpen).EndInit();
            ((System.ComponentModel.ISupportInitialize)Image_Transfer).EndInit();
            ((System.ComponentModel.ISupportInitialize)Image_Network).EndInit();
            ((System.ComponentModel.ISupportInitialize)Image_Kyokan).EndInit();
            ((System.ComponentModel.ISupportInitialize)Image_Relay).EndInit();
            ((System.ComponentModel.ISupportInitialize)Image_TopMostOFF).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox Image_ATSCut;
        private PictureBox Image_Reset;
        private PictureBox Image_Kokuchi;
        private PictureBox Image_LED;
        private PictureBox Image_Retsuban;
        private PictureBox Image_ATSReady;
        private PictureBox Image_ATSBrakeApply;
        private PictureBox Image_ATSOpen;
        private PictureBox Image_Transfer;
        private PictureBox Image_Server;
        private PictureBox Image_Kyokan;
        private PictureBox Image_Network;
        private PictureBox Image_Relay;
        private PictureBox Image_TopMostOFF;
    }
}