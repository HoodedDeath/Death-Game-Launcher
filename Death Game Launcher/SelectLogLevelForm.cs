using System;
using System.Windows.Forms;

namespace Death_Game_Launcher
{
    public partial class SelectLogLevelForm : Form
    {
        //Has the selected level changed
        private bool changed = false;
        //Property to get and set the selected level
        public int Level { get; set; }
        //The original level value given to the Form, used so the Form can silently exit if the user clicks Cancel or Escape and there hasn't been changes to which RadioButton is selected
        private readonly int original;
        //Initialization
        public SelectLogLevelForm(int lvl)
        {
            InitializeComponent();
            //Set the Form's DialogResult to Cancel at the beginning, incase the Form closes and doesn't trigger SelectLogLevelForm_FormClosing
            this.DialogResult = DialogResult.Cancel;
            //Decide which RadioButton should be checked, based on the log level given for the parameter
            //'original = Level = ' sets Level and original to the same value
            switch (lvl)
            {
                case Logger.DEBUG:
                    original = Level = Logger.DEBUG;
                    debugRadio.Checked = true;
                    break;
                case Logger.INFO:
                    original = Level = Logger.INFO;
                    infoRadio.Checked = true;
                    break;
                case Logger.WARN:
                    original = Level = Logger.WARN;
                    warnRadio.Checked = true;
                    break;
                case Logger.ERROR:
                    original = Level = Logger.ERROR;
                    errorRadio.Checked = true;
                    break;
            }
        }
        //Called when any of the RadioButtons are clicked
        private void Radio_Click(object sender, EventArgs e)
        {
            //Decides which RadioButton was clicked by using the Tag value of sender, since a RadioButton object cannot be used for a case
            switch (((RadioButton)sender).Tag)
            {
                case "DEBUG":
                    Level = Logger.DEBUG;
                    break;
                case "INFO":
                    Level = Logger.INFO;
                    break;
                case "WARN":
                    Level = Logger.WARN;
                    break;
                case "ERROR":
                    Level = Logger.ERROR;
                    break;
            }
            //If the level that was just set is not the original value from when the Form started, set changed to true
            if (Level != original)
                changed = true;
        }
        //Called when a keyboard key is clicked
        private void SelectLogLevelForm_KeyDown(object sender, KeyEventArgs e)
        {
            //Cancel when the user clicks Escape
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                Close();
            }
            //Accept when the user clicks Enter
            else if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.OK;
                Close();
            }
        }
        //Called when Accept button is clicked, sets DialogResult to OK and closes Form
        private void Accept_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }
        //Called when Cancel button is clicked, sets DialogResult to Cancel and closes Form
        private void Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }
        private void SelectLogLevelForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //If the DialogResult is OK (user clicked Accept), e.Cancel = false (allow Form close)
            //Otherwise (user clicked Cancel or Escape key):
            // If there were changes made, popup MessageBox for confirming cancel
            // Otherwise, e.Cancel = false (allow Form close)
            e.Cancel = this.DialogResult == DialogResult.OK ? false : changed ? MessageBox.Show("All changes will be lost", "Are you sure?", MessageBoxButtons.YesNo) != DialogResult.Yes : false;
        }
    }
}
