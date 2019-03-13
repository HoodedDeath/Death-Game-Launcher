using Microsoft.Win32;
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
    public partial class ExclusionsForm : Form
    {
        //Registry path
        private const string regpath = "HKEY_CURRENT_USER\\Software\\HoodedDeathApplications\\DeathGameLauncher";
        //List of games to be removed from _gamesList
        private List<Manifest> _exclusionsList = new List<Manifest>();

        public ExclusionsForm()
        {
            InitializeComponent();
            //Read through the Exclusions file
            ReadExclusions();
            //Lists out to found exclusions
            List();
        }
        //Used to list out all the exclusions
        private void List()
        {
            //The current position of the listed item
            int[] pos = new int[] { 0, -17 };
            //Goes through each Manifest (each exclusion)
            foreach (Manifest m in _exclusionsList)
            {
                //Adds a new CheckBox to the Panel, with the name of the game in the current Manifest
                panel1.Controls.Add(new CheckBox()
                {
                    AutoSize = true,
                    Size = new Size(80, 17),
                    Text = m.name,
                    UseVisualStyleBackColor = true,
                    Checked = true,
                    Location = new Point(pos[0], pos[1] += 17)
                });
            }
            //Repositions the Accept and Cancel buttons to be below the Panel after the Form and Panel auto size
            cancelButton.Location = new Point(cancelButton.Location.X, panel1.Height + 13 + 3 + 3);
            acceptButton.Location = new Point(acceptButton.Location.X, panel1.Height + 13 + 3 + 3);
        }

        //Reads through the Exclusions file
        private void ReadExclusions()
        {

            try
            {
                //Return value from the Exclusions registry entry
                string[] ret = (string[])Registry.GetValue(regpath, "Exclusions", null);
                //If the return value is not empty
                if (ret != null && ret.Length > 0)
                {
                    //Temporary variable to hold current game's details
                    Manifest manifest = new Manifest();
                    //Loops through each string in the return value
                    foreach (string s in ret)
                    {
                        string[] arr = s.Split('"');
                        switch (arr[0].Trim())
                        {
                            //Skips over opening bracket
                            case "{":
                                break;
                            //Copies the current game's details to _exclusionsList and resets manifest
                            case "}":
                                _exclusionsList.Add(manifest);
                                manifest = new Manifest();
                                break;
                            //Reads through each line for details for manifest
                            default:
                                switch (arr[1].ToLower().Trim())
                                {
                                    case "name":
                                        manifest.name = arr[arr.Length - 2];
                                        break;
                                    case "path":
                                        manifest.path = arr[arr.Length - 2];
                                        break;
                                    case "steam":
                                        manifest.steamLaunch = bool.Parse(arr[arr.Length - 2]);
                                        break;
                                    case "shortcut":
                                        manifest.useShortcut = bool.Parse(arr[arr.Length - 2]);
                                        break;
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to load list of Steam game exclusions:\n" + e.Message);
            }

            //Sorts the List of exclusions
            this._exclusionsList.Sort((x, y) => x.name.CompareTo(y.name));
        }

        //Saves the exclusions to the Exclusions file
        private void AddExclusions()
        {
            try
            {
                //String List to be saved to the registry
                List<string> list = new List<string>();
                //Loops through each CheckBox in the Panel
                foreach (CheckBox c in panel1.Controls)
                {
                    //Temporary variable for comparing values
                    Manifest m = new Manifest();
                    //Search for a Manifest whose name matches the current CheckBox
                    foreach (Manifest ma in _exclusionsList)
                    {
                        if (ma.name == c.Text)
                        {
                            m = ma;
                            break;
                        }
                    }
                    //If m is not empty, it is a Steam launch, and its CheckBox is checked, add the details to list
                    if (m != new Manifest() && m.steamLaunch && c.Checked)
                    {
                        list.Add("{");
                        list.Add("\t\"name\":\"" + m.name + "\"");
                        list.Add("\t\"path\":\"" + m.path + "\"");
                        list.Add("\t\"steam\":\"" + m.steamLaunch + "\"");
                        list.Add("\t\"shortcut\":\"" + m.useShortcut + "\"");
                        list.Add("}");
                    }
                }
                //Converts list to an array and saves it to the registry
                Registry.SetValue(regpath, "Exclusions", list.ToArray());
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to save excluded games:\n" + e.Message);
            }
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
                //Save exclusions
                AddExclusions();
                //Exit without confirmation popup
                e.Cancel = false;
            }
            //Otherwise, only exit if the user presses 'Yes' on the confirmation popup
            else
                e.Cancel = !(MessageBox.Show("All changes will be lost", "Are you sure?", MessageBoxButtons.YesNo) == DialogResult.Yes);
        }
    }
}
