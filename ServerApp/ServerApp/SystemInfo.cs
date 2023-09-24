using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace SystemInfo
{
	public class SystemInfo
	{
		public string GetSystemInformationJson()
		{
			var systemInformation = GetSystemInformation();
			return systemInformation.ToJson();
		}

		private SystemInformation GetSystemInformation()
		{
			var cpuUsage = GetCpuUsage();
			var memoryUsage = GetMemoryUsage();
			var diskInfo = GetDiskInfo("/");
			var systemUptime = GetSystemUptime();

			return new SystemInformation
			{
				CpuUsage = cpuUsage,
				MemoryUsage = memoryUsage,
				DiskInfo = diskInfo,
				SystemUptime = systemUptime
			};
		}

		private double GetCpuUsage()
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

		private MemoryUsage GetMemoryUsage()
		{
			try
			{
				using (StreamReader reader = new StreamReader("/proc/meminfo"))
				{
					ulong totalMemoryKB = 0;
					ulong freeMemoryKB = 0;

					string line;
					while ((line = reader.ReadLine()) != null)
					{
						string[] parts = line.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
						if (parts.Length == 2)
						{
							string key = parts[0].Trim();
							string value = parts[1].Trim();

							if (key == "MemTotal")
								totalMemoryKB = ParseMemoryValue(value);
							else if (key == "MemFree")
								freeMemoryKB = ParseMemoryValue(value);
						}
					}

					ulong totalMemoryMB = totalMemoryKB / 1024;
					ulong freeMemoryMB = freeMemoryKB / 1024;
					ulong usedMemoryMB = totalMemoryMB - freeMemoryMB;

					return new MemoryUsage
					{
						TotalMemory = totalMemoryMB,
						FreeMemory = freeMemoryMB,
						UsedMemory = usedMemoryMB
					};
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				return new MemoryUsage(); // Error condition.
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
				return result;
			}
			return 0;
		}

		private DiskInfo GetDiskInfo(string mountPoint)
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

						return new DiskInfo
						{
							FileSystem = fileSystem,
							TotalSizeMB = totalSizeMB,
							AvailableFreeSpaceMB = availableFreeSpaceMB
						};
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				return new DiskInfo(); // Error condition.
			}
			return new DiskInfo();
		}

		private TimeSpan GetSystemUptime()
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
				return TimeSpan.Zero; // Error condition.
			}
		}
	}

	public class SystemInformation
	{
		public double CpuUsage { get; set; }
		public MemoryUsage MemoryUsage { get; set; }
		public DiskInfo DiskInfo { get; set; }
		public TimeSpan SystemUptime { get; set; }

		public string ToJson()
		{
			var jsonResult = new
			{
				CpuUsage = CpuUsage,
				TotalMemory = MemoryUsage.TotalMemory,
				FreeMemory = MemoryUsage.FreeMemory,
				UsedMemory = MemoryUsage.UsedMemory,
				FileSystem = DiskInfo.FileSystem,
				TotalSizeMB = DiskInfo.TotalSizeMB,
				AvailableFreeSpaceMB = DiskInfo.AvailableFreeSpaceMB,
				SystemUptime = SystemUptime
			};
			return JsonConvert.SerializeObject(jsonResult);
		}
	}

	public class MemoryUsage
	{
		public ulong TotalMemory { get; set; }
		public ulong FreeMemory { get; set; }
		public ulong UsedMemory { get; set; }
	}

	public class DiskInfo
	{
		public string FileSystem { get; set; }
		public long TotalSizeMB { get; set; }
		public long AvailableFreeSpaceMB { get; set; }
	}
}
