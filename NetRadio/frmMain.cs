using System;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection; // Assembly
using System.Text.RegularExpressions;
using WMPLib;

namespace NetRadio
{
    public partial class frmMain : Form
    {
        WMPLib.WindowsMediaPlayer wPlayer;
        string winTitle;
        Version curVersion = Assembly.GetExecutingAssembly().GetName().Version;
        static bool exitFlag = false; // wg. this.Minimize statt this.Close
        static bool minClose = false; // true wenn cbMinClose.Checked (Tray
        static bool alwaysOnTop = false;
        static bool startMin = false;
        bool nothingToSave = true;
        static string appName = Application.ProductName; // "NetRadio";
        static string appPath = Assembly.GetExecutingAssembly().Location;  // EXE-Pfad
        static string xmlPath = Path.GetDirectoryName(appPath) + "\\" + appName + ".xml";
        string hkLetter = String.Empty; // Flag für existierenden Hotkey. AUSNAHME: Programmstart
        static int lastHotkeyPress = 0;
        int rowIndexFromMouseDown;
        int colIndexFromMouseDown;
        Rectangle dragBoxFromMouseDown;
        int rowIndexOfItemUnderMouseToDrop;
        bool startsPlayingFlag = false;
        bool oneRadioButtonIsChecked = false;
        string autostartStation;
        bool isValidStationArgument = false;
        string lastBalloonTip;
        bool googleMsgBox = false;

        public frmMain()
        {
            InitializeComponent();
            string formPosX = null;
            string formPosY = null;
            try
            {
                wPlayer = new WMPLib.WindowsMediaPlayer();
                wPlayer.PlayStateChange += new _WMPOCXEvents_PlayStateChangeEventHandler(WPlayer_PlayStateChange);
                wPlayer.CurrentItemChange += new _WMPOCXEvents_CurrentItemChangeEventHandler(WPlayer_CurrentItemChange);
                wPlayer.ModeChange += new _WMPOCXEvents_ModeChangeEventHandler(WPlayer_ModeChange);
                wPlayer.StatusChange += new _WMPOCXEvents_StatusChangeEventHandler(WPlayer_StatusChange);
                wPlayer.Buffering += new _WMPOCXEvents_BufferingEventHandler(WPlayer_Buffering);
                wPlayer.MediaError += new _WMPOCXEvents_MediaErrorEventHandler(WPlayer_MediaError); //wg. Falschmeldungen
                //wPlayer.EndOfStream += new _WMPOCXEvents_EndOfStreamEventHandler(wPlayer_EndOfStream);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            pnlDisplay.Click += new EventHandler(PanelDisplay_Click);
            lblD1.Click += new EventHandler(PanelDisplay_Click);
            lblD2.Click += new EventHandler(PanelDisplay_Click);
            lblD3.Click += new EventHandler(PanelDisplay_Click);
            lblD4.Click += new EventHandler(PanelDisplay_Click);
            lblD5.Click += new EventHandler(PanelDisplay_Click);

            pnlDisplay.MouseEnter += new EventHandler(PanelDisplay_MouseEnter);
            lblD1.MouseEnter += new EventHandler(PanelDisplay_MouseEnter);
            lblD2.MouseEnter += new EventHandler(PanelDisplay_MouseEnter);
            lblD3.MouseEnter += new EventHandler(PanelDisplay_MouseEnter);
            lblD4.MouseEnter += new EventHandler(PanelDisplay_MouseEnter);
            lblD5.MouseEnter += new EventHandler(PanelDisplay_MouseEnter);

            pnlDisplay.MouseLeave += new EventHandler(PanelDisplay_MouseLeave);
            lblD1.MouseLeave += new EventHandler(PanelDisplay_MouseLeave);
            lblD2.MouseLeave += new EventHandler(PanelDisplay_MouseLeave);
            lblD3.MouseLeave += new EventHandler(PanelDisplay_MouseLeave);
            lblD4.MouseLeave += new EventHandler(PanelDisplay_MouseLeave);
            lblD5.MouseLeave += new EventHandler(PanelDisplay_MouseLeave);

            rbtn01.CheckedChanged += new EventHandler(RadioButtons_CheckedChanged);
            rbtn02.CheckedChanged += new EventHandler(RadioButtons_CheckedChanged);
            rbtn03.CheckedChanged += new EventHandler(RadioButtons_CheckedChanged);
            rbtn04.CheckedChanged += new EventHandler(RadioButtons_CheckedChanged);
            rbtn05.CheckedChanged += new EventHandler(RadioButtons_CheckedChanged);
            rbtn06.CheckedChanged += new EventHandler(RadioButtons_CheckedChanged);
            rbtn07.CheckedChanged += new EventHandler(RadioButtons_CheckedChanged);
            rbtn08.CheckedChanged += new EventHandler(RadioButtons_CheckedChanged);
            rbtn09.CheckedChanged += new EventHandler(RadioButtons_CheckedChanged);
            rbtn10.CheckedChanged += new EventHandler(RadioButtons_CheckedChanged);
            rbtn11.CheckedChanged += new EventHandler(RadioButtons_CheckedChanged);
            rbtn12.CheckedChanged += new EventHandler(RadioButtons_CheckedChanged);

            //lblD4.Text = "Windows Media Player ActiveX " + wPlayer.versionInfo;
            //lblD5.Text = "Volume: "; // + wPlayer.settings.volume.ToString();
            toolTip.SetToolTip(volProgressBar, wPlayer.settings.volume.ToString());
            volProgressBar.Value = wPlayer.settings.volume;
            Text = winTitle = clsUtilities.GetDescription() + " " + new Regex(@"^\d+\.\d+").Match(curVersion.ToString()).Value;
            for (int j = 0; j < 24; j++)
            {// dgvStations.Rows.Add(24); ist wahrscheinlich schlechter, weil Cell.Value = null entsteht
                dgvStations.Rows.Add("", "");
            }
            if (File.Exists(xmlPath))
            {
                using (XmlTextReader xtr = new XmlTextReader(xmlPath))
                {
                    xtr.WhitespaceHandling = WhitespaceHandling.None; // Whitespace zwischen Elementen
                    try
                    {
                        int j = 0;
                        while (xtr.Read())
                        {
                            if (xtr.NodeType == XmlNodeType.Element && xtr.LocalName == "Station")
                            {
                                if (j < dgvStations.RowCount)
                                {// tritt ein wenn der User die Datei außerhalb des Programms editiert oder der Programmier die Zeilenzahl reduziert
                                    xtr.MoveToAttribute("Name");
                                    dgvStations.Rows[j].Cells[0].Value = xtr.Value;
                                    xtr.MoveToAttribute("URL");
                                    dgvStations.Rows[j].Cells[1].Value = xtr.Value;
                                }
                                j++;
                            }
                            else if (xtr.NodeType == XmlNodeType.Element && xtr.LocalName == "Hotkey")
                            {
                                xtr.MoveToAttribute("Enabled");
                                if (int.TryParse(xtr.Value, out int intEnabeld))
                                {
                                    cbHotkey.Checked = lblHotkey.Enabled = cmbxHotkey.Enabled = Convert.ToBoolean(Convert.ToInt16(intEnabeld));
                                }
                                else
                                {
                                    cbHotkey.Checked = lblHotkey.Enabled = cmbxHotkey.Enabled = false;
                                }
                                xtr.MoveToAttribute("Letter");
                                if (string.IsNullOrEmpty(xtr.Value))
                                {
                                    lblHotkey.Enabled = cmbxHotkey.Enabled = cbHotkey.Checked = false;
                                }
                                else
                                {
                                    if (cbHotkey.Checked && new Regex("^[A-Z0-9]$").IsMatch(xtr.Value))
                                    {// You won't be able to register a hotkey before the window is created
                                        cmbxHotkey.Text = hkLetter = xtr.Value; // => frmMain_Shown
                                    }
                                }
                            }
                            else if (xtr.NodeType == XmlNodeType.Element && xtr.LocalName == "MinClose")
                            {
                                xtr.MoveToAttribute("Enabled");
                                if (int.TryParse(xtr.Value, out int intEnabeld))
                                {
                                    cbMinClose.Checked = minClose = Convert.ToBoolean(Convert.ToInt16(intEnabeld));
                                }
                                else
                                {
                                    cbMinClose.Checked = false;
                                }
                            }
                            else if (xtr.NodeType == XmlNodeType.Element && xtr.LocalName == "AlwaysOnTop")
                            {
                                xtr.MoveToAttribute("Enabled");
                                if (int.TryParse(xtr.Value, out int intEnabeld))
                                {
                                    cbAlwaysOnTop.Checked = alwaysOnTop = Convert.ToBoolean(Convert.ToInt16(intEnabeld));
                                }
                                else
                                {
                                    cbAlwaysOnTop.Checked = false;
                                }
                            }
                            else if (xtr.NodeType == XmlNodeType.Element && xtr.LocalName == "Volume")
                            {
                                xtr.MoveToAttribute("Value");
                                if (wPlayer != null)
                                {
                                    wPlayer.settings.volume = int.TryParse(xtr.Value, out int intVolume)
                                        ? intVolume
                                        : wPlayer.settings.volume;
                                    //lblD5.Text = "Volume: "; // + wPlayer.settings.volume.ToString();
                                    toolTip.SetToolTip(volProgressBar, wPlayer.settings.volume.ToString());
                                    volProgressBar.Value = wPlayer.settings.volume;
                                }
                            }
                            else if (xtr.NodeType == XmlNodeType.Element && xtr.LocalName == "Location")
                            {
                                xtr.MoveToAttribute("PosX");
                                formPosX = xtr.Value;
                                xtr.MoveToAttribute("PosY");
                                formPosY = xtr.Value;
                            }
                            else if (xtr.NodeType == XmlNodeType.Element && xtr.LocalName == "Autostart")
                            {
                                xtr.MoveToAttribute("Station");
                                if (int.TryParse(xtr.Value, out int intStation))
                                {
                                    if (intStation > 0 && intStation <= 12) { cmbxStation.Text = autostartStation = xtr.Value; }
                                }
                            }
                        }
                    }
                    catch (XmlException ex)
                    {
                        MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    xtr.Close();
                }
                RewriteButtonText();
            }
            else
            {
                MessageBox.Show("\"" + xmlPath + "\"  is not found.");
            }
            foreach (DataGridViewColumn column in dgvStations.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            string[] args = Environment.GetCommandLineArgs();
            foreach (string x in args)
            {
                if (x.Contains("min"))
                {
                    startMin = true; // siehe frmMain_Shown-Event
                    Opacity = 0; // sonst wird GUI kurz angezeigt - unschön
                }
            }
            for (int i = 1; i < args.Length; i++)
            {
                isValidStationArgument = int.TryParse(args[i], out int intStation);
                if (isValidStationArgument)
                {
                    string btnName = "rbtn" + Math.Abs(intStation).ToString("D2");
                    Control[] controls = tcMain.TabPages[0].Controls.Find(btnName, true);
                    if (controls.Length == 1 && controls[0] is RadioButton)
                    {
                        RadioButton foundBtn = controls[0] as RadioButton;
                        foundBtn.Checked = true;
                        break;
                    }
                }
            }

            if (!isValidStationArgument && !string.IsNullOrEmpty(autostartStation))
            {
                string btnName = "rbtn" + autostartStation.PadLeft(2, '0');
                Control[] controls = tcMain.TabPages[0].Controls.Find(btnName, true);
                if (controls.Length == 1 && controls[0] is RadioButton)
                {
                    RadioButton foundBtn = controls[0] as RadioButton;
                    foundBtn.Checked = true;
                }
            }

            toolStripStatusLabel.Text = "Press <Ctrl + F> to find new radio stations";
            cbAutostart.Checked = clsUtilities.IsAutoStartEnabled(appName, "\"" + appPath + "\"" + " -min");
            Rectangle screen = Screen.PrimaryScreen.WorkingArea;
            if (int.TryParse(formPosX, out int xPos) && int.TryParse(formPosY, out int yPos))
            {// Form komplett innerhalb der WorkingArea angezeigt werden
                xPos = xPos < 0 ? 0
                    : xPos + Width > screen.Width
                    ? screen.Width - Width
                    : xPos;
                yPos = yPos < 0
                    ? yPos = 0
                    : yPos + Height > screen.Height
                    ? yPos = screen.Height - Height
                    : yPos;
            }
            else
            {// CenterOnSreen wenn keine gespeicherten Werte vorhanden sind
                xPos = (screen.Height / 2) - (Height / 2);
                yPos = (screen.Width / 2) - (Width / 2);
            }
            Location = new Point(xPos, yPos);

            //Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //SettingsSection section = (SettingsSection)config.GetSection("system.net/settings");
            //section.HttpWebRequest.UseUnsafeHeaderParsing = true;
            //config.Save(); // add the reference to the assembly System.Configuration.dll
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == clsUtilities.WM_SHOWME)
            {// MessageBox.Show("ShowMe");
                ShowMe(); // another instance is started
            }
            if (m.Msg == clsUtilities.WM_HOTKEY)
            {
                int keyPressTick = Environment.TickCount;
                int elapsed = keyPressTick - lastHotkeyPress;
                lastHotkeyPress = keyPressTick;
                if (elapsed <= 400)
                {
                    exitFlag = true;
                    Close();
                }
                else if (Visible)
                {
                    if (minClose)
                    {
                        Hide(); //ShowInTaskbar = false; verträgt sich nicht mit GlobalHotkey => zerstört Handle
                        notifyIcon.Visible = true;
                        tcMain.SelectedIndex = 0;
                    }
                    else
                    {
                        if (WindowState == FormWindowState.Minimized)
                        {
                            WindowState = FormWindowState.Normal;
                        }
                        else
                        {
                            if (clsUtilities.GetForegroundWindow() == Handle)
                            {
                                WindowState = FormWindowState.Minimized;
                                tcMain.SelectedIndex = 0;
                            }
                            else
                            {
                                BringToFront();
                                Activate();
                            }
                        }
                    }
                }
                else
                {
                    if (minClose)
                    {
                        notifyIcon.Visible = false;
                        Show(); BringToFront(); Activate();
                    }
                }
            }
            else if (m.Msg == clsUtilities.WM_QUERYENDSESSION)
            {
                exitFlag = true;
                Close();

            }
            base.WndProc(ref m);
        }

        private void ShowMe()
        {
            if (!Visible)
            {
                notifyIcon.Visible = false;
                Show();
            }
            else if (WindowState == FormWindowState.Minimized) { WindowState = FormWindowState.Normal; } // wahrscheinlich unnötig, kann nicht minimiert werden
            bool top = TopMost; // get our current "TopMost" value (ours will always be false though)
            TopMost = true; // make our form jump to the top of everything
            TopMost = top; // set it back to whatever it was
            BringToFront(); Activate();
        }

        private void RadioButtons_CheckedChanged(object sender, EventArgs e)
        {// the event is fired twice because whenever one RadioButton within a group is checked another will be unchecked
            RadioButton rbutton = sender as RadioButton;
            if (!oneRadioButtonIsChecked || rbutton != null && !rbutton.Checked) // || nooneChecked) // also das 2. Ereignis
            {// Only one radio button will be checked
                playPauseToolStripMenuItem.BackColor = btnPlayStop.BackColor = System.Drawing.SystemColors.ControlDark;
                btnPlayStop.Text = playPauseToolStripMenuItem.Text = "Pause";
                playPauseToolStripMenuItem.Enabled = true;

                oneRadioButtonIsChecked = true;
                BtnReset_Click(null, null); // nicht unbedingt nötig, falls es sich bewährt
                btnReset.Enabled = true; // beim Programmstart deaktiviert
            }
        }

        private void WPlayer_CurrentItemChange(object pMediaObject)
        {
            if (wPlayer.URL.Length > 0)
            {
                if (wPlayer.URL.Contains("ogg"))
                {
                    wPlayer.controls.stop();
                    lblD3.Text = "Sorry, this can not be played, if I'm not mistaken.";
                }
                lblD1.Visible = false; //lblD1 = Fettschrift
                lblD0.Visible = true; // keine Fettschrift
                lblD0.Text = Path.GetFileName(wPlayer.URL);
            }
        }

        private void WPlayer_ModeChange(string ModeName, bool NewValue)
        {
            lblD4.Text = ModeName;
        }

        private void WPlayer_MediaError(object pMediaObject)
        {// Kurzzeitige Internet-Unterbrechungen werden gepuffert, Fehlermeldung kann falsch sein bzw. nerven.
            IWMPMedia2 errSource = pMediaObject as WMPLib.IWMPMedia2;
            if (!(errSource is null)) // is-Operator für den Musterabgleich
            {
                WMPLib.IWMPErrorItem errorItem = errSource.Error; // errorItem.errorDescription
                MessageBox.Show("Can't play " + errSource.sourceURL, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void WPlayer_StatusChange()
        {
            try
            {
                string currPlayerStatus = wPlayer.status;
                if (!exitFlag && !currPlayerStatus.Equals(lastBalloonTip))
                {
                    toolStripStatusLabel.Text = wPlayer.status;
                    notifyIcon.ShowBalloonTip(2, Text, wPlayer.status, ToolTipIcon.Info);
                    lastBalloonTip = currPlayerStatus;
                }
            }
            catch { } // Text kann Null sein
        }

        private void WPlayer_PlayStateChange(int newState)
        {// Test the current state of the player and display a message for each state.
            switch ((WMPPlayState)newState)
            {
                case WMPPlayState.wmppsUndefined: lblD3.Text = "Windows Media Player is in an undefined state."; break;
                case WMPPlayState.wmppsStopped: lblD3.Text = "Playback is stopped."; break;
                case WMPPlayState.wmppsPaused: lblD3.Text = "Playback is paused."; break;
                case WMPPlayState.wmppsPlaying: lblD3.Text = "Stream is playing."; WmpStartsPlaying(); break;
                case WMPPlayState.wmppsScanForward: lblD3.Text = "Stream is scanning forward."; break;
                case WMPPlayState.wmppsScanReverse: lblD3.Text = "Stream is scanning backward."; break;
                case WMPPlayState.wmppsBuffering: lblD3.Text = "Stream is being buffered."; break;
                case WMPPlayState.wmppsWaiting: lblD3.Text = "Waiting for streaming data."; break;
                case WMPPlayState.wmppsMediaEnded: lblD3.Text = "The end of the media item has been reached."; break; //frmPlayStateJobs(false); break;
                case WMPPlayState.wmppsTransitioning: lblD3.Text = "Preparing new media item."; break;
                case WMPPlayState.wmppsReady: lblD3.Text = "Ready to begin playing."; break;
                case WMPPlayState.wmppsReconnecting: lblD3.Text = "Trying to reconnect for streaming data."; break;
                case WMPPlayState.wmppsLast: lblD3.Text = "Last enumerated value. Not a valid state."; break;
                default: lblD3.Text = "Unknown state."; break;
            }
        }

        private void WPlayer_Buffering(bool start)
        {
            if (start) // Determine whether buffering has started or stopped.
            {
                startsPlayingFlag = true;
            }
        }

        private void BtnIncrease_MouseDown(object sender, MouseEventArgs e) { timer.Enabled = true; timer.Start(); }
        private void BtnIncrease_MouseUp(object sender, MouseEventArgs e) { timer.Stop(); }
        private void BtnDecrease_MouseDown(object sender, MouseEventArgs e) { timer.Enabled = true; timer.Start(); }
        private void BtnDecrease_MouseUp(object sender, MouseEventArgs e) { timer.Stop(); }

        private void VolProgressBar_Click(object sender, EventArgs e)
        {
            float absMouse = (PointToClient(MousePosition).X - volProgressBar.Bounds.X);
            float clcFactor = volProgressBar.Width / (float)100;
            float relMouse = absMouse / clcFactor;
            Int32 intMouse = Convert.ToInt32(relMouse);
            volProgressBar.Value = wPlayer.settings.volume = intMouse > 100 ? 100 : intMouse < 0 ? 0 : intMouse;
            toolTip.SetToolTip(volProgressBar, wPlayer.settings.volume.ToString());
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (btnIncrease.Focused) { BtnIncrease_Click(btnIncrease, null); }
            else if (btnDecrease.Focused) { BtnDecrease_Click(btnDecrease, null); }
        }

        private void TcMain_SelectedIndexChanged(object sender, EventArgs e)
        {// 0 = Player, 1 = Stations, 2 = Settings, 3 = Help, 4 = Information
            if (tcMain.SelectedIndex == 0 && !clsUtilities.isDGVEmpty(dgvStations))
            {
                if (!nothingToSave)
                {
                    if ((int)wPlayer.playState == 3) // 3 = Playing
                    {
                        bool urlIsStillInFavorites = false;
                        for (int i = 0; i < 12; i++)
                        {
                            if (dgvStations.Rows[i].Cells[1].Value != null && dgvStations.Rows[i].Cells[1].Value.ToString() == wPlayer.URL)
                            {
                                RadioButton foundBtn = tcMain.TabPages[0].Controls.Find("rbtn" + (i + 1).ToString("D2"), true)[0] as RadioButton;
                                {// radioButtons_CheckedChanged ist die Ursache dafür, dass trotzdem der Player unterbrochen wird
                                    urlIsStillInFavorites = foundBtn.Checked = true;
                                    break;
                                }
                            }
                        }
                        if (!urlIsStillInFavorites)
                        {
                            wPlayer.controls.stop();
                            foreach (Control c in tcMain.TabPages[0].Controls)
                            {
                                if (c.GetType() == typeof(RadioButton))
                                {
                                    RadioButton rb = c as RadioButton;
                                    rb.Checked = false;
                                }
                                oneRadioButtonIsChecked = false; // darf nicht vor den Änderungen (rb.Checked) stehen
                            }
                        }
                    }
                    RewriteButtonText();
                }
            }
            if (tcMain.SelectedIndex == 1)
            {// Stations
                UpdateStatusLabelStationsList();
                dgvStations.Focus(); // sonst funktioniert F2 nicht sogleich

            }
            else if (tcMain.SelectedIndex == 4)
            {// Information
                toolStripStatusLabel.Text = "Version: " + curVersion.ToString() + " (wmp.dll: " + wPlayer.versionInfo + ")";
            }
            else
            {
                if (wPlayer.playState == WMPLib.WMPPlayState.wmppsPlaying)
                {
                    toolStripStatusLabel.Text = wPlayer.status;
                }
                else
                {
                    toolStripStatusLabel.Text = "Press <Ctrl + F> to find new radio stations";
                }
            }
        }

        private void BtnPlayStop_Click(object sender, EventArgs e)
        {
            if ((int)wPlayer.playState == 3)
            {
                wPlayer.controls.pause();
                btnPlayStop.Text = playPauseToolStripMenuItem.Text = "Play";
                playPauseToolStripMenuItem.BackColor = btnPlayStop.BackColor = System.Drawing.Color.Maroon;
            }
            else
            {
                wPlayer.controls.play();
                btnPlayStop.Text = playPauseToolStripMenuItem.Text = "Pause";
                playPauseToolStripMenuItem.BackColor = btnPlayStop.BackColor = System.Drawing.SystemColors.ControlDark;
            }
        }

        private void BtnIncrease_Click(object sender, EventArgs e)
        {
            if (wPlayer.settings.volume < 100)
            {
                //wPlayer.settings.volume = wPlayer.settings.volume + 1 > 100
                //    ? 100
                //    : wPlayer.settings.volume + 1;
                wPlayer.settings.volume = wPlayer.settings.volume + 1;
                //btnDecrease.Enabled = true;
            }
            //else { btnIncrease.Enabled = false; } // cave: dadurch erhält der nächste Button den Fokus!!!
            //lblD5.Text = "Volume: "; // + wPlayer.settings.volume.ToString();
            toolTip.SetToolTip(volProgressBar, wPlayer.settings.volume.ToString());
            volProgressBar.Value = wPlayer.settings.volume;
            nothingToSave = false;
        }

        private void BtnDecrease_Click(object sender, EventArgs e)
        {
            if (wPlayer.settings.volume > 0)
            {
                wPlayer.settings.volume = wPlayer.settings.volume - 1;
            }
            //else
            //{
            ////    wPlayer.settings.volume = 0;
            //btnDecrease.Enabled = false;
            //}
            //lblD5.Text = "Volume: "; // + wPlayer.settings.volume.ToString();
            toolTip.SetToolTip(volProgressBar, wPlayer.settings.volume.ToString());
            volProgressBar.Value = wPlayer.settings.volume;
            nothingToSave = false;
            //btnIncrease.Enabled = true;
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            playPauseToolStripMenuItem.BackColor = btnPlayStop.BackColor = System.Drawing.SystemColors.ControlDark;
            btnPlayStop.Text = playPauseToolStripMenuItem.Text = "Pause";
            playPauseToolStripMenuItem.Enabled = true;
            lblD0.Text = "-";
            lblD1.Text = "-";
            lblD2.Text = "-";
            lblD3.Text = "-";
            lblD4.Text = "-";
            //lblD5.Text = "-";
            int intVolume = wPlayer.settings.volume;
            wPlayer.close(); // close the Windows Media Player object
            wPlayer = null;
            GC.Collect(); // start .NET CLR Garbage Collection
            GC.WaitForPendingFinalizers(); // Wait for Garbage Collection to finish wPlayer.close();
            try
            {
                wPlayer = new WMPLib.WindowsMediaPlayer(); // wg. wPlayer = null; s.o.
                wPlayer.PlayStateChange += new _WMPOCXEvents_PlayStateChangeEventHandler(WPlayer_PlayStateChange);
                wPlayer.CurrentItemChange += new _WMPOCXEvents_CurrentItemChangeEventHandler(WPlayer_CurrentItemChange);
                wPlayer.ModeChange += new _WMPOCXEvents_ModeChangeEventHandler(WPlayer_ModeChange);
                wPlayer.StatusChange += new _WMPOCXEvents_StatusChangeEventHandler(WPlayer_StatusChange);
                wPlayer.Buffering += new _WMPOCXEvents_BufferingEventHandler(WPlayer_Buffering);
                wPlayer.MediaError += new _WMPOCXEvents_MediaErrorEventHandler(WPlayer_MediaError);
                //wPlayer.EndOfStream += new _WMPOCXEvents_EndOfStreamEventHandler(wPlayer_EndOfStream);
                //lblD6.Text = "Windows Media Player ActiveX " + wPlayer.versionInfo;
                wPlayer.settings.volume = intVolume;
                //lblD5.Text = "Volume: "; // + wPlayer.settings.volume.ToString();
                toolTip.SetToolTip(volProgressBar, wPlayer.settings.volume.ToString());
                volProgressBar.Value = wPlayer.settings.volume;
                foreach (Control c in tcMain.TabPages[0].Controls)
                {
                    if (c.GetType() == typeof(RadioButton))
                    {
                        RadioButton rb = c as RadioButton;
                        if (rb.Checked)
                        {//wPlayer.URL = string.Empty;
                            bool isValid = int.TryParse(rb.Tag.ToString(), out int iTag);
                            if (isValid && dgvStations.Rows.Count >= iTag && dgvStations.Rows[iTag - 1].Cells[1].Value.ToString().Length > 0)
                            {
                                wPlayer.URL = dgvStations.Rows[iTag - 1].Cells[1].Value.ToString();
                                wPlayer.controls.play();
                            }
                            else
                            {
                                lblD1.Text = "ERROR";
                                lblD3.Text = "No URL is defined.";
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RewriteButtonText()
        {
            for (int i = 1; i <= 12; i++)
            {
                RadioButton foundBtn = tcMain.TabPages[0].Controls.Find("rbtn" + i.ToString("D2"), true)[0] as RadioButton;
                if (dgvStations.Rows[i - 1].Cells[1].Value.ToString().Length > 0) // column "URL"
                {
                    if (dgvStations.Rows[i - 1].Cells[0].Value.ToString().Length > 0)
                    {
                        foundBtn.Text = dgvStations.Rows[i - 1].Cells[0].Value.ToString();
                    }
                    else
                    {
                        foundBtn.Text = "N.N.";
                    }
                    foundBtn.Enabled = true;
                }
                else
                {
                    foundBtn.Text = "-";
                    foundBtn.Enabled = false;
                }
            }
        }

        private void WmpStartsPlaying()
        {
            if (startsPlayingFlag) // wird sonst 2x kurz hintereinader aufgerufen
            { // Dieser Workaround sort dafür, dass diese Methode nur 1x aufgerufen wird.
                startsPlayingFlag = false; // wird im Bufferung-Event auf True gesetzt
                //Console.Beep();
                btnPlayStop.Enabled = btnIncrease.Enabled = btnDecrease.Enabled = true;
                Text = winTitle;
                //btnPlayStop.Text = playPauseToolStripMenuItem.Text = "Pause";
                //playPauseToolStripMenuItem.Enabled = true;
                //playPauseToolStripMenuItem.BackColor = btnPlayStop.BackColor = System.Drawing.SystemColors.ControlDark;

                try
                {
                    if (wPlayer.currentMedia.name.Contains(", "))
                    {
                        lblD1.Text = wPlayer.currentMedia.name.Substring(0, wPlayer.currentMedia.name.IndexOf(","));
                    }
                    else if (wPlayer.currentMedia.name.Contains("_"))
                    {
                        if (clsUtilities.IsAllLower(wPlayer.currentMedia.name))
                        {
                            lblD1.Text = clsUtilities.RemoveFromEnd(wPlayer.currentMedia.name.Replace("_", " "), "mp3").ToUpper();
                        }
                        else { lblD1.Text = clsUtilities.RemoveFromEnd(wPlayer.currentMedia.name.Replace("_", " "), "mp3"); }
                    }
                    else
                    {
                        lblD1.Text = wPlayer.currentMedia.name;
                    }
                    foreach (Control c in tcMain.TabPages[0].Controls)
                    {
                        if (c.GetType() == typeof(RadioButton))
                        {
                            RadioButton rb = c as RadioButton;
                            if (rb.Checked)
                            {
                                Text = rb.Text + " - " + winTitle;
                            }
                        }
                    }
                    //MessageBox.Show(wPlayer.currentMedia.getItemInfo("FileType")); // funkt nur wenn StreamURL mit z.B. .mp3 endet
                    Dictionary<string, string> metaData = ClsMetadata.GetMetdata(wPlayer.currentMedia.sourceURL);
                    Application.DoEvents(); // hilft tatsächlich gegen weißen Balken bei lblD0

                    if (metaData.ContainsKey("error") && metaData["error"].Length > 0) // keine Reaktion wenn Timeout
                    {
                        MessageBox.Show(metaData["error"], Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    string description = metaData.ContainsKey("description") ? metaData["description"] : ""; //.Length > 0 ? metaData["description"] : wPlayer.currentMedia.getItemInfo("Genre");
                    lblD2.Text = description.Length > 0 ? description.Replace(wPlayer.currentMedia.name, "") : wPlayer.currentMedia.getItemInfo("Genre");

                    string bitrate = metaData.ContainsKey("bitrate") ? metaData["bitrate"] : "";
                    bitrate = bitrate.Length > 0 ? metaData["bitrate"] : (wPlayer.network.bitRate / 1000).ToString();

                    string samplerate = metaData.ContainsKey("samplerate") ? metaData["samplerate"] : "";
                    samplerate = samplerate.Length > 0 ? ", " + (Convert.ToDouble(metaData["samplerate"]) / 1000) + " kHz" : ""; // : "Bandwith: " + (wPlayer.network.bandWidth / 1000) + " kB/s, ";

                    string codec = metaData.ContainsKey("codec") ? metaData["codec"].ToLower() : "";
                    codec = codec.Length == 0 ? "" : codec.Equals("mpeg") ? "MPEG (MP3)" : codec.Equals("aacp") ? "AAC+" : codec.Equals("ogg") ? "Ogg" : codec;

                    lblD4.Text = bitrate + " kB" + samplerate + (codec.Length > 0 ? ", " + codec : "");
                    lblD0.Text = string.Empty;
                    lblD0.Visible = false;
                    lblD1.Visible = true;
                }
                catch { }
            }
        }

        private void DgvStations_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (winTitle != null && nothingToSave)
            {// bei Programmstart tritt das Ereignis 3x ein, ohne dass winTitle bereits besteht
                nothingToSave = false;
            }
            UpdateStatusLabelStationsList();
        }

        private void DgvStations_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var dGrid = sender as DataGridView;
            string rowText = (e.RowIndex + 1).ToString() + ". ";
            var centerFormat = new StringFormat(StringFormatFlags.MeasureTrailingSpaces) // default: exclude the space at the end of each line
            {
                Alignment = StringAlignment.Far, // Bei einem Layout mit Ausrichtung von links nach rechts ist die weit entfernte Position rechts.
                LineAlignment = StringAlignment.Center // vertikale Ausrichtung der Zeichenfolge
            };
            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, dGrid.RowHeadersWidth, e.RowBounds.Height);
            int currRow = dgvStations.CurrentRow == null ? 0 : dgvStations.CurrentRow.Index;
            //if (dgvStations.Rows[e.RowIndex].Index != currRow) // Zahl weglassen wenn Dreieck gezeigt wird
            //{
            Color rhForeColor = dgvStations.Rows[e.RowIndex].Index >= 12 ? SystemColors.ControlLightLight : dGrid.RowHeadersDefaultCellStyle.ForeColor;
            using (SolidBrush sBrush = new SolidBrush(rhForeColor))
            {// the using statement automatically disposes the brush
                e.Graphics.DrawString(rowText, e.InheritedRowStyle.Font, sBrush, headerBounds, centerFormat);
            }
            //}
        }

        private void LinkPayPal_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try { Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=DK9WYLVBN7K4Y"); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
        }

        private void LinkHomepage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try { Process.Start("https://netradio.codeplex.com/"); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
        }

        private void PnlDisplay_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            LinearGradientBrush myBrush = new LinearGradientBrush(new Point(0, 0), new Point(Width, Height), Color.AliceBlue, Color.LightSteelBlue);
            g.FillRectangle(myBrush, ClientRectangle);
        }

        private void CbAutostart_CheckedChanged(object sender, EventArgs e)
        {
            if (cbAutostart.Checked)
            {
                if (!clsUtilities.IsAutoStartEnabled(appName, "\"" + appPath + "\"" + " -min"))
                {
                    clsUtilities.SetAutoStart(appName, "\"" + appPath + "\"" + " -min");
                    toolStripStatusLabel.Text = "Autorun written to Registy";
                }
            }
            else
            {
                if (clsUtilities.IsAutoStartEnabled(appName, "\"" + appPath + "\"" + " -min"))
                {
                    clsUtilities.UnSetAutoStart(appName);
                    toolStripStatusLabel.Text = "Autorun deleted from Registry";
                }
            }
            nothingToSave = false;
        }

        private void CbHotkey_CheckedChanged(object sender, EventArgs e)
        {
            if (cbHotkey.Focused)
            {
                if (cbHotkey.Checked)
                {// Liste automatisch öffnen (besser nicht)
                    //clsUtilities.SendMessage(cmbxHotkey.Handle, clsUtilities.CB_SHOWDROPDOWN, 1, IntPtr.Zero);
                    lblHotkey.Enabled = true;
                    cmbxHotkey.Enabled = true;
                    cmbxHotkey.Focus(); //cmbxHotkey.SelectedItem = "A";
                    if (cmbxHotkey.SelectedText.Length == 1 && hkLetter == string.Empty)
                    {// Regex("^[A-Z].$").IsMatch ist hier nicht erforderlich, da bereits
                        RegisterHK(cmbxHotkey.SelectedText);
                    }
                }
                else // unChecked
                {
                    if (hkLetter != string.Empty && cmbxHotkey.Enabled && clsUtilities.UnregisterHotKey(Handle, clsUtilities.HOTKEY_ID))
                    {
                        toolStripStatusLabel.Text = "Hotkey unregistered";
                        hkLetter = string.Empty;
                    }
                    lblHotkey.Enabled = false;
                    cmbxHotkey.Enabled = false;
                }
                nothingToSave = false;
            }
        }

        //private void cmbxHotkey_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    e.Handled = !Char.IsLetter(e.KeyChar) && !(e.KeyChar == (char)Keys.Back) && cmbxHotkey.Text.Length > 0;
        //    if (Char.IsLetter(e.KeyChar)) e.KeyChar = Char.ToUpper(e.KeyChar); // nur Großbuchstaben
        //}

        private void CmbxHotkey_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbxHotkey.Visible && cmbxHotkey.Focused && cmbxHotkey.Enabled)
            {
                if (hkLetter != string.Empty && clsUtilities.UnregisterHotKey(Handle, clsUtilities.HOTKEY_ID))
                {// 1. Schritt: vorhanden Hotkey löschen
                    toolStripStatusLabel.Text = "Hotkey unregistered";
                    hkLetter = String.Empty;
                }
                if (hkLetter == string.Empty && cbHotkey.Checked && new Regex("^[A-Z]+$").IsMatch(cmbxHotkey.Text))
                {// 2. Schritt: neuen Hotkey registrieren
                    RegisterHK(cmbxHotkey.Text);
                }
                nothingToSave = false;
            }
        }

        private void CmbxHotkey_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cmbxHotkey.Text))
            {
                lblHotkey.Enabled = false;
                cmbxHotkey.Enabled = false;
            }
        }

        private void RegisterHK(string hkString)
        {
            if (clsUtilities.RegisterHotKey(Handle, clsUtilities.HOTKEY_ID, (uint)(clsUtilities.Modifiers.Control | clsUtilities.Modifiers.Win), (uint)(Keys)Convert.ToChar(hkString)) == true)
            {
                toolStripStatusLabel.Text = "Hotkey registered (Ctrl+Win+" + hkString + ")";
                hkLetter = hkString;
            }
            else
            {
                hkLetter = String.Empty;
                cbHotkey.Checked = false;
                cmbxHotkey.SelectedIndex = 0;
                tcMain.SelectedIndex = 2; // Hotkey-Dialog anzeigen
                toolStripStatusLabel.Text = "Sorry, another application is using this hotkey!";
                cmbxHotkey.Enabled = false;
                lblHotkey.Enabled = false;
            }
        }

        private void CbMinClose_CheckedChanged(object sender, EventArgs e)
        {
            if (cbMinClose.Focused)
            {
                if (cbMinClose.Checked) { minClose = true; }
                else { minClose = false; }
            }
        }

        private void CbAlwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {
            if (cbAlwaysOnTop.Focused)
            {
                if (cbAlwaysOnTop.Checked) { alwaysOnTop = true; }
                else { alwaysOnTop = false; }
                this.TopMost = alwaysOnTop;
            }
        }

        private void FrmMain_Shown(object sender, EventArgs e)
        {
            //if (wPlayer.settings.volume > 99) { btnIncrease.Enabled = false; }
            //else if (wPlayer.settings.volume < 1) { btnDecrease.Enabled = false; }

            if (hkLetter != string.Empty && new Regex("^[A-Z]+$").IsMatch(hkLetter))
            {// Hotkey kann erst registriert werden, wenn das Fenster erstellt wurde
                RegisterHK(hkLetter);
            }
            if (startMin)
            {
                startMin = false;
                if (minClose)
                {
                    notifyIcon.Visible = true;
                    Hide();
                }
                else
                {
                    WindowState = FormWindowState.Minimized;
                }
                Opacity = 1; // nach Hide
                //notifyIcon.ShowBalloonTip(1, Text, "Autostart", ToolTipIcon.Info);
            }
            if (alwaysOnTop) { TopMost = true; }
        }

        private void FrmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F4 && e.Modifiers == Keys.Alt)
            {
                exitFlag = true; Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (dgvStations.CurrentCell != null && dgvStations.IsCurrentCellInEditMode)
                {
                    dgvStations.EndEdit();
                    dgvStations.CurrentCell.Selected = true;
                }
                else { Close(); } // Formular schließen
            }
            else if ((e.KeyCode == Keys.I && e.Modifiers == Keys.Control)) // && tcMain.SelectedIndex == 1)
            {
                e.Handled = true;
                if (wPlayer.playState == WMPLib.WMPPlayState.wmppsPlaying && !backgroundWorker.IsBusy)
                {
                    backgroundWorker.RunWorkerAsync();
                }
            }
            else if (e.KeyCode == Keys.F1)
            {
                tcMain.SelectedIndex = 3;
            }
            else if (e.KeyCode == Keys.F2 && tcMain.SelectedIndex == 0)
            {
                int rbi = 0;
                foreach (Control c in tcMain.TabPages[0].Controls)
                {
                    if (c.GetType() == typeof(RadioButton))
                    {
                        RadioButton rb = c as RadioButton;
                        if (rb.Checked)
                        {
                            char rbIndex = rb.Name[rb.Name.Length - 1];
                            rbi = (int)(rbIndex - '1'); // int rbi = (int)char.GetNumericValue(rbIndex) - 1;
                            break;
                        }
                    }
                }
                tcMain.SelectedIndex = 1;
                dgvStations.Rows[rbi].Selected = true;
                dgvStations.CurrentCell = dgvStations.Rows[rbi].Cells[0]; // wg. F2, öffnet sonst 1. Zeile
            }
            else if (e.KeyCode == Keys.F5 && tcMain.SelectedIndex == 0)
            {
                if (wPlayer.playState == WMPLib.WMPPlayState.wmppsPlaying && !backgroundWorker.IsBusy)
                {
                    pnlDisplay.Cursor = Cursors.Default;
                    googleMsgBox = false;
                    backgroundWorker.RunWorkerAsync();
                }
            }
            else if (e.KeyCode == Keys.OemQuestion) // && e.Shift) // && e.Modifiers == Keys.Shift && tcMain.SelectedIndex == 0)
            {
                WhyThatToolStripMenuItem_Click(null, null);
            }
            else if (e.KeyCode == Keys.F && e.Modifiers == Keys.Control)
            {
                e.Handled = true;
                if (tcMain.SelectedIndex != 1)
                {
                    tcMain.SelectedIndex = 1;
                    for (int row = 0; row < dgvStations.RowCount; row++)
                    {
                        if (clsUtilities.isDGVRowEmpty(dgvStations.Rows[row]))
                        {
                            dgvStations.Rows[row].Selected = true;
                            dgvStations.CurrentCell = dgvStations.Rows[row].Cells[0]; // wg. F2, öffnet sonst 1. Zeile
                            dgvStations.FirstDisplayedScrollingRowIndex = dgvStations.SelectedRows[0].Index;
                            break;
                        }
                    }
                }
                BtnSearch_Click(null, null);
            }
            else if (tcMain.SelectedIndex == 0)
            {
                if (e.KeyCode == Keys.Space)
                {
                    if (!btnPlayStop.Focused) { btnPlayStop.PerformClick(); btnPlayStop.Focus(); }
                }
                else if (e.KeyCode == Keys.Oemplus) { btnIncrease.PerformClick(); btnIncrease.Focus(); }
                else if (e.KeyCode == Keys.OemMinus) { btnDecrease.PerformClick(); btnDecrease.Focus(); }
                else if (e.KeyCode == Keys.Add) { btnIncrease.PerformClick(); btnIncrease.Focus(); }
                else if (e.KeyCode == Keys.Subtract) { btnDecrease.PerformClick(); btnDecrease.Focus(); }
                else if (e.KeyCode == Keys.Back) { btnReset.PerformClick(); btnReset.Focus(); }
                else if (e.KeyCode == Keys.D1) { rbtn01.Checked = true; rbtn01.Focus(); }
                else if (e.KeyCode == Keys.D2) { rbtn02.Checked = true; rbtn02.Focus(); }
                else if (e.KeyCode == Keys.D3) { rbtn03.Checked = true; rbtn03.Focus(); }
                else if (e.KeyCode == Keys.D4) { rbtn04.Checked = true; rbtn04.Focus(); }
                else if (e.KeyCode == Keys.D5) { rbtn05.Checked = true; rbtn05.Focus(); }
                else if (e.KeyCode == Keys.D6) { rbtn06.Checked = true; rbtn06.Focus(); }
                else if (e.KeyCode == Keys.D7) { rbtn07.Checked = true; rbtn07.Focus(); }
                else if (e.KeyCode == Keys.D8) { rbtn08.Checked = true; rbtn08.Focus(); }
                else if (e.KeyCode == Keys.D9) { rbtn09.Checked = true; rbtn09.Focus(); }
                else if (e.KeyCode == Keys.NumPad1) { rbtn01.Checked = true; rbtn01.Focus(); }
                else if (e.KeyCode == Keys.NumPad2) { rbtn02.Checked = true; rbtn02.Focus(); }
                else if (e.KeyCode == Keys.NumPad3) { rbtn03.Checked = true; rbtn03.Focus(); }
                else if (e.KeyCode == Keys.NumPad4) { rbtn04.Checked = true; rbtn04.Focus(); }
                else if (e.KeyCode == Keys.NumPad5) { rbtn05.Checked = true; rbtn05.Focus(); }
                else if (e.KeyCode == Keys.NumPad6) { rbtn06.Checked = true; rbtn06.Focus(); }
                else if (e.KeyCode == Keys.NumPad7) { rbtn07.Checked = true; rbtn07.Focus(); }
                else if (e.KeyCode == Keys.NumPad8) { rbtn08.Checked = true; rbtn08.Focus(); }
                else if (e.KeyCode == Keys.NumPad9) { rbtn09.Checked = true; rbtn09.Focus(); }
            }
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {// Form.Closed and Form.Closing events are not raised when the Application.Exit method is called; use Form.Close
            if ((Control.ModifierKeys & Keys.Shift) != 0) { exitFlag = true; }
            if (exitFlag || !minClose || e.CloseReason != CloseReason.UserClosing) //TrayIcon Exit
            {// Form.Closed and Form.Closing events are not raised when the Application.Exit method is called; use From.Close
                notifyIcon.Visible = false; // keine komische Meldungen an Windows-Nachrichtenzentrale
                if (hkLetter != string.Empty)
                {
                    clsUtilities.UnregisterHotKey(Handle, clsUtilities.HOTKEY_ID);
                }
                string strVolume = wPlayer.settings.volume.ToString();
                wPlayer.close(); // Close the Windows Media Player control
                wPlayer = null;
                GC.Collect(); // Start .NET CLR Garbage Collection
                GC.WaitForPendingFinalizers(); // Wait for Garbage Collection to finish  wPlayer.close();
                if (!nothingToSave)
                {
                    XmlWriterSettings xwSettings = new XmlWriterSettings()
                    {
                        IndentChars = "\t",
                        NewLineHandling = NewLineHandling.Entitize,
                        Indent = true,
                        NewLineChars = "\n",
                    };
                    try
                    {
                        using (XmlWriter xw = XmlWriter.Create(xmlPath, xwSettings))
                        {
                            xw.WriteStartDocument();
                            xw.WriteStartElement("NetRadio");

                            for (int i = 0; i < dgvStations.RowCount; ++i)
                            {
                                xw.WriteStartElement("Station");
                                var v = dgvStations.Rows[i].Cells[0].Value;
                                xw.WriteAttributeString("Name", v != null ? dgvStations.Rows[i].Cells[0].Value.ToString() : "");
                                v = dgvStations.Rows[i].Cells[1].Value;
                                xw.WriteAttributeString("URL", v != null ? dgvStations.Rows[i].Cells[1].Value.ToString() : "");
                                xw.WriteEndElement(); // für Radio
                            }

                            xw.WriteStartElement("Hotkey");
                            xw.WriteAttributeString("Enabled", cbHotkey.Checked == true ? "1" : "0");
                            xw.WriteAttributeString("Letter", hkLetter); // HIER FEHLT NOCH WAS
                            xw.WriteEndElement(); // für Hotkey

                            xw.WriteStartElement("MinClose");
                            xw.WriteAttributeString("Enabled", minClose == true ? "1" : "0");
                            xw.WriteEndElement(); // für MinClose

                            xw.WriteStartElement("AlwaysOnTop");
                            xw.WriteAttributeString("Enabled", alwaysOnTop == true ? "1" : "0");
                            xw.WriteEndElement(); // für AlwaysOnTop

                            xw.WriteStartElement("Volume");
                            xw.WriteAttributeString("Value", strVolume);
                            xw.WriteEndElement(); // für Volume

                            Point location = Location;
                            //Size size = Size;
                            if (WindowState != FormWindowState.Normal)
                            {
                                location = RestoreBounds.Location;
                                //size = RestoreBounds.Size;
                            }
                            //string[] catSpecies = { location.X.ToString(), location.Y.ToString(), size.Width.ToString(), size.Height.ToString() };
                            //formPosX = string.Join(",", catSpecies);

                            xw.WriteStartElement("Location");
                            xw.WriteAttributeString("PosX", location.X.ToString());
                            xw.WriteAttributeString("PosY", location.Y.ToString());
                            xw.WriteEndElement(); // für InitialLocation

                            xw.WriteStartElement("Autostart");
                            xw.WriteAttributeString("Station", autostartStation);
                            xw.WriteEndElement(); // für Autostart

                            xw.WriteEndElement(); // für NetRadio
                            xw.WriteEndDocument();
                            xw.Close();
                        }
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message); }
                    nothingToSave = true;
                }
            }
            else
            {
                notifyIcon.Visible = true;
                tcMain.SelectedIndex = 0;
                Hide();
                e.Cancel = true;
                notifyIcon.ShowBalloonTip(2, Text, "Left mouse click: Restore" + Environment.NewLine + "Right mouse click: Menu", ToolTipIcon.None);
            }
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (Visible)
                {
                    Hide();
                    //ShowInTaskbar = false;
                    notifyIcon.Visible = true;
                }
                else
                {
                    notifyIcon.Visible = false;
                    Show(); //BringToFront(); Visible = true; Activate();
                    //ShowInTaskbar = true;
                }
            }
        }

        private void ShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            notifyIcon.Visible = false;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exitFlag = true;
            Close();
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            using (frmSearch frmGS = new frmSearch())
            {
                if (alwaysOnTop)                {                    frmGS.TopMost = true;                }
                if (frmGS.ShowDialog() == DialogResult.OK)
                {
                    string searchString = frmGS.tbString.Text;
                    if (searchString.Length > 0)
                    {
                        using (frmBrowser frmSearch = new frmBrowser(searchString.Trim(), Location))
                        {
                            if (alwaysOnTop) { frmSearch.TopMost = true; }
                            if (frmSearch.ShowDialog() == DialogResult.OK)
                            {
                                if (dgvStations.SelectedRows.Count > 0)
                                {
                                    DataGridViewCell dgvc = dgvStations.SelectedRows[0].Cells[0];
                                    string currName;
                                    if (dgvc.Value != null && !string.IsNullOrEmpty(dgvc.Value.ToString())) { currName = dgvc.Value.ToString(); }
                                    else { currName = (dgvStations.SelectedRows[0].Index + 1) + ". row"; }
                                    if (!string.IsNullOrEmpty(dgvStations.SelectedRows[0].Cells[1].Value.ToString()) && MessageBox.Show("Overwrite " + currName + "?", "Frage", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                                    {
                                        return;
                                    }
                                    dgvStations.SelectedRows[0].Cells[0].Value = frmSearch.SelectedStation;
                                    dgvStations.SelectedRows[0].Cells[1].Value = frmSearch.SelectedURL;
                                }
                                else
                                {
                                    MessageBox.Show("Target not selected!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void BtnUp_Paint(object sender, PaintEventArgs e)
        {
            Button buttonUp = sender as Button;
            DrwaVerticalButtonText(e, buttonUp, "move row up");
        }

        private void BtnDown_Paint(object sender, PaintEventArgs e)
        {
            Button buttonDown = sender as Button;
            DrwaVerticalButtonText(e, buttonDown, "move row dwon");
        }

        private void DrwaVerticalButtonText(PaintEventArgs e, Button btn, string btnText)
        {// Code für vertikalen Text mit Unten-nach-oben-Leserichtung:
            Graphics btnGraphics = e.Graphics; // Retrieve the graphics object.
            Rectangle rec = new Rectangle()
            {
                Height = btn.Width,
                Width = btn.Height
            };
            StringFormat strf = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            btnGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            btnGraphics.TranslateTransform(0, rec.Width); // Ändert den Ursprung der Koordinaten
            btnGraphics.RotateTransform(270);      // Wendet die angegebene Drehung auf die Transformationsmatrix dieses Graphics an.
            using (SolidBrush solidBrush = new SolidBrush(btn.Enabled ? Color.Blue : SystemColors.ControlLightLight))
            {// the using statement automatically disposes the brush
                btnGraphics.DrawString(btnText, btn.Font, solidBrush, rec, strf);
            }
            //// nachfolgend Code für vertikalen Text mit oben-nach-unten-Leserichtung:
            //Graphics btnGraphics = e.Graphics; // Retrieve the graphics object.
            //Rectangle rec = btn.ClientRectangle;  // würde hier ausreichen
            //StringFormat strf = new StringFormat();
            //strf.FormatFlags = StringFormatFlags.DirectionVertical; // that's the trick
            //strf.Alignment = StringAlignment.Center;
            //strf.LineAlignment = StringAlignment.Center;
            //btnGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        }   //...

        private void BtnUp_Click(object sender, EventArgs e)
        {
            int totalRows = dgvStations.Rows.Count;
            int idx = dgvStations.SelectedCells[0].OwningRow.Index;
            if (idx != 0)
            {
                int col = dgvStations.SelectedCells[0].OwningColumn.Index;
                DataGridViewRowCollection rows = dgvStations.Rows;
                DataGridViewRow row = rows[idx];
                rows.Remove(row);
                rows.Insert(idx - 1, row);
                dgvStations.ClearSelection();
                dgvStations.CurrentCell = dgvStations.Rows[idx - 1].Cells[0];
                dgvStations.Rows[idx - 1].Selected = true;
                if (dgvStations.FirstDisplayedScrollingRowIndex == 15 && idx > 16)
                {// MessageBox.Show("FDSRI:\t" + dgvStations.FirstDisplayedScrollingRowIndex.ToString() + Environment.NewLine + "idx:\t" + idx.ToString());
                    dgvStations.FirstDisplayedScrollingRowIndex = 16;
                }
                else if (dgvStations.FirstDisplayedScrollingRowIndex >= idx - 1)
                {// MessageBox.Show("FDSRI:\t" + dgvStations.FirstDisplayedScrollingRowIndex.ToString() + Environment.NewLine + "idx:\t" + idx.ToString());
                    dgvStations.FirstDisplayedScrollingRowIndex = idx - 1;
                }
            }
            dgvStations.Focus();
        }

        private void BtnDown_Click(object sender, EventArgs e)
        {
            int totalRows = dgvStations.Rows.Count;
            int idx = dgvStations.SelectedCells[0].OwningRow.Index;
            if (idx != totalRows - 1)
            {// int col = dgvStations.SelectedCells[0].OwningColumn.Index;
                DataGridViewRowCollection rows = dgvStations.Rows;
                DataGridViewRow row = rows[idx];
                rows.Remove(row);
                rows.Insert(idx + 1, row);
                dgvStations.ClearSelection();
                dgvStations.CurrentCell = dgvStations.Rows[idx + 1].Cells[0];
                dgvStations.Rows[idx + 1].Selected = true;
                if (dgvStations.FirstDisplayedScrollingRowIndex < idx - 6)
                {
                    dgvStations.FirstDisplayedScrollingRowIndex = idx - 6;
                }
            }
            dgvStations.Focus();
        }

        private void DgvStations_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Insert)
            {
                AddToolStripMenuItem_Click(null, null);
            }
            else if (e.KeyCode == Keys.Delete)
            {
                DeleteToolStripMenuItem_Click(null, null);
            }
            else if (e.KeyCode == Keys.D1 && e.Modifiers == Keys.Alt || e.KeyCode == Keys.NumPad1 && e.Modifiers == Keys.Alt) { KeyDown_MoveRowAt(0, e); }
            else if (e.KeyCode == Keys.D2 && e.Modifiers == Keys.Alt || e.KeyCode == Keys.NumPad2 && e.Modifiers == Keys.Alt) { KeyDown_MoveRowAt(1, e); }
            else if (e.KeyCode == Keys.D3 && e.Modifiers == Keys.Alt || e.KeyCode == Keys.NumPad3 && e.Modifiers == Keys.Alt) { KeyDown_MoveRowAt(2, e); }
            else if (e.KeyCode == Keys.D4 && e.Modifiers == Keys.Alt || e.KeyCode == Keys.NumPad4 && e.Modifiers == Keys.Alt) { KeyDown_MoveRowAt(3, e); }
            else if (e.KeyCode == Keys.D5 && e.Modifiers == Keys.Alt || e.KeyCode == Keys.NumPad5 && e.Modifiers == Keys.Alt) { KeyDown_MoveRowAt(4, e); }
            else if (e.KeyCode == Keys.D6 && e.Modifiers == Keys.Alt || e.KeyCode == Keys.NumPad6 && e.Modifiers == Keys.Alt) { KeyDown_MoveRowAt(5, e); }
            else if (e.KeyCode == Keys.D7 && e.Modifiers == Keys.Alt || e.KeyCode == Keys.NumPad7 && e.Modifiers == Keys.Alt) { KeyDown_MoveRowAt(6, e); }
            else if (e.KeyCode == Keys.D8 && e.Modifiers == Keys.Alt || e.KeyCode == Keys.NumPad8 && e.Modifiers == Keys.Alt) { KeyDown_MoveRowAt(7, e); }
            else if (e.KeyCode == Keys.D9 && e.Modifiers == Keys.Alt || e.KeyCode == Keys.NumPad9 && e.Modifiers == Keys.Alt) { KeyDown_MoveRowAt(8, e); }
            else if (e.KeyCode == Keys.Home && e.Modifiers == Keys.Alt) { KeyDown_MoveRowAt(0, e); }
            else if (e.KeyCode == Keys.End && e.Modifiers == Keys.Alt) { KeyDown_MoveRowAt(dgvStations.RowCount - 1, e); }
            else if (e.KeyCode == Keys.Up && e.Modifiers == Keys.Alt)
            {
                BtnUp_Click(null, null);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Down && e.Modifiers == Keys.Alt)
            {
                BtnDown_Click(null, null);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.PageUp && e.Modifiers == Keys.Alt)
            {
                for (int j = 0; j < 8; j++)
                {
                    BtnUp_Click(null, null);
                    if (dgvStations.SelectedRows[0].Index < 1) { break; }
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.PageDown && e.Modifiers == Keys.Alt)
            {
                for (int j = 0; j < 8; j++)
                {
                    BtnDown_Click(null, null);
                    if (dgvStations.SelectedRows[0].Index >= dgvStations.RowCount - 1) { break; }
                }
                e.Handled = true;
            }
            //else if (e.KeyCode == Keys.Left)
            //{
            //    tcMain.SelectedIndex = 0;
            //    e.Handled = true;
            //}
            //else if (e.KeyCode == Keys.Right)
            //{
            //    tcMain.SelectedIndex = 2;
            //    e.Handled = true;
            //}
        }

        private void KeyDown_MoveRowAt(int rowIndex, KeyEventArgs kEA = null)
        {
            if (kEA != null) { kEA.Handled = true; kEA.SuppressKeyPress = true; }
            int idx = dgvStations.SelectedRows[0].Index;
            DataGridViewRowCollection rows = dgvStations.Rows;
            DataGridViewRow row = rows[idx];
            dgvStations.Rows.RemoveAt(idx);
            dgvStations.Rows.Insert(rowIndex, row);
            dgvStations.Rows[rowIndex].Selected = true;
            dgvStations.CurrentCell = dgvStations.Rows[rowIndex].Cells[0]; // bewirkt Scroll
        }

        private void DgvStations_SelectionChanged(object sender, EventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            int ri = -1;
            foreach (DataGridViewCell cell in dgv.SelectedCells)
            {
                ri = cell.RowIndex;
            }
            if (ri == 0)
            {
                btnUp.Enabled = false;
                btnDown.Enabled = true;
            }
            else if (ri == dgvStations.Rows.Count - 1)
            {
                btnUp.Enabled = true;
                btnDown.Enabled = false;
            }
            else
            {
                btnUp.Enabled = true;
                btnDown.Enabled = true;
            }
        }

        private void DgvStations_MouseMove(object sender, MouseEventArgs e)
        {// if (e.Button == MouseButtons.Left)
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {// If the mouse moves outside the rectangle, start the drag.
                if (dragBoxFromMouseDown != Rectangle.Empty && !dragBoxFromMouseDown.Contains(e.X, e.Y))
                {
                    DragDropEffects dropEffect = dgvStations.DoDragDrop(dgvStations.Rows[rowIndexFromMouseDown], DragDropEffects.Move);
                }
            }
        }

        private void DgvStations_MouseDown(object sender, MouseEventArgs e)
        {// Get the index of the item the mouse is below
            rowIndexFromMouseDown = dgvStations.HitTest(e.X, e.Y).RowIndex;
            colIndexFromMouseDown = dgvStations.HitTest(e.X, e.Y).ColumnIndex;
            if (e.Button == MouseButtons.Right)
            {
                dgvStations.ClearSelection();
                dgvStations.Rows[rowIndexFromMouseDown].Selected = true;
            }
            else
            {
                if (rowIndexFromMouseDown != -1)
                {// Remember the point where the mouse down occurred. The DragSize indicates the size that the mouse can move before a drag event should be started.
                    Size dragSize = SystemInformation.DragSize;
                    dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2), e.Y - (dragSize.Height / 2)), dragSize);
                }
                else
                {// Reset the rectangle if the mouse is not over an item
                    dragBoxFromMouseDown = Rectangle.Empty;
                }
            }
        }

        private void DataGridView_DragOver(object sender, DragEventArgs e)
        {// The PointToScreen conversion as dgvStations.Location.X will give co-ordinates relative to the hosted form and e.Y gives co-ordinates relative to the screen.
            if (e.Y <= PointToScreen(new Point(dgvStations.Location.X, dgvStations.Location.Y)).Y + dgvStations.Columns[0].HeaderCell.Size.Height * 2) { e.Effect = DragDropEffects.None; }
            else { e.Effect = DragDropEffects.Move; }
            int sensitveSpace = 8;
            if (e.Y <= PointToScreen(new Point(dgvStations.Location.X, dgvStations.Location.Y)).Y + dgvStations.Columns[0].HeaderCell.Size.Height * 2 + sensitveSpace)
            {// Maus nach oben
                if (dgvStations.FirstDisplayedScrollingRowIndex > 0) { dgvStations.FirstDisplayedScrollingRowIndex -= 1; }
            }
            else if (e.Y >= PointToScreen(new Point(dgvStations.Location.X + dgvStations.Width, dgvStations.Location.Y + dgvStations.Height)).Y + dgvStations.Rows[0].Height - sensitveSpace)
            {// Maus nach unten
                if (dgvStations.FirstDisplayedScrollingRowIndex <= dgvStations.RowCount) { dgvStations.FirstDisplayedScrollingRowIndex += 1; }
            }
        }

        private void DgvStations_DragDrop(object sender, DragEventArgs e)
        {// The mouse locations are relative to the screen, so they must be converted to client coordinates.
            Point clientPoint = dgvStations.PointToClient(new Point(e.X, e.Y));
            rowIndexOfItemUnderMouseToDrop = dgvStations.HitTest(clientPoint.X, clientPoint.Y).RowIndex;
            if (e.Effect == DragDropEffects.Move)
            {// If the drag operation was a move then remove and insert the row.
                DataGridViewRow rowToMove = e.Data.GetData(typeof(DataGridViewRow)) as DataGridViewRow;
                dgvStations.Rows.RemoveAt(rowIndexFromMouseDown);
                dgvStations.Rows.Insert(rowIndexOfItemUnderMouseToDrop, rowToMove);
                if (rowIndexOfItemUnderMouseToDrop > 16) { dgvStations.FirstDisplayedScrollingRowIndex += 1; }
                //if (rowIndexOfItemUnderMouseToDrop >= 0) { dgvStations.ClearSelection(); }
                dgvStations.Rows[rowIndexOfItemUnderMouseToDrop].Selected = true;
                dgvStations.CurrentCell = dgvStations.Rows[rowIndexOfItemUnderMouseToDrop].Cells[0];
            }
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvStations.SelectedRows.Count > 0)
            {
                if (!clsUtilities.isDGVRowEmpty(dgvStations.SelectedRows[0]))
                {
                    DataGridViewCell dgvc = dgvStations.SelectedRows[0].Cells[0];
                    string currName;
                    if (dgvc.Value != null && !string.IsNullOrEmpty(dgvc.Value.ToString())) { currName = dgvc.Value.ToString(); }
                    else { currName = (dgvStations.SelectedRows[0].Index + 1) + ". row"; }
                    if (MessageBox.Show("Do you want to delete " + currName + "?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) { return; }
                }
                dgvStations.Rows.RemoveAt(dgvStations.SelectedRows[0].Index);
                dgvStations.Rows.Insert(dgvStations.Rows.Count); // -1 entfällt, weil eine Zeile gelöscht wurde!
                dgvStations.CurrentCell = dgvStations.Rows[dgvStations.SelectedRows[0].Index].Cells[0]; // scrollt! //dgvStations.FirstDisplayedScrollingRowIndex = dgvStations.SelectedRows[0].Index;
            }
        }

        private void SearchStationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BtnSearch_Click(null, null);
        }

        private void AddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool isAdded = false;
            for (int row = dgvStations.RowCount - 1; row >= dgvStations.SelectedRows[0].Index; row--)
            {
                if (clsUtilities.isDGVRowEmpty(dgvStations.Rows[row]))
                {
                    if (dgvStations.SelectedRows[0].Index != row)
                    {
                        dgvStations.Rows.RemoveAt(row--); // deincrement (after the call) since we are removing the row
                        dgvStations.Rows.Insert(dgvStations.SelectedRows[0].Index);
                        dgvStations.Rows[dgvStations.SelectedRows[0].Index - 1].Selected = true;
                        dgvStations.CurrentCell = dgvStations.Rows[dgvStations.SelectedRows[0].Index].Cells[0]; // scrollt!                //dgvStations.FirstDisplayedScrollingRowIndex = dgvStations.SelectedRows[0].Index;
                        isAdded = true;
                        break;
                    }
                }
            }
            if (!isAdded) { Console.Beep(); } // MessageBox.Show("Sorry!"); }
        }

        private void Row1ToolStripMenuItem_Click(object sender, EventArgs e) { KeyDown_MoveRowAt(0); }
        private void Row2ToolStripMenuItem_Click(object sender, EventArgs e) { KeyDown_MoveRowAt(1); }
        private void Row3ToolStripMenuItem_Click(object sender, EventArgs e) { KeyDown_MoveRowAt(2); }
        private void Row4ToolStripMenuItem_Click(object sender, EventArgs e) { KeyDown_MoveRowAt(3); }
        private void Row5ToolStripMenuItem_Click(object sender, EventArgs e) { KeyDown_MoveRowAt(4); }
        private void Row6ToolStripMenuItem_Click(object sender, EventArgs e) { KeyDown_MoveRowAt(5); }
        private void Row7ToolStripMenuItem_Click(object sender, EventArgs e) { KeyDown_MoveRowAt(6); }
        private void Row8ToolStripMenuItem_Click(object sender, EventArgs e) { KeyDown_MoveRowAt(7); }
        private void Row9ToolStripMenuItem_Click(object sender, EventArgs e) { KeyDown_MoveRowAt(8); }
        private void Row10ToolStripMenuItem_Click(object sender, EventArgs e) { KeyDown_MoveRowAt(9); }
        private void Row11ToolStripMenuItem_Click(object sender, EventArgs e) { KeyDown_MoveRowAt(10); }
        private void Row12ToolStripMenuItem_Click(object sender, EventArgs e) { KeyDown_MoveRowAt(11); }
        private void UpToolStripMenuItem_Click(object sender, EventArgs e) { BtnUp_Click(null, null); }
        private void DownToolStripMenuItem_Click(object sender, EventArgs e) { BtnDown_Click(null, null); }
        private void TopToolStripMenuItem_Click(object sender, EventArgs e) { KeyDown_MoveRowAt(0); }
        private void EndToolStripMenuItem_Click(object sender, EventArgs e) { KeyDown_MoveRowAt(dgvStations.RowCount - 1); }

        private void PgUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int j = 0; j < 8; j++)
            {
                BtnUp_Click(null, null);
                if (dgvStations.SelectedRows[0].Index < 1) { break; }
            }
        }

        private void PgDnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int j = 0; j < 8; j++)
            {
                BtnDown_Click(null, null);
                if (dgvStations.SelectedRows[0].Index >= dgvStations.RowCount - 1) { break; }
            }
        }

        private void EditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rowIndexFromMouseDown >= 0 && colIndexFromMouseDown >= 0)
            {
                dgvStations.CurrentCell = dgvStations.Rows[rowIndexFromMouseDown].Cells[colIndexFromMouseDown];
                dgvStations.BeginEdit(true);
            }
        }

        private void UpdateStatusLabelStationsList()
        {
            int fullRows = 0;
            foreach (DataGridViewRow row in dgvStations.Rows) { if (!clsUtilities.isDGVRowEmpty(row)) { fullRows++; } }
            toolStripStatusLabel.Text = fullRows.ToString() + " entries";
        }

        private void DgvStations_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            UpdateStatusLabelStationsList();
        }

        private void LinkLblWebService_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try { Process.Start("http://www.radio-browser.info/"); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
        }

        private void BackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            e.Result = ClsMetadata.GetSongTilte(wPlayer.URL);
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            pnlDisplay.Cursor = Cursors.Hand;
            string songTitle = (string)e.Result;
            if (songTitle.Length > 0)
            {
                if (songTitle.StartsWith("_")) // i.e. WebException.Message
                { MessageBox.Show(songTitle.Substring(1), lblD1.Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
                else
                {
                    if (googleMsgBox && MessageBox.Show(songTitle + Environment.NewLine + Environment.NewLine + "Search on Google?", lblD1.Text, MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {// the characters \.*?+[{|()^$ must be preceded by a backslash to be seen as literal
                        try { Process.Start("https://www.google.com/#q=" + Regex.Replace(songTitle, @"[^a-zA-Z0-9äöüÄÖÜßé'\.:\(\)/]+", " ").Trim().Replace(" ", "+")); } // [^...] Matches any single character that is not in the class.
                        catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
                    }
                    lblD2.Text = songTitle;
                }
            }
            else { MessageBox.Show("No information available!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand); }
            googleMsgBox = false;
        }

        private void PanelDisplay_MouseEnter(object sender, EventArgs e)
        {
            if (wPlayer.playState == WMPLib.WMPPlayState.wmppsPlaying && !backgroundWorker.IsBusy) { { pnlDisplay.Cursor = Cursors.Hand; } }
        }

        private void PanelDisplay_MouseLeave(object sender, EventArgs e)
        {
            pnlDisplay.Cursor = Cursors.Default;
        }

        private void PanelDisplay_Click(object sender, EventArgs e)
        {
            if (wPlayer.playState == WMPLib.WMPPlayState.wmppsPlaying && !backgroundWorker.IsBusy)
            {
                pnlDisplay.Cursor = Cursors.Default;
                googleMsgBox = true;
                backgroundWorker.RunWorkerAsync();
            }
        }

        private void PlayPauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BtnPlayStop_Click(null, null);
        }

        private void CmbxStation_SelectedIndexChanged(object sender, EventArgs e)
        {
            autostartStation = cmbxStation.Text;
            nothingToSave = false;
        }

        private void PicBoxPayPal_Click(object sender, EventArgs e)
        {
            try { Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=DK9WYLVBN7K4Y"); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
        }

        private void PicBoxPayPal_MouseEnter(object sender, EventArgs e) { picBoxPayPal.Cursor = Cursors.Hand; }
        private void PicBoxPayPal_MouseLeave(object sender, EventArgs e) { picBoxPayPal.Cursor = Cursors.Default; }

        private void EditStationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RadioButton rb = ((ContextMenuStrip)(((ToolStripMenuItem)sender).Owner)).SourceControl as RadioButton;
            char rbIndex = rb.Name[rb.Name.Length - 1];
            int rbi = (int)(rbIndex - '1'); // int rbi = (int)char.GetNumericValue(rbIndex) - 1;
            tcMain.SelectedIndex = 1;
            dgvStations.Rows[rbi].Selected = true;
            dgvStations.CurrentCell = dgvStations.Rows[rbi].Cells[0]; // wg. F2, öffnet sonst 1. Zeile
        }

        private void RefreshDisplayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (wPlayer.playState == WMPLib.WMPPlayState.wmppsPlaying && !backgroundWorker.IsBusy)
            {
                pnlDisplay.Cursor = Cursors.Default;
                googleMsgBox = false;
                backgroundWorker.RunWorkerAsync();
            }
        }

        private void WhyThatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string message = @"Why does not the displayed information update itself?

For streaming and playback, the program uses a component
of the Windows Media Player, which is usually present on any
Windows-PC. (C:\Windows\System32\wmp.dll).

Unfortunately, Microsoft did not integrate the ability to read
the songtitle information.

Nevertheless, in order to get this information, it is necessary
to start a stand-alone streaming. A continuous display with
automatic change would therefore mean double internet load.";
            MessageBox.Show(message);
        }

    }
}