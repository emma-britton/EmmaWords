namespace Emma.WordLearner
{
    partial class WordLearnConfig
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
            Start = new Button();
            Reset = new Button();
            LexiconList = new ComboBox();
            label1 = new Label();
            LexiconStats = new Label();
            ReviewPeriod = new TrackBar();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            NetCorrect = new NumericUpDown();
            label6 = new Label();
            ((System.ComponentModel.ISupportInitialize)ReviewPeriod).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NetCorrect).BeginInit();
            SuspendLayout();
            // 
            // Start
            // 
            Start.Anchor = AnchorStyles.Bottom;
            Start.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            Start.Location = new Point(40, 314);
            Start.Name = "Start";
            Start.Size = new Size(252, 39);
            Start.TabIndex = 7;
            Start.Text = "Start";
            Start.UseVisualStyleBackColor = true;
            Start.Click += Start_Click;
            // 
            // Reset
            // 
            Reset.Anchor = AnchorStyles.Top;
            Reset.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            Reset.Location = new Point(89, 268);
            Reset.Name = "Reset";
            Reset.Size = new Size(148, 31);
            Reset.TabIndex = 6;
            Reset.Text = "Reset learned words";
            Reset.UseVisualStyleBackColor = true;
            Reset.Click += Reset_Click;
            // 
            // LexiconList
            // 
            LexiconList.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            LexiconList.DropDownStyle = ComboBoxStyle.DropDownList;
            LexiconList.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            LexiconList.FormattingEnabled = true;
            LexiconList.Location = new Point(98, 17);
            LexiconList.Name = "LexiconList";
            LexiconList.Size = new Size(218, 33);
            LexiconList.TabIndex = 1;
            LexiconList.SelectedIndexChanged += LexiconList_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(12, 20);
            label1.Name = "label1";
            label1.Size = new Size(80, 25);
            label1.TabIndex = 0;
            label1.Text = "Lexicon:";
            // 
            // LexiconStats
            // 
            LexiconStats.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            LexiconStats.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            LexiconStats.Location = new Point(12, 57);
            LexiconStats.Name = "LexiconStats";
            LexiconStats.Size = new Size(304, 52);
            LexiconStats.TabIndex = 2;
            // 
            // ReviewPeriod
            // 
            ReviewPeriod.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ReviewPeriod.LargeChange = 10;
            ReviewPeriod.Location = new Point(12, 137);
            ReviewPeriod.Maximum = 175;
            ReviewPeriod.Minimum = 25;
            ReviewPeriod.Name = "ReviewPeriod";
            ReviewPeriod.Size = new Size(304, 45);
            ReviewPeriod.SmallChange = 10;
            ReviewPeriod.TabIndex = 3;
            ReviewPeriod.TickFrequency = 10;
            ReviewPeriod.Value = 100;
            // 
            // label2
            // 
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            label2.Location = new Point(12, 164);
            label2.Name = "label2";
            label2.Size = new Size(120, 41);
            label2.TabIndex = 4;
            label2.Text = "Fewer new words\r\nReview more often";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            label3.Location = new Point(202, 164);
            label3.Name = "label3";
            label3.Size = new Size(114, 41);
            label3.TabIndex = 5;
            label3.Text = "More new words\r\nReview less often";
            label3.TextAlign = ContentAlignment.TopRight;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label4.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            label4.Location = new Point(12, 117);
            label4.Name = "label4";
            label4.Size = new Size(304, 24);
            label4.TabIndex = 8;
            label4.Text = "Change how often words are reviewed:";
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label5.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            label5.Location = new Point(12, 222);
            label5.Name = "label5";
            label5.Size = new Size(126, 24);
            label5.TabIndex = 9;
            label5.Text = "Stop reviewing after";
            // 
            // NetCorrect
            // 
            NetCorrect.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            NetCorrect.Location = new Point(138, 220);
            NetCorrect.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
            NetCorrect.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            NetCorrect.Name = "NetCorrect";
            NetCorrect.Size = new Size(44, 25);
            NetCorrect.TabIndex = 10;
            NetCorrect.Value = new decimal(new int[] { 3, 0, 0, 0 });
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label6.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            label6.Location = new Point(188, 222);
            label6.Name = "label6";
            label6.Size = new Size(128, 24);
            label6.TabIndex = 11;
            label6.Text = "net correct answers";
            // 
            // WordLearnConfig
            // 
            AcceptButton = Start;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(328, 365);
            Controls.Add(label6);
            Controls.Add(NetCorrect);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(ReviewPeriod);
            Controls.Add(LexiconStats);
            Controls.Add(label1);
            Controls.Add(LexiconList);
            Controls.Add(Reset);
            Controls.Add(Start);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "WordLearnConfig";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Emma Word Learning";
            ((System.ComponentModel.ISupportInitialize)ReviewPeriod).EndInit();
            ((System.ComponentModel.ISupportInitialize)NetCorrect).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button Start;
        private Button Reset;
        private ComboBox LexiconList;
        private Label label1;
        private Label LexiconStats;
        private TrackBar ReviewPeriod;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private NumericUpDown NetCorrect;
        private Label label6;
    }
}