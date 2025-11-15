/*
  
  This is the auto-update installer for the main aMogusManager program. 

 */

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

    int pid = int.Parse(Environment.GetCommandLineArgs()[1]);

    while (true)
    {
        try { Process.GetProcessById(pid); }
        catch { break; }
        await Task.Delay(200);
    }

    if (File.Exists("update.zip"))
    {
        using var zip = ZipFile.OpenRead("update.zip");
        foreach (var entry in zip.Entries)
        {
            if (entry.Name.StartsWith("updater")) continue;
            string dest = Path.GetFullPath(entry.FullName);
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            if (!string.IsNullOrEmpty(entry.Name))
                entry.ExtractToFile(dest, true);
        }
        File.Delete("update.zip");
    }

    if (OperatingSystem.IsWindows()) Process.Start("aMogusManager.exe");
    if (OperatingSystem.IsLinux()) Process.Start("xterm", "-e ./aMogusManager");

Environment.Exit(0);
