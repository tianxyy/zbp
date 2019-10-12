using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WindowsService.windowApi
{
   

    class ScreenCapture
    { 
        //win7 截不到QQ解决方案
        [DllImport("Dll1.dll")]
        public static extern IntPtr Capture();


      
       


        public static Bitmap GetScreenSnapshot()
        {
            if (isWin7())
            {
                try
                {
                    IntPtr handle = Capture();
                    try
                    {
                        return Image.FromHbitmap(handle);
                        //Marshal.FreeHGlobal(handle);                      
                    }
                    catch (Exception ex)
                    {
                        File.WriteAllText("b.log", ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    File.WriteAllText("c.log", ex.Message);
                }
            }
            else {
                return getScreen();
            }

            return null;



        }

        private static bool isWin7() {
            return Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1;

        }

        private static Bitmap getScreen() {

            return CaptureWindow(User32.GetDesktopWindow());
            //IntPtr dcHandle = User32.GetDC(User32.GetDesktopWindow());

            //IntPtr cdcHandle = User32.CreateCompatibleDC(dcHandle);
            

            //if (cdcHandle == IntPtr.Zero) return null;

            //if (dcHandle != cdcHandle)
            //{
            //    User32.DeleteObject(dcHandle);
            //}
            

            //IntPtr bitmapHandle = User32.CreateCompatibleBitmap(cdcHandle, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            //Bitmap image = Bitmap.FromHbitmap(bitmapHandle);

            //return image;

        }
        /// <summary>
        /// Creates an Image object containing a screen shot of a specific window
        /// </summary>
        /// <param name="handle">The handle to the window. (In windows forms, this is obtained by the Handle property)</param>
        /// <returns></returns>
        private static Bitmap CaptureWindow(IntPtr handle)
        {
            
            // get te hDC of the target window
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            // get the size
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;
            // create a device context we can copy to
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            // bitblt over
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
            // restore selection
            GDI32.SelectObject(hdcDest, hOld);
            // clean up
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            // get a .NET image object for it
            Bitmap img = Image.FromHbitmap(hBitmap);
            
            // free up the Bitmap object
            GDI32.DeleteObject(hBitmap);
            return img;
        }
        /// <summary>
        /// Captures a screen shot of a specific window, and saves it to a file
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="filename"></param>
        /// <param name="format"></param>
        public void CaptureWindowToFile(IntPtr handle, string filename, ImageFormat format)
        {
            Image img = CaptureWindow(handle);
            img.Save(filename, format);
        }
   

        /// <summary>
        /// Helper class containing Gdi32 API functions
        /// </summary>
        private class GDI32
        {

            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        /// <summary>
        /// Helper class containing User32 API functions
        /// </summary>
        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
            [DllImport("User32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

            [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)]
            public static extern IntPtr CreateCompatibleDC([In] IntPtr hdc);

            [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
            public static extern IntPtr CreateCompatibleBitmap([In] IntPtr hdc, int nWidth, int nHeight);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr GetDC(IntPtr hWnd);

            [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
            public static extern bool DeleteDC([In] IntPtr hdc);

            [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool DeleteObject([In] IntPtr hObject);

        }
    }
}
