# ArcGIS Runtime SDK for .NET samples

This project contains samples for the ArcGIS Runtime SDK for .NET including WPF, UWP and Xamarin platforms.

## Samples tables of contents

See each platform's TOC:

* [WPF](src/WPF)
* [Universal Windows Platform](src/UWP)
* [Xamarin.Android](src/Android)
* [Xamarin.iOS](src/iOS)
* [Xamarin.Forms](src/Forms)
* [WinUI](src/WinUI) (Preview)

## Instructions

1. Fork and then clone the repo or download the .zip file.
2. Confirm the supported system configuration for the API of interest in the ArcGIS Runtime SDK for .NET:
    * [System requirements for ArcGIS Runtime API for .NET](https://developers.arcgis.com/net/reference/system-requirements/)
3. For the platform you want to view: open the solution, restore NuGet packages, build, and run the application
    * WPF (.NET Framework): `src\WPF\ArcGISRuntime.WPF.Viewer.sln`
    * WPF (.NET Core): `src\WPF\ArcGISRuntime.WPF.Viewer.NetCore.sln`
    * UWP: `src\Windows\ArcGISRuntime.UWP.Viewer.sln`  
    * Xamarin.Android: `src\Android\ArcGISRuntime.Xamarin.Samples.Android.sln`  
    * Xamarin.iOS: `src\iOS\ArcGISRuntime.Xamari.Samples.iOS.sln`  
    * Xamarin.Forms: `src\Windows\ArcGISRuntime.Xamarin.Samples.Forms.sln`  
    * WinUI: `\src\WinUI\ArcGISRuntime.WinUI.Viewer.sln` (Preview)

    or
  
    * All: `src\ArcGISRuntime.Viewers.All.sln`
    * Windows ( WPF / UWP ): `src\ArcGISRuntime.Viewers.Windows.sln`
    * Xamarin (iOS, Android, Forms): `src\ArcGISRuntime.Viewers.Xamarin.sln`  

Notes:

* Authentication: Use of Esri location services, including basemaps and geocoding, requires either an ArcGIS identity or an API key.
For more information see https://links.esri.com/arcgis-runtime-security-auth.
The .NET sample viewers have a prompt for setting an API key. You can also hardcode your API key in the [`GetLocalKey() method`](https://github.com/Esri/arcgis-runtime-samples-dotnet/tree/main/src/ArcGISRuntime.Samples.Shared/Managers/ApiKeyManager.cs#L89) of the [`ApiKeyManager class`](https://github.com/Esri/arcgis-runtime-samples-dotnet/tree/main/src/ArcGISRuntime.Samples.Shared/Managers/ApiKeyManager.cs).

* When compiling Universal Windows Platform samples, make sure that you are compiling against x86/x64/ARM platform and not using AnyCPU.

## Requirements

* [Supported system configurations for ArcGIS Runtime API for .NET](https://developers.arcgis.com/net/reference/system-requirements/)

## Resources

* [ArcGIS Runtime SDK for .NET](http://esriurl.com/dotnetsdk)

## Offline data

Several samples require local data to function properly. That data is downloaded to local storage automatically at runtime. 
This process is handled by the `DataManager` class (located in the 'Managers' folder in each view project). Samples
that use the data manager to download their data are differentiated as follows:

* They have `RequiresOfflineData` set to true in their metadata.json files
* They have one or more entries under `DataItemIds` in their metadata.json files (these are portal item Ids)
* They use the data manager to identify the correct path for their offline files at run time

See the [contribution guidelines](https://github.com/Esri/arcgis-runtime-samples-dotnet/wiki/Contributing) for more detailed information.

## Issues

Find a bug or want to request a new feature?  Please let us know by submitting an issue.

## Contributing

Anyone and everyone is welcome to [contribute](https://github.com/Esri/arcgis-runtime-samples-dotnet/wiki/Contributing).

## Tools

Esri uses several tools to more efficiently manage the content in this repo. See [Tools](tools/readme.md) for more information.

## Licensing

Copyright 2021 Esri

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

https://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

A copy of the license is available in the repository's [license.txt](/license.txt) file.
