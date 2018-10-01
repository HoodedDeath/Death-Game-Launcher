using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Death_Game_Launcher
{
    public partial class ShortcutSettings : Form
    {
        /*public ShortcutSettings()
        {
            InitializeComponent();
        }*/
        public ShortcutSettings(string name, string id, bool isSteamLaunch)
        {
            InitializeComponent();
            nameLabel.Text = name;
            idLabel.Text = id;
        }
        public ShortcutSettings(string name, string id)
        {
            InitializeComponent();
            nameLabel.Text = name;
            idLabel.Text = id;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
