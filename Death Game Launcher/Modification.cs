namespace Death_Game_Launcher
{
    class Modification
    {
        public string OldName;
        public string OldPath;
        public string OldArgs;
        [Newtonsoft.Json.JsonProperty("OldSteamLaunch")]
        public bool OldSteam;
        [Newtonsoft.Json.JsonProperty("OldShortcutLaunch")]
        public bool OldShort;
        public bool OldUseSApp;
        public string OldSApp;
        public string NewName;
        public string NewPath;
        public string NewArgs;
        [Newtonsoft.Json.JsonProperty("NewSteamLaunch")]
        public bool NewSteam;
        [Newtonsoft.Json.JsonProperty("NewShortcutLaunch")]
        public bool NewShort;
        public bool NewUseSApp;
        public string NewSApp;
    }
}
