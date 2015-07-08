using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WApi
{
    public static class Wapi
    {
        #region Message
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint wMsg, int wParam, IntPtr lParam);
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint wMsg, uint wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int SendMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, int lParam);
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int SendMessage(IntPtr hWnd, uint wMsg, int wParam, int lParam);
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int SendMessage(IntPtr hWnd, uint wMsg, uint wParam, int lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int SendMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, StringBuilder lParam);
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int SendMessage(IntPtr hWnd, uint wMsg, int wParam, StringBuilder lParam);
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int SendMessage(IntPtr hWnd, uint wMsg, uint wParam, StringBuilder lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, bool lParam);
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint wMsg, int wParam, bool lParam);
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint wMsg, uint wParam, bool lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int SendMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, ref RECT R);
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int SendMessage(IntPtr hWnd, uint wMsg, int wParam, ref RECT R);
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int SendMessage(IntPtr hWnd, uint wMsg, uint wParam, ref RECT R);

        [DllImport("user32.dll", EntryPoint = "PostMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", EntryPoint = "PostMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint wMsg, int wParam, IntPtr lParam);
        [DllImport("user32.dll", EntryPoint = "PostMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint wMsg, uint wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "PostMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int PostMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, int lParam);
        [DllImport("user32.dll", EntryPoint = "PostMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int PostMessage(IntPtr hWnd, uint wMsg, int wParam, int lParam);
        [DllImport("user32.dll", EntryPoint = "PostMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int PostMessage(IntPtr hWnd, uint wMsg, uint wParam, int lParam);

        [DllImport("user32.dll", EntryPoint = "PostMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int PostMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, StringBuilder lParam);
        [DllImport("user32.dll", EntryPoint = "PostMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int PostMessage(IntPtr hWnd, uint wMsg, int wParam, StringBuilder lParam);
        [DllImport("user32.dll", EntryPoint = "PostMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int PostMessage(IntPtr hWnd, uint wMsg, uint wParam, StringBuilder lParam);

        [DllImport("user32.dll", EntryPoint = "PostMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, bool lParam);
        [DllImport("user32.dll", EntryPoint = "PostMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint wMsg, int wParam, bool lParam);
        [DllImport("user32.dll", EntryPoint = "PostMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint wMsg, uint wParam, bool lParam);

        [DllImport("user32.dll", EntryPoint = "PostMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int PostMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, ref RECT R);
        [DllImport("user32.dll", EntryPoint = "PostMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int PostMessage(IntPtr hWnd, uint wMsg, int wParam, ref RECT R);
        [DllImport("user32.dll", EntryPoint = "PostMessage", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern int PostMessage(IntPtr hWnd, uint wMsg, uint wParam, ref RECT R);

        public static class WM
        {
            public const uint GETTEXT = 0x0D;
            public const uint GETTEXTLENGTH = 0x0E;
            public const uint SETTEXT = 0x000C;
            public const uint COPY = 0x0301;
            public const uint PASTE = 0x0302;
            public const uint CUT = 0x0300;
        }       
        
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }        
        #endregion
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);
        private static int GetTextBoxTextLength(IntPtr hTextBox)
        {
            return Wapi.SendMessage(hTextBox, WM.GETTEXTLENGTH, 0, 0);
        }
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        public static string GetClassName(IntPtr H)
        {
            StringBuilder Buff = new StringBuilder(256);
            int j = Wapi.GetClassName(H, Buff, Buff.Capacity);
            return Buff.ToString().Trim();
        }
        public static string GetTextWin(IntPtr H)
        {
            StringBuilder Buff = new StringBuilder(GetTextBoxTextLength(H) + 1);
            int j = Wapi.SendMessage(H, WM.GETTEXT, Buff.Capacity, Buff);
            return Buff.ToString().Trim();
        }
        public static string GetTextWinL(IntPtr H)
        {
            string s = GetTextWin(H);
            if (s.Length > 50) s = s.Substring(0, 50) + "...";
            return s;
        }
        public static void SetText(IntPtr hWnd, string s)
        {
            Wapi.SendMessage(hWnd, WM.SETTEXT, 0, new StringBuilder(""));
            if (s.Length > 0)
            {
                Clipboard.SetText(s);
                Wapi.SendMessage(hWnd, WM.PASTE, 0, 0);
            }
        }
    }
}
