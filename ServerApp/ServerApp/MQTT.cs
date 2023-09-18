using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace MQTT
{
	public class mqtt
	{
		public static async Task Publish_Application_Message()
		{

			var mqttFactory = new MqttFactory();

			using (var mqttClient = mqttFactory.CreateMqttClient())
			{
				var mqttClientOptions = new MqttClientOptionsBuilder()
					.WithTcpServer("test.mosquitto.org", 1883)
					.Build();

				await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

				var applicationMessage = new MqttApplicationMessageBuilder()
					.WithTopic("test/test123")
					.WithPayload("testtest")
					.Build();

				await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

				await mqttClient.DisconnectAsync();

				Console.WriteLine("MQTT application message is published.");
			}
		}
	}
}
