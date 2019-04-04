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
    public partial class SelectLogLevelForm : Form
    {
        private bool changed = false;

        public int Level { get; set; }
        private readonly int original;

        public SelectLogLevelForm(int lvl)
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
            switch (lvl)
            {
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

        private void Radio_Click(object sender, EventArgs e)
        {
            switch (((RadioButton)sender).Tag)
            {
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
            if (Level != original)
                changed = true;
        }

        private void SelectLogLevelForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                Close();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void Accept_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void SelectLogLevelForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = this.DialogResult == DialogResult.OK ? false : changed ? MessageBox.Show("All changes will be lost", "Are you sure?", MessageBoxButtons.YesNo) != DialogResult.Yes : false;
        }
    }
}
