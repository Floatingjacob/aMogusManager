/*
 
 Welcome to my super-fragile, idiot-succeptible, OCD-inducing Among Us mod manager.

 I slightly modified DepotDownloader so i could use it here as a library instead of a platform specific program
 (pretty much, i just made everything in it's Program.cs file public)
 i know that this code probably gave someone a stroke when they saw it, so if you don't want others to be hospitalized, 
 consider improving it and making a pull request. 
 On second thought, if you want to hospitalize as many people as possible (cus you're a psycho),
 consider spreading around my github profile. (https://github.com/floatingjacob/)

 */

using c_;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Compression;
#pragma warning disable CS8602
#pragma warning disable CS8618
public class Program
{
    static bool interaction = false;
    static string zipmod;
    static string moguspath;
    //static string plugin;
    //static string selectedversion;
    static async Task Main()
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        if (File.Exists("bootstrap.zip"))
        {
            Thread.Sleep(200);
            Console.WriteLine("Updating bootstrap...");
            ZipFile.ExtractToDirectory("bootstrap.zip", ".", true);
            File.Delete("bootstrap.zip");
        }
        interaction = true;
        await updater();
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
                case 2: installerStuffs.installMod(); break;
                case 3: installerStuffs.installPlugin(); break;
                case 4: installerStuffs.installVanilla(); break;
                case 5: removeMod(); break;
                case 67: await updater(); break; // Siiiix Seeeven
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
        bool modFound = false;
        interaction = false;
        JArray mods = JArray.Parse(File.ReadAllText("mods.json"));
  
        while (!modFound)
        {
            Console.Clear();
            for (int id = 0; id < mods.Count; id++)
            {
                Console.WriteLine($"{id + 1}. {mods[id]["name"]}");
            }
            Console.Write("\nWhat mod do you want to run?: ");
            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                if (choice >= 1 && choice <= mods.Count)
                {
                    modFound = true;
                    JObject mogusMod = (JObject)mods[choice - 1];
                    if (Directory.Exists(moguspath)) Directory.Delete(moguspath, true);

                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        //does a ton of fancy stuff to create the symlink's cousion
                        var junction = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/c mklink /J \"{moguspath}\" \"{Path.GetFullPath(".")}\\{mogusMod["installDir"]}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        Process.Start(junction).WaitForExit();
                    }
                    else
                    {
                        Directory.CreateSymbolicLink(moguspath, $"{Path.GetFullPath(".")}/{mogusMod["installDir"]}");
                    }

                    Process.Start(new ProcessStartInfo
                    {
                        // launches Among Us though Steam so networking features work
                        UseShellExecute = true,
                        FileName = "steam://launch/945360"
                    });
                    Console.WriteLine($"Launching {mogusMod["name"]}...");
                    Thread.Sleep(2500);
                    return;
                }
            }
        }


        Console.WriteLine("Mod not found.");
    }
    public static async Task DownloadInstance(string manifestID, bool modded, string selectedVersion, string instanceName)
    {
        string cacheDir = $"cache/{selectedVersion}";

        // If the version of Among Us was cached, skip downloading it from the internet
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
            Console.Write("Enter your Steam username: "); // For some reason, DepotDownloader's built-in 'Enter Username' prompt doesn't show up.
            string input = Console.ReadLine();
            DepotDownloader.Program.Main(["-app", "945360", "-depot", "945361", "-remember-password", "-manifest", manifestID, "-dir", cacheDir, "-user", input]).Wait();
            foreach (var filePath in Directory.GetFiles(cacheDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(cacheDir, filePath);
                string targetFile = Path.Combine($"instances/{instanceName}", relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetFile) ?? "");
                File.Copy(filePath, targetFile, true);
            }
        }
    }

    static async Task removeMod()
    {
        bool instanceFound = false;
        
        while (!instanceFound)
        {
            Console.Clear();
            JArray mods = JArray.Parse(File.ReadAllText("mods.json"));
            for (int id = 0; id < mods.Count; id++)
            {
                Console.WriteLine($"{id + 1}. {mods[id]["name"]}");
            }
            Console.Write("\nWhat mod do you want to uninstall?: ");
            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                if (choice >= 1 && choice <= mods.Count)
                {
                    JObject mogusMod = (JObject)mods[choice - 1];
                    Directory.Delete(mogusMod["installDir"].ToString() ?? "", true);
                    mogusMod.Remove();
                    File.WriteAllText("mods.json", mods.ToString());
                    Console.WriteLine("Mod uninstalled.");
                    Thread.Sleep(2000);
                    return;
                }
            }


        }

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
            Console.WriteLine($"There are updates avalible ({currentVersion} ==> {latestVersion}).");
            if (OperatingSystem.IsLinux())
            {
                // Fancy downloader
                await downloader.Download($"https://github.com/floatingjacob/amogusmanager/releases/download/{tag}/linux.zip", "update.zip");
                Process.Start(new ProcessStartInfo { FileName = "aMogusManager", Arguments = "-update", UseShellExecute = true });
                Environment.Exit(0);
            }
            else if (OperatingSystem.IsWindows())
            {
                // Fancy downloader
                await downloader.Download($"https://github.com/floatingjacob/amogusmanager/releases/download/{tag}/windows.zip", "update.zip");
                Process.Start(new ProcessStartInfo { FileName = "aMogusManager.exe", Arguments = "-update", UseShellExecute = true });
                Environment.Exit(0);
            }
        }
        else if (latestVersion == currentVersion)
        {
            Console.WriteLine("Up to date!");
            Thread.Sleep(1000);
            return;
        }
    }
}
