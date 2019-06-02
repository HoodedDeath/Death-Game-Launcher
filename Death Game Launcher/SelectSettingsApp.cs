//using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;

namespace Death_Game_Launcher
{
    public partial class SelectSettingsApp : Form
    {
        private SettingsApp[] _apps;
        private Manifest manifest;
        public SelectSettingsApp(Manifest manifest)
        {
            InitializeComponent();
            this.manifest = manifest;
            _apps = Read();
            List(_apps);
            //select app
            if (manifest.settingsApp != null)
            {
                foreach (RadioButton rb in this.Controls.Find("radioButton", true))
                {
                    if (rb.Text.ToLower() == this.manifest.settingsApp.Name.ToLower())
                    {
                        rb.Checked = true;
                        //break;
                        //Look through _apps
                        foreach (SettingsApp a in _apps)
                        {
                            if (a.Name == rb.Text)
                            {
                                this.SelectedApp = a;
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            foreach (SettingsApp a in _apps)
                Form1._log.Info(a.IDString());
        }
        private SettingsApp[] Read()
        {
            List<SettingsApp> apps = new List<SettingsApp>();
            StreamReader sr = new StreamReader(File.OpenRead(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoodedDeathApplications", "DLG", "sapps.cfg")));
            List<SAppJson> sa = JsonConvert.DeserializeObject<List<SAppJson>>(sr.ReadToEnd());
            sr.Close();
            sr.Dispose();
            foreach (SAppJson s in sa)
                apps.Add(s.SApp);
            /*string regpath = "HKEY_CURRENT_USER\\Software\\HoodedDeathApplications\\DeathGameLauncher";
            string[] ret = (string[])Registry.GetValue(regpath, "SettingsApps", new string[0]);
            if (ret != null && ret.Length > 0)
            {
                foreach (string s in ret)
                {
                    string[] arr = s.Split('"');
                    apps.Add(new SettingsApp(arr[1], bool.Parse(arr[3]), arr[5], arr[7], bool.Parse(arr[9])));
                }
            }*/
            return apps.ToArray();
        }
        private int[] location = new int[] { 12, -11 };
        private void List(SettingsApp[] apps)
        {
            location = new int[] { 12, -11 };
            foreach (SettingsApp app in apps)
            {
                RadioButton rb = new RadioButton()
                {
                    Text = app.Name,
                    Location = new Point(location[0], location[1] += 23),
                    Name = "radioButton"
                };
                rb.Click += new EventHandler(Radio_Clicked);
                this.Controls.Add(rb);
                rb = null;
            }
        }
        private void Radio_Clicked(object sender, EventArgs e)
        {
            //MessageBox.Show(((RadioButton)sender).Text);
            string s = ((RadioButton)sender).Text;
            foreach (SettingsApp sa in _apps)
            {
                if (s == sa.Name)
                {
                    SelectedApp = sa;
                    break;
                }
            }
            MessageBox.Show(SelectedApp.IDString());
        }

        private void AcceptBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void CancelBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        private void SelectSettingsApp_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                CancelBtn_Click(sender, e);
            else if (e.KeyCode == Keys.Enter)
                AcceptBtn_Click(sender, e);
        }
        private void SelectSettingsApp_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult == DialogResult.OK)
                e.Cancel = false;
            else
                e.Cancel = MessageBox.Show("Are you sure you want to cancel?", "Cancel?", MessageBoxButtons.YesNo) != DialogResult.Yes;
        }

        public SettingsApp SelectedApp { get; set; }
    }
}
