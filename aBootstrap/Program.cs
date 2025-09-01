using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

Main();

static void Main()
{
    Directory.SetCurrentDirectory(AppContext.BaseDirectory);

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

    if (OperatingSystem.IsWindows())
        Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = "mainApplication.exe" });
    else if (OperatingSystem.IsLinux())
        Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = "mainApplication" });

    Environment.Exit(0);
}
