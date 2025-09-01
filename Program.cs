/*
 
 Welcome to my super-fragile, idiot-succeptible, OCD-inducing Among Us mod manager.

 I slightly modified DepotDownloader so i could use it here as a library instead of a platform specific program
 (pretty much, i just made everything in it's Program.cs file public)
 i know that this code probably gave someone a stroke when they saw it, so if you don't want others to be hospitalized, 
 consider improving it and making a pull request. 
 On second thought, if you want to hospitalize as many people as possible (cus you're a psycho),
 consider spreading around my github profile. (https://github.com/floatingjacob/)

 */
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Compression;
#pragma warning disable CS8602
#pragma warning disable CS8618
class Program
{
    static bool interaction = false;
    static string zipmod;
    static string moguspath;
    static string plugin;
    static string selectedversion;
    static async Task Main()
    {
        interaction = true;
        while (true)
        {

            Console.CursorVisible = true;
            Console.Clear();
            // Determines your OS and changes a few settings
            if (OperatingSystem.IsLinux())
            {
                if (!File.Exists("gamefolder.txt")) File.WriteAllText($"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/Steam/steamapps/common/Among Us")}", "gamefolder.txt");
                moguspath = File.ReadAllText("gamefolder.txt");
                LinuxPrefix();
            }
            else if (OperatingSystem.IsWindows())
            {

                if (!File.Exists("gamefolder.txt")) File.WriteAllText("gamefolder.txt", "C:/Program Files (x86)/Steam/steamapps/common/Among Us");
                moguspath = File.ReadAllText("gamefolder.txt");
            }

            if (!File.Exists("mods.json")) File.WriteAllText("mods.json", "[]");
            pruneMods();
            await updater();
            Console.WriteLine("Welcome To aMogusManager");
            Console.Write(@"1. Run an installed instance Of Among Us
2. Install a new mod from a .ZIP file
3. Install a plugin to an instance of Among Us
4. Install vanilla Among Us
5. Uninstall a mod
0. Exit
What is your selection?: ");

            if (!int.TryParse(Console.ReadLine(), out int choice)) continue;

            switch (choice)
            {
                case 0: return;
                // case 1: runMod(); break;
                case 1: runMod(); return;
                case 2: installFromZip(); break;
                case 3: installPlugin(); break;
                case 4: installVanilla(); break;
                case 5: removeMod(); break;
                case 67: await updater(); break;
            }
            if (interaction)
            {
                Console.WriteLine("Press any key to return to the main menu...");
                Console.ReadKey();
            }


        }
    }

    static void LinuxPrefix()
    {
        if (!File.Exists(".prefix`d") || int.Parse(File.ReadAllText(".prefix`d")) > 10)
        {
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string prefix = Path.Combine(homeDir, ".local/share/Steam/steamapps/compatdata/945360/pfx");
            Process.Start("/bin/bash", $"-c WINEDEBUG=-all WINEPREFIX='{prefix}' wine reg add HKCU\\Software\\Wine\\DllOverrides /v winhttp /d native,builtin /f >/dev/null 2>error.log").WaitForExit();
            File.WriteAllText(".prefix`d", "1");
        }
        else if (int.Parse(File.ReadAllText(".prefix`d")) < 10)
        {
            File.WriteAllText(".prefix`d", $"{int.Parse(File.ReadAllText(".prefix`d")) + 1}");
        }
    }

    static void pruneMods()
    {
        JArray mods = JArray.Parse(File.ReadAllText("mods.json"));
        var toRemove = new List<JObject>();

        foreach (JObject mogusmod in mods)
        {
            string installDir = mogusmod["installDir"].ToString();
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), installDir)))
                toRemove.Add(mogusmod);
        }

        foreach (var badMod in toRemove) mods.Remove(badMod);
        if (toRemove.Count > 0)
        {
            File.WriteAllText("mods.json", mods.ToString());
            Console.WriteLine($"{toRemove.Count} invalid Mod(s) removed.");
        }
    }

    static void runMod()
    {
        interaction = false;
        JArray mods = JArray.Parse(File.ReadAllText("mods.json"));
        foreach (JObject mogusmod in mods) Console.WriteLine(mogusmod["name"].ToString());

        Console.Write("What mod do you want to run?: ");
        string input = Console.ReadLine()?.Trim() ?? "";

        foreach (JObject mogusmod in mods)
        {
            if (string.Equals(mogusmod["name"].ToString().Trim(), input, StringComparison.OrdinalIgnoreCase))
            {
                if (Directory.Exists(moguspath)) Directory.Delete(moguspath, true);

                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    //does a ton of fancy stuff to create the symlink's cousion
                    var junction = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c mklink /J \"{moguspath}\" \"{Path.GetFullPath(".")}\\{mogusmod["installDir"]}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    Process.Start(junction).WaitForExit();
                }
                else
                {
                    Directory.CreateSymbolicLink(moguspath, $"{Path.GetFullPath(".")}/{mogusmod["installDir"]}");
                }

                Process.Start(new ProcessStartInfo
                {
                    // launches Among Us though Steam so networking features work
                    UseShellExecute = true,
                    FileName = "steam://launch/945360"
                });
                Console.WriteLine($"Launching {mogusmod["name"]}...");
                Thread.Sleep(2500);
                return;
            }
        }

        Console.WriteLine("Mod not found.");
    }

    static void installFromZip()
    {
        Console.Write("\nEnter the path to the mod's .zip file: ");
        zipmod = Console.ReadLine()?.Trim().Trim('"');
        if (string.IsNullOrWhiteSpace(zipmod))
        {
            Console.WriteLine("Error: .zip path cannot be empty.");
            return;
        }

        JArray versions = JArray.Parse(File.ReadAllText("versions.json"));
        foreach (JObject version in versions) Console.WriteLine(version["version"]);

        Console.Write("What version of Among Us does this mod run on?: ");
        string input = Console.ReadLine()?.Trim() ?? "";

        foreach (JObject version in versions)
        {
            if (input == version["version"].ToString())
            {
                selectedversion = input;
                DownloadInstance(version["manifestID"].ToString(), true);
                return;
            }
        }

        Console.WriteLine("Error: Version not found.");
    }

    static void DownloadInstance(string manifestID, bool modded)
    {
        Console.Write("What do you want to name this instance? ");
        string instancename = Console.ReadLine()?.Trim() ?? "";

        string cacheDir = $"cache/{selectedversion}";

        // If the version of Among Us was cached, skip downloading it from the internet
        if (Directory.Exists(cacheDir))
        {
            foreach (var dirPath in Directory.GetDirectories(cacheDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(cacheDir, dirPath);
                string targetDir = Path.Combine($"instances/{instancename}", relativePath);
                Directory.CreateDirectory(targetDir);
            }

            foreach (var filePath in Directory.GetFiles(cacheDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(cacheDir, filePath);
                string targetFile = Path.Combine($"instances/{instancename}", relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetFile) ?? "");
                File.Copy(filePath, targetFile, true);
            }
        }
        else
        {
            Console.Write("Enter your Steam username: "); // For some reason, DepotDownloader's built-in 'Enter Username' prompt doesn't show up.
            string input = Console.ReadLine();

            //DepotDownloader.Program.Main(new string[] { "-app", "945360", "-depot", "945361", "-remember-password", "-manifest", manifestID, "-dir", cacheDir, "-user", input }).Wait();
            DepotDownloader.Program.Main(["-app", "945360", "-depot", "945361", "-remember-password", "-manifest", manifestID, "-dir", cacheDir, "-user", input]).Wait();

            foreach (var filePath in Directory.GetFiles(cacheDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(cacheDir, filePath);
                string targetFile = Path.Combine($"instances/{instancename}", relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetFile) ?? "");
                File.Copy(filePath, targetFile, true);
            }
        }
        // a ton of fancy nothings
        JArray mods = JArray.Parse(File.ReadAllText("mods.json"));
        mods.Add(new JObject(
            new JProperty("name", instancename),
            new JProperty("installDir", $"./instances/{instancename}")
        ));
        File.WriteAllText("mods.json", mods.ToString());

        if (modded)
        {
            ZipFile.ExtractToDirectory(zipmod, "tmp", true);
            foreach (string dir in Directory.GetDirectories("tmp"))
            {
                foreach (string f in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                {
                    string relPath = f.Substring(dir.Length).TrimStart(Path.DirectorySeparatorChar);
                    string dest = Path.Combine($"./instances/{instancename}", relPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(dest) ?? "");
                    File.Copy(f, dest, true);
                }
            }
            Directory.Delete("tmp", true);
        }

        Console.WriteLine($"Instance '{instancename}' installed successfully.");
    }

    static void removeMod()
    {
        JArray mods = JArray.Parse(File.ReadAllText("mods.json"));
        foreach (JObject mogusmod in mods) Console.WriteLine(mogusmod["name"].ToString());

        Console.Write("What mod do you want to uninstall? ");
        string input = Console.ReadLine()?.Trim() ?? "";

        foreach (JObject mogusmod in mods)
        {
            if (string.Equals(mogusmod["name"].ToString().Trim(), input, StringComparison.OrdinalIgnoreCase))
            {
                Directory.Delete(mogusmod["installDir"].ToString() ?? "", true);
                mogusmod.Remove();
                File.WriteAllText("mods.json", mods.ToString());
                Console.WriteLine("Mod uninstalled.");
                return;
            }
        }

        Console.WriteLine("Mod not found.");
    }

    static void installVanilla()
    {
        JArray versions = JArray.Parse(File.ReadAllText("versions.json"));
        foreach (JObject version in versions) Console.WriteLine(version["version"]);

        Console.Write("What version of Among Us do you want to install?: ");
        string input = Console.ReadLine()?.Trim() ?? "";

        foreach (JObject version in versions)
        {
            if (input == version["version"].ToString())
            {
                selectedversion = input;
                DownloadInstance(version["manifestID"].ToString(), false);
                return;
            }
        }

        Console.WriteLine("Error: Version not found.");
    }

    static void installPlugin()
    {
        // works kinda like installFromZip() but it doesn't create a new instance. instead, it just slaps some files onto an existing instance
        Console.Write("\nEnter the path to the plugin's .zip file: ");
        plugin = Console.ReadLine().Trim().Trim('"');

        if (string.IsNullOrWhiteSpace(plugin))
        {
            Console.WriteLine("Error: .zip path cannot be empty.");
            return;
        }

        JArray mods = JArray.Parse(File.ReadAllText("mods.json"));
        foreach (JObject mogusmod in mods) Console.WriteLine(mogusmod["name"].ToString());

        Console.Write("What instance of Among Us do you want to install this plugin to?: ");
        string input = Console.ReadLine()?.Trim() ?? "";

        foreach (JObject mogusmod in mods)
        {
            if (string.Equals(mogusmod["name"].ToString().Trim(), input, StringComparison.OrdinalIgnoreCase))
            {
                ZipFile.ExtractToDirectory(plugin, mogusmod["installDir"].ToString(), true);
                Console.WriteLine("Plugin installed successfully.");
                return;
            }
        }

        Console.WriteLine("Error: Instance not found.");
    }
    
    static async Task updater()
    {
        Console.Clear();
        Console.WriteLine("Checking for updates...");
        interaction = false;
        var client = new HttpClient();
        var downloader = new DownloadWithProgress(); // Totally didn't steal this off the internet and modify it
        client.DefaultRequestHeaders.Add("User-Agent", "aMogusManager/1.0");
        var response = await client.GetStringAsync("https://api.github.com/repos/floatingjacob/amogusmanager/releases/latest");
        string tag = JObject.Parse(response)["tag_name"].ToString();
        string version = await client.GetStringAsync($"https://github.com/floatingjacob/amogusmanager/releases/download/{tag}/version.txt");
        Version currentVersion = new Version(File.ReadAllText("version.txt").Trim());
        Version latestVersion = new Version(version.Trim());

        if (latestVersion > currentVersion)
        {
            Console.WriteLine($"There are update avalible ({currentVersion} ==> {latestVersion}).\n Installing now...");
            if (OperatingSystem.IsLinux())
            {
                // Fancy downloader
                await downloader.Download($"https://github.com/floatingjacob/amogusmanager/releases/download/{tag}/linux.zip", "update.zip");
                Process.Start(new ProcessStartInfo { FileName = "aMogusManager", UseShellExecute = true });
                Environment.Exit(0);
            }
            else if (OperatingSystem.IsWindows())
            {
                // Fancy downloader
                await downloader.Download($"https://github.com/floatingjacob/amogusmanager/releases/download/{tag}/windows.zip", "update.zip");
                Process.Start(new ProcessStartInfo { FileName = "aMogusManager.exe", UseShellExecute = true });
                Environment.Exit(0);
            }
        }
        else if (latestVersion == currentVersion)
        {
            Console.WriteLine("Up to date!");
            await Task.Delay(1000);
            return;
        }
    }
}
