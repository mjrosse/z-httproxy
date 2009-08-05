using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
namespace Proxy
{
    public class ProcessException
    {
        public static void Process(Exception e)
        {
            //MessageBox.Show(e.ToString());
        }

        public static void Process(string Message)
        {
            //MessageBox.Show(Message);
        }
    }
}
