// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Xamarin.Forms;
using Xamarin.Forms;
#if WINDOWS_UWP
using Colors = Windows.UI.Colors;
#else
using Colors = System.Drawing.Color;
#endif

namespace ArcGISRuntime.Samples.ListRelatedFeatures
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "List related features",
        "Data",
        "This sample demonstrates how to query features related to an identified feature.",
        "Click on a feature to identify it. Related features will be listed in the window above the map.")]
    public partial class ListRelatedFeatures : ContentPage
    {
        // URL to the web map
        private readonly Uri _mapUri =
            new Uri("https://arcgisruntime.maps.arcgis.com/home/item.html?id=dcc7466a91294c0ab8f7a094430ab437");

        // Reference to the feature layer
        private FeatureLayer _myFeatureLayer;

        public ListRelatedFeatures()
        {
            InitializeComponent();

            Title = "List related features";

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Create the portal item from the URL to the webmap
            PortalItem alaskaPortalItem = await PortalItem.CreateAsync(_mapUri);

            // Create the map from the portal item
            Map myMap = new Map(alaskaPortalItem);

            // Add the map to the mapview
            MyMapView.Map = myMap;

            // Wait for the map to load
            await myMap.LoadAsync();

            // Get the feature layer from the map
            _myFeatureLayer = (FeatureLayer)myMap.OperationalLayers.First();

            // Make the selection color yellow and the width thick
            _myFeatureLayer.SelectionColor = Colors.Yellow;
            _myFeatureLayer.SelectionWidth = 5;

            // Listen for GeoViewTapped events
            MyMapView.GeoViewTapped += MyMapViewOnGeoViewTapped;
        }
        private async void MyMapViewOnGeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Clear any existing feature selection and results list
            _myFeatureLayer.ClearSelection();
            MyResultsView.ItemsSource = null;

            // Identify the tapped feature
            IdentifyLayerResult results = await MyMapView.IdentifyLayerAsync(_myFeatureLayer, e.Position, 10, false);

            // Return if there are no results
            if (results.GeoElements.Count < 1) { return; }

            // Get the first result
            ArcGISFeature myFeature = (ArcGISFeature)results.GeoElements.First();

            // Select the feature
            _myFeatureLayer.SelectFeature(myFeature);

            // Get the feature table for the feature
            ArcGISFeatureTable myFeatureTable = (ArcGISFeatureTable)myFeature.FeatureTable;

            // Query related features
            IReadOnlyList<RelatedFeatureQueryResult> relatedFeaturesResult = await myFeatureTable.QueryRelatedFeaturesAsync(myFeature);

            // Create a list to hold the formatted results of the query
            List<String> queryResultsForUi = new List<string>();

            // For each query result
            foreach (RelatedFeatureQueryResult result in relatedFeaturesResult)
            {
                // And then for each feature in the result
                foreach (Feature resultFeature in result)
                {
                    // Get a reference to the feature's table
                    ArcGISFeatureTable relatedTable = (ArcGISFeatureTable)resultFeature.FeatureTable;

                    // Get the display field name - this is the name of the field that is intended for display
                    string displayFieldName = relatedTable.LayerInfo.DisplayFieldName;

                    // Get the name of the feature's table
                    string tableName = relatedTable.TableName;

                    // Get the display name for the feature
                    string featureDisplayname = resultFeature.Attributes[displayFieldName].ToString();

                    // Create a formatted result string
                    string formattedResult = String.Format("{0} - {1}", tableName, featureDisplayname);

                    // Add the result to the list
                    queryResultsForUi.Add(formattedResult);
                }
            }

            // Update the UI with the result list
            MyResultsView.ItemsSource = queryResultsForUi;
        }
    }
}