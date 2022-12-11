namespace EmmaWords
{
    partial class RulesetEditor
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
            ContinueButton = new Button();
            RackSize = new NumericUpDown();
            label1 = new Label();
            label2 = new Label();
            BoardSize = new NumericUpDown();
            TileDistributionEditor = new TableLayoutPanel();
            label4 = new Label();
            BoardEditor = new TableLayoutPanel();
            label3 = new Label();
            AllowFlip = new CheckBox();
            TileTotal = new Label();
            Player1Name = new TextBox();
            label5 = new Label();
            label6 = new Label();
            Player2Name = new TextBox();
            ClearBoardDesign = new Button();
            ClearTilesButton = new Button();
            ValidateWords = new CheckBox();
            label7 = new Label();
            ResetBoardButton = new Button();
            ResetTilesButton = new Button();
            BingoScore = new NumericUpDown();
            label8 = new Label();
            label9 = new Label();
            Description = new TextBox();
            LoadButton = new Button();
            SaveButton = new Button();
            ((System.ComponentModel.ISupportInitialize)RackSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)BoardSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)BingoScore).BeginInit();
            SuspendLayout();
            // 
            // ContinueButton
            // 
            ContinueButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            ContinueButton.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            ContinueButton.Location = new Point(1090, 740);
            ContinueButton.Name = "ContinueButton";
            ContinueButton.Size = new Size(127, 43);
            ContinueButton.TabIndex = 0;
            ContinueButton.Text = "Continue";
            ContinueButton.UseVisualStyleBackColor = true;
            ContinueButton.Click += ContinueButton_Click;
            // 
            // RackSize
            // 
            RackSize.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            RackSize.Location = new Point(128, 51);
            RackSize.Maximum = new decimal(new int[] { 15, 0, 0, 0 });
            RackSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            RackSize.Name = "RackSize";
            RackSize.Size = new Size(73, 33);
            RackSize.TabIndex = 1;
            RackSize.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label1.Location = new Point(12, 53);
            label1.Name = "label1";
            label1.Size = new Size(87, 25);
            label1.TabIndex = 2;
            label1.Text = "Rack size";
            label1.TextAlign = ContentAlignment.TopRight;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label2.Location = new Point(12, 92);
            label2.Name = "label2";
            label2.Size = new Size(97, 25);
            label2.TabIndex = 4;
            label2.Text = "Board size";
            label2.TextAlign = ContentAlignment.TopRight;
            // 
            // BoardSize
            // 
            BoardSize.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            BoardSize.Location = new Point(128, 90);
            BoardSize.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
            BoardSize.Minimum = new decimal(new int[] { 3, 0, 0, 0 });
            BoardSize.Name = "BoardSize";
            BoardSize.Size = new Size(73, 33);
            BoardSize.TabIndex = 3;
            BoardSize.Value = new decimal(new int[] { 3, 0, 0, 0 });
            BoardSize.ValueChanged += BoardSize_ValueChanged;
            // 
            // TileDistributionEditor
            // 
            TileDistributionEditor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            TileDistributionEditor.AutoScroll = true;
            TileDistributionEditor.ColumnCount = 1;
            TileDistributionEditor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            TileDistributionEditor.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            TileDistributionEditor.Location = new Point(717, 168);
            TileDistributionEditor.Name = "TileDistributionEditor";
            TileDistributionEditor.RowCount = 1;
            TileDistributionEditor.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            TileDistributionEditor.Size = new Size(500, 566);
            TileDistributionEditor.TabIndex = 7;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label4.Location = new Point(717, 136);
            label4.Name = "label4";
            label4.Size = new Size(454, 25);
            label4.TabIndex = 8;
            label4.Text = "Tile                Display                Number                Points";
            label4.TextAlign = ContentAlignment.TopRight;
            // 
            // BoardEditor
            // 
            BoardEditor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            BoardEditor.AutoScroll = true;
            BoardEditor.ColumnCount = 1;
            BoardEditor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            BoardEditor.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            BoardEditor.Location = new Point(12, 168);
            BoardEditor.Margin = new Padding(0);
            BoardEditor.Name = "BoardEditor";
            BoardEditor.RowCount = 1;
            BoardEditor.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            BoardEditor.Size = new Size(702, 566);
            BoardEditor.TabIndex = 9;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label3.Location = new Point(777, 51);
            label3.Name = "label3";
            label3.Size = new Size(86, 25);
            label3.TabIndex = 10;
            label3.Text = "Allow flip";
            label3.TextAlign = ContentAlignment.TopRight;
            // 
            // AllowFlip
            // 
            AllowFlip.Location = new Point(751, 52);
            AllowFlip.Name = "AllowFlip";
            AllowFlip.Size = new Size(20, 24);
            AllowFlip.TabIndex = 11;
            AllowFlip.UseVisualStyleBackColor = true;
            AllowFlip.CheckedChanged += AllowFlip_CheckedChanged;
            // 
            // TileTotal
            // 
            TileTotal.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            TileTotal.AutoSize = true;
            TileTotal.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            TileTotal.Location = new Point(717, 749);
            TileTotal.Name = "TileTotal";
            TileTotal.Size = new Size(44, 25);
            TileTotal.TabIndex = 12;
            TileTotal.Text = "tiles";
            TileTotal.TextAlign = ContentAlignment.TopRight;
            // 
            // Player1Name
            // 
            Player1Name.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            Player1Name.Location = new Point(396, 56);
            Player1Name.Name = "Player1Name";
            Player1Name.Size = new Size(177, 33);
            Player1Name.TabIndex = 13;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label5.Location = new Point(266, 59);
            label5.Name = "label5";
            label5.Size = new Size(124, 25);
            label5.TabIndex = 14;
            label5.Text = "Player 1 name";
            label5.TextAlign = ContentAlignment.TopRight;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label6.Location = new Point(266, 98);
            label6.Name = "label6";
            label6.Size = new Size(127, 25);
            label6.TabIndex = 15;
            label6.Text = "Player 2 name";
            label6.TextAlign = ContentAlignment.TopRight;
            // 
            // Player2Name
            // 
            Player2Name.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            Player2Name.Location = new Point(396, 95);
            Player2Name.Name = "Player2Name";
            Player2Name.Size = new Size(177, 33);
            Player2Name.TabIndex = 16;
            // 
            // ClearBoardDesign
            // 
            ClearBoardDesign.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ClearBoardDesign.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            ClearBoardDesign.Location = new Point(12, 740);
            ClearBoardDesign.Name = "ClearBoardDesign";
            ClearBoardDesign.Size = new Size(127, 43);
            ClearBoardDesign.TabIndex = 17;
            ClearBoardDesign.Text = "Clear board";
            ClearBoardDesign.UseVisualStyleBackColor = true;
            ClearBoardDesign.Click += ClearBoardDesign_Click;
            // 
            // ClearTilesButton
            // 
            ClearTilesButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            ClearTilesButton.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            ClearTilesButton.Location = new Point(846, 740);
            ClearTilesButton.Name = "ClearTilesButton";
            ClearTilesButton.Size = new Size(116, 43);
            ClearTilesButton.TabIndex = 18;
            ClearTilesButton.Text = "Clear tiles";
            ClearTilesButton.UseVisualStyleBackColor = true;
            ClearTilesButton.Click += ClearTilesButton_Click;
            // 
            // ValidateWords
            // 
            ValidateWords.Location = new Point(751, 21);
            ValidateWords.Name = "ValidateWords";
            ValidateWords.Size = new Size(20, 24);
            ValidateWords.TabIndex = 20;
            ValidateWords.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label7.Location = new Point(777, 17);
            label7.Name = "label7";
            label7.Size = new Size(132, 25);
            label7.TabIndex = 19;
            label7.Text = "Validate words";
            label7.TextAlign = ContentAlignment.TopRight;
            // 
            // ResetBoardButton
            // 
            ResetBoardButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            ResetBoardButton.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            ResetBoardButton.Location = new Point(145, 740);
            ResetBoardButton.Name = "ResetBoardButton";
            ResetBoardButton.Size = new Size(127, 43);
            ResetBoardButton.TabIndex = 21;
            ResetBoardButton.Text = "Reset board";
            ResetBoardButton.UseVisualStyleBackColor = true;
            ResetBoardButton.Click += ResetBoardButton_Click;
            // 
            // ResetTilesButton
            // 
            ResetTilesButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            ResetTilesButton.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            ResetTilesButton.Location = new Point(968, 740);
            ResetTilesButton.Name = "ResetTilesButton";
            ResetTilesButton.Size = new Size(116, 43);
            ResetTilesButton.TabIndex = 22;
            ResetTilesButton.Text = "Reset tiles";
            ResetTilesButton.UseVisualStyleBackColor = true;
            ResetTilesButton.Click += ResetTilesButton_Click;
            // 
            // BingoScore
            // 
            BingoScore.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            BingoScore.Location = new Point(128, 128);
            BingoScore.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            BingoScore.Name = "BingoScore";
            BingoScore.Size = new Size(73, 33);
            BingoScore.TabIndex = 23;
            BingoScore.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label8.Location = new Point(12, 130);
            label8.Name = "label8";
            label8.Size = new Size(108, 25);
            label8.TabIndex = 24;
            label8.Text = "Bingo score";
            label8.TextAlign = ContentAlignment.TopRight;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            label9.Location = new Point(15, 15);
            label9.Name = "label9";
            label9.Size = new Size(186, 25);
            label9.TabIndex = 25;
            label9.Text = "Description of variant";
            label9.TextAlign = ContentAlignment.TopRight;
            // 
            // Description
            // 
            Description.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            Description.Location = new Point(207, 12);
            Description.Name = "Description";
            Description.Size = new Size(507, 33);
            Description.TabIndex = 26;
            // 
            // LoadButton
            // 
            LoadButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            LoadButton.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            LoadButton.Location = new Point(1090, 12);
            LoadButton.Name = "LoadButton";
            LoadButton.Size = new Size(127, 43);
            LoadButton.TabIndex = 27;
            LoadButton.Text = "Load";
            LoadButton.UseVisualStyleBackColor = true;
            LoadButton.Click += LoadButton_Click;
            // 
            // SaveButton
            // 
            SaveButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            SaveButton.Font = new Font("Segoe UI Semilight", 14.25F, FontStyle.Regular, GraphicsUnit.Point);
            SaveButton.Location = new Point(1090, 61);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new Size(127, 43);
            SaveButton.TabIndex = 28;
            SaveButton.Text = "Save";
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;
            // 
            // RulesetEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1229, 795);
            Controls.Add(SaveButton);
            Controls.Add(LoadButton);
            Controls.Add(Description);
            Controls.Add(label9);
            Controls.Add(label8);
            Controls.Add(BingoScore);
            Controls.Add(ResetTilesButton);
            Controls.Add(ResetBoardButton);
            Controls.Add(ValidateWords);
            Controls.Add(label7);
            Controls.Add(ClearTilesButton);
            Controls.Add(ClearBoardDesign);
            Controls.Add(Player2Name);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(Player1Name);
            Controls.Add(TileTotal);
            Controls.Add(AllowFlip);
            Controls.Add(label3);
            Controls.Add(BoardEditor);
            Controls.Add(label4);
            Controls.Add(TileDistributionEditor);
            Controls.Add(label2);
            Controls.Add(BoardSize);
            Controls.Add(label1);
            Controls.Add(RackSize);
            Controls.Add(ContinueButton);
            Name = "RulesetEditor";
            Text = "Rules editor";
            ((System.ComponentModel.ISupportInitialize)RackSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)BoardSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)BingoScore).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button ContinueButton;
        private NumericUpDown RackSize;
        private Label label1;
        private Label label2;
        private NumericUpDown BoardSize;
        private TableLayoutPanel TileDistributionEditor;
        private Label label4;
        private TableLayoutPanel BoardEditor;
        private Label label3;
        private CheckBox AllowFlip;
        private Label TileTotal;
        private TextBox Player1Name;
        private Label label5;
        private Label label6;
        private TextBox Player2Name;
        private Button ClearBoardDesign;
        private Button ClearTilesButton;
        private CheckBox ValidateWords;
        private Label label7;
        private Button ResetBoardButton;
        private Button ResetTilesButton;
        private NumericUpDown BingoScore;
        private Label label8;
        private Label label9;
        private TextBox Description;
        private Button LoadButton;
        private Button SaveButton;
    }
}