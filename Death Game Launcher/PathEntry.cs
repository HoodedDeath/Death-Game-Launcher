using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Death_Game_Launcher
{
    public partial class PathEntry : Form
    {
        public PathEntry()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Controls.Add(new PathEntryGroup(0, 0).Group);
        }
    }

    class PathEntryGroup
    {
        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.TextBox pathBox;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button okButton;

        public PathEntryGroup(int posX, int posY)
        {
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.deleteButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.pathBox = new System.Windows.Forms.TextBox();
            this.browseButton = new System.Windows.Forms.Button();
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.deleteButton);
            this.groupBox.Controls.Add(this.okButton);
            this.groupBox.Controls.Add(this.pathBox);
            this.groupBox.Controls.Add(this.browseButton);
            this.groupBox.Location = new System.Drawing.Point(posX, posY);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(313, 79);
            this.groupBox.TabIndex = 0;
            this.groupBox.TabStop = false;
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(100, 46);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(75, 23);
            this.deleteButton.TabIndex = 2;
            this.deleteButton.Text = "Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Enabled = false;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(262, 46);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(45, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.Ok_Click);
            // 
            // pathBox
            // 
            this.pathBox.Location = new System.Drawing.Point(7, 20);
            this.pathBox.Name = "pathBox";
            this.pathBox.Size = new System.Drawing.Size(300, 20);
            this.pathBox.TabIndex = 0;
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(181, 46);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(75, 23);
            this.browseButton.TabIndex = 1;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = true;
        }

        public GroupBox Group { get { return this.groupBox; } }

        private void Ok_Click(object sender, EventArgs e)
        {
            if (pathBox.Text.ToString() != null && pathBox.Text.ToString() != "")
                deleteButton.Enabled = true;
        }
    }
}
