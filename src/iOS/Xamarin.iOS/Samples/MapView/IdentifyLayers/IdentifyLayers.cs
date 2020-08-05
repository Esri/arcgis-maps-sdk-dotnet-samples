// Copyright 2018 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.IdentifyLayers
{
    [Register("IdentifyLayers")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Identify layers",
        category: "MapView",
        description: "Identify features in all layers in a map. MapView supports identifying features across multiple layers. Because some layer types have sublayers, the sample recursively counts results for sublayers within each layer.",
        instructions: "Tap to identify features. An alert will show all layers with features under the cursor.",
        tags: new[] { "identify", "recursion", "recursive", "sublayers" })]
    public class IdentifyLayers : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        public IdentifyLayers()
        {
            Title = "Identify layers";
        }

        private async void Initialize()
        {
            // Create a map with an initial viewpoint.
            Map myMap = new Map(Basemap.CreateTopographic());
            myMap.InitialViewpoint = new Viewpoint(new MapPoint(-10977012.785807, 4514257.550369, SpatialReference.Create(3857)), 68015210);
            _myMapView.Map = myMap;

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
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Perform an identify across all layers, taking up to 10 results per layer.
                IReadOnlyList<IdentifyLayerResult> identifyResults = await _myMapView.IdentifyLayersAsync(e.Position, 15, false, 10);

                // Add a line to the output for each layer, with a count of features in the layer.
                string result = "";
                foreach (IdentifyLayerResult layerResult in identifyResults)
                {
                    // Note: because some layers have sublayers, a recursive function is required to count results.
                    result = result + layerResult.LayerContent.Name + ": " + recursivelyCountIdentifyResultsForSublayers(layerResult) + "\n";
                }

                if (!String.IsNullOrEmpty(result))
                {
                    new UIAlertView("Identify result", result, (IUIAlertViewDelegate) null, "OK", null).Show();
                }
            }
            catch (Exception ex)
            {
                new UIAlertView("Error", ex.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the view.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            UILabel helpLabel = new UILabel
            {
                Text = "Tap to identify features in all layers.",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, helpLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                helpLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                helpLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                helpLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                helpLabel.HeightAnchor.ConstraintEqualTo(40)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= MyMapView_GeoViewTapped;
        }
    }
}