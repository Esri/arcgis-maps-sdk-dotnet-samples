// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.AccessLoadStatus
{
    [Register("AccessLoadStatus")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Access load status",
        category: "Map",
        description: "Determine the map's load status which can be: `NotLoaded`, `FailedToLoad`, `Loading`, `Loaded`.",
        instructions: "The load status of the map will be displayed as the sample loads.",
        tags: new[] { "LoadStatus", "Loadable pattern", "Map" })]
    public class AccessLoadStatus : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UILabel _loadStatusLabel;

        public AccessLoadStatus()
        {
            Title = "Access load status";
        }

        private void OnMapsLoadStatusChanged(object sender, LoadStatusEventArgs e)
        {
            // Make sure that the UI changes are done in the UI thread.
            InvokeOnMainThread(() =>
            {
                // Update the load status information.
                _loadStatusLabel.Text = $"Map's load status: {e.Status}";
            });
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _loadStatusLabel = new UILabel
            {
                BackgroundColor = UIColor.FromWhiteAlpha(0f, .6f),
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, _loadStatusLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),

                _loadStatusLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _loadStatusLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _loadStatusLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _loadStatusLabel.HeightAnchor.ConstraintEqualTo(40)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Create new Map with basemap.
            Map myMap = new Map(Basemap.CreateImagery());

            // Register to handle loading status changes.
            myMap.LoadStatusChanged += OnMapsLoadStatusChanged;

            // Show the map.
            _myMapView.Map = myMap;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.Map.LoadStatusChanged -= OnMapsLoadStatusChanged;
        }
    }
}