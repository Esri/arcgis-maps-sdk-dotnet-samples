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
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI.Controls;
using System;

namespace ArcGISRuntimeXamarin.Samples.ShowCallout
{
    [Activity]
    public class ShowCallout : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Initialize();

            Title = "Show Callout";
        }

        void Initialize()
        {
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a new Map instance with the basemap  
            Basemap myBasemap = Basemap.CreateStreets();
            Map myMap = new Map(myBasemap);

            // Create a new map view control to display the map
            _myMapView = new MapView();
            _myMapView.Map = myMap;
            _myMapView.GeoViewTapped += _myMapView_GeoViewTapped;

            layout.AddView(_myMapView);

            // Apply the layout to the app
            SetContentView(layout);
        }

        void _myMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
			// Get tap location
			MapPoint mapLocation = e.Location;

			// Convert to Traditional Lat/Lng display
			MapPoint projectedLocation = (MapPoint)GeometryEngine.Project(mapLocation, SpatialReferences.Wgs84);

			// Format string for display
			string mapLocationString = String.Format("Lat: {0:F3} Lng:{1:F3}", projectedLocation.Y, projectedLocation.X);

			// Display Callout
			_myMapView.ShowCalloutAt(mapLocation, new Esri.ArcGISRuntime.UI.CalloutDefinition("Location:", mapLocationString));
        }
    }
}