# arcgis-runtime-samples-dotnet

This project contains samples for the ArcGIS Runtime SDK for .NET including WPF, UWP and Xamarin platforms.

## Samples - Table of Contents

See Table of Contents under different platforms
  * [WPF](src/WPF)
  * [Universal Windows Platform](src/UWP)
  * [Xamarin.Android](src/Android)
  * [Xamarin.iOS](src/iOS)
  * [Xamarin.Forms](src/Forms) 

## Instructions 

1. Fork and then clone the repo or download the .zip file. 
2. Confirm the supported system configuration for the API of interest in the ArcGIS Runtime SDK for .NET:
  * [WPF](https://developers.arcgis.com/net/latest/wpf/guide/system-requirements.htm)
  * [UWP](https://developers.arcgis.com/net/latest/uwp/guide/system-requirements.htm)
  * [Xamarin.Android](https://developers.arcgis.com/net/latest/android/guide/system-requirements.htm)
  * [Xamarin.iOS](https://developers.arcgis.com/net/latest/ios/guide/system-requirements.htm)
  * [Xamarin.Forms](https://developers.arcgis.com/net/latest/forms/guide/system-requirements.htm) 
3. For the platform you want to view: open the solution, restore NuGet packages, build, and run the application
  * WPF: `src\Desktop\ArcGISRuntime.WPF.Viewer.sln`  
  * UWP: `src\Windows\ArcGISRuntime.UWP.Viewer.sln`  
  * Xamarin.Android: `src\Android\ArcGISRuntime.Xamarin.Samples.Android.sln`  
  * Xamarin.iOS: `src\iOS\ArcGISRuntime.Xamari.Samples.iOS.sln`  
  * Xamarin.Forms: `src\Windows\ArcGISRuntime.Xamarin.Samples.Forms.sln`  
  
  or
  
  * All: `src\ArcGISRuntime.Viewers.All.sln`
  * Windows ( WPF / UWP ): `src\ArcGISRuntime.Viewers.Windows.sln`
  * Xamarin (iOS, Android, Forms): `src\ArcGISRuntime.Viewers.Xamarin.sln`  
  
Notes:

Running sample viewer for WPF / UWP so that it uses VB samples
  * WPF
       To run samples in VB, your sample viewer can be started with `/vb` parameter on start up. When the application is compiled, we also deploy `Launch Viewer VB.bat` file to the output folder.
       
   * Universal Windows Platform
      To run samples in VB, you need to manually change the constant in `App.cs` file. 
      
      ````CSharp
      private const Language SamplesLanguageUsed = Language.CSharp;
      // or 
      private const Language SamplesLanguageUsed = Language.VBnet;
      ````
When compiling Universal Windows Platform samples, make sure that you are compiling against x86/x64/ARM platform and not using AnyCPU.
	  
## Requirements

* Supported system configurations for: 
  * [WPF](https://developers.arcgis.com/net/latest/wpf/guide/system-requirements.htm)
  * [UWP](https://developers.arcgis.com/net/latest/uwp/guide/system-requirements.htm)
  * [Xamarin.Android](https://developers.arcgis.com/net/latest/android/guide/system-requirements.htm)
  * [Xamarin.iOS](https://developers.arcgis.com/net/latest/ios/guide/system-requirements.htm)
  * [Xamarin.Forms](https://developers.arcgis.com/net/latest/forms/guide/system-requirements.htm) 

## Resources

* [ArcGIS Runtime SDK for .NET](http://esriurl.com/dotnetsdk)

## Offline Data

Several samples require local data to function properly. That data is downloaded to local storage automatically at runtime. 
This process is handled by the `DataManager` class (located in the 'Managers' folder in each view project). Samples
that use the data manager to download their data are differentiated as follows:

* They have `RequiresOfflineData` set to true in their metadata.json files
* They have one or more entries under `DataItemIds` in their metadata.json files (these are portal item Ids)
* They use the data manager to identify the correct path for their offline files at run time

See the [contribution guidelines] (https://github.com/Esri/arcgis-runtime-samples-dotnet/wiki/Contributing) for more detailed information.

## Issues

Find a bug or want to request a new feature?  Please let us know by submitting an issue.

## Contributing

Anyone and everyone is welcome to [contribute] (https://github.com/Esri/arcgis-runtime-samples-dotnet/wiki/Contributing). 

## Licensing
Copyright 2016 Esri

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

A copy of the license is available in the repository's [license.txt](/license.txt) file.

### Modern UI Icons
Icons included under the Assets folder in the Phone and Store projects are from [Modern UI Icons](http://modernuiicons.com/). License information can be found at https://github.com/Templarian/WindowsIcons/blob/master/WindowsPhone/license.txt 
