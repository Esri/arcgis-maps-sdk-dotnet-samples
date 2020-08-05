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

namespace ArcGISRuntime.Samples.SetMinMaxScale
{
    [Register("SetMinMaxScale")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Set min & max scale",
        category: "Map",
        description: "Restrict zooming between specific scale ranges.",
        instructions: "Zoom in and out of the map. The zoom extents of the map are limited between the given minimum and maximum scales.",
        tags: new[] { "area of interest", "level of detail", "maximum", "minimum", "scale", "viewpoint" })]
    public class SetMinMaxScale : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        public SetMinMaxScale()
        {
            Title = "Set min & max scale";
        }

        private void Initialize()
        {
            // Create new Map with Streets basemap.
            Map myMap = new Map(Basemap.CreateStreets())
            {
                // Set the scale at which this layer can be viewed.
                // MinScale defines how far 'out' you can zoom.
                MinScale = 8000,
                // MaxScale defines how far 'in' you can zoom.
                MaxScale = 2000
            };

            // Create central point where map is centered.
            MapPoint centralPoint = new MapPoint(-355453, 7548720, SpatialReferences.WebMercator);

            // Create starting viewpoint.
            Viewpoint startingViewpoint = new Viewpoint(centralPoint, 3000);

            // Set starting viewpoint.
            myMap.InitialViewpoint = startingViewpoint;

            // Set map to mapview.
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