// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrography;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.UWP.Samples.EncDisplaySettings
{
    public partial class EncDisplaySettings
    {
        public EncDisplaySettings()
        {
            InitializeComponent();

            // Setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Apply initial display settings
            UpdateDisplaySettings();

            // Initialize the map with an oceans basemap
            MyMapView.Map = new Map(Basemap.CreateOceans());

            // Get the path to the ENC Exchange Set
            string encPath = await GetEncPath();

            // Create the Exchange Set
            // Note: this constructor takes an array of paths because so that update sets can be loaded alongside base data
            EncExchangeSet _encExchangeSet = new EncExchangeSet(new string[] { encPath });

            // Wait for the exchange set to load
            await _encExchangeSet.LoadAsync();

            // Store a list of data set extent's - will be used to zoom the mapview to the full extent of the Exchange Set
            List<Envelope> dataSetExtents = new List<Envelope>();

            // Add each data set as a layer
            foreach (EncDataSet _encDataSet in _encExchangeSet.DataSets)
            {
                // Create the cell and layer
                EncLayer _encLayer = new EncLayer(new EncCell(_encDataSet));

                // Add the layer to the map
                MyMapView.Map.OperationalLayers.Add(_encLayer);

                // Wait for the layer to load
                await _encLayer.LoadAsync();

                // Add the extent to the list of extents
                dataSetExtents.Add(_encLayer.FullExtent);
            }

            // Use the geometry engine to compute the full extent of the ENC Exchange Set
            Envelope fullExtent = GeometryEngine.CombineExtents(dataSetExtents);

            // Set the viewpoint
            MyMapView.SetViewpoint(new Viewpoint(fullExtent));
        }

        private async Task<String> GetEncPath()
        {
            #region offlinedata

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "AddEncExchangeSet", "ExchangeSet", "ENC_ROOT", "CATALOG.031");

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the file
                await DataManager.GetData("9d3ddb20afe3409eae25b3cdeb82215b", "AddEncExchangeSet");
            }

            return filepath;

            #endregion offlinedata
        }

        private void UpdateDisplaySettings()
        {
            // Apply color scheme
            if ((bool)radDay.IsChecked) { EncEnvironmentSettings.Default.EncDisplaySettings.MarinerSettings.ColorScheme = EncColorScheme.Day; }
            else if ((bool)radDusk.IsChecked) { EncEnvironmentSettings.Default.EncDisplaySettings.MarinerSettings.ColorScheme = EncColorScheme.Dusk; }
            else if ((bool)radNight.IsChecked) { EncEnvironmentSettings.Default.EncDisplaySettings.MarinerSettings.ColorScheme = EncColorScheme.Night; }

            // Apply area symbolization
            if ((bool)radAreaPlain.IsChecked) { EncEnvironmentSettings.Default.EncDisplaySettings.MarinerSettings.AreaSymbolizationType = EncAreaSymbolizationType.Plain; }
            else { EncEnvironmentSettings.Default.EncDisplaySettings.MarinerSettings.AreaSymbolizationType = EncAreaSymbolizationType.Symbolized; }

            // Apply point symbolization
            if ((bool)radPointPaper.IsChecked) { EncEnvironmentSettings.Default.EncDisplaySettings.MarinerSettings.PointSymbolizationType = EncPointSymbolizationType.PaperChart; }
            else { EncEnvironmentSettings.Default.EncDisplaySettings.MarinerSettings.PointSymbolizationType = EncPointSymbolizationType.Simplified; }
        }

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Apply display settings
            UpdateDisplaySettings();
        }
    }
}