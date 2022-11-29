// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Arcade;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.QueryFeaturesWithArcadeExpression
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Query features with Arcade expression",
        category: "Data",
        description: "Query features on a map using an Arcade expression.",
        instructions: "Click on any neighborhood to see the number of crimes in the last 60 days in a callout.",
        tags: new[] { "Arcade evaluator", "Arcade expression", "identify layers", "portal", "portal item", "query" })]
    public partial class QueryFeaturesWithArcadeExpression
    {
        // Hold a reference to the layer for use in event handlers.
        private Layer _layer;

        // Hold a reference to the feature for use in event handlers.
        private ArcGISFeature _previousFeature;

        // The name of the layer used in this sample.
        private const string RPDBeatsLayerName = "RPD Beats  - City_Beats_Border_1128-4500";

        // Hold a reference to the callout text content to store it between clicks.
        private string _calloutText = string.Empty;

        public QueryFeaturesWithArcadeExpression()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Create an ArcGIS portal item.
                var portal = await ArcGISPortal.CreateAsync();
                var item = await PortalItem.CreateAsync(portal, "14562fced3474190b52d315bc19127f6");

                // Create a map using the portal item.
                MyMapView.Map = new Map(item);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
            }

            // Load the map.
            await MyMapView.Map.LoadAsync();

            // Set the visibility of all but the RDT Beats layer to false to prevent UI clutter.
            MyMapView.Map.OperationalLayers.ToList().ForEach(l => l.IsVisible = l.Name == RPDBeatsLayerName);

            // Hold the layer value so that the clicked geoelement can be identified in the event handler.
            _layer = MyMapView.Map.OperationalLayers.FirstOrDefault(l => l.Name == RPDBeatsLayerName);
        }

        private async Task GeoViewTappedTask(GeoViewInputEventArgs e)
        {
            try
            {
                // Get the layer based on the position tapped on the MapView.
                IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_layer, e.Position, 12, false);

                if (identifyResult == null || !identifyResult.GeoElements.Any())
                {
                    MyMapView.DismissCallout();
                    return;
                }

                // Get the tapped GeoElement as an ArcGISFeature.
                GeoElement element = identifyResult.GeoElements.First();
                ArcGISFeature feature = element as ArcGISFeature;

                // If the previously clicked feature is null or the previous feature ID does not match the current feature ID
                // run the arcade expression query to get the crime count for a given feature.
                if (_previousFeature == null || !(feature.Attributes["ID"].Equals(_previousFeature.Attributes["ID"])))
                {
                    // Show the loading indicator as the arcade evaluator evaluation call can take time to complete.
                    MyLoadingGrid.Visibility = Visibility.Visible;

                    // Instantiate a string containing the arcade expression.
                    string expressionValue = "var crimes = FeatureSetByName($map, 'Crime in the last 60 days');\n" +
                                             "return Count(Intersects($feature, crimes));";

                    // Create an ArcadeExpression using the string expression.
                    var expression = new ArcadeExpression(expressionValue);

                    // Create an ArcadeEvaluator with the ArcadeExpression and an ArcadeProfile enum.
                    var evaluator = new ArcadeEvaluator(expression, ArcadeProfile.FormCalculation);

                    // Instantiate a list of profile variable key value pairs.
                    var profileVariables = new List<KeyValuePair<string, object>>();
                    profileVariables.Add(new KeyValuePair<string, object>("$feature", feature));
                    profileVariables.Add(new KeyValuePair<string, object>("$map", MyMapView.Map));

                    // Get the arcade evaluation result given the previously set profile variables.
                    ArcadeEvaluationResult arcadeEvaluationResult = await evaluator.EvaluateAsync(profileVariables);

                    if (arcadeEvaluationResult == null) return;

                    // Construct the callout text content.
                    var crimeCount = Convert.ToInt32(arcadeEvaluationResult.Result);
                    _calloutText = $"Crimes in the last 60 days: {crimeCount}";

                    // Set the current feature as the previous feature for the next click detection.
                    _previousFeature = feature;

                    // Hide the loading indicator.
                    MyLoadingGrid.Visibility = Visibility.Collapsed;
                }

                // Display a callout showing the number of crimes in the last 60 days.
                MyMapView.ShowCalloutAt(e.Location, new CalloutDefinition(string.Empty, _calloutText));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            _ = GeoViewTappedTask(e);
        }
    }
}