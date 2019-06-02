namespace Death_Game_Launcher
{
    partial class SelectLogLevelForm
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
            this.infoRadio = new System.Windows.Forms.RadioButton();
            this.debugRadio = new System.Windows.Forms.RadioButton();
            this.warnRadio = new System.Windows.Forms.RadioButton();
            this.errorRadio = new System.Windows.Forms.RadioButton();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.acceptBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // infoRadio
            // 
            this.infoRadio.AutoSize = true;
            this.infoRadio.Location = new System.Drawing.Point(12, 35);
            this.infoRadio.Name = "infoRadio";
            this.infoRadio.Size = new System.Drawing.Size(43, 17);
            this.infoRadio.TabIndex = 1;
            this.infoRadio.TabStop = true;
            this.infoRadio.Tag = "INFO";
            this.infoRadio.Text = "Info";
            this.infoRadio.UseVisualStyleBackColor = true;
            this.infoRadio.Click += new System.EventHandler(this.Radio_Click);
            // 
            // debugRadio
            // 
            this.debugRadio.AutoSize = true;
            this.debugRadio.Location = new System.Drawing.Point(12, 12);
            this.debugRadio.Name = "debugRadio";
            this.debugRadio.Size = new System.Drawing.Size(57, 17);
            this.debugRadio.TabIndex = 0;
            this.debugRadio.TabStop = true;
            this.debugRadio.Tag = "DEBUG";
            this.debugRadio.Text = "Debug";
            this.debugRadio.UseVisualStyleBackColor = true;
            this.debugRadio.Click += new System.EventHandler(this.Radio_Click);
            // 
            // warnRadio
            // 
            this.warnRadio.AutoSize = true;
            this.warnRadio.Location = new System.Drawing.Point(12, 58);
            this.warnRadio.Name = "warnRadio";
            this.warnRadio.Size = new System.Drawing.Size(51, 17);
            this.warnRadio.TabIndex = 2;
            this.warnRadio.TabStop = true;
            this.warnRadio.Tag = "WARN";
            this.warnRadio.Text = "Warn";
            this.warnRadio.UseVisualStyleBackColor = true;
            this.warnRadio.Click += new System.EventHandler(this.Radio_Click);
            // 
            // errorRadio
            // 
            this.errorRadio.AutoSize = true;
            this.errorRadio.Location = new System.Drawing.Point(12, 81);
            this.errorRadio.Name = "errorRadio";
            this.errorRadio.Size = new System.Drawing.Size(47, 17);
            this.errorRadio.TabIndex = 3;
            this.errorRadio.TabStop = true;
            this.errorRadio.Tag = "ERROR";
            this.errorRadio.Text = "Error";
            this.errorRadio.UseVisualStyleBackColor = true;
            this.errorRadio.Click += new System.EventHandler(this.Radio_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(12, 104);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 4;
            this.cancelBtn.TabStop = false;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // acceptBtn
            // 
            this.acceptBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.acceptBtn.Location = new System.Drawing.Point(93, 104);
            this.acceptBtn.Name = "acceptBtn";
            this.acceptBtn.Size = new System.Drawing.Size(75, 23);
            this.acceptBtn.TabIndex = 5;
            this.acceptBtn.TabStop = false;
            this.acceptBtn.Text = "Accept";
            this.acceptBtn.UseVisualStyleBackColor = true;
            this.acceptBtn.Click += new System.EventHandler(this.Accept_Click);
            // 
            // SelectLogLevelForm
            // 
            this.AcceptButton = this.acceptBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(176, 140);
            this.Controls.Add(this.acceptBtn);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.errorRadio);
            this.Controls.Add(this.warnRadio);
            this.Controls.Add(this.debugRadio);
            this.Controls.Add(this.infoRadio);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectLogLevelForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select Logging Level";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SelectLogLevelForm_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SelectLogLevelForm_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton infoRadio;
        private System.Windows.Forms.RadioButton debugRadio;
        private System.Windows.Forms.RadioButton warnRadio;
        private System.Windows.Forms.RadioButton errorRadio;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button acceptBtn;
    }
}