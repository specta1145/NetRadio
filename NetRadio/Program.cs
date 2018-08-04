using System;
using System.IO;
using System.Threading; // Mutex
using System.Windows.Forms;
using System.Reflection;

namespace NetRadio
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static Mutex mutex = new Mutex(true, "{8F4J0AC4-WH29-57GD-A8CF-72F04E6BDE8F}");
        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                string dllPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Interop.WMPLib.dll";
                if (File.Exists(dllPath))
                {
                    Application.Run(new frmMain());
                }
                else
                {
                    MessageBox.Show("Missing \"" + dllPath + "\"!", "NetRadio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                mutex.ReleaseMutex();
            }
            else
            {// send our Win32 message to make the currently running instance; jump on top of all the other windows
                clsUtilities.PostMessage((IntPtr)clsUtilities.HWND_BROADCAST, clsUtilities.WM_SHOWME, IntPtr.Zero, IntPtr.Zero);
            }
        }
    }
}
