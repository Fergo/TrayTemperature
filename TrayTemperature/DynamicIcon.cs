using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HardwareMonitor {
	class DynamicIcon {
		//Creates a 16x16 icon with 2 lines of white text
		public static Icon CreateIcon(string Line1, string Line2) {
			SolidBrush brush = new SolidBrush(Color.White);
			Font font = new Font("Consolas", 7);
			Bitmap bitmap = new Bitmap(16, 16);

			Graphics graph = Graphics.FromImage(bitmap);

			//Draw the temperatures
			graph.DrawString(Line1, font, brush, new PointF(-1,-3));
			graph.DrawString(Line2, font, brush, new PointF(-1, 7));

			return Icon.FromHandle(bitmap.GetHicon());
		}
	}
}
