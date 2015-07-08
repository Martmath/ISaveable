using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using SaveableClass;

namespace WindowsFormsApplication12
{
    public class TB : Button
    {
        public string ttest = "begin";
    }

    public class tt : List<string>, ISaveable
    {
        public string T;
        private ExtISaveable.TempSave SCache;
        public ExtISaveable.TempSave GetCache()
        {
            if (SCache == null) SCache = new ExtISaveable.TempSave(this);
            return SCache;
        }
    }

    public class TT : ISaveable
    {
        public List<object> Anon1
        { get; set; }
        public List<object> Anon2
        { get; set; }
        public DataSet DS
        { get; set; }
        public Dictionary<string, TB>[] AR
        { get; set; }
        public DataTable TA
        { get; set; }
        public List<Dictionary<string, TB>> l1
        { get; set; }
        public List<Dictionary<string, TB>> l2
        { get; set; }
        public TB TB1
        { get; set; }
        public Tuple<int, TB> TB2
        { get; set; }
        public SortedList<string, TB> D_str_TB
        { get; set; }
        public Dictionary<SortedList<string, TB>, TB> DD
        { get; set; }
        public List<DataSet> DDS
        { get; set; }
        public List<Dictionary<string, TB>> L
        { get; set; }
        public List<Tuple<int, TB>> TB3
        { get; set; }
        public List<Dictionary<string, TB>[]> AAR
        { get; set; }
        public List<List<Dictionary<string, TB>>> ll
        { get; set; }
        public List<string> St
        { get; set; }
        private ExtISaveable.TempSave SCache;
        public ExtISaveable.TempSave GetCache()
        {
            if (SCache == null) SCache = new ExtISaveable.TempSave(this);
            return SCache;
        }
    }

    public class TTEST : ISaveable
    {
        private ExtISaveable.TempSave SCache;
        /* public void SetCache(ExtISaveable.TempSave s)
         {
             ExtISaveable.TempSave S = s.ReturnClone();
             S.This = this;
             SCache = S;
         }
         */
        public ExtISaveable.TempSave GetCache()
        {
            if (SCache == null) SCache = new ExtISaveable.TempSave(this);
            return SCache;
        }
        private Button z = null;
        public Button Z
        {
            get
            {
                return this.z;
            }
            set
            {
                this.z = value;
            }
        }
        private string[,] INN = null;
        public string[,] inn
        {
            get
            {
                return this.INN;
            }
            set
            {
                this.INN = value;
            }
        }
        private List<Button> y = new List<Button>();
        public List<Button> Y
        {
            get
            {
                return this.y;
            }
            set
            {

                this.y = value;

            }
        }
        Tuple<int, Button> eer;// = Tuple.Create(65, "uuu");
        public Tuple<int, Button> EER
        {
            get
            {
                return this.eer;
            }
            set
            {
                this.eer = value;
            }
        }
        DataTable ta = null;
        public DataTable TA
        {
            get
            {
                return this.ta;
            }
            set
            {
                this.ta = value;

            }
        }
        DataSet st = new DataSet("office");
        public DataSet ST
        {
            get
            {
                return this.st;
            }
            set
            {
                this.st = value;
            }
        }
        private List<string> IIN = new List<string>() { "oo", "ooo" };
        public List<string> iin
        {
            get
            {
                return this.IIN;
            }
            /* set
            {
                this.IIN = value;          
            }*/
        }
        Dictionary<int, string> gg = new Dictionary<int, string>() { { 12, "ww" }, { 22, "bhh" }, { 232, "wwwq" } };
        public Dictionary<int, string> GG
        {
            get
            {
                return this.gg;
            }
            set
            {
                this.gg = value;
            }
        }
        private object[,] qq;// = new object[,] { { "oo", "ooo" }, { "ddf", "fde" } };
        public object[,] QQ
        {
            get
            {
                return this.qq;
            }
            set
            {
                this.qq = value;
            }
        }
    }
}
