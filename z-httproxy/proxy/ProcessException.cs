using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
namespace Proxy
{
    public class ProcessException
    {
        private static Logger log = new Logger();
        public static void Process(Exception e)
        {
            //MessageBox.Show(e.ToString());
            log.Error(e.ToString());
        }

        public static void Process(string Message)
        {
            //MessageBox.Show(Message);
            log.Error(Message);
        }
    }
}
