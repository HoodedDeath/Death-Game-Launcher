using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Death_Game_Launcher
{
    class Modification
    {
        public string OldName;
        public string OldPath;
        public string OldArgs;
        [JsonProperty("OldSteamLaunch")]
        public bool OldSteam;
        [JsonProperty("OldShortcutLaunch")]
        public bool OldShort;
        public bool OldUseSApp;
        public string OldSApp;
        public string NewName;
        public string NewPath;
        public string NewArgs;
        [JsonProperty("NewSteamLaunch")]
        public bool NewSteam;
        [JsonProperty("NewShortcutLaunch")]
        public bool NewShort;
        public bool NewUseSApp;
        public string NewSApp;
    }
}
