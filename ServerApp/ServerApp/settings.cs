using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace ServerApp
{
	public class Configuration
	{
		public string serverID { get; set; }

		public static Configuration LoadConfiguration()
		{
			string filePath = "settings.json";
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("Configuration file not found.");
			}

			using (var fs = File.OpenRead(filePath))
			{
				return JsonSerializer.Deserialize<Configuration>(fs);
			}
		}
	}
}
