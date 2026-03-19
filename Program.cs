/*
 
 Welcome to my super-fragile, idiot-succeptible, OCD-inducing Among Us mod manager.
 Viewer discretion advised.
 I slightly modified DepotDownloader so i could use it here as a library instead of a platform specific program
 (pretty much, i just made everything in it's Program.cs file public)

 This file was created by FloatingJacob for use with aMogusManager.

*/



#pragma warning disable CS8602 // *ahem*
#pragma warning disable CS8618
#pragma warning disable CS8600
#pragma warning disable CS1998
#pragma warning disable IDE1006

namespace aMogusManager
{
    public class Program
    {

        static string mogusPath;
        static int FrontendPID;
        public static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(killFrontend);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Warning] Closing this window will cause the frontend to stop working.");
            Console.ForegroundColor = ConsoleColor.White;
            await checkUpdates();
            FrontendBackend.startFrontendBackend(); // 👁️👄👁️
            Environment.CurrentDirectory = AppContext.BaseDirectory;
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "aMogusManager"));
            File.Copy("version.txt", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "aMogusManager/version.txt"), true);
            File.Copy("versions.json", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "aMogusManager/versions.json"), true);

            Directory.SetCurrentDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "aMogusManager"));

            if (!File.Exists("mods.json")) File.WriteAllText("mods.json", "[]"); // If the mod file doesn't exist, create an empty one to prevent errors.
            pruneMods();

            // Determines your OS and changes a few settings
            if (OperatingSystem.IsLinux())
            {
                if (!File.Exists("gamefolder.txt")) File.WriteAllText("gamefolder.txt", $"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/Steam/steamapps/common/Among Us")}");
                mogusPath = File.ReadAllText("gamefolder.txt");
                await LinuxPrefix();
                await startUI(false);
            }
            else if (OperatingSystem.IsWindows())
            {
                if (!File.Exists("gamefolder.txt")) File.WriteAllText("gamefolder.txt", "C:/Program Files (x86)/Steam/steamapps/common/Among Us");
                mogusPath = File.ReadAllText("gamefolder.txt");
                await startUI(true);
            }
        }
        public static async Task LinuxPrefix()
        {

            if (!File.Exists("prefix`d"))
            {

                string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string prefix = Path.Combine(homeDir, ".local/share/Steam/steamapps/compatdata/945360/pfx");
                Process.Start(new ProcessStartInfo { FileName = "/bin/bash", Arguments = $"-c \"WINEPREFIX={prefix} {Path.Combine(homeDir, ".local/share/Steam/steamapps/common/Proton\\ 9.0\\ \\(Beta\\)/files/bin/wine64")} reg add HKCU\\\\Software\\\\Wine\\\\DllOverrides /v winhttp /d native,builtin /f\"", UseShellExecute = true }).WaitForExit(); // Changes some wine (proton) registry settings so Among Us mods actually work
                File.Create("prefix`d"); // Never do it again
                // TODO: make the registry-editing process less assumptive (actually detect if proton exists before hurling commands off into the void)
            }
        }
        public static void pruneMods() // Automatically removes mod entries if their install directory does not exist.
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
                Console.WriteLine($"[Info] {toRemove.Count} invalid Mod(s) removed.");
            }
        }

        public static void runMod(int modID)
        {
            bool modFound = false;
            int pos = -1;
            JArray mods = JArray.Parse(File.ReadAllText("mods.json"));

            while (!modFound)
            {
                pos++;
                if (mods[pos]["modID"].ToString() == modID.ToString())
                {
                    modFound = true;
                    JObject mogusMod = (JObject)mods[pos];

                    if (Directory.Exists(mogusPath)) Directory.Delete(mogusPath, true);

                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        // Does a ton of fancy stuff to create a junction on windows (a folder-only symlink) 
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
                    Console.WriteLine($"[Info] Launching {mogusMod["name"]}...");
                    Thread.Sleep(2500);
                    Environment.Exit(0);
                }
            }
        }
  
        public static async Task checkUpdates()
        {
            // Sorry about all the messy variables.
            try
            {
                Console.WriteLine("[Info] UpdateChecker: Checking for updates...");
                var client = new HttpClient();
                client.Timeout = new TimeSpan(10000 * 10000);
                client.DefaultRequestHeaders.Add("User-Agent", "aMogusManager/1.0"); // Github requires a user agent header
                var response = await client.GetStringAsync("https://api.github.com/repos/floatingjacob/amogusmanager/releases/latest");
                string tag = JObject.Parse(response)["tag_name"].ToString();
                string version = await client.GetStringAsync($"https://github.com/floatingjacob/amogusmanager/releases/download/{tag}/version.txt");
                Version currentVersion = new Version(File.ReadAllText("version.txt").Trim());
                Version latestVersion = new Version(version.Trim());

                if (latestVersion > currentVersion)
                {
                    Console.WriteLine("[Info] UpdateChecker: A newer version of aMogusManager is available. You can download it for your platform at https://github.com/floatingjacob/aMogusManager/releases/latest");
                    Console.Title = $"aMogusManager v{currentVersion} (Update Available)";
                }
                else if (latestVersion == currentVersion)
                {
                    Console.WriteLine("[Info] UpdateChecker: Up to date!");
                    Console.Title = $"aMogusManager v{currentVersion}";
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[ERROR] UpdateChecker:  {ex}");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        
        public static async Task startUI(bool isWindows)
        {
            Console.WriteLine("[Info] Launching frontend...");

            if (isWindows)
            {
                using var ui = Process.Start(new ProcessStartInfo { FileName = Path.Combine(AppContext.BaseDirectory, "amogusmanager-ui.exe") });
                FrontendPID = ui.Id;
                ui.WaitForExit();

            }
            else
            {
                using var ui = Process.Start(new ProcessStartInfo { FileName = Path.Combine(AppContext.BaseDirectory, "amogusmanager-ui") });
                FrontendPID = ui.Id;
                ui.WaitForExit();
            }
            Console.WriteLine("[Info] Frontend window closed. Bye!");
        }
        public static async Task changePath(string newPath)
        {
            newPath = newPath.Trim().Trim('"').Trim('\'');
            File.WriteAllText("gamefolder.txt", newPath);
        }

        static void killFrontend(object sender, EventArgs e)
        {
            try {Process.GetProcessById(FrontendPID).Kill();}
            catch {} // The manager throws tantrums if the frontend closes first.
            
        }
    }
}