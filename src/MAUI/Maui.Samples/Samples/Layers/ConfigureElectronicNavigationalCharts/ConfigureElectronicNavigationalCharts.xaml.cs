﻿// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrography;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;

namespace ArcGIS.Samples.ConfigureElectronicNavigationalCharts
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Configure electronic navigational charts",
        category: "Layers",
        description: "Display and configure electronic navigational charts per ENC specification.",
        instructions: "When opened, the sample displays an electronic navigational chart. Tap on the map to select Enc features and view the feature's acronyms and descriptions shown in a callout. Tap \"Display Settings\" and use the options to adjust some of the Enc mariner display settings, such as the colors and symbology.",
        tags: new[] { "ENC", "IHO", "S-52", "S-57", "hydrography", "identify", "layers", "maritime", "nautical chart", "select", "settings", "symbology" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("9d2987a825c646468b3ce7512fb76e2d")]
    public partial class ConfigureElectronicNavigationalCharts : ContentPage, IDisposable
    {
        public ConfigureElectronicNavigationalCharts()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization.
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Add display options to UI.
            ColorSchemePicker.ItemsSource = new List<String>() { "Day", "Dusk", "Night" };
            AreaPicker.ItemsSource = new List<String>() { "Plain", "Symbolized" };
            PointPicker.ItemsSource = new List<String>() { "Paper Chart", "Simplified" };

            // Provide initial selection.
            ColorSchemePicker.SelectedIndex = 0; AreaPicker.SelectedIndex = 0; PointPicker.SelectedIndex = 0;

            // Apply initial display settings.
            UpdateDisplaySettings();

            // Initialize the map with an oceans basemap.
            MyMapView.Map = new Map(BasemapStyle.ArcGISOceans);

            // Get the path to the ENC Exchange Set.
            string encPath = DataManager.GetDataFolder("9d2987a825c646468b3ce7512fb76e2d", "ExchangeSetwithoutUpdates", "ENC_ROOT",
                "CATALOG.031");

            // Create the Exchange Set.
            // Note: this constructor takes an array of paths because so that update sets can be loaded alongside base data.
            EncExchangeSet myEncExchangeSet = new EncExchangeSet(new string[] { encPath });

            try
            {
                // Wait for the exchange set to load.
                await myEncExchangeSet.LoadAsync();

                // Store a list of data set extent's - will be used to zoom the mapview to the full extent of the Exchange Set.
                List<Envelope> dataSetExtents = new List<Envelope>();

                // Add each data set as a layer.
                foreach (EncDataset myEncDataSet in myEncExchangeSet.Datasets)
                {
                    EncLayer myEncLayer = new EncLayer(new EncCell(myEncDataSet));

                    // Add the layer to the map.
                    MyMapView.Map.OperationalLayers.Add(myEncLayer);

                    // Wait for the layer to load.
                    await myEncLayer.LoadAsync();

                    // Add the extent to the list of extents.
                    dataSetExtents.Add(myEncLayer.FullExtent);
                }

                // Use the geometry engine to compute the full extent of the ENC Exchange Set.
                Envelope fullExtent = dataSetExtents.CombineExtents();

                // Set the viewpoint.
                await MyMapView.SetViewpointAsync(new Viewpoint(fullExtent));
            }
            catch (Exception e)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Error", e.ToString(), "OK");
            }
        }

        private void UpdateDisplaySettings()
        {
            // Hold a reference to the application-wide ENC Display Settings.
            EncDisplaySettings globalDisplaySettings = EncEnvironmentSettings.Default.DisplaySettings;

            // Hold a reference to the application-wide ENC Mariner Settings (part of display settings).
            EncMarinerSettings globalMarinerSettings = globalDisplaySettings.MarinerSettings;

            // Apply color scheme.
            string selectedTheme = ColorSchemePicker.SelectedItem.ToString();
            switch (selectedTheme)
            {
                case "Day":
                    globalMarinerSettings.ColorScheme = EncColorScheme.Day;
                    break;

                case "Dusk":
                    globalMarinerSettings.ColorScheme = EncColorScheme.Dusk;
                    break;

                case "Night":
                    globalMarinerSettings.ColorScheme = EncColorScheme.Night;
                    break;
            }

            // Apply area symbolization.
            string selectedAreaType = AreaPicker.SelectedItem.ToString();
            globalMarinerSettings.AreaSymbolizationType = selectedAreaType == "Plain" ? EncAreaSymbolizationType.Plain : EncAreaSymbolizationType.Symbolized;

            // Apply point symbolization.
            string selectedPointType = PointPicker.SelectedItem.ToString();
            globalMarinerSettings.PointSymbolizationType = selectedPointType == "Paper Chart" ? EncPointSymbolizationType.PaperChart : EncPointSymbolizationType.Simplified;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            UpdateDisplaySettings();
        }

        private void ClearAllSelections()
        {
            // For each layer in the operational layers that is an ENC layer
            foreach (EncLayer layer in MyMapView.Map.OperationalLayers.OfType<EncLayer>())
            {
                // Clear the layer's selection
                layer.ClearSelection();
            }

            // Clear the callout
            MyMapView.DismissCallout();
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            // First clear any existing selections
            ClearAllSelections();

            try
            {
                // Perform the identify operation.
                IReadOnlyList<IdentifyLayerResult> results = await MyMapView.IdentifyLayersAsync(e.Position, 10, false);

                // Return if there are no results.
                if (results.Count < 1) { return; }

                // Get the results that are from ENC layers.
                IEnumerable<IdentifyLayerResult> encResults = results.Where(result => result.LayerContent is EncLayer);

                // Get the first result with ENC features. (Depending on the data, there may be more than one IdentifyLayerResult that contains ENC features.)
                IdentifyLayerResult firstResult = encResults.First();

                // Get the layer associated with this set of results.
                EncLayer containingLayer = (EncLayer)firstResult.LayerContent;

                // Get the GeoElement identified in this layer.
                EncFeature encFeature = (EncFeature)firstResult.GeoElements.First();

                // Select the feature.
                containingLayer.SelectFeature(encFeature);

                // Create the callout definition.
                CalloutDefinition definition = new CalloutDefinition(encFeature.Acronym, encFeature.Description);

                // Show the callout.
                MyMapView.ShowCalloutAt(e.Location, definition);
            }
            catch (Exception ex)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        public void Dispose()
        {
            // ENC environment settings apply to the entire application.
            // They need to be reset after leaving the sample to avoid affecting other samples.
            EncEnvironmentSettings.Default.DisplaySettings.MarinerSettings.ResetToDefaults();
            EncEnvironmentSettings.Default.DisplaySettings.ViewingGroupSettings.ResetToDefaults();
            EncEnvironmentSettings.Default.DisplaySettings.TextGroupVisibilitySettings.ResetToDefaults();
        }
    }
}