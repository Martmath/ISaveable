using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SaveableClass;

namespace WindowsFormsApplication12
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static SaveableClass.ExtISaveable.StatD.WhatMustDo wmd()
        {
            //The supported C # does not inherit from multiple classes ((
            return SaveableClass.ExtISaveable.StatD.WhatMustDo.IgnoreDifferentType;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            var TF1 = new TestForm1();
            TF1.Show();            
            string Label=TF1.Save();
            Debug_.Debug.Print( TF1.GetCache().ToString());          
            var ISCache = ((ISaveable)TF1).GetCache();
            var TF2 = new TestForm2();
            TF2.GetCache().whatMustDo = wmd;
            TF2.LoadData(Label);            
            TF2.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var TF2 = new TestForm2();
            TF2.Show();
            TF2.GetCache().Init();
            string S=TF2.GetCache().ToSerializableString();
            Debug_.Debug.Print(S);           
            var TF1 = new TestForm1();            
            TF1.GetCache().whatMustDo = wmd;
            TF1.GetCache().FromSerializableString(S);
            TF1.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {  
            var a1 = new { tyn = (UInt16)12, jj = "test111" };
            var a2 = new { rnh = (UInt16)12, rttr = button1 };   

            TT t = new TT();
            t.St = new List<string>() { "iii", "hyiy" };
            t.Anon1 = new List<object>() { a1, a2 };
            t.Anon2 = new List<object>() { a2, a1 };
            t.TA = new DataTable("patients_tName", "tabNsp");
            t.TA.Columns.Add("name");
            t.TA.Columns.Add("id", typeof(Button));
            t.TA.Rows.Add("sam", button1);
            t.TA.Rows.Add("mark", button2);
            t.TA.Rows.Add("mareek", button2);
            t.DS = new DataSet("uuu");
            t.DS.Tables.Add(t.TA);
            //   var wtw = (Button)t.TA.Rows[1][1];
            TB b = new TB();
            b.ttest = "1";
            TB B = new TB();
            B.ttest = "2";
            t.TB1 = b;
            t.TB2 = Tuple.Create(11, b);
            t.TB3 = new List<Tuple<int, TB>>() { t.TB2 };
            //   t.Save();
            t.D_str_TB = new SortedList<string, TB>();
            t.DDS = new List<DataSet>() { t.DS };
            string tt = t.D_str_TB.GetType().FullName;
            t.D_str_TB.Add("ku", b);
            Dictionary<string, TB> D_s_TB = new Dictionary<string, TB>();
            D_s_TB.Add("oo", B);
            t.AR = new Dictionary<string, TB>[2] { D_s_TB, D_s_TB };
            t.AAR = new List<Dictionary<string, TB>[]> { t.AR };
            t.DD = new Dictionary<SortedList<string, TB>, TB>();
            t.DD.Add(t.D_str_TB, b);
            t.L = new List<Dictionary<string, TB>>();
            // t.L.Add(t.D_str_TB);
            t.L.Add(D_s_TB);
            t.l1 = t.L;
            t.l2 = t.L;
            t.ll = new List<List<Dictionary<string, TB>>>();
            t.ll.Add(t.L);
            t.GetCache().Init();//Save();
            var ww = t.GetCache();
            string ert = t.GetCache().ToSerializableString();
            t.Save();            
            t.LoadData();
            TT t2 = new TT();
            // t2.GetCache().FromSerializableString(ert);
            t2.LoadData();
            Debug_.Debug.InitF(2, 1);
            Debug_.Debug.Print(t2.GetCache().ToString(), 1);            
            Debug_.Debug.Print(t.GetCache().ToString(), 0);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            tt bg = new tt() { "iii", "ouu" };
            bg.T = "9999";
            bg.GetCache().Init();
            bg.Save();
            tt bg2 = new tt();
            bg2.T = "4444";
            // string fr = bg.GetCache().ToSerializableString();
            // bg2.GetCache().FromSerializableString(fr);
            bg2.LoadData();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            TTEST t = new TTEST();
            t.Z = button1;
            t.inn = new string[,] { { "oo", "ooo" }, { "ddf", "fde" } };
            t.iin[1] = "uuuu";
            // t.iin = T;
            t.TA = new DataTable("patients");
            t.TA.Columns.Add("name");
            t.TA.Columns.Add("id", typeof(Button));
            t.TA.Rows.Add("sam", button1);
            t.TA.Rows.Add("mark", button2);
            var ww = (Button)t.TA.Rows[1][1];
            DataTable table1 = new DataTable("patients");
            table1.Columns.Add("name");
            table1.Columns.Add("id");
            table1.Rows.Add("sam", 1);
            table1.Rows.Add("mark", 2);

            DataTable table2 = new DataTable("medications");
            table2.Columns.Add("nnn");
            table2.Columns.Add("xxxx");
            table2.Rows.Add(1, "zzz");
            table2.Rows.Add(2, "yyy");
                     
            t.ST.Tables.Add(t.TA);
            t.ST.Tables.Add(table2);

            t.EER = Tuple.Create(65, this.button2);

            t.Y.Add(this.button1);
            t.Y.Add(this.button2);

            TTEST t2 = new TTEST();

            t2.LoadData(t.Save());
        }
    }
}
