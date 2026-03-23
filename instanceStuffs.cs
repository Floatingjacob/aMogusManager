/*
 
 This file contains most of the stuff relating to manipulting instances of Among Us.
 This file was created by FloatingJacob for use with aMogusManager.
 
*/

namespace aMogusManager
{
	public class instanceStuffs
	{
		public static string selectedVersion;
		static string plugin;
		static string zipMod;

		public static async Task<string> installVanilla(string instanceName, string gameVersion)
		{
			bool versionFound = false;
			JArray versions = JArray.Parse(File.ReadAllText("versions.json"));
			JArray instances = JArray.Parse(File.ReadAllText("mods.json"));

            foreach (JObject instance in instances) if (instance["name"].ToString().ToUpper() == instanceName.ToUpper()) return "alreadyExists"; // Prevents mulitple instances from having the same name
			while (!versionFound)
			{
				
				foreach (JObject version in versions)
				{
					if (gameVersion == version["version"].ToString())
					{
						versionFound = true;
						selectedVersion = gameVersion;
						await DownloadInstance(version["manifestID"].ToString(), false, selectedVersion, instanceName);

						JArray mods = JArray.Parse(File.ReadAllText("mods.json"));
						int highest = 0;
						int current = 0;
						foreach (JObject modID in mods)
						{
							int.TryParse(modID["modID"].ToString(), out current);
							if (current > highest) highest = current;
						}
						mods.Add(new JObject(
							new JProperty("name", instanceName),
							new JProperty("modID", highest + 1),
							new JProperty("installDir", $"./instances/{instanceName}")
						));
						File.WriteAllText("mods.json", mods.ToString());
					}
				}
			}
			return "gud";
		}

		public static async Task installPlugin(int modID, string zipMod)
		{
			// works kinda like installMod() but it doesn't create a new instance. instead, it just slaps some files onto an existing instance
			bool instanceFound = false;
			plugin = zipMod.Trim().Trim('"').Trim('\'');
			int pos = -1;

			while (!instanceFound)
			{
				pos++;
				JObject selectedMod = [];
				JArray mods = JArray.Parse(File.ReadAllText("mods.json"));

				if (mods[pos]["modID"].ToString() == modID.ToString())
				{
					instanceFound = true;
					selectedMod = (JObject)mods[pos];

					string dest = selectedMod["installDir"].ToString();
					instanceFound = true;
					string root = plugin;
					if (File.Exists(plugin))
					{
						string tmpf = "tmp";
						Directory.CreateDirectory(tmpf);
						ZipFile.ExtractToDirectory(plugin, tmpf, true);
						root = tmpf;
						while (true)
						{
							var dirs = Directory.GetDirectories(root);
							var files = Directory.GetFiles(root);
							if (files.Length == 0 && dirs.Length == 1)
								root = dirs[0];
							else break;
						}
					}
					else if (!Directory.Exists(plugin))
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("[ERROR] Invalid path.");
						Console.ForegroundColor = ConsoleColor.White;
						return;
					}
					while (true)
					{
						var dirs = Directory.GetDirectories(root);
						var files = Directory.GetFiles(root);
						if (files.Length == 0 && dirs.Length == 1)
							root = dirs[0];
						else break;
					}
					foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
					{
						string rel = file.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar);
						string target = Path.Combine(dest, rel);
						Directory.CreateDirectory(Path.GetDirectoryName(target));
						File.Copy(file, target, true);
					}
					if (Directory.Exists("tmp")) Directory.Delete("tmp", true);

					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("[Info] Plugin installed successfully.");
					Console.ForegroundColor = ConsoleColor.White;
					return;
				}
			}
		}

		public static async Task<string> installMod(string modFile, string instanceName, string gameVersion)
		{
			bool versionFound = false;
			JArray mods = JArray.Parse(File.ReadAllText("mods.json"));

            zipMod = modFile.Trim().Trim('"').Trim('\'');

            foreach (JObject instance in mods) if (instance["name"].ToString().ToUpper() == instanceName.ToUpper()) return "alreadyExists"; // Prevents mulitple instances from having the same name
            JArray versions = JArray.Parse(File.ReadAllText("versions.json"));
			while (!versionFound)
			{

				foreach (JObject version in versions)
				{
					if (gameVersion == version["version"].ToString())
					{
						selectedVersion = gameVersion;
						versionFound = true;
						await DownloadInstance(version["manifestID"].ToString(), true, selectedVersion, instanceName);
						break;
					}
				}
			}

			string instancePath = $"./instances/{instanceName}";
			string root = zipMod;

			if (File.Exists(zipMod))
			{
				string tmpf = $"./tmp_{instanceName}";
				Directory.CreateDirectory(tmpf);
				ZipFile.ExtractToDirectory(zipMod, tmpf, true);
				root = tmpf;
				while (true)
				{
					var dirs = Directory.GetDirectories(root);
					var files = Directory.GetFiles(root);
					if (files.Length == 0 && dirs.Length == 1)
						root = dirs[0];
					else break;
				}
			}
			else if (!Directory.Exists(zipMod))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("[ERROR] Invalid path.");
				Console.ForegroundColor = ConsoleColor.White;
				return "invalid mod path";
			}
			while (true)
			{
				var dirs = Directory.GetDirectories(root);
				var files = Directory.GetFiles(root);
				if (files.Length == 0 && dirs.Length == 1)
					root = dirs[0];
				else break;
			}
			foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
			{
				string rel = file.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar);
				string target = Path.Combine(instancePath, rel);
				Directory.CreateDirectory(Path.GetDirectoryName(target));
				File.Copy(file, target, true);
			}
			if (Directory.Exists($"./tmp_{instanceName}"))
				Directory.Delete($"./tmp_{instanceName}", true);
			JArray modsArr = JArray.Parse(File.ReadAllText("mods.json"));
			int highest = -1;
			int current = -1;
			foreach (JObject modID in modsArr)
			{
				int.TryParse(modID["modID"].ToString(), out current);
				if (current > highest) highest = current;
			}
			modsArr.Add(new JObject(
				new JProperty("name", instanceName),
				new JProperty("modID", highest + 1),
				new JProperty("installDir", instancePath)
			));
			File.WriteAllText("mods.json", modsArr.ToString());
			return "gud";
		}
		public static async Task DownloadInstance(string manifestID, bool modded, string selectedVersion, string instanceName)
		{
			string cacheDir = $"cache/{selectedVersion}";

			if (Directory.Exists(cacheDir))
			{

				foreach (var dirPath in Directory.GetDirectories(cacheDir, "*", SearchOption.AllDirectories))
				{
					string relativePath = Path.GetRelativePath(cacheDir, dirPath);
					string targetDir = Path.Combine($"instances/{instanceName}", relativePath);
					Directory.CreateDirectory(targetDir);
				}

				foreach (var filePath in Directory.GetFiles(cacheDir, "*", SearchOption.AllDirectories))
				{
					string relativePath = Path.GetRelativePath(cacheDir, filePath);
					string targetFile = Path.Combine($"instances/{instanceName}", relativePath);
					Directory.CreateDirectory(Path.GetDirectoryName(targetFile) ?? "");
					File.Copy(filePath, targetFile, true);
				}
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Blue;
				string input = null;
				while (string.IsNullOrWhiteSpace(input))
				{
					Console.Write("Enter your Steam username: "); // For some reason, DepotDownloader's built-in 'Enter Username' prompt doesn't show up.
					input = Console.ReadLine();
				} 
				DepotDownloader.Program.Main(["-app", "945360", "-depot", "945361", "-remember-password", "-manifest", manifestID, "-dir", cacheDir, "-user", input]).Wait();
				foreach (var filePath in Directory.GetFiles(cacheDir, "*", SearchOption.AllDirectories))
				{
					string relativePath = Path.GetRelativePath(cacheDir, filePath);
					string targetFile = Path.Combine($"instances/{instanceName}", relativePath);
					Directory.CreateDirectory(Path.GetDirectoryName(targetFile) ?? "");
					File.Copy(filePath, targetFile, true);
				}
				Console.ForegroundColor = ConsoleColor.White;
			}
		}
		public static async Task removeMod(int modID)
		{
			bool instanceFound = false;
			int pos = -1;
			JObject mogusMod = null;
			while (!instanceFound)
			{
				JArray mods = JArray.Parse(File.ReadAllText("mods.json"));

				pos++;
				if (mods[pos]["modID"].ToString() == modID.ToString())
				{
					instanceFound = true;
					mogusMod = (JObject)mods[pos];
					Directory.Delete(mogusMod["installDir"].ToString() ?? "", true);
					mogusMod.Remove();
					File.WriteAllText("mods.json", mods.ToString());
					Console.WriteLine("[Info] Mod uninstalled.");
					return;
				}
			}
		}
        public static double cacheSize() // I could not for the LIFE OF ME figure out how to do this, so I had AI tutor me on it lol
        {
            long totalBytes = 0;
			try
			{
				foreach (string file in Directory.EnumerateFiles("cache", "*", SearchOption.AllDirectories))
				{
					totalBytes += new FileInfo(file).Length;
				}
			}
			catch (DirectoryNotFoundException) { return 0.0; } // If there's no cache, say its size is 0 (because it is, dummy)
            return (totalBytes / 1024.0) / 1024.0 / 1024.0;
        }


    }

}