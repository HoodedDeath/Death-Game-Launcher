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
        public ShortcutSettings(string name, string path, bool isSteamLaunch, bool useShortcut)
        {
            InitializeComponent();
            this.GameName = name;
            this.GamePath = path;
            this.IsSteamLaunch = isSteamLaunch;
            this.UseShortcut = useShortcut;
        }

        ToolTip tip = new ToolTip();
        private void LaunchPathLabel_MouseHover(object sender, EventArgs e)
        {
            tip.Show("The path used to launch the game. If it is a Steam launch, the launch path will be shown as the game's Steam ID number.", this, ((Control)sender).Location, 10000);
        }
        private void LaunchPathLabel_MouseLeave(object sender, EventArgs e)
        {
            tip.Hide(this);
        }

        private void SteamCheckBox_MouseHover(object sender, EventArgs e)
        {
            tip.Show("Whether or not this game will be launched through Steam", this, ((Control)sender).Location, 10000);
        }
        private void SteamCheckBox_MouseLeave(object sender, EventArgs e)
        {
            tip.Hide(this);
        }

        private void ShortcutCheckBox_MouseHover(object sender, EventArgs e)
        {
            tip.Show("Whether or not the launcher will create a temporary shortcut to launch the game, as long as it is not a Steam launch", this, ((Control)sender).Location, 10000);
        }
        private void ShortcutCheckBox_MouseLeave(object sender, EventArgs e)
        {
            tip.Hide(this);
        }

        private void SteamCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.IsSteamLaunch = ((CheckBox)sender).Checked;
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove this game from the listing?", "Remove Game", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                DialogResult = DialogResult.Abort;
                Close();
            }
        }
        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void Confirm_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
        private void ShortcutSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
                e.Cancel = false;
            else if (DialogResult == DialogResult.Abort)
                e.Cancel = false;
            else
                e.Cancel = MessageBox.Show("Any changes will be lost if you exit without confirming.", "Discard Changes", MessageBoxButtons.YesNo) == DialogResult.No;
        }

        public bool IsSteamLaunch { get { return this.steamCheckBox.Checked; } set { this.steamCheckBox.Checked = value; } }
        public bool UseShortcut { get { return this.shortcutCheckBox.Checked; } set { this.shortcutCheckBox.Checked = value; } }
        public string GameName { get { return this.nameBox.Text; } set { this.nameBox.Text = value; } }
        public string GamePath { get { return this.pathBox.Text; } set { this.pathBox.Text = value; } }
    }
}
