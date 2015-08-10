# arcgis-runtime-samples-dotnet

This project contains samples for the ArcGIS Runtime SDK for .NET - Desktop, Store, and Phone APIs.  Samples for each API are hosted in a simple application framework.  

## Instructions 

1. Fork and then clone the repo or download the .zip file. 
2. Confirm the supported system configuration for the API of interest in the ArcGIS Runtime SDK for .NET:
  * [Windows Desktop](http://developers.arcgis.com/net/desktop/guide/system-requirements.htm)
  * [Windows Phone](http://developers.arcgis.com/net/store/guide/system-requirements.htm)
  * [Windows Store](http://developers.arcgis.com/net/store/guide/system-requirements.htm) 
  * The [ArcGIS Runtime SDK for .NET](http://esriurl.com/dotnetsdk) is referenced using a NuGet package. It is automatically downloaded when the solution is built for the first time.

3. In Visual Studio, open the solution for the Desktop, Phone, or Store samples.   
  * Windows Desktop: src\Desktop\ArcGISRuntime.Samples.Desktop.sln  
	   - Clean, build, and run the application.
  * Windows Phone: src\Phone\ArcGISRuntime.Samples.Phone.sln  
	   - In Visual Studio go to Build > Configuration Manager. Select the appropriate active solution platform. If deploying to a device, use ARM. If deploying to the emulator, use x86.
	   - Clean, build, and run the application.
  * Windows Store: src\Store\ArcGISRuntime.Samples.Store.sln  
    - In Visual Studio go to Build > Configuration Manager. Select the appropriate active solution platform. If deploying to a Windows RT 8.1 device, use ARM. If deploying to a Windows 8.1 device (or simulator), select the appropriate processor type, x86 or x64.  
    - Clean, build, and run the application.
    
Optional : Open all platform solutions in Visual Studio using src\ArcGISRuntime.Samples.All.sln


Notes:

* Sample data for ArcGIS Runtime SDKs is included as a submodule with this repo: https://github.com/Esri/arcgis-runtime-samples-data. If downloading the zip or using a source control application that does not pull submodules from this repo, get the sample data from https://github.com/Esri/arcgis-runtime-samples-data directly.  Place sample data contents in a folder named "samples-data" at the same level as the "src" folder for this repo.
* The Windows Phone samples project contains a reference to the Nuget package for the [Windows Phone Toolkit](http://www.nuget.org/packages/WPtoolkit/). To enable download of Nuget packages on project build, in Visual Studio go to Tools > Options > Package Manager and check the box next to "Allow Nuget to download missing packages during build".  You can also choose to download and install the [October 2012 version of the Windows Phone Toolkit](http://phone.codeplex.com/). 

#### Optional: Change references to use the installed SDK instead of NuGet package
Some of the samples require either the SDK installation or deployment to work, which is not available with the NuGet deployment. If you have installed the full [ArcGIS Runtime SDK for .NET SDK](http://esriurl.com/dotnetsdk) and prefer to use it instead of using NuGet installation, follow the steps below: 

1. Remove / Uninstall Esri.ArcGISRuntime NuGet-package reference from the solution.
  * Click "Manage NuGet Packages for solution..." from Tools \ NuGet Package Manager
  * See Installed Packages tab and remove "Esri.ArcGISRuntime" package
2. [Add references](https://developers.arcgis.com/net/desktop/guide/add-arcgis-runtime-sdk-references.htm) to projects. 
3. Prepare deployment
	* Windows Desktop - Delete deployment folder (arcgisruntime10.2.5) from output folder if it exists. This step makes sure that centralized developer deployment is used and no deployment is needed
	* Windows Store / Windows Phone - create deployment following [guide](https://developers.arcgis.com/net/desktop/guide/deployment.htm) and make sure all extra capabilities are checked for deployment.

## Requirements

* Supported system configurations for: 
  * [Windows Destkop](http://developers.arcgis.com/net/desktop/guide/system-requirements.htm)
  * [Windows Phone](http://developers.arcgis.com/net/store/guide/system-requirements.htm)
  * [Windows Store](http://developers.arcgis.com/net/store/guide/system-requirements.htm)

## Resources

* [ArcGIS Runtime SDK for .NET](http://esriurl.com/dotnetsdk)

## Issues

Find a bug or want to request a new feature?  Please let us know by submitting an issue.

## Contributing

Anyone and everyone is welcome to [contribute] (https://github.com/Esri/arcgis-runtime-samples-dotnet/wiki/Contributing). 

## Licensing
Copyright 2014 Esri

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

[](Esri Tags: ArcGIS Runtime SDK Windows Desktop Store Phone C-Sharp C# XAML)
[](Esri Language: DotNet)
