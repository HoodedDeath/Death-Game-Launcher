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
using static Death_Game_Launcher.Form1;

namespace Death_Game_Launcher
{
    public partial class ModificationsForm : Form
    {
        //Used to get the final list of changes made to games
        public List<Manifest[]> Modifs { get; set; } = new List<Manifest[]>();
        //Path to the file listing out game changes
        private readonly string _gameModsFile = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "DLG"), "Mods.cfg");
        //List of game changes initially found
        private List<Manifest[]> _gameMods = new List<Manifest[]>();

        public ModificationsForm()
        {
            InitializeComponent();
            //Reads through the file of changes
            Read();
            //Removes any duplicates
            EliminateDuplicates();
            //Displays the list of changes
            List();
        }
        //Reads through the changes file. Essentially identical to the modification section of Form1.ReadGameConfigs
        private void Read()
        {
            //Modify any games based on the Modifications file
            try
            {
                //Makes sure all subfolders exist
                GenSubfolders(_gameModsFile);
                //Only run if modifications file exists
                if (File.Exists(_gameModsFile))
                {
                    StreamReader sr = new StreamReader(File.Open(_gameModsFile, FileMode.Open));
                    //Variable _gameMods is a List containing Manifest arrays, which hold the old details in the first and the new details in the second, so this array does the same on an individual level
                    Manifest[] m = new Manifest[] { new Manifest(), new Manifest() };
                    //Used just to tell if we're reading the old details or the new details of the game
                    int i = 0;
                    //Infinite loop to read file to the end
                    for (; ; )
                    {
                        string s = sr.ReadLine();
                        //Breaks at end of file
                        if (s == null || s == "") break;
                        string[] arr = s.Split('"');
                        //If this is the end of the details for specific game
                        if (arr[0] == "}")
                        {
                            //Adds this games details to _gameMods
                            this._gameMods.Add(m);
                            //Resets the Manifest array for the next game's details
                            m = new Manifest[] { new Manifest(), new Manifest() };
                            //Resets i since we'll be reading the next game's old details first
                            i = 0;
                        }
                        //The semicolon in the file marks when the old details end and the new details begin
                        else if (arr[0] == ";")
                        {
                            //Incriments i since we'll be reading the game's new details next
                            i++;
                        }
                        //As long as this line isn't the start of a game's details
                        else if (arr[0] != "{")
                        {
                            //Switches on the value in the array that will be holding what value it is holding (name, path, steamLaunch, or useShortcut)
                            //Cases are self-explanatory.
                            // 'm[i]' is deciding if the value read will be saved in the new details or the old details: 0 for old details, 1 for new details
                            switch (arr[1].ToLower().Trim())
                            {
                                case "name":
                                    m[i].name = arr[arr.Length - 2];
                                    break;
                                case "path":
                                    m[i].path = arr[arr.Length - 2];
                                    break;
                                case "steam":
                                    m[i].steamLaunch = bool.Parse(arr[arr.Length - 2]);
                                    break;
                                case "shortcut":
                                    m[i].useShortcut = bool.Parse(arr[arr.Length - 2]);
                                    break;
                            }
                        }
                    }
                    sr.Close();
                    sr.Dispose();
                }
            }
            catch (Exception e) { MessageBox.Show("Failed to load modification list:\n" + e.Message); }
        }
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
                }.Group);
                //If there are more than 3 items listed, we need a scroll bar. AutoScroll will display it, but this compensates for the extra width by adding 14 pixels to the width of the panel, avoiding the need for the horizontal scroll bar 
                if (i > 3) panel1.Size = new Size(460, panel1.Size.Height);
            }
        }
        //Makes sure all folders leading to the given path exist to avoid System.IO.DirectoryNotFoundException
        private void GenSubfolders(string path)
        {
            //Splits the path to seperate folders
            string[] arr = path.Split('\\');
            //String to keep track of full directory
            //Starts as the drive letter (example: 'C:'), gets the '\' added before each folder as the loop runs
            string t = arr[0];
            //Goes through each folder in the path and makes sure they exist, stops before the file at the end of the path
            for (int i = 1; i < arr.Length - 1; i++)
                if (!Directory.Exists(t += ("\\" + arr[i])))
                    Directory.CreateDirectory(t);
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

    public class ModifGroup
    {
        private GroupBox groupBox1;
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
            this.groupBox1 = new GroupBox();
            this.browseBtn = new Button();
            this.removeBtn = new Button();
            this.label6 = new Label();
            this.shortcutLaunchNewBox = new CheckBox();
            this.label10 = new Label();
            this.steamLaunchNewBox = new CheckBox();
            this.label9 = new Label();
            this.idNewBox = new TextBox();
            this.label8 = new Label();
            this.nameNewBox = new TextBox();
            this.label7 = new Label();
            this.label1 = new Label();
            this.shortcutLaunchOldBox = new CheckBox();
            this.label5 = new Label();
            this.steamLaunchOldBox = new CheckBox();
            this.label4 = new Label();
            this.idOldBox = new TextBox();
            this.label3 = new Label();
            this.nameOldBox = new TextBox();
            this.label2 = new Label();
            //
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.browseBtn);
            this.groupBox1.Controls.Add(this.removeBtn);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.shortcutLaunchNewBox);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.steamLaunchNewBox);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.idNewBox);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.nameNewBox);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.shortcutLaunchOldBox);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.steamLaunchOldBox);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.idOldBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.nameOldBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(440, 145);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new Size(32, 16);
            this.label1.TabIndex = 8;
            this.label1.Text = "Old";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new Point(24, 34);
            this.label2.Name = "label2";
            this.label2.Size = new Size(38, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Name:";
            // 
            // nameOldBox
            // 
            this.nameOldBox.Location = new Point(68, 31);
            this.nameOldBox.Name = "nameOldBox";
            this.nameOldBox.ReadOnly = true;
            this.nameOldBox.Size = new Size(150, 20);
            this.nameOldBox.TabIndex = 1;
            this.nameOldBox.TabStop = false;
            this.nameOldBox.Text = oldMan.name;
            this.nameOldBox.WordWrap = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new Point(224, 34);
            this.label3.Name = "label3";
            this.label3.Size = new Size(48, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Path/ID:";
            this.label3.MouseHover += new EventHandler(this.Path_MouseHover);
            this.label3.MouseLeave += new EventHandler(this.Path_MouseLeave);
            // 
            // idOldBox
            // 
            this.idOldBox.Location = new Point(278, 31);
            this.idOldBox.Name = "idOldBox";
            this.idOldBox.ReadOnly = true;
            this.idOldBox.Size = new Size(150, 20);
            this.idOldBox.TabIndex = 3;
            this.idOldBox.TabStop = false;
            this.idOldBox.Text = oldMan.path;
            this.idOldBox.WordWrap = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new Point(24, 57);
            this.label4.Name = "label4";
            this.label4.Size = new Size(76, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Steam Launch";
            this.label4.MouseHover += new EventHandler(this.SteamLaunch_MouseHover);
            this.label4.MouseLeave += new EventHandler(this.SteamLaunch_MouseLeave);
            // 
            // steamLaunchOldBox
            // 
            this.steamLaunchOldBox.AutoCheck = false;
            this.steamLaunchOldBox.AutoSize = true;
            this.steamLaunchOldBox.Checked = oldMan.steamLaunch;
            this.steamLaunchOldBox.Enabled = false;
            this.steamLaunchOldBox.Location = new Point(106, 57);
            this.steamLaunchOldBox.Name = "steamLaunchOldBox";
            this.steamLaunchOldBox.Size = new Size(15, 14);
            this.steamLaunchOldBox.TabIndex = 5;
            this.steamLaunchOldBox.TabStop = false;
            this.steamLaunchOldBox.UseVisualStyleBackColor = true;
            this.steamLaunchOldBox.MouseHover += new EventHandler(this.SteamLaunch_MouseHover);
            this.steamLaunchOldBox.MouseLeave += new EventHandler(this.SteamLaunch_MouseLeave);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new Point(137, 57);
            this.label5.Name = "label5";
            this.label5.Size = new Size(86, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Shortcut Launch";
            this.label5.MouseHover += new EventHandler(this.ShortcutLaunch_MouseHover);
            this.label5.MouseLeave += new EventHandler(this.ShortcutLaunch_MouseLeave);
            // 
            // shortcutLaunchOldBox
            // 
            this.shortcutLaunchOldBox.AutoCheck = false;
            this.shortcutLaunchOldBox.AutoSize = true;
            this.shortcutLaunchOldBox.Checked = oldMan.useShortcut;
            this.shortcutLaunchOldBox.Enabled = false;
            this.shortcutLaunchOldBox.Location = new Point(229, 57);
            this.shortcutLaunchOldBox.Name = "shortcutLaunchOldBox";
            this.shortcutLaunchOldBox.Size = new Size(15, 14);
            this.shortcutLaunchOldBox.TabIndex = 7;
            this.shortcutLaunchOldBox.TabStop = false;
            this.shortcutLaunchOldBox.UseVisualStyleBackColor = true;
            this.shortcutLaunchOldBox.MouseHover += new EventHandler(this.ShortcutLaunch_MouseHover);
            this.shortcutLaunchOldBox.MouseLeave += new EventHandler(this.ShortcutLaunch_MouseLeave);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new Font("Microsoft Sans Serif", 9.75F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new Point(6, 75);
            this.label6.Name = "label6";
            this.label6.Size = new Size(38, 16);
            this.label6.TabIndex = 17;
            this.label6.Text = "New";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new Point(24, 93);
            this.label7.Name = "label7";
            this.label7.Size = new Size(38, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "Name:";
            // 
            // nameNewBox
            // 
            this.nameNewBox.Location = new Point(68, 90);
            this.nameNewBox.Name = "nameNewBox";
            this.nameNewBox.Size = new Size(150, 20);
            this.nameNewBox.TabIndex = 10;
            this.nameNewBox.Text = newMan.name;
            this.nameNewBox.WordWrap = false;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new Point(224, 93);
            this.label8.Name = "label8";
            this.label8.Size = new Size(48, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Path/ID:";
            this.label8.MouseHover += new EventHandler(this.Path_MouseHover);
            this.label8.MouseLeave += new EventHandler(this.Path_MouseLeave);
            // 
            // idNewBox
            // 
            this.idNewBox.Location = new Point(278, 90);
            this.idNewBox.Name = "idNewBox";
            this.idNewBox.Size = new Size(150, 20);
            this.idNewBox.TabIndex = 12;
            this.idNewBox.Text = newMan.path;
            this.idNewBox.WordWrap = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new Point(24, 116);
            this.label9.Name = "label9";
            this.label9.Size = new Size(76, 13);
            this.label9.TabIndex = 13;
            this.label9.Text = "Steam Launch";
            this.label9.MouseHover += new EventHandler(this.SteamLaunch_MouseHover);
            this.label9.MouseLeave += new EventHandler(this.SteamLaunch_MouseLeave);
            // 
            // steamLaunchNewBox
            // 
            this.steamLaunchNewBox.AutoSize = true;
            this.steamLaunchNewBox.Checked = newMan.steamLaunch;
            this.steamLaunchNewBox.Location = new Point(106, 116);
            this.steamLaunchNewBox.Name = "steamLaunchNewBox";
            this.steamLaunchNewBox.Size = new Size(15, 14);
            this.steamLaunchNewBox.TabIndex = 14;
            this.steamLaunchNewBox.UseVisualStyleBackColor = true;
            this.steamLaunchNewBox.Click += new EventHandler(this.SteamCheck_Click);
            this.steamLaunchNewBox.MouseHover += new EventHandler(this.SteamLaunch_MouseHover);
            this.steamLaunchNewBox.MouseLeave += new EventHandler(this.SteamLaunch_MouseLeave);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new Point(137, 116);
            this.label10.Name = "label10";
            this.label10.Size = new Size(86, 13);
            this.label10.TabIndex = 15;
            this.label10.Text = "Shortcut Launch";
            this.label10.MouseHover += new EventHandler(this.ShortcutLaunch_MouseHover);
            this.label10.MouseLeave += new EventHandler(this.ShortcutLaunch_MouseLeave);
            // 
            // shortcutLaunchNewBox
            // 
            this.shortcutLaunchNewBox.AutoSize = true;
            this.shortcutLaunchNewBox.Checked = newMan.useShortcut;
            this.shortcutLaunchNewBox.Location = new Point(229, 116);
            this.shortcutLaunchNewBox.Name = "shortcutLaunchNewBox";
            this.shortcutLaunchNewBox.Size = new Size(15, 14);
            this.shortcutLaunchNewBox.TabIndex = 16;
            this.shortcutLaunchNewBox.UseVisualStyleBackColor = true;
            this.shortcutLaunchNewBox.Click += new EventHandler(this.ShortcutCheck_Click);
            this.shortcutLaunchNewBox.MouseHover += new EventHandler(this.ShortcutLaunch_MouseHover);
            this.shortcutLaunchNewBox.MouseLeave += new EventHandler(this.ShortcutLaunch_MouseLeave);
            // 
            // browseBtn
            // 
            this.browseBtn.Location = new Point(332, 116);
            this.browseBtn.Name = "browseBtn";
            this.browseBtn.Size = new Size(25, 23);
            this.browseBtn.TabIndex = 2;
            this.browseBtn.Text = "...";
            this.browseBtn.UseVisualStyleBackColor = true;
            this.browseBtn.Click += new EventHandler(this.Browse_Click);
            this.browseBtn.MouseHover += new EventHandler(this.Browse_MouseHover);
            this.browseBtn.MouseLeave += new EventHandler(this.Browse_MouseLeave);
            // 
            // removeBtn
            // 
            this.removeBtn.Location = new Point(363, 116);
            this.removeBtn.Name = "removeBtn";
            this.removeBtn.Size = new Size(65, 23);
            this.removeBtn.TabIndex = 2;
            this.removeBtn.Text = "Remove";
            this.removeBtn.UseVisualStyleBackColor = true;
            this.removeBtn.Click += new EventHandler(this.Remove_Click);
            this.removeBtn.MouseHover += new EventHandler(this.Remove_MouseHover);
            this.removeBtn.MouseLeave += new EventHandler(this.Remove_MouseLeave);
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
        public GroupBox Group { get { return this.groupBox1; } }
        //Sets the Location of the GroupBox
        public Point Location { set { this.groupBox1.Location = value; } }

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
