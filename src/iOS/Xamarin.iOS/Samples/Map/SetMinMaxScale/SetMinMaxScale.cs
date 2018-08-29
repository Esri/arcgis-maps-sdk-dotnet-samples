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
        "Set min & max scale",
        "Map",
        "This sample demonstrates how to set the minimum and maximum scale of a Map. Setting the minimum and maximum scale for the Map can be useful in keeping the user focused at a certain level of detail.",
        "")]
    public class SetMinMaxScale : UIViewController
    {
        // Create and hold reference to the MapView.
        private MapView _myMapView;

        public SetMinMaxScale()
        {
            Title = "Set min & max scale";
        }

        public override void LoadView()
        {
            base.LoadView();

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubviews(_myMapView);

            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Initialize();
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
    }
}