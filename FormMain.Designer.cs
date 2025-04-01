
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            findProfileButton = new System.Windows.Forms.Button();
            profileFolderTextBox = new System.Windows.Forms.TextBox();
            decodeReadyStatusLabel = new System.Windows.Forms.Label();
            decodeButton = new System.Windows.Forms.Button();
            decodeProgressBar = new System.Windows.Forms.ProgressBar();
            decodeProgressLabel = new System.Windows.Forms.Label();
            planetGridView = new System.Windows.Forms.DataGridView();
            NameCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            MinimapCol = new System.Windows.Forms.DataGridViewImageColumn();
            ScoreCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            Giant1Col = new System.Windows.Forms.DataGridViewImageColumn();
            Giant2Col = new System.Windows.Forms.DataGridViewImageColumn();
            Giant3Col = new System.Windows.Forms.DataGridViewImageColumn();
            SpiritCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            SpiritIconCol = new System.Windows.Forms.DataGridViewImageColumn();
            CompletionCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ReadOptionCol = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ReadStatusCol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            exportStatsButton = new System.Windows.Forms.Button();
            exportReadyLabel = new System.Windows.Forms.Label();
            spotCheckButton = new System.Windows.Forms.Button();
            spotCheckWriteCheckBox = new System.Windows.Forms.CheckBox();
            writeDecodedCheckBox = new System.Windows.Forms.CheckBox();
            label2 = new System.Windows.Forms.Label();
            planetLooperBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            readAllButton = new System.Windows.Forms.Button();
            readNoneButton = new System.Windows.Forms.Button();
            debugPanel = new System.Windows.Forms.Panel();
            debugMenuButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)planetGridView).BeginInit();
            debugPanel.SuspendLayout();
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
            profileFolderTextBox.ReadOnly = true;
            profileFolderTextBox.Size = new System.Drawing.Size(679, 23);
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
            decodeProgressBar.Size = new System.Drawing.Size(521, 23);
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
            planetGridView.AllowUserToResizeColumns = false;
            planetGridView.AllowUserToResizeRows = false;
            planetGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            planetGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            planetGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { NameCol, MinimapCol, ScoreCol, Giant1Col, Giant2Col, Giant3Col, SpiritCol, SpiritIconCol, CompletionCol, ReadOptionCol, ReadStatusCol });
            planetGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            planetGridView.Location = new System.Drawing.Point(12, 124);
            planetGridView.Name = "planetGridView";
            planetGridView.Size = new System.Drawing.Size(760, 301);
            planetGridView.TabIndex = 7;
            // 
            // NameCol
            // 
            NameCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            NameCol.HeaderText = "Name";
            NameCol.Name = "NameCol";
            NameCol.ReadOnly = true;
            NameCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // MinimapCol
            // 
            MinimapCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            MinimapCol.HeaderText = "Minimap";
            MinimapCol.Name = "MinimapCol";
            // 
            // ScoreCol
            // 
            ScoreCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            ScoreCol.HeaderText = "Score";
            ScoreCol.Name = "ScoreCol";
            ScoreCol.ReadOnly = true;
            ScoreCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            ScoreCol.Width = 42;
            // 
            // Giant1Col
            // 
            Giant1Col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Giant1Col.HeaderText = "G1";
            Giant1Col.Name = "Giant1Col";
            Giant1Col.Width = 25;
            // 
            // Giant2Col
            // 
            Giant2Col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Giant2Col.HeaderText = "G2";
            Giant2Col.Name = "Giant2Col";
            Giant2Col.Width = 25;
            // 
            // Giant3Col
            // 
            Giant3Col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Giant3Col.HeaderText = "G3";
            Giant3Col.Name = "Giant3Col";
            Giant3Col.Width = 25;
            // 
            // SpiritCol
            // 
            SpiritCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            SpiritCol.HeaderText = "Spirit";
            SpiritCol.Name = "SpiritCol";
            SpiritCol.ReadOnly = true;
            SpiritCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            SpiritCol.Width = 60;
            // 
            // SpiritIconCol
            // 
            SpiritIconCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            SpiritIconCol.HeaderText = "S";
            SpiritIconCol.Name = "SpiritIconCol";
            SpiritIconCol.Width = 25;
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
            ReadOptionCol.HeaderText = "Read?";
            ReadOptionCol.Name = "ReadOptionCol";
            ReadOptionCol.Width = 44;
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
            spotCheckButton.Location = new System.Drawing.Point(92, 1);
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
            spotCheckWriteCheckBox.Location = new System.Drawing.Point(173, 3);
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
            label2.Location = new System.Drawing.Point(3, 5);
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
            // readAllButton
            // 
            readAllButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            readAllButton.Location = new System.Drawing.Point(605, 95);
            readAllButton.Name = "readAllButton";
            readAllButton.Size = new System.Drawing.Size(75, 23);
            readAllButton.TabIndex = 15;
            readAllButton.Text = "Check All";
            readAllButton.UseVisualStyleBackColor = true;
            readAllButton.Click += readAllButton_Click;
            // 
            // readNoneButton
            // 
            readNoneButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            readNoneButton.Location = new System.Drawing.Point(686, 95);
            readNoneButton.Name = "readNoneButton";
            readNoneButton.Size = new System.Drawing.Size(86, 23);
            readNoneButton.TabIndex = 16;
            readNoneButton.Text = "Check None";
            readNoneButton.UseVisualStyleBackColor = true;
            readNoneButton.Click += readNoneButton_Click;
            // 
            // debugPanel
            // 
            debugPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            debugPanel.Controls.Add(label2);
            debugPanel.Controls.Add(spotCheckButton);
            debugPanel.Controls.Add(spotCheckWriteCheckBox);
            debugPanel.Location = new System.Drawing.Point(463, 432);
            debugPanel.Name = "debugPanel";
            debugPanel.Size = new System.Drawing.Size(280, 24);
            debugPanel.TabIndex = 17;
            debugPanel.Visible = false;
            // 
            // debugMenuButton
            // 
            debugMenuButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            debugMenuButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            debugMenuButton.Location = new System.Drawing.Point(749, 433);
            debugMenuButton.Name = "debugMenuButton";
            debugMenuButton.Size = new System.Drawing.Size(23, 23);
            debugMenuButton.TabIndex = 18;
            debugMenuButton.Text = "D";
            debugMenuButton.UseVisualStyleBackColor = true;
            debugMenuButton.Click += debugMenuButton_Click;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(784, 461);
            Controls.Add(debugMenuButton);
            Controls.Add(readNoneButton);
            Controls.Add(readAllButton);
            Controls.Add(writeDecodedCheckBox);
            Controls.Add(exportReadyLabel);
            Controls.Add(exportStatsButton);
            Controls.Add(planetGridView);
            Controls.Add(decodeProgressLabel);
            Controls.Add(decodeProgressBar);
            Controls.Add(decodeButton);
            Controls.Add(decodeReadyStatusLabel);
            Controls.Add(profileFolderTextBox);
            Controls.Add(findProfileButton);
            Controls.Add(debugPanel);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "FormMain";
            Text = "Reus 2 Planet Surveyor";
            Load += FormMain_Load;
            ((System.ComponentModel.ISupportInitialize)planetGridView).EndInit();
            debugPanel.ResumeLayout(false);
            debugPanel.PerformLayout();
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
        private System.Windows.Forms.Button readAllButton;
        private System.Windows.Forms.Button readNoneButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn NameCol;
        private System.Windows.Forms.DataGridViewImageColumn MinimapCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn ScoreCol;
        private System.Windows.Forms.DataGridViewImageColumn Giant1Col;
        private System.Windows.Forms.DataGridViewImageColumn Giant2Col;
        private System.Windows.Forms.DataGridViewImageColumn Giant3Col;
        private System.Windows.Forms.DataGridViewTextBoxColumn SpiritCol;
        private System.Windows.Forms.DataGridViewImageColumn SpiritIconCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn CompletionCol;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ReadOptionCol;
        private System.Windows.Forms.DataGridViewTextBoxColumn ReadStatusCol;
        private System.Windows.Forms.Panel debugPanel;
        private System.Windows.Forms.Button debugMenuButton;
    }
}

