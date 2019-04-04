namespace Death_Game_Launcher
{
    partial class ShortcutSettings
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
            this.label1 = new System.Windows.Forms.Label();
            this.launchPathLabel = new System.Windows.Forms.Label();
            this.steamCheckBox = new System.Windows.Forms.CheckBox();
            this.shortcutCheckBox = new System.Windows.Forms.CheckBox();
            this.nameBox = new System.Windows.Forms.TextBox();
            this.pathBox = new System.Windows.Forms.TextBox();
            this.confirmBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.settingsAppCheckbox = new System.Windows.Forms.CheckBox();
            this.selectAppButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name: ";
            // 
            // launchPathLabel
            // 
            this.launchPathLabel.AutoSize = true;
            this.launchPathLabel.Location = new System.Drawing.Point(13, 39);
            this.launchPathLabel.Name = "launchPathLabel";
            this.launchPathLabel.Size = new System.Drawing.Size(32, 13);
            this.launchPathLabel.TabIndex = 1;
            this.launchPathLabel.Text = "Path:";
            this.launchPathLabel.MouseLeave += new System.EventHandler(this.LaunchPathLabel_MouseLeave);
            this.launchPathLabel.MouseHover += new System.EventHandler(this.LaunchPathLabel_MouseHover);
            // 
            // steamCheckBox
            // 
            this.steamCheckBox.AutoSize = true;
            this.steamCheckBox.Location = new System.Drawing.Point(12, 62);
            this.steamCheckBox.Name = "steamCheckBox";
            this.steamCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.steamCheckBox.Size = new System.Drawing.Size(101, 17);
            this.steamCheckBox.TabIndex = 5;
            this.steamCheckBox.Text = "?Steam Launch";
            this.steamCheckBox.UseVisualStyleBackColor = true;
            this.steamCheckBox.CheckedChanged += new System.EventHandler(this.SteamCheckBox_CheckedChanged);
            this.steamCheckBox.MouseLeave += new System.EventHandler(this.SteamCheckBox_MouseLeave);
            this.steamCheckBox.MouseHover += new System.EventHandler(this.SteamCheckBox_MouseHover);
            // 
            // shortcutCheckBox
            // 
            this.shortcutCheckBox.AutoSize = true;
            this.shortcutCheckBox.Location = new System.Drawing.Point(12, 85);
            this.shortcutCheckBox.Name = "shortcutCheckBox";
            this.shortcutCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.shortcutCheckBox.Size = new System.Drawing.Size(151, 17);
            this.shortcutCheckBox.TabIndex = 6;
            this.shortcutCheckBox.Text = "?Use Shortcut For Launch";
            this.shortcutCheckBox.UseVisualStyleBackColor = true;
            this.shortcutCheckBox.MouseLeave += new System.EventHandler(this.ShortcutCheckBox_MouseLeave);
            this.shortcutCheckBox.MouseHover += new System.EventHandler(this.ShortcutCheckBox_MouseHover);
            // 
            // nameBox
            // 
            this.nameBox.Location = new System.Drawing.Point(60, 10);
            this.nameBox.Name = "nameBox";
            this.nameBox.Size = new System.Drawing.Size(220, 20);
            this.nameBox.TabIndex = 7;
            // 
            // pathBox
            // 
            this.pathBox.Location = new System.Drawing.Point(51, 36);
            this.pathBox.Name = "pathBox";
            this.pathBox.Size = new System.Drawing.Size(229, 20);
            this.pathBox.TabIndex = 8;
            // 
            // confirmBtn
            // 
            this.confirmBtn.Location = new System.Drawing.Point(205, 133);
            this.confirmBtn.Name = "confirmBtn";
            this.confirmBtn.Size = new System.Drawing.Size(75, 23);
            this.confirmBtn.TabIndex = 9;
            this.confirmBtn.Text = "Confirm";
            this.confirmBtn.UseVisualStyleBackColor = true;
            this.confirmBtn.Click += new System.EventHandler(this.Confirm_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(124, 133);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 10;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(12, 133);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(75, 23);
            this.deleteButton.TabIndex = 11;
            this.deleteButton.Text = "Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.Delete_Click);
            // 
            // settingsAppCheckbox
            // 
            this.settingsAppCheckbox.AutoSize = true;
            this.settingsAppCheckbox.Location = new System.Drawing.Point(12, 108);
            this.settingsAppCheckbox.Name = "settingsAppCheckbox";
            this.settingsAppCheckbox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.settingsAppCheckbox.Size = new System.Drawing.Size(111, 17);
            this.settingsAppCheckbox.TabIndex = 12;
            this.settingsAppCheckbox.Text = "?Use settings app";
            this.settingsAppCheckbox.UseVisualStyleBackColor = true;
            this.settingsAppCheckbox.CheckedChanged += new System.EventHandler(this.SettingsAppCheckbox_CheckedChanged);
            this.settingsAppCheckbox.MouseLeave += new System.EventHandler(this.SettingsAppCheckBox_MouseLeave);
            this.settingsAppCheckbox.MouseHover += new System.EventHandler(this.SettingsAppCheckBox_MouseHover);
            // 
            // selectAppButton
            // 
            this.selectAppButton.Location = new System.Drawing.Point(129, 104);
            this.selectAppButton.Name = "selectAppButton";
            this.selectAppButton.Size = new System.Drawing.Size(75, 23);
            this.selectAppButton.TabIndex = 13;
            this.selectAppButton.Text = "Select App";
            this.selectAppButton.UseVisualStyleBackColor = true;
            this.selectAppButton.Click += new System.EventHandler(this.SelectAppButton_Click);
            // 
            // ShortcutSettings
            // 
            this.AcceptButton = this.confirmBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cancelBtn;
            this.ClientSize = new System.Drawing.Size(286, 162);
            this.Controls.Add(this.selectAppButton);
            this.Controls.Add(this.settingsAppCheckbox);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.confirmBtn);
            this.Controls.Add(this.pathBox);
            this.Controls.Add(this.nameBox);
            this.Controls.Add(this.shortcutCheckBox);
            this.Controls.Add(this.steamCheckBox);
            this.Controls.Add(this.launchPathLabel);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ShortcutSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ShortcutSettings_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label launchPathLabel;
        private System.Windows.Forms.CheckBox steamCheckBox;
        private System.Windows.Forms.CheckBox shortcutCheckBox;
        private System.Windows.Forms.TextBox nameBox;
        private System.Windows.Forms.TextBox pathBox;
        private System.Windows.Forms.Button confirmBtn;
        private System.Windows.Forms.Button cancelBtn;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.CheckBox settingsAppCheckbox;
        private System.Windows.Forms.Button selectAppButton;
    }
}