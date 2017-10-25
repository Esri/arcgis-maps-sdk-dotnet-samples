﻿// Copyright 2016 Esri.
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
using System;
using System.Collections.Generic;
using Xamarin.Forms;
#if WINDOWS_UWP
using Colors = Windows.UI.Colors;
#else
using Colors = System.Drawing.Color;
#endif

namespace ArcGISRuntimeXamarin.Samples.CreateFeatureCollectionLayer
{
	public partial class CreateFeatureCollectionLayer : ContentPage
	{
		public CreateFeatureCollectionLayer ()
		{
            InitializeComponent ();

            Title = "Create a feature collection layer";

            // call a function to initialize a map to display in the MyMapView control
            Initialize();
		}

        private void Initialize()
        {
            try
            {
                // Create a new map with the oceans basemap and add it to the map view
                Map myMap = new Map(Basemap.CreateOceans());
                MyMapView.Map = myMap;

                // Call a function that will create a new feature collection layer and zoom to it
                CreateNewFeatureCollection();
            }
            catch (Exception ex)
            {
                DisplayAlert("Error", "Unable to create feature collection layer: " + ex.Message, "OK");
            }
        }

        private async void CreateNewFeatureCollection()
        {
            // Create the schema for a points table (one text field to contain a name attribute)
            List<Field> pointFields = new List<Field>();
            Field placeField = new Field(FieldType.Text, "Place", "Place Name", 50);
            pointFields.Add(placeField);

            // Create the schema for a lines table (one text field to contain a name attribute)
            List<Field> lineFields = new List<Field>();
            Field boundaryField = new Field(FieldType.Text, "Boundary", "Boundary Name", 50);
            lineFields.Add(boundaryField);

            // Create the schema for a polygon table (one text field to contain a name attribute)
            List<Field> polyFields = new List<Field>();
            Field areaField = new Field(FieldType.Text, "AreaName", "Area Name", 50);
            polyFields.Add(areaField);

            // Instantiate FeatureCollectionTables with schema and geometry type
            FeatureCollectionTable pointsTable = new FeatureCollectionTable(pointFields, GeometryType.Point, SpatialReferences.Wgs84);
            FeatureCollectionTable linesTable = new FeatureCollectionTable(lineFields, GeometryType.Polyline, SpatialReferences.Wgs84);
            FeatureCollectionTable polysTable = new FeatureCollectionTable(polyFields, GeometryType.Polygon, SpatialReferences.Wgs84);

            // Set rendering for each table
            pointsTable.Renderer = CreateRenderer(GeometryType.Point);
            linesTable.Renderer = CreateRenderer(GeometryType.Polyline);
            polysTable.Renderer = CreateRenderer(GeometryType.Polygon);

            // Create a new point feature, provide geometry and attribute values
            Feature pointFeature = pointsTable.CreateFeature();
            pointFeature.SetAttributeValue(placeField, "Current location");
            MapPoint point1 = new MapPoint(-79.497238, 8.849289, SpatialReferences.Wgs84);
            pointFeature.Geometry = point1;

            // Create a new line feature, provide geometry and attribute values
            Feature lineFeature = linesTable.CreateFeature();
            lineFeature.SetAttributeValue(boundaryField, "AManAPlanACanalPanama");
            MapPoint point2 = new MapPoint(-80.035568, 9.432302, SpatialReferences.Wgs84);
            Polyline line = new Polyline(new MapPoint[] { point1, point2 });
            lineFeature.Geometry = line;

            // Create a new polygon feature, provide geometry and attribute values
            Feature polyFeature = polysTable.CreateFeature();
            polyFeature.SetAttributeValue(areaField, "Restricted area");
            MapPoint point3 = new MapPoint(-79.337936, 8.638903, SpatialReferences.Wgs84);
            MapPoint point4 = new MapPoint(-79.11409, 8.895422, SpatialReferences.Wgs84);
            Polygon poly = new Polygon(new MapPoint[] { point1, point3, point4 });
            polyFeature.Geometry = poly;

            // Add the new features to the appropriate feature collection table 
            await pointsTable.AddFeatureAsync(pointFeature);
            await linesTable.AddFeatureAsync(lineFeature);
            await polysTable.AddFeatureAsync(polyFeature);

            // Create a feature collection and add the feature collection tables
            FeatureCollection featuresCollection = new FeatureCollection();
            featuresCollection.Tables.Add(pointsTable);
            featuresCollection.Tables.Add(linesTable);
            featuresCollection.Tables.Add(polysTable);

            // Create a FeatureCollectionLayer 
            FeatureCollectionLayer collectionLayer = new FeatureCollectionLayer(featuresCollection);

            // When the layer loads, zoom the map view to the extent of the feature collection
            collectionLayer.Loaded += (s, e) => Device.BeginInvokeOnMainThread(async() =>
            {
                await MyMapView.SetViewpointAsync(new Viewpoint(collectionLayer.FullExtent));
            });

            // Add the layer to the Map's Operational Layers collection
            MyMapView.Map.OperationalLayers.Add(collectionLayer);
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
                    sym = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, Colors.Red, 18);
                    break;
                case GeometryType.Polyline:
                    // Create a line symbol
                    sym = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Colors.Green, 3);
                    break;
                case GeometryType.Polygon:
                    // Create a fill symbol
                    var lineSym = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.DarkBlue, 2);
                    sym = new SimpleFillSymbol(SimpleFillSymbolStyle.DiagonalCross, Colors.Cyan, lineSym);
                    break;
                default:
                    break;
            }

            // Return a new renderer that uses the symbol created above
            return new SimpleRenderer(sym);
        }
    }
}
