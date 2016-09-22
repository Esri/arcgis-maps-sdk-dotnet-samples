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
using Esri.ArcGISRuntime.UI;
using Foundation;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.SetMinMaxScale
{
    [Register("SetMinMaxScale")]
    public class SetMinMaxScale : UIViewController
    {
        // Constant holding offset where the MapView control should start
        private const int yPageOffset = 60;

        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        public SetMinMaxScale()
        {
            Title = "Set Min & Max Scale";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
           
            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with Streets basemap 
            Map myMap = new Map(Basemap.CreateStreets());

            // Set the scale at which this layer can be viewed
            // MinScale defines how far 'out' you can zoom where
            // MaxScale defines how far 'in' you can zoom.
            myMap.MinScale = 8000;
            myMap.MaxScale = 2000;

            // Create central point where map is centered
            MapPoint centralPoint = new MapPoint(-355453, 7548720, SpatialReferences.WebMercator);

            // Create starting viewpoint
            Viewpoint startingViewpoint = new Viewpoint(
                centralPoint,
                3000);
            // Set starting viewpoint
            myMap.InitialViewpoint = startingViewpoint;

            // Set map to mapview
            _myMapView.Map = myMap;
        }

        private void CreateLayout()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(
                0, yPageOffset, View.Bounds.Width, View.Bounds.Height - yPageOffset);

            // Add MapView to the page
            View.AddSubviews(_myMapView);
        }
    }
}