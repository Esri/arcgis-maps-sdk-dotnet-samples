// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntimeXamarin.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrography;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.ChangeEncDisplaySettings
{
    public partial class ChangeEncDisplaySettings : ContentPage
    {
        public ChangeEncDisplaySettings()
        {
            InitializeComponent();

            Title = "ENC Display Settings";

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Add display options to UI
            ColorSchemePicker.ItemsSource = new List<String>() { "Day", "Dusk", "Night" };
            AreaPicker.ItemsSource = new List<String>() { "Plain", "Symbolized" };
            PointPicker.ItemsSource = new List<String>() { "Paper Chart", "Simplified" };

            // Provide initial selection
            ColorSchemePicker.SelectedIndex = 0; AreaPicker.SelectedIndex = 0; PointPicker.SelectedIndex = 0;

            // Apply initial display settings
            UpdateDisplaySettings();

            // Initialize the map with an oceans basemap
            MyMapView.Map = new Map(Basemap.CreateOceans());

            // Get the path to the ENC Exchange Set
            string encPath = await GetEncPath();

            // Create the Exchange Set
            // Note: this constructor takes an array of paths because so that update sets can be loaded alongside base data
            EncExchangeSet myEncExchangeSet = new EncExchangeSet(new string[] { encPath });

            // Wait for the exchange set to load
            await myEncExchangeSet.LoadAsync();

            // Store a list of data set extent's - will be used to zoom the mapview to the full extent of the Exchange Set
            List<Envelope> dataSetExtents = new List<Envelope>();

            // Add each data set as a layer
            foreach (EncDataset myEncDataSet in myEncExchangeSet.Datasets)
            {
                var path = myEncDataSet.Name.Replace("\\", "/");
                // Create the cell and layer
                EncCell cell = new EncCell(Path.Combine(Path.GetDirectoryName(encPath), path));
                EncLayer myEncLayer = new EncLayer(cell);

                // Add the layer to the map
                MyMapView.Map.OperationalLayers.Add(myEncLayer);

                // Wait for the layer to load
                await myEncLayer.LoadAsync();

                // Add the extent to the list of extents
                dataSetExtents.Add(myEncLayer.FullExtent);
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

            // Get the full path - the catalog is within a hierarchy in the downloaded data;
            // /SampleData/AddEncExchangeSet/ExchangeSet/ENC_ROOT/CATALOG.031
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
            // Hold a reference to the application-wide ENC Display Settings
            EncDisplaySettings globalDisplaySettings = EncEnvironmentSettings.Default.EncDisplaySettings;

            // Hold a reference to the application-wide ENC Mariner Settings (part of display settings)
            EncMarinerSettings globalMarinerSettings = globalDisplaySettings.MarinerSettings;

            // Apply color scheme
            string _selectedTheme = ColorSchemePicker.SelectedItem.ToString();
            if (_selectedTheme == "Day") { globalMarinerSettings.ColorScheme = EncColorScheme.Day; }
            else if (_selectedTheme == "Dusk") { globalMarinerSettings.ColorScheme = EncColorScheme.Dusk; }
            else if (_selectedTheme == "Night") { globalMarinerSettings.ColorScheme = EncColorScheme.Night; }

            // Apply area symbolization
            string _selectedAreaType = AreaPicker.SelectedItem.ToString();
            if (_selectedAreaType == "Plain") { globalMarinerSettings.AreaSymbolizationType = EncAreaSymbolizationType.Plain; }
            else { globalMarinerSettings.AreaSymbolizationType = EncAreaSymbolizationType.Symbolized; }

            // Apply point symbolization
            string _selectedPointType = PointPicker.SelectedItem.ToString();
            if (_selectedPointType == "Paper Chart") { globalMarinerSettings.PointSymbolizationType = EncPointSymbolizationType.PaperChart; }
            else { globalMarinerSettings.PointSymbolizationType = EncPointSymbolizationType.Simplified; }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            UpdateDisplaySettings();
        }
    }
}