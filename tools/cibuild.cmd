REM @ECHO OFF

REM Default environment variables
IF "%DOTNET_VERSION%" == "" (
  set DOTNET_VERSION=10.0.203
)
IF "%RELEASE_VERSION%" == "" (
  SET RELEASE_VERSION=300.1.0
)

REM Install the latest dotnet version if not already cached to the build machine
if "%DOTNET_CACHE_FOLDER%" == "" (
  set DOTNET_INSTALL_FOLDER=%WORKSPACE%\.dotnet
) ELSE (
  set DOTNET_INSTALL_FOLDER=%DOTNET_CACHE_FOLDER%\%DOTNET_VERSION%
)

if NOT EXIST "%DOTNET_INSTALL_FOLDER%\dotnet.exe" (
  curl -L https://dot.net/v1/dotnet-install.ps1 -o %WORKSPACE%\dotnet-install.ps1
  powershell -File %WORKSPACE%\dotnet-install.ps1 -version %DOTNET_VERSION% -InstallDir %DOTNET_INSTALL_FOLDER%
)

SET DOTNET_EXE=%DOTNET_INSTALL_FOLDER%\dotnet.exe
ECHO Installed dotnet at %DOTNET_EXE%

REM Configure NuGet
%DOTNET_EXE% new nugetconfig --force -o ../
IF "%NUGET_REPO%" NEQ "" IF EXIST "%NUGET_REPO%" (
%DOTNET_EXE% nuget add source %NUGET_REPO%
)
SET NUGET_PACKAGES=%~dp0..\.nuget\packages
SET NUGET_HTTP_CACHE_PATH=%~dp0..\.nuget\cache
md %NUGET_PACKAGES%
md %NUGET_HTTP_CACHE_PATH%

REM Install maui workload
%DOTNET_EXE% workload install maui --version %DOTNET_VERSION%

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
  IF "%ArcGISAdvancedEditingUserTypeLicenseKey%" NEQ "" (
    ECHO ArcGISAdvancedEditingUserTypeLicenseKey = "%ArcGISAdvancedEditingUserTypeLicenseKey%"; >>%licenseFile%
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

set PUBLISHER=%PUBLISHER:\"="%
%DOTNET_EXE% msbuild %~dp0GenerateApps.msbuild -t:BuildAll -p:BUILD_NUM=%BUILD_NUM% -p:RELEASE_VERSION=%RELEASE_VERSION% -p:PFXSignaturePassword=%PFXSignaturePassword% -p:PFXSignatureFile=%PFXSignatureFile% -p:PackageCertificateThumbprint=%PackageCertificateThumbprint% -p:KeyStoreFile=%KeyStoreFile% -p:KeyPass=%KeyPass% -p:KeyAlias=%KeyAlias% -v:diag
