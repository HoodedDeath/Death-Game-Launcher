﻿using System;
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
    public partial class AddGameForm : Form
    {
        private List<Game> games = new List<Game>();
        struct Game
        {
            public string name;
            public string path;
            public bool isSteam;
        }
        public AddGameForm()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(GameDrag);
            this.DragDrop += new DragEventHandler(GameDrop);
            AddGroup();
        }
        private void GameDrag(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
        private void GameDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files) AddGame(file); // MessageBox.Show(file); // Console.WriteLine(file);
        }

        private void AddGame(string path)
        {

        }
        private void AddGroup()
        {
            panel1.AutoScrollPosition = new Point(0, 0);
            AddGameGrouping g = new AddGameGrouping();
            g.Location = new Point(location[0], location[1] += 136);
            panel1.Controls.Add(g.Group);
            count++;
            if (count >= 3 && panel1.Size.Width != 265) panel1.Size = new Size(265, panel1.Size.Height);
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.ShowDialog();
            string path = d.FileName;
            pathTextBox.Text = path;
            MessageBox.Show(path);
            /*if (nameTextBox.Text == null || nameTextBox.Text == "")
            {
                string[] splitpath = path.Split('\\');
                string name = splitpath[splitpath.Length - 1].Split('.')[0];
                nameTextBox.Text = name;
                MessageBox.Show(name);
            }*/
            if (nameTextBox.Text == null || nameTextBox.Text == "")
                nameTextBox.Text = path.Split('\\')[path.Split('\\').Length - 1].Split('.')[0];
        }

        private void AddGameForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Cancel_Click(sender, null);
        }

        private bool exit = false;
        private void Confirm_Click(object sender, EventArgs e)
        {
            foreach (GroupBox g in panel1.Controls)
            {
                //CheckBox check = (CheckBox)(g.Controls.Find("steamCheckBox", true)[0]);
                CheckBox check = g.Controls.Find("steamCheckBox", true).FirstOrDefault() as CheckBox;
                games.Add(new Game()
                {
                    //name = g.Controls.Find("nameTextBox", true)[0].Text,
                    name = (g.Controls.Find("nameTextBox", true).FirstOrDefault() as TextBox).Text,
                    //path = g.Controls.Find("pathTextBox", true)[0].Text,
                    path = (g.Control.Find("pathTextBox", true).FirstOrDefault() as TextBox).Text,
                    //isSteam = ((CheckBox)(g.Controls.Find("steamCheckBox", true)[0])).Checked
                    isSteam = (g.Controls.Find("steamCheckBox", true).FirstOrDefault() as CheckBox).Checked
                });
            }
            this.DialogResult = DialogResult.OK;
            exit = true;
            Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.exit = false;
            Close();
        }

        private void AddGameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (exit)
                e.Cancel = false;
            else if (MessageBox.Show("Cancelling will lose list of games to add", "Confirm Cancel", MessageBoxButtons.YesNo) == DialogResult.Yes)
                e.Cancel = false;
            else
                e.Cancel = true;
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            //Does nothing yet. Removal from list will be added soon
        }

        private int[] location = new int[] { 3, -133 };
        private int count = 0;
        private void AddGame_Click(object sender, EventArgs e)
        {
            Control[] groupbox = panel1.Controls.Find("GroupBox", false);
            TextBox textbox = groupbox[groupbox.Length - 1].Controls.Find("pathTextBox", true).FirstOrDefault() as TextBox;
            //TextBox box = (TextBox)textbox[0];
            bool useless = false;
            string lastpath = textbox.Text; // ((TextBox)panel1.Controls.Find("GroupBox", false)[panel1.Controls.Find("GroupBox", false).Length - 1].Controls.Find("pathTextBox", true)[0]).Text;
            bool lastempty = !(lastpath == null || lastpath == "");

            if (lastempty)
            {
                panel1.AutoScrollPosition = new Point(0, 0);
                AddGameGrouping g = new AddGameGrouping();
                g.Location = new Point(location[0], location[1] += 136);
                panel1.Controls.Add(g.Group);
                count++;
                if (count >= 3 && panel1.Size.Width != 265) panel1.Size = new Size(265, panel1.Size.Height);
            }
        }
    }

    class AddGameGrouping
    {
        private GroupBox groupBox1;
        private Label pathLabel;
        private TextBox nameTextBox;
        private Label nameLabel;
        private TextBox pathTextBox;
        private Button browseButton;
        private CheckBox steamCheckBox;
        private Button removeButton;
        private string path = "";
        private string name = "";
        private bool steamLaunch = false;

        public AddGameGrouping()
        {
            this.groupBox1 = new GroupBox();
            this.browseButton = new Button();
            this.steamCheckBox = new CheckBox();
            this.pathTextBox = new TextBox();
            this.pathLabel = new Label();
            this.nameTextBox = new TextBox();
            this.nameLabel = new Label();
            this.removeButton = new Button();
            this.groupBox1.SuspendLayout();
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
            //this.groupBox1.Location = new Point(548, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(240, 130);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            // 
            // browseButton
            // 
            this.browseButton.Location = new Point(213, 76);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new Size(24, 20);
            this.browseButton.TabIndex = 1;
            this.browseButton.Text = "...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new EventHandler(this.BrowseButton_Click);
            // 
            // steamCheckBox
            // 
            this.steamCheckBox.AutoSize = true;
            this.steamCheckBox.Location = new Point(7, 102);
            this.steamCheckBox.Name = "steamCheckBox";
            this.steamCheckBox.RightToLeft = RightToLeft.Yes;
            this.steamCheckBox.Size = new Size(95, 17);
            this.steamCheckBox.TabIndex = 4;
            this.steamCheckBox.Text = "Steam Launch";
            this.steamCheckBox.UseVisualStyleBackColor = true;
            // 
            // pathTextBox
            // 
            this.pathTextBox.Location = new Point(7, 76);
            this.pathTextBox.Name = "pathTextBox";
            this.pathTextBox.Size = new Size(200, 20);
            this.pathTextBox.TabIndex = 3;
            // 
            // pathLabel
            // 
            this.pathLabel.AutoSize = true;
            this.pathLabel.Location = new Point(7, 60);
            this.pathLabel.Name = "pathLabel";
            this.pathLabel.Size = new Size(68, 13);
            this.pathLabel.TabIndex = 2;
            this.pathLabel.Text = "Launch Path";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new Point(7, 37);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new Size(200, 20);
            this.nameTextBox.TabIndex = 1;
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new Point(7, 20);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new Size(35, 13);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "Name";
            // 
            // removeButton
            // 
            this.removeButton.Location = new Point(162, 102);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new Size(75, 23);
            this.removeButton.TabIndex = 3;
            this.removeButton.Text = "Remove";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new EventHandler(this.Remove_Click);

            this.groupBox1.ResumeLayout();
        }
        public Point Location
        {
            get { return this.groupBox1.Location; }
            set { this.groupBox1.Location = value; }
        }
        public GroupBox Group { get { return this.groupBox1; } }
        public string Path { get { return this.pathTextBox.Text; } }
        public string Name { get { return this.nameTextBox.Text; } }
        public bool IsSteamLaunch { get { return this.steamCheckBox.Checked; } }

        private void Remove_Click(object sender, EventArgs e)
        {
            //Does nothing yet. Removal from list will be added soon
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.ShowDialog();
            string path = d.FileName;
            pathTextBox.Text = path;
            if (nameTextBox.Text == null || nameTextBox.Text == "")
                nameTextBox.Text = path.Split('\\')[path.Split('\\').Length - 1].Split('.')[0];
        }

    }
}