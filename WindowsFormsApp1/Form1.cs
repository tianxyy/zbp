using System;
using System.Drawing;
using System.Windows.Forms;
using WindowsService.windowApi;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Image image = ScreenCapture.GetScreenSnapshot();
            if (image != null) {
                image.Save("test.jpg");
            }
        }

    }
}
