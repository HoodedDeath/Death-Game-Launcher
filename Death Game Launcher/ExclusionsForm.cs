using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Death_Game_Launcher;

namespace Death_Game_Launcher
{
    public partial class ExclusionsForm : Form
    {
        //List of games to be removed from _gamesList
        public List<Manifest> Exclusions { get; set; } = new List<Manifest>();

        public ExclusionsForm(List<Manifest> ex)
        {
            Form1._log.Debug("Calling ExclusionForm.InitializeCompnent ...");
            InitializeComponent();
            Form1._log.Debug("ExclusionForm initialized.");
            //Store the exclusions from ex in Exclusions
            Exclusions = ex;
            Form1._log.Debug("Listing exclusions ...");
            //Lists out exclusions
            List();
        }
        //Used to list out all the exclusions
        private void List()
        {
            Form1._log.Debug("Entering ExclusionForm.List ...");
            //The current position of the listed item
            //X position is always 0
            //Y position is defaulted to -17 to make the math of where each CheckBox is simpler
            int[] pos = new int[] { 0, -17 };
            Form1._log.Debug("Looping through manifests ...");
            //Goes through each Manifest in Exclusions
            foreach (Manifest m in Exclusions)
            {
                Form1._log.Debug(string.Format("Adding CheckBox for manifest for {0} ...", m.name));
                //Adds a new CheckBox to the Panel, with the name of the game in the current Manifest
                panel1.Controls.Add(new CheckBox()
                {
                    //Auto size for text
                    AutoSize = true,
                    //Default size
                    Size = new Size(80, 17),
                    //Set Text to name of the game from the current manifest
                    Text = m.name,
                    //Idk
                    UseVisualStyleBackColor = true,
                    //Default checked
                    //Checked items will continue to be excluded
                    Checked = true,
                    //Location will be:
                    // X - always 0, aligned left edge of window
                    // Y - 17 below the previous CheckBox added, adjusting pos[1] simultaneously
                    Location = new Point(pos[0], pos[1] += 17)
                });
            }
            Form1._log.Debug("Relocating confirmation buttons.");
            //Repositions the Accept and Cancel buttons to be below the Panel after the Form and Panel auto size
            cancelButton.Location = new Point(cancelButton.Location.X, panel1.Height + 13 + 3 + 3);
            acceptButton.Location = new Point(acceptButton.Location.X, panel1.Height + 13 + 3 + 3);
            Form1._log.Debug("ExclusionForm.List returning.");
        }

        //Saves the exclusions to the Exclusions file
        private void AddExclusions()
        {
            Form1._log.Debug("Entering ExclusionForm.AddExclusions ...");
            //Temporary list to keep games that are still excluded
            List<Manifest> ex = new List<Manifest>();
            Form1._log.Debug("Looping through controls to find needed exclusions ...");
            //Loops through each CheckBox in the Panel
            foreach (CheckBox c in panel1.Controls)
            {
                Form1._log.Debug(string.Format("Looking for manifest for {0} ...", c.Text));
                //Temporary variable for comparing values
                Manifest m = new Manifest();
                //Search for a Manifest whose name matches the current CheckBox
                foreach (Manifest ma in Exclusions)
                {
                    //If the name of the current game's name ( 'ma.name' ) doesn't equal the text in the current check box ( 'c.Text' ), skip back to the top of the loop
                    if (ma.name != c.Text) continue;
                    //Store ma in m for later use in the loop
                    m = ma;
                    //Break out of this loop since the game matching the checkbox ( 'c' ) has been found
                    break;
                }
                //If there was no matching manifest found for this check box, log a warning (since this shouldn't ever happen) and skip back to the top of the loop
                if (m == new Manifest()) { Form1._log.Warn(string.Format("No manifest matching name '{0}' found during Exclusions Form.", c.Text)); continue; }
                //If the current game is not a Steam launch, skip back to the top of the loop (since there shouldn't be any non-Steam launch games here)
                if (!m.steamLaunch) continue;
                //If the current check box is not checked, skip back to the top of the loop (since only checked games will continue to be excluded)
                if (!c.Checked) continue;
                Form1._log.Debug("Adding exclusion to list to keep.");
                //Add m to the ex list
                ex.Add(m);
            }
            //Saves the list of apps still excluded ( 'ex' ) to the public Exclusions variable for the parent form to read
            Exclusions = ex;
            Form1._log.Debug("ExclusionForm.AddExclusions returning.");
        }

        //When the user presses a key
        private void ExclusionsForm_KeyDown(object sender, KeyEventArgs e)
        {
            //Cancel style exit if the user presses Escape
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                Close();
            }
            //Confirm style exit if the user presses Enter
            else if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.OK;
                Close();
            }
        }
        //Cancel button clicked, sets the DialogResult to Cancel
        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }
        //Accept button clicked, sets the DialogResult to OK
        private void AcceptButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }
        //Before Form closes
        private void ExclusionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //If the Form's DialogResult is OK
            if (this.DialogResult == DialogResult.OK)
            {
                Form1._log.Debug("Exiting Form, calling AddExclusions ...");
                //Save exclusions
                AddExclusions();
                //Exit without confirmation pop-up
                e.Cancel = false;
            }
            //Otherwise, only exit if the user presses 'Yes' on the confirmation popup
            else
                e.Cancel = !(MessageBox.Show("All changes will be lost", "Are you sure?", MessageBoxButtons.YesNo) == DialogResult.Yes);
        }
    }
}
