// Copyright 2017 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ShowCallout
{
    [Register("ShowCallout")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Show callout",
        "MapView",
        "This sample illustrates how to show callouts on a map in response to user interaction.",
        "Tap on the map to show that point's coordinates.")]
    public class ShowCallout : UIViewController
    {
        private MapView _myMapView = new MapView();

        public ShowCallout()
        {
            Title = "Show callout";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create a new basemap using the streets base layer
            Basemap myBasemap = Basemap.CreateStreets();

            // Create a new map based on the streets basemap
            Map myMap = new Map(myBasemap);

            // Assign the map to the MapView
            _myMapView.Map = myMap;

            // Wire up the MapView GeoVewTapped event
            _myMapView.GeoViewTapped += _myMapView_GeoViewTapped;

            // Add the MapView to the page
            View.AddSubview(_myMapView);
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            base.ViewDidLayoutSubviews();
        }

        private void _myMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Get the user-tapped location
            MapPoint mapLocation = e.Location;

            // Project the user-tapped map point location to a geometry
            Geometry myGeometry = GeometryEngine.Project(mapLocation, SpatialReferences.Wgs84);

            // Convert to geometry to a traditional Lat/Long map point
            MapPoint projectedLocation = (MapPoint)myGeometry;

            // Format the display callout string based upon the projected map point (example: "Lat: 100.123, Long: 100.234")
            string mapLocationDescription = string.Format("Lat: {0:F3} Long:{1:F3}", projectedLocation.Y, projectedLocation.X);

            // Create a new callout definition using the formatted string
            CalloutDefinition myCalloutDefinition = new CalloutDefinition("Location:", mapLocationDescription);

            // Display the callout
            _myMapView.ShowCalloutAt(mapLocation, myCalloutDefinition);
        }
    }
}