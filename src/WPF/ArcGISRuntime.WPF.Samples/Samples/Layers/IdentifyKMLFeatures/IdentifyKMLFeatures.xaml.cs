// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Data;
using System;
using Esri.ArcGISRuntime.Geometry;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace ArcGISRuntime.WPF.Samples.IdentifyKMLFeatures
{
    public partial class IdentifyKMLFeatures
    {
        public IdentifyKMLFeatures()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();

            MyMapView.GraphicsOverlays.Add(new GraphicsOverlay());

            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private async void Initialize()
        {
            // Create a new map
            Map myMap = new Map();

            // Define the basemap
            myMap.Basemap = Basemap.CreateDarkGrayCanvasVector();

            // Create a Uri for the Kml data from ArcGIS Server using a Query with the "f-kmz" parameter option
            // See the ArcGIS Server REST API documentation for more information: http://sampleserver1.arcgisonline.com/ArcGIS/SDK/REST/index.html?query.html
            Uri myKmlUri = new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Specialty/ESRI_StatesCitiesRivers_USA/MapServer/2/query?text=&geometry=&geometryType=esriGeometryPoint&inSR=&spatialRel=esriSpatialRelIntersects&relationParam=&objectIds=&where=1%3D1&time=&returnCountOnly=false&returnIdsOnly=false&returnGeometry=true&maxAllowableOffset=&outSR=&outFields=&f=kmz");

            // Create a Kml dataset from the Uri
            KmlDataset myKmlDataset = new KmlDataset(myKmlUri);

            // Create a new instance of a KmlLayer layer 
            KmlLayer myKmlLayer = new KmlLayer(myKmlDataset);

            // Add the KmLayer to the map's operational layers
            myMap.OperationalLayers.Add(myKmlLayer);

            // Assign the map to the MapView
            MyMapView.Map = myMap;

            // Define and extent to zoom to (in the case: the continental United States)
            Envelope myExtent = new Envelope(-14029640.7178862, 2641221.12421063, -7285597.58055383, 6520185.12465264, SpatialReferences.WebMercator);

            // Zoom to the extent
            await MyMapView.SetViewpointGeometryAsync(myExtent);
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Get the graphics overlay collection
            GraphicsOverlayCollection myGraphicsOverlayerCollection = MyMapView.GraphicsOverlays;

            if (myGraphicsOverlayerCollection.Count > 0)
            {
                // Get the first graphics overlay in the collection
                GraphicsOverlay myGraphicsOverlay = myGraphicsOverlayerCollection[0];

                // Get the graphic collection 
                GraphicCollection myGraphicCollection = myGraphicsOverlay.Graphics;

                // Clear out all of the graphics in the graphic collection
                myGraphicCollection.Clear();
            }

            // Set the maximum number of features to be selected/returned from the identify operation
            long myMaxSelectedFeatures = 50;

            // Set the search tolerance in pixels for the identify to find features from where the user touched on the map 
            double myTolerance = 3;

            try
            {
                // Get the location on the map where the user touched (tapped, mouse click, etc.)
                var myScreenPoint = e.Position;

                // Get the map from the map view
                Esri.ArcGISRuntime.Mapping.Map myMap = MyMapView.Map;

                // Get the collection of all layers in the map
                IReadOnlyList<Layer> myReadOnlyListOfLayers = myMap.AllLayers;

                // Get the second item in the collection of layers (it should be the KML layer)
                Layer myLayer = myReadOnlyListOfLayers[1];

                // Get the identify layer result from where the user touched on the map subject to the parameters entered
                IdentifyLayerResult myIdentifyLayerResult = await MyMapView.IdentifyLayerAsync(myLayer, myScreenPoint, myTolerance, false, myMaxSelectedFeatures);

                // Get the collection of identify layer result values from the sub layers
                IReadOnlyList<IdentifyLayerResult> mySublayerResults = myIdentifyLayerResult.SublayerResults;

                // Proceed if we have at least one result
                if (mySublayerResults.Count > 0)
                {
                    // Loop through each identify layer result
                    foreach (var oneIdentifyLayerResult in mySublayerResults)
                    {
                        // Get the geo element (shape + attributes) for the identify layer result
                        IReadOnlyList<GeoElement> myGeoElements = oneIdentifyLayerResult.GeoElements;

                        // Loop through each geo element
                        foreach (var oneGeoElement in myGeoElements)
                        {
                            // Get the geometry from the geo element
                            Esri.ArcGISRuntime.Geometry.Geometry myGeometry = oneGeoElement.Geometry;

                            // Create symbol for the polygon
                            SimpleFillSymbol mySimpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Colors.Yellow, null);

                            // Create new graphic using the geometry and symbol to display to the user
                            Graphic myPolygonGraphic = new Graphic(myGeometry, mySimpleFillSymbol);

                            // Create overlay to where graphics are shown
                            MyMapView.GraphicsOverlays[0].Graphics.Add(myPolygonGraphic);

                            // TODO:
                            // It might be confusing to have GraphicsOverlays in the mix. 
                            // It might be simpler to use ShowCalloutAt and not do any other presentation part.

                            string keyValues = "";

                            // NOTE: the field names that are returned are the alias and not the actual field name. If the
                            // dynamic layer is based upon a table join, there could be field names (aka aliases) that are the
                            // same in both tables. Duplicate field names are not returned.
                            foreach (KeyValuePair<string, object> element in oneGeoElement.Attributes)
                            {
                                keyValues += element.Key + " = " + (element.Value ?? "Null").ToString() + Environment.NewLine;
                            }

                            MessageBox.Show(keyValues);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If there was a problem display it to the user
                MessageBox.Show(ex.Message);
            }
        }

    }
}