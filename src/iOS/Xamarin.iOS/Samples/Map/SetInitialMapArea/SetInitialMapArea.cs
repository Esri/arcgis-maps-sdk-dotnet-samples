// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.SetInitialMapArea
{
    [Register("SetInitialMapArea")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Map initial extent",
        category: "Map",
        description: "Display the map at an initial viewpoint representing a bounding geometry.",
        instructions: "When the sample loads, note that the map view opens at the initial viewpoint defined on the map.",
        tags: new[] { "envelope", "extent", "initial", "viewpoint", "zoom" })]
    public class SetInitialMapArea : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        public SetInitialMapArea()
        {
            Title = "Set initial map area";
        }

        private void Initialize()
        {
            // Create new Map with basemap.
            Map myMap = new Map(Basemap.CreateImagery());

            // Create and set initial map area.
            Envelope initialLocation = new Envelope(-12211308.778729, 4645116.003309, -12208257.879667, 4650542.535773, SpatialReferences.WebMercator);
            myMap.InitialViewpoint = new Viewpoint(initialLocation);

            // Provide used Map to the MapView.
            _myMapView.Map = myMap;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

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