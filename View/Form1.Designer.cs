namespace LangBiasTester
{
    partial class Form1
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
            participantIDBox = new TextBox();
            SuspendLayout();
            // 
            // participantIDBox
            // 
            participantIDBox.Anchor = AnchorStyles.None;
            participantIDBox.BackColor = Color.Silver;
            participantIDBox.BorderStyle = BorderStyle.None;
            participantIDBox.Font = new Font("Consolas", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            participantIDBox.Location = new Point(0, 0);
            participantIDBox.Name = "participantIDBox";
            participantIDBox.PlaceholderText = "Participant ID";
            participantIDBox.Size = new Size(200, 15);
            participantIDBox.TabIndex = 0;
            participantIDBox.KeyDown += ParticipantIDBox_KeyDown;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(1920, 1080);
            Controls.Add(participantIDBox);
            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "Form1";
            ShowIcon = false;
            Text = "Form1";
            WindowState = FormWindowState.Maximized;
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        internal TextBox participantIDBox;
    }
}
