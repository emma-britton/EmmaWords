namespace Emma.Anagramming
{
    partial class AnagramConfig
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            StartButton = new Button();
            LexiconList = new ComboBox();
            MinWordLength = new NumericUpDown();
            label1 = new Label();
            TwitchUsername = new TextBox();
            label2 = new Label();
            label3 = new Label();
            MaxWordLength = new NumericUpDown();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            TwitchChannel = new TextBox();
            TwitchOAuth = new TextBox();
            label7 = new Label();
            TwitchClientID = new TextBox();
            label8 = new Label();
            ((System.ComponentModel.ISupportInitialize)MinWordLength).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MaxWordLength).BeginInit();
            SuspendLayout();
            // 
            // StartButton
            // 
            StartButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            StartButton.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            StartButton.Location = new Point(542, 378);
            StartButton.Name = "StartButton";
            StartButton.Size = new Size(143, 40);
            StartButton.TabIndex = 0;
            StartButton.Text = "Start";
            StartButton.UseVisualStyleBackColor = true;
            StartButton.Click += StartButton_Click;
            // 
            // LexiconList
            // 
            LexiconList.DropDownStyle = ComboBoxStyle.DropDownList;
            LexiconList.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            LexiconList.FormattingEnabled = true;
            LexiconList.Location = new Point(177, 19);
            LexiconList.Name = "LexiconList";
            LexiconList.Size = new Size(184, 33);
            LexiconList.TabIndex = 1;
            // 
            // MinWordLength
            // 
            MinWordLength.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            MinWordLength.Location = new Point(177, 58);
            MinWordLength.Name = "MinWordLength";
            MinWordLength.Size = new Size(81, 33);
            MinWordLength.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(91, 22);
            label1.Name = "label1";
            label1.Size = new Size(80, 25);
            label1.TabIndex = 3;
            label1.Text = "Lexicon:";
            // 
            // TwitchUsername
            // 
            TwitchUsername.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            TwitchUsername.Location = new Point(177, 200);
            TwitchUsername.Name = "TwitchUsername";
            TwitchUsername.Size = new Size(245, 33);
            TwitchUsername.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label2.Location = new Point(15, 60);
            label2.Name = "label2";
            label2.Size = new Size(156, 25);
            label2.TabIndex = 5;
            label2.Text = "Min word length:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label3.Location = new Point(12, 99);
            label3.Name = "label3";
            label3.Size = new Size(159, 25);
            label3.TabIndex = 6;
            label3.Text = "Max word length:";
            // 
            // MaxWordLength
            // 
            MaxWordLength.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            MaxWordLength.Location = new Point(177, 97);
            MaxWordLength.Name = "MaxWordLength";
            MaxWordLength.Size = new Size(81, 33);
            MaxWordLength.TabIndex = 7;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            label4.Location = new Point(12, 163);
            label4.Name = "label4";
            label4.Size = new Size(174, 25);
            label4.TabIndex = 8;
            label4.Text = "Twitch integration";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label5.Location = new Point(70, 203);
            label5.Name = "label5";
            label5.Size = new Size(101, 25);
            label5.TabIndex = 9;
            label5.Text = "Username:";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label6.Location = new Point(85, 242);
            label6.Name = "label6";
            label6.Size = new Size(86, 25);
            label6.TabIndex = 10;
            label6.Text = "Channel:";
            // 
            // TwitchChannel
            // 
            TwitchChannel.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            TwitchChannel.Location = new Point(177, 239);
            TwitchChannel.Name = "TwitchChannel";
            TwitchChannel.Size = new Size(245, 33);
            TwitchChannel.TabIndex = 11;
            // 
            // TwitchOAuth
            // 
            TwitchOAuth.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            TwitchOAuth.Location = new Point(177, 317);
            TwitchOAuth.Name = "TwitchOAuth";
            TwitchOAuth.Size = new Size(493, 33);
            TwitchOAuth.TabIndex = 12;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label7.Location = new Point(49, 320);
            label7.Name = "label7";
            label7.Size = new Size(122, 25);
            label7.TabIndex = 13;
            label7.Text = "OAuth token:";
            // 
            // TwitchClientID
            // 
            TwitchClientID.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            TwitchClientID.Location = new Point(177, 278);
            TwitchClientID.Name = "TwitchClientID";
            TwitchClientID.Size = new Size(493, 33);
            TwitchClientID.TabIndex = 14;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label8.Location = new Point(83, 281);
            label8.Name = "label8";
            label8.Size = new Size(88, 25);
            label8.TabIndex = 15;
            label8.Text = "Client ID:";
            // 
            // AnagramConfig
            // 
            AcceptButton = StartButton;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(697, 430);
            Controls.Add(label8);
            Controls.Add(TwitchClientID);
            Controls.Add(label7);
            Controls.Add(TwitchOAuth);
            Controls.Add(TwitchChannel);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(MaxWordLength);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(TwitchUsername);
            Controls.Add(label1);
            Controls.Add(MinWordLength);
            Controls.Add(LexiconList);
            Controls.Add(StartButton);
            Name = "AnagramConfig";
            Text = "Anagramming Game";
            Load += AnagramConfig_Load;
            ((System.ComponentModel.ISupportInitialize)MinWordLength).EndInit();
            ((System.ComponentModel.ISupportInitialize)MaxWordLength).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button StartButton;
        private ComboBox LexiconList;
        private NumericUpDown MinWordLength;
        private Label label1;
        private TextBox TwitchUsername;
        private Label label2;
        private Label label3;
        private NumericUpDown MaxWordLength;
        private Label label4;
        private Label label5;
        private Label label6;
        private TextBox TwitchChannel;
        private TextBox TwitchOAuth;
        private Label label7;
        private TextBox TwitchClientID;
        private Label label8;
    }
}