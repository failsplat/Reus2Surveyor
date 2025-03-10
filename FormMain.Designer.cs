
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
            testDecodeButton = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // testDecodeButton
            // 
            testDecodeButton.Location = new System.Drawing.Point(12, 12);
            testDecodeButton.Name = "testDecodeButton";
            testDecodeButton.Size = new System.Drawing.Size(75, 23);
            testDecodeButton.TabIndex = 0;
            testDecodeButton.Text = "button1";
            testDecodeButton.UseVisualStyleBackColor = true;
            testDecodeButton.Click += testDecodeButton_Click;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(testDecodeButton);
            Name = "FormMain";
            Text = "Form1";
            Load += FormMain_Load;
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button testDecodeButton;
    }
}

