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
using System.Diagnostics;
using Newtonsoft.Json;
using HoodedDeathHelperLibrary;

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
        //Path to the folder of config files
        private readonly string _cfgPath;
        //Instance of the Config class, used for logging level and close on launch setting
        private Config cfg;
        //Whether or not WaitForThreadDone should wait or continue
        private bool threadDone = false;

        //The thread worker class used to scan for Steam games
        ScanThreadWorker worker;

        Thread loadingThread;
        //Logger instance to be used for logging throughout the application
        public static Logger _log;

        public Form1()
        {

            _cfgPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeathApplications");
            if (!Directory.Exists(_cfgPath))
                Directory.CreateDirectory(_cfgPath);
            _cfgPath = Path.Combine(_cfgPath, "DLG");
            if (!Directory.Exists(_cfgPath))
                Directory.CreateDirectory(_cfgPath);
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
            //Initialize the Logger
            _log = new Logger(cfg.LogLvl, Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DLG-Log.txt"));
            _log.Info("Logger initialized.");
            _log.Debug("Already checked path to ApplicationData folder and \"config.cfg\" file.");
            //
            _log.Debug("Calling Form1.InitializeComponent ...");
            InitializeComponent();
            _log.Debug("Form1 Initialized.");
            //
            _log.Debug("Checking 'sapps.cfg' ...");
            if (!File.Exists(Path.Combine(_cfgPath, "sapps.cfg")))
            {
                _log.Debug("Config file 'sapps.cfg' doesn't exist, creating file ...");
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
                _log.Debug("Creating StreamWriter ...");
                StreamWriter sw = new StreamWriter(File.Open(Path.Combine(_cfgPath, "sapps.cfg"), FileMode.OpenOrCreate));
                _log.Debug("Serializing default SettingsApp, Writing through StreamWriter ...");
                sw.WriteLine(JsonConvert.SerializeObject(t, Formatting.Indented));
                _log.Debug("Flushing and closing StreamWriter ...");
                sw.Flush();
                sw.Close();
                sw.Dispose();
                _log.Debug("Done generating 'sapps.cfg'");
            }
            else
                _log.Debug("Config file 'sapps.cfg' already exists.");
            _log.Debug("Asking user to scan for Steam games ...");
            //If the user wants to scan for Steam games
            if (MessageBox.Show("Scan for installed Steam games?", "Continue?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _log.Debug("Answered yes, starting Steam game scan ...");
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
                _log.Debug("Answered no, skipping Steam scan.");
                threadDone = true;
            }
            //Call method to list out the games
            WaitForThreadDone();
        }
        //Called to list out the found games
        private void WaitForThreadDone()
        {
            //Waits until the scanning Thread is done (if it ran at all)
            if (worker != null) _log.Debug("Waiting for scanning to finish ...");
            while (!threadDone) { }
            if (worker != null) _log.Debug("Done waiting.");
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
            _log.Debug("Reading in game configurations ...");
            ReadGames();
            _log.Debug("Done reading game configurations.");
            //Calls ListGames to list out all the games found
            _log.Debug("Displaying games found ...");
            ListGames(_gamesList.ToArray());
            _log.Debug("Done displaying games.");
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
            _log.Debug("Done initializing.");
        }
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
                _log.Debug("Fetching Inclusions ...");
                //Path to inclusions file
                string path = Path.Combine(_cfgPath, "inclusions.cfg");
                _log.Debug("Checking for 'inclusions.cfg' file ...");
                if (File.Exists(path))
                {
                    _log.Debug("Config file 'inclusions.cfg' does exist.");
                    _log.Debug("Creating StreamReader for inclusions ...");
                    StreamReader sr = new StreamReader(File.OpenRead(path));
                    _log.Debug("Deserializing Inclusions ...");
                    List<InclusionJson> inclusionsJ = JsonConvert.DeserializeObject<List<InclusionJson>>(sr.ReadToEnd());
                    _log.Debug("Done, closing StreamReader.");
                    sr.Close();
                    sr.Dispose();
                    _log.Debug("Looping through deserialized InclusionJson instances ...");
                    foreach (InclusionJson ij in inclusionsJ)
                    {
                        _log.Debug(string.Format("Adding new manifest: Name>{0}, Path>{1}, Args{2}, SteamLaunch>{3}, UseShortcut>{4}, UseSettingsApp>{5}, SettingsApp>{6}", ij.Game.Name, ij.Game.Path, ij.Game.Args, ij.Game.SteamLaunch, ij.Game.ShortcutLaunch, ij.Game.UseSApp, ij.Game.SApp));
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
                else
                    _log.Debug("Config file 'inclusions.cfg' doesn not exist.");
            }
            catch (Exception e)
            {
                //Log error from failing to load Inclusions
                _log.Error(e, "Failed to load games from saved list.");
                //MessageBox.Show("Failed to load games from saved list:\n" + e.Message);
            }
            _log.Debug("Done with Inclusions.");
            //
            _log.Debug("Sorting game list.");
            //Sort _gamesList before modifying, for pretty much no reason but my own desire
            _gamesList.Sort((x, y) => x.name.CompareTo(y.name));
            _log.Debug("Backing up _gameMods.");
            //Temporary variable to hold the list of modifications incase the try block fails after erasing the list
            List<Manifest[]> holdmod = this._gameMods;
            //Modify any games based on the Modifications file
            try
            {
                _log.Debug("Clearing _gameMods.");
                //Makes sure modifications List is empty (avoids duplications caused by loading the modifications Form and exiting by saving even though there were no changes made)
                this._gameMods = new List<Manifest[]>();
                //Path to modifications file
                string path = Path.Combine(_cfgPath, "modifications.cfg");
                _log.Debug("Checking 'modifications.cfg' ...");
                if (File.Exists(path))
                {
                    _log.Debug("Config file 'modifications.cfg' exists.");
                    _log.Debug("Creating StreamReader for modifications ...");
                    StreamReader sr = new StreamReader(File.OpenRead(path));
                    _log.Debug("Deserializing modifications ...");
                    List<ModJson> modsJ = JsonConvert.DeserializeObject<List<ModJson>>(sr.ReadToEnd());
                    _log.Debug("Done, closing StreamReader.");
                    sr.Close();
                    sr.Dispose();
                    _log.Debug("Looping through each ModJson instance ...");
                    foreach (ModJson m in modsJ)
                    {
                        _log.Debug(string.Format("Adding new modification manifest: Old [ Name>{0}, Path>{1}, Args>{2}, SteamLaunch>{3}, UseShortcut>{4}, UseSettingsApp>{5}, SettingsApp>{6} ], New [ Name>{7}, Path>{8}, Args>{9}, SteamLaunch>{10}, UseShortcut>{11}, UseSettingsApp>{12}, SettingsApp>{13} ]", m.Mod.OldName, m.Mod.OldPath, m.Mod.OldArgs, m.Mod.OldSteam, m.Mod.OldShort, m.Mod.OldUseSApp, m.Mod.OldSApp, m.Mod.NewName, m.Mod.NewPath, m.Mod.NewArgs, m.Mod.NewSteam, m.Mod.NewShort, m.Mod.NewUseSApp, m.Mod.NewSApp));
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
                _log.Debug("Modifying games ...");
                foreach (Manifest[] ma in this._gameMods.ToArray())
                {
                    _log.Debug("Attempting to remove old details ...");
                    //First tries to remove the game that doesn't have the modifications from _gameList
                    //True if the game was found and removed, false if not
                    bool found = this._gamesList.Remove(ma[0]);
                    //Only adds the modified game details if the old ones were found
                    //Prevents listing a Steam game when the user did not scan for Steam games when launching
                    if (found)
                    {
                        _log.Debug("Old details removed. Adding new details.");
                        //Adds in the new game details
                        this._gamesList.Add(ma[1]);
                    }
                    else
                        _log.Debug("Old details not removed, game was not in _gamesList.");
                }
            }
            catch (Exception e)
            {
                //Restores the game modifications List whenever the try block fails, avoiding loss of changes to games due to an exception
                this._gameMods = holdmod;
                //Logs error of failed load from Modifications
                _log.Error(e, "Failed to load modification list.");
            }
            _log.Debug("Finished modifications.");
            //
            _log.Debug("Backing up _exclusionsList.");
            //Temporary variable to hold the exclusions List incase the try block fails after erasing the exclusions List
            List<Manifest> holdex = this._exclusionsList;
            //Remove excluded Steam games
            try
            {
                _log.Debug("Clearing _exclusionsList.");
                //Clears the exclusions List before reading anything, avoiding duplication in the case of opening the Exclusions Form and saving it without changing anything
                this._exclusionsList = new List<Manifest>();
                //Path to exclusions file
                string path = Path.Combine(_cfgPath, "exclusions.cfg");
                _log.Debug("Checking 'exclusions.cfg' ...");
                if (File.Exists(path))
                {
                    _log.Debug("Config file 'exclusions.cfg' does exist.");
                    _log.Debug("Creating StreamReader for exclusions ...");
                    StreamReader sr = new StreamReader(File.OpenRead(path));
                    _log.Debug("Deserializing exclusions ...");
                    List<ExJson> exj = JsonConvert.DeserializeObject<List<ExJson>>(sr.ReadToEnd());
                    _log.Debug("Done. Closing StreamReader.");
                    sr.Close();
                    sr.Dispose();
                    _log.Debug("Looping through ExJson instances ...");
                    foreach (ExJson x in exj)
                    {
                        _log.Debug(string.Format("Adding new exclusion manifest: Name>{0}, Path>{1}, Args>{2}, SteamLaunch>{3}, UseShortcut>{4}, UseSettingsApp>{5}, SettingsApp>{6}", x.Ex.Name, x.Ex.Path, x.Ex.Args, x.Ex.SteamLaunch, x.Ex.UseShortcut, x.Ex.UseSApp, x.Ex.SApp));
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
                }
                else
                    _log.Debug("Config file 'exclusions.cfg' does not exist.");
                _log.Debug("Removing exclusions ...");
                foreach (Manifest m in _exclusionsList.ToArray())
                    _gamesList.Remove(m);
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
            _log.Debug("Finished exclusions.");
            //
            _log.Debug("Sorting _gamesList.");
            //Sort the list of games to make the final listing alphabetically sorted
            _gamesList.Sort((x, y) => x.name.CompareTo(y.name));
            _log.Debug("Form1.ReadGames returning.");
        }
        //Decides which settings app is wanted based on the id string
        private SettingsApp ParseSAppID(string id)
        {
            _log.Debug(string.Format("Entering Form1.ParseSAppID(string id = {0}).", id));
            //Read in the saved SettingsApps
            List<SettingsApp> apps = new List<SettingsApp>();
            _log.Debug("Creating StreamReader for SApps ...");
            StreamReader sr = new StreamReader(File.OpenRead(Path.Combine(_cfgPath, "sapps.cfg")));
            _log.Debug("Deserializing SApps ...");
            List<SAppJson> saj = JsonConvert.DeserializeObject<List<SAppJson>>(sr.ReadToEnd());
            _log.Debug("Done. Closing StreamReader.");
            sr.Close();
            sr.Dispose();
            foreach (SAppJson sa in saj)
            {
                _log.Debug(string.Format("Adding {0}.", sa.SApp.IDString()));
                apps.Add(sa.SApp);
            }
            _log.Debug("Looking for matching SApp ...");
            //Loops through all SettingsApps to attempt to find the match to the given id string
            foreach (SettingsApp a in apps)
            {
                if (a.IDString() != id) continue;
                _log.Debug("Found matching SApp, returning.");
                return a;
            }
            _log.Info(string.Format("Couldn't find SApp match id string '{0}', returning null.", id));
            //Shouldn't reach this unless there was no SettingsApp matching the id string
            return null;
        }
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _log.Debug("Exit button clicked, storing closeCheckBox.Checked in cfg.CloseOnLaunch.");
            cfg.CloseOnLaunch = closeCheckBox.Checked;
            _log.Debug("Exiting.");
            Application.Exit();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _log.Debug("Form1 closing ...");
            if (!this.isExit) _log.Debug("Prompting user ...");
            //isExit is used for the check box 'Close on launch' to close launcher without prompt
            //MessageBox is a prompt to ask the user if they want to cancel the exit
            bool t = (e.Cancel = !(this.isExit || MessageBox.Show("Are you sure you want to exit?", "Exit", MessageBoxButtons.YesNo) == DialogResult.Yes));
            //If the exit was not cancelled
            if (!t)
            {
                _log.Debug("Exit not cancelled, saving configs ...");
                _log.Debug("Calling Form1.AddSavedGames ...");
                //Saves the local games the user added
                AddSavedGames();
                _log.Debug("Calling Form1.AddExcludedGames ...");
                //Saves any game exclusions the user made
                AddExcludedGames();
                _log.Debug("Calling Form1.AddGameModifications ...");
                //Saves any game detail modifications the user made
                AddGameModifications();
                _log.Debug("Writing 'config.cfg' ...");
                string path = Path.Combine(_cfgPath, "config.cfg");
                if (File.Exists(path))
                    File.Delete(path);
                _log.Debug("Creating StreamWriter for config ...");
                StreamWriter sw = new StreamWriter(File.Open(path, FileMode.OpenOrCreate));
                _log.Debug("Serializing config ...");
                sw.WriteLine(JsonConvert.SerializeObject(cfg, Formatting.Indented));
                _log.Debug("Done. Closing StreamWriter.");
                sw.Flush();
                sw.Close();
                sw.Dispose();
                _log.Info("Goodbye");
                _log.Close();
            }
            else
                _log.Debug("Exit cancelled.");
        }
        //Saves local launch games to config file
        private void AddSavedGames()
        {
            _log.Debug("Entering Form1.AddSavedGames.");
            try
            {
                List<InclusionJson> ij = new List<InclusionJson>();
                _log.Debug("Looping through _gamesList ...");
                foreach (Manifest m in this._gamesList)
                {
                    if (!m.steamLaunch)
                    {
                        _log.Debug(string.Format("Creating InclusionJson: Game [ Name>{0}, Path>{1}, Args>{2}, SteamLaunch>{3}, UseShortcut>{4}, UseSettingsApp>{5}, SApp>{6} ]", m.name, m.path, m.args, m.steamLaunch, m.useShortcut, m.useSettingsApp, (m.settingsApp == null) ? "" : m.settingsApp.IDString()));
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
                    }
                }
                _log.Debug("Writing inclusions ...");
                string path = Path.Combine(_cfgPath, "inclusions.cfg");
                if (File.Exists(path))
                    File.Delete(path);
                _log.Debug("Creating StreamWriter for inclusions ...");
                StreamWriter sw = new StreamWriter(File.Open(path, FileMode.OpenOrCreate));
                _log.Debug("Serializing inclusions ...");
                sw.WriteLine(JsonConvert.SerializeObject(ij, Formatting.Indented));
                _log.Debug("Done. Closing StreamWriter.");
                sw.Flush();
                sw.Close();
                sw.Dispose();
            }
            catch (NullReferenceException e)
            {
                _log.Error(e, "Caught NullReferenceException while saving added games.");
            }
            catch (Exception e)
            {
                //Log the error in saving the Inclusions list
                _log.Error(e, "Failed to save added games.");
            }
            _log.Debug("Form1.AddSavedGames returning.");
        }
        //Saves excluded Steam games to config file
        private void AddExcludedGames()
        {
            _log.Debug("Entering Form1.AddExcludedGames.");
            try
            {
                List<ExJson> xj = new List<ExJson>();
                _log.Debug("Looping through _exclusionsList ...");
                foreach (Manifest m in this._exclusionsList)
                {
                    if (m.steamLaunch)
                    {
                        _log.Debug(string.Format("Adding ExJson: Ex [ Name>{0}, Path>{1}, Args>{2}, SteamLaunch>{3}, UseShortcut>{4}, UseSettingsApp>{5}, SApp>{6} ].", m.name, m.path, m.args, m.steamLaunch, m.useShortcut, m.useSettingsApp, (m.settingsApp == null) ? "" : m.settingsApp.IDString()));
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
                            }
                        });
                    }
                }
                _log.Debug("Saving exclusions ...");
                string path = Path.Combine(_cfgPath, "exclusions.cfg");
                if (File.Exists(path))
                    File.Delete(path);
                _log.Debug("Creating StreamWriter for exclusions ...");
                StreamWriter sw = new StreamWriter(File.Open(path, FileMode.OpenOrCreate));
                _log.Debug("Serializing exclusions ...");
                sw.WriteLine(JsonConvert.SerializeObject(xj, Formatting.Indented));
                _log.Debug("Done. Closing StreamWriter.");
                sw.Flush();
                sw.Close();
                sw.Dispose();
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
            _log.Debug("Form1.AddExcludedGames returning.");
        }
        //Saves changes made to listings
        private void AddGameModifications()
        {
            _log.Debug("Entering Form1.AddGameModifications.");
            _log.Debug("Calling Form1.ElimChains(List<Manifest[]> l = _gameMods) ...");
            //Used to eliminate chains of changes to a single game
            //Example: 'a' -> 'ab', 'ab' -> 'ba'; results in 'a' -> 'ba'
            this._gameMods = ElimChains(this._gameMods);
            try
            {
                List<ModJson> mj = new List<ModJson>();
                _log.Debug("Looping through _gameMods ...");
                foreach (Manifest[] m in this._gameMods)
                {
                    if (m[0].steamLaunch || m[1].steamLaunch)
                    {
                        _log.Debug(string.Format("Adding ModJson: Mod [ Old [ Name>{0}, Path>{1}, Args>{2}, SteamLaunch>{3}, UseShortcut>{4}, UseSettingsApp>{5}, SApp>{6} ], New [ Name>{7}, Path>{8}, Args>{9}, SteamLaunch>{10}, UseShortcut>{11}, UseSettingsApp>{12}, SApp>{13} ] ]", m[0].name, m[0].path, m[0].args, m[0].steamLaunch, m[0].useShortcut, m[0].useSettingsApp, (m[0].settingsApp == null) ? "" : m[0].settingsApp.IDString(), m[1].name, m[1].path, m[1].args, m[1].steamLaunch, m[1].useShortcut, m[1].useSettingsApp, (m[1].settingsApp == null) ? "" : m[1].settingsApp.IDString()));
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
                                NewName = m[1].name,
                                NewPath = m[1].path,
                                NewArgs = m[1].args,
                                NewSteam = m[1].steamLaunch,
                                NewShort = m[1].useShortcut,
                                NewUseSApp = m[1].useSettingsApp,
                                NewSApp = (m[1].settingsApp == null) ? "" : m[1].settingsApp.IDString()
                            }
                        });
                    }
                }
                _log.Debug("Saving modifications ...");
                string path = Path.Combine(_cfgPath, "modifications.cfg");
                if (File.Exists(path))
                    File.Delete(path);
                _log.Debug("Creating StreamWriter for modifications ...");
                StreamWriter sw = new StreamWriter(File.Open(path, FileMode.OpenOrCreate));
                _log.Debug("Serializing modifications ...");
                sw.WriteLine(JsonConvert.SerializeObject(mj, Formatting.Indented));
                _log.Debug("Done. Closing StreamWriter.");
                sw.Flush();
                sw.Close();
                sw.Dispose();
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
            _log.Debug("Form1.AddGameModifications returning.");
        }
        //Eliminates chains of changes to a single game
        //Example: 'a' -> 'ab', 'ab' -> 'ba'; results in 'a' -> 'ba'
        private List<Manifest[]> ElimChains(List<Manifest[]> l)
        {
            _log.Debug("Entering Form1.ElimChains.");
            //Easier (in my experience) to deal with arrays vs. lists
            Manifest[][] m = l.ToArray();
            //The List of values to be returned
            List<Manifest[]> r = new List<Manifest[]>();
            //Bool to track if there were any chains found and fixed
            //Used to end recursive element
            bool found = false;
            _log.Debug("Looping through each modification ...");
            //Loops through each change that was given in the input
            for (int i = 0; i < m.Length; i++)
            {
                //If it's the first change, just add it to the return list
                if (i == 0)
                {
                    _log.Debug("Adding first element.");
                    r.Add(m[i]);
                    continue;
                }
                _log.Debug("Checking for link ...");
                //If the 'old' details of the current change are the same as the 'new' details of the previous change, this is a link in a chain and needs to be fixed
                if (m[i][0] == m[i - 1][1])
                {
                    _log.Debug("Link found. Removing last.");
                    //A chain has been found
                    found = true;
                    //Remove the previous change from the return list to prevent duplicates
                    r.Remove(r.Last());
                    _log.Debug("Adding modified element.");
                    //Add the 'old' details of the previous change and the 'new' details of the current change to the return list, eliminating the link between the two and merging them into a single change
                    r.Add(new Manifest[] { m[i - 1][0], m[i][1] });
                    continue;
                }
                _log.Debug("No link found. Adding current element.");
                //If there's no link found, just add the current change to the return list
                r.Add(m[i]);
            }
            //If there were any links found, recursively run the method again to check for any remaining chains, though this may be unnecessary, otherwise just return the list that won't contain direct chains
            if (found)
            {
                _log.Debug("Links were found. Running Form1.ElimChains again to check for more, then returning ...");
                return ElimChains(r);
            }
            _log.Debug("Form1.ElimChains returning.");
            return r;
        }

        //Default height value used to arrange the game listings in the panel
        private const int def_location_height = -86;
        //Location the listings are currently att
        private int[] location = new int[] { 3, -83 };
        //Lists out games in the Manifest[] m parameter
        private void ListGames(Manifest[] m)
        {
            _log.Debug("Entering Form1.ListGames.");
            _log.Debug("Reseting location height to default.");
            //Resets the height of the listing
            location[1] = def_location_height;
            _log.Debug("Clearing panel1 controls.");
            //Clears the panel of all previously listed games
            panel1.Controls.Clear();
            //Checks if the parameter 'm' is empty
            if (m == null || m.Count() == 0)
            {
                _log.Debug("No games given to display, returning.");
                return;
            }
            _log.Debug("Resetting panel1 auto scroll position.");
            //Resets auto scroll position to prevent strange height values
            //Comment this line out to find out what I mean. Scan for games, scroll some way down, then change a game's details or add a game so that the list is regenerated
            panel1.AutoScrollPosition = new Point(0, 0);
            _log.Debug("Looping through manifests ...");
            //Loops through each manifest in m
            for (int i = 0; i < m.Length; i++)
            {
                _log.Debug("Creating new Grouping ...");
                //Grouping is the class that holds all the data for each listed game
                Grouping group = new Grouping(m[i], this)
                {
                    //If i modulo 4 returns 0:
                    //True: The next game should be listed on the next row, add 86 (the height of the Grouping with padding) to the current Y value
                    //False: The next game should be listed on the same row, the X value is 236 (width of the Grouping with padding) multiplied my the remainder when dividing i by 4
                    Location = (i % 4 == 0 ? new Point(location[0], location[1] += 86) : new Point(location[0] + (236 * (i % 4)), location[1]))
                };
                _log.Debug(string.Format("Adding group to panel1 controls a location {0}.", group.Location));
                //Add the Grouping to the panel's controls
                panel1.Controls.Add(group/*.Group*/);
                //If there is enough games listed for there to be a need for a scroll bar, adjust the size of the panel to fit the scroll bar
                if (i > 24 && panel1.Size.Width != 960)
                {
                    _log.Debug("Adjusting panel width.");
                    panel1.Size = new Size(960, panel1.Size.Height);
                }
            }
            _log.Debug("Form1.ListGames returning.");
        }
        //Restarts application
        //Probably to be removed, it is mostly for easy testing
        private void RestartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _log.Debug("Restarting ...");
            Application.Restart();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _log.Debug("Loading cfg.CloseOnLaunch to Form1.closeCheckBox.");
            this.closeCheckBox.Checked = cfg.CloseOnLaunch;
        }

        //Saves changes to the 'Close on Launch' setting
        private void CloseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _log.Debug("Form1.closeCheckBox.Checked changed, storing in cfg.CloseOnLaunch.");
            this.cfg.CloseOnLaunch = closeCheckBox.Checked;
        }

        //Called by 'Add Game' button in menu strip
        private void AddGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _log.Debug("Entering Form1.AddGameToolStringMenuItem_Click.");
            _log.Debug("Creating new AddGameForm ...");
            using (var form = new AddGameForm())
            {
                _log.Debug("Show form as dialog ...");
                form.ShowDialog();
                //If the AddGameForm closed with the result of 'OK'
                if (form.DialogResult == DialogResult.OK)
                {
                    _log.Debug("Dialog confirmed.");
                    List<Manifest> manifests = new List<Manifest>();
                    _log.Debug("Looping through added games ...");
                    //Saves each of the games added by the user
                    foreach (AddGameForm.Game g in form.Games)
                    {
                        _log.Debug(string.Format("Creating manifest: Name>{0}, Path>{1}, Args>{2}, SteamLaunch>{3}, UseShortcut>{4}, UseSettingsApp>{5}, SApp>{6}", g.name, g.path, g.args, g.isSteam, g.useShortcut, g.useSettingsApp, g.settingsApp));
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
                    _log.Debug("Adding new games to _gamesList.");
                    //Adds games to the list of games to be displayed
                    this._gamesList.AddRange(manifests.ToArray<Manifest>());
                    _log.Debug("Sorting _gamesList.");
                    //Sorts games list alphabetically
                    this._gamesList.Sort((x, y) => x.name.CompareTo(y.name));
                    _log.Debug("Calling Form1.AddSavedGames ...");
                    AddSavedGames();
                    _log.Debug("Calling Form1.ListGames(Manifest[] m = _gamesList) ...");
                    //Re-lists the games
                    ListGames(this._gamesList.ToArray<Manifest>());
                }
                else
                    _log.Debug("Dialog cancelled.");
            }
            _log.Debug("Form1.AddGameToolStringMenuItem_Click returning.");
        }

        //Used by 'Grouping' class for Deleting games
        public void RemoveGame(Manifest manifest)
        {
            _log.Debug(string.Format("Entering Form1.RemoveGame, manifest is '{0}'.", manifest.ToString()));
            //Generates the manifest for the game to be deleted
            Manifest torem = manifest;
            _log.Debug("Attempting to remove game ...");
            //Tries to remove the game from the games list and returns true if successful, false otherwise
            bool found = this._gamesList.Remove(torem);
            //If the game was found and removed
            if (found)
            {
                _log.Debug("Removed.");
                _log.Debug("Calling Form1.ListGames(Manifest[] m = _gamesList) ...");
                //Re-list the games
                ListGames(this._gamesList.ToArray());
                _log.Debug("Adding manifest to exclusions.");
                //Add the game to the exclusion list
                this._exclusionsList.Add(torem);
            }
            //If the game was not found in the games list, notify
            //Should never have to run this, since all listed games should always be in the games list
            else _log.Warn(string.Format("Error removing game during RemoveGame(Manifest manifest = '{0}') in Form1.cs. Game not found in games list.", manifest.ToString()));
            _log.Debug("Form1.RemoveGame returning.");
        }

        //Used by 'Grouping' class for updating game info
        public void UpdateGame(Manifest oldMan, Manifest newMan)
        {
            _log.Debug(string.Format("Entering Form1.UpdateGame, oldMan is '{0}', newMan is '{1}'.", oldMan, newMan));
            _log.Debug("Attempting to remove old details ...");
            //Tries to remove the game from the games list and returns true if successful, false otherwise
            //Reason for removal is I felt it was simpler than finding and editing the game in the list
            bool rem = this._gamesList.Remove(oldMan);
            //If the game was found and removed
            if (rem)
            {
                _log.Debug("Removed.");
                _log.Debug("Adding new details.");
                //Add the updated information to the games list
                this._gamesList.Add(newMan);

                //Compare any changes to the Steam launch setting for the game
                if (oldMan.steamLaunch && !newMan.steamLaunch)
                {
                    _log.Debug("Old details had Steam launch, new details have local launch. Excluding old details from Steam scan.");
                    //Was a Steam launch but is now a local launch
                    //Exclude game from Steam game scanning
                    this._exclusionsList.Add(oldMan);
                }
                else if (oldMan.steamLaunch && newMan.steamLaunch)
                {
                    _log.Debug("Game has remained a Steam launch. Adding details to Modifications.");
                    this._gameMods.Add(new Manifest[] { oldMan, newMan });
                }
            }
            //If the game was not found
            //Should never run this, since all listed games should always be in games list
            else
            {
                _log.Warn("Update failed during UpdateGame(Manifest oldMan = '" + oldMan + "', Manifest = '" + newMan + "') in Form1.cs");
                MessageBox.Show("Update failed.");
            }
            _log.Debug("Sorting _gamesList.");
            //Sorts games list alphabetically
            this._gamesList.Sort((x, y) => x.name.CompareTo(y.name));
            _log.Debug("Calling Form1.ListGames(Manifest[] m = _gamesList) ...");
            //Re-lists games
            ListGames(this._gamesList.ToArray());
            _log.Debug("Form1.UpdateGame returning.");
        }

        //Called by clicking the 'Exclusions' button on the tool strip
        private void ExclusionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _log.Debug("Entering Form1.ExclusionsToolStripMenuItem_Click.");
            //Are there any games that have been excluded?
            //If so, show the form
            if (this._exclusionsList.Count > 0)
            {
                _log.Debug("Creating new ExclusionsForm ...");
                //Saves the list of excluded games
                //AddExcludedGames();
                //Using the form that will display all the excluded games
                using (var form = new ExclusionsForm(this._exclusionsList))
                {
                    _log.Debug("Showing form as dialog ...");
                    //Shows the Form
                    DialogResult res = form.ShowDialog();
                    //If the Form exited by the user clicking 'Confirm'
                    if (res == DialogResult.OK)
                    {
                        _log.Debug("Dialog confirmed.");
                        this._exclusionsList = form.Exclusions;
                        _log.Debug("Calling Form1.AddExcludedGames ...");
                        AddExcludedGames();
                        //Clears the list of excluded games so it can be read again without creating duplicates
                        this._exclusionsList = new List<Manifest>();
                        //Clears the list of games to prevent duplicates
                        this._gamesList = new List<Manifest>();
                        //If the user scanned for Steam games on launch
                        if (_scannedSteam)
                        {
                            _log.Debug("Starting new Steam Scan ...");
                            //Show loading animation
                            _log.Debug("Showing new LoadingForm.");
                            _loadingForm = new LoadingForm();
                            _loadingForm.Show();
                            _log.Debug("Declaring new ScanThreadWorker ...");
                            //Creates a new ScanThreadWorker and sets its ThreadDone EventHandler
                            worker = new ScanThreadWorker();
                            worker.ThreadDone += HandleThreadDone;
                            _log.Debug("Creating new Steam scan thread. Thread name > 'SteamScanThread-AfterExclusions' ...");
                            //Runs worker in a separate thread to scan for Steam games
                            Thread thread = new Thread(worker.Run)
                            { Name = "SteamScanThread-AfterExclusions" };
                            _log.Debug("Starting Steam scan thread ...");
                            thread.Start();
                        }
                        //Otherwise just set threadDone to true so WaitForThreadDone will continue
                        else
                        {
                            _log.Debug("Skipping Steam scan.");
                            threadDone = true;
                        }
                        //Call WaitForThreadDone to display games
                        WaitForThreadDone();
                    }
                }
            }
            else
            {
                _log.Debug("No exclusions to load, not showing ExclusionsForm.");
                //If there were no games excluded, there's no need to show the form, let the user know
                MessageBox.Show("There are no excluded games. Use the settings button on a game in the listing to exclude that game.");
            }
            _log.Debug("Form1.ExclusionsToolStripMenuItem_Click returning.");
        }

        //Called by clicking 'Changes' button on tool strip
        private void ChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _log.Debug("Entering Form1.ChangesToolStripMenuItem_Click.");
            _log.Debug("Creating new ModificationsForm ...");
            //Using the Form that will list all the changes saved
            using (ModificationsForm form = new ModificationsForm(this._gameMods))
            {
                _log.Debug("Showing form as dialog ...");
                //Shows the form
                DialogResult res = form.ShowDialog();
                //If the Form exited by the user pressing 'Confirm'
                if (res == DialogResult.OK)
                {
                    _log.Debug("Dialog confirmed.");
                    //Clears the list of games loaded to prevent duplicating games when listing
                    this._gamesList = new List<Manifest>();
                    //Gets the final list of changes from the modification Form
                    this._gameMods = form.Modifs;
                    _log.Debug("Calling Form1.AddGameModifications ...");
                    //Saves the list of changes
                    AddGameModifications();
                    //If the user wanted Steam games scanned for on launch
                    if (_scannedSteam)
                    {
                        _log.Debug("Starting new Steam scan ...");
                        _log.Debug("Showing new LoadingForm.");
                        //Show loading animation
                        _loadingForm = new LoadingForm();
                        _loadingForm.Show();
                        _log.Debug("Creating new ScanThreadWorker.");
                        //Create a new instance of of the ScanThreadWorker
                        worker = new ScanThreadWorker();
                        //Sets the worker's ThreadDone EventHandler to HandleThreadDone
                        worker.ThreadDone += HandleThreadDone;
                        _log.Debug("Creating new Steam scan thread. Thread name > 'SteamScanThread-AfterModifications' ...");
                        //Creates a new Thread from worker and runs it to scan for Steam games
                        Thread thread = new Thread(worker.Run)
                        { Name = "SteamScanThread-AfterModifications" };
                        _log.Debug("Starting Steam scan ...");
                        thread.Start();
                    }
                    //Otherwise just set threadDone to true, so WaitForThreadDone will continue
                    else
                    {
                        _log.Debug("Skipping Steam scan.");
                        threadDone = true;
                    }
                    //Calls WaitForThreadDone to display games
                    WaitForThreadDone();
                }
            }
            _log.Debug("Form1.ChangesToolStripMenuItem_Click returning.");
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
            _log.Debug("Entering Form1.LoggingLevelToolStripMenuItem_Click.");
            _log.Debug("Creating new SelectLogLevelForm ...");
            //``````````````````````````````````````
            using (var form = new SelectLogLevelForm(_log.LogLevel))
            {
                _log.Debug("Showing form as dialog ...");
                DialogResult res = form.ShowDialog();
                if (res == DialogResult.OK)
                {
                    _log.Debug("Dialog confirmed. Saving new log level.");
                    _log.LogLevel = form.Level;
                    cfg.LogLvl = form.Level;
                }
                else
                    _log.Debug("Dialog cancelled.");
            }
            _log.Debug("Form1.LoggingLevelToolStripMenuItem_Click returning.");
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
        //Called when the launch button is clicked
        private void Start_Click(object sender, EventArgs e)
        {
            Form1._log.Debug(string.Format("Entering Grouping(Name>'{0}').Start_Click.", this.Manifest.name));
            try
            {
                //If the game is a Steam launch, use Windows Explorer to call the Steam run game command
                if (this.Manifest.steamLaunch)
                {
                    Form1._log.Debug(string.Format("Starting Steam launch. Game id > '{0}' ...", this.Manifest.path));
                    Process.Start("explorer.exe", "steam://rungameid/" + this.Manifest.path);
                }
                else
                {
                    Form1._log.Debug("Game is a local launch.");
                    //If the game should launch using a shortcut
                    //Shortcuts can help with games that are programmed to use a working directory to search for their files. If a game is failing to launch, try to launch it with the shortcut
                    if (this.Manifest.useShortcut)
                    {
                        Form1._log.Debug("Preparing shortcut launch ...");
                        //Uses IWshRuntimeLibrary to make the shortcuts
                        IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                        //Path to the folder the launcher's executable is in, combined with the name of the shortcut
                        string spath = Path.Combine(Application.StartupPath, "temp.lnk");
                        Form1._log.Debug(string.Format("Creating shortcut at '{0}' ...", spath));
                        //Creates the shortcut
                        IWshRuntimeLibrary.IWshShortcut wsh = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(spath);
                        Form1._log.Debug(string.Format("Shortcut target path is '{0}'.", this.Manifest.path));
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
                        Form1._log.Debug(string.Format("Shortcut working directory is '{0}'.", tpath));
                        //Sets the working directory
                        wsh.WorkingDirectory = tpath;
                        wsh.Save();
                        Form1._log.Debug(string.Format("Launching through shortcut at '{0}' with args '{1}'.", spath, this.Manifest.args));
                        //Launches the game through the shortcut
                        Process.Start(spath, this.Manifest.args);
                    }
                    else
                    {
                        Form1._log.Debug(string.Format("Launching executable at '{0}' with args '{1}'.", this.Manifest.path, this.Manifest.args));
                        //Launches the game
                        Process.Start(this.Manifest.path, this.Manifest.args);
                    }
                }
                this._parent.FlagExit(new Config().CloseOnLaunch);
                if (new Config().CloseOnLaunch)
                {
                    Form1._log.Debug("Exiting based on Config.CloseOnLaunch ...");
                    Application.Exit();
                }
            }
            catch (Win32Exception ex) { Form1._log.Warn("Application cannot find path specified for game.\n" + ex.StackTrace); }
            catch (Exception ex) { Form1._log.Error(ex, ex.Message); MessageBox.Show(ex.Message); }
            Form1._log.Debug(string.Format("Grouping(Name>'{0}').Start_Click returning.", this.Manifest.name));
        }

        //Called when a mouse button is clicked on the Settings button
        private void Settings_MouseDown(object sender, MouseEventArgs e)
        {
            Form1._log.Debug(string.Format("Entering Grouping(Name>'{0}').Settings_MouseDown.", this.Manifest.name));
            switch (e.Button)
            {
                //If it is the left mouse button, call Settings_App
                case MouseButtons.Left:
                    Form1._log.Debug(string.Format("Left mouse button clicked. Calling Grouping(Name>'{0}').Settings_App ...", this.Manifest.name));
                    Settings_App(sender, null);
                    break;
                //If it is the right mouse button, pull up a menu to choose between the external settings app and the shortcut settings
                case MouseButtons.Right:
                    Form1._log.Debug("Right mouse button clicked. Showing context menu.");
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
            Form1._log.Debug(string.Format("Entering Grouping(Name>'{0}').Settings_App.", this.Manifest.name));
            try
            {
                if (this.Manifest.settingsApp.IDString().ToLower() != "sapp:default:::1")
                {
                    if (this.Manifest.settingsApp.Path != null && this.Manifest.settingsApp.Path != "")
                    {
                        try
                        {
                            Form1._log.Info(string.Format("Attempting to start external settings application '{0}' at path '{1}' ...", this.Manifest.settingsApp.Name, this.Manifest.settingsApp.Path));
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
                {
                    Form1._log.Debug(string.Format("SApp is set to default. Calling Grouping(Name>'{0}').SettingsSwtich(string test = '{1}') ...", this.Manifest.name, this.Manifest.path));
                    SettingsSwitch(this.Manifest.path);
                }
            }
            catch(Exception ez)
            {
                Form1._log.Error(ez, "Failed to launch custom settings application.");
                Short(sender, e);
            }
            Form1._log.Debug(string.Format("Grouping(Name>'{0}').Settings_App returning.", this.Manifest.name));
        }
        private void SettingsSwitch(string test)
        {
            Form1._log.Debug(string.Format("Entering Grouping(Name>'{0}').SettingsSwitch.", this.Manifest.name));
            switch (test)
            {
                //If the path is the Steam game id for Subnautica, try to launch the external app for it
                case "264710": //Subnautica
                    Form1._log.Debug("Test matched Subnautica. Testing for 'Subnautica Options.txt' ...");
                    //The external app saves its most recent path to a file in the Appdata\Roaming folder, check if that exists
                    if (File.Exists(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "Subnautica Options"), "Subnautica Options.txt")))
                    {
                        Form1._log.Debug("File does exist.");
                        try
                        {
                            Form1._log.Debug("Creating StreamReader for file ...");
                            //Reads the path from the file
                            StreamReader sr = new StreamReader(File.OpenRead(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "Subnautica Options"), "Subnautica Options.txt")));
                            string path = sr.ReadLine();
                            Form1._log.Debug(string.Format("Path to Subnautica Options application was read as {0}.", path));
                            Form1._log.Debug("Closing StreamReader.");
                            sr.Close();
                            sr.Dispose();
                            Form1._log.Debug(string.Format("Attempting to launch Subnautica Options at '{0}' ...", path));
                            //Launch the application at the path found with the argument 'nolaunch' to hide the launch button in the application
                            Process.Start(path, "nolaunch");
                        }
                        //Incase of any failure, default to shortcut settings
                        catch (Exception e)
                        {
                            Form1._log.Error(e, "Failed to launch Subnautica Options.");
                            Form1._log.Debug("Defaulting to shortcut settings.");
                            Short(this, EventArgs.Empty);
                        }
                    }
                    //Default to shortcut settings
                    else
                    {
                        Form1._log.Debug("File does not exist. Defaulting to shortcut settings.");
                        Short(this, EventArgs.Empty);
                    }
                    break;
                //If the path is the Steam game id for Terraria, try to launch the external app for it
                case "105600": //Terraria
                    Form1._log.Debug("Test matched Terraria. Testing for 'Terraria Options.txt' ...");
                    //The external app saves its most recent path to a file in the Appdata\Roaming folder, check if that exists
                    if (File.Exists(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "Terraria Options"), "Terraria Options.txt")))
                    {
                        Form1._log.Debug("File does exist.");
                        try
                        {
                            Form1._log.Debug("Creating StreamReader for file ...");
                            //Reads the path from the file
                            StreamReader sr = new StreamReader(File.OpenRead(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeath"), "Terraria Options"), "Terraria Options.txt")));
                            string path = sr.ReadLine();
                            Form1._log.Debug(string.Format("Path to Terraria Options application was read as {0}.", path));
                            Form1._log.Debug("Closing StreamReader.");
                            sr.Close();
                            sr.Dispose();
                            Form1._log.Debug(string.Format("Attempting to launch Terraria Options at '{0}' ...", path));
                            //Launch the application at the path found with the argument 'nolaunch' to hide the launch button in the application
                            Process.Start(path, "nolaunch");
                        }
                        //Incase of any failure, default to shortcut settings
                        catch (Exception e)
                        {
                            Form1._log.Error(e, "Failed to launch Subnautica Options.");
                            Form1._log.Debug("Defaulting to shortcut settings.");
                            Short(this, EventArgs.Empty);
                        }
                    }
                    //Default to shortcut settings
                    else
                    {
                        Form1._log.Debug("File does not exist. Defaulting to shortcut settings.");
                        Short(this, EventArgs.Empty);
                    }
                    break;
                //Default to shortcut settings
                default:
                    Form1._log.Debug("No match for test. Defaulting to shortcut settings.");
                    Short(this, EventArgs.Empty);
                    break;
            }
            Form1._log.Debug(string.Format("Grouping(Name>'{0}').SettingsSwitch returning.", this.Manifest.name));
        }
        //Displays the shortcut settings form
        private void Short(object sender, EventArgs e)
        {
            Form1._log.Debug(string.Format("Entering Grouping(Name>'{0}').Short.", this.Manifest.name));
            Form1._log.Debug("Creating new ShortcutSettings form ...");
            using (var form = new ShortcutSettings(this.Manifest))
            {
                Form1._log.Debug("Showing form as dialog ...");
                DialogResult res = form.ShowDialog();
                //If the changes were confirmed, update the game details
                if (form.DialogResult == DialogResult.OK)
                {
                    Form1._log.Debug("Dialog confirmed.");
                    //Old details
                    Manifest oldMan = this.Manifest;
                    Form1._log.Debug(string.Format("Creating new manifest: Name>{0}, Path>{1}, Args>{2}, SteamLaunch>{3}, UseShortcut>{4}, UseSettingsApp>{5}, SApp>{6} ...", form.GameName, form.GamePath, form.Args, form.IsSteamLaunch, form.UseShortcut, form.UseSettingsApp, form.SettingsApp));
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
                    Form1._log.Debug(string.Format("Calling Form1.UpdateGame(Manifest newMan = '{0}', Manifest oldMan = '{1}') ...", oldMan.ToString(), newMan.ToString()));
                    //Calls Form1.UpdateGame to update this game's details
                    this._parent.UpdateGame(oldMan, newMan);
                }
                //If the Delete button was clicked
                else if (form.DialogResult == DialogResult.Abort)
                {
                    Form1._log.Debug(string.Format("Deletion requested. Calling Form1.RemoveGame(Manifest manifest = '{0}') ...", this.Manifest.ToString()));
                    //Call Form1.RemoveGame to delete game from listing
                    this._parent.RemoveGame(this.Manifest);
                }
                else
                    Form1._log.Debug("Dialog cancelled.");
            }
            Form1._log.Debug(string.Format("Grouping(Name>'{0}').Short returning.", this.Manifest.name));
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
        //private string _args;
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
            /*bool n = name == m.name;
            bool p = path == m.path;
            bool a = (args ?? "") == (m.args ?? "");
            bool st = steamLaunch == m.steamLaunch;
            bool sh = useShortcut == m.useShortcut;
            bool usa = useSettingsApp == m.useSettingsApp;
            bool sa = settingsApp == m.settingsApp;
            return n && p && a && st && sh && usa && sa;*/
            return (name ?? "") == (m.name ?? "") &&
                (path ?? "") == (m.path ?? "") &&
                (args ?? "") == (m.args ?? "") &&
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
            Form1._log.Debug("Entering ScanThreadWorker.Run.");
            Form1._log.Debug("Checking Steam installation path from Windows Registry ...");
            //Gets the value that is held in the Registry to find out where Steam is located
            object ret = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", "NO VAL");
            //If Steam install path is not found in registry, the user will have to input the path manually
            if (ret == null || (string)ret == "NO VAL")
            {
                Form1._log.Warn("Steam is not installed as expected, scanning thread is unable to continue.");
            }
            else
            {
                Form1._log.Debug(string.Format("Steam installation path found as '{0}'.", (string)ret));
                //Scan default games dir
                string defPath = (string)ret + "\\steamapps";
                Form1._log.Debug("Getting '.acf' files from Steam install path ...");
                //Gets full path to each Steam app manifest file in the default game directory
                string[] sa = Directory.GetFiles(defPath, "*.acf");
                //As long as there is at least one file in that directory
                if (sa.Length > 0)
                {
                    Form1._log.Debug(string.Format("{0} '.acf' file(s) found. Looping through each to fetch game details ...", sa.Length));
                    //Loops through all found app manifest files
                    foreach (string s in sa)
                    {
                        try
                        {
                            Form1._log.Debug("Creating StreamReader for current acf file ...");
                            //Used to store data of the current game
                            Manifest manifest = new Manifest();
                            StreamReader sr = new StreamReader(File.OpenRead(s));
                            //Padding to reach the Steam id of the game
                            sr.ReadLine(); sr.ReadLine();
                            string[] t = sr.ReadLine().Split('"');
                            Form1._log.Debug(string.Format("Steam app id > {0}.", t[t.Length - 2]));
                            //Saves the Steam id of the game
                            manifest.path = t[t.Length - 2];
                            //Padding to reach the name of the game
                            sr.ReadLine();
                            t = null; t = sr.ReadLine().Split('"');
                            Form1._log.Debug(string.Format("Steam app name > {0}.", t[t.Length - 2]));
                            //Saves the name of the game
                            manifest.name = t[t.Length - 2];
                            manifest.steamLaunch = true;
                            Form1._log.Debug("Closing StreamReader.");
                            sr.Close();
                            sr.Dispose();
                            //Sets useShortcut to false since launching will not need to use a shortcut for any reason
                            manifest.useShortcut = false;
                            manifest.useSettingsApp = false;
                            manifest.settingsApp = null;
                            manifest.args = "";
                            //Adds the game to the list of games to be returned if it is not already in the list
                            //Should always evaluate true
                            if (!val.Contains(manifest))
                            {
                                Form1._log.Debug("Adding manifest to ScanThreadWorker.Value.");
                                val.Add(manifest);
                            }
                            else
                                Form1._log.Debug("Manifest already in ScanThreadWorker.Value, skipping.");
                        }
                        catch (Exception e) { Form1._log.Error(e, "Exception while scanning Steam default directory."); }
                    }
                }
                else
                    Form1._log.Debug("No acf files in default Steam directory.");
                //Scan user created game dirs
                try
                {
                    Form1._log.Debug("Attempting to read Steam library folders from 'libraryfolders.vdf' ...");
                    //Path to Steam's file containing paths to user-created library folders
                    string libraryFoldersFile = (string)ret + "\\steamapps\\libraryfolders.vdf";
                    //List of user-created library folders to be scanned
                    List<string> libraryFolders = new List<string>();
                    Form1._log.Debug("Creating StreamReader for file ...");
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
                        Form1._log.Debug(string.Format("Found library folder at '{0}'.", t[t.Length - 2]));
                        //Saves the path of the library folder
                        libraryFolders.Add(t[t.Length - 2]);
                    }
                    Form1._log.Debug("Closing StreamReader.");
                    streamReader.Close();
                    streamReader.Dispose();
                    Form1._log.Debug("Looping through each library folder found ...");
                    //Loops through each user-created library folder
                    foreach (string folder in libraryFolders.ToArray<string>())
                    {
                        Form1._log.Debug(string.Format("Current library folder is in path '{0}'.", folder));
                        //Directory where the Steam app manifest files are
                        string path = folder + "\\steamapps";
                        Form1._log.Debug("Getting '.acf' files in current library folder ...");
                        //Gets all app manifest files in the directory
                        string[] files = Directory.GetFiles(path, "*.acf");
                        Form1._log.Debug(string.Format("{0} '.acf' file(s) found. Looping through each to fetch game details ...", sa.Length));
                        //Loops through each file to read each game
                        foreach (string file in files)
                        {
                            Form1._log.Debug("Creating StreamReader for current acf file ...");
                            //Used to hold data for the game
                            Manifest manifest = new Manifest();
                            StreamReader sr = new StreamReader(File.OpenRead(file));
                            //Padding to reach the Steam application id
                            sr.ReadLine(); sr.ReadLine();
                            string[] t = sr.ReadLine().Split('"');
                            Form1._log.Debug(string.Format("Steam app id > {0}.", t[t.Length - 2]));
                            //Saves app id
                            manifest.path = t[t.Length - 2];
                            //Padding to reach app name
                            sr.ReadLine();
                            t = null; t = sr.ReadLine().Split('"');
                            Form1._log.Debug(string.Format("Steam app name > {0}.", t[t.Length - 2]));
                            //Saves name
                            manifest.name = t[t.Length - 2];
                            manifest.steamLaunch = true;
                            Form1._log.Debug("Closing StreamReader.");
                            sr.Close();
                            sr.Dispose();
                            //Sets useShortcut to false since launching will not need to use a shortcut for any reason
                            manifest.useShortcut = false;
                            manifest.useSettingsApp = false;
                            manifest.settingsApp = null;
                            manifest.args = "";
                            //Adds the game to the list of games to be returned if it is not already in the list
                            //Should always evaluate true
                            if (!val.Contains(manifest))
                            {
                                Form1._log.Debug("Adding manifest to ScanThreadWorker.Value.");
                                val.Add(manifest);
                            }
                            else
                                Form1._log.Debug("Manifest already in ScanThreadWorker.Value, skipping.");
                        }
                    }
                }
                catch (Exception e) { Form1._log.Error(e, "Exception while loading games from user-created Steam library folders"); }
            }
            Form1._log.Debug("Sorting ScanThreadWorker.Value.");
            //Sorts the found Games in alphabetical order
            val.Sort((x, y) => x.name.CompareTo(y.name));
            Form1._log.Debug("Finished scanning. Invoking ScanThreadWorker.ThreadDone event handler ...");
            ThreadDone?.Invoke(this, EventArgs.Empty);
        }
    }
}
