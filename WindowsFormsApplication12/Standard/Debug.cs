using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WindowsFormsApplication12;
using WApi;

namespace Debug_
{
    public static class Debug //:WindowsWork.BackRoom.Singleton<Debug_>
    {
        private static bool _InFile = false;
        private static string _Path = string.Empty;
        private static int _H = 200;
        private static int _W = 200;        
        private static List<RichTextBox> _RHWnds = new List<RichTextBox>();
        private static int _h = 1;
        private static int _w = 1;
        private static IntPtr _HWnd = IntPtr.Zero;        
        public static IntPtr HWnd
        {
            get { return _HWnd; }
            set { _HWnd = value; }
        }       
   
       // private IntPtr _MHWnd = IntPtr.Zero;

        private static  string _Header = string.Empty;
        public static string Header
        {
            get { return _Header; }
            private  set { _Header = value; }
        }

        public static void SetOutPut(IntPtr HWnd)
        {
            _RHWnds.Clear();
            _InFile = false;
            _HWnd = HWnd;
        }
        public static void SetOutPut(string Path)
        {
            _RHWnds.Clear();
            _InFile = true;
            _Path = Path;

        }
        public static void Clear()
        {
            if (_InFile) System.IO.File.WriteAllText(_Path, string.Empty);
            else
                Wapi.SetText(_HWnd, string.Empty);         
        
        }
        public static void Print(string S, int N = 0, System.Drawing.Color? Colour = null)
        { OutT(S, Environment.NewLine, "", N, Colour); }

        public static void Write(string S) 
        {OutT(S);}
       
        public static void WriteLine(string S)
        { OutT(S, "", Environment.NewLine); }

        private static void OutT(string S, string Before = "", string After = "", int N = 0, System.Drawing.Color? Colour = null)
        {
           // _C = Colour ?? System.Drawing.Color.Black;            
            S = Before + S + After;
            if (_InFile) System.IO.File.AppendAllText(_Path, S);
            else
            {
                if ((_HWnd == IntPtr.Zero)||(!Wapi.IsWindow(_HWnd))) InitF(_w,_h);
                if (N >= _RHWnds.Count) N = -1;
                if (N > -1)
                {
                    _RHWnds[N].AppendText(S);
                    if ((Colour != null) && (Colour != System.Drawing.Color.Black))
                    {
                        int e = _RHWnds[N].Text.LastIndexOf((char)10);
                        _RHWnds[N].Select(e, _RHWnds[N].TextLength-e);
                        _RHWnds[N].SelectionColor = (System.Drawing.Color)Colour;
                        _RHWnds[N].Select(_RHWnds[N].TextLength, 0);
                        _RHWnds[N].SelectionColor = System.Drawing.Color.Black;
                    }
                }
                else Wapi.SetText(_HWnd, Wapi.GetTextWin(_HWnd) + S);
            }
        }
        public static void InitF(int W=1,int H=1)
{
            _h=(H > 0) ?  H :  1;
            _w = (W > 0) ? W : 1;            
            Form frm = new Form();           
            frm.Name = "frm_test";       
            frm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;   
            frm.Width = _W;
            frm.Height = _H;
            frm.Text = "MDebug";
            frm.Visible = true;
            frm.ResizeEnd += new EventHandler(ResizeEnd);
            frm.Resize += new EventHandler(ResizeEnd);
            RichTextBox f1 = new RichTextBox();            
            frm.Controls.Add(f1);
           // f1.TextChanged += new EventHandler(TextChanged);
            _RHWnds.Clear(); _HWnd = f1.Handle; 
            _RHWnds.Add(f1);
            for (int i = 1; i < W * H; i++) {
                f1 = new RichTextBox(); frm.Controls.Add(f1);
             //   f1.TextChanged += new EventHandler(TextChanged);
                _RHWnds.Add(f1);
             }            
            ResizeEnd(frm, null);
                  
        }

        private static void TextChanged(object sender, EventArgs e)
        {
            //пример
            RichTextBox f = (RichTextBox)sender;
            if ((f.Handle == _HWnd) && (f.Text != "")
                // &&(_C != System.Drawing.Color.Black)
                )
            { // System.Drawing.Color _c=_C;_C = System.Drawing.Color.Black;            
                f.TextChanged -= TextChanged;
                f.Select(f.GetFirstCharIndexFromLine(f.Lines.Length - 1), f.Lines[f.Lines.Length - 1].Length);
                f.SelectionColor = System.Drawing.Color.Blue;
                f.TextChanged += TextChanged;
            }
        }

private static void ResizeEnd(object sender, EventArgs e)
        {
            Form f = (Form)sender;
            _H = f.Height; _W = f.Width;
            int w = _W - 20;
            int h = _H - 40;
            int wl, hl,t,l;
            CompWH(w, h, _w, _h, out wl, out hl); 
            for(int i=0; i < f.Controls.Count;i++){
            CompPlace(i%_w, i/_w, wl, hl, out l, out t);
            f.Controls[i].Width = wl;  f.Controls[i].Height = hl;
            f.Controls[i].Top = t;     f.Controls[i].Left = l;
           // f.Controls[i].Text = i.ToString();
        }
        }
        private static void CompWH(int W, int H,int WN,int HN,out int w,out int h)
        {
            w = W / WN; h = H / HN;
        }
        private static void CompPlace(int W, int H, int w, int h, out int _w, out int _h)
        {
            _w = W *w; _h = H *h;
        }

        public static void RunApp(string S)
        {

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = S;            
            proc.Start();
            proc.WaitForInputIdle();
            _Header=proc.MainWindowTitle;
        }

     /*   public static void RunApp()
        {

            IntPtr ee = IntPtr.Zero;
            ee = Wapi.FindWindow(null, "OutPutWindow");
            if (ee == IntPtr.Zero)
            {

                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory + "OutPutWindow.exe";
                proc.Start();
                proc.WaitForInputIdle();
                Wapi.Wait(1);
                _Header = proc.MainWindowTitle;
                ee = Wapi.FindWindow(null, Debug.Header);
                IntPtr ChildWin1 = Generate.GetChildWindowAll(ee, "WindowsForms10.RichEdit", 1);
                Debug.SetOutPut(ChildWin1);
            }
            else
            {
                Debug._Header = "OutPutWindow";
                IntPtr ChildWin1 = Generate.GetChildWindowAll(ee, "WindowsForms10.RichEdit", 1);
                Debug.SetOutPut(ChildWin1);


            }
        }*/

     

   
    }
}
