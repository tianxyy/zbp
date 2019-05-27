using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MoniterMaster.service
{



    class Server
    {
        static UdpClient server;

        private static Socket sock;
        private static IPEndPoint iep1;


        public Server()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 5000;
            timer.Start();

            sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);
            iep1 =
            new IPEndPoint(IPAddress.Broadcast, 5125);

            string hostname = Dns.GetHostName();

            sock.SetSocketOption(SocketOptionLevel.Socket,
            SocketOptionName.Broadcast, 1);

        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            string data = "zbp";
            sock.SendTo(Encoding.Default.GetBytes(data), iep1);
        }

        public void morniter(Func<Image, int> callback, Func<string, int> callback2)
        {

            ThreadPool.QueueUserWorkItem(doMoniter, callback);

            IPEndPoint receivePoint = new IPEndPoint(IPAddress.Any, 0);
            server = new UdpClient(5124);
            callback2("checking alive....");
            while (true)
            {
               

                byte[] recData = server.Receive(ref receivePoint);
                if (recData != null && recData.Length > 0)
                {
                    // callback(recData);
                    try
                    {

                        string msg = Encoding.Default.GetString(recData);
                        callback2(msg + " ip:" + receivePoint.Address.ToString());
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        private void doMoniter(object obj)
        {

            TcpListener server = new TcpListener(IPAddress.Parse("0.0.0.0"), 8085);
            server.Start();
          
            while (true)
            {
                //if (!server.Pending())
                //{
                //    //为了避免每次都被tcpListener.AcceptTcpClient()阻塞线程，添加了此判断，
                //    //no connection requests have arrived。
                //    //当没有连接请求时，什么也不做，有了请求再执行到tcpListener.AcceptTcpClient()

                //}
                //else {
                TcpClient client = server.AcceptTcpClient();
                Dictionary<string, object> state = new Dictionary<string, object>();
                state.Add("client", client);
                state.Add("callback", obj);
                ThreadPool.QueueUserWorkItem(threadWorker, state);
                //}
            }
        }

        public void threadWorker(Object obj)
        {
            Dictionary<string, Object> state = obj as Dictionary<string, object>;
            object clientObject;
            state.TryGetValue("client", out clientObject);
            object funcObject;
            state.TryGetValue("callback", out funcObject);
            TcpClient client = clientObject as TcpClient;
            Func<Image, int> func = funcObject as Func<Image, int>;
            getData(client, getResult, func);
        }

        private void getData(TcpClient client, Func<Byte[], int> callback, Func<Image, int> callback2)
        {
            NetworkStream stream = client.GetStream();
            
            byte[] bytes = StreamToBytes(stream);
            try
            {
                callback(bytes);
            }
            catch (Exception ex)
            {
                
            }
            try
            {
                callback2(ImageHelper.BytesToImage(bytes));
            }
            catch (Exception ex)
            {
            }
        }
        public byte[] StreamToBytes(NetworkStream stream)

        {

            byte[] result;
            using (stream)
            {
                byte[] data = new byte[1024];
                using (MemoryStream ms = new MemoryStream())
                {

                    int numBytesRead;
                    while ((numBytesRead = stream.Read(data, 0, data.Length)) > 0)
                    {
                        ms.Write(data, 0, numBytesRead);

                    }
                    result = ms.ToArray();
                }
            }
            return result;

        }

        public int getResult(Byte[] datas)
        {

            String fileName = "pic/" + getTimeStamp();
            ImageHelper.CreateImageFromBytes(fileName, datas);
            return 0;
        }

        private long getTimeStamp()
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            DateTime nowTime = DateTime.Now;
            long unixTime = (long)System.Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
            return unixTime;
        }


        public System.Drawing.Image ReturnPhoto(byte[] streamByte)
        {
            return ImageHelper.BytesToImage(streamByte);
        }

        private void checkAlive(Byte[] data)
        {
            string result = Encoding.Default.GetString(data);
            Console.WriteLine(result);

        }

        public void sendData(String msg)
        {
            Byte[] sendBytes = Encoding.Default.GetBytes(msg);
            sendData(sendBytes);
        }

        public void sendData(Byte[] sendBytes)
        {
            try
            {
                server.Send(sendBytes, sendBytes.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
