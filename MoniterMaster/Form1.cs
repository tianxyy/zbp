using MoniterMaster.service;
using MoniterMaster.tool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MoniterMaster
{
    public partial class Form1 : Form
    {
        Server server = new Server();
        private bool isPlay = false;
        System.Timers.Timer timer;
        private int index = 0;
        FileInfo[] files;


        KeyboardHook kh;

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;//在其他线程中可以调用主窗体控件
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.RunWorkerAsync();
                
        }

        



        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (index < files.Length)
            {
                var file = files[index];
                string timeStamp =  file.Name.Substring(0, file.Name.LastIndexOf("."));
                lb_time.Text = GetTime(timeStamp);
                setPic(Image.FromFile(file.FullName));
                index++;
            }
            else {
                isPlay = false;
                timer.Close();
                timer = null;
                button1.Text = "已停止";
            }

        }

        /// <summary>  
        /// 时间戳转为C#格式时间  
        /// </summary>  
        /// <param name="timeStamp">Unix时间戳格式</param>  
        /// <returns>C#格式时间</returns>  
        public  string  GetTime(string timeStamp)
        {
            DateTime converted = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime newDateTime = converted.AddSeconds(double.Parse(timeStamp)/1000);              
            string result = newDateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            return result;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy == false) {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        public int setBox(Image image) {
            if (isPlay == false) {
                setPic(image);
            }
           
            return 0;
        }

        private void setPic(Image image) {
            System.Drawing.Image thumbImage = image.GetThumbnailImage(pictureBox1.Width, pictureBox1.Height, null, System.IntPtr.Zero);
            this.pictureBox1.Image = thumbImage;
        }

        public int setStatus(string msg) {
            label2.Text = msg;
            return 0;
        }

        public System.Drawing.Image ReturnPhoto(byte[] streamByte)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream(streamByte);
            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
            return img;
        }

        private void Button2_Click(object sender, EventArgs e)
        {

        }

        private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            server.morniter(setBox,setStatus);
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            if (timer == null) {
                timer= new System.Timers.Timer();
                timer.Elapsed += Timer_Elapsed;
                int second = (int)numericUpDown1.Value;
                timer.Interval = second * 1000;
            }

            if (timer.Enabled)
            {
                isPlay = false;
                timer.Close();
                button1.Text = "播放";
            }
            else {
                button1.Text = "停止";
                DirectoryInfo di = new DirectoryInfo("pic");
                files = di.GetFiles();
                isPlay = true;
                timer.Start();
            }
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
       
            kh.UnHook();

        }

        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();                                //窗体显示
            this.WindowState = FormWindowState.Normal;  //窗体状态默认大小
            this.Activate();

        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
            this.notifyIcon1.Visible = false;
        }

        private void Form1_MaximumSizeChanged(object sender, EventArgs e)
        {
           
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            kh = new KeyboardHook();
            kh.SetHook();
            kh.OnKeyDownEvent += kh_OnKeyDownEvent;
        }

        void kh_OnKeyDownEvent(object sender, KeyEventArgs e)

        {

            if (e.KeyData == (Keys.D9 | Keys.Shift|Keys.Alt)) {
                if (this.Visible)
                {
                    this.Hide();
                }
                else {
                    this.Show();
                }
                
            }

        }

    }
}
