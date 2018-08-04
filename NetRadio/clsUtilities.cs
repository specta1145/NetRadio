using System;
using System.Windows.Forms;
using System.Reflection; // Assembly
//using System.ComponentModel;
using System.Threading; // Mutex
using Microsoft.Win32; // Registry
using System.Runtime.InteropServices;

namespace NetRadio
{
    class clsUtilities // internal is standard
    {
        private const string runLocation = @"Software\Microsoft\Windows\CurrentVersion\Run";
        public const int WM_HOTKEY = 0x312;
        public const int HOTKEY_ID = 0x0312; // 0; // 42;
        public const int WM_QUERYENDSESSION = 0x0011;
        public enum Modifiers : uint { Alt = 0x0001, Control = 0x0002, Shift = 0x0004, Win = 0x0008 }

        public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);
        
        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        public static void SetAutoStart(string appName, string assemblyLocation)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(runLocation);
            key.SetValue(appName, assemblyLocation);
        }

        public static bool IsAllLower(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (Char.IsLetter(input[i]) && !Char.IsLower(input[i]))
                    return false;
            }
            return true;
        }

        public static bool IsAutoStartEnabled(string appName, string assemblyLocation)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(runLocation);
            if (key == null) return false;
            string value = (string)key.GetValue(appName);
            if (value == null) return false;
            return (value == assemblyLocation);
        }

        public static void UnSetAutoStart(string appName)
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(runLocation);
            key.DeleteValue(appName);
        }

        public static string RemoveFromEnd(string str, string toRemove)
        {
            if (str.EndsWith(toRemove))
                return str.Substring(0, str.Length - toRemove.Length);
            else
                return str;
        }

        public static string GetDescription()
        {
            Type clsType = typeof(frmMain);
            Assembly assy = clsType.Assembly;
            AssemblyDescriptionAttribute adAttr = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assy, typeof(AssemblyDescriptionAttribute));
            if (adAttr == null) { return String.Empty; }
            return adAttr.Description;
        }

        public static bool isDGVEmpty(DataGridView gridView)
        {
            bool isEmpty = true;
            for (int row = 0; row < gridView.RowCount - 1; row++)
            {
                for (int col = 0; col < gridView.Columns.Count; col++)
                {
                    if (gridView.Rows[row].Cells[col].Value != null && !String.IsNullOrEmpty(gridView.Rows[row].Cells[col].Value.ToString()))
                    { isEmpty = false; break; }
                }
            }
            return isEmpty;
        }

        public static bool isDGVRowEmpty(DataGridViewRow row)
        {
            for (int i = 0; i < row.Cells.Count; i++)
            {
                if (row.Cells[i].Value != null)
                {// if datagridview is databound, you'd better check whether the cell value is string.Empty
                    if (!isNullOrWhiteSpace(row.Cells[i].Value.ToString())) //  if (row.Cells[i].Value.ToString() != string.Empty)
                    {// if value of any cell is not null, this row need to be readonly
                        return false;
                    }// if there is an unbound checkbox column, you may need to check whether the cell value is null or false(uncheck).
                }
            } 
            return true;
        }

        public static bool isNullOrWhiteSpace(string value)
        {// ab .Net 4.5 verfügbar
            if (value == null) return true;
            for (int i = 0; i < value.Length; ++i)
            {// return value.All(char.IsWhiteSpace); using System.Linq (auch > .Net 4.0;)
                if (!char.IsWhiteSpace(value[i])) return false;
            }
            return true;
        }

        public static void ResizeColumns(ListView lv, bool bBlockUIUpdate)
        {// KeePass\UI\UIUtil.cs
            if (lv == null) { return; }
            int nColumns = 0;
            foreach (ColumnHeader ch in lv.Columns)
            {
                if (ch.Width > 0) ++nColumns;
            }
            if (nColumns == 0) return;
            int cx = (lv.ClientSize.Width - 1) / nColumns;
            int cx0 = (lv.ClientSize.Width - 1) - (cx * (nColumns - 1));
            if ((cx0 <= 0) || (cx <= 0)) return;
            if (bBlockUIUpdate) lv.BeginUpdate();
            bool bFirst = true;
            foreach (ColumnHeader ch in lv.Columns)
            {
                int nCurWidth = ch.Width;
                if (nCurWidth == 0) continue;
                if (bFirst && (nCurWidth == cx0)) { bFirst = false; continue; }
                if (!bFirst && (nCurWidth == cx)) continue;

                ch.Width = (bFirst ? cx0 : cx);
                bFirst = false;
            }
            if (bBlockUIUpdate) lv.EndUpdate();
        }

    }
}
