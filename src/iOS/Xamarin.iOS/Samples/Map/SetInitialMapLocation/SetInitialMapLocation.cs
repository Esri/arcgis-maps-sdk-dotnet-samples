// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.SetInitialMapLocation
{
    [Register("SetInitialMapLocation")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Set initial map location",
        "Map",
        "This sample demonstrates how to create a map with a standard ESRI Imagery with Labels basemap that is centered on a latitude and longitude location and zoomed into a specific level of detail.",
        "")]
    public class SetInitialMapLocation : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        public SetInitialMapLocation()
        {
            Title = "Set initial map location";
        }

        private void Initialize()
        {
            // Show a map with 'Imagery with Labels' basemap and an initial location.
            _myMapView.Map = new Map(BasemapType.ImageryWithLabels, -33.867886, -63.985, 16);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView();

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_myMapView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }
    }
}