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

echo "Starting WPF .NET Build"

msbuild /restore /t:reBuild /p:Platform=AnyCPU;Configuration=Release;BuildUsingArcGISNuGetPackages=true;ArcGISNugetPackageVersion=%NUGETVERSION% "%Samples_dir%\WPF\WPF.Viewer\ArcGIS.WPF.Viewer.NET.csproj" /p:OutDir=%BuildOutDir% /p:RestorePackagesPath=%BuildOutDir%\.NugetPackageCache /p:RestoreConfigFile=%BuildOutDir%\.NugetPackageCache\nuget.config

IF %ERRORLEVEL% NEQ 0 (
    ECHO "Build has failed..exiting.."
    GOTO Quit
)

echo "Process completed"
:Quit
EXIT /B %ERRORLEVEL%