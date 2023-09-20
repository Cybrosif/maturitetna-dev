using System;
using System.Timers;
using System.Threading;
using Newtonsoft.Json;
using SystemInfo;
using MQTT;
using MQTTnet.Client;

namespace ServerApp
{
    public class Program
    {
        private static SystemInfo.SystemInfo systemInfo;
        private static MQTT.mqtt mqtt;
        private static string topic1 = "test/test123";
		private static string topic2 = "test/test123/onlinecheck";

		public static async Task Main(string[] args)
        {
            systemInfo = new SystemInfo.SystemInfo();
            mqtt = new MQTT.mqtt();

            // Create and configure System.Timers.Timer (every second)
            System.Timers.Timer timer1 = new System.Timers.Timer(1000);
            timer1.Elapsed += Timer1Elapsed;
            timer1.Start();

            // Create and configure System.Timers.Timer (every 30 seconds)
            System.Timers.Timer timer2 = new System.Timers.Timer(30000);
            timer2.Elapsed += Timer2Elapsed;
            timer2.Start();

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();

            // Dispose of resources, if necessary
            timer1.Dispose();
            timer2.Dispose();
        }

        private static async void Timer1Elapsed(object sender, ElapsedEventArgs e)
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

            await mqtt.Publish_Application_Message(json, topic1);
        }

		private static async void Timer2Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("");
			Console.WriteLine("test");
			Console.WriteLine("");
			var test = new
            {
                Test = "test",
            };
            string json = JsonConvert.SerializeObject(test);
			await mqtt.Publish_Application_Message(json, topic2);
		}
    }
}
