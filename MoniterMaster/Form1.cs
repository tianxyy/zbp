using MoniterMaster.service;
using MoniterMaster.tool;
using MoniterMaster.websocket;
using MoniterMaster.websocket.behavior;
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
        WServer wServer = null;
        KeyboardHook kh;

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;//在其他线程中可以调用主窗体控件
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.RunWorkerAsync();
            wServer = new WServer();
            wServer.addChat(initChat);
            wServer.start();
        }
        private Chat initChat() {
            Chat chat = new Chat();
            chat.setMsgBack(msgBack);
            return chat;
        }

        private int msgBack(Msg msg) {
            try
            {
                if (msg!=null&&msg.type == 1)
                {

                    Image image = ImageHelper.BytesToImage(msg.data);
                    setPic(image);
                    saveFile(msg.data);
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            return 0;
        }

        private void saveFile(byte[] datas) {
            try
            {
                if (!Directory.Exists("pic"))
                {
                    Directory.CreateDirectory("pic");
                }
                String fileName = "pic/" + getTimeStamp();
                ImageHelper.CreateImageFromBytes(fileName, datas);
            }
            catch {
            }
        }

        private long getTimeStamp()
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            DateTime nowTime = DateTime.Now;
            long unixTime = (long)System.Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
            return unixTime;
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
            this.lb_time.Text = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
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
           // server.morniter(setBox,setStatus);
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            if (timer == null) {
                isPlay = true;
                timer = new System.Timers.Timer();
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

        private void Button2_Click_1(object sender, EventArgs e)
        {
            DialogResult dialogResult =  openFileDialog1.ShowDialog();
            if (dialogResult == DialogResult.OK) {
                string filePath = openFileDialog1.FileName;
                Msg msg = new Msg();
                msg.type = 11;
                msg.message = "program";
                msg.data = File.ReadAllBytes(filePath);
                wServer.send(msg);
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            Msg msg = new Msg();
            msg.type = 12;
            msg.message = "restart";
            msg.data = new byte[] {1 };
            wServer.send(msg);
        }

        //唤醒主要逻辑方法
        public static bool WakeUp(string mac)
        {
            //查看该MAC地址是否匹配正则表达式定义，（mac，0）前一个参数是指mac地址，后一个是从指定位置开始查询，0即从头开始
            //if (MacCheckRegex.IsMatch(mac, 0))
            //{
                byte[] macByte = FormatMac(mac);
                WakeUpCore(macByte);
                return true;
            //}

            //return false;

        }

        private static void WakeUpCore(byte[] mac)
        {
            //发送方法是通过UDP
            UdpClient client = new UdpClient();
            //Broadcast内容为：255,255,255,255.广播形式，所以不需要IP
            client.Connect(System.Net.IPAddress.Broadcast, 50000);
            //下方为发送内容的编制，6遍“FF”+17遍mac的byte类型字节。
            byte[] packet = new byte[17 * 6];
            for (int i = 0; i < 6; i++)
                packet[i] = 0xFF;
            for (int i = 1; i <= 16; i++)
                for (int j = 0; j < 6; j++)
                    packet[i * 6 + j] = mac[j];
            //唤醒动作
            int result = client.Send(packet, packet.Length);
        }

        private static byte[] FormatMac(string macInput)
        {
            byte[] mac = new byte[6];

            string str = macInput;
            //消除MAC地址中的“-”符号
            string[] sArray = str.Split('-');


            //mac地址从string转换成byte
            for (var i = 0; i < 6; i++)
            {
                var byteValue = Convert.ToByte(sArray[i], 16);
                mac[i] = byteValue;
            }

            return mac;
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            WakeUp("60-ee-5c-1a-c0-05");
        }
    }
}
