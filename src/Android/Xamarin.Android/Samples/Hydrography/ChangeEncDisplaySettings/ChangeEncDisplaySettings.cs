// Copyright 2017 Esri.
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
using ArcGISRuntimeXamarin.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrography;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ArcGISRuntimeXamarin.Samples.ChangeEncDisplaySettings
{
    [Activity]
    public class ChangeEncDisplaySettings : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Hold a reference to the (static) app-wide mariner settings
        EncMarinerSettings _encMarinerSettings = EncEnvironmentSettings.Default.EncDisplaySettings.MarinerSettings;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "ENC Display Settings";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Initialize the map with an oceans basemap
            _myMapView.Map = new Map(Basemap.CreateOceans());

            // Get the path to the ENC Exchange Set
            string encPath = await GetEncPath();

            // Create the Exchange Set
            // Note: this constructor takes an array of paths because so that update sets can be loaded alongside base data
            EncExchangeSet myEncExchangeSet = new EncExchangeSet(new string[] { encPath });

            // Wait for the layer to load
            await myEncExchangeSet.LoadAsync();

            // Store a list of data set extent's - will be used to zoom the mapview to the full extent of the Exchange Set
            List<Envelope> dataSetExtents = new List<Envelope>();

            // Add each data set as a layer
            foreach (EncDataSet myEncDataSet in myEncExchangeSet.DataSets)
            {
                var path = myEncDataSet.Name.Replace("\\", "/");
                // Create the cell and layer
                EncCell cell = new EncCell(Path.Combine(Path.GetDirectoryName(encPath), path));
                EncLayer myEncLayer = new EncLayer(cell);

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
            ArrayAdapter<String> colorAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, new List<String>() { "Day", "Dusk", "Night" });
            colorSpinner.Adapter = colorAdapter;
            layout.AddView(colorSpinner);

            layout.AddView(new TextView(this) { Text = "Area Symbolization:" });
            Spinner areaSpinner = new Spinner(this);
            ArrayAdapter<String> areaAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, new List<String>() { "Plain", "Symbolized" });
            areaSpinner.Adapter = areaAdapter;
            layout.AddView(areaSpinner);

            layout.AddView(new TextView(this) { Text = "Point Symbolization:" });
            Spinner pointSpinner = new Spinner(this);
            ArrayAdapter<String> pointAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, new List<String>() { "Paper Chart", "Simplified" });
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
    }
}