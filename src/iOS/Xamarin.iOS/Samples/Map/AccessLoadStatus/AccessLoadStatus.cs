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
        "Access load status",
        "Map",
        "This sample demonstrates how to access the Maps' LoadStatus. The LoadStatus will be considered loaded when the following are true: The Map has a valid SpatialReference and the Map has an been set to the MapView.",
        "")]
    public class AccessLoadStatus : UIViewController
    {
        // Create and hold references to the UI controls.
        MapView _myMapView;
        UILabel _loadStatusLabel;

        public AccessLoadStatus()
        {
            Title = "Access load status";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create new Map with basemap.
            Map myMap = new Map(Basemap.CreateImagery());

            // Register to handle loading status changes.
            myMap.LoadStatusChanged += OnMapsLoadStatusChanged;

            // Show the map.
            _myMapView.Map = myMap;
        }

        public override void LoadView()
        {
            // Create the MapView.
            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Create the label.
            _loadStatusLabel = new UILabel()
            {
                BackgroundColor = UIColor.FromWhiteAlpha(0f, .6f),
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views to the layout.
            View = new UIView();
            View.AddSubviews(_myMapView, _loadStatusLabel);

            // Set up constraints.
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;

            _loadStatusLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _loadStatusLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _loadStatusLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _loadStatusLabel.HeightAnchor.ConstraintEqualTo(40).Active = true;
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // Ensure that the map is centered on the visible portion of the MapView.
            _myMapView.ViewInsets = new UIEdgeInsets(_loadStatusLabel.Frame.Bottom, 0, 0, 0);
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
    }
}