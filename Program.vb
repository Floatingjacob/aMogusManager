
Imports Newtonsoft.Json.Linq
Imports System.IO
Imports System.IO.Compression


Module Program
    Public zipmod = ""
    Sub Main()
        Console.Clear()
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
        Console.Write("1. Run an installed mod
2. Install a new mod from a .zip file
3. Uninstall a mod
What is your selection?: ")
        Select Case Console.ReadLine()
            Case 1
                runmod()
            Case 2
                installfromzip()
            Case 3
                RemoveMod()
        End Select

    End Sub


    Sub runmod()

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
            Dim modname = mogusmod("name")
            ' Launches modded Among Us if the installation exists
            If String.Equals(mogusmod("name").ToString?.Trim(), input, StringComparison.OrdinalIgnoreCase) Then
                Dim amogus As New ProcessStartInfo
                amogus.FileName = ($"{mogusmod("installDir")}/Among Us.exe")
                Process.Start(amogus)
                Console.WriteLine($"Launching {mogusmod("name")}. Bye!")
                Exit For
            End If
        Next
    End Sub
    Public selectedversion = ""
    Sub installfromzip()
        Dim installmodname = ""
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
                DownloadInstance(version("manifestID"))
                Exit For
            End If
        Next

        If versionMatch = False Then
            Console.WriteLine("Error: Version not found")
            installfromzip()
        End If

    End Sub

    Sub DownloadInstance(manifestID As String)
        Console.Write("What do you want to name this mod? ")
        Dim installmodname = Console.ReadLine().Trim()

        Dim mods As JArray = JArray.Parse(File.ReadAllText("mods.json"))
        Dim downloader As New ProcessStartInfo
        Dim input = ""

        downloader.FileName = "DepotDownloader.exe"

        Dim cacheDir = $"cache/{selectedversion}"

        ' If the selected version of Among Us has already been downloaded, skip downloading from the internet, and use the locally cached version.
        If Directory.Exists(cacheDir) Then
            For Each f In Directory.GetFiles(cacheDir, "*", SearchOption.AllDirectories)
                Dim relPath = Path.GetRelativePath(cacheDir, f)
                Dim dest = Path.Combine($"instances/{installmodname}", relPath)
                Directory.CreateDirectory(Path.GetDirectoryName(dest))
                File.Copy(f, dest, True)
            Next
        Else
            ' If the selected version has not been cached, download it from the internet.
            Console.Write("Enter your steam username: ")
            input = Console.ReadLine()
            downloader.Arguments = $"-app 945360 -depot 945361 -remember-password -manifest {manifestID} -dir cache/{selectedversion} -user {input}"
            downloader.UseShellExecute = True
            Process.Start(downloader).WaitForExit()
        End If
        ' Updates the list of installed mods.
        mods.Add(New JObject(
                    New JProperty("name", $"{installmodname}"),
                    New JProperty("installDir", $"instances/{installmodname}")
                ))
        File.WriteAllText("mods.json", mods.ToString)
        ' Installs the actual mod to the new instance of Among Us.
        ZipFile.ExtractToDirectory(zipmod, "tmp/", True)
        ' If there is only a folder in the temp direcrory, copy it's contents to the Among Us installation.
        If Directory.GetDirectories("tmp/").Count <= 2 Then
            For Each direc In Directory.GetDirectories("tmp")
                For Each f In Directory.GetFiles(direc, "*", SearchOption.AllDirectories)
                    Dim relPath = f.Substring(direc.Length).TrimStart(Path.DirectorySeparatorChar)
                    Dim dest = Path.Combine($"instances/{installmodname}", $"{relPath}")
                    Directory.CreateDirectory(Path.GetDirectoryName(dest))
                    File.Copy(f, dest, True)
                Next
            Next
        Else
            For Each direc In Directory.GetDirectories("tmp")
                For Each f In Directory.GetFiles(direc, "*", SearchOption.AllDirectories)
                    Dim relPath = f.Substring(direc.Length).TrimStart(Path.DirectorySeparatorChar)
                    Dim dest = Path.Combine($"instances/{installmodname}", $"{relPath}")
                    Directory.CreateDirectory(Path.GetDirectoryName(dest))
                    File.Copy(f, dest, True)
                Next
            Next
        End If
        Directory.Delete("tmp", True) ' Clears the temp folder
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
End Module