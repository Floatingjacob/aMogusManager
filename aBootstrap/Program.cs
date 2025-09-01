using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

Main();

static async Task Main()
{
    Directory.SetCurrentDirectory(AppContext.BaseDirectory);
    if (Environment.GetCommandLineArgs().Length > 1)
    {
        string[] args = Environment.GetCommandLineArgs();
        if (args[1] == "--update")
        {
            await Task.Delay(1000); // Wait a second to make sure the main app is closed
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
                        {
                            file.ExtractToFile(dest, overwrite: true);
                        }
                    }
                }
                File.Delete("update.zip");
            }
        }
    }
    
    if (OperatingSystem.IsWindows())
        Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = "mainApplication.exe" });
    else if (OperatingSystem.IsLinux())
        Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = "mainApplication" });

    Environment.Exit(0);
}
