using System;
using System.Windows.Forms;

namespace Death_Game_Launcher
{
    public partial class ShortcutSettings : Form
    {
        //Gets or sets the checked value of the steam check box
        public bool IsSteamLaunch { get { return this.steamCheckBox.Checked; } set { this.steamCheckBox.Checked = value; } }
        //Gets or sets the checked value of the shortcut check box
        public bool UseShortcut { get { return this.shortcutCheckBox.Checked; } set { this.shortcutCheckBox.Checked = value; } }
        //Gets or sets the text value of the name text box
        public string GameName { get { return this.nameBox.Text; } set { this.nameBox.Text = value; } }
        //Gets or sets the text value of the path text box (will be a Steam app id number if the game is a settings app)
        public string GamePath { get { return this.pathBox.Text; } set { this.pathBox.Text = value; } }
        //Gets or sets the text value of the arguments text box
        public string Args { get { return this.argsTextBox.Text; } set { this.argsTextBox.Text = value; } }
        //Gets or sets the checked value of the settings app check box
        public bool UseSettingsApp { get { return this.settingsAppCheckbox.Checked; } set { this.settingsAppCheckbox.Checked = value; } }
        //Gets or sets the Settings App the game will use
        public SettingsApp SettingsApp { get; set; }
        //Gets or sets the Manifest used
        public Manifest Manifest { get; set; }

        //Declares a new instance of this form with a given manifest
        public ShortcutSettings(Manifest manifest)
        {
            InitializeComponent();
            //Set the manifest
            this.Manifest = manifest;
            //Set the name from manifest.name
            this.GameName = manifest.name;
            //Sets the path (or Steam id) from manifest.path
            this.GamePath = manifest.path;
            //Sets the steam launch from manifest.steamLaunch
            this.IsSteamLaunch = manifest.steamLaunch;
            //Sets the shortcut launch from manifest.useShortcut (not needed for Steam launch)
            this.UseShortcut = manifest.useShortcut;
            //Sets the settings app check box from manifest.useSettingsApp
            this.settingsAppCheckbox.Checked = manifest.useSettingsApp;
            //Call CheckChanged event handler for the settings app check box
            SettingsAppCheckbox_CheckedChanged(this, EventArgs.Empty);
            //Sets arguments from manifest.args
            this.Args = manifest.args;
        }
        //When the user clicks the Steam Launch check box
        private void SteamCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.IsSteamLaunch = ((CheckBox)sender).Checked;
        }
        //Called when the user clicks the Delete button
        private void Delete_Click(object sender, EventArgs e)
        {
            //Prompts the user if they want to remove this game
            if (MessageBox.Show("Are you sure you want to remove this game from the listing?", "Remove Game", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                //DialogResult.Abort is used as a flag to the parent Form to remove this game
                DialogResult = DialogResult.Abort;
                //Closes this form
                Close();
            }
        }
        //Called when the user clicks the Cancel button
        private void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        //Called when the user clicks the Confirm button
        private void Confirm_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
        //Called as the Form is closing
        private void ShortcutSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            //If the Confirm button was clicked, allow close
            if (DialogResult == DialogResult.OK)
                e.Cancel = false;
            //If the Remove button was clicked and confirmed, allow close
            else if (DialogResult == DialogResult.Abort)
                e.Cancel = false;
            //Otherwise, prompt the user if they want to cancel
            else
                e.Cancel = MessageBox.Show("Any changes will be lost if you exit without confirming.", "Discard Changes", MessageBoxButtons.YesNo) == DialogResult.No;
        }
        //When the user clicks the use settings app check box
        private void SettingsAppCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            this.UseSettingsApp = settingsAppCheckbox.Checked;
            //Show/Hide the select settings app button
            if (this.settingsAppCheckbox.Checked)
                this.selectAppButton.Show();
            else
                this.selectAppButton.Hide();
        }
        //Called when the user clicks the select settings app button
        private void SelectAppButton_Click(object sender, EventArgs e)
        {
            //Declares a new SelectSettingsApp Form
            using (var form = new SelectSettingsApp(this.Manifest))
            {
                //Opens form as a dialog box, and stores the dialog result
                DialogResult res = form.ShowDialog();
                //If form returned a DialogResult of OK
                if (res == DialogResult.OK)
                {
                    //Copy the manifest stored to here
                    Manifest m = this.Manifest;
                    //Set the settings app from form.SelectedApp
                    m.settingsApp = form.SelectedApp;
                    //Store m in the global manifest variable
                    this.Manifest = m;
                    //Set the form's SettingsApp variable from form.SelectedApp
                    this.SettingsApp = form.SelectedApp;
                }
            }
        }
        //Mouse Hover tool tip and stuff
        private ToolTip tip = new ToolTip();
        private void LaunchPathLabel_MouseHover(object sender, EventArgs e)
        {
            tip.Show("The path used to launch the game. If it is a Steam launch, the launch path will be shown as the game's Steam ID number.", this, ((Control)sender).Location, 10000);
        }
        private void SteamCheckBox_MouseHover(object sender, EventArgs e)
        {
            tip.Show("Whether or not this game will be launched through Steam", this, ((Control)sender).Location, 10000);
        }
        private void ShortcutCheckBox_MouseHover(object sender, EventArgs e)
        {
            tip.Show("Whether or not the launcher will create a temporary shortcut to launch the game, as long as it is not a Steam launch", this, ((Control)sender).Location, 10000);
        }
        private void SettingsAppCheckBox_MouseHover(object sender, EventArgs e)
        {
            tip.Show("Use an app settings application instead of the shortcut settings window when clicking the settings button", this, ((Control)sender).Location, 10000);
        }
        private void Global_MouseLeave(object sender, EventArgs e)
        {
            tip.Hide(this);
        }
    }
}
