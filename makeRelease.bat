@echo off
cls


powershell rm -r .\Release
mkdir Release

cls

echo Building Windows Frontend...
cd frontend
call npx tauri build
copy src-tauri\target\release\amogusmanager-ui.exe ..
cd ..
cls
echo Killing VBCSCompiler if it is running...
taskkill /F /IM VBCSCompiler.exe
echo Building Windows release...
dotnet publish --self-contained true -p:PublishSingleFile=false -p:Configuration=Release -p:PublishReadyToRun=false -p:PublishDir="Release\windows\"
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" aMogusManager.iss

pause