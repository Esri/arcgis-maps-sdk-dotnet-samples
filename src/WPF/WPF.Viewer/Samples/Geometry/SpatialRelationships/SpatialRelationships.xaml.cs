// Copyright 2018 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace ArcGIS.WPF.Samples.SpatialRelationships
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Spatial relationships",
        category: "Geometry",
        description: "Determine spatial relationships between two geometries.",
        instructions: "Select one of the three graphics. The tree view will list the relationships the selected graphic has to the other graphic geometries.",
        tags: new[] { "geometries", "relationship", "spatial analysis" })]
    public partial class SpatialRelationships
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
            SimpleLineSymbol polygonOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Green, 2);
            SimpleFillSymbol polygonFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.ForwardDiagonal, System.Drawing.Color.Green, polygonOutlineSymbol);

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
            _polylineGraphic = new Graphic(polylineGeometry, new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.Red, 4));
            _graphicsOverlay.Graphics.Add(_polylineGraphic);

            // Create the point geometry that defines the point graphic
            MapPoint pointGeometry = new MapPoint(-4487263.495911, 3699176.480377, SpatialReferences.WebMercator);

            // Define the symbology for the point
            SimpleMarkerSymbol locationMarker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Blue, 10);

            // Create the point graphic and add it to the graphics overlay
            _pointGraphic = new Graphic(pointGeometry, locationMarker);
            _graphicsOverlay.Graphics.Add(_pointGraphic);

            // Update the selection color.
            MyMapView.SelectionProperties.Color = Color.Yellow;

            // Listen for taps; the spatial relationships will be updated in the handler
            MyMapView.GeoViewTapped += MapViewTapped;

            // Set the viewpoint to center on the point
            MyMapView.SetViewpointCenterAsync(pointGeometry, 200000000);
        }

        private async void MapViewTapped(object sender, GeoViewInputEventArgs geoViewInputEventArgs)
        {
            IdentifyGraphicsOverlayResult result = null;

            try
            {
                // Identify the tapped graphics
                result = await MyMapView.IdentifyGraphicsOverlayAsync(_graphicsOverlay, geoViewInputEventArgs.Position, 1, false);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
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

            // Update the spatial relationship display
            switch (selectedGeometry.GeometryType)
            {
                case GeometryType.Point:
                    PointTreeEntry.ItemsSource = null;
                    PolygonTreeEntry.ItemsSource = GetSpatialRelationships(selectedGeometry, _polygonGraphic.Geometry);
                    PolylineTreeEntry.ItemsSource = GetSpatialRelationships(selectedGeometry, _polylineGraphic.Geometry);
                    break;

                case GeometryType.Polygon:
                    PolygonTreeEntry.ItemsSource = null;
                    PointTreeEntry.ItemsSource = GetSpatialRelationships(selectedGeometry, _pointGraphic.Geometry);
                    PolylineTreeEntry.ItemsSource = GetSpatialRelationships(selectedGeometry, _polylineGraphic.Geometry);
                    break;

                case GeometryType.Polyline:
                    PolylineTreeEntry.ItemsSource = null;
                    PointTreeEntry.ItemsSource = GetSpatialRelationships(selectedGeometry, _pointGraphic.Geometry);
                    PolygonTreeEntry.ItemsSource = GetSpatialRelationships(selectedGeometry, _polygonGraphic.Geometry);
                    break;
            }

            // Expand the tree view
            PolylineTreeEntry.IsExpanded = true;
            PolygonTreeEntry.IsExpanded = true;
            PointTreeEntry.IsExpanded = true;
        }

        /// <summary>
        /// Returns a list of spatial relationships between two geometries
        /// </summary>
        /// <param name="a">The 'a' in "a contains b"</param>
        /// <param name="b">The 'b' in "a contains b"</param>
        /// <returns>A list of spatial relationships that are true for a and b.</returns>
        private static IEnumerable<SpatialRelationship> GetSpatialRelationships(Geometry a, Geometry b)
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