@echo on
cls
echo "Starting Build Checks"
cd %~dp0
SET Samples_dir=%~dp0..\src
:: remove output folder if exists. Since we wipe out the workspace we don't have to remove the output folder. Removing just to be safe. 
:: and just need to create
rmdir %WORKSPACE%\output /S /Q
mkdir %WORKSPACE%\output
echo "Setting up Nuget config file"
ECHO SEARCHING FOR VISUAL STUDIO...
"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -version [17.0,18.0) -prerelease -requires Microsoft.Component.MSBuild -products * -property InstallationPath > %TEMP%\vsinstalldir.txt
SET /p _VSINSTALLDIR=<%TEMP%\vsinstalldir.txt
DEL %TEMP%\vsinstalldir.txt 
IF "%_VSINSTALLDIR%"=="" (
  ECHO ERROR: VISUAL STUDIO NOT FOUND
  SET ERRORLEVEL=1
  GOTO Quit
)
IF "%VSINSTALLDIR%"=="" (
  CALL "%_VSINSTALLDIR%\Common7\Tools\VsDevCmd.bat"
)  
IF '%RELEASE_VERSION%' == '' (
  SET RELEASE_VERSION=200.2.0
)
IF "%BUILD_NUM%"=="" (
  SET /p BUILD_NUM=<\\runtime\windows\api_dotnet\%RELEASE_VERSION%\daily_windows_api_OK.txt
  echo "BUILD_NUM was not set..pulling info from daily_win_DotNet_API_OK txt file"
)
IF "%CONFIGURATION%"=="" (
  SET CONFIGURATION=Release
)
SET WORKSPACE=%~dp0..
Set BuildOutDir=%WORKSPACE%\output
mkdir %WORKSPACE%\output\.NugetPackageCache
SET NuGetExePath=%WORKSPACE%\nuget.exe
IF NOT EXIST %NuGetExePath% (
  ECHO Downloading nuget.exe to %NuGetExePath%...
  powershell -ExecutionPolicy ByPass -command "[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe' -OutFile '%NuGetExePath%'"
)
SET NugetConfigFile=%BuildOutDir%\.NugetPackageCache\nuget.config
ECHO ^<configuration^>^<packageSources^>^<clear /^>^</packageSources^>^</configuration^> > "%NugetConfigFile%"
"%NuGetExePath%" config -SET repositoryPath=%BuildOutDir%\.NugetPackageCache -ConfigFile "%NugetConfigFile%"
"%NuGetExePath%" sources Add -Name LocalNugetOutput -Source "\\runtime\windows\api_dotnet\%RELEASE_VERSION%\%BUILD_NUM%\output\dotnet_AnyCPU_Release\NuGet" -ConfigFile "%NugetConfigFile%"
"%NuGetExePath%" sources Add -Name nuget.org -Source "https://api.nuget.org/v3/index.json" -ConfigFile "%NugetConfigFile%"
SET NUGETVERSION=%RELEASE_VERSION%
echo "Starting WinUI Build"
set PLATFORM=x64
set SAMPLES_PATH=WinUI\ArcGIS.WinUI.Viewer\ArcGIS.WinUI.Viewer.csproj
msbuild /restore /t:Build /p:Platform=%PLATFORM%;Configuration=%CONFIGURATION%;BuildUsingArcGISNuGetPackages=true;ArcGISNugetPackageVersion=%NUGETVERSION% "%Samples_dir%\%SAMPLES_PATH%" /p:OutDir=%BuildOutDir% /p:RestorePackagesPath=%BuildOutDir%\.NugetPackageCache /p:RestoreConfigFile=%BuildOutDir%\.NugetPackageCache\nuget.config
set PLATFORM=x86
set SAMPLES_PATH=WinUI\ArcGIS.WinUI.Viewer\ArcGIS.WinUI.Viewer.csproj
msbuild /restore /t:Build /p:Platform=%PLATFORM%;Configuration=%CONFIGURATION%;BuildUsingArcGISNuGetPackages=true;ArcGISNugetPackageVersion=%NUGETVERSION% "%Samples_dir%\%SAMPLES_PATH%" /p:OutDir=%BuildOutDir% /p:RestorePackagesPath=%BuildOutDir%\.NugetPackageCache /p:RestoreConfigFile=%BuildOutDir%\.NugetPackageCache\nuget.config
set PLATFORM=arm64
set SAMPLES_PATH=WinUI\ArcGIS.WinUI.Viewer\ArcGIS.WinUI.Viewer.csproj
msbuild /restore /t:Build /p:Platform=%PLATFORM%;Configuration=%CONFIGURATION%;BuildUsingArcGISNuGetPackages=true;ArcGISNugetPackageVersion=%NUGETVERSION% "%Samples_dir%\%SAMPLES_PATH%" /p:OutDir=%BuildOutDir% /p:RestorePackagesPath=%BuildOutDir%\.NugetPackageCache /p:RestoreConfigFile=%BuildOutDir%\.NugetPackageCache\nuget.config
echo "Starting WPF Build"
set PLATFORM=AnyCPU
set SAMPLES_PATH=WPF\WPF.Viewer\ArcGIS.WPF.Viewer.NetFramework.csproj
msbuild /restore /t:reBuild /p:Platform=%PLATFORM%;Configuration=%CONFIGURATION%;BuildUsingArcGISNuGetPackages=true;ArcGISNugetPackageVersion=%NUGETVERSION% "%Samples_dir%\%SAMPLES_PATH%" /p:OutDir=%BuildOutDir% /p:RestorePackagesPath=%BuildOutDir%\.NugetPackageCache /p:RestoreConfigFile=%BuildOutDir%\.NugetPackageCache\nuget.config
set PLATFORM=AnyCPU
set SAMPLES_PATH=WPF\WPF.Viewer\ArcGIS.WPF.Viewer.Net.csproj
msbuild /restore /t:reBuild /p:Platform=%PLATFORM%;Configuration=%CONFIGURATION%;BuildUsingArcGISNuGetPackages=true;ArcGISNugetPackageVersion=%NUGETVERSION% "%Samples_dir%\%SAMPLES_PATH%" /p:OutDir=%BuildOutDir% /p:RestorePackagesPath=%BuildOutDir%\.NugetPackageCache /p:RestoreConfigFile=%BuildOutDir%\.NugetPackageCache\nuget.config
echo "Starting UWP Build"
set PLATFORM=x64
set SAMPLES_PATH=UWP\ArcGIS.UWP.Viewer\ArcGIS.UWP.Viewer.csproj
msbuild /restore /t:reBuild /p:Platform=%PLATFORM%;Configuration=%CONFIGURATION%;BuildUsingArcGISNuGetPackages=true;ArcGISNugetPackageVersion=%NUGETVERSION% "%Samples_dir%\%SAMPLES_PATH%" /p:OutDir=%BuildOutDir% /p:RestorePackagesPath=%BuildOutDir%\.NugetPackageCache /p:RestoreConfigFile=%BuildOutDir%\.NugetPackageCache\nuget.config
set PLATFORM=x86
set SAMPLES_PATH=UWP\ArcGIS.UWP.Viewer\ArcGIS.UWP.Viewer.csproj
msbuild /restore /t:reBuild /p:Platform=%PLATFORM%;Configuration=%CONFIGURATION%;BuildUsingArcGISNuGetPackages=true;ArcGISNugetPackageVersion=%NUGETVERSION% "%Samples_dir%\%SAMPLES_PATH%" /p:OutDir=%BuildOutDir% /p:RestorePackagesPath=%BuildOutDir%\.NugetPackageCache /p:RestoreConfigFile=%BuildOutDir%\.NugetPackageCache\nuget.config
set PLATFORM=arm64
set SAMPLES_PATH=UWP\ArcGIS.UWP.Viewer\ArcGIS.UWP.Viewer.csproj
msbuild /restore /t:reBuild /p:Platform=%PLATFORM%;Configuration=%CONFIGURATION%;BuildUsingArcGISNuGetPackages=true;ArcGISNugetPackageVersion=%NUGETVERSION% "%Samples_dir%\%SAMPLES_PATH%" /p:OutDir=%BuildOutDir% /p:RestorePackagesPath=%BuildOutDir%\.NugetPackageCache /p:RestoreConfigFile=%BuildOutDir%\.NugetPackageCache\nuget.config
IF %ERRORLEVEL% NEQ 0 (
    ECHO "Build has failed..exiting.."
    GOTO Quit
)
echo "Process completed"
:Quit
@REM EXIT /B %ERRORLEVEL%