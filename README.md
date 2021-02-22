# Fergo TrayTemperature

A very simple CPU and GPU temperature monitor for the system tray.

![TrayTemperature](https://i.imgur.com/pwWbuCm.jpg)

Tons of other softwares already provide this functionality, but I wanted something that specifically only displayed the temperatures in the system tray without any additional user interface or other overheads.

# Features

* Minimalistic text display, matching default Windows 10 tray icon style
* Real-time CPU and GPU temperatures
* Customizable text colors for each temperature range (low, mid high)
* Customizable refresh interval 
* Basic logging functionality
* Celsius and Fahrenheit supported
* Tooltip with statistics (average, minimum, maximum)

# Usage

Just run  `TrayTemperature.exe` and a tray icon will be added displaying the CPU temperature (top) and GPU temperature (bottom). 

The application requires elevated priviledges in order to properly acquire the sensor data.

.NET Framework 4.6.1 is required.

# Customization

To customize the temperature ranges and their different colors, edit the following section in the `TrayTemperature.exe.config` file:

```xml
<setting name="CPUMed" serializeAs="String">
	<value>#ffff00</value>
</setting>
<setting name="CPULow" serializeAs="String">
	<value>#ffffff</value>
</setting>
<setting name="CPUHigh" serializeAs="String">
	<value>#ff0000</value>
</setting>
<setting name="CPUTempMed" serializeAs="String">
	<value>65</value>
</setting>
<setting name="CPUTempHigh" serializeAs="String">
	<value>80</value>
</setting>
<setting name="GPULow" serializeAs="String">
	<value>#ffffff</value>
</setting>
<setting name="GPUMed" serializeAs="String">
	<value>#ffff00</value>
</setting>
<setting name="GPUHigh" serializeAs="String">
	<value>#ff0000</value>
</setting>
<setting name="GPUTempMed" serializeAs="String">
	<value>60</value>
</setting>
<setting name="GPUTempHigh" serializeAs="String">
	<value>85</value>
</setting>
```

# Download

Check the **Releases** page: 

https://github.com/Fergo/TrayTemperature/releases

# Source code requirements 

This software makes use of OpenHardwareMonitorLib.

https://github.com/openhardwaremonitor/openhardwaremonitor

The pre-compiled dll is already available with the release of TrayTemperature.

