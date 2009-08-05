using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Proxy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void On_StartClick(object sender, EventArgs e)
        {
            HttpProxy t= new HttpProxy();
            Thread ts = new Thread(new ThreadStart(t.Run));
            ts.Start();
            this.button1.Enabled = false;
        }
    }
}
