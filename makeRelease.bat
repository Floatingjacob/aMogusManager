@echo off
cls


powershell rm -r .\Release
cd aBootstrap
powershell rm -r .\bin

set /p zipBootstrap="Do you want to zip the bootstrap as an update? (y/n): "

echo Building Windows bootstrap...
dotnet publish --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishDir="..\Release\bootstrap\windows\"
cd ..\Release\bootstrap\windows\

if /i "%zipBootstrap%"=="y" (
    powershell Compress-Archive * bootstrap.zip -Force
) else (
    echo ""
    echo Skipping bootstrap zip...
)

cd ..\..\..\
cls
echo Building Windows release...
dotnet publish --self-contained true -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=true  -p:Configuration=Release -p:PublishReadyToRun=true -p:PublishDir="Release\windows\"
move Release\bootstrap\windows\* Release\windows\
powershell Compress-Archive Release\windows\* windows.zip -Force

cls
cd aBootstrap
echo Building Linux bootstrap...
dotnet publish --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true  --os linux -p:PublishDir="..\Release\bootstrap\linux\"
cd ..\Release\bootstrap\linux\

if /i "%zipBootstrap%"=="y" (
    powershell Compress-Archive * bootstrap.zip -Force
) else (
    echo ""
    echo Skipping bootstrap zip...
)

cd ..\..\..\
cls
echo Building Linux release...
dotnet publish --self-contained true -p:PublishSingleFile=false -p:IncludeNativeLibrariesForSelfExtract=true  -p:Configuration=Release -p:PublishReadyToRun=true --os linux -p:PublishDir="Release\linux\"
move Release\bootstrap\linux\* Release\linux\
powershell Compress-Archive Release\linux\* linux.zip -Force
pause