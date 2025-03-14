
namespace Reus2Surveyor
{
    partial class FormMain
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
            findProfileButton = new System.Windows.Forms.Button();
            profileFolderTextBox = new System.Windows.Forms.TextBox();
            decodeReadyStatusLabel = new System.Windows.Forms.Label();
            decodeButton = new System.Windows.Forms.Button();
            decodeProgressBar = new System.Windows.Forms.ProgressBar();
            decodeProgressLabel = new System.Windows.Forms.Label();
            planetGridView = new System.Windows.Forms.DataGridView();
            NameCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            HasCompleteCol = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ReadOKCol = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            exportStatsButton = new System.Windows.Forms.Button();
            exportReadyLabel = new System.Windows.Forms.Label();
            spotCheckButton = new System.Windows.Forms.Button();
            spotCheckWriteCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)planetGridView).BeginInit();
            SuspendLayout();
            // 
            // findProfileButton
            // 
            findProfileButton.Location = new System.Drawing.Point(12, 12);
            findProfileButton.Name = "findProfileButton";
            findProfileButton.Size = new System.Drawing.Size(75, 23);
            findProfileButton.TabIndex = 1;
            findProfileButton.Text = "Find Profile";
            findProfileButton.UseVisualStyleBackColor = true;
            findProfileButton.Click += findProfileButton_Click;
            // 
            // profileFolderTextBox
            // 
            profileFolderTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            profileFolderTextBox.Location = new System.Drawing.Point(93, 12);
            profileFolderTextBox.Name = "profileFolderTextBox";
            profileFolderTextBox.Size = new System.Drawing.Size(804, 23);
            profileFolderTextBox.TabIndex = 2;
            // 
            // decodeReadyStatusLabel
            // 
            decodeReadyStatusLabel.AutoSize = true;
            decodeReadyStatusLabel.Location = new System.Drawing.Point(12, 49);
            decodeReadyStatusLabel.Name = "decodeReadyStatusLabel";
            decodeReadyStatusLabel.Size = new System.Drawing.Size(62, 15);
            decodeReadyStatusLabel.TabIndex = 3;
            decodeReadyStatusLabel.Text = "Not Ready";
            // 
            // decodeButton
            // 
            decodeButton.Location = new System.Drawing.Point(12, 67);
            decodeButton.Name = "decodeButton";
            decodeButton.Size = new System.Drawing.Size(123, 23);
            decodeButton.TabIndex = 4;
            decodeButton.Text = "Decode Planets";
            decodeButton.UseVisualStyleBackColor = true;
            decodeButton.Click += decodeButton_Click;
            // 
            // decodeProgressBar
            // 
            decodeProgressBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            decodeProgressBar.Location = new System.Drawing.Point(141, 67);
            decodeProgressBar.Name = "decodeProgressBar";
            decodeProgressBar.Size = new System.Drawing.Size(756, 23);
            decodeProgressBar.TabIndex = 5;
            // 
            // decodeProgressLabel
            // 
            decodeProgressLabel.AutoSize = true;
            decodeProgressLabel.Location = new System.Drawing.Point(141, 49);
            decodeProgressLabel.Name = "decodeProgressLabel";
            decodeProgressLabel.Size = new System.Drawing.Size(37, 15);
            decodeProgressLabel.TabIndex = 6;
            decodeProgressLabel.Text = "------";
            // 
            // planetGridView
            // 
            planetGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            planetGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            planetGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { NameCol, HasCompleteCol, ReadOKCol });
            planetGridView.Location = new System.Drawing.Point(12, 111);
            planetGridView.Name = "planetGridView";
            planetGridView.Size = new System.Drawing.Size(885, 372);
            planetGridView.TabIndex = 7;
            // 
            // NameCol
            // 
            NameCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            NameCol.HeaderText = "Name";
            NameCol.Name = "NameCol";
            NameCol.ReadOnly = true;
            // 
            // HasCompleteCol
            // 
            HasCompleteCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            HasCompleteCol.HeaderText = "C";
            HasCompleteCol.Name = "HasCompleteCol";
            HasCompleteCol.ReadOnly = true;
            HasCompleteCol.Width = 21;
            // 
            // ReadOKCol
            // 
            ReadOKCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            ReadOKCol.HeaderText = "ReadOK";
            ReadOKCol.Name = "ReadOKCol";
            ReadOKCol.ReadOnly = true;
            ReadOKCol.Width = 55;
            // 
            // exportStatsButton
            // 
            exportStatsButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            exportStatsButton.Location = new System.Drawing.Point(12, 489);
            exportStatsButton.Name = "exportStatsButton";
            exportStatsButton.Size = new System.Drawing.Size(86, 23);
            exportStatsButton.TabIndex = 8;
            exportStatsButton.Text = "Export Stats";
            exportStatsButton.UseVisualStyleBackColor = true;
            exportStatsButton.Click += exportStatsButton_Click;
            // 
            // exportReadyLabel
            // 
            exportReadyLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            exportReadyLabel.AutoSize = true;
            exportReadyLabel.Location = new System.Drawing.Point(104, 493);
            exportReadyLabel.Name = "exportReadyLabel";
            exportReadyLabel.Size = new System.Drawing.Size(37, 15);
            exportReadyLabel.TabIndex = 9;
            exportReadyLabel.Text = "------";
            // 
            // spotCheckButton
            // 
            spotCheckButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            spotCheckButton.Location = new System.Drawing.Point(712, 490);
            spotCheckButton.Name = "spotCheckButton";
            spotCheckButton.Size = new System.Drawing.Size(75, 23);
            spotCheckButton.TabIndex = 10;
            spotCheckButton.Text = "Spot Check";
            spotCheckButton.UseVisualStyleBackColor = true;
            spotCheckButton.Click += spotCheckButton_Click;
            // 
            // spotCheckWriteCheckBox
            // 
            spotCheckWriteCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            spotCheckWriteCheckBox.AutoSize = true;
            spotCheckWriteCheckBox.Checked = true;
            spotCheckWriteCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            spotCheckWriteCheckBox.Location = new System.Drawing.Point(793, 493);
            spotCheckWriteCheckBox.Name = "spotCheckWriteCheckBox";
            spotCheckWriteCheckBox.Size = new System.Drawing.Size(104, 19);
            spotCheckWriteCheckBox.TabIndex = 11;
            spotCheckWriteCheckBox.Text = "Write Decoded";
            spotCheckWriteCheckBox.UseVisualStyleBackColor = true;
            spotCheckWriteCheckBox.CheckStateChanged += spotCheckWriteCheckBox_CheckStateChanged;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(909, 519);
            Controls.Add(spotCheckWriteCheckBox);
            Controls.Add(spotCheckButton);
            Controls.Add(exportReadyLabel);
            Controls.Add(exportStatsButton);
            Controls.Add(planetGridView);
            Controls.Add(decodeProgressLabel);
            Controls.Add(decodeProgressBar);
            Controls.Add(decodeButton);
            Controls.Add(decodeReadyStatusLabel);
            Controls.Add(profileFolderTextBox);
            Controls.Add(findProfileButton);
            Name = "FormMain";
            Text = "Form1";
            Load += FormMain_Load;
            ((System.ComponentModel.ISupportInitialize)planetGridView).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Button findProfileButton;
        private System.Windows.Forms.TextBox profileFolderTextBox;
        private System.Windows.Forms.Label decodeReadyStatusLabel;
        private System.Windows.Forms.Button decodeButton;
        private System.Windows.Forms.ProgressBar decodeProgressBar;
        private System.Windows.Forms.Label decodeProgressLabel;
        private System.Windows.Forms.DataGridView planetGridView;
        private System.Windows.Forms.Button exportStatsButton;
        private System.Windows.Forms.Label exportReadyLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn NameCol;
        private System.Windows.Forms.DataGridViewCheckBoxColumn HasCompleteCol;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ReadOKCol;
        private System.Windows.Forms.Button spotCheckButton;
        private System.Windows.Forms.CheckBox spotCheckWriteCheckBox;
    }
}

