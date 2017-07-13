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
using Esri.ArcGISRuntime.Geometry;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ShowCallout
{
    [Register("ShowCallout")]
    public class ShowCallout : UIViewController
    {

        private MapView _myMapView;

        public ShowCallout()
        {
            Title = "Show callout";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create a new MapView control and provide its location coordinates on the frame
            _myMapView = new MapView();
            _myMapView.GeoViewTapped += _myMapView_GeoViewTapped;
            
            // Create a new Map instance with the basemap  
            var myBasemap = Basemap.CreateStreetsVector();

            Map myMap = new Map(myBasemap);

            // Assign the Map to the MapView
            _myMapView.Map = myMap;

            View.AddSubview(_myMapView);
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            base.ViewDidLayoutSubviews();
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
			_myMapView.ShowCalloutAt(mapLocation, new Esri.ArcGISRuntime.UI.CalloutDefinition("Location:",mapLocationString));
		}
    }
}