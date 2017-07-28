// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System.Linq;
using Esri.ArcGISRuntime.Mapping;
using Xamarin.Forms;
using System;
using System.IO;
using System.Reflection;

namespace ArcGISRuntimeXamarin.Samples.OpenMobileMap
{
    public partial class OpenMobileMap : ContentPage
    {
        // Hold a reference to a Mobile Map Package
        private MobileMapPackage MyMapPackage;

        public OpenMobileMap()
        {
            InitializeComponent();

            Title = "Open mobile map (map package)";

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Because each platform handles resource embedding differently, 
            //     we take a three-part approach:
            //     1. Include the mobile map pack as an 'embedded resource'
            //     2. Copy the embedded resource (opened with a stream) to the platform-specific home directory
            //     3. Get the platform-specific file path

            // Compute the platform-specific details
            string mmpkName = "Yellowstone.mmpk";
#if NETFX_CORE //UWP
             var assembly = typeof(App).GetTypeInfo().Assembly;
             var folder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
             var resourcePrefix= "ArcGISRuntimeXamarin.";
#elif __IOS__
             var resourcePrefix = "ArcGISRuntimeXamarin.";
             var assembly = this.GetType().Assembly;
             var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif __ANDROID__
			var resourcePrefix = "ArcGISRuntimeXamarin.";
			var assembly = this.GetType().Assembly;
			var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
            // The path on disk for the file
			var path = Path.Combine(folder, mmpkName);

            // Copy the file to disk if it isn't already there
			if (!File.Exists(path))
			{
				var resourceName = resourcePrefix + "Resources.MobileMapPackages.Yellowstone.mmpk";
				using (var stream = assembly.GetManifestResourceStream(resourceName))
				{
					using (var mem = new MemoryStream())
					{
						stream.CopyTo(mem);
						File.WriteAllBytes(path, mem.ToArray());
					}
				}
			}

            // Load the Mobile Map Package from the path computed earlier
            MyMapPackage = await MobileMapPackage.OpenAsync(path);

            // Check that there is at least one map
            if (MyMapPackage.Maps.Count > 0)
            {
                // Display the first map in the package
                MyMapView.Map = MyMapPackage.Maps.First();
            }
        }
    }
}