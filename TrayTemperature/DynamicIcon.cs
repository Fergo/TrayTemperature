using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace TrayTemperature
{
    class DynamicIcon
    {
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = CharSet.Auto)]
        public extern static bool DestroyIcon(IntPtr handle);

        //Creates a 16x16 icon with 2 lines of  text
        public static Icon CreateIcon(string Line1Text, Color Line1Color, string Line2Text, Color Line2Color)
        {
            Font font = new Font("Consolas", 7);
            Bitmap bitmap = new Bitmap(16, 16);

            Graphics graph = Graphics.FromImage(bitmap);

            //Draw the temperatures
            graph.DrawString(Line1Text, font, new SolidBrush(Line1Color), new PointF(-1, -3));
            graph.DrawString(Line2Text, font, new SolidBrush(Line2Color), new PointF(-1, 7));

            IntPtr ico = bitmap.GetHicon();
            Icon bitmapIcon = Icon.FromHandle(ico);


            return bitmapIcon;
        }
    }
}
