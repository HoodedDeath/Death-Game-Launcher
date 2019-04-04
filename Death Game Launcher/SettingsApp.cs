using System;
using System.IO;

namespace Death_Game_Launcher
{
    public class SettingsApp
    {
        public SettingsApp(string name, bool readPathFromFile, string path, string args, bool hereByDefault)
        {
            Name = name;
            Args = args;
            HereByDefault = hereByDefault;

            if (readPathFromFile)
            {
                //Read File
                string s = ParsePath(path);
                StreamReader sr = new StreamReader(s);
                Path = sr.ReadLine();
                sr.Close();
                sr.Dispose();
            }
            else
                Path = path;
        }
        private string ParsePath(string path)
        {
            string s = "";

            if (path.Contains("$"))
            {
                string[] arr = path.Split('$');
                foreach (string st in arr)
                {
                    if (st.Contains("@"))
                    {
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
                        }
                    }
                    else
                        s += st;
                }
            }

            return s;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public string Args { get; set; }
        public bool HereByDefault { get; set; }

        public string IDString()
        {
            string ret = "SApp:" + this.Name + ":";
            
            string[] arr = this.Path.Split('\\');
            ret += arr[arr.Length - 1] + ":" + this.Args + ":" + (this.HereByDefault ? "1" : "0");
            
            return ret;
        }
    }
}
