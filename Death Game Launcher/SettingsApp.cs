using System;
using System.IO;

namespace Death_Game_Launcher
{
    public class SettingsApp
    {
        //Gets or sets the name of the settings app
        public string Name { get; set; }
        //Gets or sets the path of the settings app, enabling the use of ParsePath when setting
        private string _path;
        public string Path { get { return _path; } set { _path = ParsePath(value); } }
        //Gets or sets the arguments of the settings app
        public string Args { get; set; }
        //Gets or sets the value saying whether or not the settings app should be allowed to be removed
        public bool HereByDefault { get; set; }

        //Declare an empty settings app, all values will need to be filled in using auto property calls
        public SettingsApp()
        {

        }
        /// <summary>
        /// Declares a new Settings App
        /// </summary>
        /// <param name="name">The name of the Settings App</param>
        /// <param name="readPathFromFile">If the path to the executable path is held in a file</param>
        /// <param name="path">The path to either the executable or the file with the executable path in it</param>
        /// <param name="args">The arguments for the Settings App</param>
        /// <param name="hereByDefault">Is this app always here (made by me)</param>
        public SettingsApp(string name, bool readPathFromFile, string path, string args, bool hereByDefault)
        {
            //Set name
            Name = name;
            //Set arguments
            Args = args;
            //Set HereByDefault
            HereByDefault = hereByDefault;
            //If the exe path is in a file, have to read that file
            if (readPathFromFile)
            {
                try
                {
                    //Parse the path given to the file that'll be read
                    string s = ParsePath(path);
                    //Open a StreamReader for the file
                    StreamReader sr = new StreamReader(s);
                    //Read the path from the file
                    //The path in this file needs to be on the first line and needs to be the only thing in the file
                    Path = sr.ReadLine();
                    //Close the StreamReader
                    sr.Close();
                    sr.Dispose();
                }
                catch (FileNotFoundException e)
                {
                    //Log error
                    Form1._log.Error(e, "Failed to read path file for settings app " + name + ".");
                }
            }
            //Otherwise, just parse the path and store it
            else
                Path = ParsePath(path);
        }
        //Parses a path given in the parameter
        //To use a special folder with the method's syntax, the path needs to be in the format of '$@<SpecialFolderName>$\<RestOfPath>
        //Replace '<SpecialFolderName>' with one of the values in the switch below
        //Replace '<RestOfPath>' with the trailing end of the path, after the path to the special folder
        private string ParsePath(string path)
        {
            //String to store the eventual return value
            string s = "";
            //If the given path contains at least one '$' (the first symbol representing a special folder)
            if (path.Contains("$"))
            {
                //Split the path at each '$'
                string[] arr = path.Split('$');
                //Loop through each section after the split
                foreach (string st in arr)
                {
                    //If the current string contains '@' (the second symbol representing a special folder)
                    if (st.Contains("@"))
                    {
                        //The switch determines which special folder is wanted.
                        //The cases represent all currently avaible SpecialFolder options in Environment
                        //The default just adds the string that was meant to be one of the special folders, hopefully making it easier to notice when you've misspelled a special folder name
                        switch (st.Split('@')[1].ToLower())
                        {
                            case "applicationdata":
                                s += Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                                break;
                            case "desktop":
                                s += Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                                break;
                            case "favorites":
                                s += Environment.GetFolderPath(Environment.SpecialFolder.Favorites);
                                break;
                            case "localapplicationdata":
                                s += Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                                break;
                            case "mydocuments":
                                s += Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                                break;
                            case "mymusic":
                                s += Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                                break;
                            case "mypictures":
                                s += Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                                break;
                            case "myvideos":
                                s += Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                                break;
                            case "recent":
                                s += Environment.GetFolderPath(Environment.SpecialFolder.Recent);
                                break;
                            case "startup":
                                s += Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                                break;
                            case "userprofile":
                                s += Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                                break;
                            default:
                                s += st.Split('@')[1];
                                break;
                        }
                    }
                    //Otherwise, this string is at the end of the previous special folder section, simply add it to the end of the return string
                    else
                        s += st;
                }
            }
            //Otherwise, the given path doesn't contain any special folders that need parsing, add the unchanged path to the return string
            else
                s = path;
            //Return the string that's been added to and parsed
            return s;
        }
        //Generates an identifier string for the Settings App
        public string IDString()
        {
            //Creates the string to be returned
            //The return string always starts with 'SApp:<Name>:'
            string ret = "SApp:" + this.Name + ":";
            //Splits the path on each '\' to easily get just the actual file name
            string[] arr = this.Path.Split('\\');
            //Adds '<ExeName>:<Arguments>:' to the return string
            ret += arr[arr.Length - 1] + ":" + this.Args + ":";
            //if HereByDefault is true, adds a '1' to the end of the return string
            //Otherwise, adds a '0' to the end of the return string
             ret += (this.HereByDefault ? "1" : "0");
            //Return the identifier string
            return ret;
        }
    }
}
