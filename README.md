# arcgis-runtime-samples-dotnet

This project contains samples for the ArcGIS Runtime SDK for .NET.

## Samples

Please see [Table of Contents](TableOfContents.md).

## Instructions 

1. Fork and then clone the repo or download the .zip file. 
2. Confirm the supported system configuration for the API of interest in the ArcGIS Runtime SDK for .NET:
  * [Windows Desktop](http://developers.arcgis.com/net/desktop/guide/system-requirements.htm)
  * [Universal Windows Platform]((http://developers.arcgis.com/net/uwp/guide/system-requirements.htm))
3. In Visual Studio, open the solution
  * Windows Desktop: `src\Desktop\ArcGISRuntime.Desktop.Viewer.sln`  
	   - Clean, build, and run the application.
  * Universal Windows Platform: `src\Windows\ArcGISRuntime.Windows.Viewer.sln`  
	   - Make sure that you are compiling against x86/x64/ARM platform and not using AnyCPU.
	   - Clean, build, and run the application.
  * Both: `src\ArcGISRuntime.Viewers.All.sln`  
	   - Make sure that you are compiling Windows project against x86/x64/ARM platform and not using AnyCPU.
	   - Clean, build, and run the application.

Notes:

Running sample viewer so that it uses VB samples
  * Desktop
       To run samples in VB, your sample viewer can be started with `/vb` parameter on start up. When the application is compiled, we also deploy `Launch Viewer VB.bat` file to the output folder.
       
   * Universal Windows Platform
      To run samples in VB, you need to manually change the constant in `App.cs` file. 
      
      ````CSharp
      private const Language SamplesLanguageUsed = Language.CSharp;
      // or 
      private const Language SamplesLanguageUsed = Language.VBnet;
      ````

## Requirements

* Supported system configurations for: 
  * [Windows Destkop](http://developers.arcgis.com/net/desktop/guide/system-requirements.htm)
  * [Universal Windows Platform]((http://developers.arcgis.com/net/uwp/guide/system-requirements.htm))

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
