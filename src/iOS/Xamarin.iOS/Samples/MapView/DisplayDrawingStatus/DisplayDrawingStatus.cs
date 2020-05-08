// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.DisplayDrawingStatus
{
    [Register("DisplayDrawingStatus")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display draw status",
        category: "MapView",
        description: "Get the draw status of your map view or scene view to know when all layers in the map or scene have finished drawing.",
        instructions: "Pan and zoom around the map. Observe how the status changes from a loading animation to solid, indicating that drawing has completed.",
        tags: new[] { "draw", "loading", "map", "render" })]
    public class DisplayDrawingStatus : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIActivityIndicatorView _activityIndicator;
        private UILabel _statusLabel;

        public DisplayDrawingStatus()
        {
            Title = "Display drawing status";
        }

        private async void Initialize()
        {
            // Create new Map with basemap.
            Map myMap = new Map(BasemapType.Topographic, 34.056, -117.196, 4);

            // URL to the feature service.
            Uri serviceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0");

            // Initialize a new feature layer.
            ServiceFeatureTable myFeatureTable = new ServiceFeatureTable(serviceUri);
            FeatureLayer myFeatureLayer = new FeatureLayer(myFeatureTable);

            // Load the feature layer.
            await myFeatureLayer.LoadAsync();

            // Add the feature layer to the Map.
            myMap.OperationalLayers.Add(myFeatureLayer);

            // Provide used Map to the MapView.
            _myMapView.Map = myMap;

            // Animate the activity spinner.
            _activityIndicator.StartAnimating();
        }

        private void OnMapViewDrawStatusChanged(object sender, DrawStatusChangedEventArgs e)
        {
            // Make sure that the UI changes are done in the UI thread.
            BeginInvokeOnMainThread(() =>
            {
                // Show the activity indicator if the map is drawing.
                if (e.Status == DrawStatus.InProgress)
                {
                    _activityIndicator.Hidden = false;
                    _statusLabel.Text = "Drawing status: In progress";
                }
                else
                {
                    _activityIndicator.Hidden = true;
                    _statusLabel.Text = "Drawing status: Completed";
                }
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _activityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge)
            {
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _statusLabel = new UILabel
            {
                Text = "Drawing status: Unknown",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, _activityIndicator, _statusLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                _statusLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _statusLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _statusLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _statusLabel.HeightAnchor.ConstraintEqualTo(40),

                _activityIndicator.TopAnchor.ConstraintEqualTo(_statusLabel.BottomAnchor),
                _activityIndicator.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _activityIndicator.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _activityIndicator.HeightAnchor.ConstraintEqualTo(40)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events, removing any existing subscriptions.
            _myMapView.DrawStatusChanged -= OnMapViewDrawStatusChanged;
            _myMapView.DrawStatusChanged += OnMapViewDrawStatusChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.DrawStatusChanged -= OnMapViewDrawStatusChanged;
        }
    }
}