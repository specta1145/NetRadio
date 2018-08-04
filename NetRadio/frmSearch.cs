using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace NetRadio
{
    public partial class frmSearch : Form
    {
        internal TextBox tbString { get { return tbSearch; } }

        public frmSearch()
        {
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void frmSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                { Close(); } // Formular schließen
            }
        }

        private void linkLblRadioBrowserInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try { Process.Start("http://www.radio-browser.info/"); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
        }

        private void lblSearchButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void lblSearchButton_MouseDown(object sender, MouseEventArgs e)
        {
            lblSearchButton.BackColor = System.Drawing.Color.LightSteelBlue;
        }

        private void lblSearchButton_MouseEnter(object sender, EventArgs e)
        {
            lblSearchButton.BackColor = System.Drawing.Color.AliceBlue;
        }

        private void lblSearchButton_MouseLeave(object sender, EventArgs e)
        {
            lblSearchButton.BackColor = System.Drawing.SystemColors.Control;
        }
    }
}
