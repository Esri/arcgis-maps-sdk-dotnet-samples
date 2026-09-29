#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$PackageArchive,
    [Parameter(Mandatory)]
    [uri]$DailySource,
    [string]$DailySourceName = 'Artifactory'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (!$IsWindows) { throw 'This bootstrap script requires Windows and the .NET 10 Android workload.' }
if (!$DailySource.IsAbsoluteUri -or $DailySource.Scheme -ne 'https' -or
    $DailySource.UserInfo -or $DailySource.Query -or $DailySource.Fragment) {
    throw 'DailySource must be an HTTPS feed URL without embedded credentials, query parameters, or fragments.'
}
if ([string]::IsNullOrWhiteSpace($DailySourceName) -or $DailySourceName -in @('VulkanLocal', 'nuget.org')) {
    throw 'DailySourceName must be the configured credential source name, distinct from VulkanLocal and nuget.org.'
}

$archivePath = (Resolve-Path -LiteralPath $PackageArchive).ProviderPath
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$sdkVersion = '300.2.0-vulkan.20260922.1'
$dailyVersion = '300.2.0-daily5088'
$packageHashes = [ordered]@{
    'Esri.ArcGISRuntime' = '12e4e1d3b043a6d84f80d2419d971b4fa7f49c9add76cceb45aadacf423d39de'
    'Esri.ArcGISRuntime.Android' = 'db698c149800ab5cb661c4c04223c99e59d6c8dbe32123482953da5129efa5a2'
    'Esri.ArcGISRuntime.Maui' = 'a51be305d6709f6111f6b60eefd582826e8317e2f15545d1247aa2c53120f340'
    'Esri.ArcGISRuntime.runtimes.Android' = '3f5f9a4b133d4294c19407e6b4ddef2c6ac623ebebb3f333753e687490ead877'
}

function Invoke-DotNet([string[]]$Arguments) {
    & dotnet @Arguments *>> $script:log
    if ($LASTEXITCODE -ne 0) {
        Get-Content -LiteralPath $script:log -Tail 40 | Write-Host
        throw "dotnet failed with exit code $LASTEXITCODE. See artifacts\vulkan-repro\build.log."
    }
}

function Get-EntryHash($Entry) {
    if ($null -eq $Entry) { throw 'Required archive entry is missing.' }
    $stream = $Entry.Open()
    try { [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)).ToLowerInvariant() }
    finally { $stream.Dispose() }
}

# Android resource tools still require short paths, even with Windows long paths enabled.
$drive = @('V', 'W', 'X', 'Y', 'Z', 'U', 'T') |
    Where-Object { !(Test-Path "${_}:\") } | Select-Object -First 1
if (!$drive) { throw 'No free temporary drive letter. Free one of T: through Z: and retry.' }
& subst "${drive}:" $root
if ($LASTEXITCODE -ne 0) { throw 'Could not map the checkout to a short path.' }
$locationPushed = $false
$configCreated = $false
$configPath = Join-Path $root 'NuGet.Config'
try {
    Push-Location "${drive}:\"
    $locationPushed = $true
    $output = Join-Path (Get-Location).Path 'artifacts\vulkan-repro'
    $feed = Join-Path $output 'feed'
    $cache = Join-Path $output 'cache'
    New-Item -ItemType Directory -Force $feed, $cache | Out-Null
    $script:log = Join-Path $output 'build.log'
    Set-Content -LiteralPath $script:log -Value 'Vulkan bundle reproduction build'

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead($archivePath)
    try {
        $packages = @($zip.Entries | Where-Object FullName -Like '*.nupkg')
        if ($packages.Count -ne $packageHashes.Count) { throw 'Expected exactly four SDK packages in the bundle.' }
        foreach ($id in $packageHashes.Keys) {
            $file = "$id.$sdkVersion.nupkg"
            $entries = @($packages | Where-Object FullName -EQ "packages/$file")
            if ($entries.Count -ne 1) { throw "Expected one packages/$file entry." }
            if ((Get-EntryHash $entries[0]) -ne $packageHashes[$id]) { throw "Bundle hash mismatch: $file" }
            [IO.Compression.ZipFileExtensions]::ExtractToFile($entries[0], (Join-Path $feed $file), $true)
        }
    }
    finally { $zip.Dispose() }

    $project = 'src\MAUI\Maui.Samples\ArcGIS.Samples.Maui.csproj'
    $properties = @(
        '-p:VulkanRepro=true'
        '-p:EnableVulkanRendering=true'
        '-p:RuntimeIdentifier=android-arm64'
        '-p:EmbedAssembliesIntoApk=true'
        '-p:ApplicationId=com.esri.arcgisruntime.samples.maui.vulkan5088'
        '-p:ApplicationTitle=ArcGIS Samples Vulkan 5088'
        "-p:RestorePackagesPath=$cache"
    )
    # Keep credential inheritance while limiting sources to this checkout for this command.
    $sourceName = [Security.SecurityElement]::Escape($DailySourceName)
    $sourceUrl = [Security.SecurityElement]::Escape($DailySource.AbsoluteUri)
    $localFeed = [Security.SecurityElement]::Escape($feed)
    $localPatterns = ($packageHashes.Keys | ForEach-Object { "<package pattern=`"$_`" />" }) -join ''
    $config = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="VulkanLocal" value="$localFeed" />
    <add key="$sourceName" value="$sourceUrl" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <disabledPackageSources><clear /></disabledPackageSources>
  <packageSourceMapping>
    <clear />
    <packageSource key="VulkanLocal">$localPatterns</packageSource>
    <packageSource key="$sourceName"><package pattern="Esri.ArcGISRuntime.*" /></packageSource>
    <packageSource key="nuget.org"><package pattern="*" /></packageSource>
  </packageSourceMapping>
</configuration>
"@
    $configStream = [IO.File]::Open($configPath, [IO.FileMode]::CreateNew, [IO.FileAccess]::Write)
    $configCreated = $true
    try {
        $bytes = [Text.Encoding]::UTF8.GetBytes($config)
        $configStream.Write($bytes, 0, $bytes.Length)
    }
    finally { $configStream.Dispose() }
    Invoke-DotNet (@('restore', $project, '--verbosity', 'quiet') + $properties)

    $assets = Get-Content 'src\MAUI\Maui.Samples\obj\project.assets.json' -Raw | ConvertFrom-Json -AsHashtable
    $expected = [ordered]@{}
    foreach ($id in $packageHashes.Keys) { $expected[$id] = $sdkVersion }
    $expected['Esri.ArcGISRuntime.Toolkit.Maui'] = $dailyVersion
    $expected['Esri.ArcGISRuntime.Hydrography'] = $dailyVersion
    foreach ($id in $expected.Keys) {
        if (!$assets.libraries.ContainsKey("$id/$($expected[$id])")) { throw "Unexpected restored version: $id" }
    }
    foreach ($id in $packageHashes.Keys) {
        $cached = Join-Path $cache "$($id.ToLowerInvariant())\$sdkVersion\$($id.ToLowerInvariant()).$sdkVersion.nupkg"
        if ((Get-FileHash -LiteralPath $cached -Algorithm SHA256).Hash.ToLowerInvariant() -ne $packageHashes[$id]) {
            throw "Restored package does not match the ZIP: $id"
        }
    }
    Invoke-DotNet (@('build', $project, '--no-restore', '-c', 'Debug',
        '-f', 'net10.0-android', '--verbosity', 'quiet') + $properties)

    $buildOutput = 'src\MAUI\Maui.Samples\bin\Debug\net10.0-android\android-arm64'
    $dll = [IO.File]::ReadAllBytes((Join-Path (Get-Location).Path "$buildOutput\ArcGIS.dll"))
    $switch = 'Switch.Esri.ArcGISRuntime.EnableVulkanRendering'
    # Metadata strings can start at either byte alignment.
    if (![Text.Encoding]::Unicode.GetString($dll).Contains($switch) -and
        ![Text.Encoding]::Unicode.GetString($dll, 1, $dll.Length - 1).Contains($switch)) {
        throw 'The compiled app is missing the Vulkan opt-in switch.'
    }
    $apkPath = Join-Path (Get-Location).Path "$buildOutput\com.esri.arcgisruntime.samples.maui.vulkan5088-Signed.apk"
    $apk = [IO.Compression.ZipFile]::OpenRead($apkPath)
    try {
        $shaderCount = @($apk.Entries | Where-Object FullName -Like 'assets/arcgisruntime/shaders/*.spv').Count
        if ($shaderCount -ne 417) { throw "Expected 417 Vulkan shaders; found $shaderCount." }
        $nativeHashes = [ordered]@{
            'lib/arm64-v8a/libruntimecore.so' = 'fdf19171ce049b03d6c2d9f88989083675a5b0861e6392c097499bef18f7653c'
            'lib/arm64-v8a/libRuntimeCoreNet.so' = '3f12a6bafcb6a631166f1849ce6491c944a94b0ca80a24b9947ecb88f95112bd'
        }
        foreach ($name in $nativeHashes.Keys) {
            if ((Get-EntryHash $apk.GetEntry($name)) -ne $nativeHashes[$name]) { throw "Wrong native binary: $name" }
        }
        foreach ($name in @('ArcGIS', 'Esri.ArcGISRuntime', 'Esri.ArcGISRuntime.Android', 'Esri.ArcGISRuntime.Maui')) {
            if (!$apk.GetEntry("lib/arm64-v8a/lib_$name.dll.so")) { throw "Missing embedded assembly: $name" }
        }
    }
    finally { $apk.Dispose() }
    $destination = Join-Path $output 'ArcGIS-Samples-Vulkan-5088.apk'
    Copy-Item -LiteralPath $apkPath -Destination $destination
    [ordered]@{
        Packages = $expected
        NativeDaily = $dailyVersion
        ShaderCount = $shaderCount
        NativeHashes = $nativeHashes
        ApkSHA256 = (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash
        VulkanEnabled = $true
        ApplicationId = 'com.esri.arcgisruntime.samples.maui.vulkan5088'
        DeviceTested = $false
    } | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $output 'validation.json')
    Write-Host "Built and verified: $root\artifacts\vulkan-repro\ArcGIS-Samples-Vulkan-5088.apk"
    Write-Host 'No device was changed. See tools\VulkanRepro\README.md for installation and reproduction steps.'
}
finally {
    if ($configCreated) { Remove-Item -LiteralPath $configPath }
    if ($locationPushed) { Pop-Location }
    & subst "${drive}:" /D
    if ($LASTEXITCODE -ne 0) { Write-Warning "Could not remove temporary drive ${drive}:." }
}
