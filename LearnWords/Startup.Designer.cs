namespace LearnWords
{
    partial class Startup
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
            SuspendLayout();
            // 
            // Start
            // 
            Start.Anchor = AnchorStyles.Bottom;
            Start.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            Start.Location = new Point(52, 210);
            Start.Name = "Start";
            Start.Size = new Size(241, 39);
            Start.TabIndex = 0;
            Start.Text = "Start";
            Start.UseVisualStyleBackColor = true;
            Start.Click += Start_Click;
            // 
            // Reset
            // 
            Reset.Anchor = AnchorStyles.Top;
            Reset.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            Reset.Location = new Point(52, 119);
            Reset.Name = "Reset";
            Reset.Size = new Size(241, 39);
            Reset.TabIndex = 1;
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
            LexiconList.Size = new Size(234, 33);
            LexiconList.TabIndex = 2;
            LexiconList.SelectedIndexChanged += LexiconList_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(12, 20);
            label1.Name = "label1";
            label1.Size = new Size(80, 25);
            label1.TabIndex = 3;
            label1.Text = "Lexicon:";
            // 
            // LexiconStats
            // 
            LexiconStats.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            LexiconStats.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            LexiconStats.Location = new Point(12, 60);
            LexiconStats.Name = "LexiconStats";
            LexiconStats.Size = new Size(320, 38);
            LexiconStats.TabIndex = 4;
            // 
            // Startup
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(344, 261);
            Controls.Add(LexiconStats);
            Controls.Add(label1);
            Controls.Add(LexiconList);
            Controls.Add(Reset);
            Controls.Add(Start);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Startup";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Emma Word Learning";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button Start;
        private Button Reset;
        private ComboBox LexiconList;
        private Label label1;
        private Label LexiconStats;
    }
}