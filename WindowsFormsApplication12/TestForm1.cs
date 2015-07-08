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
    
    public partial class TestForm1 : Form, ISaveable
    {
        private ExtISaveable.TempSave SCache;
        public ExtISaveable.TempSave GetCache()
        {
            if (SCache == null) SCache = new ExtISaveable.TempSave(this);
            return SCache;
        }
        
        public TestForm1()
        {
            InitializeComponent();
        }

        private void TestForm1_Load(object sender, EventArgs e)
        {

        }

        private void TestForm1_Load_1(object sender, EventArgs e)
        {
        
        }
    }
}
