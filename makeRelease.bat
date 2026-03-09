@echo off
cls


powershell rm -r .\Release

cls
echo Building Windows release...
dotnet publish --self-contained true -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=true  -p:Configuration=Release -p:PublishReadyToRun=true -p:PublishDir="Release\windows\"
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" aMogusManager.iss

pause
