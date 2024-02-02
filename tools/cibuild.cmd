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

IF "%BUILD_NUM%" == "" (
  SET BUILD_NUM=0
)

REM Configure NuGet
dotnet new nugetconfig --force -o ../
IF "%NUGET_REPO%" NEQ "" IF EXIST "%NUGET_REPO%" (
dotnet nuget add source %NUGET_REPO%
)
SET NUGET_PACKAGES=%~dp0..\.nuget\packages
SET NUGET_HTTP_CACHE_PATH=%~dp0..\.nuget\cache
md %NUGET_PACKAGES%
md %NUGET_HTTP_CACHE_PATH%

SET licenseFile=%~dp0..\src\Samples.Shared\Managers\LicenseStrings.generated.cs
IF "%ArcGISLicenseKey%" NEQ "" (
  REM Override LicenseKeys if available
  ECHO namespace ArcGIS.Samples.Shared.Managers ^{ >%licenseFile%
  ECHO internal static partial class LicenseStrings ^{ >>%licenseFile%
  ECHO static LicenseStrings^(^) ^{ >>%licenseFile%
  ECHO ArcGISLicenseKey = "%ArcGISLicenseKey%"; >>%licenseFile%
  IF "%ArcGISAnalysisLicenseKey%" NEQ "" (
    ECHO ArcGISAnalysisLicenseKey = "%ArcGISAnalysisLicenseKey%"; >>%licenseFile%
  )
  IF "%ArcGISUtilityNetworkLicenseKey%" NEQ "" (
    ECHO ArcGISUtilityNetworkLicenseKey = "%ArcGISUtilityNetworkLicenseKey%"; >>%licenseFile%
  )
  ECHO ^}^}^} >>%licenseFile%
)

SET keyFile=%~dp0..\src\Samples.Shared\Managers\ApiKeyManager.generated.cs
IF "%ARCGIS_API_KEY%" NEQ "" (
  ECHO namespace ArcGIS.Samples.Shared.Managers ^{ >%keyFile%
  ECHO public static partial class ApiKeyManager ^{ >>%keyFile%
  ECHO static ApiKeyManager^(^) ^{ >>%keyFile%
  ECHO Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = _key = "%ARCGIS_API_KEY%"; >>%keyFile%
  ECHO DisableApiKeyUI = true; >>%keyFile%
  ECHO ^}^}^} >>%keyFile%
)


msbuild /t:BuildAll %~dp0GenerateApps.msbuild /p:BUILD_NUM=%BUILD_NUM% /p:RELEASE_VERSION=%RELEASE_VERSION% /p:PUBLISHER=%PUBLISHER% /p:PFXSignaturePassword=%PFXSignaturePassword% /p:PFXSignatureFile=%PFXSignatureFile% /p:PackageCertificateThumbprint=%PackageCertificateThumbprint%

