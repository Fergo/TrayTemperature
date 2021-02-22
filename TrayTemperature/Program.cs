using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;

namespace HardwareMonitor {
	static class Program {

		static Computer computer = new Computer() { CPUEnabled = true, GPUEnabled = true };
		static int CPU = 0, GPU = 0;
		static bool isCelsius = true;
		static NotifyIcon ni;
		static ContextMenu contextMenu;

		[STAThread]
		static void Main() {
			//Application.EnableVisualStyles();
			//Application.SetCompatibleTextRenderingDefault(false);

			//Inititalize  OpenHardwareMonitorLib
			computer.Open();

			//Setup timer
			Timer tmr = new Timer {
				Interval = 1000,
				Enabled = true
			};

			tmr.Tick += tmr_tick;

			//Setup context menu
			contextMenu = new ContextMenu();

			contextMenu.MenuItems.AddRange(new MenuItem[] {
				new MenuItem {
					Text = "TrayTemperature",
					Enabled = false
				},
				new MenuItem {
					Text = "www.fergonez.net",
					Enabled = false
				},
				new MenuItem {
					Text = "-",
				},
				new MenuItem {
					Name = "menCel",
					Text = "Celsius",
					Checked = true,
				},
				new MenuItem {
					Name = "menFah",
					Text = "Fahrenheit"
				},
				new MenuItem {
					Text = "-",
				},
				new MenuItem {
					Name = "menExit",
					Text = "Exit"
				},
			});

			foreach(MenuItem menuItem in contextMenu.MenuItems)
				menuItem.Click += menu_Click;

			//Setup tray icon
			ni = new NotifyIcon {
				Visible = true,
				ContextMenu = contextMenu
			};

			//Enforce first tick as soon as possible
			tmr_tick(null, null);

			Application.Run();

			ni.Visible = false;
		}

		//Handles context menu items click
		private static void menu_Click(object sender, EventArgs e) {
			switch (((MenuItem)sender).Name) {
				case "menCel":
					isCelsius = true;
					contextMenu.MenuItems.Find("menCel", false).First().Checked = true;
					contextMenu.MenuItems.Find("menFah", false).First().Checked = false;
					break;
				case "menFah":
					isCelsius = false;
					contextMenu.MenuItems.Find("menCel", false).First().Checked = false;
					contextMenu.MenuItems.Find("menFah", false).First().Checked = true;
					break;
				case "menExit":
					Application.Exit();
					break;
			}
		}

		//Updates the temperatures
		private static void tmr_tick(object sender, EventArgs e) {
			foreach (IHardware hardware in computer.Hardware) {
				hardware.Update();

				//Get all temperature censors
				ISensor sensor = hardware.Sensors.FirstOrDefault(d => d.SensorType == SensorType.Temperature);

				if (sensor != null) {
					//Format as C/F and assigns to the correct variable
					int formattedValue = Convert.ToInt32(isCelsius ? sensor.Value : sensor.Value * 1.8 + 32);

					if (hardware.HardwareType == HardwareType.CPU)
						CPU = formattedValue;
					else
						GPU = formattedValue;
				}
			}

			string tempUnit = isCelsius ? "°C" : "°F";

			ni.Text = $"CPU Temperature: {CPU}{tempUnit}\r\nGPU Temperature: {GPU}{tempUnit}";
			ni.Icon = DynamicIcon.CreateIcon(CPU.ToString() + tempUnit, GPU.ToString() + tempUnit);
		}
	}

}
