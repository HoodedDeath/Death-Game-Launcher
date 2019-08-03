using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;

namespace Death_Game_Launcher
{
    public partial class SelectSettingsApp : Form
    {
        //Array to hold all the available settings apps
        private SettingsApp[] _apps;
        //Location used for setting up the Radio Buttons
        private int[] location = new int[] { 12, -11 };

        //The settings app selected by the user, to be read by the parent form
        public SettingsApp SelectedApp { get; set; }

        public SelectSettingsApp(Manifest manifest)
        {
            InitializeComponent();
            //Read in the available settings apps and store them in '_apps'
            _apps = Read();
            //List out the available settings app
            List(_apps);

            //Find which settings app is declared in the given manifest

            //If the settings app in the given manifest is not empty
            if (manifest.settingsApp != null)
            {
                //Loop through each radio button in the form
                foreach (RadioButton rb in this.Controls.Find("radioButton", true))
                {
                    //If the text of the current radio button does not equal the name of the settings app in the given manifest, skip to the beginning of the loop
                    if (rb.Text.ToLower() != manifest.settingsApp.Name.ToLower()) continue;
                    //Check the radio button
                    rb.Checked = true;
                    //Loop through '_apps' to find the corresponding settings app
                    foreach (SettingsApp a in _apps)
                    {
                        //if the name of settings app 'a' does not equal the current radio button, skip to the beginning of the loop
                        if (a.Name != rb.Text) continue;
                        //Store 'a' in SelectedApp
                        this.SelectedApp = a;
                        //Break out of this loop, since the matching settings app has been found
                        break;
                    }
                    //Break out of the loop, since we've found the matching radio button
                    break;
                }
            }
            //Otherwise, set the app to default
            else { } // No work
            foreach (SettingsApp a in _apps)
                Form1._log.Info(a.IDString());
        }
        //Reads the available settings apps from path "%APPDATA%\HoodedDeathApplications\DLG\sapps.cfg"
        private SettingsApp[] Read()
        {
            //List to store the avaible settings apps
            List<SettingsApp> apps = new List<SettingsApp>();
            //Stream reader to read from the cfg file and give input to JsonConvert.DeserializeObject
            StreamReader sr = new StreamReader(File.OpenRead(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeathApplications", "DLG", "sapps.cfg")));
            //Converts the data from the cfg file to a usable object, containing settings apps
            List<SAppJson> sa = JsonConvert.DeserializeObject<List<SAppJson>>(sr.ReadToEnd());
            //Close the Stream Reader
            sr.Close();
            sr.Dispose();
            //Add each settings app in 'sa' to 'apps' list
            foreach (SAppJson s in sa)
                apps.Add(s.SApp);
            //Return the array of avaible settings apps
            return apps.ToArray();
        }
        //Make a radio button for each available settings app
        private void List(SettingsApp[] apps)
        {
            //Set 'location' to default values
            location = new int[] { 12, -11 };
            //Loop through each settings app given in the 'apps' parameter
            foreach (SettingsApp app in apps)
            {
                //Create a new Radio button
                RadioButton rb = new RadioButton()
                {
                    Text = app.Name,
                    Location = new Point(location[0], location[1] += 23),
                    Name = "radioButton"
                };
                //Give the new radio button a Click event handler
                rb.Click += new EventHandler(Radio_Clicked);
                //Add the radio button to the form controls
                this.Controls.Add(rb);
            }
        }
        //Called when the user clicks one of the Radio Buttons
        private void Radio_Clicked(object sender, EventArgs e)
        {
            //The text of the sender radio button
            string s = ((RadioButton)sender).Text;
            //Loop through each available settings app
            foreach (SettingsApp sa in _apps)
            {
                //If 's' does not equal the name of the current settings app, skip to the beginning of the loop
                if (s != sa.Name) continue;
                //Store the current settings app in 'SelectedApp'
                SelectedApp = sa;
                //Break out of the loop since the matching settings app has been found
                break;
            }
        }
        //Called when the user clicks the accept button
        private void AcceptBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        //Called when the user clicks the cancel button
        private void CancelBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        //Called when the user presses any key on the keyboard
        private void SelectSettingsApp_KeyDown(object sender, KeyEventArgs e)
        {
            //If the user pressed escape, act as if the user clicked the cancel button
            if (e.KeyCode == Keys.Escape)
                CancelBtn_Click(sender, e);
            //If the user pressed enter, act as if the user clicked the accept button
            else if (e.KeyCode == Keys.Enter)
                AcceptBtn_Click(sender, e);
        }
        //Called as the Form is closing
        private void SelectSettingsApp_FormClosing(object sender, FormClosingEventArgs e)
        {
            //If the accept button was clicked, close without any pop-up
            if (this.DialogResult == DialogResult.OK)
                e.Cancel = false;
            //Otherwise, prompt the user if they are sure they want to cancel
            else
                e.Cancel = MessageBox.Show("Are you sure you want to cancel?", "Cancel?", MessageBoxButtons.YesNo) != DialogResult.Yes;
        }
    }
}
