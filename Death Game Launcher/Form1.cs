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

namespace Death_Game_Launcher
{
    public partial class Form1 : Form
    {
        public static int isExit = 0;
        private List<Manifest> _gamesList = new List<Manifest>();
        private List<Manifest> _exclusionsList = new List<Manifest>();
        private LoadingForm _loadingForm = new LoadingForm();
        private readonly string _gameInclusionFile = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "DLG"), "Inclusions.cfg");
        private readonly string _gameExclusionFile = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "DLG"), "Exclusions.cfg");

        public Form1()
        {
            Thread thread = new Thread(new ThreadStart(ThreaderStart));
            InitializeComponent();
            if (MessageBox.Show("Scan for installed Steam games?", "Continue?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                thread.Start();
                _gamesList.AddRange(Scan());
            }
            ReadGameConfigs();
            ListGames(_gamesList.ToArray());
            thread.Abort();
        }
        private void ThreaderStart()
        {
            _loadingForm.ShowDialog();
        }
        private void ReadGameConfigs()
        {
            try
            {
                GenSubfolders(_gameInclusionFile);
                if (File.Exists(_gameInclusionFile))
                {
                    StreamReader sr = new StreamReader(File.Open(_gameInclusionFile, FileMode.Open));
                    Manifest ma = new Manifest();
                    for (; ; )
                    {
                        string s = sr.ReadLine();
                        if (s == null || s == "") break;
                        string[] arr = s.Split('"');

                        if (arr[0].Trim() == "}")
                        {
                            _gamesList.Add(ma);
                            ma = new Manifest();
                        }
                        else if (arr[0].Trim() != "{")
                        {
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
            //Remove excluded Steam games
            try
            {
                GenSubfolders(_gameExclusionFile);
                if (File.Exists(_gameExclusionFile))
                {
                    StreamReader sr = new StreamReader(File.Open(_gameExclusionFile, FileMode.Open));
                    Manifest ma = new Manifest();
                    for (; ; )
                    {
                        string s = sr.ReadLine();
                        if (s == null || s == "") break;
                        string[] arr = s.Split('"');

                        if (arr[0].Trim() == "}")
                        {
                            _exclusionsList.Add(ma);
                            ma = new Manifest();
                        }
                        else if (arr[0].Trim() != "{")
                        {
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
            Config config = new Config();
            config.CloseOnLaunch = closeCheckBox.Checked;
            config.Save();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //isExit is used for the check box 'Close on launch' to close launcher without prompt
            //MessageBox is a prompt to ask the user if they want to cancel the exit
            bool t = (e.Cancel = !(isExit == 1 || MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButtons.YesNo) == DialogResult.Yes));
            //Saves the local games the user added
            if (!t) { AddSavedGames(); AddExcludedGames(); }
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
        private struct Manifest
        {
            public string name;
            public string path;
            public bool steamLaunch;
            public bool useShortcut;
        }
        //Scans paths for games
        private Manifest[] Scan()
        {
            List<Manifest> manifests = new List<Manifest>();

            object ret = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", "NO VAL");
            if (ret == null || (string)ret == "NO VAL") { MessageBox.Show("Steam is not installed as expected, unable to continue."); return null; }
            //Scan default games dir
            string defPath = (string)ret + "\\steamapps";
            string[] sa = Directory.GetFiles(defPath, "*.acf");
            if (sa.Length > 0)
            {
                //Add the Manifests
                foreach (string s in sa)
                {
                    try
                    {
                        Manifest manifest = new Manifest();
                        StreamReader sr = new StreamReader(System.IO.File.OpenRead(s));
                        sr.ReadLine(); sr.ReadLine();
                        string[] t = sr.ReadLine().Split('"');
                        manifest.path = t[t.Length - 2];
                        sr.ReadLine();
                        t = null; t = sr.ReadLine().Split('"');
                        manifest.name = t[t.Length - 2];
                        manifest.steamLaunch = true;
                        sr.Close();
                        sr.Dispose();
                        manifest.useShortcut = false;
                        if (!manifests.Contains(manifest)) manifests.Add(manifest);
                    }
                    catch { }
                }
            }
            //Scan user created game dirs
            try
            {
                string libraryFoldersFile = (string)ret + "\\steamapps\\libraryfolders.vdf";
                List<string> libraryFolders = new List<string>();
                StreamReader streamReader = new StreamReader(System.IO.File.OpenRead(libraryFoldersFile));
                streamReader.ReadLine(); streamReader.ReadLine(); streamReader.ReadLine(); streamReader.ReadLine();
                for (; ; )
                {
                    string temp = streamReader.ReadLine();
                    if (temp == null || temp == "" || temp == "}") break;
                    string[] t = temp.Split('"');
                    libraryFolders.Add(t[t.Length - 2]);
                }
                streamReader.Close();
                streamReader.Dispose();
                foreach (string folder in libraryFolders.ToArray<string>())
                {
                    string path = folder + "\\steamapps";
                    string[] files = Directory.GetFiles(path, "*.acf");
                    foreach (string file in files)
                    {
                        Manifest manifest = new Manifest();
                        StreamReader sr = new StreamReader(System.IO.File.OpenRead(file));
                        sr.ReadLine();sr.ReadLine();
                        string[] t = sr.ReadLine().Split('"');
                        manifest.path = t[t.Length - 2];
                        sr.ReadLine();
                        t = null; t = sr.ReadLine().Split('"');
                        manifest.name = t[t.Length - 2];
                        manifest.steamLaunch = true;
                        sr.Close();
                        sr.Dispose();
                        manifest.useShortcut = false;
                        if (!manifests.Contains(manifest)) manifests.Add(manifest);
                    }
                }
            }
            catch { }
            manifests.Sort((x, y) => x.name.CompareTo(y.name));
            return manifests.ToArray<Manifest>();
        }

        private readonly int[] def_location = new int[] { 3, -83 };
        private readonly int def_location_height = -86;
        private int[] location = new int[] { 3, -83 };
        private void ListGames(Manifest[] m)
        {
            location[1] = def_location_height;
            panel1.Controls.Clear();
            if (m == null || m.Count() == 0) return;
            panel1.AutoScrollPosition = new Point(0, 0);
            for (int i = 0; i < m.Length; i++)
            {
                Grouping group = new Grouping(m[i].name, m[i].path, m[i].steamLaunch, m[i].useShortcut, this);
                group.Location = (i % 4 == 0 ? new Point(location[0], location[1] += 86) : new Point(location[0] + (236 * (i % 4)), location[1]));
                panel1.Controls.Add(group.Group);
                if (i > 24 && panel1.Size.Width != 960) panel1.Size = new Size(960, panel1.Size.Height);
            }
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.closeCheckBox.Checked = new Config().CloseOnLaunch;
        }

        public static string Truncate(string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }

        private void closeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Config config = new Config();
            config.CloseOnLaunch = closeCheckBox.Checked;
            config.Save();
        }

        private void addGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new AddGameForm())
            {
                form.ShowDialog();
                if (form.DialogResult == DialogResult.OK)
                {
                    List<Manifest> manifests = new List<Manifest>();
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
                    this._gamesList.AddRange(manifests.ToArray<Manifest>());
                    this._gamesList.Sort((x, y) => x.name.CompareTo(y.name));
                    ListGames(this._gamesList.ToArray<Manifest>());
                }
            }
        }

        //Used for Deleting games
        public void RemoveGame(string name, string path, bool steam, bool shortcut)
        {
            Manifest torem = new Manifest { name = name, path = path, steamLaunch = steam, useShortcut = shortcut };
            bool found = this._gamesList.Remove(torem);
            if (found)
            {
                ListGames(this._gamesList.ToArray());
                this._exclusionsList.Add(torem);
            }
            else MessageBox.Show("Error removing game. Game not found in games list.");
        }
    }

    public class Grouping
    {
        private GroupBox groupBox = new GroupBox();
        private PictureBox iconBox = new PictureBox();
        private PictureBox settingsBox = new PictureBox();
        private PictureBox launchBox = new PictureBox();
        private string path = "";
        private string name = "";
        private bool isSteamLaunch = false;
        private bool useShortcut = false;
        public Form1 parent;

        public Grouping(string name, string path, bool steamLaunch, bool shortcut, Form1 parent)
        {
            this.parent = parent;
            this.path = path;
            this.name = name;
            this.isSteamLaunch = steamLaunch;
            this.useShortcut = shortcut;
            int i = new Random().Next(1, 7);
            ComponentResourceManager resources = new ComponentResourceManager(typeof(Form1));
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
            // groupBox
            // 
            this.groupBox.Controls.Add(this.iconBox);
            this.groupBox.Controls.Add(this.settingsBox);
            this.groupBox.Controls.Add(this.launchBox);
            this.groupBox.Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new Size(230, 80);
            this.groupBox.TabStop = false;
            this.groupBox.Text = Truncate(name, 22);

            //Add circle border to Play Button
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, this.launchBox.Size.Width, this.launchBox.Size.Height);
            
        }
        public Point Location
        {
            get { return this.groupBox.Location; }
            set { this.groupBox.Location = value; }
        }

        private void Start_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.isSteamLaunch)
                {
                    System.Diagnostics.Process.Start("explorer.exe", "steam://rungameid/" + path);
                }
                else
                {
                    if (this.useShortcut)
                    {
                        IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                        string spath = Path.Combine(Application.StartupPath, "temp.lnk");
                        IWshRuntimeLibrary.IWshShortcut wsh = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(spath);
                        wsh.TargetPath = this.path;
                        string[] t = path.Split('\\');
                        string tpath = "";
                        for (int i = 0; i < t.Length - 1; i++)
                            if (i == 0)
                                tpath += t[i];
                            else
                                tpath += "\\" + t[i];
                        wsh.WorkingDirectory = tpath;
                        wsh.Save();
                        System.Diagnostics.Process.Start(spath);
                    }
                    else
                    {
                        DialogResult result = MessageBox.Show("Launch with a shortcut instead of exe?", "", MessageBoxButtons.YesNoCancel);
                        if (result == DialogResult.Yes)
                        {
                            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                            string spath = Path.Combine(Application.StartupPath, "temp.lnk");
                            IWshRuntimeLibrary.IWshShortcut wsh = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(spath);
                            wsh.TargetPath = this.path;
                            string[] t = path.Split('\\');
                            string tpath = "";
                            for (int i = 0; i < t.Length - 1; i++)
                                if (i == 0)
                                    tpath += t[i];
                                else
                                    tpath += "\\" + t[i];
                            wsh.WorkingDirectory = tpath;
                            wsh.Save();
                            System.Diagnostics.Process.Start(spath);
                        }
                        else if (result == DialogResult.Cancel)
                        {
                            return;
                        }
                        else
                        {
                            System.Diagnostics.Process.Start(path);
                        }
                    }
                }
                if (new Config().CloseOnLaunch)
                {
                    Form1.isExit = 1;
                    Application.Exit();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void Settings_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    Settings_App(sender, null);
                    break;
                case MouseButtons.Right:
                    ContextMenu cm = new ContextMenu();
                    cm.MenuItems.Add("Shortcut Settings", new EventHandler(this.Short));
                    cm.MenuItems.Add("Settings Application", new EventHandler(this.Settings_App));
                    cm.Show(this.Group, new Point(e.X, e.Y));
                    break;
            }
        }
        private void Settings_App(object sender, EventArgs e)
        {
            switch (this.path)
            {
                case "":
                    Short(sender, e);
                    break;
                case "264710": //Subnautica
                    if (File.Exists(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "Subnautica Options"), "Subnautica Options.txt")))
                    {
                        try
                        {
                            StreamReader sr = new StreamReader(File.OpenRead(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "Subnautica Options"), "Subnautica Options.txt")));
                            string path = sr.ReadLine();
                            sr.Close();
                            sr.Dispose();
                            System.Diagnostics.Process.Start(path, "nolaunch");
                        }
                        catch { Short(sender, e); }
                    }
                    else
                        Short(sender, e);
                    break;
                case "105600": //Terraria
                    if (File.Exists(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "Terraria Options"), "Terraria Options.txt")))
                    {
                        try
                        {
                            StreamReader sr = new StreamReader(File.OpenRead(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "Terraria Options"), "Terraria Options.txt")));
                            string path = sr.ReadLine();
                            sr.Close();
                            sr.Dispose();
                            System.Diagnostics.Process.Start(path, "nolaunch");
                        }
                        catch { Short(sender, e); }
                    }
                    else
                        Short(sender, e);
                    break;
                default:
                    Short(sender, e);
                    break;
            }
        }
        private void Short(object sender, EventArgs e)
        {
            using (var form = new ShortcutSettings(this.name, this.path, this.isSteamLaunch, this.useShortcut))
            {
                DialogResult res = form.ShowDialog();
                MessageBox.Show(form.DialogResult.ToString());
                if (form.DialogResult == DialogResult.OK)
                {
                    this.name = form.GameName;
                    this.path = form.GamePath;
                    this.isSteamLaunch = form.IsSteamLaunch;
                    this.useShortcut = form.UseShortcut;
                    this.groupBox.Text = Truncate(form.GameName, 22);
                }
                else if (form.DialogResult == DialogResult.Abort)
                {
                    //Call Deletion method in Form1
                    this.parent.RemoveGame(this.name, this.path, this.isSteamLaunch, this.useShortcut);
                }
            }
        }

        private static string Truncate(string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }

        public GroupBox Group { get { return this.groupBox; } }
    }
}
