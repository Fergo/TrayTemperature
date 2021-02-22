using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

using OpenHardwareMonitor.Hardware;
using System.Text;
using System.Reflection;

namespace TrayTemperature {
	static class Program {

		static Computer computer = new Computer() { CPUEnabled = true, GPUEnabled = true };
		static int CPU = 0, GPU = 0, CPUMax = 0, GPUMax = 0, CPUMin = 99999, GPUMin = 99999;
		static ulong CPUAcc = 0, GPUAcc = 0, regCount = 0;
		static bool isLogging = false;
		static Timer tmr;
		static NotifyIcon ni;
		static ContextMenu contextMenu;
		static StreamWriter sw;



		[STAThread]
		static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			Properties.Settings.Default.Upgrade();

			//Inititalize OpenHardwareMonitorLib
			computer.Open();

			//Setup timer
			tmr = new Timer {
				Interval = Properties.Settings.Default.Refresh * 1000,
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
					Name = "menRefresh",
					Text = "Refresh",
				},
				new MenuItem {
					Name = "menLog",
					Text = "Log"
				},
				new MenuItem {
					Name = "menReset",
					Text = "Reset statistics"
				},
				new MenuItem {
					Text = "-",
				},
				new MenuItem {
					Name = "menExit",
					Text = "Exit"
				},
			});

			MenuItem refreshMenu = contextMenu.MenuItems.Find("menRefresh", false).First();
			refreshMenu.MenuItems.AddRange(new MenuItem[] {
				new MenuItem { Name = "1", Text = "1s" },
				new MenuItem { Name = "2", Text = "2s"},
				new MenuItem { Name = "5", Text = "5s" },
				new MenuItem { Name = "10", Text = "10s" },
				new MenuItem { Name = "15", Text = "15s" },
				new MenuItem { Name = "30", Text = "30s" },
				new MenuItem { Name = "60", Text = "60s" }
			});

			refreshMenu.MenuItems.Find(Properties.Settings.Default.Refresh.ToString(), false).First().Checked = true;

			//Add event listeners to the menus
			foreach (MenuItem menuItem in contextMenu.MenuItems)
				menuItem.Click += menu_Click;

			foreach (MenuItem menuItem in refreshMenu.MenuItems)
				menuItem.Click += menuRefresh_Click; ;


			if (Properties.Settings.Default.Celsius) {
				contextMenu.MenuItems.Find("menCel", false).First().Checked = true;
				contextMenu.MenuItems.Find("menFah", false).First().Checked = false;
			} else {
				contextMenu.MenuItems.Find("menCel", false).First().Checked = false;
				contextMenu.MenuItems.Find("menFah", false).First().Checked = true;
			}


			//Setup tray icon
			ni = new NotifyIcon {
				Visible = true,
				ContextMenu = contextMenu
			};

			//Enforce first tick as soon as possible
			tmr_tick(null, null);

			Application.Run();

			//Save settings when exiting
			Properties.Settings.Default.Save();

			ni.Visible = false;
		}

		private static void menuRefresh_Click(object sender, EventArgs e) {
			MenuItem clicked = (MenuItem)sender;

			//Uncheck all other intervals
			foreach (MenuItem item in clicked.Parent.MenuItems)
				item.Checked = false;

			//Check current
			clicked.Checked = true;

			Properties.Settings.Default.Refresh = Convert.ToInt32(clicked.Name);
			tmr.Interval = Properties.Settings.Default.Refresh * 1000;
			
		}

		//Handles context menu items click
		private static void menu_Click(object sender, EventArgs e) {
			switch (((MenuItem)sender).Name) {
				case "menCel":
					Properties.Settings.Default.Celsius = true;

					//Swap checked mark
					contextMenu.MenuItems.Find("menCel", false).First().Checked = true;
					contextMenu.MenuItems.Find("menFah", false).First().Checked = false;
					break;
				case "menFah":
					Properties.Settings.Default.Celsius = false;

					//Swap checked mark
					contextMenu.MenuItems.Find("menCel", false).First().Checked = false;
					contextMenu.MenuItems.Find("menFah", false).First().Checked = true;
					break;
				case "menExit":
					Application.Exit();
					break;
				case "menReset":
					CPUMax = 0; GPUMax = 0; CPUAcc = 0; GPUAcc = 0; regCount = 0; CPUMin = 99999; GPUMin = 99999;
					break;
				case "menLog":
					if (!isLogging) {
						if (MessageBox.Show("Starting a log will reset the current average, minimum and maximum temperatures. Proceed?", "Log start", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
							return;

						sw = new StreamWriter(string.Format("temp.log", DateTime.Now), false);
						sw.WriteLine("DateTime,CPU Temperature,GPU Temperature");

						CPUMax = 0; GPUMax = 0;	CPUAcc = 0; GPUAcc = 0; regCount = 0; CPUMin = 99999; GPUMin = 99999;

						contextMenu.MenuItems.Find("menLog", false).First().Checked = true;
						isLogging = true;

						contextMenu.MenuItems.Find("menCel", false).First().Enabled = false;
						contextMenu.MenuItems.Find("menFah", false).First().Enabled = false;
					} else {
						sw.Close();
						sw = null;

						//Create the summary table
						StringBuilder sb = new StringBuilder();
						sb.AppendLine("Hardware,Average,Minimum,Maximum");
						sb.AppendLine(string.Format("CPU,{0:F2},{1},{2}", (float)CPUAcc / regCount, CPUMin, CPUMax));
						sb.AppendLine(string.Format("GPU,{0:F2},{1},{2}", (float)GPUAcc / regCount, GPUMin, GPUMax));
						sb.AppendLine("");

						//Append the summary table with the temp timeseries
						string fileName = string.Format("{0:yyyy-MM-dd_hh-mm-ss}.log", DateTime.Now);
						File.WriteAllText(fileName, sb.ToString() + File.ReadAllText("temp.log"));
						File.Delete("temp.log");
						MessageBox.Show("Log saved to:\r\n\r\n" + Path.Combine(Application.ExecutablePath, fileName), "Log saved", MessageBoxButtons.OK, MessageBoxIcon.Information);

						contextMenu.MenuItems.Find("menLog", false).First().Checked = false;
						isLogging = false;

						contextMenu.MenuItems.Find("menCel", false).First().Enabled = true;
						contextMenu.MenuItems.Find("menFah", false).First().Enabled = true;
					}
					break;
			}

			tmr_tick(null, null);
		}

		//Updates the temperatures
		private static void tmr_tick(object sender, EventArgs e) {
			foreach (IHardware hardware in computer.Hardware) {
				hardware.Update();

				//Get all temperature censors
				ISensor sensor = hardware.Sensors.FirstOrDefault(d => d.SensorType == SensorType.Temperature);

				if (sensor != null) {
					if (hardware.HardwareType == HardwareType.CPU)
						CPU = Convert.ToInt32(sensor.Value);
					else
						GPU = Convert.ToInt32(sensor.Value);
				}
			}

			//Select appropriate color based on settings
			Color cpuColor, gpuColor;

			if (CPU >= Properties.Settings.Default.CPUTempHigh)
				cpuColor = ColorTranslator.FromHtml(Properties.Settings.Default.CPUHigh);
			else if (CPU >= Properties.Settings.Default.CPUTempMed)
				cpuColor = ColorTranslator.FromHtml(Properties.Settings.Default.CPUMed);
			else
				cpuColor = ColorTranslator.FromHtml(Properties.Settings.Default.CPULow);

			if (GPU >= Properties.Settings.Default.GPUTempHigh)
				gpuColor = ColorTranslator.FromHtml(Properties.Settings.Default.GPUHigh);
			else if (GPU >= Properties.Settings.Default.GPUTempMed)
				gpuColor = ColorTranslator.FromHtml(Properties.Settings.Default.GPUMed);
			else
				gpuColor = ColorTranslator.FromHtml(Properties.Settings.Default.GPULow);

			//Unit conversion for loggin and displaying
			int convertedCPU = Convert.ToInt32(Properties.Settings.Default.Celsius ? CPU : CPU * 1.8 + 32);
			int convertedGPU = Convert.ToInt32(Properties.Settings.Default.Celsius ? GPU : GPU * 1.8 + 32);
			string tempUnit = Properties.Settings.Default.Celsius ? "°C" : "°F";

			CPUAcc += (ulong)convertedCPU;
			GPUAcc += (ulong)convertedGPU;
			regCount++;

			if (CPU > CPUMax)
				CPUMax = CPU;

			if (CPU < CPUMin)
				CPUMin = CPU;

			if (GPU > GPUMax)
				GPUMax = GPU;

			if (GPU < GPUMin)
				GPUMin = GPU;

			//Appends a new line to the current log file (CSV format)
			if (isLogging && sw != null) {
				sw.WriteLine(DateTime.Now.ToString() + "," + convertedCPU + "," + convertedGPU);
			}

			//Updates the icon and tooltip
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("CPU");
			sb.AppendLine($"  Cur: {convertedCPU}{tempUnit}");
			sb.AppendLine($"  Avg: {(float)CPUAcc / regCount:F2}{tempUnit}");
			sb.AppendLine($"  Min: {CPUMin}{tempUnit}");
			sb.AppendLine($"  Max: {CPUMax}{tempUnit}");
			sb.AppendLine("GPU");
			sb.AppendLine($"  Cur: {convertedGPU}{tempUnit}");
			sb.AppendLine($"  Avg: {(float)GPUAcc / regCount:F2}{tempUnit}");
			sb.AppendLine($"  Min: {GPUMin}{tempUnit}");
			sb.AppendLine($"  Max: {GPUMax}{tempUnit}");

			Fixes.SetNotifyIconText(ni, sb.ToString());

			ni.Icon = DynamicIcon.CreateIcon(convertedCPU.ToString() + tempUnit, cpuColor, convertedGPU.ToString() + tempUnit, gpuColor);
		}
	}

	//Little hack to bypass the 63 char limit of the tooltip (still limited to the 128 chars of the original win32)
	public class Fixes {
		public static void SetNotifyIconText(NotifyIcon ni, string text) {
			if (text.Length >= 128) throw new ArgumentOutOfRangeException("Text limited to 127 characters");
			Type t = typeof(NotifyIcon);
			BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;
			t.GetField("text", hidden).SetValue(ni, text);
			if ((bool)t.GetField("added", hidden).GetValue(ni))
				t.GetMethod("UpdateIcon", hidden).Invoke(ni, new object[] { true });
		}
	}

}
