using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.Win32;

namespace Death_Game_Launcher
{
    public partial class Form1 : Form
    {
        //Used for Close On Launch option
        private bool isExit = false;
        //Were Steam games scanned for at start
        private bool _scannedSteam = false;
        //List of games to be displayed
        private List<Manifest> _gamesList = new List<Manifest>();
        //List of games to be removed from _gamesList
        private List<Manifest> _exclusionsList = new List<Manifest>();
        //List of detail modifications to be made to games within _gamesList
        private List<Manifest[]> _gameMods = new List<Manifest[]>();
        //Form that just displays a loading circle animation, used while scanning for games
        private LoadingForm _loadingForm = new LoadingForm();
        //Path to file of all executable-launch games added by user
        private readonly string _gameInclusionFile = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "DLG"), "Inclusions.cfg");
        //Path to file of all games to be excluded from listing
        private readonly string _gameExclusionFile = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "DLG"), "Exclusions.cfg");
        //Path to file of all modifications to games in listing
        private readonly string _gameModsFile = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "DLG"), "Mods.cfg");

        //Registry path to application settings
        private const string regpath = "HKEY_CURRENT_USER\\Software\\HoodedDeathApplications\\DeathGameLauncher";


        public Form1()
        {
            //Check if Registry directory exists
            try
            {
                /*RegistryKey r = RegistryKey.OpenRemoteBaseKey(RegistryHive.CurrentUser, regpath);
                if (r != null)
                    MessageBox.Show("Exists");
                else
                    MessageBox.Show("No Exists");*/
                object ret = Registry.GetValue(regpath, "ExistTest", null);
                /*if (ret != null && ret.ToString() != "Test Failed")
                    MessageBox.Show("Exists, " + ret.ToString());
                else
                    MessageBox.Show("No Exists");*/
                if (ret == null)
                {
                    //When the Registry does not exist
                    Registry.SetValue(regpath, "ExistTest", 1, RegistryValueKind.DWord);
                }
                //MessageBox.Show((string)Registry.GetValue(regpath, "Inclusions", "FAIL"));
                string s = "";
                foreach (string a in (string[])Registry.GetValue(regpath, "Inclusions", null))
                    s += (a + "\n");
                MessageBox.Show(s);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to checked Registry\n" + ex.Message);
            }
            //

            Thread thread = new Thread(new ThreadStart(ThreaderStart));
            InitializeComponent();
            if (MessageBox.Show("Scan for installed Steam games?", "Continue?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                this._scannedSteam = true;
                thread.Start();
                _gamesList.AddRange(Scan());
            }
            //Reads the three config files to make needed changes
            ReadGameConfigs();
            //Called to display games
            ListGames(_gamesList.ToArray());
            thread.Abort();
        }
        private void ThreaderStart()
        {
            //Loading animation used while scanning
            _loadingForm.ShowDialog();
        }
        //Reads through the three config files for user-made changes
        private void ReadGameConfigs()
        {
            try
            {
                //Makes sure all subfolders exist
                GenSubfolders(_gameInclusionFile);
                //Only runs if inclusions file exists
                if (File.Exists(_gameInclusionFile))
                {
                    StreamReader sr = new StreamReader(File.Open(_gameInclusionFile, FileMode.Open));
                    //Used for storing data while reading file
                    Manifest ma = new Manifest();
                    //Infinite loop to read through file one line at a time
                    for (; ; )
                    {
                        string s = sr.ReadLine();
                        //Breaks at end of file
                        if (s == null || s == "") break;
                        string[] arr = s.Split('"');
                        //If you have read to the end of the details for one specific game
                        if (arr[0].Trim() == "}")
                        {
                            //Save game to _gamesList
                            _gamesList.Add(ma);
                            //Resent the Manifest for next game
                            ma = new Manifest();
                        }
                        //As long as this isn't denoting the start of a game's details
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
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to load games from saved list:\n" + e.Message);
            }
            //
            //Sort _gamesList before modifying, for pretty much no reason but my own desire
            _gamesList.Sort((x, y) => x.name.CompareTo(y.name));
            //Modify any games based on the Modifications file
            try
            {
                //Makes sure modifications List is empty (avoids duplications caused by loading the modifications Form and exiting by saving even though there were no changes made)
                this._gameMods = new List<Manifest[]>();
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
                    //Loops through _gameMods to modify the games listing
                    foreach (Manifest[] ma in this._gameMods.ToArray())
                    {
                        //First tries to remove the game that doesn't have the modifications from _gameList
                        //True if the game was found and removed, false if not
                        bool found = this._gamesList.Remove(ma[0]);
                        //Only adds the modified game details if the old ones were found
                        //Prevents listing a Steam game when the user did not scan for Steam games when launching
                        if (found)
                        {
                            //Adds in the new game details
                            this._gamesList.Add(ma[1]);
                        }
                    }
                }
            }
            catch (Exception e) { MessageBox.Show("Failed to load modification list:\n" + e.Message); }
            //
            //Remove excluded Steam games
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
                foreach (Manifest m in _exclusionsList.ToArray())
                {
                    _gamesList.Remove(m);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to load list of Steam game exclusions:\n" + e.Message);
            }
            //
            //Sort the list of games to make the final listing alphabetically sorted
            _gamesList.Sort((x, y) => x.name.CompareTo(y.name));
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
            //Saves the state of the 'Close on Launch' check box
            Config config = new Config();
            config.CloseOnLaunch = closeCheckBox.Checked;
            config.Save();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //isExit is used for the check box 'Close on launch' to close launcher without prompt
            //MessageBox is a prompt to ask the user if they want to cancel the exit
            bool t = (e.Cancel = !(this.isExit || MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButtons.YesNo) == DialogResult.Yes));
            //If the exit was not cancelled
            if (!t)
            {
                //Saves the local games the user added
                AddSavedGames();
                //Saves any game exclusions the user made
                AddExcludedGames();
                //Saves any game detail modifications the user made
                AddGameModifications();
            }
        }
        //Saves local launch games to config file
        private void AddSavedGames()
        {
            try
            {
                //Makes sure all subfolders exist to help prevent the StreamWriter from throwing an exception
                GenSubfolders(_gameInclusionFile);
                //Used to avoid the process of checking what games are already listed in the file by removing the file
                if (File.Exists(_gameInclusionFile)) File.Delete(_gameInclusionFile);
                //Creates file to store list of local launch games
                StreamWriter sw = new StreamWriter(File.Open(_gameInclusionFile, FileMode.OpenOrCreate));
                //Loops through all games listed
                foreach (Manifest m in this._gamesList)
                {
                    //Only acts on the games that do not launch through Steam
                    if (!m.steamLaunch)
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
                MessageBox.Show("Failed to save added games:\n" + e.Message);
            }
        }
        //Saves excluded Steam games to config file
        private void AddExcludedGames()
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
                foreach (Manifest m in this._exclusionsList)
                {
                    //Only acts on the games that launch through Steam
                    if (m.steamLaunch)
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
        //Saves changes made to listings
        private void AddGameModifications()
        {
            //Used to eliminate chains of changes to a single game
            //Example: 'a' -> 'ab', 'ab' -> 'ba'; results in 'a' -> 'ba'
            this._gameMods = ElimChains(this._gameMods);
            try
            {
                //Makes sure all subfolders exist to help prevent the StreamWriter from throwing an exception
                GenSubfolders(_gameModsFile);
                //Used to avoid the process of checking what games are already listed in the file by removing the file
                if (File.Exists(_gameModsFile)) File.Delete(_gameModsFile);
                //Creates file to store list of detail modifications
                StreamWriter sw = new StreamWriter(File.Open(_gameModsFile, FileMode.OpenOrCreate));
                //Loops through games stored int the list of modifications
                foreach (Manifest[] m in _gameMods)
                {
                    //If the game was a Steam launch either before or after the modification (avoiding writing local-launch games to this file, as their changes will be written to the inclusions file)
                    if (m[0].steamLaunch || m[1].steamLaunch)
                    {
                        //Writes details of modification
                        //m[0] is the old details, m[1] is the new details
                        sw.WriteLine("{");
                        sw.WriteLine("\t\"name\":\"{0}\"", m[0].name);
                        sw.WriteLine("\t\"path\":\"{0}\"", m[0].path);
                        sw.WriteLine("\t\"steam\":\"{0}\"", m[0].steamLaunch);
                        sw.WriteLine("\t\"shortcut\":\"{0}\"", m[0].useShortcut);
                        sw.WriteLine(";");
                        sw.WriteLine("\t\"name\":\"{0}\"", m[1].name);
                        sw.WriteLine("\t\"path\":\"{0}\"", m[1].path);
                        sw.WriteLine("\t\"steam\":\"{0}\"", m[1].steamLaunch);
                        sw.WriteLine("\t\"shortcut\":\"{0}\"", m[1].useShortcut);
                        sw.WriteLine("}");
                    }
                }
                sw.Close();
                sw.Dispose();
            }
            catch (Exception e) { MessageBox.Show("Failed to save edits to games in listing:\n" + e.Message); }
        }
        //Eliminates chains of changes to a single game
        //Example: 'a' -> 'ab', 'ab' -> 'ba'; results in 'a' -> 'ba'
        private List<Manifest[]> ElimChains(List<Manifest[]> l)
        {
            //Easier (in my experience) to deal with arrays vs. lists
            Manifest[][] m = l.ToArray();
            //The List of values to be returned
            List<Manifest[]> r = new List<Manifest[]>();
            //Bool to track if there were any chains found and fixed
            //Used to end recursive element
            bool b = false;
            //Loops through each change that was given in the input
            for (int i = 0; i < m.Length; i++)
            {
                //If it's the first change, just add it to the return list
                if (i == 0)
                    r.Add(m[i]);
                else
                {
                    //If the 'old' details of the current change are the same as the 'new' details of the previous change, this is a link in a chain and needs to be fixed
                    if (m[i][0] == m[i - 1][1])
                    {
                        //A chain has been found
                        b = true;
                        //Remove the previous change from the return list to prevent duplicates
                        r.Remove(r.Last());
                        //Add the 'old' details of the previous change and the 'new' details of the current change to the return list, eliminating the link between the two and merging them into a single change
                        r.Add(new Manifest[] { m[i - 1][0], m[i][1] });
                    }
                    else //If there's no link found, just add the current change to the return list
                        r.Add(m[i]);
                }
            }
            //If there were any links found, recursively run the method again to check for any remaining chains, though this may be unnecessary, otherwise just return the list that won't contain direct chains
            if (b)
                return ElimChains(r);
            else
                return r;
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

        //App Manifest Data
        public struct Manifest
        {
            public string name;
            public string path;
            public bool steamLaunch;
            public bool useShortcut;
            //Easy way to print out details of game Manifest, mostly for debugging/testing
            public override string ToString()
            {
                return "[name:" + name + ", path:" + path + ", steamLaunch:" + steamLaunch + ", useShortcut:" + useShortcut + "]";
            }
            //
            public static bool operator ==(Manifest a, Manifest b)
            {
                return a.name == b.name && a.path == b.path && a.steamLaunch == b.steamLaunch && a.useShortcut == b.useShortcut;
            }
            public static bool operator !=(Manifest a, Manifest b)
            {
                return a.name != b.name || a.path != b.path || a.steamLaunch != b.steamLaunch || a.useShortcut != b.useShortcut;
            }
        }
        //Scans paths for games
        private Manifest[] Scan()
        {
            //List of games to be returned
            List<Manifest> manifests = new List<Manifest>();
            //Gets the value that is held in the Registry to find out where Steam is located
            object ret = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", "NO VAL");
            //If Steam install path is not found in registry, the user will have to input the path manually
            if (ret == null || (string)ret == "NO VAL") { MessageBox.Show("Steam is not installed as expected, unable to continue."); return null; }
            //Scan default games dir
            string defPath = (string)ret + "\\steamapps";
            //Gets full path to each Steam app manifest file in the default game directory
            string[] sa = Directory.GetFiles(defPath, "*.acf");
            //As long as there is at least one file in that directory
            if (sa.Length > 0)
            {
                //Loops through all found app manifest files
                foreach (string s in sa)
                {
                    try
                    {
                        //Used to store data of the current game
                        Manifest manifest = new Manifest();
                        StreamReader sr = new StreamReader(File.OpenRead(s));
                        //Padding to reach the Steam id of the game
                        sr.ReadLine(); sr.ReadLine();
                        string[] t = sr.ReadLine().Split('"');
                        //Saves the Steam id of the game
                        manifest.path = t[t.Length - 2];
                        //Padding to reach the name of the game
                        sr.ReadLine();
                        t = null; t = sr.ReadLine().Split('"');
                        //Saves the name of the game
                        manifest.name = t[t.Length - 2];
                        manifest.steamLaunch = true;
                        sr.Close();
                        sr.Dispose();
                        //Sets useShortcut to false since launching will not need to use a shortcut for any reason
                        manifest.useShortcut = false;
                        //Adds the game to the list of games to be returned if it is not already in the list
                        //Should always evaluate true
                        if (!manifests.Contains(manifest)) manifests.Add(manifest);
                    }
                    catch (Exception e) { MessageBox.Show("Exception while scanning Steam default directory:\n" + e.Message); }
                }
            }
            //Scan user created game dirs
            try
            {
                //Path to Steam's file containing paths to user-created library folders
                string libraryFoldersFile = (string)ret + "\\steamapps\\libraryfolders.vdf";
                //List of user-created library folders to be scanned
                List<string> libraryFolders = new List<string>();
                StreamReader streamReader = new StreamReader(File.OpenRead(libraryFoldersFile));
                //Padding to reach the first listed library folder
                streamReader.ReadLine(); streamReader.ReadLine(); streamReader.ReadLine(); streamReader.ReadLine();
                //Infinite loop to read to end of file
                for (; ; )
                {
                    string temp = streamReader.ReadLine();
                    //Breaks at end of file
                    if (temp == null || temp == "" || temp == "}") break;
                    string[] t = temp.Split('"');
                    //Saves the path of the library folder
                    libraryFolders.Add(t[t.Length - 2]);
                }
                streamReader.Close();
                streamReader.Dispose();
                //Loops through each user-created library folder
                foreach (string folder in libraryFolders.ToArray<string>())
                {
                    //Directory where the Steam app manifest files are
                    string path = folder + "\\steamapps";
                    //Gets all app manifest files in the directory
                    string[] files = Directory.GetFiles(path, "*.acf");
                    //Loops through each file to read each game
                    foreach (string file in files)
                    {
                        //Used to hold data for the game
                        Manifest manifest = new Manifest();
                        StreamReader sr = new StreamReader(File.OpenRead(file));
                        //Padding to reach the Steam application id
                        sr.ReadLine();sr.ReadLine();
                        string[] t = sr.ReadLine().Split('"');
                        //Saves app id
                        manifest.path = t[t.Length - 2];
                        //Padding to reach app name
                        sr.ReadLine();
                        t = null; t = sr.ReadLine().Split('"');
                        //Saves name
                        manifest.name = t[t.Length - 2];
                        manifest.steamLaunch = true;
                        sr.Close();
                        sr.Dispose();
                        //Sets useShortcut to false since launching will not need to use a shortcut for any reason
                        manifest.useShortcut = false;
                        //Adds the game to the list of games to be returned if it is not already in the list
                        //Should always evaluate true
                        if (!manifests.Contains(manifest)) manifests.Add(manifest);
                    }
                }
            }
            catch (Exception e) { MessageBox.Show("Failed loading games from user-created Steam library folders:\n" + e.Message); }
            //Sorts the found Games in alphabetical order
            manifests.Sort((x, y) => x.name.CompareTo(y.name));
            //Returns the list of all found games
            return manifests.ToArray<Manifest>();
        }

        //Default height value used to arrange the game listings in the panel
        private const int def_location_height = -86;
        //Location the listings are currently att
        private int[] location = new int[] { 3, -83 };
        //Lists out games in the Manifest[] m parameter
        private void ListGames(Manifest[] m)
        {
            //Resets the height of the listing
            location[1] = def_location_height;
            //Clears the panel of all previously listed games
            panel1.Controls.Clear();
            //Checks if the parameter 'm' is empty
            if (m == null || m.Count() == 0) return;
            //Resets auto scroll position to prevent strange height values
            //Comment this line out to find out what I mean. Scan for games, scroll some way down, then change a game's details or add a game so that the list is regenerated
            panel1.AutoScrollPosition = new Point(0, 0);
            //Loops through each manifest in m
            for (int i = 0; i < m.Length; i++)
            {
                //Grouping is the class that holds all the data for each listed game
                Grouping group = new Grouping(m[i], this);

                //Grouping group = new Grouping(m[i].name, m[i].path, m[i].steamLaunch, m[i].useShortcut, this);
                
                //If i modulo 4 returns 0:
                //True: The next game should be listed on the next row, add 86 (the height of the Grouping with padding) to the current Y value
                //False: The next game should be listed on the same row, the X value is 236 (width of the Grouping with padding) multiplied my the remainder when dividing i by 4
                group.Location = (i % 4 == 0 ? new Point(location[0], location[1] += 86) : new Point(location[0] + (236 * (i % 4)), location[1]));
                //Add the Grouping to the panel's controls
                panel1.Controls.Add(group/*.Group*/);
                //If there is enough games listed for there to be a need for a scroll bar, adjust the size of the panel to fit the scroll bar
                if (i > 24 && panel1.Size.Width != 960) panel1.Size = new Size(960, panel1.Size.Height);
            }
        }

        //Restarts application
        //Probably to be removed, it is mostly for easy testing
        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.closeCheckBox.Checked = new Config().CloseOnLaunch;
        }

        //Saves changes to the 'Close on Launch' setting
        private void closeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Config config = new Config();
            config.CloseOnLaunch = closeCheckBox.Checked;
            config.Save();
        }

        //Called by 'Add Game' button in menu strip
        private void addGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new AddGameForm())
            {
                form.ShowDialog();
                //If the AddGameForm closed with the result of 'OK'
                if (form.DialogResult == DialogResult.OK)
                {
                    List<Manifest> manifests = new List<Manifest>();
                    //Saves each of the games added by the user
                    foreach (AddGameForm.Game g in form.Games)
                    {
                        manifests.Add(new Manifest
                        {
                            name = g.name,
                            path = g.path,
                            steamLaunch = g.isSteam,
                            useShortcut = g.useShortcut
                        });
                    }
                    //Adds games to the list of games to be displayed
                    this._gamesList.AddRange(manifests.ToArray<Manifest>());
                    //Sorts games list alphabetically
                    this._gamesList.Sort((x, y) => x.name.CompareTo(y.name));
                    //Re-lists the games
                    ListGames(this._gamesList.ToArray<Manifest>());
                }
            }
        }

        //Used by 'Grouping' class for Deleting games
        public void RemoveGame(Manifest manifest /*string name, string path, bool steam, bool shortcut*/)
        {
            //Generates the manifest for the game to be deleted
            Manifest torem = manifest; // new Manifest { name = name, path = path, steamLaunch = steam, useShortcut = shortcut };
            //Tries to remove the game from the games list and returns true if successful, false otherwise
            bool found = this._gamesList.Remove(torem);
            //If the game was found and removed
            if (found)
            {
                //Re-list the games
                ListGames(this._gamesList.ToArray());
                //Add the game to the exclusion list
                this._exclusionsList.Add(torem);
            }
            //If the game was not found in the games list, notify
            //Should never have to run this, since all listed games should always be in the games list
            else MessageBox.Show("Error removing game. Game not found in games list.");
        }

        //Used by 'Grouping' class for updating game info
        public void UpdateGame(Manifest oldMan, Manifest newMan)
        {
            //Tries to remove the game from the games list and returns true if successful, false otherwise
            //Reason for removal is I felt it was simpler than finding and editing the game in the list
            bool rem = this._gamesList.Remove(oldMan);
            //If the game was found and removed
            if (rem)
            {
                //Add the updated information to the games list
                this._gamesList.Add(newMan);
                //Add the old and new information to the list of game changes
                //this._gameMods.Add(new Manifest[] { oldMan, newMan });

                //Compare any changes to the Steam launch setting for the game
                if (oldMan.steamLaunch && !newMan.steamLaunch)
                {
                    //Was a Steam launch but is now a local launch
                    //Exclude game from Steam game scanning
                    this._exclusionsList.Add(oldMan);
                    //Add to the game list. Game will be seen as a local launch and saved to the inclusions file upon closing
                    //this._gamesList.Add(newMan);
                }
                else if (oldMan.steamLaunch && newMan.steamLaunch)
                {
                    this._gameMods.Add(new Manifest[] { oldMan, newMan });
                }
            }
            //If the game was not found
            //Should never run this, since all listed games should always be in games list
            else
            {
                MessageBox.Show("Update failed.");
            }
            //Sorts games list alphabetically
            this._gamesList.Sort((x, y) => x.name.CompareTo(y.name));
            //Re-lists games
            ListGames(this._gamesList.ToArray());
        }

        //Called by clicking the 'Exclusions' button on the tool strip
        private void exclusionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Are there any games that have been excluded?
            //If so, show the form
            if (this._exclusionsList.Count > 0)
            {
                //Saves the list of excluded games
                AddExcludedGames();
                //Using the form that will display all the excluded games
                using (var form = new ExclusionsForm())
                {
                    //Shows the Form
                    DialogResult res = form.ShowDialog();
                    //Debugging, to be removed
                    MessageBox.Show(res.ToString());
                    //If the Form exited by the user clicking 'Confirm'
                    if (res == DialogResult.OK)
                    {
                        //Clears the list of excluded games so it can be read again without creating duplicates
                        this._exclusionsList = new List<Manifest>();
                        //Clears the list of games to prevent duplicates
                        this._gamesList = new List<Manifest>();
                        //Thread to display loading animation incase of Steam game scanning
                        Thread thread = new Thread(new ThreadStart(ThreaderStart));
                        //Did we scan for Steam games when initially launching? If so, scan them again now
                        if (this._scannedSteam)
                        {
                            //Displays loading animation while loading
                            thread.Start();
                            //Scans for Steam games
                            this._gamesList.AddRange(Scan());
                        }
                        //Reads through the inclusions, exclusions, and changes files 
                        ReadGameConfigs();
                        //Lists the games
                        ListGames(this._gamesList.ToArray());
                        //Aborts the loading animation thread. If it was never started, this line won't effect anything
                        thread.Abort();
                    }
                }
            }
            else //If there were no games excluded, there's no need to show the form, let the user know
                MessageBox.Show("There are no excluded games. Use the settings button on a game in the listing to exclude that game.");
            
            
        }

        //Called by clicking 'Changes' button on tool strip
        private void changesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Saves the current list of changes to games
            AddGameModifications();
            //Using the Form that will list all the changes saved
            using (ModificationsForm form = new ModificationsForm())
            {
                //Shows the form
                DialogResult res = form.ShowDialog();
                //Debugging, to be removed
                MessageBox.Show(res.ToString());
                //If the Form exited by the user pressing 'Confirm'
                if (res == DialogResult.OK)
                {
                    //Clears the list of games loaded to prevent duplicating games when listing
                    this._gamesList = new List<Manifest>();
                    //Gets the final list of changes from the modification Form
                    this._gameMods = form.Modifs;
                    //Saves the list of changes
                    AddGameModifications();
                    //Thread to display loading animation incase of Steam game scanning
                    Thread thread = new Thread(new ThreadStart(ThreaderStart));
                    //Did we scan for Steam games when initially launching? If so, scan them again now
                    if (this._scannedSteam)
                    {
                        //Displays loading animation while loading
                        thread.Start();
                        //Scans for Steam games
                        this._gamesList.AddRange(Scan());
                    }
                    //Reads through the inclusions, exclusions, and changes files 
                    ReadGameConfigs();
                    //Lists the games
                    ListGames(this._gamesList.ToArray());
                    //Aborts the loading animation thread. If it was never started, this line won't effect anything
                    thread.Abort();
                }
            }
        }

        /// <summary>
        /// Flags whether the launcher should close without a prompt (used for Close On Exit)
        /// </summary>
        /// <param name="b">If it should exit or not</param>
        /// <returns>True if the flag was set, False if the user tries to set the flag to what it already is</returns>
        public bool FlagExit(bool b)
        {
            bool r = !(b == this.isExit);
            this.isExit = b;
            return r;
        }
    }

    public class Grouping : GroupBox
    {
        //private GroupBox groupBox = new GroupBox();
        private PictureBox iconBox; // = new PictureBox();
        private PictureBox settingsBox; // = new PictureBox();
        private PictureBox launchBox; // = new PictureBox();
        //Path can either be the Steam launch id or the path to the executable file
        /*private string path = "";
        private string name = "";
        private bool isSteamLaunch = false;
        private bool useShortcut = false;*/
        //Used for making update and deletion calls
        //public Form1 parent;

        //New Declaration
        public Form1.Manifest Manifest { get; set; }
        private Form1 _parent;
        public Grouping(Form1.Manifest manifest, Form1 parent)
        {
            this._parent = parent;
            this.Manifest = manifest;
            InitializeComponent();
        }
        private void InitializeComponent()
        {
            this.iconBox = new PictureBox();
            this.settingsBox = new PictureBox();
            this.launchBox = new PictureBox();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(Grouping));
            int i = new Random().Next(1, 7);
            //
            this.SuspendLayout();
            //
            // iconBox
            //
            this.iconBox.BackgroundImage = (i % 2 == 0) ? Properties.Resources.Logo_n_inv : Properties.Resources.Logo_n;
            this.iconBox.BackgroundImageLayout = ImageLayout.Zoom;
            this.iconBox.Location = new Point(6, 19);
            this.iconBox.Name = "iconBox";
            this.iconBox.Size = new Size(50, 50);
            this.iconBox.TabStop = false;
            // 
            // settingsBox
            // 
            this.settingsBox.MouseClick += new MouseEventHandler(this.Settings_MouseDown);
            this.settingsBox.BackgroundImage = Properties.Resources.Settings;
            this.settingsBox.BackgroundImageLayout = ImageLayout.Zoom;
            this.settingsBox.Location = new Point(62, 19);
            this.settingsBox.Name = "settingsBox";
            this.settingsBox.Size = new Size(50, 50);
            this.settingsBox.TabStop = false;
            // 
            // launchBox
            // 
            this.launchBox.BackgroundImage = Properties.Resources.Play;
            this.launchBox.BackgroundImageLayout = ImageLayout.Zoom;
            this.launchBox.Location = new Point(118, 19);
            this.launchBox.Name = "launchBox";
            this.launchBox.Size = new Size(50, 50);
            this.launchBox.TabStop = false;
            this.launchBox.Click += new EventHandler(this.Start_Click);
            //
            // Grouping
            //
            this.Controls.Add(this.iconBox);
            this.Controls.Add(this.settingsBox);
            this.Controls.Add(this.launchBox);
            this.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            this.Name = "groupBox";
            this.Size = new Size(230, 80);
            this.TabStop = false;
            this.Text = Truncate(this.Manifest.name, 22);
            //
            this.ResumeLayout();
            this.PerformLayout();
        }
        //

        //Called when the launch button is clicked
        private void Start_Click(object sender, EventArgs e)
        {
            try
            {
                //If the game is a Steam launch, use Windows Explorer to call the Steam run game command
                if (this.Manifest.steamLaunch)
                {
                    System.Diagnostics.Process.Start("explorer.exe", "steam://rungameid/" + this.Manifest.path);
                }
                else
                {
                    //If the game should launch using a shortcut
                    //Shortcuts can help with games that are programmed to use a working directory to search for their files. If a game is failing to launch, try to launch it with the shortcut
                    if (this.Manifest.useShortcut)
                    {
                        //Uses IWshRuntimeLibrary to make the shortcuts
                        IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                        //Path to the folder the launcher's executable is in, combined with the name of the shortcut
                        string spath = Path.Combine(Application.StartupPath, "temp.lnk");
                        //Creates the shortcut
                        IWshRuntimeLibrary.IWshShortcut wsh = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(spath);
                        //Sets the launch path for the shortcut
                        wsh.TargetPath = this.Manifest.path;
                        string[] t = this.Manifest.path.Split('\\');
                        string tpath = "";
                        //Gets the full path of the executable, except for the executable file itself, leaving the folder that will be used as the working directory
                        for (int i = 0; i < t.Length - 1; i++)
                            if (i == 0)
                                tpath += t[i];
                            else
                                tpath += "\\" + t[i];
                        //Sets the working directory
                        wsh.WorkingDirectory = tpath;
                        wsh.Save();
                        //Launches the game through the shortcut
                        System.Diagnostics.Process.Start(spath);
                    }
                    else
                    {
                        //Launches the game
                        System.Diagnostics.Process.Start(this.Manifest.path);
                    }
                }
                this._parent.FlagExit(new Config().CloseOnLaunch);
                if (new Config().CloseOnLaunch)
                    Application.Exit();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        //Called when a mouse button is clicked on the Settings button
        private void Settings_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                //If it is the left mouse button, call Settings_App
                case MouseButtons.Left:
                    Settings_App(sender, null);
                    break;
                //If it is the right mouse button, pull up a menu to choose between the external settings app and the shortcut settings
                case MouseButtons.Right:
                    ContextMenu cm = new ContextMenu();
                    cm.MenuItems.Add("Shortcut Settings", new EventHandler(this.Short));
                    cm.MenuItems.Add("Settings Application", new EventHandler(this.Settings_App));
                    cm.Show(this, new Point(e.X, e.Y));
                    break;
            }
        }
        //Used to launch external game settings apps based on which game it is, defaults to the launcher shortcut settings for that game
        private void Settings_App(object sender, EventArgs e)
        {
            switch (this.Manifest.path)
            {
                //If there is no path for some reason, default
                case "":
                    Short(sender, e);
                    break;
                //If the path is the Steam game id for Subnautica, try to launch the external app for it
                case "264710": //Subnautica
                    //The external app saves its most recent path to a file in the Appdata\Roaming folder, check if that exists
                    if (File.Exists(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "Subnautica Options"), "Subnautica Options.txt")))
                    {
                        try
                        {
                            //Reads the path from the file
                            StreamReader sr = new StreamReader(File.OpenRead(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "Subnautica Options"), "Subnautica Options.txt")));
                            string path = sr.ReadLine();
                            sr.Close();
                            sr.Dispose();
                            //Launch the application at the path found with the argument 'nolaunch' to hide the launch button in the application
                            System.Diagnostics.Process.Start(path, "nolaunch");
                        }
                        //Incase of any failure, default to shortcut settings
                        catch { Short(sender, e); }
                    }
                    //Default to shortcut settings
                    else
                        Short(sender, e);
                    break;
                //If the path is the Steam game id for Terraria, try to launch the external app for it
                case "105600": //Terraria
                    //The external app saves its most recent path to a file in the Appdata\Roaming folder, check if that exists
                    if (File.Exists(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "Terraria Options"), "Terraria Options.txt")))
                    {
                        try
                        {
                            //Reads the path from the file
                            StreamReader sr = new StreamReader(File.OpenRead(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "Terraria Options"), "Terraria Options.txt")));
                            string path = sr.ReadLine();
                            sr.Close();
                            sr.Dispose();
                            //Launch the application at the path found with the argument 'nolaunch' to hide the launch button in the application
                            System.Diagnostics.Process.Start(path, "nolaunch");
                        }
                        //Incase of any failure, default to shortcut settings
                        catch { Short(sender, e); }
                    }
                    //Default to shortcut settings
                    else
                        Short(sender, e);
                    break;
                //Default to shortcut settings
                default:
                    Short(sender, e);
                    break;
            }
        }
        //Displays the shortcut settings form
        private void Short(object sender, EventArgs e)
        {
            using (var form = new ShortcutSettings(this.Manifest))
            {
                DialogResult res = form.ShowDialog();
                //If the changes were confirmed, update the game details
                if (form.DialogResult == DialogResult.OK)
                {
                    //Old details
                    Form1.Manifest oldMan = this.Manifest;
                    //Updated details
                    Form1.Manifest newMan = new Form1.Manifest
                    {
                        name = form.GameName,
                        path = form.GamePath,
                        steamLaunch = form.IsSteamLaunch,
                        useShortcut = form.UseShortcut
                    };
                    this.Manifest = newMan;
                    this.Text = Truncate(form.GameName, 22);
                    //Calls Form1.UpdateGame to update this game's details
                    this._parent.UpdateGame(oldMan, newMan);
                }
                //If the Delete button was clicked
                else if (form.DialogResult == DialogResult.Abort)
                {
                    //Call Form1.RemoveGame to delete game from listing
                    this._parent.RemoveGame(this.Manifest);
                }
            }
        }

        //Adds an elipses ('...') to the end of the name if it is too long to fit in place
        private static string Truncate(string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }

        //Returns the GroupBox control
        //public GroupBox Group { get { return this/*.groupBox*/; } }
    }
}
