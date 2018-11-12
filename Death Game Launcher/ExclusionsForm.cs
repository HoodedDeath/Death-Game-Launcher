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
        //Path to file of all games to be excluded from listing
        private readonly string _gameExclusionFile = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "DLG"), "Exclusions.cfg");
        //List of games to be removed from _gamesList
        private List<Manifest> _exclusionsList = new List<Manifest>();

        public ExclusionsForm()
        {
            InitializeComponent();
            ReadExclusions();
            List();
        }
        private void List()
        {
            int[] pos = new int[] { 0, -17 };
            foreach (Manifest m in _exclusionsList)
            {
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
            cancelButton.Location = new Point(cancelButton.Location.X, panel1.Height + 13 + 3 + 3);
            acceptButton.Location = new Point(acceptButton.Location.X, panel1.Height + 13 + 3 + 3);
        }

        private void ReadExclusions()
        {
            try
            {
                //Makes sure all subfolders exist
                GenSubfolders(_gameExclusionFile);
                //Only runs if exclusion file exists
                if (File.Exists(_gameExclusionFile))
                {
                    StreamReader sr = new StreamReader(File.Open(_gameExclusionFile, FileMode.Open));
                    //Used for storing data of the current game
                    Manifest ma = new Manifest();
                    //Infinite loop to read file to the end
                    for (; ; )
                    {
                        string s = sr.ReadLine();
                        //Breaks at end of file
                        if (s == null || s == "") break;
                        string[] arr = s.Split('"');
                        //If the line read is the end of the current game's details
                        if (arr[0].Trim() == "}")
                        {
                            //Save game details to _exclusionList
                            _exclusionsList.Add(ma);
                            //Reset the Manifest for next game
                            ma = new Manifest();
                        }
                        //As long as the line read is not the beginning of a game's details
                        else if (arr[0].Trim() != "{")
                        {
                            //Switches on the value in the array that will be holding what value it is holding (name, path, steamLaunch, or useShortcut)
                            //Cases are self-explanatory.
                            switch (arr[1].ToLower().Trim())
                            {
                                case "name":
                                    ma.name = arr[arr.Length - 2];
                                    break;
                                case "path":
                                    ma.path = arr[arr.Length - 2];
                                    break;
                                case "steam":
                                    ma.steamLaunch = bool.Parse(arr[arr.Length - 2]);
                                    break;
                                case "shortcut":
                                    ma.useShortcut = bool.Parse(arr[arr.Length - 2]);
                                    break;
                            }
                        }
                    }
                    sr.Close();
                    sr.Dispose();
                }
                //Loops through each game in the exclusions and removes it from the list of games
                /*foreach (Manifest m in _exclusionsList.ToArray())
                {
                    _gamesList.Remove(m);
                }*/
                this._exclusionsList.Sort((x, y) => x.name.CompareTo(y.name));
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to load list of Steam game exclusions:\n" + e.Message);
            }
        }

        private void AddExclusions()
        {
            try
            {
                //Makes sure all subfolders exist to help prevent the StreamWriter from throwing an exception
                GenSubfolders(_gameExclusionFile);
                //Used to avoid the process of checking what games are already listed in the file by removing the file
                if (File.Exists(_gameExclusionFile)) File.Delete(_gameExclusionFile);
                //Creates file to store list of excluded Steam games
                StreamWriter sw = new StreamWriter(File.Open(_gameExclusionFile, FileMode.OpenOrCreate));
                //Loops through all games listed
                foreach (CheckBox c in panel1.Controls)
                {
                    Manifest m = new Manifest();
                    foreach (Manifest ma in this._exclusionsList)
                    {
                        if (ma.name == c.Text)
                        {
                            m = ma;
                            break;
                        }
                    }
                    //Only acts on the games that launch through Steam
                    if (m.steamLaunch && c.Checked)
                    {
                        //Writes details of game in the file, self explanatory
                        sw.WriteLine("{");
                        sw.WriteLine("\t\"name\":\"{0}\"", m.name);
                        sw.WriteLine("\t\"path\":\"{0}\"", m.path);
                        sw.WriteLine("\t\"steam\":\"{0}\"", m.steamLaunch);
                        sw.WriteLine("\t\"shortcut\":\"{0}\"", m.useShortcut);
                        sw.WriteLine("}");
                    }
                }
                sw.Close();
                sw.Dispose();
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to save excluded games:\n" + e.Message);
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

        private void ExclusionsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                Close();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void acceptButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void ExclusionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
            {
                AddExclusions();
                e.Cancel = false;
            }
            else
                e.Cancel = !(MessageBox.Show("All changes will be lost", "Are you sure?", MessageBoxButtons.YesNo) == DialogResult.Yes);
        }
    }
}
