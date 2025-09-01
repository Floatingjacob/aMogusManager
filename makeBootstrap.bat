@echo off
cd aBootstrap
echo Building Windows bootstrap
dotnet publish --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
echo Building Linux bootstrap
dotnet publish --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true --os linux
cd ..