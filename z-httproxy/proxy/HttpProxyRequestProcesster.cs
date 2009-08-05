using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;


namespace Proxy
{
    public class HttpProxyRequestProcesster
    {
        private Socket sk;
        public static ArrayList Threads;

        public HttpProxyRequestProcesster(Socket TcpClientSocket)
        {
            sk = TcpClientSocket;
        }

        public void Process()
        {
            lock (Threads)
            {
                Threads.Add(Thread.CurrentThread);
            }



            lock (Threads)
            {
                Threads.Remove(Thread.CurrentThread);
            }
            
        }
    }
}
