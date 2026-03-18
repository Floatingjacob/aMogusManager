@echo off
cls


powershell rm -r .\Release
mkdir Release

cls

echo Building Windows Frontend...
cd frontend
call npm run tauri build --no-bundle
cd ..
cls
echo Building Windows release...
dotnet publish --self-contained true -p:PublishSingleFile=false -p:Configuration=Release -p:PublishReadyToRun=false -p:PublishDir="Release\windows\"
copy frontend\src-tauri\target\release\amogusmanager-ui.exe .\Release
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" aMogusManager.iss

pause