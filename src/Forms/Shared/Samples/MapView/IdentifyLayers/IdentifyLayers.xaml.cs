﻿// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Xamarin.Forms;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.IdentifyLayers
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Identify layers",
        category: "MapView",
        description: "Identify features in all layers in a map. MapView supports identifying features across multiple layers. Because some layer types have sublayers, the sample recursively counts results for sublayers within each layer.",
        instructions: "Tap to identify features. An alert will show all layers with features under the cursor.",
        tags: new[] { "identify", "recursion", "recursive", "sublayers" })]
    public partial class IdentifyLayers : ContentPage
    {
        public IdentifyLayers()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Create a map with an initial viewpoint.
            Map myMap = new Map(Basemap.CreateTopographic());
            myMap.InitialViewpoint = new Viewpoint(new MapPoint(-10977012.785807, 4514257.550369, SpatialReference.Create(3857)), 68015210);
            MyMapView.Map = myMap;

            try
            {
                // Add a map image layer to the map after turning off two sublayers.
                ArcGISMapImageLayer cityLayer = new ArcGISMapImageLayer(new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer"));
                await cityLayer.LoadAsync();
                cityLayer.Sublayers[1].IsVisible = false;
                cityLayer.Sublayers[2].IsVisible = false;
                myMap.OperationalLayers.Add(cityLayer);

                // Add a feature layer to the map.
                FeatureLayer damageLayer = new FeatureLayer(new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0"));
                myMap.OperationalLayers.Add(damageLayer);

                // Listen for taps/clicks to start the identify operation.
                MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
            }
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Perform an identify across all layers, taking up to 10 results per layer.
                IReadOnlyList<IdentifyLayerResult> identifyResults = await MyMapView.IdentifyLayersAsync(e.Position, 15, false, 10);

                // Add a line to the output for each layer, with a count of features in the layer.
                string result = "";
                foreach (IdentifyLayerResult layerResult in identifyResults)
                {
                    // Note: because some layers have sublayers, a recursive function is required to count results.
                    result = result + layerResult.LayerContent.Name + ": " + recursivelyCountIdentifyResultsForSublayers(layerResult) + "\n";
                }

                if (!String.IsNullOrEmpty(result))
                {
                    await Application.Current.MainPage.DisplayAlert("Identify result", result, "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private int recursivelyCountIdentifyResultsForSublayers(IdentifyLayerResult result)
        {
            int sublayerResultCount = 0;
            foreach (IdentifyLayerResult res in result.SublayerResults)
            {
                // This function calls itself to count results on sublayers.
                sublayerResultCount += recursivelyCountIdentifyResultsForSublayers(res);
            }

            return result.GeoElements.Count + sublayerResultCount;
        }
    }
}