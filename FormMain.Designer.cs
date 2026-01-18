
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
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
            genericTestButton = new System.Windows.Forms.Button();
            debugMenuButton = new System.Windows.Forms.Button();
            ProfilePanel = new System.Windows.Forms.Panel();
            resetProfileButton = new System.Windows.Forms.Button();
            DecodePanel = new System.Windows.Forms.Panel();
            planetGridPanel = new System.Windows.Forms.Panel();
            planetCountLabel = new System.Windows.Forms.Label();
            ExportPanel = new System.Windows.Forms.Panel();
            heatmapCheckbox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)planetGridView).BeginInit();
            debugPanel.SuspendLayout();
            ProfilePanel.SuspendLayout();
            DecodePanel.SuspendLayout();
            planetGridPanel.SuspendLayout();
            ExportPanel.SuspendLayout();
            SuspendLayout();
            // 
            // findProfileButton
            // 
            findProfileButton.Location = new System.Drawing.Point(14, 4);
            findProfileButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            findProfileButton.Name = "findProfileButton";
            findProfileButton.Size = new System.Drawing.Size(121, 28);
            findProfileButton.TabIndex = 1;
            findProfileButton.Text = "Find Profile";
            findProfileButton.UseVisualStyleBackColor = true;
            findProfileButton.Click += findProfileButton_Click;
            // 
            // profileFolderTextBox
            // 
            profileFolderTextBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            profileFolderTextBox.Location = new System.Drawing.Point(14, 36);
            profileFolderTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            profileFolderTextBox.Name = "profileFolderTextBox";
            profileFolderTextBox.ReadOnly = true;
            profileFolderTextBox.Size = new System.Drawing.Size(956, 22);
            profileFolderTextBox.TabIndex = 2;
            // 
            // decodeReadyStatusLabel
            // 
            decodeReadyStatusLabel.AutoSize = true;
            decodeReadyStatusLabel.Location = new System.Drawing.Point(13, 4);
            decodeReadyStatusLabel.Name = "decodeReadyStatusLabel";
            decodeReadyStatusLabel.Size = new System.Drawing.Size(70, 14);
            decodeReadyStatusLabel.TabIndex = 3;
            decodeReadyStatusLabel.Text = "Not Ready";
            // 
            // decodeButton
            // 
            decodeButton.Location = new System.Drawing.Point(13, 25);
            decodeButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            decodeButton.Name = "decodeButton";
            decodeButton.Size = new System.Drawing.Size(141, 28);
            decodeButton.TabIndex = 4;
            decodeButton.Text = "Decode Planets";
            decodeButton.UseVisualStyleBackColor = true;
            decodeButton.Click += decodeButton_Click;
            // 
            // decodeProgressBar
            // 
            decodeProgressBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            decodeProgressBar.Location = new System.Drawing.Point(286, 24);
            decodeProgressBar.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            decodeProgressBar.Name = "decodeProgressBar";
            decodeProgressBar.Size = new System.Drawing.Size(683, 28);
            decodeProgressBar.TabIndex = 5;
            // 
            // decodeProgressLabel
            // 
            decodeProgressLabel.AutoSize = true;
            decodeProgressLabel.Location = new System.Drawing.Point(286, 6);
            decodeProgressLabel.Name = "decodeProgressLabel";
            decodeProgressLabel.Size = new System.Drawing.Size(49, 14);
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
            planetGridView.ColumnHeadersHeight = 30;
            planetGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            planetGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { NameCol, MinimapCol, ScoreCol, Giant1Col, Giant2Col, Giant3Col, SpiritCol, SpiritIconCol, CompletionCol, ReadOptionCol, ReadStatusCol });
            planetGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            planetGridView.Location = new System.Drawing.Point(14, 39);
            planetGridView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            planetGridView.Name = "planetGridView";
            planetGridView.RowHeadersWidth = 51;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            planetGridView.RowsDefaultCellStyle = dataGridViewCellStyle1;
            planetGridView.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            planetGridView.ShowEditingIcon = false;
            planetGridView.Size = new System.Drawing.Size(955, 417);
            planetGridView.TabIndex = 7;
            // 
            // NameCol
            // 
            NameCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            NameCol.HeaderText = "Name";
            NameCol.MinimumWidth = 6;
            NameCol.Name = "NameCol";
            NameCol.ReadOnly = true;
            NameCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // MinimapCol
            // 
            MinimapCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            MinimapCol.HeaderText = "Minimap";
            MinimapCol.MinimumWidth = 6;
            MinimapCol.Name = "MinimapCol";
            MinimapCol.Width = 125;
            // 
            // ScoreCol
            // 
            ScoreCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            ScoreCol.HeaderText = "Score";
            ScoreCol.MinimumWidth = 6;
            ScoreCol.Name = "ScoreCol";
            ScoreCol.ReadOnly = true;
            ScoreCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            ScoreCol.Width = 48;
            // 
            // Giant1Col
            // 
            Giant1Col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Giant1Col.HeaderText = "G1";
            Giant1Col.MinimumWidth = 6;
            Giant1Col.Name = "Giant1Col";
            Giant1Col.Width = 25;
            // 
            // Giant2Col
            // 
            Giant2Col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Giant2Col.HeaderText = "G2";
            Giant2Col.MinimumWidth = 6;
            Giant2Col.Name = "Giant2Col";
            Giant2Col.Width = 25;
            // 
            // Giant3Col
            // 
            Giant3Col.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            Giant3Col.HeaderText = "G3";
            Giant3Col.MinimumWidth = 6;
            Giant3Col.Name = "Giant3Col";
            Giant3Col.Width = 25;
            // 
            // SpiritCol
            // 
            SpiritCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            SpiritCol.HeaderText = "Spirit";
            SpiritCol.MinimumWidth = 6;
            SpiritCol.Name = "SpiritCol";
            SpiritCol.ReadOnly = true;
            SpiritCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // SpiritIconCol
            // 
            SpiritIconCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            SpiritIconCol.HeaderText = "S";
            SpiritIconCol.MinimumWidth = 6;
            SpiritIconCol.Name = "SpiritIconCol";
            SpiritIconCol.Width = 25;
            // 
            // CompletionCol
            // 
            CompletionCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            CompletionCol.HeaderText = "SaveSlot";
            CompletionCol.MinimumWidth = 6;
            CompletionCol.Name = "CompletionCol";
            CompletionCol.ReadOnly = true;
            CompletionCol.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            CompletionCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            CompletionCol.Width = 69;
            // 
            // ReadOptionCol
            // 
            ReadOptionCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            ReadOptionCol.HeaderText = "Read?";
            ReadOptionCol.MinimumWidth = 6;
            ReadOptionCol.Name = "ReadOptionCol";
            ReadOptionCol.Width = 48;
            // 
            // ReadStatusCol
            // 
            ReadStatusCol.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            ReadStatusCol.HeaderText = "Status";
            ReadStatusCol.MinimumWidth = 6;
            ReadStatusCol.Name = "ReadStatusCol";
            ReadStatusCol.ReadOnly = true;
            ReadStatusCol.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            ReadStatusCol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            ReadStatusCol.Width = 55;
            // 
            // exportStatsButton
            // 
            exportStatsButton.Enabled = false;
            exportStatsButton.Location = new System.Drawing.Point(14, 5);
            exportStatsButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            exportStatsButton.Name = "exportStatsButton";
            exportStatsButton.Size = new System.Drawing.Size(98, 28);
            exportStatsButton.TabIndex = 8;
            exportStatsButton.Text = "Export XLSX";
            exportStatsButton.UseVisualStyleBackColor = true;
            exportStatsButton.Click += exportStatsButton_Click;
            // 
            // exportReadyLabel
            // 
            exportReadyLabel.AutoSize = true;
            exportReadyLabel.Location = new System.Drawing.Point(119, 9);
            exportReadyLabel.Name = "exportReadyLabel";
            exportReadyLabel.Size = new System.Drawing.Size(49, 14);
            exportReadyLabel.TabIndex = 9;
            exportReadyLabel.Text = "------";
            // 
            // spotCheckButton
            // 
            spotCheckButton.Location = new System.Drawing.Point(84, 5);
            spotCheckButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            spotCheckButton.Name = "spotCheckButton";
            spotCheckButton.Size = new System.Drawing.Size(86, 28);
            spotCheckButton.TabIndex = 10;
            spotCheckButton.Text = "Spot Check";
            spotCheckButton.UseVisualStyleBackColor = true;
            spotCheckButton.Click += spotCheckButton_Click;
            // 
            // spotCheckWriteCheckBox
            // 
            spotCheckWriteCheckBox.AutoSize = true;
            spotCheckWriteCheckBox.Checked = true;
            spotCheckWriteCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            spotCheckWriteCheckBox.Location = new System.Drawing.Point(176, 11);
            spotCheckWriteCheckBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            spotCheckWriteCheckBox.Name = "spotCheckWriteCheckBox";
            spotCheckWriteCheckBox.Size = new System.Drawing.Size(117, 18);
            spotCheckWriteCheckBox.TabIndex = 11;
            spotCheckWriteCheckBox.Text = "Write Decoded";
            spotCheckWriteCheckBox.UseVisualStyleBackColor = true;
            spotCheckWriteCheckBox.CheckStateChanged += spotCheckWriteCheckBox_CheckStateChanged;
            // 
            // writeDecodedCheckBox
            // 
            writeDecodedCheckBox.AutoSize = true;
            writeDecodedCheckBox.Location = new System.Drawing.Point(160, 29);
            writeDecodedCheckBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            writeDecodedCheckBox.Name = "writeDecodedCheckBox";
            writeDecodedCheckBox.Size = new System.Drawing.Size(117, 18);
            writeDecodedCheckBox.TabIndex = 12;
            writeDecodedCheckBox.Text = "Write Decoded";
            writeDecodedCheckBox.UseVisualStyleBackColor = true;
            writeDecodedCheckBox.CheckStateChanged += writeDecodedCheckBox_CheckStateChanged;
            // 
            // label2
            // 
            label2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(3, -79);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(105, 14);
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
            readAllButton.Location = new System.Drawing.Point(779, 4);
            readAllButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            readAllButton.Name = "readAllButton";
            readAllButton.Size = new System.Drawing.Size(86, 28);
            readAllButton.TabIndex = 15;
            readAllButton.Text = "Check All";
            readAllButton.UseVisualStyleBackColor = true;
            readAllButton.Click += readAllButton_Click;
            // 
            // readNoneButton
            // 
            readNoneButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            readNoneButton.Location = new System.Drawing.Point(872, 4);
            readNoneButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            readNoneButton.Name = "readNoneButton";
            readNoneButton.Size = new System.Drawing.Size(98, 28);
            readNoneButton.TabIndex = 16;
            readNoneButton.Text = "Check None";
            readNoneButton.UseVisualStyleBackColor = true;
            readNoneButton.Click += readNoneButton_Click;
            // 
            // debugPanel
            // 
            debugPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            debugPanel.Controls.Add(genericTestButton);
            debugPanel.Controls.Add(label2);
            debugPanel.Controls.Add(spotCheckButton);
            debugPanel.Controls.Add(spotCheckWriteCheckBox);
            debugPanel.Location = new System.Drawing.Point(617, 580);
            debugPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            debugPanel.Name = "debugPanel";
            debugPanel.Size = new System.Drawing.Size(320, 36);
            debugPanel.TabIndex = 17;
            debugPanel.Visible = false;
            // 
            // genericTestButton
            // 
            genericTestButton.Location = new System.Drawing.Point(3, 5);
            genericTestButton.Name = "genericTestButton";
            genericTestButton.Size = new System.Drawing.Size(75, 28);
            genericTestButton.TabIndex = 15;
            genericTestButton.Text = "Test!";
            genericTestButton.UseVisualStyleBackColor = true;
            genericTestButton.Visible = false;
            genericTestButton.Click += genericTestButton_Click;
            // 
            // debugMenuButton
            // 
            debugMenuButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            debugMenuButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            debugMenuButton.Location = new System.Drawing.Point(943, 585);
            debugMenuButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            debugMenuButton.Name = "debugMenuButton";
            debugMenuButton.Size = new System.Drawing.Size(26, 28);
            debugMenuButton.TabIndex = 18;
            debugMenuButton.Text = "D";
            debugMenuButton.UseVisualStyleBackColor = true;
            debugMenuButton.Click += debugMenuButton_Click;
            // 
            // ProfilePanel
            // 
            ProfilePanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            ProfilePanel.Controls.Add(resetProfileButton);
            ProfilePanel.Controls.Add(findProfileButton);
            ProfilePanel.Controls.Add(profileFolderTextBox);
            ProfilePanel.Location = new System.Drawing.Point(0, 0);
            ProfilePanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            ProfilePanel.Name = "ProfilePanel";
            ProfilePanel.Size = new System.Drawing.Size(984, 68);
            ProfilePanel.TabIndex = 19;
            // 
            // resetProfileButton
            // 
            resetProfileButton.Location = new System.Drawing.Point(141, 4);
            resetProfileButton.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            resetProfileButton.Name = "resetProfileButton";
            resetProfileButton.Size = new System.Drawing.Size(86, 28);
            resetProfileButton.TabIndex = 3;
            resetProfileButton.Text = "⟳ Reset";
            resetProfileButton.UseVisualStyleBackColor = true;
            resetProfileButton.Click += resetProfileButton_Click;
            // 
            // DecodePanel
            // 
            DecodePanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            DecodePanel.Controls.Add(writeDecodedCheckBox);
            DecodePanel.Controls.Add(decodeProgressLabel);
            DecodePanel.Controls.Add(decodeProgressBar);
            DecodePanel.Controls.Add(decodeButton);
            DecodePanel.Controls.Add(decodeReadyStatusLabel);
            DecodePanel.Location = new System.Drawing.Point(0, 57);
            DecodePanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            DecodePanel.Name = "DecodePanel";
            DecodePanel.Size = new System.Drawing.Size(984, 69);
            DecodePanel.TabIndex = 20;
            // 
            // planetGridPanel
            // 
            planetGridPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            planetGridPanel.Controls.Add(planetCountLabel);
            planetGridPanel.Controls.Add(readNoneButton);
            planetGridPanel.Controls.Add(readAllButton);
            planetGridPanel.Controls.Add(planetGridView);
            planetGridPanel.Location = new System.Drawing.Point(0, 112);
            planetGridPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            planetGridPanel.Name = "planetGridPanel";
            planetGridPanel.Size = new System.Drawing.Size(984, 460);
            planetGridPanel.TabIndex = 21;
            // 
            // planetCountLabel
            // 
            planetCountLabel.AutoSize = true;
            planetCountLabel.Location = new System.Drawing.Point(14, 14);
            planetCountLabel.Name = "planetCountLabel";
            planetCountLabel.Size = new System.Drawing.Size(49, 14);
            planetCountLabel.TabIndex = 17;
            planetCountLabel.Text = "label1";
            // 
            // ExportPanel
            // 
            ExportPanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            ExportPanel.Controls.Add(heatmapCheckbox);
            ExportPanel.Controls.Add(exportStatsButton);
            ExportPanel.Controls.Add(exportReadyLabel);
            ExportPanel.Location = new System.Drawing.Point(0, 568);
            ExportPanel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            ExportPanel.Name = "ExportPanel";
            ExportPanel.Size = new System.Drawing.Size(368, 56);
            ExportPanel.TabIndex = 22;
            // 
            // heatmapCheckbox
            // 
            heatmapCheckbox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            heatmapCheckbox.AutoSize = true;
            heatmapCheckbox.Location = new System.Drawing.Point(14, 35);
            heatmapCheckbox.Name = "heatmapCheckbox";
            heatmapCheckbox.Size = new System.Drawing.Size(138, 18);
            heatmapCheckbox.TabIndex = 10;
            heatmapCheckbox.Text = "Include Heatmaps";
            heatmapCheckbox.UseVisualStyleBackColor = true;
            heatmapCheckbox.CheckedChanged += heatmapCheckbox_CheckedChanged;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(984, 622);
            Controls.Add(ExportPanel);
            Controls.Add(planetGridPanel);
            Controls.Add(DecodePanel);
            Controls.Add(ProfilePanel);
            Controls.Add(debugMenuButton);
            Controls.Add(debugPanel);
            Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            Name = "FormMain";
            Text = "Reus 2 Planet Surveyor";
            Load += FormMain_Load;
            ((System.ComponentModel.ISupportInitialize)planetGridView).EndInit();
            debugPanel.ResumeLayout(false);
            debugPanel.PerformLayout();
            ProfilePanel.ResumeLayout(false);
            ProfilePanel.PerformLayout();
            DecodePanel.ResumeLayout(false);
            DecodePanel.PerformLayout();
            planetGridPanel.ResumeLayout(false);
            planetGridPanel.PerformLayout();
            ExportPanel.ResumeLayout(false);
            ExportPanel.PerformLayout();
            ResumeLayout(false);
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
        private System.Windows.Forms.Panel debugPanel;
        private System.Windows.Forms.Button debugMenuButton;
        private System.Windows.Forms.Panel ProfilePanel;
        private System.Windows.Forms.Panel DecodePanel;
        private System.Windows.Forms.Panel planetGridPanel;
        private System.Windows.Forms.Panel ExportPanel;
        private System.Windows.Forms.Button resetProfileButton;
        private System.Windows.Forms.Label planetCountLabel;
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
        private System.Windows.Forms.Button genericTestButton;
        private System.Windows.Forms.CheckBox heatmapCheckbox;
    }
}

