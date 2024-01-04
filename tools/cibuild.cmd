REM @ECHO OFF
ECHO SEARCHING FOR VISUAL STUDIO...
"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -prerelease -version [17.8,18.0) -sort -requires Microsoft.Component.MSBuild -products * -property InstallationPath > %TEMP%\vsinstalldir.txt
SET /p _VSINSTALLDIR=<%TEMP%\vsinstalldir.txt
DEL %TEMP%\vsinstalldir.txt	
IF "%_VSINSTALLDIR%"=="" (
  ECHO ERROR: VISUAL STUDIO NOT FOUND
  EXIT /B 1
)
IF "%VSINSTALLDIR%"=="" (
  CALL "%_VSINSTALLDIR%\Common7\Tools\VsDevCmd.bat"
)

IF "%RELEASE_VERSION%" == "" (
  SET RELEASE_VERSION=200.3.0
)
SET ArcGISMapsSDKVersion=%RELEASE_VERSION%

REM Configure NuGet
dotnet new nugetconfig --force -o ../
IF "%NUGET_REPO%" NEQ "" AND EXIST %NUGET_REPO% (
dotnet nuget add source %NUGET_REPO%
)
SET NUGET_PACKAGES=%~dp0..\output\.nuget\packages
SET NUGET_HTTP_CACHE_PATH=%~dp0..\output\.nuget\cache
md %NUGET_PACKAGES%
md %NUGET_HTTP_CACHE_PATH%

REM BUILD

ECHO Building WPF .NET Framework
msbuild /restore /t:Build %~dp0..\src\WPF\WPF.Viewer\ArcGIS.WPF.Viewer.Net.csproj /p:Configuration=Release /p:TargetFramework=net472

ECHO Generating WPF Store packages
powershell -Command "(gc %~dp0..\src\WPF\WPF.Viewer\ArcGIS.WPF.Viewer.Net.csproj) -replace '<TargetFrameworks>net8.0-windows10.0.19041.0;net472</TargetFrameworks>', '<TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>' | sc %~dp0..\src\WPF\WPF.Viewer\ArcGIS.WPF.Viewer.Net.csproj"
powershell -Command "(gc %~dp0..\src\WPF\WPF.StorePackage\Package.appxmanifest) -replace 'Version=""200.0.0.0\""', 'Version="\"%RELEASE_VERSION%.%BUILD_NUM%\""' | sc %~dp0..\src\WPF\WPF.StorePackage\Package.appxmanifest"
msbuild /restore /t:Build %~dp0..\src\WPF\WPF.Viewer\ArcGIS.WPF.Viewer.Net.csproj /p:Configuration=Release
msbuild /restore /t:Build %~dp0..\src\WPF\WPF.StorePackage\ArcGIS.WPF.StorePackage.wapproj /p:Configuration=Release /p:Platform=x86 /p:PackageCertificateKeyFile=%PFXSignatureFile% /p:PackageCertificatePassword=%PFXSignaturePassword% /p:PackageCertificateThumbprint=%PackageCertificateThumbprint% 
msbuild /restore /t:Build %~dp0..\src\WPF\WPF.StorePackage\ArcGIS.WPF.StorePackage.wapproj /p:Configuration=Release /p:Platform=x64 /p:PackageCertificateKeyFile=%PFXSignatureFile% /p:PackageCertificatePassword=%PFXSignaturePassword% /p:PackageCertificateThumbprint=%PackageCertificateThumbprint% 
msbuild /restore /t:Build %~dp0..\src\WPF\WPF.StorePackage\ArcGIS.WPF.StorePackage.wapproj /p:Configuration=Release /p:Platform=ARM64 /p:PackageCertificateKeyFile=%PFXSignatureFile% /p:PackageCertificatePassword=%PFXSignaturePassword% /p:PackageCertificateThumbprint=%PackageCertificateThumbprint% 
powershell -Command "(gc %~dp0..\src\WPF\WPF.StorePackage\Package.appxmanifest) -replace 'Version=""%RELEASE_VERSION%.%BUILD_NUM%\""', 'Version="\"200.0.0.0\""' | sc %~dp0..\src\WPF\WPF.StorePackage\Package.appxmanifest"
powershell -Command "(gc %~dp0..\src\WPF\WPF.Viewer\ArcGIS.WPF.Viewer.Net.csproj) -replace '<TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>', '<TargetFrameworks>net8.0-windows10.0.19041.0;net472</TargetFrameworks>' | sc %~dp0..\src\WPF\WPF.Viewer\ArcGIS.WPF.Viewer.Net.csproj"

ECHO Generating UWP store packages
msbuild /restore /t:Build %~dp0..\src\UWP\ArcGIS.UWP.Viewer\ArcGIS.UWP.Viewer.csproj /p:Configuration=Release /p:Platform=x86 /p:PackageCertificateKeyFile=%PFXSignatureFile% /p:PackageCertificatePassword=%PFXSignaturePassword%
msbuild /restore /t:Build %~dp0..\src\UWP\ArcGIS.UWP.Viewer\ArcGIS.UWP.Viewer.csproj /p:Configuration=Release /p:Platform=x64 /p:PackageCertificateKeyFile=%PFXSignatureFile% /p:PackageCertificatePassword=%PFXSignaturePassword%
msbuild /restore /t:Build %~dp0..\src\UWP\ArcGIS.UWP.Viewer\ArcGIS.UWP.Viewer.csproj /p:Configuration=Release /p:Platform=ARM64 /p:PackageCertificateKeyFile=%PFXSignatureFile% /p:PackageCertificatePassword=%PFXSignaturePassword%

ECHO Generating WinUI store packages
powershell -Command "(gc %~dp0..\src\WinUI\ArcGIS.WinUI.Viewer\Package.appxmanifest) -replace 'Version=""1.0.0.0\""', 'Version="\"%RELEASE_VERSION%.%BUILD_NUM%\""' | sc %~dp0..\src\WinUI\ArcGIS.WinUI.Viewer\Package.appxmanifest"
msbuild /restore /t:Build %~dp0..\src\WinUI\ArcGIS.WinUI.Viewer\ArcGIS.WinUI.Viewer.csproj /p:Configuration=Release /p:Platform=x86 /p:GenerateAppxPackageOnBuild=true /p:PackageCertificateKeyFile=%PFXSignatureFile% /p:PackageCertificatePassword=%PFXSignaturePassword% /p:PackageCertificateThumbprint=%PackageCertificateThumbprint% /p:AppxAutoIncrementPackageRevision=false
msbuild /restore /t:Build %~dp0..\src\WinUI\ArcGIS.WinUI.Viewer\ArcGIS.WinUI.Viewer.csproj /p:Configuration=Release /p:Platform=x64 /p:GenerateAppxPackageOnBuild=true /p:PackageCertificateKeyFile=%PFXSignatureFile% /p:PackageCertificatePassword=%PFXSignaturePassword% /p:PackageCertificateThumbprint=%PackageCertificateThumbprint% /p:AppxAutoIncrementPackageRevision=false
msbuild /restore /t:Build %~dp0..\src\WinUI\ArcGIS.WinUI.Viewer\ArcGIS.WinUI.Viewer.csproj /p:Configuration=Release /p:Platform=ARM64 /p:GenerateAppxPackageOnBuild=true /p:PackageCertificateKeyFile=%PFXSignatureFile% /p:PackageCertificatePassword=%PFXSignaturePassword% /p:PackageCertificateThumbprint=%PackageCertificateThumbprint% /p:AppxAutoIncrementPackageRevision=false
powershell -Command "(gc %~dp0..\src\WinUI\ArcGIS.WinUI.Viewer\Package.appxmanifest) -replace 'Version=""%RELEASE_VERSION%.%BUILD_NUM%\""', 'Version="\"1.0.0.0\""' | sc %~dp0..\src\WinUI\ArcGIS.WinUI.Viewer\Package.appxmanifest"

ECHO Generating .NET MAUI Windows Packages
dotnet publish %~dp0..\src\MAUI\Maui.Samples\ArcGIS.Samples.Maui.csproj -f net8.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifier=win10-x64 -p:RuntimeIdentifierOverride=win10-x64 -p:UseRidGraph=true -p:PackageCertificateKeyFile=%PFXSignatureFile% -p:PackageCertificatePassword=%PFXSignaturePassword% -p:ApplicationDisplayVersion=%RELEASE_VERSION% -p:ApplicationVersion=%BUILD_NUM% 
dotnet publish %~dp0..\src\MAUI\Maui.Samples\ArcGIS.Samples.Maui.csproj -f net8.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifier=win10-x86 -p:RuntimeIdentifierOverride=win10-x86 -p:UseRidGraph=true -p:PackageCertificateKeyFile=%PFXSignatureFile% -p:PackageCertificatePassword=%PFXSignaturePassword% -p:ApplicationDisplayVersion=%RELEASE_VERSION% -p:ApplicationVersion=%BUILD_NUM%
dotnet publish %~dp0..\src\MAUI\Maui.Samples\ArcGIS.Samples.Maui.csproj -f net8.0-windows10.0.19041.0 -c Release -p:RuntimeIdentifier=win10-arm64 -p:RuntimeIdentifierOverride=win10-arm64 -p:UseRidGraph=true -p:PackageCertificateKeyFile=%PFXSignatureFile% -p:PackageCertificatePassword=%PFXSignaturePassword% -p:ApplicationDisplayVersion=%RELEASE_VERSION% -p:ApplicationVersion=%BUILD_NUM%

ECHO Generating .NET MAUI Android Package
dotnet publish %~dp0..\src\MAUI\Maui.Samples\ArcGIS.Samples.Maui.csproj -f net8.0-android -c Release