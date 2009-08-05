using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace Proxy
{
    public class Logger
    {
        private enum DebugLevel
        {
            None=0,
            Debug,
            Message,
            Warning,
            Error,
            
        }

        private DebugLevel debugLv;

        public Logger()
        {
            debugLv = (DebugLevel)Int32.Parse(System.Configuration.ConfigurationSettings.AppSettings["DebugLevel"]);
        }

        public void Error(string Message)
        {
            if (debugLv == DebugLevel.None)
                return;
        }

        public void Warning(string Message)
        {
            if (debugLv < DebugLevel.Warning || debugLv == DebugLevel.None)
                return;
        }

        public void Message(string Message)
        {
            if (debugLv < DebugLevel.Message || debugLv == DebugLevel.None)
                return;
        }

        public void Debug(string Message)
        {
            if (debugLv == DebugLevel.None)
                return;
        }
    }

}
