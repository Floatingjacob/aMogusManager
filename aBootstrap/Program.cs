/*
  
 This is the bootstrap for 'aMogusmanager'.
 Pretty much, if it is launched with the '-update' flag and finds an 
 update file (update.zip) it installs it and then launches the main application.

 */


using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

if (args.Contains<string>("-update")) // Ooh...Fancy...
{
    await Task.Delay(1000);
    if (File.Exists("update.zip"))
    {
        using (ZipArchive update = ZipFile.OpenRead("update.zip"))
        {
            foreach (ZipArchiveEntry file in update.Entries)
            {
                if (string.Equals(file.Name, "aMogusManager.exe", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (string.Equals(file.Name, "aMogusManager.pdb", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (string.Equals(file.Name, "aMogusManager", StringComparison.OrdinalIgnoreCase))
                    continue;

                string dest = Path.GetFullPath(Path.Combine(".", file.FullName));
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                if (!string.IsNullOrEmpty(file.Name))
                    file.ExtractToFile(dest, overwrite: true);
            }
        }
        File.Delete("update.zip");
    }
}

if (OperatingSystem.IsWindows())
    Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = "mainApplication.exe" });
else if (OperatingSystem.IsLinux())
    // Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = "mainApplication" }); // This doesn't want to work for some reason.
    Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = "x-terminal-emulator", Arguments = "-e ./mainApplication" });
Environment.Exit(0);
