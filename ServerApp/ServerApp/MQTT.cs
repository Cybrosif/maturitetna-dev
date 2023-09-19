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
		public static async Task Publish_Application_Message(string json, string topic)
		{

			var mqttFactory = new MqttFactory();

			using (var mqttClient = mqttFactory.CreateMqttClient())
			{
				var mqttClientOptions = new MqttClientOptionsBuilder()
					.WithTcpServer("91.121.93.94", 1883)
					.Build();

				await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

				var applicationMessage = new MqttApplicationMessageBuilder()
					.WithTopic(topic)
					.WithPayload(json)
					//.WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
					.Build();

				await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

				await mqttClient.DisconnectAsync();

				Console.WriteLine("MQTT application message is published.");
			}
		}
	}
}
