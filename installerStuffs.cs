/*
 
 This file contains most of the stuff relating to installing instances of Among Us.
 This file was created by FloatingJacob (who else, besides Meta, could've done it so messily?)
 
 */

using Newtonsoft.Json.Linq;
using System.IO.Compression;

namespace c_
{
    public class installerStuffs // The part that has the part that installs things
    {
        public static string selectedVersion;
        static string plugin;
        static string zipMod;

        public static async Task installVanilla()
        {
            bool versionFound = false;
            string instanceName;
            JArray versions = JArray.Parse(File.ReadAllText("versions.json"));
            Console.Write("What do you want to name this instance?: ");
            instanceName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(instanceName)) return;
            while (!versionFound)
            {
                Console.Clear();
                foreach (JObject version in versions) Console.WriteLine(version["version"]);
                Console.Write("What version of Among Us do you want to install?: ");
                string input = Console.ReadLine();

                foreach (JObject version in versions)
                {
                    if (input == version["version"].ToString())
                    {
                        versionFound = true;
                        selectedVersion = input;
                        await Program.DownloadInstance(version["manifestID"].ToString(), false, selectedVersion, instanceName);

                        JArray mods = JArray.Parse(File.ReadAllText("mods.json"));
                        mods.Add(new JObject( // Creates a new mod entry
                            new JProperty("name", instanceName),
                            new JProperty("installDir", $"./instances/{instanceName}")
                        ));
                        File.WriteAllText("mods.json", mods.ToString());
                        return;
                    }
                }
            }
        }

        public static async Task installPlugin()
        {
            // works kinda like installMod() but it doesn't create a new instance. instead, it just slaps some files onto an existing instance
            Console.Clear();
            Console.Write("Enter the path to the plugin's .zip file: ");
            bool instanceFound = false;
            plugin = Console.ReadLine().Trim().Trim('"').Trim('\'');

            if (string.IsNullOrWhiteSpace(plugin = Console.ReadLine())) return;

            while (!instanceFound)
            {
                Console.Clear();
                JArray mods = JArray.Parse(File.ReadAllText("mods.json"));

                for (int id = 0; id < mods.Count; id++)
                {
                    Console.WriteLine($"{id + 1}. {mods[id]["name"]}");
                }
                Console.Write("\nWhat instance do you want to install this plugin to?: ");
                if (int.TryParse(Console.ReadLine(), out int choice))
                {
                    if (choice >= 1 && choice <= mods.Count)
                    {
                        JObject selectedMod = (JObject)mods[choice - 1];
                        instanceFound = true;
                        ZipFile.ExtractToDirectory(plugin, selectedMod["installDir"].ToString(), true);
                        Console.WriteLine("Plugin installed successfully.");
                        return;
                    }
                }
            }
        }

        public static async Task installMod()
        {
            bool versionFound = false;
            string instanceName;

            Console.Write("What do you want to name this instance?: ");
            instanceName = Console.ReadLine();

            Console.Write("Enter the path to the mod's .zip file: ");
            zipMod = Console.ReadLine()?.Trim().Trim('"').Trim('\'');
            if (string.IsNullOrWhiteSpace(zipMod)) return;
            JArray versions = JArray.Parse(File.ReadAllText("versions.json"));
            while (!versionFound)
            {
                Console.Clear();
                foreach (JObject version in versions) Console.WriteLine(version["version"]);
                Console.Write("What version of Among Us does this mod run on?: ");
                string input = Console.ReadLine();

                foreach (JObject version in versions)
                {
                    if (input == version["version"].ToString())
                    {
                        selectedVersion = input;
                        versionFound = true;
                        await Program.DownloadInstance(version["manifestID"].ToString(), true, selectedVersion, instanceName);
                        break;
                    }
                }
            }

            ZipFile.ExtractToDirectory(zipMod, "tmp", true);
            foreach (string dir in Directory.GetDirectories("tmp"))
            {
                foreach (string f in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                {
                    string relPath = f.Substring(dir.Length).TrimStart(Path.DirectorySeparatorChar);
                    string dest = Path.Combine($"./instances/{instanceName}", relPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(dest) ?? "");
                    File.Copy(f, dest, true);
                }
            }
            // Adds a new instance entry *so you know it's installed*
            JArray mods = JArray.Parse(File.ReadAllText("mods.json"));
            mods.Add(new JObject(
                new JProperty("name", instanceName),
                new JProperty("installDir", $"./instances/{instanceName}")
            ));
            File.WriteAllText("mods.json", mods.ToString());
            Directory.Delete("tmp", true);

        }
    }
}
