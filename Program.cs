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
#pragma warning disable CS8600
#pragma warning disable CS1998
#pragma warning disable IDE1006
public class Program
{

    static bool interaction = false;
    static string mogusPath;
    static async Task Main(string[] args)
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        if (File.Exists("bootstrap.zip")) // Automatically updates the bootstrap.
        {
            Thread.Sleep(200);
            Console.WriteLine("Updating bootstrap...");
            ZipFile.ExtractToDirectory("bootstrap.zip", ".", true);
            File.Delete("bootstrap.zip");
        }
        // Determines your OS and changes a few settings
        if (OperatingSystem.IsLinux())
        {
            if (!File.Exists("gamefolder.txt")) File.WriteAllText("gamefolder.txt", $"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/Steam/steamapps/common/Among Us")}");
            mogusPath = File.ReadAllText("gamefolder.txt");
            await LinuxPrefix();
        }
        else if (OperatingSystem.IsWindows())
        {
            if (!File.Exists("gamefolder.txt")) File.WriteAllText("gamefolder.txt", "C:/Program Files (x86)/Steam/steamapps/common/Among Us");
            mogusPath = File.ReadAllText("gamefolder.txt");
        }

        if (!File.Exists("mods.json")) File.WriteAllText("mods.json", "[]"); // If the mod file doesn't exist, create an empty one to prevent errors.
        pruneMods();
        interaction = true;
        if (!args.Contains<string>("-noupdate"))
            await updater();
        while (true)
        {
            Console.CursorVisible = true;
            Console.Clear();
            Console.WriteLine("Welcome to aMogusManager.");
            Console.Write(@"
   1. Run an installed instance Of Among Us
   2. Install a new mod from a .ZIP file
   3. Install a plugin to an instance of Among Us
   4. Install vanilla Among Us
   5. Uninstall a mod
   6. Change Steam's Among Us installation path
   0. Exit

What is your selection?: ");

            if (!int.TryParse(Console.ReadLine(), out int choice)) continue;
            switch (choice)
            {
                case 0: Environment.Exit(0); return;
                case 1: runMod(); break;
                case 2: await installerStuffs.installMod(); break;
                case 3: await installerStuffs.installPlugin(); break;
                case 4: await installerStuffs.installVanilla(); break;
                case 5: await removeMod(); break;
                case 6: await changePath(); break;
                case 67: await updater(); break; // Siiiix Seeeven
            }
            if (interaction)
            {
                Console.WriteLine("Press any key to return to the main menu...");
                Console.ReadKey();
            }
        }
    }

    static async Task LinuxPrefix()
    {

        if (!File.Exists("prefix`d"))
        {
            
            //Process detectWine = null;
            //bool sos = false;
          //  Console.Write("Are you using SteamOS or Arch Linux?\ny/n: ");
          //  string input = Console.ReadLine();
          //  if (input.ToUpper() == "Y") sos = true;
           // if (sos) {
                //detectWine = Process.Start(new ProcessStartInfo { FileName = "pacman", Arguments = "-Q wine", RedirectStandardError = true, RedirectStandardOutput = true }); 
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string prefix = Path.Combine(homeDir, ".local/share/Steam/steamapps/compatdata/945360/pfx");
            Process.Start(new ProcessStartInfo { FileName = "/bin/bash", Arguments = $"-c \"WINEPREFIX={prefix} {Path.Combine(homeDir, ".local/share/Steam/steamapps/common/Proton\\ 9.0\\ \\(Beta\\)/files/bin/wine64")} reg add HKCU\\\\Software\\\\Wine\\\\DllOverrides /v winhttp /d native,builtin /f\"", UseShellExecute = true }).WaitForExit();
            File.Create("prefix`d");

          //  }
            //else detectWine = Process.Start(new ProcessStartInfo { FileName = "dpkg", Arguments = "-s wine64", RedirectStandardError = true, RedirectStandardOutput = true });

           /* detectWine.WaitForExit();
            if (detectWine.ExitCode == 0)
            {
                // This whole thing makes it so the mods actually appear inside Among Us.
                Console.WriteLine("Wine detected!");
                Console.WriteLine("Setting up Wine prefix...");
                string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string prefix = Path.Combine(homeDir, ".local/share/Steam/steamapps/compatdata/945360/pfx");
                Process.Start(new ProcessStartInfo { FileName = "/bin/bash", Arguments = $"-c \"WINEPREFIX={prefix} wine reg add HKCU\\\\Software\\\\Wine\\\\DllOverrides /v winhttp /d native,builtin /f\"", UseShellExecute = true }).WaitForExit();
                File.Create("prefix`d");
                Thread.Sleep(2000);
            }
            else
            {
                Process installWine = null;
                Console.Write("Wine is required to run this program and is not installed on your system.\nInstall now? (y/N): ");
                if (Console.ReadLine().ToLower().StartsWith("y"))
                {
                    if (sos) {installWine = Process.Start(new ProcessStartInfo { FileName = "sudo", Arguments = "pacman -S wine --noconfirm" }); }
                    
                    else { installWine = Process.Start(new ProcessStartInfo { FileName = "sudo", Arguments = "apt install wine64 -y" }); }

                    installWine.WaitForExit();
                    if (installWine.ExitCode == 0)
                    {
                        Console.Clear();
                        Console.WriteLine("Wine installed successfully! Please relaunch this program.");
                        Thread.Sleep(5000);
                        Environment.Exit(0);
                    }
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Wine is required to run this program and has not been installed.\nBye!");
                    Thread.Sleep(2500);
                    Environment.Exit(0);
                }
            }*/
        }
        
    }
    static void pruneMods() // Automatically removes mod entries if their install directory does not exist.
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

    static void runMod() // Runs mods (duh)
    {
        bool modFound = false;
        string input;
        interaction = false;
        JArray mods = JArray.Parse(File.ReadAllText("mods.json"));

        while (!modFound)
        {
            Console.Clear();
            for (int id = 0; id < mods.Count; id++)
            {
                Console.WriteLine($"{id + 1}. {mods[id]["name"]}");
            }
            Console.Write("\nWhat mod do you want to run? (Leave empty to cancel): ");
            input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) return;

            else if (int.TryParse(input, out int choice))
            {
                if (choice >= 1 && choice <= mods.Count)
                {
                    modFound = true;
                    JObject mogusMod = (JObject)mods[choice - 1];

                    if (Directory.Exists(mogusPath)) Directory.Delete(mogusPath, true);

                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        // Does a ton of fancy stuff to create a junction (a folder-only symlink) 
                        var junction = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/c mklink /J \"{mogusPath}\" \"{Path.GetFullPath(".")}\\{mogusMod["installDir"]}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        Process.Start(junction).WaitForExit();
                    }
                    else
                    {
                        // It's easier on linux cus you don't normally need admin perms.
                        Directory.CreateSymbolicLink(mogusPath, $"{Path.GetFullPath(".")}/{mogusMod["installDir"]}");
                    }

                    Process.Start(new ProcessStartInfo
                    {
                        // launches Among Us though Steam so networking features work
                        UseShellExecute = true,
                        FileName = "steam://launch/945360"
                    });
                    Console.WriteLine($"Launching {mogusMod["name"]}...");
                    Thread.Sleep(2500);
                    Environment.Exit(0);
                }
            }
        }
        Console.WriteLine("Mod not found.");
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
        string input;
        while (!instanceFound)
        {
            Console.Clear();
            JArray mods = JArray.Parse(File.ReadAllText("mods.json"));
            for (int id = 0; id < mods.Count; id++)
            {
                Console.WriteLine($"{id + 1}. {mods[id]["name"]}");
            }
            Console.Write("\nWhat mod do you want to uninstall? (Leave empty to cancel): ");
            if (string.IsNullOrWhiteSpace(input = Console.ReadLine())) return;
            if (int.TryParse(input, out int choice))
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
        // Sorry about all the messy variables.
        Console.Clear();
        Console.WriteLine("Checking for updates...");
        interaction = false;
        var client = new HttpClient();
        var downloader = new DownloadWithProgress(); // Totally didn't steal this off the internet and modify it
        client.DefaultRequestHeaders.Add("User-Agent", "aMogusManager/1.0"); // Github requires a user agent header
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

    static async Task changePath()
    {
        string input;
        Console.Clear();
        Console.Write("Enter the path to Steam's Among Us folder: ");
        while (string.IsNullOrWhiteSpace(input = Console.ReadLine()))
        {
            Console.Clear();
            Console.Write("Enter the path to Steam's Among Us folder: ");
        }

        if (!string.IsNullOrWhiteSpace(input))
        {
            interaction = false;
            input = input.Trim().Trim('"').Trim('\'');
            Console.WriteLine($"The manager will now assume Among Us is installed at \"{input}\".");
            File.WriteAllText("gamefolder.txt", input);
            Thread.Sleep(3000);
        }
    }
}
