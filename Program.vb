
Imports Newtonsoft.Json.Linq
Imports System.IO
Imports System.IO.Compression
Imports System.Threading


Module Program
    Public zipmod = ""
    Public depotdownloader = ""
    Public moguspath = ""
    Sub Main()
        Console.Clear()
        If OperatingSystem.IsLinux Then
            depotdownloader = "./DepotDownloader"
            moguspath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/Steam/steamapps/common/Among Us")
            If Not File.Exists(".prefix`d") Then
                Dim homeDir As String = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                Dim prefix As String = Path.Combine(homeDir, ".local/share/Steam/steamapps/compatdata/945360/pfx")
                Process.Start("/bin/bash", $"-c ""WINEDEBUG=-all WINEPREFIX='{prefix}' wine reg add HKCU\\Software\\Wine\\DllOverrides /v winhttp /d native,builtin /f >/dev/null 2>error.log""").WaitForExit()
                File.WriteAllText(".prefix`d", "1")
            ElseIf File.ReadAllText(".prefix`d") < 10 Then
                File.WriteAllText(".prefix`d", $"{File.ReadAllText(".prefix`d") + 1}")
            ElseIf File.ReadAllText(".prefix`d") > 10 Then
                Dim homeDir As String = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                Dim prefix As String = Path.Combine(homeDir, ".local/share/Steam/steamapps/compatdata/945360/pfx")
                Process.Start("/bin/bash", $"-c ""WINEDEBUG=-all WINEPREFIX='{prefix}' wine reg add HKCU\\Software\\Wine\\DllOverrides /v winhttp /d native,builtin /f >/dev/null 2>error.log""").WaitForExit()
                File.WriteAllText(".prefix`d", "1")
            End If

        ElseIf OperatingSystem.IsWindows Then
            depotdownloader = "DepotDownloader.exe"
            moguspath = "C:/Program Files (x86)/Steam/steamapps/common/Among Us"
        End If


        If Not File.Exists("mods.json") Then File.WriteAllText("mods.json", "[]")
        Dim mods As JArray = JArray.Parse(File.ReadAllText("mods.json"))
        Dim toRemove As New List(Of JObject)
        ' *************Auto prunes invalid mod entries**************
        For Each mogusmod As JObject In mods
            Dim installDir = mogusmod("installDir").ToString()
            If Not Directory.Exists(installDir) Then
                toRemove.Add(mogusmod)
            End If
        Next

        For Each badMod In toRemove
            mods.Remove(badMod)
        Next

        If toRemove.Count > 0 Then
            File.WriteAllText("mods.json", mods.ToString())
            Console.WriteLine($"{toRemove.Count} invalid mod(s) removed.")
        End If
        '   ********************************************************


        Console.WriteLine("Welcome to aMogusManager")
        Console.Write("1. Run an installed instance of Among Us
2. Install a new mod from a .zip file
3. Install vanilla Among Us.
4. Uninstall a mod
What is your selection?: ")
        Select Case Console.ReadLine()
            Case 1
                Runmod()
            Case 2
                installfromzip()
            Case 3
                installvanilla()
            Case 4
                RemoveMod()
        End Select

    End Sub


    Sub Runmod()
        Dim mods As JArray = JArray.Parse(File.ReadAllText("mods.json"))
        Dim input = ""
        ' Spits out a list of the installed mods
        For Each mogusmod As JObject In mods
            Dim modname = mogusmod("name")
            Console.WriteLine($"{modname}")
        Next
        Console.Write("What mod do you want to run? ")
        input = Console.ReadLine
        For Each mogusmod As JObject In mods
            Dim modname = mogusmod("name").ToString

            If String.Equals(modname?.Trim(), input, StringComparison.OrdinalIgnoreCase) Then
                If Directory.Exists(moguspath) Then
                    Directory.Delete(moguspath, True)
                End If
                ' Creates a symlink to the Among Us instance's folder, instead of manually copying all the files to Steam
                Directory.CreateSymbolicLink(moguspath, $"{Path.GetFullPath(".")}\{mogusmod("installDir")}")
                Dim amogus As New ProcessStartInfo
                amogus.UseShellExecute = True
                amogus.FileName = "steam://launch/945360"
                Process.Start(amogus)
                Console.WriteLine($"Launching {mogusmod("name")}. Bye!")
                Thread.Sleep(2500)
                    Exit For
                End If
        Next
    End Sub
    Public selectedversion = ""
    Sub installfromzip()
        Console.Write($"
Enter the path to the mod's .zip file: ")
        zipmod = Console.ReadLine()
        If String.IsNullOrWhiteSpace(zipmod) Then
            Console.WriteLine("Error: .zip path cannot be empty.")
            installfromzip()
        End If

        Dim input = ""

        Dim versions As JArray = JArray.Parse(File.ReadAllText("versions.json"))
        For Each version As JObject In versions
            Console.WriteLine(version("version"))
        Next

        Console.Write("What version of Among Us does this mod run on?: ")
        input = Console.ReadLine().ToString
        Dim versionMatch As Boolean = False
        For Each version As JObject In versions
            If input = version("version").ToString Then ' Makes sure the inputed version exists,
                selectedversion = input.ToString        ' and then matches it with it's ManifestID
                versionMatch = True
                DownloadInstance(version("manifestID"), True)
                Exit For
            End If
        Next

        If versionMatch = False Then
            Console.WriteLine("Error: Version not found")
            installfromzip()
        End If

    End Sub

    Sub DownloadInstance(manifestID As String, modded As Boolean)
        Console.Write("What do you want to name this instance? ")
        Dim instancename = Console.ReadLine().Trim()

        Dim mods As JArray = JArray.Parse(File.ReadAllText("mods.json"))
        Dim downloader As New ProcessStartInfo
        Dim input = ""
        Dim cacheDir = $"cache/{selectedversion}"

        ' If the selected version of Among Us has already been downloaded, skip downloading from the internet, and use the locally cached version.
        If Directory.Exists(cacheDir) Then
            For Each f In Directory.GetFiles(cacheDir, "*", SearchOption.AllDirectories)
                Dim relPath = Path.GetRelativePath(cacheDir, f)
                Dim dest = Path.Combine($"instances/{instancename}", relPath)
                Directory.CreateDirectory(Path.GetDirectoryName(dest))
                File.Copy(f, dest, True)
            Next
        Else
            ' If the selected version has not been cached, download it from the internet.
            downloader.FileName = depotdownloader
            Console.Write("Enter your steam username: ")
            input = Console.ReadLine()
            downloader.Arguments = $"-app 945360 -depot 945361 -remember-password -manifest {manifestID} -dir cache/{selectedversion} -user {input}"
            downloader.UseShellExecute = True
            Process.Start(downloader).WaitForExit()
            For Each f In Directory.GetFiles(cacheDir, "*", SearchOption.AllDirectories)
                Dim relPath = Path.GetRelativePath(cacheDir, f)
                Dim dest = Path.Combine($"./instances/{instancename}", relPath)
                Directory.CreateDirectory(Path.GetDirectoryName(dest))
                File.Copy(f, dest, True)
            Next
        End If
        ' Updates the list of installed mods.
        mods.Add(New JObject(
                    New JProperty("name", $"{instancename}"),
                    New JProperty("installDir", $"./instances/{instancename}")
                ))
        File.WriteAllText("mods.json", mods.ToString)
        ' Installs the actual mod to the new instance of Among Us.
        If modded Then
            ZipFile.ExtractToDirectory(zipmod, "tmp/", True)
            ' If there is only a folder in the temp direcrory, copy it's contents to the Among Us installation.
            For Each direc In Directory.GetDirectories("tmp")
                For Each f In Directory.GetFiles(direc, "*", SearchOption.AllDirectories)
                    Dim relPath = f.Substring(direc.Length).TrimStart(Path.DirectorySeparatorChar)
                    Dim dest = Path.Combine($"./instances/{instancename}", $"{relPath}")
                    Directory.CreateDirectory(Path.GetDirectoryName(dest))
                    File.Copy(f, dest, True)
                Next
            Next
            Directory.Delete("tmp", True) ' Clears the temp folder
        End If
        Main()
    End Sub
    Sub RemoveMod()
        Dim mods As JArray = JArray.Parse(File.ReadAllText("mods.json"))
        For Each mogusmod As JObject In mods
            Dim modname = mogusmod("name")
            Console.WriteLine($"{modname}")
        Next
        Console.Write("What mod do you want to uninstall? ")
        Dim input = ""
        input = Console.ReadLine

        For Each mogusmod As JObject In mods
            Dim modname = mogusmod("name")
            If String.Equals(mogusmod("name").ToString, input, StringComparison.OrdinalIgnoreCase) Then
                Directory.Delete(mogusmod("installDir"), True)
                mogusmod.Remove()
                File.WriteAllText("mods.json", mods.ToString)
                Main()
                Exit For
            End If
        Next
    End Sub


    Sub installvanilla()
        Dim input = ""

        Dim versions As JArray = JArray.Parse(File.ReadAllText("versions.json"))
        For Each version As JObject In versions
            Console.WriteLine(version("version"))
        Next

        Console.Write("What version of Among Us do you want to install?: ")
        input = Console.ReadLine().ToString
        Dim versionMatch As Boolean = False
        For Each version As JObject In versions
            If input = version("version").ToString Then ' Makes sure the inputed version exists,
                selectedversion = input.ToString        ' and then matches it with it's ManifestID
                versionMatch = True
                DownloadInstance(version("manifestID"), False)
                Exit For
            End If
        Next

        If versionMatch = False Then
            Console.WriteLine("Error: Version not found")
            installvanilla()
        End If
    End Sub
End Module
