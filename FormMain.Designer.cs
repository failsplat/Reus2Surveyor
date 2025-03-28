
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
            exportStatsButton = new System.Windows.Forms.Button();
            exportReadyLabel = new System.Windows.Forms.Label();
            spotCheckButton = new System.Windows.Forms.Button();
            spotCheckWriteCheckBox = new System.Windows.Forms.CheckBox();
            writeDecodedCheckBox = new System.Windows.Forms.CheckBox();
            label2 = new System.Windows.Forms.Label();
            planetLooperBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            NameCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ScoreCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            SpiritCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            CompletionCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ReadOptionCol = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ReadStatusCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            profileFolderTextBox.Size = new System.Drawing.Size(629, 23);
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
            decodeProgressBar.Location = new System.Drawing.Point(251, 66);
            decodeProgressBar.Name = "decodeProgressBar";
            decodeProgressBar.Size = new System.Drawing.Size(471, 23);
            decodeProgressBar.TabIndex = 5;
            // 
            // decodeProgressLabel
            // 
            decodeProgressLabel.AutoSize = true;
            decodeProgressLabel.Location = new System.Drawing.Point(251, 48);
            decodeProgressLabel.Name = "decodeProgressLabel";
            decodeProgressLabel.Size = new System.Drawing.Size(37, 15);
            decodeProgressLabel.TabIndex = 6;
            decodeProgressLabel.Text = "------";
            decodeProgressLabel.Click += decodeProgressLabel_Click;
            // 
            // planetGridView
            // 
            planetGridView.AllowUserToAddRows = false;
            planetGridView.AllowUserToDeleteRows = false;
            planetGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            planetGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            planetGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { NameCol, ScoreCol, SpiritCol, CompletionCol, ReadOptionCol, ReadStatusCol });
            planetGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            planetGridView.Location = new System.Drawing.Point(12, 111);
            planetGridView.Name = "planetGridView";
            planetGridView.Size = new System.Drawing.Size(710, 314);
            planetGridView.TabIndex = 7;
            // 
            // exportStatsButton
            // 
            exportStatsButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            exportStatsButton.Enabled = false;
            exportStatsButton.Location = new System.Drawing.Point(12, 431);
            exportStatsButton.Name = "exportStatsButton";
            exportStatsButton.Size = new System.Drawing.Size(86, 23);
            exportStatsButton.TabIndex = 8;
            exportStatsButton.Text = "Export XLSX";
            exportStatsButton.UseVisualStyleBackColor = true;
            exportStatsButton.Click += exportStatsButton_Click;
            // 
            // exportReadyLabel
            // 
            exportReadyLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            exportReadyLabel.AutoSize = true;
            exportReadyLabel.Location = new System.Drawing.Point(104, 435);
            exportReadyLabel.Name = "exportReadyLabel";
            exportReadyLabel.Size = new System.Drawing.Size(37, 15);
            exportReadyLabel.TabIndex = 9;
            exportReadyLabel.Text = "------";
            // 
            // spotCheckButton
            // 
            spotCheckButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            spotCheckButton.Location = new System.Drawing.Point(537, 432);
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
            spotCheckWriteCheckBox.Location = new System.Drawing.Point(618, 435);
            spotCheckWriteCheckBox.Name = "spotCheckWriteCheckBox";
            spotCheckWriteCheckBox.Size = new System.Drawing.Size(104, 19);
            spotCheckWriteCheckBox.TabIndex = 11;
            spotCheckWriteCheckBox.Text = "Write Decoded";
            spotCheckWriteCheckBox.UseVisualStyleBackColor = true;
            spotCheckWriteCheckBox.CheckStateChanged += spotCheckWriteCheckBox_CheckStateChanged;
            // 
            // writeDecodedCheckBox
            // 
            writeDecodedCheckBox.AutoSize = true;
            writeDecodedCheckBox.Location = new System.Drawing.Point(141, 70);
            writeDecodedCheckBox.Name = "writeDecodedCheckBox";
            writeDecodedCheckBox.Size = new System.Drawing.Size(104, 19);
            writeDecodedCheckBox.TabIndex = 12;
            writeDecodedCheckBox.Text = "Write Decoded";
            writeDecodedCheckBox.UseVisualStyleBackColor = true;
            writeDecodedCheckBox.CheckStateChanged += writeDecodedCheckBox_CheckStateChanged;
            // 
            // label2
            // 
            label2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(442, 436);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(89, 15);
            label2.TabIndex = 14;
            label2.Text = "For Debugging:";
            // 
            // planetLooperBackgroundWorker
            // 
            planetLooperBackgroundWorker.WorkerReportsProgress = true;
            planetLooperBackgroundWorker.DoWork += planetLooperBackgroundWorker_DoWork;
            planetLooperBackgroundWorker.ProgressChanged += planetLooperBackgroundWorker_ProgressChanged;
            planetLooperBackgroundWorker.RunWorkerCompleted += planetLooperBackgroundWorker_RunWorkerCompleted;
            // 
            // NameCol
            // 
            NameCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            NameCol.HeaderText = "Name";
            NameCol.Name = "NameCol";
            NameCol.ReadOnly = true;
            // 
            // ScoreCol
            // 
            ScoreCol.HeaderText = "Score";
            ScoreCol.Name = "ScoreCol";
            // 
            // SpiritCol
            // 
            SpiritCol.HeaderText = "Spirit";
            SpiritCol.Name = "SpiritCol";
            // 
            // CompletionCol
            // 
            CompletionCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            CompletionCol.HeaderText = "SaveSlot";
            CompletionCol.Name = "CompletionCol";
            CompletionCol.ReadOnly = true;
            CompletionCol.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            CompletionCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            CompletionCol.Width = 57;
            // 
            // ReadOptionCol
            // 
            ReadOptionCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            ReadOptionCol.HeaderText = "ToRead?";
            ReadOptionCol.Name = "ReadOptionCol";
            ReadOptionCol.Width = 56;
            // 
            // ReadStatusCol
            // 
            ReadStatusCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            ReadStatusCol.HeaderText = "Status";
            ReadStatusCol.Name = "ReadStatusCol";
            ReadStatusCol.ReadOnly = true;
            ReadStatusCol.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            ReadStatusCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            ReadStatusCol.Width = 45;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(734, 461);
            Controls.Add(label2);
            Controls.Add(writeDecodedCheckBox);
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
            Text = "Reus 2 Planet Surveyor";
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
        private System.Windows.Forms.Button spotCheckButton;
        private System.Windows.Forms.CheckBox spotCheckWriteCheckBox;
        private System.Windows.Forms.CheckBox writeDecodedCheckBox;
        private System.Windows.Forms.Label label2;
        private System.ComponentModel.BackgroundWorker planetLooperBackgroundWorker;
        private System.Windows.Forms.DataGridViewTextBoxColumn NameCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn ScoreCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn SpiritCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn CompletionCol;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ReadOptionCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn ReadStatusCol;
    }
}

