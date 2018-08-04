using System;
using System.Diagnostics;
using System.Xml; // XmlTextReader
using System.Windows.Forms;
using System.Net; // WebClient
using System.Reflection; // Assembly
using System.Text.RegularExpressions;
using System.Drawing;

namespace NetRadio
{
    public partial class frmBrowser : Form
    {
        string radioStation;
        string radioURL;

        internal string SelectedStation { get { return radioStation; } }
        internal string SelectedURL { get { return radioURL; } }

        Version curVersion = Assembly.GetExecutingAssembly().GetName().Version;

        public frmBrowser(string seachText, Point point)
        {
            InitializeComponent();
            Text = "Search results";
            bool success = false;

            using (frmWait f2 = new frmWait(new Point(point.X, point.Y)))
            {
                f2.Show(); // Please wait...
                f2.Update();

                using (var wClient = new WebClient())
                {
                    try
                    {
                        wClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                        wClient.Headers.Add("Cache-Control", "no-cache");
                        wClient.Headers.Add("user-agent", Application.ProductName + "/" + new Regex(@"^\d+\.\d+").Match(curVersion.ToString()).Value);
                        using (var stream = wClient.OpenRead("http://www.radio-browser.info/webservice/xml/stations/byname/" + Regex.Replace(seachText, @"\s+", " ")))
                        {
                            if (stream != null)
                            {
                                stream.ReadTimeout = 3000; // Millisekunden; bestimmt, wie lange der Stream versucht, Lesevorgänge durchzuführen
                                using (XmlTextReader xtReader = new XmlTextReader(stream))
                                {
                                    xtReader.WhitespaceHandling = WhitespaceHandling.None; // Return no Whitespace and no SignificantWhitespace nodes.
                                    try
                                    {
                                        string radioName = string.Empty; // = "prog" + i.ToString();
                                        while (xtReader.Read())
                                        {
                                            if (xtReader.NodeType == XmlNodeType.Element && xtReader.LocalName == "station")
                                            {
                                                xtReader.MoveToAttribute("name");
                                                radioName = xtReader.Value;
                                                if (radioName.Length > 0)
                                                {
                                                    success = true;
                                                    xtReader.MoveToAttribute("url");
                                                    ListViewItem item = new ListViewItem(radioName);
                                                    item.SubItems.Add(xtReader.Value); listView.Items.Add(item);
                                                }
                                            }
                                        }
                                        clsUtilities.ResizeColumns(listView, false);
                                    }
                                    catch (XmlException ex)
                                    {
                                        f2.Close();
                                        MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        this.DialogResult = DialogResult.Abort;
                                        Load += (s, e) => Close();
                                        return;
                                    }
                                    finally { xtReader.Close(); }
                                }
                            }
                        }
                    }
                    catch (WebException ex)
                    {
                        f2.Close();
                        DialogResult dialogResult = MessageBox.Show(ex.Message + "\n\nWould you rather do a simple Google search?", Application.ProductName, MessageBoxButtons.YesNo);
                        if (dialogResult == DialogResult.Yes)
                        {
                            try { Process.Start("https://www.google.com/#q=" + Regex.Replace(seachText + " stream url", @"[^a-zA-Z0-9äöüÄÖÜßé'-]+", " ").Trim().Replace(" ", "+")); } // [^...] Matches any single character that is not in the class.  
                            catch { }
                        }
                        this.DialogResult = DialogResult.Abort;
                        Load += (s, e) => Close();
                        return;
                    }
                }
                if (!success)
                {
                    f2.Close();
                    MessageBox.Show("No results found!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.Abort;
                    Load += (s, e) => Close();
                    return;
                }
            }
        }

        private void ListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2 && listView.SelectedItems.Count > 0) { listView.SelectedItems[0].BeginEdit(); }
            else if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Abort; }
            else if (e.KeyCode == Keys.Enter) { SaveAndLeave(); }
        }

        private void SaveAndLeave()
        {
            if (listView.SelectedItems != null)
            {
                radioStation = listView.SelectedItems[0].SubItems[0].Text;
                radioURL = listView.SelectedItems[0].SubItems[1].Text;
            }
            DialogResult = DialogResult.OK;
        }

        private void ListView_DoubleClick(object sender, EventArgs e)
        {
            SaveAndLeave();
        }

        private void FrmBrowser_Load(object sender, EventArgs e)
        {
            this.Top = this.Top + 25;
            this.Left = this.Left + 50;
            toolStripStatusLabel.Text = listView.Items.Count.ToString() + (listView.Items.Count < 2 ? " item " : " items ") + "found. Double-click an item to copy it to the station list."; // Double-click to accept an entry.";
        } //Double-click list entry to copy to station list.

        private void FrmBrowser_Resize(object sender, EventArgs e)
        {// Spaltenbreite automatisch anpassen, wenn die Größe des Hauptfensters verändert wird.
            clsUtilities.ResizeColumns(listView, true);
        }

        private void EditNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0) { listView.SelectedItems[0].BeginEdit(); }
        }

        private void AcceptItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0) { SaveAndLeave(); }
        }

        private void CancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
        }

        private void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                editNameToolStripMenuItem.Enabled = true;
                acceptItemToolStripMenuItem.Enabled = true;
            }
            else
            {
                editNameToolStripMenuItem.Enabled = false;
                acceptItemToolStripMenuItem.Enabled = false;
            }
        }

    }
}
