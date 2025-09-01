@echo off
cls
cd aBootstrap
echo Building Windows bootstrap
dotnet publish --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
cd bin\Release\net9.0\win-x64\publish
powershell Compress-Archive * bootstrap.zip -Force
cd ..\..\..\..\..
echo Building Linux bootstrap
dotnet publish --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --os linux
cd bin\Release\net9.0\linux-x64\publish
powershell Compress-Archive * bootstrap.zip -Force
cd ..\..\..\..\..\..