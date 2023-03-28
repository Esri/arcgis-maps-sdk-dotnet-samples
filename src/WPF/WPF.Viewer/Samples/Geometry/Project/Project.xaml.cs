// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System.Threading.Tasks;
using Color = System.Drawing.Color;

namespace ArcGIS.WPF.Samples.Project
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Project",
        category: "Geometry",
        description: "Project a point from one spatial reference to another.",
        instructions: "Click anywhere on the map. A callout will display the clicked location's coordinate in the original (basemap's) spatial reference and in the projected spatial reference.",
        tags: new[] { "WGS 84", "Web Mercator", "coordinate system", "coordinates", "latitude", "longitude", "projected", "projection", "spatial reference" })]
    public partial class Project
    {
        public Project()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Show a map in the default WebMercator spatial reference.
            MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic);

            // Add a graphics overlay for showing the tapped point.
            GraphicsOverlay overlay = new GraphicsOverlay();
            SimpleMarkerSymbol markerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Red, 5);
            overlay.Renderer = new SimpleRenderer(markerSymbol);
            MyMapView.GraphicsOverlays.Add(overlay);

            // Respond to user taps.
            MyMapView.GeoViewTapped += MapView_Tapped;

            // Zoom to Minneapolis.
            Envelope startingEnvelope = new Envelope(-10995912.335747, 5267868.874421, -9880363.974046, 5960699.183877,
                SpatialReferences.WebMercator);
            await MyMapView.SetViewpointGeometryAsync(startingEnvelope);
        }

        private void MapView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            // Get the tapped point - this is in the map's spatial reference,
            // which in this case is WebMercator because that is the SR used by the included basemaps.
            MapPoint tappedPoint = e.Location;

            // Update the graphics.
            MyMapView.GraphicsOverlays[0].Graphics.Clear();
            MyMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(tappedPoint));

            // Project the point to WGS84
            MapPoint projectedPoint = (MapPoint)tappedPoint.Project(SpatialReferences.Wgs84);

            // Format the results in strings.
            string originalCoords = string.Format("Original: {0:F4}, {1:F4}", tappedPoint.X, tappedPoint.Y);
            string projectedCoords = string.Format("Projected: {0:F4}, {1:F4}", projectedPoint.X, projectedPoint.Y);
            string formattedString = string.Format("{0}\n{1}", originalCoords, projectedCoords);

            // Define a callout and show it in the map view.
            CalloutDefinition calloutDef = new CalloutDefinition("Coordinates:", formattedString);
            MyMapView.ShowCalloutAt(tappedPoint, calloutDef);
        }
    }
}