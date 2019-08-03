using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Death_Game_Launcher;

namespace Death_Game_Launcher
{
    public partial class ModificationsForm : Form
    {
        //Used to get the final list of changes made to games
        public List<Manifest[]> Modifs { get; set; } = new List<Manifest[]>();

        //Default height value for the panel to display the ModifGroup at, allows for use of 'height += 151'
        private const int def_height = -148;
        //Current possition of the item last added to the panel, currently set to the default state
        private int[] pos = new int[] { 3, -148 };
        //Used by 'Confirm_Click' as a flag to signal that an exit is wanted
        bool exit = false;

        public ModificationsForm(List<Manifest[]> mods)
        {
            Form1._log.Debug("Calling ModificationsForm.InitializeComponent ...");
            InitializeComponent();
            Form1._log.Debug("Done initializing.");
            Modifs = mods;
            Form1._log.Debug("Eliminating duplicates ...");
            //Removes any duplicates
            EliminateDuplicates();
            Form1._log.Debug("Listing modifications ...");
            //Displays the list of changes
            List();
        }
        //Used to eliminate duplicate items
        private void EliminateDuplicates()
        {
            //Temporary List for keeping items that are not duplicates
            List<Manifest[]> temp = new List<Manifest[]>();
            //Convert _gameMods List to an array for simplicity
            Manifest[][] m = this.Modifs.ToArray();
            //Loop through each item in 'm'
            for (int i = 0; i < m.Length; i++)
            {
                //Copy 'temp' List into an array to loop through
                Manifest[][] jm = temp.ToArray();
                //Used to see if there was a duplicate
                bool t = false;
                //Loop through each item in 'jm'
                for (int j = 0; j < jm.Length; j++)
                {
                    //If a duplicate was found, break this loop, we don't need to keep checking
                    if (t) break;
                    //Manually compare the individual Manifest classes inside 'm' and 'jm' since testing revealed 'm[i] == jm[j]' never worked
                    t = t || (m[i][0] == jm[j][0] && m[i][1] == jm[j][1]);
                }
                //If the current item ('m[i]') was not found in the 'temp' List, add it in
                if (!t)
                    temp.Add(m[i]);
            }
            //Overwrite 'Modifs' with the 'temp' List, which has no duplicates
            this.Modifs = temp;
        }
        //Displays the changes found
        private void List()
        {
            //Copies 'Modifs' List to array for simplicity
            Manifest[][] m = this.Modifs.ToArray<Manifest[]>();
            //Resets the size of the panel to fit the size of the GroupBoxes
            panel1.Size = new Size(446, panel1.Size.Height);
            //Resets the current height in 'pos'
            pos[1] = def_height;
            //Clears the panel of any items, should be unnecessary, but is a precaution
            panel1.Controls.Clear();
            //If there are no changes to display, don't bother trying
            if (m == null || m.Length == 0) return;
            //Resets the AutoScrollPosition of the panel to prevent any funky spacing
            panel1.AutoScrollPosition = new Point(0, 0);
            //Loops through each item to be displayed
            for (int i = 0; i < m.Length; i++)
            {
                //Create a new 'ModifGroup' item, give it the information of the changes and the 'this' object to use for calls, then set its height location to the current height location plus 151 (height of the GroupBox plus padding), then add the group to the Panel
                panel1.Controls.Add(new ModifGroup(m[i][0], m[i][1], this)
                {
                    Location = new Point(pos[0], pos[1] += 151)
                });
                //If there are more than 3 items listed, we need a scroll bar. AutoScroll will display it, but this compensates for the extra width by adding 14 pixels to the width of the panel, avoiding the horizontal scroll bar 
                if (i > 3) panel1.Size = new Size(460, panel1.Size.Height);
            }
        }
        //Called from within the ModifGroup class when the 'Remove' button is clicked
        public void Remove(Manifest old)
        {
            //Copies 'Modifs' to array for simplicity
            Manifest[][] m = this.Modifs.ToArray<Manifest[]>();
            //Loops through each change listed, manually checking for the correct one to remove. Easier (in my opinion) than keeping a variable for the items that the user is able to change within each GroupBox
            for (int i = 0; i < m.Length; i++)
            {
                //If the old details of the game (the ones the user can't change) don't match one of the ones in the list, skip to the beginning of the loop
                if (!(old == m[i][0] || old.Equals(m[i][0]))) continue;
                //Shows 'Removed." in a pop-up in the game was able to be removed, 'Not removed.' if it failed
                MessageBox.Show(this.Modifs.Remove(m[i]) ? "Removed." : "Not removed.");
                //Break out of loop, since we've found and removed the needed item
                break;
            }
            //Refresh the updated list
            List();
        }
        //Called when the user presses any key on the keyboard
        private void ModificationsForm_KeyDown(object sender, KeyEventArgs e)
        {
            //If the user presses Escape on keyboard, act as if the 'Cancel' button was pressed
            if (e.KeyCode == Keys.Escape) Cancel_Click(sender, null);
        }
        //Called whe the confirm button pressed
        private void Confirm_Click(object sender, EventArgs e)
        {
            //Runs through each GroupBox in the Panel
            foreach (ModifGroup g in panel1.Controls)
            {
                //Creates a new array of Manifest items to represent the change to this game and adds it to 'Modifs' List
                this.Modifs.Add(new Manifest[]
                {
                    new Manifest()
                    {
                        name = g.NameOld,
                        path = g.PathOld,
                        steamLaunch = g.SteamOld,
                        useShortcut = g.ShortcutOld
                    },
                    new Manifest()
                    {
                        name = g.NameNew,
                        path = g.PathNew,
                        steamLaunch = g.SteamNew,
                        useShortcut = g.ShortcutNew,
                    }
                });
            }
            this.DialogResult = DialogResult.OK;
            //An exit is wanted without prompt
            exit = true;
            //Closes the Form, triggering the Form.FormClosing event
            Close();
        }
        //'Cancel' button clicked or Escape key pressed
        private void Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            //Exit is not wanted
            exit = false;
            //Closes the Form, triggering the Form.FormClosing event
            Close();
        }
        //Handles Form.FormClosing event
        private void ClosingForm (object sender, FormClosingEventArgs e)
        {
            //If an exit is wanted without prompt (user clicked 'Confirm'), do not cancel
            if (exit)
                e.Cancel = false;
            //Otherwise, prompt the user if they want to exit and discard any changes made
            else
                e.Cancel = !(MessageBox.Show("Cancelling will revert any changes made. Are you sure?", "Confirm Cancel", MessageBoxButtons.YesNo) == DialogResult.Yes);
        }
    }

    class ModifGroup : GroupBox
    {
        private readonly Label label1;
        private readonly Label label2;
        private readonly Label label3;
        private readonly Label label4;
        private readonly Label label5;
        private readonly Label label6;
        private readonly Label label7;
        private readonly Label label8;
        private readonly Label label9;
        private readonly Label label10;
        private readonly TextBox nameOldBox;
        private readonly TextBox nameNewBox;
        private readonly TextBox pathOldBox;
        private readonly TextBox pathNewBox;
        private readonly CheckBox steamLaunchOldBox;
        private readonly CheckBox steamLaunchNewBox;
        private readonly CheckBox shortcutLaunchOldBox;
        private readonly CheckBox shortcutLaunchNewBox;
        private readonly Button browseBtn;
        private readonly Button removeBtn;
        //The Form containing this object. Used for calling ModificationsForm.Remove
        private readonly ModificationsForm _parent;

        public string NameOld { get { return this.nameOldBox.Text; } }
        public string NameNew { get { return this.nameNewBox.Text; } }
        public string PathOld { get { return this.pathOldBox.Text; } }
        public string PathNew { get { return this.pathNewBox.Text; } }
        public bool SteamOld { get { return this.steamLaunchOldBox.Checked; } }
        public bool SteamNew { get { return this.steamLaunchNewBox.Checked; } }
        public bool ShortcutOld { get { return this.shortcutLaunchOldBox.Checked; } }
        public bool ShortcutNew { get { return this.shortcutLaunchNewBox.Checked; } }

        public ModifGroup(Manifest oldMan, Manifest newMan, ModificationsForm parent)
        {
            this._parent = parent;
            this.label1 = new Label()
            {
                AutoSize = true,
                Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0))),
                Location = new Point(6, 16),
                Name = "label1",
                Size = new Size(32, 16),
                TabIndex = 8,
                Text = "Old"
            };
            this.label2 = new Label()
            {
                AutoSize = true,
                Location = new Point(23, 34),
                Name = "label2",
                Size = new Size(38, 13),
                TabIndex = 0,
                Text = "Name:"
            };
            this.label3 = new Label()
            {
                AutoSize = true,
                Location = new Point(224, 34),
                Name = "label3",
                Size = new Size(48, 13),
                TabIndex = 2,
                Text = "Path/ID:"
            };
            this.label3.MouseHover += new EventHandler(this.Path_MouseHover);
            this.label3.MouseLeave += new EventHandler(this.Global_MouseLeave);
            this.label4 = new Label()
            {
                AutoSize = true,
                Location = new Point(24, 57),
                Name = "label4",
                Size = new Size(76, 13),
                TabIndex = 4,
                Text = "Steam Launch"
            };
            this.label4.MouseHover += new EventHandler(this.SteamLaunch_MouseHover);
            this.label4.MouseLeave += new EventHandler(this.Global_MouseLeave);
            this.label5 = new Label()
            {
                AutoSize = true,
                Location = new Point(137, 57),
                Name = "label5",
                Size = new Size(86, 13),
                TabIndex = 6,
                Text = "Shortcut Launch"
            };
            this.label5.MouseHover += new EventHandler(this.ShortcutLaunch_MouseHover);
            this.label5.MouseLeave += new EventHandler(this.Global_MouseLeave);
            this.label6 = new Label()
            {
                AutoSize = true,
                Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0))),
                Location = new Point(6, 75),
                Name = "label6",
                Size = new Size(38, 16),
                TabIndex = 17,
                Text = "New"
            };
            this.label7 = new Label()
            {
                AutoSize = true,
                Location = new Point(24, 93),
                Name = "label7",
                Size = new Size(38, 13),
                TabIndex = 9,
                Text = "Name:"
            };
            this.label8 = new Label()
            {
                AutoSize = true,
                Location = new Point(224, 93),
                Name = "label8",
                Size = new Size(48, 13),
                TabIndex = 11,
                Text = "Path/ID:"
            };
            this.label8.MouseHover += new EventHandler(this.Path_MouseHover);
            this.label8.MouseLeave += new EventHandler(this.Global_MouseLeave);
            this.label9 = new Label()
            {
                AutoSize = true,
                Location = new Point(24, 116),
                Name = "label9",
                Size = new Size(76, 13),
                TabIndex = 13,
                Text = "Steam Launch"
            };
            this.label9.MouseHover += new EventHandler(this.SteamLaunch_MouseHover);
            this.label9.MouseLeave += new EventHandler(this.Global_MouseLeave);
            this.label10 = new Label()
            {
                AutoSize = true,
                Location = new Point(137, 116),
                Name = "label10",
                Size = new Size(86, 13),
                Text = "Shortcut Launch"
            };
            this.label10.MouseHover += new EventHandler(this.ShortcutLaunch_MouseHover);
            this.label10.MouseLeave += new EventHandler(this.Global_MouseLeave);
            this.browseBtn = new Button()
            {
                Location = new Point(332, 116),
                Name = "browseBtn",
                Size = new Size(25, 23),
                TabIndex = 2,
                Text = "...",
                UseVisualStyleBackColor = true
            };
            this.browseBtn.Click += new EventHandler(this.Browse_Click);
            this.browseBtn.MouseHover += new EventHandler(this.Browse_MouseHover);
            this.browseBtn.MouseLeave += new EventHandler(this.Global_MouseLeave);
            this.removeBtn = new Button()
            {
                Location = new Point(363, 116),
                Name = "removeBtn",
                Size = new Size(65, 23),
                TabIndex = 2,
                Text = "Remove",
                UseVisualStyleBackColor = true
            };
            this.removeBtn.Click += new EventHandler(this.Remove_Click);
            this.removeBtn.MouseHover += new EventHandler(this.Remove_MouseHover);
            this.removeBtn.MouseLeave += new EventHandler(this.Global_MouseLeave);
            this.steamLaunchOldBox = new CheckBox()
            {
                AutoCheck = false,
                AutoSize = true,
                Checked = oldMan.steamLaunch,
                Enabled = false,
                Location = new Point(106, 57),
                Name = "steamLaunchOldBox",
                Size = new Size(15, 14),
                TabIndex = 5,
                TabStop = false,
                UseVisualStyleBackColor = true
            };
            this.steamLaunchOldBox.MouseHover += new EventHandler(this.SteamLaunch_MouseHover);
            this.steamLaunchOldBox.MouseLeave += new EventHandler(this.Global_MouseLeave);
            this.steamLaunchNewBox = new CheckBox()
            {
                AutoSize = true,
                Checked = newMan.steamLaunch,
                Location = new Point(106, 116),
                Name = "steamLaunchNewBox",
                Size = new Size(15, 14),
                TabIndex = 14,
                UseVisualStyleBackColor = true
            };
            this.steamLaunchNewBox.Click += new EventHandler(this.SteamCheck_Click);
            this.steamLaunchNewBox.MouseHover += new EventHandler(this.SteamLaunch_MouseHover);
            this.steamLaunchNewBox.MouseLeave += new EventHandler(this.Global_MouseLeave);
            this.shortcutLaunchOldBox = new CheckBox()
            {
                AutoCheck = false,
                AutoSize = true,
                Checked = oldMan.useShortcut,
                Enabled = false,
                Location = new Point(229, 57),
                Name = "shortcutLaunchOldBox",
                Size = new Size(15, 14),
                TabIndex = 7,
                TabStop = false,
                UseVisualStyleBackColor = true
            };
            this.shortcutLaunchOldBox.MouseHover += new EventHandler(this.ShortcutLaunch_MouseHover);
            this.shortcutLaunchOldBox.MouseLeave += new EventHandler(this.Global_MouseLeave);
            this.shortcutLaunchNewBox = new CheckBox()
            {
                AutoSize = true,
                Checked = newMan.useShortcut,
                Location = new Point(229, 116),
                Name = "shortcutLaunchNewBox",
                Size = new Size(15, 14),
                TabIndex = 16,
                UseVisualStyleBackColor = true
            };
            this.shortcutLaunchNewBox.Click += new EventHandler(this.ShortcutCheck_Click);
            this.shortcutLaunchNewBox.MouseHover += new EventHandler(this.ShortcutLaunch_MouseHover);
            this.shortcutLaunchNewBox.MouseLeave += new EventHandler(this.Global_MouseLeave);
            this.pathOldBox = new TextBox()
            {
                Location = new Point(278, 31),
                Name = "pathOldBox",
                ReadOnly = true,
                Size = new Size(150, 20),
                TabIndex = 3,
                TabStop = false,
                Text = oldMan.path,
                WordWrap = false
            };
            this.pathNewBox = new TextBox()
            {
                Location = new Point(278, 90),
                Name = "pathNewBox",
                Size = new Size(150, 20),
                TabIndex = 12,
                Text = newMan.path,
                WordWrap = false
            };
            this.nameOldBox = new TextBox()
            {
                Location = new Point(68, 31),
                Name = "nameOldBox",
                ReadOnly = true,
                Size = new Size(150, 20),
                TabIndex = 1,
                TabStop = false,
                Text = oldMan.name,
                WordWrap = false
            };
            this.nameNewBox = new TextBox()
            {
                Location = new Point(68, 90),
                Name = "nameNewBox",
                Size = new Size(150, 20),
                TabIndex = 10,
                Text = newMan.name,
                WordWrap = false
            };
            
            this.Controls.Add(this.browseBtn);
            this.Controls.Add(this.removeBtn);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.shortcutLaunchNewBox);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.steamLaunchNewBox);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.pathNewBox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.nameNewBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.shortcutLaunchOldBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.steamLaunchOldBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.pathOldBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.nameOldBox);
            this.Controls.Add(this.label2);
            this.Name = "modifGroup";
            this.Size = new Size(440, 145);
            this.TabIndex = 1;
            this.TabStop = false;
        }
        //Un-checks the 'Shortcut Launch' CheckBox if the 'Steam Launch' CheckBox gets checked, as a shortcut launch with a Steam game is not needed
        private void SteamCheck_Click(object sender, EventArgs e)
        {
            if (this.steamLaunchNewBox.Checked)
                this.shortcutLaunchNewBox.Checked = false;
        }
        //If the 'Steam Launch' CheckBox is checked and the user checks the 'Shortcut Launch' CheckBox, alert them that is not needed
        private void ShortcutCheck_Click(object sender, EventArgs e)
        {
            if (this.steamLaunchNewBox.Checked && this.shortcutLaunchNewBox.Checked)
                MessageBox.Show("Shortcut launch is not needed for Steam game launches.");
        }
        //Browse for the executable for a local launch game
        private void Browse_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            DialogResult res = d.ShowDialog();
            if (res == DialogResult.OK)
                this.pathNewBox.Text = d.FileName;
        }
        //Removes the changes from the listing
        private void Remove_Click(object sender, EventArgs e)
        {
            //Calls ModificationForm.Remove and gives it a new Manifest with the data of the 'old' details of the game
            this._parent.Remove(new Manifest()
            {
                name = this.nameOldBox.Text,
                path = this.pathOldBox.Text,
                steamLaunch = this.steamLaunchOldBox.Checked,
                useShortcut = this.shortcutLaunchOldBox.Checked
            });
        }
        //Tooltip Methods for labels and such
        ToolTip tip = new ToolTip();
        private void Path_MouseHover(object sender, EventArgs e)
        {
            tip.Show("The path used to launch the game. If it is a Steam launch, the launch path should be set as the game's Steam ID number.", _parent, ((Control)sender).Location, 10000);
        }
        private void SteamLaunch_MouseHover(object sender, EventArgs e)
        {
            tip.Show("Whether or not the game should be launched through Steam", _parent, ((Control)sender).Location, 10000);
        }
        private void ShortcutLaunch_MouseHover(object sender, EventArgs e)
        {
            tip.Show("Whether or not the game should be launched by creating a shortcut. This may help if a local game is crashing while launching or not launching at all", _parent, ((Control)sender).Location, 10000);
        }
        private void Browse_MouseHover(object sender, EventArgs e)
        {
            tip.Show("Browse files", _parent, ((Control)sender).Location, 10000);
        }
        private void Remove_MouseHover(object sender, EventArgs e)
        {
            tip.Show("Remove modifications to this game", _parent, ((Control)sender).Location, 10000);
        }
        private void Global_MouseLeave(object sender, EventArgs e)
        {
            tip.Hide(_parent);
        }
    }
}
