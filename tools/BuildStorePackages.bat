@ECHO OFF
ECHO SEARCHING FOR VISUAL STUDIO...
"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -prerelease -version [17.0,18.0) -sort -requires Microsoft.Component.MSBuild -products * -property InstallationPath > %TEMP%\vsinstalldir.txt
SET /p _VSINSTALLDIR=<%TEMP%\vsinstalldir.txt
DEL %TEMP%\vsinstalldir.txt	
IF "%_VSINSTALLDIR%"=="" (
  ECHO ERROR: VISUAL STUDIO NOT FOUND
  EXIT /B 1
)
IF "%VSINSTALLDIR%"=="" (
  CALL "%_VSINSTALLDIR%\Common7\Tools\VsDevCmd.bat"
)

IF %RUNTIMEVERSION%=="" (
   SET RUNTIMEVERSION=200.3.0
)


REM Configure NuGet
dotnet new nugetconfig --force

IF EXIST '%CUSTOM_NUGET_REPO%` (
   dotnet nuget add source %CUSTOM_NUGET_REPO%
)

SET NUGET_PACKAGES=%~dp0.nuget\packages
SET NUGET_HTTP_CACHE_PATH=%~dp0.nuget\cache
md %NUGET_PACKAGES%
md %NUGET_HTTP_CACHE_PATH%

REM BUILD

ECHO Building WPF .NET Framework
msbuild /restore /t:Build arcgis-maps-sdk-dotnet-samples\src\WPF\WPF.Viewer\ArcGIS.WPF.Viewer.NetFramework.csproj /p:Configuration=Release 

ECHO Generating WPF Store packages
msbuild /restore /t:Build arcgis-maps-sdk-dotnet-samples\src\WPF\WPF.StorePackage\ArcGIS.WPF.StorePackage.wapproj /p:Configuration=Release /p:Platform=x86
msbuild /restore /t:Build arcgis-maps-sdk-dotnet-samples\src\WPF\WPF.StorePackage\ArcGIS.WPF.StorePackage.wapproj /p:Configuration=Release /p:Platform=x64
msbuild /restore /t:Build arcgis-maps-sdk-dotnet-samples\src\WPF\WPF.StorePackage\ArcGIS.WPF.StorePackage.wapproj /p:Configuration=Release /p:Platform=ARM64

ECHO Generating UWP store packages
msbuild /restore /t:Build arcgis-maps-sdk-dotnet-samples\src\UWP\ArcGIS.UWP.Viewer\ArcGIS.UWP.Viewer.csproj /p:Configuration=Release /p:Platform=x86
msbuild /restore /t:Build arcgis-maps-sdk-dotnet-samples\src\UWP\ArcGIS.UWP.Viewer\ArcGIS.UWP.Viewer.csproj /p:Configuration=Release /p:Platform=x64
msbuild /restore /t:Build arcgis-maps-sdk-dotnet-samples\src\UWP\ArcGIS.UWP.Viewer\ArcGIS.UWP.Viewer.csproj /p:Configuration=Release /p:Platform=ARM64

ECHO Generating WinUI store packages
msbuild /restore /t:Buildarcgis-maps-sdk-dotnet-samples\src\WinUI\ArcGIS.WinUI.Viewer\ArcGIS.WinUI.Viewer.csproj /p:Configuration=Release /p:Platform=x86 /p:GenerateAppxPackageOnBuild=true
msbuild /restore /t:Build arcgis-maps-sdk-dotnet-samples\src\WinUI\ArcGIS.WinUI.Viewer\ArcGIS.WinUI.Viewer.csproj /p:Configuration=Release /p:Platform=x64 /p:GenerateAppxPackageOnBuild=true
msbuild /restore /t:Build arcgis-maps-sdk-dotnet-samples\src\WinUI\ArcGIS.WinUI.Viewer\ArcGIS.WinUI.Viewer.csproj /p:Configuration=Release /p:Platform=ARM64 /p:GenerateAppxPackageOnBuild=true

ECHO Generating .NET MAUI Windows Packages
dotnet publish arcgis-maps-sdk-dotnet-samples\src\MAUI\Maui.Samples\ArcGIS.Samples.Maui.csproj -f net8.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifierOverride=win10-x86
dotnet publish arcgis-maps-sdk-dotnet-samples\src\MAUI\Maui.Samples\ArcGIS.Samples.Maui.csproj -f net8.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifierOverride=win10-x64
dotnet publish arcgis-maps-sdk-dotnet-samples\src\MAUI\Maui.Samples\ArcGIS.Samples.Maui.csproj -f net8.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifierOverride=win10-arm64

ECHO Generating .NET MAUI Android Package
dotnet publish arcgis-maps-sdk-dotnet-samples\src\MAUI\Maui.Samples\ArcGIS.Samples.Maui.csproj -f net8.0-android -c Release