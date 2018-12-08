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
    public partial class AddGameForm : Form
    {
        //List of the names given to the GroupBoxes within the panel, for searching
        private List<string> groupBoxes = new List<string>();
        //List of games added, to be filled when the form is closing and to be retrieved later by the object that created and called this this Form
        public List<Game> Games { get; set; } = new List<Game>();
        //Structure to hold and return the data of a game
        public struct Game
        {
            public string name;
            public string path;
            public bool isSteam;
            public bool useShortcut;
        }
        //Initialization
        public AddGameForm()
        {
            InitializeComponent();
            //File drag and drop
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(GameDrag);
            this.DragDrop += new DragEventHandler(GameDrop);
            //Adds the first GroupBox to the panel
            AddGroup();
        }
        //Initial drag event
        private void GameDrag(object sender, DragEventArgs e)
        {
            //Only shows the copy effect if the drag type is a file
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
        //File Dropped event
        private void GameDrop(object sender, DragEventArgs e)
        {
            //Only happens if the drag type is a file
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //Paths of all files dropped into the Form
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                //Runs through each file dropped in to add them to the list
                foreach (string file in files) AddGame(file);
            }
        }
        //Called when a file is dropped, used to Add the name and path of the file to the list
        private void AddGame(string path)
        {
            //If the bottom-most GroupBox's Path TextBox is not empty, add a new GroupBox to the Panel
            if (!TestLastEmpty())
                AddGroup();
            //Finds the 'nameTextBox' and sets its text to the name of the file
            /*Process:
             * Split the path to the file at each '\' character
             * Get the last item in the split array (by splitting again and counting it, since I am not storing the array, as it's only used once and I found no measureable performance difference)
             * Split the resulting string at the '.' character, splitting the name from the extension of the file (this can, though, split other areas of the file. Example: A file named 'App.v1.exe' will result in the name being input as 'App')
             * Save the first item (the file name) to 'nameTextBox'
            */
            ((panel1.Controls.Find(this.groupBoxes[count - 1], false).LastOrDefault() as GroupBox).Controls.Find("nameTextBox", true).FirstOrDefault() as TextBox).Text = path.Split('\\')[path.Split('\\').Length - 1].Split('.')[0];
            //Finds the 'pathTextBox' and saves the path of the file to it
            ((panel1.Controls.Find(this.groupBoxes[count - 1], false).LastOrDefault() as GroupBox).Controls.Find("pathTextBox", true).FirstOrDefault() as TextBox).Text = path;
            //Finds the 'steamCheckBox' and sets it to false, as the drag drop is a local file
            ((panel1.Controls.Find(this.groupBoxes[count - 1], false).LastOrDefault() as GroupBox).Controls.Find("steamCheckBox", true).FirstOrDefault() as CheckBox).Checked = false;
            //Finds the 'shortcutCheckBox' and sets it to false, as a shortcut launch may not be wanted by default, though it can solve problems with games not launching correctly
            ((panel1.Controls.Find(this.groupBoxes[count - 1], false).LastOrDefault() as GroupBox).Controls.Find("shortcutCheckBox", true).FirstOrDefault() as CheckBox).Checked = false;
        }

        //Adds an empty GroupBox to the bottom of the Listing
        private void AddGroup()
        {
            //Reset the Panel's Auto Scroll Position to avoid strange spacing between the GroupBoxes
            panel1.AutoScrollPosition = new Point(0, 0);
            //Creates a new AddGameGrouping to add in the values
            AddGameGrouping g = new AddGameGrouping
            {
                //The Location of the new GroupBox is 136 px below the previous one (136 being the height of the GroupBox plus the 3 padding on top and bottom)
                Location = new Point(location[0], location[1] += 136),
                //The name given to the GroupBox, which is "groupBox" and the number of the previous GroupBox added plus one (used for finding the most recent one added)
                BoxName = "groupBox" + this.count++
            };
            //Get the GroupBox from the AddGameGrouping instance and add it into the Panel
            panel1.Controls.Add(g.Group);
            //Adds the name into the 'groupBoxes' List for later searching
            groupBoxes.Add(g.BoxName);
            //If there are 3 or more GroupBoxes in the Panel, increase the width of the Panel to compensate for the vertical Scroll Bar, avoiding a horizontal Scroll Bar showing
            if (count >= 3 && panel1.Size.Width != 265) panel1.Size = new Size(265, panel1.Size.Height);
        }

        //Captures the KeyDown event while the Form is active
        private void AddGameForm_KeyDown(object sender, KeyEventArgs e)
        {
            //If the user pressed escape, try to exit as if the 'Cancel' button was clicked
            if (e.KeyCode == Keys.Escape)
                Cancel_Click(sender, null);
        }

        //Called when the user clicks the 'Confirm' Button
        private void Confirm_Click(object sender, EventArgs e)
        {
            //Runs through each GroupBox in the Panel
            foreach (GroupBox g in panel1.Controls)
            {
                //Creates a new 'Game' instance to add the details of the GroupBox to the 'Games' List
                this.Games.Add(new Game()
                {
                    //Finds the 'nameTextBox' and saves its value
                    name = (g.Controls.Find("nameTextBox", true).FirstOrDefault() as TextBox).Text,
                    //Finds the 'pathTextBox' and saves its value
                    path = (g.Controls.Find("pathTextBox", true).FirstOrDefault() as TextBox).Text,
                    //Finds the 'steamCheckBox' and saves its value
                    isSteam = (g.Controls.Find("steamCheckBox", true).FirstOrDefault() as CheckBox).Checked,
                    //Finds the 'shortcutCheckBox' and saves its value
                    //Only true if the CheckBox is both checked and enabled (checking the Steam launch CheckBox disables this CheckBox, as a shortcut launch is not needed for a Steam launch)
                    useShortcut = (g.Controls.Find("shortcutCheckBox", true).FirstOrDefault() as CheckBox).Checked && (g.Controls.Find("shortcutCheckBox", true).FirstOrDefault() as CheckBox).Enabled
                });
            }
            //Sets the Form's DialogResult to OK
            this.DialogResult = DialogResult.OK;
            //Closes the Form
            Close();
        }
        //Called when the user clicks the 'Cancel' button or presses the Escape key
        private void Cancel_Click(object sender, EventArgs e)
        {
            //Sets the Form's DialogResult to Cancel
            this.DialogResult = DialogResult.Cancel;
            //Closes the Form
            Close();
        }
        //Event happens just before the Form actually closes
        private void AddGameForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //If the user pressed the 'Confirm' button, allow exit without confirmation
            if (this.DialogResult == DialogResult.OK)
                e.Cancel = false;
            //Otherwise, only exit if the user clicks the 'Yes' button on the confirmation dialog
            else
                e.Cancel = !(MessageBox.Show("Cancelling will lose list of games to add", "Confirm Cancel", MessageBoxButtons.YesNo) == DialogResult.Yes);
        }

        //The current location of the last added GroupBox in the Panel. The X value is not changed, the Y value defaults to the the height a GroupBox would be above the Panel, height plus padding of the bottom
        private int[] location = new int[] { 3, -133 };
        //Current count of the GroupBoxes in the Panel
        private int count = 0;
        //Called when the user clicks the 'Add Game' button
        private void AddGame_Click(object sender, EventArgs e)
        {
            //If the bottom-most GroupBox's Path TextBox is not empty, add a new GroupBox to the Panel
            if (!TestLastEmpty())
                AddGroup();
        }
        //Tests if the bottom-most GroupBox has an empty Path TextBox and should be considered empty
        private bool TestLastEmpty()
        {
            //Finds every GroupBox in the panel that has the name of the last GroupBox added, this should only result in one item returned
            Control[] groupbox = panel1.Controls.Find(this.groupBoxes[count - 1], false);
            //A TextBox used to test if the pathTextBox is found within the GroupBox
            TextBox textbox = new TextBox() { Text = "#FAILED#" };
            //As long as there was a GroupBox found in the panel,
            if (groupbox != null && groupbox.Length > 0)
                //Attempt to find the 'pathTextBox' within the last GroupBox (using the last one, incase there happened to be multiple, to make sure we get the bottom-most GroupBox, which should be the empty one if any are empty) and save that TextBox to the 'textbox' variable for later testing
                textbox = groupbox.LastOrDefault().Controls.Find("pathTextBox", true).FirstOrDefault() as TextBox;
            //String to store the value of the 'pathTextBox', if there is a value
            string lastpath = "";
            //If the 'textbox' variable is not empty and it is not the 'fail' default TextBox
            if (textbox != null && textbox.Text != "#FAILED#")
                //Get the text of the TextBox and store it in the 'lastpath' variable
                lastpath = textbox.Text;
            //If the 'lastpath' string is empty (either a null value or a zero-length string), this will be true, signalling that the last GroupBox data is considered empty
            return lastpath == null || lastpath == "";
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
        private CheckBox shortcutCheckBox;

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
            this.shortcutCheckBox = new CheckBox();
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
            this.groupBox1.Controls.Add(this.shortcutCheckBox);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new Size(240, 154);
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
            this.steamCheckBox.CheckedChanged += new EventHandler(this.SteamChecked_Changed);
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
            // 
            // shortcutCheckBox
            // 
            this.shortcutCheckBox.AutoSize = true;
            this.shortcutCheckBox.Location = new Point(7, 125);
            this.shortcutCheckBox.Name = "shortcutCheckBox";
            this.shortcutCheckBox.RightToLeft = RightToLeft.Yes;
            this.shortcutCheckBox.Size = new Size(127, 17);
            this.shortcutCheckBox.TabIndex = 5;
            this.shortcutCheckBox.Text = "Use Shortcut Launch";
            this.shortcutCheckBox.UseVisualStyleBackColor = true;

            this.groupBox1.ResumeLayout();
        }
        //Gets and Sets the location of the groupBox1
        public Point Location { get { return this.groupBox1.Location; } set { this.groupBox1.Location = value; } }
        //Gets groupBox1 for it to be added into other Controls
        public GroupBox Group { get { return this.groupBox1; } }
        //Gets the game path from the 'pathTextBox'
        public string Path { get { return this.pathTextBox.Text; } }
        //Gets the game name from the 'nameTextBox'
        public string Name { get { return this.nameTextBox.Text; } }
        //Gets whether or not the 'steamCheckBox' is checked
        public bool IsSteamLaunch { get { return this.steamCheckBox.Checked; } }
        //Gets whether or not the 'shortcutCheckBox' is checked and enabled
        public bool UseShortcut { get { return this.shortcutCheckBox.Checked && this.shortcutCheckBox.Enabled; } }
        //Gets and Sets the Name of groupBox1
        public string BoxName { get { return this.groupBox1.Name; } set { this.groupBox1.Name = value; } }

        //
        private void Remove_Click(object sender, EventArgs e)
        {
            //Does nothing yet. Removal from list will be added soon
        }

        //Called when the user clicks the '...' (Browse) button
        private void BrowseButton_Click(object sender, EventArgs e)
        {
            //OpenFileDialog to select a file to get data from
            OpenFileDialog d = new OpenFileDialog();
            //Shows the Dialog and saves the DialogResult
            DialogResult res = d.ShowDialog();
            //Only if the user actually selected a file
            if (res == DialogResult.OK)
            {
                //Gets the path of the selected file
                string path = d.FileName;
                //Sets the 'pathTextBox' Text value to the path of the file
                pathTextBox.Text = path;
                //If the name was not already set, we'll set the 'nameTextBox' Text value to the file's name
                if (nameTextBox.Text == null || nameTextBox.Text == "")
                    /*Process:
                     * Split the path to the file at each '\' character
                     * Get the last item in the split array (by splitting again and counting it, since I am not storing the array, as it's only used once and I found no measureable performance difference)
                     * Split the resulting string at the '.' character, splitting the name from the extension of the file (this can, though, split other areas of the file. Example: A file named 'App.v1.exe' will result in the name being input as 'App')
                     * Save the first item (the file name) to 'nameTextBox'
                    */
                    nameTextBox.Text = path.Split('\\')[path.Split('\\').Length - 1].Split('.')[0];
            }
        }

        //Enables the 'shortcutCheckBox' if the 'steamCheckBox' is not checked and disables the 'shortcutCheckBox' if the 'steamCheckBox' is checked
        private void SteamChecked_Changed(object sender, EventArgs e)
        {
            this.shortcutCheckBox.Enabled = !(this.steamCheckBox.Checked);
        }
    }
}
