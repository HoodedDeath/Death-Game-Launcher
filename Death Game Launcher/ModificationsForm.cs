//using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using Newtonsoft.Json;

namespace Death_Game_Launcher
{
    public partial class ModificationsForm : Form
    {
        //Used to get the final list of changes made to games
        public List<Manifest[]> Modifs { get; set; } = new List<Manifest[]>();
        //Registry path
        //private const string regpath = "HKEY_CURRENT_USER\\Software\\HoodedDeathApplications\\DeathGameLauncher";
        //List of game changes initially found
        private List<Manifest[]> _gameMods = new List<Manifest[]>();

        public ModificationsForm(List<Manifest[]> mods)
        {
            InitializeComponent();
            Modifs = mods;
            //Reads through the file of changes
            //Read();
            //Removes any duplicates
            EliminateDuplicates();
            //Displays the list of changes
            List();
        }

        //Reads through the changes file. Essentially identical to the modification section of Form1.ReadGameConfigs
        /*private void Read()
        {
            //Modify any games based on the Modifications file
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeathApplications", "DLG", "modifications.cfg");
                if (File.Exists(path))
                {
                    StreamReader sr = new StreamReader(File.OpenRead(path));
                    List<ModJson> modsJ = JsonConvert.DeserializeObject<List<ModJson>>(sr.ReadToEnd());
                    sr.Close();
                    sr.Dispose();
                    foreach (ModJson m in modsJ)
                    {
                        this._gameMods.Add(new Manifest[]
                        {
                            new Manifest()
                            {
                                name = m.Mod.OldName,
                                path = m.Mod.OldPath,
                                args = m.Mod.OldArgs,
                                steamLaunch = m.Mod.OldSteam,
                                useShortcut = m.Mod.OldShort,
                                useSettingsApp = m.Mod.OldUseSApp,
                                settingsApp = ParseSAppID(m.Mod.OldSApp)
                            },
                            new Manifest()
                            {
                                name = m.Mod.NewName,
                                path = m.Mod.NewPath,
                                args = m.Mod.NewArgs,
                                steamLaunch = m.Mod.NewSteam,
                                useShortcut = m.Mod.NewShort,
                                useSettingsApp = m.Mod.NewUseSApp,
                                settingsApp = ParseSAppID(m.Mod.NewSApp)
                            }
                        });
                    }
                }

                //Return value from the Modifications registry entry
                /*string[] ret = (string[])Registry.GetValue(regpath, "Modifications", null);
                //If ret is not empty
                if (ret != null && ret.Length > 0)
                {
                    //Temporary variable for reading game details
                    Manifest[] manifests = new Manifest[] { new Manifest(), new Manifest() };
                    //Just to signal if we're reading the old or new details
                    int i = 0;
                    foreach (string s in ret)
                    {
                        string[] arr = s.Split('"');
                        switch (arr[0].ToLower())
                        {
                            //Skips over opening bracket
                            case "{":
                                break;
                            //Increments i since ; signals the start of the new details
                            case ";":
                                i++;
                                break;
                            //Copies the current game's details to _gameMods and resets manifests and i
                            case "}":
                                _gameMods.Add(manifests);
                                manifests = new Manifest[] { new Manifest(), new Manifest() };
                                i = 0;
                                break;
                            //Reads through each line and adds details to manifests, i determining old details vs new details
                            default:
                                switch (arr[1].ToLower().Trim())
                                {
                                    case "name":
                                        manifests[i].name = arr[arr.Length - 2];
                                        break;
                                    case "path":
                                        manifests[i].path = arr[arr.Length - 2];
                                        break;
                                    case "steam":
                                        manifests[i].steamLaunch = bool.Parse(arr[arr.Length - 2]);
                                        break;
                                    case "shortcut":
                                        manifests[i].useShortcut = bool.Parse(arr[arr.Length - 2]);
                                        break;
                                }
                                break;
                        }
                    }
                }*//*
            }
            catch (Exception e) { Form1._log.Error(e, "Failed to load modifications list.");  }
            //Sorts _gameMods alphabetically based on the name given in the new details of each game
            _gameMods.Sort((x, y) => x[1].name.CompareTo(y[1].name));
        }*/
        //Decides which settings app is wanted based on the id string
        /*private SettingsApp ParseSAppID(string id)
        {
            //Read in the saved SettingsApps
            List<SettingsApp> apps = new List<SettingsApp>();
            StreamReader sr = new StreamReader(File.OpenRead(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeathApplications", "DLG", "sapps.cfg")));
            List<SAppJson> saj = JsonConvert.DeserializeObject<List<SAppJson>>(sr.ReadToEnd());
            sr.Close();
            sr.Dispose();
            foreach (SAppJson sa in saj)
                apps.Add(sa.SApp);
            //Loops through all SettingsApps to attempt to find the match to the given id string
            foreach (SettingsApp a in apps)
            {
                if (a.IDString() == id)
                    return a;
            }
            //Shouldn't reach this unless there was no SettingsApp matching the id string
            return null;
        }*/
        //Used to eliminate duplicate items
        private void EliminateDuplicates()
        {
            //Temporary List for keeping items that are not duplicates
            List<Manifest[]> temp = new List<Manifest[]>();
            //Convert _gameMods List to an array for simplicity
            Manifest[][] m = this._gameMods.ToArray();
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
            //Overwrite '_gameMods' with the 'temp' List, which has no duplicates
            this._gameMods = temp;
        }
        //Default height value for the panel to display the ModifGroup at, allows for use of 'height += 151'
        private const int def_height = -148;
        //Current possition of the item last added to the panel, currently set to the default state
        private int[] pos = new int[] { 3, -148 };
        //Displays the changes found
        private void List()
        {
            //Copies '_gameMods' List to array for simplicity
            Manifest[][] m = this._gameMods.ToArray<Manifest[]>();
            //Resets the size of the panel to fit the size of the GroupBoxes
            panel1.Size = new Size(446, panel1.Size.Height);
            //Resets the current height in 'pos'
            pos[1] = def_height;
            //Clears the panel of any items, should be unnecessary, but is a precoution
            panel1.Controls.Clear();
            //If there are no changes to display, don't bother trying
            if (m == null || m.Length == 0) return;
            //Resets the AutoScrollPosition of the panel to prevent any funky spacing
            panel1.AutoScrollPosition = new Point(0, 0);
            //Loops through each item to be displayed
            for (int i = 0; i < m.Length; i++)
            {
                //Create a new 'ModifGroup' item, give it the information of the changes and the 'this' object to use for calls, then set its height location to the current height location plus 151 (height of the GroupBox plus padding), then call ModifGroup.Group to get the GroupBox object to add to the Panel controls
                panel1.Controls.Add(new ModifGroup(m[i][0], m[i][1], this)
                {
                    Location = new Point(pos[0], pos[1] += 151)
                });
                //If there are more than 3 items listed, we need a scroll bar. AutoScroll will display it, but this compensates for the extra width by adding 14 pixels to the width of the panel, avoiding the need for the horizontal scroll bar 
                if (i > 3) panel1.Size = new Size(460, panel1.Size.Height);
            }
        }

        //Called from within the ModifGroup class when the 'Remove' button is clicked
        public void Remove(Manifest old)
        {
            //Copies '_gameMods' to array for simplicity
            Manifest[][] m = this._gameMods.ToArray<Manifest[]>();
            //Loops through each change listed, manually checking for the correct one to remove. Easier (in my opinion) than keeping a variable for the items that the user is able to change within each GroupBox
            for (int i = 0; i < m.Length; i++)
            {
                //If the 'old' details of the game (the ones the user can't change) match one of the ones in the list, remove that item
                if (old == m[i][0])
                {
                    //Shows "Removed." if the game was able to be removed, "Not removed." if it failed
                    MessageBox.Show(this._gameMods.Remove(m[i]) ? "Removed." : "Not removed.");
                    break;
                }
            }
            //Refresh the updated list
            List();
        }

        private void ModificationsForm_KeyDown(object sender, KeyEventArgs e)
        {
            //If the user presses Escape on keyboard, act as if the 'Cancel' button was pressed
            if (e.KeyCode == Keys.Escape)
                Cancel_Click(sender, null);
        }

        //Used by 'Confirm_Click' as a flag to signal that an exit is wanted
        bool exit = false;
        //'Confirm' button pressed
        private void Confirm_Click(object sender, EventArgs e)
        {
            //Runs through each GroupBox in the Panel
            foreach (GroupBox g in panel1.Controls)
            {
                //Creates a new array of Manifest items to represent the change to this game and adds it to 'Modifs' List
                this.Modifs.Add(new Manifest[]
                {
                    new Manifest()
                    {
                        name = (g.Controls.Find("nameOldBox", true).FirstOrDefault() as TextBox).Text,
                        path = (g.Controls.Find("idOldBox", true).FirstOrDefault() as TextBox).Text,
                        steamLaunch = (g.Controls.Find("steamLaunchOldBox", true).FirstOrDefault() as CheckBox).Checked,
                        useShortcut = (g.Controls.Find("shortcutLaunchOldBox", true).FirstOrDefault() as CheckBox).Checked
                    },
                    new Manifest()
                    {
                        name = (g.Controls.Find("nameNewBox", true).FirstOrDefault() as TextBox).Text,
                        path = (g.Controls.Find("idNewBox", true).FirstOrDefault() as TextBox).Text,
                        steamLaunch = (g.Controls.Find("steamLaunchNewBox", true).FirstOrDefault() as CheckBox).Checked,
                        useShortcut = (g.Controls.Find("shortcutLaunchNewBox", true).FirstOrDefault() as CheckBox).Checked
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
            else if (MessageBox.Show("Cancelling will revert any changes made. Are you sure?", "Confirm Cancel", MessageBoxButtons.YesNo) == DialogResult.Yes)
                e.Cancel = false;
            //If Form is closing for any reason that isn't caught by the above statements (user clicked 'No' on prompt, or other reason), cancel the close
            else
                e.Cancel = true;
        }
    }

    class ModifGroup : GroupBox
    {
        private Label label1, label2, label3, label4, label5, label6, label7, label8, label9, label10;
        private TextBox nameOldBox, nameNewBox, idOldBox, idNewBox;
        private CheckBox steamLaunchOldBox, steamLaunchNewBox, shortcutLaunchOldBox, shortcutLaunchNewBox;
        private Button browseBtn, removeBtn;
        //The Form containing this object. Used for calling ModificationsForm.Remove
        private readonly ModificationsForm _parent;

        public ModifGroup(Manifest oldMan, Manifest newMan, ModificationsForm parent)
        {
            this._parent = parent;
            //
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
            this.label3.MouseLeave += new EventHandler(this.Path_MouseLeave);

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
            this.label4.MouseLeave += new EventHandler(this.SteamLaunch_MouseLeave);

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
            this.label5.MouseLeave += new EventHandler(this.ShortcutLaunch_MouseLeave);

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
            this.label8.MouseLeave += new EventHandler(this.Path_MouseLeave);

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
            this.label9.MouseLeave += new EventHandler(this.SteamLaunch_MouseLeave);

            this.label10 = new Label()
            {
                AutoSize = true,
                Location = new Point(137, 116),
                Name = "label10",
                Size = new Size(86, 13),
                Text = "Shortcut Launch"
            };
            this.label10.MouseHover += new EventHandler(this.ShortcutLaunch_MouseHover);
            this.label10.MouseLeave += new EventHandler(this.ShortcutLaunch_MouseLeave);

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
            this.browseBtn.MouseLeave += new EventHandler(this.Browse_MouseLeave);

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
            this.removeBtn.MouseLeave += new EventHandler(this.Remove_MouseLeave);

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
            this.steamLaunchOldBox.MouseLeave += new EventHandler(this.SteamLaunch_MouseLeave);

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
            this.steamLaunchNewBox.MouseLeave += new EventHandler(this.SteamLaunch_MouseLeave);

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
            this.shortcutLaunchOldBox.MouseLeave += new EventHandler(this.ShortcutLaunch_MouseLeave);

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
            this.shortcutLaunchNewBox.MouseLeave += new EventHandler(this.ShortcutLaunch_MouseLeave);

            this.idOldBox = new TextBox()
            {
                Location = new Point(278, 31),
                Name = "idOldBox",
                ReadOnly = true,
                Size = new Size(150, 20),
                TabIndex = 3,
                TabStop = false,
                Text = oldMan.path,
                WordWrap = false
            };

            this.idNewBox = new TextBox()
            {
                Location = new Point(278, 90),
                Name = "idNewBox",
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

            //
            //
            //
            this.Controls.Add(this.browseBtn);
            this.Controls.Add(this.removeBtn);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.shortcutLaunchNewBox);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.steamLaunchNewBox);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.idNewBox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.nameNewBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.shortcutLaunchOldBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.steamLaunchOldBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.idOldBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.nameOldBox);
            this.Controls.Add(this.label2);
            this.Name = "modifGroup";
            this.Size = new Size(440, 145);
            this.TabIndex = 1;
            this.TabStop = false;
            //
            //
            //
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
                this.idNewBox.Text = d.FileName;
        }

        //Removes the changes from the listing
        private void Remove_Click(object sender, EventArgs e)
        {
            //Calls ModificationForm.Remove and gives it a new Manifest with the data of the 'old' details of the game
            this._parent.Remove(new Manifest()
            {
                name = this.nameOldBox.Text,
                path = this.idOldBox.Text,
                steamLaunch = this.steamLaunchOldBox.Checked,
                useShortcut = this.shortcutLaunchOldBox.Checked
            });
        }

        //Used to get the GroupBox from this object so it can be put into the Panel's controls
        //public GroupBox Group { get { return this.groupBox1; } }
        //Sets the Location of the GroupBox
        //public Point Location { set { this.groupBox1.Location = value; } }

        //Tooltip Methods for labels and such
        ToolTip tip = new ToolTip();
        private void Path_MouseHover(object sender, EventArgs e)
        {
            tip.Show("The path used to launch the game. If it is a Steam launch, the launch path should be set as the game's Steam ID number.", _parent, ((Control)sender).Location, 10000);
        }
        private void Path_MouseLeave(object sender, EventArgs e)
        {
            tip.Hide(_parent);
        }
        private void SteamLaunch_MouseHover(object sender, EventArgs e)
        {
            tip.Show("Whether or not the game should be launched through Steam", _parent, ((Control)sender).Location, 10000);
        }
        private void SteamLaunch_MouseLeave(object sender, EventArgs e)
        {
            tip.Hide(_parent);
        }
        private void ShortcutLaunch_MouseHover(object sender, EventArgs e)
        {
            tip.Show("Whether or not the game should be launched by creating a shortcut. This may help if a local game is crashing while launching or not launching at all", _parent, ((Control)sender).Location, 10000);
        }
        private void ShortcutLaunch_MouseLeave(object sender, EventArgs e)
        {
            tip.Hide(_parent);
        }
        private void Browse_MouseHover(object sender, EventArgs e)
        {
            tip.Show("Browse files", _parent, ((Control)sender).Location, 10000);
        }
        private void Browse_MouseLeave(object sender, EventArgs e)
        {
            tip.Hide(_parent);
        }
        private void Remove_MouseHover(object sender, EventArgs e)
        {
            tip.Show("Remove modifications to this game", _parent, ((Control)sender).Location, 10000);
        }
        private void Remove_MouseLeave(object sender, EventArgs e)
        {
            tip.Hide(_parent);
        }
    }
}
