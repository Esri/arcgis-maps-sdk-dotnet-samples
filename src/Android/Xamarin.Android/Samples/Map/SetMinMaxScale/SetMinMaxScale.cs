// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;

namespace ArcGISRuntimeXamarin.Samples.SetMinMaxScale
{
    [Activity]
    public class SetMinMaxScale : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Set Min & Max Scale";

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
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}