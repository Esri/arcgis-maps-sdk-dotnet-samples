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
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrography;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISRuntimeXamarin.Samples.SelectEncFeatures
{
    [Activity]
    public class SelectEncFeatures : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Select ENC Features";

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
            EncExchangeSet _encExchangeSet = new EncExchangeSet(new string[] { encPath });

            // Wait for the exchange set to load
            await _encExchangeSet.LoadAsync();

            // Store a list of data set extent's - will be used to zoom the mapview to the full extent of the Exchange Set
            List<Envelope> dataSetExtents = new List<Envelope>();

            // Add each data set as a layer
            EncDataSet _encDataSet = _encExchangeSet.DataSets.First();

            // Create the cell and layer
            EncLayer _encLayer = new EncLayer(new EncCell(_encDataSet));

            // Add the layer to the map
            _myMapView.Map.OperationalLayers.Add(_encLayer);

            // Wait for the layer to load
            await _encLayer.LoadAsync();

            // Set the viewpoint
            _myMapView.SetViewpoint(new Viewpoint(_encLayer.FullExtent));

            // Subscribe to tap events (in order to use them to identify and select features)
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }

        private void ClearAllSelections()
        {
            // For each layer in the operational layers that is an ENC layer
            foreach (EncLayer layer in _myMapView.Map.OperationalLayers.OfType<EncLayer>())
            {
                // Clear the layer's selection
                layer.ClearSelection();
            }
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // First clear any existing selections
            ClearAllSelections();

            // Perform the identify operation
            IReadOnlyList<IdentifyLayerResult> results = await _myMapView.IdentifyLayersAsync(e.Position, 5, false);

            // Return if there are no results
            if (results.Count < 1) { return; }

            // Get the first result with ENC features
            IdentifyLayerResult firstResult = results.Where(res => res.GeoElements.OfType<EncFeature>().Count() > 0).First();

            // Get the layer associated with this set of results
            EncLayer containingLayer = firstResult.LayerContent as EncLayer;

            // Get the first identified feature
            EncFeature firstFeature = results.First().GeoElements.OfType<EncFeature>().First();

            // Select the feature
            containingLayer.SelectFeature(firstFeature);

            // Create the callout definition
            CalloutDefinition definition = new CalloutDefinition("Feature", firstFeature.Attributes["FeatureDescription"].ToString());

            // Show the callout
            _myMapView.ShowCalloutAt(e.Location, definition);
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
    }
}