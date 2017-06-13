// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.OpenMobileMapPackage
{
    public partial class OpenMobileMapPackage
    {
        public OpenMobileMapPackage()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Get path to the used data. This can be a mobile map package (.mmpk) file or 
                // an exploded mobile map package (folder that contains .info file).
                var dataPath = Path.Combine(DataManager.GetDataFolder(), "SampleData", "Open mobile map (map package)");
                
                // Open a local mobile map package
                MobileMapPackage myMobileMapPackage = await MobileMapPackage.OpenAsync(dataPath);

                // Find the first map from the package
                Map myMap = myMobileMapPackage.Maps.First();

                // Assign the map to the MapView
                MyMapView.Map = myMap;
            }
            catch (FileNotFoundException ex)
            {
                dataNotFoundMessage.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred. " + ex.ToString(), "Sample error");
            }
            finally
            {
                // Hide loading UI
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
    }
}