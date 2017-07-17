// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;

namespace ArcGISRuntime.WPF.Samples.ShowCallout
{
    public partial class ShowCallout
    {
        public ShowCallout()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            // Create a new Map instance with the basemap
            Basemap myBasemap = Basemap.CreateStreets();
            Map myMap = new Map(myBasemap);

            // Assign the map to the MapView
            MyMapView.Map = myMap;

            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Get tap location
            MapPoint mapLocation = e.Location;

            // Convert to Traditional Lat/Lng display
            MapPoint projectedLocation = (MapPoint)GeometryEngine.Project(mapLocation, SpatialReferences.Wgs84);

            // Format string for display
            string mapLocationString = String.Format("Lat: {0:F3} Lng:{1:F3}", projectedLocation.Y, projectedLocation.X);

            // Display Callout
            MyMapView.ShowCalloutAt(mapLocation, new Esri.ArcGISRuntime.UI.CalloutDefinition("Location:", mapLocationString));
        }
    }
}