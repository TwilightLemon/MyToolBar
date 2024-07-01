using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyToolBar.Common.WinApi
{
    public static class ScreenAPI
    {
        public static Bitmap CaptureScreenArea(int x , int y, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            using Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(x, y, 0, 0, new Size(width, height));
            return bmp;
        }
    }
}
