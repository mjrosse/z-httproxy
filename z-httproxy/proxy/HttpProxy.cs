using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;
using System.Net.Sockets;

namespace Proxy
{
    public class HttpProxy
    {
        private static HttpProxy pro = new HttpProxy();
        private int port = 8080;

        private bool isRunning;
        private TcpListener tcp;

        public HttpProxy()
        {

        }

        public bool IsRunning
        {
            get { return isRunning; }
            set { isRunning = value; }
        }
        
        public int Port
        {
            get { return this.port; }
            set
            {
                if (Port <= 0 || Port >= 65535)
                {
                    MessageBox.Show("端口范围不对！");
                    return;
                }

                this.port = value;

            }
        }

        public HttpProxy GetInstance()
        {
            if (pro != null)
            {
                return pro;
            }
            else
            {
                pro = new HttpProxy();
                return pro;
            }
        }

        public void Run()
        {
            
            if (isRunning == true)
                return;

            tcp = new TcpListener(new IPAddress(0), 8080);

            try
            {
                tcp.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                return;
            }
            isRunning = true;
            Socket sk;
            while (isRunning)
            {
                try
                {
                    sk = tcp.AcceptSocket();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    continue;
                }

                HttpProxyRequestProcesster sp = new HttpProxyRequestProcesster(sk);
                
                
            }
        }

        public void Stop()
        {
            isRunning = false;
        }

        public void ProcessRequest()
        {

        }
    }
}
