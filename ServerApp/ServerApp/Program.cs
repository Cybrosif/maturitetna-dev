using System;
using System.Timers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SystemInfo;
using MQTT;
using MQTTnet.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Hsu.Daemon;

namespace ServerApp
{
	public class Program
	{
		private static SystemInfo.SystemInfo systemInfo;
		private static MQTT.mqtt mqtt;
		private static string topic1 = "test/test123";
		private static string topic2 = "test/test123/onlinecheck";
		private static Configuration configuration;

		public static async Task Main(string[] args)
		{
			configuration = Configuration.LoadConfiguration();

			systemInfo = new SystemInfo.SystemInfo();
			mqtt = new MQTT.mqtt();

			// Create and configure System.Timers.Timer (every second)
			System.Timers.Timer timer1 = new System.Timers.Timer(1000);
			timer1.Elapsed += Timer1Elapsed;
			timer1.Start();

			// Create and configure System.Timers.Timer (every 30 seconds)
			System.Timers.Timer timer2 = new System.Timers.Timer(15000);
			timer2.Elapsed += Timer2Elapsed;
			timer2.Start();

			await CreateHostBuilder(args).Build().RunAsync();
		}

		private static async void Timer1Elapsed(object sender, ElapsedEventArgs e)
		{
			string jsonSystemInfo = systemInfo.GetSystemInformationJson();
			await mqtt.Publish_Application_Message(jsonSystemInfo, topic1);
		}

		private static async void Timer2Elapsed(object sender, ElapsedEventArgs e)
		{
			string serverID = configuration.serverID;
			var test = new
			{
				serverID = serverID,
			};
			string json = JsonConvert.SerializeObject(test);
			await mqtt.Publish_Application_Message(json, topic2);
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					services.AddHostedService<Worker>();
				});
	}

	public class Worker : BackgroundService
	{
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			// Add any long-running background tasks here.
			while (!stoppingToken.IsCancellationRequested)
			{
				// Your background service logic here

				await Task.Delay(1000, stoppingToken); // Adjust the delay as needed
			}
		}
	}
}
