// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Maui;
using Esri.ArcGISRuntime.UI;

namespace ArcGIS.Samples.ShowCallout
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Show callout",
        category: "MapView",
        description: "Show a callout with the latitude and longitude of user-tapped points.",
        instructions: "Tap anywhere on the map. A callout showing the WGS84 coordinates for the tapped point will appear.",
        tags: new[] { "balloon", "bubble", "callout", "flyout", "flyover", "info window", "popup", "tap" })]
    public partial class ShowCallout : ContentPage
    {
        public ShowCallout()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            // Create a new basemap using the streets base layer
            Basemap myBasemap = new Basemap(BasemapStyle.ArcGISStreets);

            // Create a new map based on the streets basemap
            Map myMap = new Map(myBasemap);

            // Assign the map to the MapView
            MyMapView.Map = myMap;

            // Wire up the MapView GeoVewTapped event
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Get the user-tapped location
            MapPoint mapLocation = e.Location;

            // Project the user-tapped map point location to a geometry
            Geometry myGeometry = mapLocation.Project(SpatialReferences.Wgs84);

            // Convert to geometry to a traditional Lat/Long map point
            MapPoint projectedLocation = (MapPoint)myGeometry;

            // Format the display callout string based upon the projected map point (example: "Lat: 100.123, Long: 100.234")
            string mapLocationDescription = $"Lat: {projectedLocation.Y:F3} Long:{projectedLocation.X:F3}";

            // Create a new callout definition using the formatted string
            CalloutDefinition myCalloutDefinition = new CalloutDefinition("Location:", mapLocationDescription);

            // Display the callout
            MyMapView.ShowCalloutAt(mapLocation, myCalloutDefinition);
        }
    }
}