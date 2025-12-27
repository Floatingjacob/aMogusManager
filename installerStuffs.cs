/*
 
 This file contains most of the stuff relating to installing instances of Among Us.
 This file was created by FloatingJacob for use with aMogusManager.
 
 */

using Newtonsoft.Json.Linq;
using System.IO.Compression;

namespace aMogusManager
{
    public class installerStuffs
    {
        public static string selectedVersion;
        static string plugin;
        static string zipMod;

        public static async Task installVanilla(string instanceName, string gameVersion)
        {
            bool versionFound = false;
            JArray versions = JArray.Parse(File.ReadAllText("versions.json"));

            while (!versionFound)
            {

                foreach (JObject version in versions)
                {
                    if (gameVersion == version["version"].ToString())
                    {
                        versionFound = true;
                        selectedVersion = gameVersion;
                        await Program.DownloadInstance(version["manifestID"].ToString(), false, selectedVersion, instanceName);

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
                        return;
                    }
                }
            }
        }

        public static async Task installPlugin(string modID, string zipMod)
        {
            // works kinda like installMod() but it doesn't create a new instance. instead, it just slaps some files onto an existing instance
            bool instanceFound = false;
            plugin = zipMod.Trim().Trim('"').Trim('\'');
            int pos = -1;

            while (!instanceFound)
            {
                pos++;
                JObject selectedMod = null;
                JArray mods = JArray.Parse(File.ReadAllText("mods.json"));

                if (mods[pos]["modID"].ToString() == modID.ToString())
                {
                    instanceFound = true;
                    selectedMod = (JObject)mods[pos];
                }
                {

                    string dest = selectedMod["installDir"].ToString();
                    instanceFound = true;
                    string root = plugin;
                    if (File.Exists(plugin))
                    {
                        string tmpf = "./tmp";
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
                    if (Directory.Exists("./tmp"))
                        Directory.Delete("./tmp", true);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[Info] Plugin installed successfully.");
                    Console.ForegroundColor= ConsoleColor.White;
                    return;
                }
            }
        }

        public static async Task installMod(string modFile, string instanceName, string gameVersion)
        {
            bool versionFound = false;
            zipMod = modFile;

            JArray versions = JArray.Parse(File.ReadAllText("versions.json"));
            while (!versionFound)
            {

                foreach (JObject version in versions)
                {
                    if (gameVersion == version["version"].ToString())
                    {
                        selectedVersion = gameVersion;
                        versionFound = true;
                        await Program.DownloadInstance(version["manifestID"].ToString(), true, selectedVersion, instanceName);
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
                Console.ForegroundColor= ConsoleColor.White;
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
        }
    }

}
