using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using WindowsFormsApp1.core;


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
            //Application.Run(new Form1());

            App app = new App();
            while (!app.isDone)
            {
                Application.DoEvents();
                Thread.Sleep(50000);
            }

        }

    }
}
