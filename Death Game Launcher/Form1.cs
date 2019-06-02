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
//using Microsoft.Win32;
using System.Diagnostics;
using Newtonsoft.Json;

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
        //Registry path to application settings
        //private const string regpath = "HKEY_CURRENT_USER\\Software\\HoodedDeathApplications\\DeathGameLauncher";
        private readonly string _cfgPath;
        private Config cfg;

        //The thread worker class used to scan for Steam games
        ScanThreadWorker worker;

        Thread loadingThread;
        //Logger instance to be used for logging throughout the application
        public static Logger _log;

        public Form1()
        {
            InitializeComponent();
            //
            _cfgPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeathApplications");
            if (!Directory.Exists(_cfgPath))
                Directory.CreateDirectory(_cfgPath);
            _cfgPath = Path.Combine(_cfgPath, "DLG");
            if (!Directory.Exists(_cfgPath))
                Directory.CreateDirectory(_cfgPath);
            //_cfgPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeathApplications", "DLG");
            if (File.Exists(Path.Combine(_cfgPath, "config.cfg")))
            {
                StreamReader sr = new StreamReader(File.OpenRead(Path.Combine(_cfgPath, "config.cfg")));
                cfg = JsonConvert.DeserializeObject<Config>(sr.ReadToEnd());
                sr.Close();
                sr.Dispose();
            }
            else
                cfg = new Config()
                {
                    LogLvl = Logger.ERROR,
                    CloseOnLaunch = false
                };
            if (!File.Exists(Path.Combine(_cfgPath, "sapps.cfg")))
            {
                List<SAppJson> t = new List<SAppJson>(new SAppJson[]
                {
                    new SAppJson() { SApp = new SettingsApp()
                    {
                        Name = "Default",
                        Path = "",
                        Args = "",
                        HereByDefault = true
                    } }
                });
                StreamWriter sw = new StreamWriter(File.Open(Path.Combine(_cfgPath, "sapps.cfg"), FileMode.OpenOrCreate));
                sw.WriteLine(JsonConvert.SerializeObject(t, Formatting.Indented));
                sw.Flush();
                sw.Close();
                sw.Dispose();
            }
            //Initialize the Logger
            _log = new Logger(cfg.LogLvl, Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DLG-Log.txt"));
            _log.Debug("Logger initialized.");
            //If the user wants to scan for Steam games
            if (MessageBox.Show("Scan for installed Steam games?", "Continue?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _log.Debug("Starting Steam game scan.");
                //Show loading animation
                loadingThread = new Thread(new ThreadStart(TW));
                _log.Debug("Showing loading animation...");
                loadingThread.Start();
                //Used to tell other methods to scan for Steam games after certain events
                _scannedSteam = true;
                //Set the ThreadWorker to a new instance with the correct Event Handler
                _log.Debug("Declaring scanner thread worker.");
                worker = new ScanThreadWorker();
                worker.ThreadDone += HandleThreadDone;
                //New Thread to scan for Steam games
                _log.Debug("Declaring scanner thread.");
                Thread thread = new Thread(worker.Run)
                { Name = "SteamScanThread" };
                _log.Debug("Starting scanner thread.");
                //Start scanning
                thread.Start();
            }
            //Otherwise just set threadDone to true, since HandleThreadDone won't be triggered
            else
            {
                _log.Debug("Skipping Steam scan.");
                threadDone = true;
            }
            //Call method to list out the games
            WaitForThreadDone();
        }
        //Called to list out the found games
        private void WaitForThreadDone()
        {
            //Waits until the scanning Thread is done (if it ran at all)
            if (worker != null) _log.Debug("Waiting for scanning to finish");
            while (!threadDone) { }
            //Array to hold the games found by the scanning Thread
            Manifest[] manifests = new Manifest[0];
            //If the ThreadWorker was set (essentially if the scanning thread was ran)
            if (worker != null)
            {
                _log.Debug("Copying Steam scan results.");
                //Copy the array the ThreadWorker got into manifests
                manifests = worker.Value;
            }
            //If there are any Steam games found, add them to _gamesList
            if (manifests != null && manifests.Length > 0)
            {
                _log.Debug("Adding Steam scan results to games list.");
                _gamesList.AddRange(manifests);
            }
            //Calls ReadGames to add local games, remove exclusions, and makes changes to games
            _log.Debug("Reading in game configurations...");
            ReadGames();
            //Calls ListGames to list out all the games found
            _log.Debug("Displaying games found.");
            ListGames(_gamesList.ToArray());
            //Close the loading animation, if it was shown at all
            if (loadingThread != null)
            {
                _log.Debug("Killing loading animation.");
                loadingThread.Abort();
            }
            //Resets threadDone to false for the next time the method is called
            threadDone = false;
            //Resets worker to nothing for the next time it's needed
            worker = null;
        }
        //Whether or not WaitForThreadDone should wait or continue
        private bool threadDone = false;
        //EventHnadler for when the scanning thread finishes
        public void HandleThreadDone(object sender, EventArgs e)
        {
            //Sets threadDone to true so WaitForThreadDone will continue
            threadDone = true;
        }
        //Used to show the loading animation Form
        private void TW()
        {
            _loadingForm.ShowDialog();
        }
        //Reads through the user-made changes
        private void ReadGames()
        {
            _log.Debug("Attempting to read game inclusions...");
            //Inclusions
            try
            {
                _log.Debug("Fetching Inclusions.");
                //Return value for the Inclusions entry
                //string[] ret = (string[])Registry.GetValue(regpath, "Inclusions", null);
                string path = Path.Combine(_cfgPath, "inclusions.cfg");
                if (File.Exists(path))
                {
                    StreamReader sr = new StreamReader(File.OpenRead(path));
                    List<InclusionJson> inclusionsJ = JsonConvert.DeserializeObject<List<InclusionJson>>(sr.ReadToEnd());
                    sr.Close();
                    sr.Dispose();
                    foreach (InclusionJson ij in inclusionsJ)
                    {
                        this._gamesList.Add(new Manifest()
                        {
                            name = ij.Game.Name,
                            path = ij.Game.Path,
                            args = ij.Game.Args,
                            steamLaunch = ij.Game.SteamLaunch,
                            useShortcut = ij.Game.ShortcutLaunch,
                            useSettingsApp = ij.Game.UseSApp,
                            settingsApp = ParseSAppID(ij.Game.SApp)
                        });
                    }
                }
                //If there is a value found
                /*if (ret != null && ret.Length > 0)
                {
                    //Manifest to be added to _gamesList
                    Manifest manifest = new Manifest();
                    //Run through each line from the return value
                    _log.Debug("Reading through values...");
                    foreach (string s in ret)
                    {
                        string[] arr = s.Split('"');
                        switch (arr[0].Trim())
                        {
                            //Skips the opening bracket
                            case "{":
                                _log.Debug("Start of new game values.");
                                break;
                            //At each closing bracket, save the currently found game to _gamesList and reset manifest to a blank manifest
                            case "}":
                                _log.Debug("End of current game\'s values, saving.");
                                this._gamesList.Add(manifest);
                                manifest = new Manifest();
                                break;
                            //Sets the values for the manifest
                            default:
                                switch (arr[1].ToLower().Trim())
                                {
                                    case "name":
                                        _log.Debug("Current game's name is " + arr[arr.Length - 2]);
                                        manifest.name = arr[arr.Length - 2];
                                        break;
                                    case "path":
                                        _log.Debug("Current game's path is " + arr[arr.Length - 2]);
                                        manifest.path = arr[arr.Length - 2];
                                        break;
                                    case "args":
                                        _log.Debug("Current game's arguments are " + arr[arr.Length - 2]);
                                        manifest.args = arr[arr.Length - 2];
                                        break;
                                    case "steam":
                                        _log.Debug("Current game will" + (bool.Parse(arr[arr.Length - 2]) ? " " : " not ") + "launch through Steam.");
                                        manifest.steamLaunch = bool.Parse(arr[arr.Length - 2]);
                                        break;
                                    case "shortcut":
                                        _log.Debug("Current game will" + (bool.Parse(arr[arr.Length - 2]) ? " " : " not ") + "launch through a shortcut.");
                                        manifest.useShortcut = bool.Parse(arr[arr.Length - 2]);
                                        break;
                                    case "usesapp":
                                        _log.Debug("Current game will" + (bool.Parse(arr[arr.Length - 2]) ? " " : " not ") + "use an external settings app.");
                                        manifest.useSettingsApp = bool.Parse(arr[arr.Length - 2]);
                                        break;
                                    case "sapp":
                                        manifest.settingsApp = ParseSAppID(arr[arr.Length - 2]);
                                        break;
                                }
                                break;
                        }
                    }
                }
                else
                    _log.Debug("No Inclusions found.");*/
            }
            catch (Exception e)
            {
                //Log error from failing to load Inclusions
                _log.Error(e, "Failed to load games from saved list.");
                //MessageBox.Show("Failed to load games from saved list:\n" + e.Message);
            }
            //
            //Sort _gamesList before modifying, for pretty much no reason but my own desire
            _gamesList.Sort((x, y) => x.name.CompareTo(y.name));
            //Temporary variable to hold the list of modifications incase the try block fails after erasing the list
            List<Manifest[]> holdmod = this._gameMods;
            //Modify any games based on the Modifications file
            try
            {
                //Makes sure modifications List is empty (avoids duplications caused by loading the modifications Form and exiting by saving even though there were no changes made)
                this._gameMods = new List<Manifest[]>();
                //Return value from the Modifications entry
                //string[] ret = (string[])Registry.GetValue(regpath, "Modifications", null);
                string path = Path.Combine(_cfgPath, "modifications.cfg");
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
                //If the return value wasn't empty
                /*if (ret != null && ret.Length > 0)
                {
                    //Temporary variable for reading in the modifications
                    Manifest[] manifests = new Manifest[] { new Manifest(), new Manifest() };
                    //Used just to tell if we're reading the old details or the new details of the game
                    int i = 0;
                    //Loops through each line of the array returned from the registry
                    foreach (string s in ret)
                    {
                        string[] arr = s.Split('"');
                        switch (arr[0].Trim())
                        {
                            //Skips past opening bracket
                            case "{":
                                break;
                            //Increases i since ; signals these are the new details for the game
                            case ";":
                                i++;
                                break;
                            //Adds the current game to the list of modifications and resets manifests and i
                            case "}":
                                this._gameMods.Add(manifests);
                                manifests = new Manifest[] { new Manifest(), new Manifest() };
                                i = 0;
                                break;
                            //Reads through the lines of the current game's details, i determines if the line is old or new details
                            default:
                                switch (arr[1].ToLower().Trim())
                                {
                                    case "name":
                                        manifests[i].name = arr[arr.Length - 2];
                                        break;
                                    case "path":
                                        manifests[i].path = arr[arr.Length - 2];
                                        break;
                                    case "args":
                                        manifests[i].args = arr[arr.Length - 2];
                                        break;
                                    case "steam":
                                        manifests[i].steamLaunch = bool.Parse(arr[arr.Length - 2]);
                                        break;
                                    case "shortcut":
                                        manifests[i].useShortcut = bool.Parse(arr[arr.Length - 2]);
                                        break;
                                    case "usesapp":
                                        manifests[i].useSettingsApp = bool.Parse(arr[arr.Length - 2]);
                                        break;
                                    case "sapp":
                                        manifests[i].settingsApp = ParseSAppID(arr[arr.Length - 2]);
                                        break;
                                }
                                break;
                        }
                    }
                    //Loops through _gameMods to modify the games listing
                    foreach (Manifest[] ma in this._gameMods.ToArray())
                    {
                        //First tries to remove the game that doesn't have the modifications from _gameList
                        //True if the game was found and removed, false if not
                        bool found = this._gamesList.Remove(ma[0]);
                        //Only adds the modified game details if the old ones were found
                        //Prevents listing a Steam game when the user did not scan for Steam games when launching
                        if (found || true)
                        {
                            //Adds in the new game details
                            this._gamesList.Add(ma[1]);
                        }
                    }
                }*/
            }
            catch (Exception e)
            {
                //Restores the game modifications List whenever the try block fails, avoiding loss of changes to games due to an exception
                this._gameMods = holdmod;
                //Logs error of failed load from Modifications
                _log.Error(e, "Failed to load modification list.");
            }
            //
            //Temporary variable to hold the exclusions List incase the try block fails after erasing the exclusions List
            List<Manifest> holdex = this._exclusionsList;
            //Remove excluded Steam games
            try
            {
                //Clears the exclusions List before reading anything, avoiding duplication in the case of opening the Exclusions Form and saving it without changing anything
                this._exclusionsList = new List<Manifest>();
                //Return value from the Exclusions entry
                //string[] ret = (string[])Registry.GetValue(regpath, "Exclusions", null);
                string path = Path.Combine(_cfgPath, "exclusions.cfg");
                if (File.Exists(path))
                {
                    StreamReader sr = new StreamReader(File.OpenRead(path));
                    List<ExJson> exj = JsonConvert.DeserializeObject<List<ExJson>>(sr.ReadToEnd());
                    sr.Close();
                    sr.Dispose();
                    foreach (ExJson x in exj)
                        this._exclusionsList.Add(new Manifest()
                        {
                            name = x.Ex.Name,
                            path = x.Ex.Path,
                            args = x.Ex.Args,
                            steamLaunch = x.Ex.SteamLaunch,
                            useShortcut = x.Ex.UseShortcut,
                            useSettingsApp = x.Ex.UseSApp,
                            settingsApp = ParseSAppID(x.Ex.SApp)
                        });
                }
                foreach (Manifest m in _exclusionsList.ToArray())
                    _gamesList.Remove(m);
                //If the return value is not empty
                /*if (ret != null && ret.Length > 0)
                {
                    //Temporary variable for reading in excluded games
                    Manifest manifest = new Manifest();
                    foreach (string s in ret)
                    {
                        string[] arr = s.Split('"');
                        switch (arr[0].Trim())
                        {
                            //Skips over the opening bracket
                            case "{":
                                break;
                            //Copy the current details to _exclusionsList and reset manifest
                            case "}":
                                this._exclusionsList.Add(manifest);
                                manifest = new Manifest();
                                break;
                            //Reads through the lines of the game details
                            default:
                                switch (arr[1].ToLower().Trim())
                                {
                                    case "name":
                                        manifest.name = arr[arr.Length - 2];
                                        break;
                                    case "path":
                                        manifest.path = arr[arr.Length - 2];
                                        break;
                                    case "args":
                                        manifest.args = arr[arr.Length - 2];
                                        break;
                                    case "steam":
                                        manifest.steamLaunch = bool.Parse(arr[arr.Length - 2]);
                                        break;
                                    case "shortcut":
                                        manifest.useShortcut = bool.Parse(arr[arr.Length - 2]);
                                        break;
                                    case "usesapp":
                                        manifest.useSettingsApp = bool.Parse(arr[arr.Length - 2]);
                                        break;
                                    case "sapp":
                                        manifest.settingsApp = ParseSAppID(arr[arr.Length - 2]);
                                        break;
                                }
                                break;
                        }
                    }
                    //Loops through each game in the exclusions and removes it from the list of games
                    foreach (Manifest m in _exclusionsList.ToArray())
                    {
                        _gamesList.Remove(m);
                    }
                }*/
            }
            catch (Exception e)
            {
                //Restores the exclusions list whenever the try block fails
                this._exclusionsList = holdex;
                //Logs the error in loading exclusions
                _log.Error(e, "Failed to load Exclusion list.");
                //Displays error
                MessageBox.Show("Failed to load list of Steam game exclusions:\n" + e.Message);
            }
            //
            //Sort the list of games to make the final listing alphabetically sorted
            _gamesList.Sort((x, y) => x.name.CompareTo(y.name));
        }
        //Decides which settings app is wanted based on the id string
        private SettingsApp ParseSAppID(string id)
        {
            //Read in the saved SettingsApps
            List<SettingsApp> apps = new List<SettingsApp>();
            StreamReader sr = new StreamReader(File.OpenRead(Path.Combine(_cfgPath, "sapps.cfg")));
            List<SAppJson> saj = JsonConvert.DeserializeObject<List<SAppJson>>(sr.ReadToEnd());
            sr.Close();
            sr.Dispose();
            foreach (SAppJson sa in saj)
                apps.Add(sa.SApp);

            /*string[] ret = (string[])Registry.GetValue(regpath, "SettingsApps", new string[0]);
            if (ret != null && ret.Length > 0)
            {
                foreach (string s in ret)
                {
                    string[] arr = s.Split('"');
                    apps.Add(new SettingsApp(arr[1], bool.Parse(arr[3]), arr[5], arr[7], bool.Parse(arr[9])));
                }
            }
            else
                return null;*/
            //Loops through all SettingsApps to attempt to find the match to the given id string
            foreach (SettingsApp a in apps)
            {
                if (a.IDString() == id)
                    return a;
            }
            //Shouldn't reach this unless there was no SettingsApp matching the id string
            return null;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cfg.CloseOnLaunch = closeCheckBox.Checked;
            Application.Exit();
            //Saves the state of the 'Close on Launch' check box
            /*Config config = new Config
            {
                CloseOnLaunch = closeCheckBox.Checked
            };*/
            //config.Save();
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
                //Saves the log to a file
                //string p = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                //_log.SaveToFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DLG-Log.txt"));
                //Saves log level
                //Registry.SetValue(regpath, "logLvl", _log.LogLevel);
                string path = Path.Combine(_cfgPath, "config.cfg");
                if (File.Exists(path))
                    File.Delete(path);
                StreamWriter sw = new StreamWriter(File.Open(path, FileMode.OpenOrCreate));
                sw.WriteLine(JsonConvert.SerializeObject(cfg, Formatting.Indented));
                sw.Flush();
                sw.Close();
                sw.Dispose();
                _log.Info("Goodbye");
                _log.Close();
            }
        }
        //Saves local launch games to config file
        private void AddSavedGames()
        {
            try
            {
                List<InclusionJson> ij = new List<InclusionJson>();
                foreach (Manifest m in this._gamesList)
                    if (!m.steamLaunch)
                        ij.Add(new InclusionJson()
                        {
                            Game = new Inclusion()
                            {
                                Name = m.name,
                                Path = m.path,
                                Args = m.args,
                                SteamLaunch = m.steamLaunch,
                                ShortcutLaunch = m.useShortcut,
                                UseSApp = m.useSettingsApp,
                                SApp = (m.settingsApp == null) ? "" : m.settingsApp.IDString()
                                //SApp = m.settingsApp.IDString()
                            }
                        });
                string path = Path.Combine(_cfgPath, "inclusions.cfg");
                if (File.Exists(path))
                    File.Delete(path);
                StreamWriter sw = new StreamWriter(File.Open(path, FileMode.OpenOrCreate));
                sw.WriteLine(JsonConvert.SerializeObject(ij, Formatting.Indented));
                sw.Flush();
                sw.Close();
                sw.Dispose();
                //String List to be saved in the registry
                /*List<string> games = new List<string>();
                //Loops through each local launch game and adds its details to the games List
                foreach (Manifest m in this._gamesList)
                {
                    if (!m.steamLaunch)
                    {

                        games.Add("{");
                        games.Add("\t\"name\":\"" + m.name + "\"");
                        games.Add("\t\"path\":\"" + m.path + "\"");
                        games.Add("\t\"args\":\"" + m.args + "\"");
                        games.Add("\t\"steam\":\"" + m.steamLaunch + "\"");
                        games.Add("\t\"shortcut\":\"" + m.useShortcut + "\"");
                        games.Add("\t\"useSApp\":\"" + m.useSettingsApp + "\"");
                        games.Add("\t\"SApp\":\"" + ((m.settingsApp != null) ? m.settingsApp.IDString() : "") + "\"");
                        games.Add("}");
                    }
                }
                //Converts the games List to an array and saves it to the registry
                Registry.SetValue(regpath, "Inclusions", games.ToArray());*/
            }
            catch (NullReferenceException e)
            {
                _log.Error(e, "Caught NullReferenceException while saving added games.");
            }
            catch (Exception e)
            {
                //Log the error in saving the Inclusions list
                _log.Error(e, "Failed to save added games.");
                //MessageBox.Show("Failed to save added games:\n" + e.Message);
            }
        }
        //Saves excluded Steam games to config file
        private void AddExcludedGames()
        {
            try
            {
                List<ExJson> xj = new List<ExJson>();
                foreach (Manifest m in this._exclusionsList)
                    if (m.steamLaunch)
                        xj.Add(new ExJson()
                        {
                            Ex = new Exclusion()
                            {
                                Name = m.name,
                                Path = m.path,
                                Args = m.args,
                                SteamLaunch = m.steamLaunch,
                                UseShortcut = m.useShortcut,
                                UseSApp = m.useSettingsApp,
                                SApp = (m.settingsApp == null) ? "" : m.settingsApp.IDString()
                                //SApp = m.settingsApp.IDString()
                            }
                        });
                string path = Path.Combine(_cfgPath, "exclusions.cfg");
                if (File.Exists(path))
                    File.Delete(path);
                StreamWriter sw = new StreamWriter(File.Open(path, FileMode.OpenOrCreate));
                sw.WriteLine(JsonConvert.SerializeObject(xj, Formatting.Indented));
                sw.Flush();
                sw.Close();
                sw.Dispose();
                //String List to be saved in the registry
                /*List<string> games = new List<string>();
                //Loops through each excluded game and adds the details to the games List if it is a Steam launch game
                foreach (Manifest m in this._exclusionsList)
                {
                    if (m.steamLaunch)
                    {
                        games.Add("{");
                        games.Add("\t\"name\":\"" + m.name + "\"");
                        games.Add("\t\"path\":\"" + m.path + "\"");
                        games.Add("\t\"args\":\"" + m.args + "\"");
                        games.Add("\t\"steam\":\"" + m.steamLaunch + "\"");
                        games.Add("\t\"shortcut\":\"" + m.useShortcut + "\"");
                        games.Add("\t\"useSApp\":\"" + m.useSettingsApp + "\"");
                        games.Add("\t\"SApp\":\"" + ((m.settingsApp != null) ? m.settingsApp.IDString() : "") + "\"");
                        games.Add("}");
                    }
                }
                //Converts games to an array and saves it to the registry
                Registry.SetValue(regpath, "Exclusions", games.ToArray());*/
            }
            catch (NullReferenceException e)
            {
                _log.Error(e, "Caught NullReferenceException while saving excluded games.");
            }
            catch (Exception e)
            {
                //Log the error in saving Exclusions list
                _log.Error(e, "Failed to save excluded games.");
                //MessageBox.Show("Failed to save excluded games:\n" + e.Message);
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
                List<ModJson> mj = new List<ModJson>();
                foreach (Manifest[] m in this._gameMods)
                    if (m[0].steamLaunch || m[1].steamLaunch)
                        mj.Add(new ModJson()
                        {
                            Mod = new Modification()
                            {
                                OldName = m[0].name,
                                OldPath = m[0].path,
                                OldArgs = m[0].args,
                                OldSteam = m[0].steamLaunch,
                                OldShort = m[0].useShortcut,
                                OldUseSApp = m[0].useSettingsApp,
                                OldSApp = (m[0].settingsApp == null) ? "" : m[0].settingsApp.IDString(),
                                //OldSApp = m[0].settingsApp.IDString(),
                                NewName = m[1].name,
                                NewPath = m[1].path,
                                NewArgs = m[1].args,
                                NewSteam = m[1].steamLaunch,
                                NewShort = m[1].useShortcut,
                                NewUseSApp = m[1].useSettingsApp,
                                NewSApp = (m[1].settingsApp == null) ? "" : m[1].settingsApp.IDString()
                                //NewSApp = m[1].settingsApp.IDString()
                            }
                        });
                string path = Path.Combine(_cfgPath, "modifications.cfg");
                if (File.Exists(path))
                    File.Delete(path);
                StreamWriter sw = new StreamWriter(File.Open(path, FileMode.OpenOrCreate));
                sw.WriteLine(JsonConvert.SerializeObject(mj, Formatting.Indented));
                sw.Flush();
                sw.Close();
                sw.Dispose();
                //String List to be saved in the registry
                /*List<string> games = new List<string>();
                //Loops through each game change and adds the details to games if the games is a Steam launch either before or after changes
                foreach (Manifest[] m in this._gameMods)
                {
                    if (m[0].steamLaunch || m[1].steamLaunch)
                    {
                        games.Add("{");
                        games.Add("\t\"name\":\"" + m[0].name + "\"");
                        games.Add("\t\"path\":\"" + m[0].path + "\"");
                        games.Add("\t\"args\":\"" + m[0].args + "\"");
                        games.Add("\t\"steam\":\"" + m[0].steamLaunch + "\"");
                        games.Add("\t\"shortcut\":\"" + m[0].useShortcut + "\"");
                        games.Add("\t\"useSApp\":\"" + m[0].useSettingsApp + "\"");
                        games.Add("\t\"SApp\":\"" + ((m[0].settingsApp != null) ? m[0].settingsApp.IDString() : "")  + "\"");
                        games.Add(";");
                        games.Add("\t\"name\":\"" + m[1].name + "\"");
                        games.Add("\t\"path\":\"" + m[1].path + "\"");
                        games.Add("\t\"args\":\"" + m[1].args + "\"");
                        games.Add("\t\"steam\":\"" + m[1].steamLaunch + "\"");
                        games.Add("\t\"shortcut\":\"" + m[1].useShortcut + "\"");
                        games.Add("\t\"useSApp\":\"" +  m[1].useSettingsApp + "\"");
                        games.Add("\t\"SApp\":\"" + ((m[1].settingsApp != null) ? m[1].settingsApp.IDString() : "")  + "\"");
                        games.Add("}");
                    }
                }
                Registry.SetValue(regpath, "Modifications", games.ToArray());*/
            }
            catch (NullReferenceException e)
            {
                _log.Error(e, "Caught NullReferenceException while saving edits to games in listing.");
            }
            catch (Exception e)
            {
                //Log the error in saving the Modifications list
                _log.Error(e, "Failed to save edits to games in listing.");
                //MessageBox.Show("Failed to save edits to games in listing:\n" + e.Message);
            }
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
            bool found = false;
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
                        found = true;
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
            if (found)
                return ElimChains(r);
            else
                return r;
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
                Grouping group = new Grouping(m[i], this)
                {
                    //If i modulo 4 returns 0:
                    //True: The next game should be listed on the next row, add 86 (the height of the Grouping with padding) to the current Y value
                    //False: The next game should be listed on the same row, the X value is 236 (width of the Grouping with padding) multiplied my the remainder when dividing i by 4
                    Location = (i % 4 == 0 ? new Point(location[0], location[1] += 86) : new Point(location[0] + (236 * (i % 4)), location[1]))
                };
                //Add the Grouping to the panel's controls
                panel1.Controls.Add(group/*.Group*/);
                //If there is enough games listed for there to be a need for a scroll bar, adjust the size of the panel to fit the scroll bar
                if (i > 24 && panel1.Size.Width != 960) panel1.Size = new Size(960, panel1.Size.Height);
            }
        }

        //Restarts application
        //Probably to be removed, it is mostly for easy testing
        private void RestartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.closeCheckBox.Checked = cfg.CloseOnLaunch;
        }

        //Saves changes to the 'Close on Launch' setting
        private void CloseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.cfg.CloseOnLaunch = closeCheckBox.Checked;
            /*Config config = new Config
            {
                CloseOnLaunch = closeCheckBox.Checked
            };
            config.Save();*/
        }

        //Called by 'Add Game' button in menu strip
        private void AddGameToolStripMenuItem_Click(object sender, EventArgs e)
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
                            args = g.args,
                            steamLaunch = g.isSteam,
                            useShortcut = g.useShortcut,
                            useSettingsApp = g.useSettingsApp,
                            settingsApp = g.settingsApp
                        });
                    }
                    //Adds games to the list of games to be displayed
                    this._gamesList.AddRange(manifests.ToArray<Manifest>());
                    //Sorts games list alphabetically
                    this._gamesList.Sort((x, y) => x.name.CompareTo(y.name));
                    AddSavedGames();
                    //Re-lists the games
                    ListGames(this._gamesList.ToArray<Manifest>());
                }
            }
        }

        //Used by 'Grouping' class for Deleting games
        public void RemoveGame(Manifest manifest)
        {
            //Generates the manifest for the game to be deleted
            Manifest torem = manifest;
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
            else _log.Warn("Error removing game during RemoveGame(Manifest manifest) in Form1.cs. Game not found in games list.");
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

                //Compare any changes to the Steam launch setting for the game
                if (oldMan.steamLaunch && !newMan.steamLaunch)
                {
                    //Was a Steam launch but is now a local launch
                    //Exclude game from Steam game scanning
                    this._exclusionsList.Add(oldMan);
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
                _log.Warn("Update failed during UpdateGame(Manifest oldMan, Manifest newMan) in Form1.cs");
                MessageBox.Show("Update failed.");
            }
            //Sorts games list alphabetically
            this._gamesList.Sort((x, y) => x.name.CompareTo(y.name));
            //Re-lists games
            ListGames(this._gamesList.ToArray());
        }

        //Called by clicking the 'Exclusions' button on the tool strip
        private void ExclusionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Are there any games that have been excluded?
            //If so, show the form
            if (this._exclusionsList.Count > 0)
            {
                //Saves the list of excluded games
                //AddExcludedGames();
                //Using the form that will display all the excluded games
                using (var form = new ExclusionsForm(this._exclusionsList))
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
                        //If the user scanned for Steam games on launch
                        if (_scannedSteam)
                        {
                            //Show loading animation
                            _loadingForm = new LoadingForm();
                            _loadingForm.Show();
                            //Creates a new ScanThreadWorker and sets its ThreadDone EventHandler
                            worker = new ScanThreadWorker();
                            worker.ThreadDone += HandleThreadDone;
                            //Runs worker in a separate thread to scan for Steam games
                            Thread thread = new Thread(worker.Run)
                            { Name = "SteamScanThread-AfterExclusions" };
                            thread.Start();
                        }
                        //Otherwise just set threadDone to true so WaitForThreadDone will continue
                        else
                            threadDone = true;
                        //Call WaitForThreadDone to display games
                        WaitForThreadDone();
                    }
                }
            }
            else //If there were no games excluded, there's no need to show the form, let the user know
                MessageBox.Show("There are no excluded games. Use the settings button on a game in the listing to exclude that game.");
            
            
        }

        //Called by clicking 'Changes' button on tool strip
        private void ChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Saves the current list of changes to games
            //AddGameModifications();
            //Using the Form that will list all the changes saved
            using (ModificationsForm form = new ModificationsForm(this._gameMods))
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
                    //If the user wanted Steam games scanned for on launch
                    if (_scannedSteam)
                    {
                        //Show loading animation
                        _loadingForm = new LoadingForm();
                        _loadingForm.Show();
                        //Create a new instance of of the ScanThreadWorker
                        worker = new ScanThreadWorker();
                        //Sets the worker's ThreadDone EventHandler to HandleThreadDone
                        worker.ThreadDone += HandleThreadDone;
                        //Creates a new Thread from worker and runs it to scan for Steam games
                        Thread thread = new Thread(worker.Run)
                        { Name = "SteamScanThread-AfterExclusions" };
                        thread.Start();
                    }
                    //Otherwise just set threadDone to true, so WaitForThreadDone will continue
                    else
                        threadDone = true;
                    //Calls WaitForThreadDone to display games
                    WaitForThreadDone();
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

        private void LoggingLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //``````````````````````````````````````
            using (var form = new SelectLogLevelForm(_log.LogLevel))
            {
                DialogResult res = form.ShowDialog();
                if (res == DialogResult.OK)
                {
                    _log.LogLevel = form.Level;
                    cfg.LogLvl = form.Level;
                }
            }
        }
    }

    public class Grouping : GroupBox
    {
        private PictureBox iconBox;
        private PictureBox settingsBox;
        private PictureBox launchBox;

        //New Declaration
        public Manifest Manifest { get; set; }
        private readonly Form1 _parent;
        public Grouping(Manifest manifest, Form1 parent)
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
                    Process.Start("explorer.exe", "steam://rungameid/" + this.Manifest.path);
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
                        wsh.TargetPath = this.Manifest.path;// + ((this.Manifest.args != null) ? " " + this.Manifest.args : "");
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
                        Process.Start(spath, this.Manifest.args);
                    }
                    else
                    {
                        //Launches the game
                        Process.Start(this.Manifest.path, this.Manifest.args);
                    }
                }
                this._parent.FlagExit(new Config().CloseOnLaunch);
                if (new Config().CloseOnLaunch)
                    Application.Exit();
            }
            catch (Win32Exception ex) { Form1._log.Warn("Application cannot find path specified for game.\n" + ex.StackTrace); }
            catch (Exception ex) { Form1._log.Error(ex, ex.Message); MessageBox.Show(ex.Message); }
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
            try
            {
                if (this.Manifest.settingsApp.IDString().ToLower() != "sapp:default:::1")
                {
                    if (this.Manifest.settingsApp.Path != null && this.Manifest.settingsApp.Path != "")
                    {
                        try
                        {
                            Form1._log.Info("Attempting to start external settings application \"" + this.Manifest.settingsApp.Name + "\" at path \"" + this.Manifest.settingsApp.Path + "\"...");
                            Process.Start(this.Manifest.settingsApp.Path, this.Manifest.settingsApp.Args);
                        }
                        catch (Exception ex)
                        {
                            Form1._log.Error(ex, "Failed to start external settings application, defaulting to shortcut settings.");
                            Short(sender, e);
                        }
                    }
                    else
                    {
                        Form1._log.Warn("Cannot launch external settings app \"" + this.Manifest.settingsApp.Name + "\", path is empty.");
                        Short(sender, e);
                    }
                }
                else
                    SettingsSwitch(this.Manifest.path);
            }
            catch(Exception ez)
            {
                Form1._log.Error(ez, "Failed to launch custom settings application.");
                Short(sender, e);
            }
        }
        private void SettingsSwitch(string test)
        {
            switch (test)
            {
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
                            Process.Start(path, "nolaunch");
                        }
                        //Incase of any failure, default to shortcut settings
                        catch { Short(this, EventArgs.Empty); }
                    }
                    //Default to shortcut settings
                    else
                        Short(this, EventArgs.Empty);
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
                            Process.Start(path, "nolaunch");
                        }
                        //Incase of any failure, default to shortcut settings
                        catch { Short(this, EventArgs.Empty); }
                    }
                    //Default to shortcut settings
                    else
                        Short(this, EventArgs.Empty);
                    break;
                //Default to shortcut settings
                default:
                    Short(this, EventArgs.Empty);
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
                    Manifest oldMan = this.Manifest;
                    //Updated details
                    Manifest newMan = new Manifest
                    {
                        name = form.GameName,
                        path = form.GamePath,
                        steamLaunch = form.IsSteamLaunch,
                        useShortcut = form.UseShortcut,
                        useSettingsApp = form.UseSettingsApp,
                        settingsApp = form.SettingsApp,
                        args = form.Args
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
    }

    //App Manifest Data
    public struct Manifest
    {
        public string name;
        public string path;
        public bool steamLaunch;
        public bool useShortcut;
        public bool useSettingsApp;
        public SettingsApp settingsApp;
        public string args;
        
        public override string ToString()
        {
            return "[name:" + name + ", path:" + path + ", args:" + args + ", steamLaunch:" + steamLaunch + ", useShortcut:" + useShortcut + ", useSApp:" + useSettingsApp + ", SApp:" + ((useSettingsApp) ? settingsApp.IDString() : "") + "]";
        }
        public static bool operator ==(Manifest a, Manifest b)
        {
            return a.name == b.name &&
                a.path == b.path &&
                a.args == b.args &&
                a.steamLaunch == b.steamLaunch &&
                a.useShortcut == b.useShortcut &&
                a.useSettingsApp == b.useSettingsApp &&
                ((a.useSettingsApp || b.useSettingsApp) ? a.settingsApp == b.settingsApp : true);
        }
        public static bool operator !=(Manifest a, Manifest b)
        {
            return a.name != b.name ||
                a.path != b.path ||
                a.args != b.args ||
                a.steamLaunch != b.steamLaunch ||
                a.useShortcut != b.useShortcut ||
                a.useSettingsApp != b.useSettingsApp ||
                ((a.useSettingsApp || b.useSettingsApp) ? a.settingsApp != b.settingsApp : false);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is Manifest))
                return false;
            Manifest m = (Manifest)obj;
            return name == m.name &&
                path == m.path &&
                args == m.args &&
                steamLaunch == m.steamLaunch &&
                useShortcut == m.useShortcut &&
                useSettingsApp == m.useSettingsApp &&
                ((useSettingsApp && m.useSettingsApp) ? settingsApp == m.settingsApp : true);
        }
        public override int GetHashCode()
        {
            var hashCode = -875002707;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(path);
            hashCode = hashCode * -1521134295 + steamLaunch.GetHashCode();
            hashCode = hashCode * -1521134295 + useShortcut.GetHashCode();
            hashCode = hashCode * -1521134295 + useSettingsApp.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<SettingsApp>.Default.GetHashCode(settingsApp);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(args);
            return hashCode;
        }
    }

    class ScanThreadWorker
    {
        public event EventHandler ThreadDone;

        //The return value
        private List<Manifest> val = new List<Manifest>();
        //To get return value
        public Manifest[] Value { get { return val.ToArray(); } }
        //The method to scan for Steam games
        public void Run()
        {
            //Gets the value that is held in the Registry to find out where Steam is located
            object ret = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", "NO VAL");
            //If Steam install path is not found in registry, the user will have to input the path manually
            if (ret == null || (string)ret == "NO VAL")
            {
                Form1._log.Warn("Steam is not installed as expected, scanning thread is unable to continue.");
                /*return;
                MessageBox.Show("Steam is not installed as expected, unable to continue.");*/
            }
            else
            {
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
                            manifest.useSettingsApp = false;
                            manifest.settingsApp = null;
                            manifest.args = "";
                            //Adds the game to the list of games to be returned if it is not already in the list
                            //Should always evaluate true
                            if (!val.Contains(manifest)) val.Add(manifest);
                        }
                        catch (Exception e) { Form1._log.Error(e, "Exception while scanning Steam default directory."); }
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
                            sr.ReadLine(); sr.ReadLine();
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
                            manifest.useSettingsApp = false;
                            manifest.settingsApp = null;
                            manifest.args = "";
                            //Adds the game to the list of games to be returned if it is not already in the list
                            //Should always evaluate true
                            if (!val.Contains(manifest)) val.Add(manifest);
                        }
                    }
                }
                catch (Exception e) { Form1._log.Error(e, "Exception while loading games from user-created Steam library folders"); }
            }
            
            //Sorts the found Games in alphabetical order
            val.Sort((x, y) => x.name.CompareTo(y.name));

            ThreadDone?.Invoke(this, EventArgs.Empty);
        }
    }
}
