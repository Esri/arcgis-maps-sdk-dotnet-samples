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
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ArcGISRuntimeXamarin.Samples.CreateFeatureCollectionLayer
{
    [Activity(Label = "CreateFeatureCollectionLayer")]
    public class CreateFeatureCollectionLayer : Activity
    {
        private MapView _myMapView = new MapView();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Create feature collection layer";

            // Create the UI
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                // Create a new map with the oceans basemap and add it to the map view
                var map = new Map(Basemap.CreateOceans());
                _myMapView.Map = map;

                // Call a function that will create a new feature collection layer and zoom to it
                CreateNewFeatureCollection();
            }
            catch (Exception ex)
            {
                var alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("Error");
                alertBuilder.SetMessage("Unable to create feature collection layer: " + ex.Message);
                alertBuilder.Show();
            }
        }

        private void CreateLayout()
        {
            // Create a new layout
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }

        private async void CreateNewFeatureCollection()
        {
            // Create the schema for a points table (one text field to contain a name attribute)
            var pointFields = new List<Field>();
            var placeField = new Field(FieldType.Text, "Place", "Place Name", 50);
            pointFields.Add(placeField);

            // Create the schema for a lines table (one text field to contain a name attribute)
            var lineFields = new List<Field>();
            var boundaryField = new Field(FieldType.Text, "Boundary", "Boundary Name", 50);
            lineFields.Add(boundaryField);

            // Create the schema for a polygon table (one text field to contain a name attribute)
            var polyFields = new List<Field>();
            var areaField = new Field(FieldType.Text, "AreaName", "Area Name", 50);
            polyFields.Add(areaField);

            // Instantiate FeatureCollectionTables with schema and geometry type
            var pointsTable = new FeatureCollectionTable(pointFields, GeometryType.Point, SpatialReferences.Wgs84);
            var linesTable = new FeatureCollectionTable(lineFields, GeometryType.Polyline, SpatialReferences.Wgs84);
            var polysTable = new FeatureCollectionTable(polyFields, GeometryType.Polygon, SpatialReferences.Wgs84);

            // Set rendering for each table
            pointsTable.Renderer = CreateRenderer(GeometryType.Point);
            linesTable.Renderer = CreateRenderer(GeometryType.Polyline);
            polysTable.Renderer = CreateRenderer(GeometryType.Polygon);

            // Create a new point feature, provide geometry and attribute values
            var pointFeature = pointsTable.CreateFeature();
            pointFeature.SetAttributeValue(placeField, "Current location");
            var point1 = new MapPoint(-79.497238, 8.849289, SpatialReferences.Wgs84);
            pointFeature.Geometry = point1;

            // Create a new line feature, provide geometry and attribute values
            var lineFeature = linesTable.CreateFeature();
            lineFeature.SetAttributeValue(boundaryField, "AManAPlanACanalPanama");
            var point2 = new MapPoint(-80.035568, 9.432302, SpatialReferences.Wgs84);
            var line = new Polyline(new MapPoint[] { point1, point2 });
            lineFeature.Geometry = line;

            // Create a new polygon feature, provide geometry and attribute values
            var polyFeature = polysTable.CreateFeature();
            polyFeature.SetAttributeValue(areaField, "Restricted area");
            var point3 = new MapPoint(-79.337936, 8.638903, SpatialReferences.Wgs84);
            var point4 = new MapPoint(-79.11409, 8.895422, SpatialReferences.Wgs84);
            var poly = new Polygon(new MapPoint[] { point1, point3, point4 });
            polyFeature.Geometry = poly;

            // Add the new features to the appropriate feature collection table 
            await pointsTable.AddFeatureAsync(pointFeature);
            await linesTable.AddFeatureAsync(lineFeature);
            await polysTable.AddFeatureAsync(polyFeature);

            // Create a feature collection and add the feature collection tables
            var featuresCollection = new FeatureCollection();
            featuresCollection.Tables.Add(pointsTable);
            featuresCollection.Tables.Add(linesTable);
            featuresCollection.Tables.Add(polysTable);

            // Create a FeatureCollectionLayer and add to the Map's Operational Layers collection
            var collectionLayer = new FeatureCollectionLayer(featuresCollection);
            _myMapView.Map.OperationalLayers.Add(collectionLayer);

            // Zoom the map view to the extent of the feature collection
            _myMapView.SetViewpointAsync(new Viewpoint(collectionLayer.FullExtent));
        }

        private Renderer CreateRenderer(GeometryType rendererType)
        {
            // Return a simple renderer to match the geometry type provided
            Symbol sym = null;

            switch (rendererType)
            {
                case GeometryType.Point:
                case GeometryType.Multipoint:
                    // Create a marker symbol
                    sym = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, Color.Red, 18);
                    break;
                case GeometryType.Polyline:
                    // Create a line symbol
                    sym = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.Green, 3);
                    break;
                case GeometryType.Polygon:
                    // Create a fill symbol
                    var lineSym = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.DarkBlue, 2);
                    sym = new SimpleFillSymbol(SimpleFillSymbolStyle.DiagonalCross, Color.Cyan, lineSym);
                    break;
                default:
                    break;
            }

            // Return a new renderer that uses the symbol created above
            return new SimpleRenderer(sym);
        }
    }
}