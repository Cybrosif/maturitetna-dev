using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Hsu.Daemon;
using SystemInfo;
using MQTT;

namespace ServerApp
{
	public class Program
	{
		private static Configuration configuration;

		public static async Task Main(string[] args)
		{
			configuration = Configuration.LoadConfiguration();

			await CreateHostBuilder(args).Build().RunAsync();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					// Add your services here

					// Add systemInfo, mqtt, and configuration as dependencies
					services.AddSingleton<SystemInfo.SystemInfo>();
					services.AddSingleton<MQTT.mqtt>();
					services.AddSingleton(configuration);

					// Configure and add the first Worker background service
					services.AddHostedService<Worker1>();

					// Configure and add the second Worker background service
					services.AddHostedService<Worker2>();
				});
	}

	public class Worker1 : BackgroundService
	{
		private readonly SystemInfo.SystemInfo systemInfo;
		private readonly MQTT.mqtt mqtt;
		private readonly string topic1;

		public Worker1(SystemInfo.SystemInfo systemInfo, MQTT.mqtt mqtt)
		{
			this.systemInfo = systemInfo;
			this.mqtt = mqtt;
			this.topic1 = "test/test123";
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				// Your background service logic here
				string jsonSystemInfo = systemInfo.GetSystemInformationJson();
				await mqtt.Publish_Application_Message(jsonSystemInfo, topic1);

				// Adjust the delay as needed
				await Task.Delay(1000, stoppingToken); // Publish the first message every second
			}
		}
	}

	public class Worker2 : BackgroundService
	{
		private readonly MQTT.mqtt mqtt;
		private readonly string topic2;
		private readonly Configuration configuration;

		public Worker2(MQTT.mqtt mqtt, Configuration configuration)
		{
			this.mqtt = mqtt;
			this.configuration = configuration;
			this.topic2 = "test/test123/onlinecheck";
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				// Your background service logic here
				string serverID = configuration.serverID;
				var test = new
				{
					serverID = serverID,
				};
				string json = JsonConvert.SerializeObject(test);
				await mqtt.Publish_Application_Message(json, topic2);

				// Adjust the delay as needed
				await Task.Delay(15000, stoppingToken); // Publish the second message every 15 seconds
			}
		}
	}
}
