using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using WindowsFormsApp1.core;
using WindowsService.windowApi;

namespace WindowsFormsApp1
{
    static class Program
    {
        static List<ManualResetEvent> manualEvents = new List<ManualResetEvent>();

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //bool flag = false;
            //System.Threading.Mutex mutex = new System.Threading.Mutex(true, "Moniter", out flag);
            //if (flag)
            //{

            //}
            //else {
                App app = new App();
                while (!app.isDone)
                {
                    Application.DoEvents();
                    Thread.Sleep(50000);
                }
            //}
         
        }
    }
}
