// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ShowMagnifier
{
    [Register("ShowMagnifier")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Show magnifier",
        "MapView",
        "This sample demonstrates how you can tap and hold on a map to get the magnifier. You can also pan while tapping and holding to move the magnifier across the map.",
        "")]
    public class ShowMagnifier : UIViewController
    {
        // Create and hold references to the UI controls.
        private MapView _myMapView;
        private UILabel _helpLabel;

        public ShowMagnifier()
        {
            Title = "Show magnifier";
        }

        public override void LoadView()
        {
            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _helpLabel = new UILabel
            {
                Text = "Tap and hold to show the magnifier.",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            View = new UIView();
            View.AddSubviews(_myMapView, _helpLabel);

            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;

            _helpLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _helpLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _helpLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _helpLabel.HeightAnchor.ConstraintEqualTo(40).Active = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap and initial location.
            Map myMap = new Map(BasemapType.Topographic, 34.056295, -117.195800, 10);

            // Enable magnifier.
            _myMapView.InteractionOptions = new MapViewInteractionOptions
            {
                IsMagnifierEnabled = true
            };

            // Show the map in the view.
            _myMapView.Map = myMap;
        }
    }
}