using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
namespace Proxy
{
    public class Logger
    {
        private enum DebugLevel
        {
            Debug=0,
            Message,
            Warning,
            Error,
            None
        }

        private DebugLevel debugLv;
        private string InitString(string dbgLv)
        {
            string ret = "";

            ret += dbgLv + "::";

            if (dbgLv == "Error")
            {
            }

            if (dbgLv == "Warning") 
            {
            }

            if (dbgLv == "Message")
            {
            }

            if (dbgLv == "Debug")
            {
            }

            ret = ret + "ThreadID=" + Thread.CurrentThread.GetHashCode().ToString() + "::";
            return ret;
        }

        public Logger()
        {
            debugLv = (DebugLevel)Int32.Parse(System.Configuration.ConfigurationSettings.AppSettings["DebugLevel"]);
        }

        public void Error(string Message)
        {
            if ( debugLv == DebugLevel.None)
                return;
            string msg = InitString("Error");
            msg = msg+"Message-->" + Message;
            ProcessMessage(msg);

        }

        public void Warning(string Message)
        {
            if (debugLv > DebugLevel.Warning)
                return;
            string msg = InitString("Warning");
            msg = msg + "Message-->" + Message;
            ProcessMessage(msg);
        }

        public void Message(string Message)
        {
            if (debugLv > DebugLevel.Message)
                return;
            string msg = InitString("Message");
            msg = msg + "Message-->" + Message;
            ProcessMessage(msg);
        }

        public void Debug(string Message)
        {
            if (debugLv>DebugLevel.Debug)
                return;
            string msg = InitString("Debug");
            msg = msg + "Message-->" + Message;
            ProcessMessage(msg);
        }

        public void ProcessMessage(string Message)
        {
            SaveToFile(Message);
        }

        public void SaveToFile(string Message)
        {
            try
            {
                StreamWriter sw = new StreamWriter(@"d:\a.log", true, Encoding.Default);
                sw.WriteLine(Message);
                sw.Close();
            }
            catch { ;}
        }
    }

}
