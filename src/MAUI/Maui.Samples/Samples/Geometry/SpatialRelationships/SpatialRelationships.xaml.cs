// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;

using Color = System.Drawing.Color;
using PointCollection = Esri.ArcGISRuntime.Geometry.PointCollection;

namespace ArcGIS.Samples.SpatialRelationships
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Spatial relationships",
        category: "Geometry",
        description: "Determine spatial relationships between two geometries.",
        instructions: "Select one of the three graphics. The tree view will list the relationships the selected graphic has to the other graphic geometries.",
        tags: new[] { "geometries", "relationship", "spatial analysis" })]
    public partial class SpatialRelationships : ContentPage
    {
        // References to the graphics and graphics overlay
        private GraphicsOverlay _graphicsOverlay;
        private Graphic _polygonGraphic;
        private Graphic _polylineGraphic;
        private Graphic _pointGraphic;

        public SpatialRelationships()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            // Configure the basemap
            MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic);

            // Create the graphics overlay
            _graphicsOverlay = new GraphicsOverlay();

            // Add the overlay to the MapView
            MyMapView.GraphicsOverlays.Add(_graphicsOverlay);

            // Update the selection color
            MyMapView.SelectionProperties.Color = Color.Yellow;

            // Create the point collection that defines the polygon
            PointCollection polygonPoints = new PointCollection(SpatialReferences.WebMercator)
            {
                new MapPoint(-5991501.677830, 5599295.131468),
                new MapPoint(-6928550.398185, 2087936.739807),
                new MapPoint(-3149463.800709, 1840803.011362),
                new MapPoint(-1563689.043184, 3714900.452072),
                new MapPoint(-3180355.516764, 5619889.608838)
            };

            // Create the polygon
            Polygon polygonGeometry = new Polygon(polygonPoints);

            // Define the symbology of the polygon
            SimpleLineSymbol polygonOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Green, 2);
            SimpleFillSymbol polygonFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.ForwardDiagonal, Color.Green, polygonOutlineSymbol);

            // Create the polygon graphic and add it to the graphics overlay
            _polygonGraphic = new Graphic(polygonGeometry, polygonFillSymbol);
            _graphicsOverlay.Graphics.Add(_polygonGraphic);

            // Create the point collection that defines the polyline
            PointCollection polylinePoints = new PointCollection(SpatialReferences.WebMercator)
            {
                new MapPoint(-4354240.726880, -609939.795721),
                new MapPoint(-3427489.245210, 2139422.933233),
                new MapPoint(-2109442.693501, 4301843.057130),
                new MapPoint(-1810822.771630, 7205664.366363)
            };

            // Create the polyline
            Polyline polylineGeometry = new Polyline(polylinePoints);

            // Create the polyline graphic and add it to the graphics overlay
            _polylineGraphic = new Graphic(polylineGeometry, new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.Red, 4));
            _graphicsOverlay.Graphics.Add(_polylineGraphic);

            // Create the point geometry that defines the point graphic
            MapPoint pointGeometry = new MapPoint(-4487263.495911, 3699176.480377, SpatialReferences.WebMercator);

            // Define the symbology for the point
            SimpleMarkerSymbol locationMarker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Blue, 10);

            // Create the point graphic and add it to the graphics overlay
            _pointGraphic = new Graphic(pointGeometry, locationMarker);
            _graphicsOverlay.Graphics.Add(_pointGraphic);

            // Listen for taps; the spatial relationships will be updated in the handler
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;

            // Set the viewpoint to center on the point
            MyMapView.SetViewpointCenterAsync(pointGeometry, 200000000);
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            // Identify the tapped graphics
            IdentifyGraphicsOverlayResult result = null;

            try
            {
                result = await MyMapView.IdentifyGraphicsOverlayAsync(_graphicsOverlay, e.Position, 5, false);
            }
            catch (Exception ex)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Error", ex.ToString(), "OK");
            }

            // Return if there are no results
            if (result == null || result.Graphics.Count < 1)
            {
                return;
            }

            // Get the first identified graphic
            Graphic identifiedGraphic = result.Graphics.First();

            // Clear any existing selection, then select the tapped graphic
            _graphicsOverlay.ClearSelection();
            identifiedGraphic.IsSelected = true;

            // Get the selected graphic's geometry
            Geometry selectedGeometry = identifiedGraphic.Geometry;

            // Perform the calculation and show the results
            ResultTextbox.Text = GetOutputText(selectedGeometry);
        }

        private string GetOutputText(Geometry selectedGeometry)
        {
            string output = "";

            // Get the relationships
            List<SpatialRelationship> polygonRelationships = GetSpatialRelationships(selectedGeometry, _polygonGraphic.Geometry);
            List<SpatialRelationship> polylineRelationships = GetSpatialRelationships(selectedGeometry, _polylineGraphic.Geometry);
            List<SpatialRelationship> pointRelationships = GetSpatialRelationships(selectedGeometry, _pointGraphic.Geometry);

            // Add the point relationships to the output
            if (selectedGeometry.GeometryType != GeometryType.Point)
            {
                output += "Point:\n";
                foreach (SpatialRelationship relationship in pointRelationships)
                {
                    output += $"\t{relationship}\n";
                }
            }
            // Add the polygon relationships to the output
            if (selectedGeometry.GeometryType != GeometryType.Polygon)
            {
                output += "Polygon:\n";
                foreach (SpatialRelationship relationship in polygonRelationships)
                {
                    output += $"\t{relationship}\n";
                }
            }
            // Add the polyline relationships to the output
            if (selectedGeometry.GeometryType != GeometryType.Polyline)
            {
                output += "Polyline:\n";
                foreach (SpatialRelationship relationship in polylineRelationships)
                {
                    output += $"\t{relationship}\n";
                }
            }

            return output;
        }

        /// <summary>
        /// Returns a list of spatial relationships between two geometries
        /// </summary>
        /// <param name="a">The 'a' in "a contains b"</param>
        /// <param name="b">The 'b' in "a contains b"</param>
        /// <returns>A list of spatial relationships that are true for a and b.</returns>
        private static List<SpatialRelationship> GetSpatialRelationships(Geometry a, Geometry b)
        {
            List<SpatialRelationship> relationships = new List<SpatialRelationship>();
            if (a.Crosses(b)) { relationships.Add(SpatialRelationship.Crosses); }
            if (a.Contains(b)) { relationships.Add(SpatialRelationship.Contains); }
            if (a.Disjoint(b)) { relationships.Add(SpatialRelationship.Disjoint); }
            if (a.Intersects(b)) { relationships.Add(SpatialRelationship.Intersects); }
            if (a.Overlaps(b)) { relationships.Add(SpatialRelationship.Overlaps); }
            if (a.Touches(b)) { relationships.Add(SpatialRelationship.Touches); }
            if (a.Within(b)) { relationships.Add(SpatialRelationship.Within); }
            return relationships;
        }
    }
}