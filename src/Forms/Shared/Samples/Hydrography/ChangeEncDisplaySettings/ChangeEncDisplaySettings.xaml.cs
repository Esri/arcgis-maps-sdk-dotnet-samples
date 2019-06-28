﻿// Copyright 2018 Esri.
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
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.ChangeEncDisplaySettings
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change ENC display settings",
        "Hydrography",
        "This sample demonstrates how to control ENC environment settings. These settings apply to the display of all ENC content in your app.",
        "This sample automatically downloads ENC data from ArcGIS Online before displaying the map.")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("9d2987a825c646468b3ce7512fb76e2d")]
    public partial class ChangeEncDisplaySettings : ContentPage
    {
        public ChangeEncDisplaySettings()
        {
            InitializeComponent();

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
                await MyMapView.SetViewpointAsync(new Viewpoint(fullExtent));

                // Subscribe to notifications about leaving so that settings can be reset.
                // This looks different because of sample viewer plumbing.
                // Replace `((ArcGISRuntime.SamplePage)this.Parent)` with `this` in your app.
                ((Page)this.Parent).Disappearing += SampleUnloaded;
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
            }
        }

        private void SampleUnloaded(object sender, EventArgs e)
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

            // Apply area symbolization
            string selectedAreaType = AreaPicker.SelectedItem.ToString();
            globalMarinerSettings.AreaSymbolizationType = selectedAreaType == "Plain" ? EncAreaSymbolizationType.Plain : EncAreaSymbolizationType.Symbolized;

            // Apply point symbolization
            string selectedPointType = PointPicker.SelectedItem.ToString();
            globalMarinerSettings.PointSymbolizationType = selectedPointType == "Paper Chart" ? EncPointSymbolizationType.PaperChart : EncPointSymbolizationType.Simplified;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            UpdateDisplaySettings();
        }
    }
}