﻿// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrography;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISRuntime.WPF.Samples.SelectEncFeatures
{
    public partial class SelectEncFeatures
    {
        public SelectEncFeatures()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
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
            EncDataset myEncDataSet = myEncExchangeSet.Datasets.First();

            // Create the cell and layer
            EncLayer myEncLayer = new EncLayer(new EncCell(myEncDataSet));

            // Add the layer to the map
            MyMapView.Map.OperationalLayers.Add(myEncLayer);

            // Wait for the layer to load
            await myEncLayer.LoadAsync();

            // Set the viewpoint
            MyMapView.SetViewpoint(new Viewpoint(myEncLayer.FullExtent));

            // Subscribe to tap events (in order to use them to identify and select features)
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private void ClearAllSelections()
        {
            // For each layer in the operational layers that is an ENC layer
            foreach (EncLayer layer in MyMapView.Map.OperationalLayers.OfType<EncLayer>())
            {
                // Clear the layer's selection
                layer.ClearSelection();
            }
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            // First clear any existing selections
            ClearAllSelections();

            // Perform the identify operation
            IReadOnlyList<IdentifyLayerResult> results = await MyMapView.IdentifyLayersAsync(e.Position, 5, false);

            // Return if there are no results
            if (results.Count < 1) { return; }

            // Get the results that are from ENC layers
            IEnumerable<IdentifyLayerResult> encResults = results.Where(result => result.LayerContent is EncLayer);

            // Get the ENC results that have features
            IEnumerable<IdentifyLayerResult> encResultsWithFeatures = encResults.Where(result => result.GeoElements.Count > 0);

            // Get the first result with ENC features
            IdentifyLayerResult firstResult = encResultsWithFeatures.First();

            // Get the layer associated with this set of results
            EncLayer containingLayer = firstResult.LayerContent as EncLayer;

            // Get the first identified ENC feature
            EncFeature firstFeature = firstResult.GeoElements.First() as EncFeature;

            // Select the feature
            containingLayer.SelectFeature(firstFeature);

            // Create the callout definition
            CalloutDefinition definition = new CalloutDefinition(firstFeature.Acronym, firstFeature.Description);

            // Show the callout
            MyMapView.ShowCalloutAt(e.Location, definition);
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