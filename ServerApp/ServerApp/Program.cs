using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SystemInfo;
using MQTT;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Newtonsoft.Json;


namespace ServerApp
{
	public class Program
	{
		static void Main(string[] args)
		{
			SystemInfo.SystemInfo systemInfo = new SystemInfo.SystemInfo();
			MQTT.mqtt mqtt = new MQTT.mqtt();
			

			while (true)
			{
				
				double cpuUsage = systemInfo.getCpuUsage;
				(ulong totalMemory, ulong freeMemory, ulong usedMemory) = systemInfo.getMemoryUsage;
				(string fileSystem, long totalSizeMB, long availableFreeSpaceMB) = systemInfo.getDiskInfo;
				TimeSpan uptime = systemInfo.getSystemUptime;
				var systemInfoData = new
				{
					CpuUsage = cpuUsage,
					TotalMemoryMB = totalMemory / (1024 * 1024),
					FreeMemoryMB = freeMemory / (1024 * 1024),
					UsedMemoryMB = usedMemory / (1024 * 1024),
					FileSystem = fileSystem,
					TotalSizeMB = totalSizeMB,
					AvailableFreeSpaceMB = availableFreeSpaceMB,
					Uptime = uptime
				};
				string json = JsonConvert.SerializeObject(systemInfoData);

				/*Console.WriteLine($"Overall CPU Usage: {cpuUsage:F2}%");

				Console.WriteLine($"Total Memory: {totalMemory / (1024 * 1024):N2} MB");
				Console.WriteLine($"Free Memory: {freeMemory / (1024 * 1024):N2} MB");
				Console.WriteLine($"Used Memory: {usedMemory / (1024 * 1024):N2} MB");
				Console.WriteLine($"File System: {fileSystem}");
				Console.WriteLine($"Total Size: {totalSizeMB} MB");
				Console.WriteLine($"Available Free Space: {availableFreeSpaceMB} MB");

				Console.WriteLine($"Upitme: {uptime}");*/

				mqtt.Publish_Application_Message(json);

				System.Threading.Thread.Sleep(1000); 
			}
		}
	}
}

