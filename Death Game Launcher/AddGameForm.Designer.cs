namespace Death_Game_Launcher
{
    partial class AddGameForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.removeButton = new System.Windows.Forms.Button();
            this.browseButton = new System.Windows.Forms.Button();
            this.steamCheckBox = new System.Windows.Forms.CheckBox();
            this.pathTextBox = new System.Windows.Forms.TextBox();
            this.pathLabel = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.addButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.removeButton);
            this.groupBox1.Controls.Add(this.browseButton);
            this.groupBox1.Controls.Add(this.steamCheckBox);
            this.groupBox1.Controls.Add(this.pathTextBox);
            this.groupBox1.Controls.Add(this.pathLabel);
            this.groupBox1.Controls.Add(this.nameTextBox);
            this.groupBox1.Controls.Add(this.nameLabel);
            this.groupBox1.Location = new System.Drawing.Point(548, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(240, 130);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // removeButton
            // 
            this.removeButton.Location = new System.Drawing.Point(162, 102);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(75, 23);
            this.removeButton.TabIndex = 3;
            this.removeButton.Text = "Remove";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.Remove_Click);
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(213, 76);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(24, 20);
            this.browseButton.TabIndex = 1;
            this.browseButton.Text = "...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // steamCheckBox
            // 
            this.steamCheckBox.AutoSize = true;
            this.steamCheckBox.Location = new System.Drawing.Point(7, 102);
            this.steamCheckBox.Name = "steamCheckBox";
            this.steamCheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.steamCheckBox.Size = new System.Drawing.Size(95, 17);
            this.steamCheckBox.TabIndex = 4;
            this.steamCheckBox.Text = "Steam Launch";
            this.steamCheckBox.UseVisualStyleBackColor = true;
            // 
            // pathTextBox
            // 
            this.pathTextBox.Location = new System.Drawing.Point(7, 76);
            this.pathTextBox.Name = "pathTextBox";
            this.pathTextBox.Size = new System.Drawing.Size(200, 20);
            this.pathTextBox.TabIndex = 3;
            // 
            // pathLabel
            // 
            this.pathLabel.AutoSize = true;
            this.pathLabel.Location = new System.Drawing.Point(7, 60);
            this.pathLabel.Name = "pathLabel";
            this.pathLabel.Size = new System.Drawing.Size(68, 13);
            this.pathLabel.TabIndex = 2;
            this.pathLabel.Text = "Launch Path";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(7, 37);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(200, 20);
            this.nameTextBox.TabIndex = 1;
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(7, 20);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(35, 13);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "Name";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(183, 423);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "Confirm";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.Confirm_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(102, 423);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(246, 405);
            this.panel1.TabIndex = 3;
            // 
            // addButton
            // 
            this.addButton.Location = new System.Drawing.Point(12, 424);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(75, 23);
            this.addButton.TabIndex = 4;
            this.addButton.Text = "Add Game";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.AddGame_Click);
            // 
            // AddGameForm
            // 
            this.AcceptButton = this.okButton;
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(800, 459);
            this.ControlBox = false;
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddGameForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AddGameForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AddGameForm_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AddGameForm_KeyDown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox pathTextBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.CheckBox steamCheckBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button addButton;
    }
}