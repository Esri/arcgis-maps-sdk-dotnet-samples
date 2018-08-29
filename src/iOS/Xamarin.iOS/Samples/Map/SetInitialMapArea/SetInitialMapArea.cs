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
        "Set initial map area",
        "Map",
        "This sample demonstrates how to set the initial viewpoint from envelope defined by minimum (x,y) and maximum (x,y) values. The map's InitialViewpoint is set to this viewpoint before the map is loaded into the MapView. Upon loading the map zoom to this initial area.",
        "")]
    public class SetInitialMapArea : UIViewController
    {
        // Create and hold a reference to the MapView.
        private MapView _myMapView;

        public SetInitialMapArea()
        {
            Title = "Set initial map area";
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
            // Create new Map with basemap.
            Map myMap = new Map(Basemap.CreateImagery());

            // Create and set initial map area.
            Envelope initialLocation = new Envelope(-12211308.778729, 4645116.003309, -12208257.879667, 4650542.535773, SpatialReferences.WebMercator);
            myMap.InitialViewpoint = new Viewpoint(initialLocation);

            // Provide used Map to the MapView.
            _myMapView.Map = myMap;
        }
    }
}