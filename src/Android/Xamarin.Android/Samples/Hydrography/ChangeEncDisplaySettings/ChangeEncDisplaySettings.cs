// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrography;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System.Collections.Generic;

namespace ArcGISRuntimeXamarin.Samples.ChangeEncDisplaySettings
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change ENC display settings",
        "Hydrography",
        "This sample demonstrates how to control ENC environment settings. These settings apply to the display of all ENC content in your app.",
        "This sample automatically downloads ENC data from ArcGIS Online before displaying the map.")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("9d2987a825c646468b3ce7512fb76e2d")]
    public class ChangeEncDisplaySettings : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Hold a reference to the (static) app-wide mariner settings
        private EncMarinerSettings _encMarinerSettings = EncEnvironmentSettings.Default.DisplaySettings.MarinerSettings;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "ENC Display Settings";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        protected override void OnStop()
        {
            base.OnStop();
            // ENC environment settings apply to the entire application
            // They need to be reset after leaving the sample to avoid affecting other samples
            EncEnvironmentSettings.Default.DisplaySettings.MarinerSettings.ResetToDefaults();
            EncEnvironmentSettings.Default.DisplaySettings.ViewingGroupSettings.ResetToDefaults();
            EncEnvironmentSettings.Default.DisplaySettings.TextGroupVisibilitySettings.ResetToDefaults();
        }

        private async void Initialize()
        {
            // Initialize the map with an oceans basemap
            _myMapView.Map = new Map(Basemap.CreateOceans());

            // Get the path to the ENC Exchange Set
            string encPath = DataManager.GetDataFolder("9d2987a825c646468b3ce7512fb76e2d", "ExchangeSetwithoutUpdates", "ENC_ROOT",
                "CATALOG.031");

            // Create the Exchange Set
            // Note: this constructor takes an array of paths because so that update sets can be loaded alongside base data
            EncExchangeSet myEncExchangeSet = new EncExchangeSet( encPath );

            // Wait for the layer to load
            await myEncExchangeSet.LoadAsync();

            // Store a list of data set extent's - will be used to zoom the mapview to the full extent of the Exchange Set
            List<Envelope> dataSetExtents = new List<Envelope>();

            // Add each data set as a layer
            foreach (EncDataset myEncDataSet in myEncExchangeSet.Datasets)
            {
                EncLayer myEncLayer = new EncLayer(new EncCell(myEncDataSet));

                // Add the layer to the map
                _myMapView.Map.OperationalLayers.Add(myEncLayer);

                // Wait for the layer to load
                await myEncLayer.LoadAsync();

                // Add the extent to the list of extents
                dataSetExtents.Add(myEncLayer.FullExtent);
            }

            // Use the geometry engine to compute the full extent of the ENC Exchange Set
            Envelope fullExtent = GeometryEngine.CombineExtents(dataSetExtents);

            // Set the viewpoint
            _myMapView.SetViewpoint(new Viewpoint(fullExtent));
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add controls for display settings
            layout.AddView(new TextView(this) { Text = "Color Scheme:" });
            Spinner colorSpinner = new Spinner(this);
            ArrayAdapter<string> colorAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, new List<string>() { "Day", "Dusk", "Night" });
            colorSpinner.Adapter = colorAdapter;
            layout.AddView(colorSpinner);

            layout.AddView(new TextView(this) { Text = "Area Symbolization:" });
            Spinner areaSpinner = new Spinner(this);
            ArrayAdapter<string> areaAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, new List<string>() { "Plain", "Symbolized" });
            areaSpinner.Adapter = areaAdapter;
            layout.AddView(areaSpinner);

            layout.AddView(new TextView(this) { Text = "Point Symbolization:" });
            Spinner pointSpinner = new Spinner(this);
            ArrayAdapter<string> pointAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, new List<string>() { "Paper Chart", "Simplified" });
            pointSpinner.Adapter = pointAdapter;
            layout.AddView(pointSpinner);

            // Subscribe to changes
            colorSpinner.ItemSelected += ColorSchemeSelected;
            areaSpinner.ItemSelected += AreaStyleSelected;
            pointSpinner.ItemSelected += PointStyleSelected;

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }

        private void PointStyleSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            // Apply point style
            switch (e.Position)
            {
                case 0:
                    _encMarinerSettings.PointSymbolizationType = EncPointSymbolizationType.PaperChart;
                    break;

                case 1:
                default:
                    _encMarinerSettings.PointSymbolizationType = EncPointSymbolizationType.Simplified;
                    break;
            }
        }

        private void AreaStyleSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            // Apply area style
            switch (e.Position)
            {
                case 0:
                    _encMarinerSettings.AreaSymbolizationType = EncAreaSymbolizationType.Plain;
                    break;

                case 1:
                default:
                    _encMarinerSettings.AreaSymbolizationType = EncAreaSymbolizationType.Symbolized;
                    break;
            }
        }

        private void ColorSchemeSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            // Apply color scheme
            switch (e.Position)
            {
                case 0:
                    _encMarinerSettings.ColorScheme = EncColorScheme.Day;
                    break;

                case 1:
                    _encMarinerSettings.ColorScheme = EncColorScheme.Dusk;
                    break;

                case 2:
                default:
                    _encMarinerSettings.ColorScheme = EncColorScheme.Night;
                    break;
            }
        }
    }
}