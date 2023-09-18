using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemInfo
{
	public class SystemInfo
	{
		public double getCpuUsage
		{
			get
			{
				return GetCpuUsage();
			}
		}

		public (ulong totalMemory, ulong freeMemory, ulong usedMemory) getMemoryUsage
		{
			get
			{
				return GetMemoryUsage();
			}
		}

		public (string fileSystem, long totalSize, long availableFreeSpace) getDiskInfo
		{
			get
			{
				return GetDiskInfo("/");
			}
		}

		public TimeSpan getSystemUptime
		{
			get
			{
				return GetSystemUptime();
			}
		}
		private static double GetCpuUsage()
		{
			try
			{
				using (StreamReader reader = new StreamReader("/proc/stat"))
				{
					string line = reader.ReadLine();
					string[] fields = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

					// Calculate total CPU time
					ulong totalCpuTime = 0;
					for (int i = 1; i < fields.Length; i++)
					{
						totalCpuTime += ulong.Parse(fields[i]);
					}

					// Calculate idle time
					ulong idleTime = ulong.Parse(fields[4]);

					// Calculate CPU usage percentage
					double cpuUsage = 100.0 - ((double)idleTime / totalCpuTime * 100.0);
					return cpuUsage;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				return -1.0; // Error condition.
			}
		}

		private static (ulong totalMemory, ulong freeMemory, ulong usedMemory) GetMemoryUsage()
		{
			try
			{
				using (StreamReader reader = new StreamReader("/proc/meminfo"))
				{
					ulong totalMemory = 0;
					ulong freeMemory = 0;
					ulong usedMemory = 0;

					string line;
					while ((line = reader.ReadLine()) != null)
					{
						string[] parts = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
						if (parts.Length == 2)
						{
							string key = parts[0].Trim();
							string value = parts[1].Trim();

							if (key == "MemTotal")
								totalMemory = ParseMemoryValue(value);
							else if (key == "MemFree")
								freeMemory = ParseMemoryValue(value);
						}
					}

					usedMemory = totalMemory - freeMemory;
					return (totalMemory, freeMemory, usedMemory);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				return (0, 0, 0); // Error condition.
			}
		}
		private static ulong ParseMemoryValue(string value)
		{
			// The format of the value may contain spaces and "kB" at the end.
			// Remove spaces and "kB" and parse it as ulong.
			value = value.Replace(" ", "").Replace("kB", "");
			if (ulong.TryParse(value, out ulong result))
			{
				// Convert from kilobytes to bytes.
				return result * 1024;
			}
			return 0;
		}

		private static (string fileSystem, long totalSizeMB, long availableFreeSpaceMB) GetDiskInfo(string mountPoint)
		{
			try
			{
				Process process = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "df",
						Arguments = $"-m -P {mountPoint}", // Use -m to get sizes in megabytes
						RedirectStandardOutput = true,
						UseShellExecute = false,
						CreateNoWindow = true
					}
				};

				process.Start();
				string output = process.StandardOutput.ReadToEnd();
				process.WaitForExit();

				string[] lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
				if (lines.Length > 1)
				{
					string[] columns = lines[1].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					if (columns.Length >= 4)
					{
						string fileSystem = columns[0];
						long totalSizeMB = long.Parse(columns[1]);
						long availableFreeSpaceMB = long.Parse(columns[3]);

						return (fileSystem, totalSizeMB, availableFreeSpaceMB);
					}
				}

				return (null, 0, 0); // Error condition or invalid data.
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				return (null, 0, 0); // Error condition.
			}
		}

		private static TimeSpan GetSystemUptime()
		{
			try
			{
				using (StreamReader uptimeReader = new StreamReader("/proc/uptime"))
				{
					string uptimeLine = uptimeReader.ReadLine();
					string[] uptimeFields = uptimeLine.Split(' ');
					double uptimeSeconds = double.Parse(uptimeFields[0]);
					return TimeSpan.FromSeconds(uptimeSeconds);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}

			return TimeSpan.Zero; // Error condition or unable to retrieve uptime.
		}
	}
}
