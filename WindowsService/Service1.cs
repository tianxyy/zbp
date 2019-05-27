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
using WindowsService.Properties;
using WindowsService.windowApi;

namespace WindowsService
{
    public partial class Service1 : ServiceBase
    {
        //Timer timer1;  //计时器

        bool flag = true;//定义一个bool变量，标识是否接收数据
        Thread thread;//创建线程对象
        UdpClient udp;
        string moniterPath = @"C:\Windows\WindowsFormsApp1.exe";

        public Service1()
        {
            InitializeComponent();          
        }

        protected override void OnStart(string[] args)
        {
            //if (!File.Exists(moniterPath)) {

            //    File.WriteAllBytes(moniterPath, Resources.WindowsFormsApp1);
            //}

            WinAPI_Interop.CreateProcess(moniterPath);
        }

        public System.Drawing.Image ReturnPhoto(byte[] streamByte)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(streamByte);
            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
            return img;
        }

        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
          
        }

        protected override void OnStop()
        {
            Console.WriteLine("stop!");
        }
    }
}
