// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrography;
using Esri.ArcGISRuntime.Mapping;
using System.Collections.Generic;
using Windows.UI.Popups;

namespace ArcGISRuntime.UWP.Samples.ChangeEncDisplaySettings
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change ENC display settings",
        "Hydrography",
        "Configure the display of ENC content.",
        "The sample displays an electronic navigational chart when it opens. Use the options to choose variations on colors and symbology.",
        "ENC", "IHO", "S-52", "S-57", "display", "hydrographic", "hydrography", "layers", "maritime", "nautical chart", "settings", "symbology")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("9d2987a825c646468b3ce7512fb76e2d")]
    public partial class ChangeEncDisplaySettings
    {
        public ChangeEncDisplaySettings()
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
            string encPath = DataManager.GetDataFolder("9d2987a825c646468b3ce7512fb76e2d", "ExchangeSetwithoutUpdates", "ENC_ROOT",
                "CATALOG.031");

            // Create the Exchange Set
            // Note: this constructor takes an array of paths because so that update sets can be loaded alongside base data
            EncExchangeSet myEncExchangeSet = new EncExchangeSet(new string[] { encPath });

            try
            {
                // Wait for the exchange set to load
                await myEncExchangeSet.LoadAsync();

                // Store a list of data set extent's - will be used to zoom the mapview to the full extent of the Exchange Set
                List<Envelope> dataSetExtents = new List<Envelope>();

                // Add each data set as a layer
                foreach (EncDataset myEncDataSet in myEncExchangeSet.Datasets)
                {
                    // Create the cell and layer
                    EncLayer myEncLayer = new EncLayer(new EncCell(myEncDataSet));

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

                // Subscribe to notifications about leaving so that settings can be re-set
                this.Unloaded += SampleUnloaded;

                // Enable the setting change UI.
                DayRadioButton.Checked += Setting_Checked;
                DuskRadioButton.Checked += Setting_Checked;
                NightRadioButton.Checked += Setting_Checked;
                PaperPointRadioButton.Checked += Setting_Checked;
                SymbolizedAreaRadioButton.Checked += Setting_Checked;
                PlainAreaRadioButton.Checked += Setting_Checked;
                SimplifiedRadioButton.Checked += Setting_Checked;
            }
            catch (Exception e)
            {
                await new MessageDialog(e.ToString(), "Error").ShowAsync();
            }
        }

        private void SampleUnloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // ENC environment settings apply to the entire application
            // They need to be reset after leaving the sample to avoid affecting other samples
            EncEnvironmentSettings.Default.DisplaySettings.MarinerSettings.ResetToDefaults();
            EncEnvironmentSettings.Default.DisplaySettings.ViewingGroupSettings.ResetToDefaults();
            EncEnvironmentSettings.Default.DisplaySettings.TextGroupVisibilitySettings.ResetToDefaults();
        }

        private void UpdateDisplaySettings()
        {
            // Hold a reference to the application-wide ENC Display Settings
            EncDisplaySettings globalDisplaySettings = EncEnvironmentSettings.Default.DisplaySettings;

            // Hold a reference to the application-wide ENC Mariner Settings (part of display settings)
            EncMarinerSettings globalMarinerSettings = globalDisplaySettings.MarinerSettings;

            // Apply color scheme
            if (DayRadioButton.IsChecked == true) { globalMarinerSettings.ColorScheme = EncColorScheme.Day; }
            else if (DuskRadioButton.IsChecked == true) { globalMarinerSettings.ColorScheme = EncColorScheme.Dusk; }
            else if (NightRadioButton.IsChecked == true) { globalMarinerSettings.ColorScheme = EncColorScheme.Night; }

            // Apply area symbolization
            if (PlainAreaRadioButton.IsChecked == true) { globalMarinerSettings.AreaSymbolizationType = EncAreaSymbolizationType.Plain; }
            else { globalMarinerSettings.AreaSymbolizationType = EncAreaSymbolizationType.Symbolized; }

            // Apply point symbolization
            if (PaperPointRadioButton.IsChecked == true) { globalMarinerSettings.PointSymbolizationType = EncPointSymbolizationType.PaperChart; }
            else { globalMarinerSettings.PointSymbolizationType = EncPointSymbolizationType.Simplified; }
        }

        private void Setting_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Apply display settings
            UpdateDisplaySettings();
        }
    }
}