# Vulkan Android reproduction: native daily 5088

This branch uses existing MAUI samples to compare MapView, global SceneView, and
LocalSceneView rendering with one exact experimental Vulkan bundle. It is a
reproduction setup, not a rendering fix.

## Exact packages

| Package | Version | Source |
| --- | --- | --- |
| Esri.ArcGISRuntime | 300.2.0-vulkan.20260922.1 | Private ZIP |
| Esri.ArcGISRuntime.Android | 300.2.0-vulkan.20260922.1 | Private ZIP |
| Esri.ArcGISRuntime.Maui | 300.2.0-vulkan.20260922.1 | Private ZIP |
| Esri.ArcGISRuntime.runtimes.Android | 300.2.0-vulkan.20260922.1 | Private ZIP |
| Esri.ArcGISRuntime.Toolkit.Maui | 300.2.0-daily5088 | Authorized daily feed |
| Esri.ArcGISRuntime.Hydrography | 300.2.0-daily5088 | Authorized daily feed |

The four ZIP packages contain Android-only managed assets and ARM64 native
libraries from **daily 5088 (September 15, 2026)**. The bundle includes 417 Vulkan
shaders. Toolkit and Hydrography are not in the ZIP and must resolve separately.
`-p:VulkanRepro=true` selects these exact versions in `src\Directory.Packages.props`.
Normal builds without this property retain their normal package versions.

This is **not** the historical August 27 daily 5069 SDK backport or the September
28 daily 5101 build. Do not substitute either package set under the same version.

## Checkout and bootstrap

Prerequisites: Windows, PowerShell 7, .NET 10 SDK, the Android/MAUI build workloads,
Android SDK 36, and a physical ARM64 Android device with a suitable Vulkan driver.
Only Android is restored/built in this mode; x64 emulators are not supported.

```powershell
git clone --branch prav/sdk-branch-packages --single-branch `
  https://github.com/Esri/arcgis-maps-sdk-dotnet-samples.git
Set-Location arcgis-maps-sdk-dotnet-samples
```

Obtain **`Vulkan.Android.300.2.0-vulkan.20260922.1.zip`** from its owner through an
authorized private channel. It is not included in this repository and is not
available from nuget.org. Obtain the HTTPS daily feed URL and authenticate using
your existing NuGet configuration or credential provider. Do not put credentials
in the URL or this repository.

```powershell
pwsh -File .\tools\VulkanRepro\Build.ps1 `
  -PackageArchive 'C:\Downloads\Vulkan.Android.300.2.0-vulkan.20260922.1.zip' `
  -DailySource 'https://YOUR-AUTHORIZED-FEED/v3/index.json' `
  -DailySourceName 'Artifactory'
```

Replace the archive path and feed URL with your actual values. `DailySourceName`
must match the source name used by your existing NuGet credentials (default:
`Artifactory`).
The script verifies the four package hashes, imports only those packages into
`artifacts\vulkan-repro\feed`, and restores into an isolated cache under
`artifacts\vulkan-repro\cache`. A temporary checkout-root `NuGet.Config` maps the
four SDK packages exclusively to the local feed, other ArcGIS Runtime packages to
the supplied daily feed, and remaining dependencies to nuget.org. It inherits
existing credentials without copying them and is removed on completion; the script
refuses to overwrite an existing checkout-root config. Do not run concurrent builds
in this checkout. No user-global NuGet configuration is changed. A temporary free
drive letter avoids Windows Android resource path-length failures and is removed
on completion.

The build enables Vulkan before the first GeoView use, embeds managed assemblies,
and produces a standalone Debug APK. It checks the restored package versions and
ZIP hashes, the Vulkan switch, embedded assemblies, 417 shader assets, and the
native library hashes. Outputs (all ignored by Git):

- `artifacts\vulkan-repro\ArcGIS-Samples-Vulkan-5088.apk`
- `artifacts\vulkan-repro\validation.json`
- `artifacts\vulkan-repro\build.log`

Package contents, SDK implementation source, native binaries, API keys, and feed
credentials must never be committed. The script does not install or launch anything.

## Install and run

Connect and unlock the device, enable USB debugging, and authorize the computer.
Replace `DEVICE_SERIAL` with the value from `adb devices -l`.

```powershell
adb devices -l
adb -s DEVICE_SERIAL install -r .\artifacts\vulkan-repro\ArcGIS-Samples-Vulkan-5088.apk
adb -s DEVICE_SERIAL shell monkey -p com.esri.arcgisruntime.samples.maui.vulkan5088 `
  -c android.intent.category.LAUNCHER 1
```

The app is named **ArcGIS Samples Vulkan 5088** and uses the separate application
ID `com.esri.arcgisruntime.samples.maui.vulkan5088`. Existing standard, 5069, and
5101 test apps/data remain untouched. If a prior installation of this exact ID
has a different signing key, do not uninstall it without backing up its data.

Enter your API key only in the app's settings prompt. Use a key authorized for the
basemap and scene services being tested. Never paste it into source, build
arguments, logs, screenshots, or a bug report.

## Reproduction and comparison

Keep Vulkan enabled for all three cases. Wait for each sample's content to load,
then repeat pans and pinch zooms, return to the sample list, and reopen it. Tap the
sample thumbnail/card to open it rather than only its description.

| Viewer | Sample to search | Code | What to compare |
| --- | --- | --- | --- |
| MapView (2D) | **Display map** | `src\MAUI\Maui.Samples\Samples\Map\DisplayMap` | Imagery tiles should align and remain stable during pans/zooms. Record displaced, duplicated, stale, or missing tiles. |
| SceneView (global 3D) | **Add 3d tiles layer** | `src\MAUI\Maui.Samples\Samples\Layers\Add3dTilesLayer` | Pan, zoom, rotate and tilt the global scene. Record tile placement, surface/3D-content alignment, visual corruption, and any crash. |
| LocalSceneView (local 3D) | **Display local scene** | `src\MAUI\Maui.Samples\Samples\Scene\DisplayLocalScene` | Compare the clipped local scene during pans/zooms and reopening against the global SceneView behavior. |

The last sample explicitly instantiates **`LocalSceneView`** in XAML and
**`Scene(SceneViewingMode.Local, ...)`** in code. It is a local-viewing-mode/tiling
comparison, **not** a test of offline scene packages. The sample still loads online
basemap, elevation, and scene services. These samples use different data/extents,
so this is a viewer comparison rather than a controlled same-dataset benchmark.

Capture device model/OS/GPU, exact sample title, gesture sequence, screenshot or
recording (without credentials), and `validation.json`. Use process-filtered
logcat to confirm `Rendering backend: Vulkan` / Vulkan device creation and retain
any native crash backtrace. A successful build or launch alone does not establish
correct rendering; record the result for each viewer separately.
