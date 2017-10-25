// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System.Linq;
using System.IO;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using System.Threading.Tasks;

namespace ArcGISRuntime.WPF.Samples.OpenMobileMap
{
    public partial class OpenMobileMap
    {
        public OpenMobileMap()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Get the path to the mobile map package
            string filepath = await GetMmpkPath();

            // Open the map package
            MobileMapPackage myMapPackage = await MobileMapPackage.OpenAsync(filepath);

            // Check that there is at least one map
            if (myMapPackage.Maps.Count > 0)
            {
                // Display the first map in the package
                MyMapView.Map = myMapPackage.Maps.First();
            }
        }

        /// <summary>
        /// This abstracts away platform & sample viewer-specific code for accessing local files
        /// </summary>
        /// <returns>String that is the path to the file on disk</returns>
        private async Task<string> GetMmpkPath()
        {
            #region offlinedata

            // The mobile map package will be downloaded from ArcGIS Online
            // The data manager (a component of the sample viewer, *NOT* the runtime
            //     handles the offline data process

            // The desired MMPK is expected to be called Yellowstone.mmpk
            string filename = "Yellowstone.mmpk";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "OpenMobileMap", filename);

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the map package file
                await DataManager.GetData("e1f3a7254cb845b09450f54937c16061", "OpenMobileMap");
            }
            return filepath;

            #endregion offlinedata
        }
    }
}