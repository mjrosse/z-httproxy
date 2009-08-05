using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Proxy
{
    public class HttpProxyRequestProcesster
    {
        private Socket ClientSocket;
        private Encoding ASCII = Encoding.ASCII;
        private Logger log;
        private static ArrayList _Threads=new ArrayList();
        public static ArrayList Threads
        {
            get { return HttpProxyRequestProcesster._Threads; }
        }

        
        public HttpProxyRequestProcesster(Socket TcpClientSocket)
        {
            ClientSocket = TcpClientSocket;
            log = new Logger();
        }

        public void Process()
        {
            Init();

            Byte[] ReadBuff = new byte[1024 * 10];
            try
            {
                int Length = ClientSocket.Receive(ReadBuff);
                if (0 == Length)
                {
                    End();
                    return;
                }
            }
            catch(Exception e)
            {
                ProcessException.Process(e);
                End();
            }

            string ClientMsg = ASCII.GetString(ReadBuff);
            string Line = ClientMsg.Substring(0, ClientMsg.IndexOf("\r\n"));
            string[] CmdArray = Line.Split(' ');
            string Cmd = CmdArray[0];
            string RawUrl = CmdArray[1];
            if (Cmd == "CONNECT")
            {
                DoConnect(RawUrl);
            }
            else
            {
                DoOther(RawUrl, ClientMsg);
            } 
            End();
        }


        private void DoConnect(string RawUrl)
        {
            string[] Args = RawUrl.Split(':');
            string Host = Args[0];
            int Port = int.Parse(Args[1]);
            Socket ServerSocket = null;
            try
            {
                IPAddress[] IpList = Dns.GetHostEntry(Host).AddressList;
                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ServerSocket.Connect(IpList[0], Port);
            }
            catch (Exception e)
            {
                ProcessException.Process(e);
            }


            if (ServerSocket.Connected)
            {
                ClientSocket.Send(ASCII.GetBytes("HTTP/0 200 Connection establishedrnrn"));
            }
            else
            {
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Close();
            }

            ForwardTcpData(ClientSocket, ServerSocket); 
        }

        public void Init()
        {
            lock (_Threads)
            {
                _Threads.Add(Thread.CurrentThread);
            }
        }


        public void End()
        {
            lock (_Threads)
            {
                _Threads.Remove(Thread.CurrentThread);
            }
        }


        /// <summary> 
        /// 处理GET，POST等命令。使用了POLL，在代理服务器中强制去掉了Keep-Alive能力 
        /// </summary> 
        /// <param name="RawUrl"></param> 
        /// <param name="ClientMsg"></param> 
        public void DoOther(string RawUrl, string ClientMsg)
        {

            RawUrl = RawUrl.Substring(0 + "http://".Length);
            int Port;
            string Host;
            string Url;

            // 下面是分割处理请求，此处应该用正则匹配，不过我不擅长，因此手动切割，—_—! 
            int index1 = RawUrl.IndexOf(':');
            // 没有端口 
            if (index1 == -1)
           {
                Port = 80;

                int index2 = RawUrl.IndexOf('/');
                // 没有目录 
                if (index2 == -1)
                {
                    Host = RawUrl;
                    Url = "/";
                }
                else
                {
                    Host = RawUrl.Substring(0, index2);
                    Url = RawUrl.Substring(index2);
                }
            }

            else
            {
                int index2 = RawUrl.IndexOf('/');
                // 没有目录 
                if (index2 == -1)
                {
                    Host = RawUrl.Substring(0, index1);
                    Port = Int32.Parse(RawUrl.Substring(index1 + 1));
                    Url = "/";
                }
                else
                {
                    // /出现在:之前，则说明:后面的不是端口 
                    if (index2 < index1)
                    {
                        Host = RawUrl.Substring(0, index2);
                        Port = 80;
                    }
                    else
                    {
                        Host = RawUrl.Substring(0, index1);
                        Port = Int32.Parse(RawUrl.Substring(index1 + 1, index2 - index1 - 1));
                    }
                    Url = RawUrl.Substring(index2);
                }
            }

            IPAddress[] address = null;
            try
            {
                IPHostEntry IPHost = Dns.GetHostEntry(Host);
                address = IPHost.AddressList;
            }
            catch (Exception e)
            {
                ProcessException.Process(new Exception("解析服务器地址异常: " + e.Message));
            }
            Socket IPsocket = null;
            try
            {
                // 连接到真实WEB服务器 
                IPEndPoint ipEndpoint = new IPEndPoint(address[0], Port);
                IPsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPsocket.Connect(ipEndpoint);

                // 对WEB服务器端传送HTTP请求命令，将原始HTTP请求中HTTP PROXY部分包装去掉 
                string ReqData = ClientMsg;

                // 改写头中的URL,http://www.test.com/index.php改为/index.php 
                ReqData = ReqData.Replace("http://" + RawUrl, Url);

                // 按照rn切分HTTP头 
                string[] ReqArray = ReqData.Split(new string[1] { "rn" }, StringSplitOptions.None);
                ReqData = "";



                // 改写Keep-Alive等字段 
                for (int index = 0; index < ReqArray.Length; index++)
                {
                    /* 

                    if (ReqArray[index].StartsWith("Accept-Encoding:")) 

                    { 

                    ReqArray[index] = "Accept-Encoding: deflate"; 

                    } 

                    */

                    if (ReqArray[index].StartsWith("Proxy-Connection:"))
                    {

                        ReqArray[index] = ReqArray[index].Replace("Proxy-Connection:", "Connection:");

                        //ReqArray[index] = "Connection: close"; 

                    }

                    /* 

                    else if (ReqArray[index].StartsWith("Keep-Alive:")) 

                    { 

                    ReqArray[index] = ""; 

                    } 

                    */

                    // 修改后的字段组合成请求 
                    if (ReqArray[index] != "")
                    {
                        ReqData = ReqData + ReqArray[index] + "rn";
                    }
                }

                ReqData = ReqData.Trim();
                byte[] SendBuff = ASCII.GetBytes(ReqData);
                IPsocket.Send(SendBuff);
            }

            catch (Exception e)
            {
                ProcessException.Process(new Exception("发送请求到服务器异常: " + e.Message));
            }



            // 使用Poll来判断完成，某些站点会出问题 

            while (true)
            {
                Byte[] RecvBuff = new byte[1024 * 20];
                try
                {
                    if (!IPsocket.Poll(15 * 1000 * 1000, SelectMode.SelectRead))
                    {
                        Console.WriteLine("HTTP超时，关闭连接");
                        break;
                    }
                }
                catch (Exception e)
                {
                    ProcessException.Process("Poll: " + e.Message);
                    break;
                }

                int Length = 0;
                try
                {
                    Length = IPsocket.Receive(RecvBuff);
                    if (0 == Length)
                    {
                        Console.WriteLine("服务端关闭");
                        break;
                    }
                    Console.WriteLine("从服务端收到字节", Length);
                }
                catch (Exception e)
                {
                    ProcessException.Process("Recv: " + e.Message);
                    break;
                }
                try
                {
                    Length = ClientSocket.Send(RecvBuff, Length, 0);
                    Console.WriteLine("发送字节到客户端", Length);
                }
                catch (Exception e)
                {
                    ProcessException.Process("Send: " + e.Message);
                }

            }



            /* 

            // 根据接收字节数来判断完成，某些站点会出问题 

            try 

            { 

            while (true) 

            { 

            Byte[] RecvBuff = new byte[1024 * 10]; 

            int Length = IPsocket.Receive(RecvBuff); 



            if (Length <= 0) 

            { 

            Console.WriteLine("从服务端接收数据完成"); 

            break; 

            } 

            Console.WriteLine("从服务端收到字节", Length); 



            Length = ClientSocket.Send(RecvBuff, Length, 0); 

            Console.WriteLine("发送字节到客户端", Length); 

            } 

            } 

            catch (Exception e) 

            { 

            Console.WriteLine(e.Message); 

            } 

            */

            try
            {
                ClientSocket.Shutdown(SocketShutdown.Both);
                ClientSocket.Close();
                IPsocket.Shutdown(SocketShutdown.Both);
                IPsocket.Close();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.Message); 
                ProcessException.Process(e);
            }

        }

        /// <summary> 
        /// 在客户端和服务器之间中转数据 
        /// </summary> 
        /// <param name="client">客户端socket</param> 
        /// <param name="server">服务端socket</param> 
        private void ForwardTcpData(Socket client, Socket server)
        {
            ArrayList ReadList = new ArrayList(2);

            while (true)
            {
                ReadList.Clear();
                ReadList.Add(client);
                ReadList.Add(server);

                try
                {
                    Socket.Select(ReadList, null, null, 1 * 1000 * 1000);
                }
                catch (SocketException e)
                {
                    ProcessException.Process("Select error: " + e.Message);
                    break;
                }

                // 超时 
                if (ReadList.Count == 0)
                {
                    //Console.WriteLine("Time out"); 
                    continue;
                }

                // 客户端可读 
                if (ReadList.Contains(client))
                {
                    byte[] Recv = new byte[1024 * 10];
                    int Length = 0;
                    try
                    {
                        Length = client.Receive(Recv, Recv.Length, 0);
                        if (Length == 0)
                        {
                            Console.WriteLine("Client is disconnect.");
                            break;
                        }
                        Console.WriteLine(" Recv bytes from client", Length);
                    }
                    catch (Exception e)
                    {
                        ProcessException.Process("Read from client error: " + e.Message);
                        break;
                    }

                    try
                    {
                        Length = server.Send(Recv, Length, 0);
                        Console.WriteLine(" Write bytes to server", Length);
                    }
                    catch (Exception e)
                    {
                        ProcessException.Process("Write data to server error: " + e.Message);
                        break;
                    }
                }

                // 真实服务端可读 
                if (ReadList.Contains(server))
                {
                    byte[] Recv = new byte[1024 * 10];
                    int Length = 0;

                    try
                    {
                        Length = server.Receive(Recv, Recv.Length, 0);
                        if (Length == 0)
                        {
                            Console.WriteLine("Server is disconnect");
                            break;
                        }
                        Console.WriteLine("Recv bytes from server", Length);
                    }
                    catch (Exception e)
                    {
                        ProcessException.Process("Read from server error: " + e.Message);
                        break;
                    }

                    try
                    {
                        Length = client.Send(Recv, Length, 0);
                        Console.WriteLine(" Write bytes to client", Length);
                    }
                    catch (Exception e)
                    {
                        ProcessException.Process("Write data to client error: " + e.Message);
                        break;
                    }
                }
            }

            try
            {
                client.Shutdown(SocketShutdown.Both);
                server.Shutdown(SocketShutdown.Both);
                client.Close();
                server.Close();
            }
            catch (Exception e)
            {
                ProcessException.Process(e.Message);
            }
            finally
            {
                Console.WriteLine("转发完毕");
            }
        } 
    }
}
