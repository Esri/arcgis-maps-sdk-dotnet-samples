// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System.Collections.Generic;
using System.Drawing;

namespace ArcGISRuntimeXamarin.Samples.SpatialOperations
{
    [Activity]
    public class SpatialOperations : Activity
    {
        // MapView control to display the operation polygons.
        MapView _myMapView = new MapView();

        // GraphicsOverlay to hold the polygon graphics.
        private GraphicsOverlay _polygonsOverlay;

        // Polygon graphics to run spatial operations on.
        private Graphic _graphicOne;
        private Graphic _graphicTwo;

        // Graphic to display the spatial operation result polygon.
        private Graphic _resultGraphic;

        // Picker control to choose a spatial operation.
        Spinner _operationPicker;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Spatial operations";

            // Create the UI.
            CreateLayout();

            // Create a new map, add polygon graphics, and fill the spatial operations list.
            Initialize();
        }

        private void Initialize()
        {
            // Create the map with a gray canvas basemap and an initial location centered on London, UK.
            Map myMap = new Map(BasemapType.LightGrayCanvas, 51.5017, -0.12714, 14);

            // Add the map to the map view.
            _myMapView.Map = myMap;

            // Create and add two overlapping polygon graphics to operate on.
            CreatePolygonsOverlay();
        }

        private void CreateLayout()
        {
            // Create a label to prompt for the spatial operation.
            TextView operationLabel = new TextView(this)
            {
                Text = "Choose a spatial operation:",
                TextAlignment = TextAlignment.ViewStart
            };
            
            // Create a list of spatial operations and use it to create an array adapter.
            List<string> operationsList = new List<string> { "", "Difference", "Intersection", "Symmetric difference", "Union" };
            ArrayAdapter operationsAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, operationsList);

            // Create a picker (Spinner) to show the list of operations.
            _operationPicker = new Spinner(this)
            {
                Adapter = operationsAdapter
            };
            _operationPicker.SetPadding(5, 10, 0, 10);

            // Handle the selection event to apply the selected operation.
            _operationPicker.ItemSelected += OperationsPicker_ItemSelected;

            // Create a button to clear the spatial operation result.
            Button resetOperationButton = new Button(this)
            {
                Text = "Reset"
            };
            resetOperationButton.Click += ResetOperationButton_Click;

            // Create a layout for the app tools.
            LinearLayout toolsLayout = new LinearLayout(this) { Orientation = Orientation.Vertical };
            toolsLayout.SetPadding(10, 0, 0, 0);
            toolsLayout.SetMinimumHeight(250);

            // Add the controls to the tools layout (label, picker, button).
            toolsLayout.AddView(operationLabel);
            toolsLayout.AddView(_operationPicker);
            toolsLayout.AddView(resetOperationButton);

            // Create a new vertical layout for the app UI.
            LinearLayout mainLayout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the tools layout and map view to the main layout.
            mainLayout.AddView(toolsLayout);
            mainLayout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(mainLayout);
        }

        private void ResetOperationButton_Click(object sender, System.EventArgs e)
        {
            // Remove any currently displayed result.
            _polygonsOverlay.Graphics.Remove(_resultGraphic);

            // Clear the selected spatial operation.
            _operationPicker.SetSelection(0);
        }

        private void OperationsPicker_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            // If an operation hasn't been selected, return.
            if (_operationPicker.SelectedItem.ToString() == "") { return; }

            // Remove any currently displayed result.
            _polygonsOverlay.Graphics.Remove(_resultGraphic);

            // Polygon geometry from the input graphics.
            Geometry polygonOne = _graphicOne.Geometry;
            Geometry polygonTwo = _graphicTwo.Geometry;

            // Result polygon for spatial operations.
            Geometry resultPolygon = null;

            // Run the selected spatial operation on the polygon graphics and get the result geometry.
            string selectedOperation = _operationPicker.SelectedItem.ToString();
            switch (selectedOperation)
            {
                case "Union":
                    resultPolygon = GeometryEngine.Union(polygonOne, polygonTwo);
                    break;
                case "Difference":
                    resultPolygon = GeometryEngine.Difference(polygonOne, polygonTwo);
                    break;
                case "Symmetric difference":
                    resultPolygon = GeometryEngine.SymmetricDifference(polygonOne, polygonTwo);
                    break;
                case "Intersection":
                    resultPolygon = GeometryEngine.Intersection(polygonOne, polygonTwo);
                    break;
            }

            // Create a black outline symbol to use for the result polygon.
            SimpleLineSymbol outlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Black, 1);

            // Create a solid red fill symbol for the result polygon graphic.
            SimpleFillSymbol resultSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.Red, outlineSymbol);

            // Create the result polygon graphic and add it to the graphics overlay.
            _resultGraphic = new Graphic(resultPolygon, resultSymbol);
            _polygonsOverlay.Graphics.Add(_resultGraphic);
        }

        private void CreatePolygonsOverlay()
        {
            // Create a black outline symbol to use for the polygons.
            SimpleLineSymbol outlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Black, 1);

            // Create a point collection to define polygon vertices.
            PointCollection polygonVertices = new PointCollection(SpatialReferences.WebMercator)
            {
                new MapPoint(-13960, 6709400),
                new MapPoint(-14660, 6710000),
                new MapPoint(-13760, 6710730),
                new MapPoint(-13300, 6710500),
                new MapPoint(-13160, 6710100)
            };

            // Create a polygon graphic with a blue fill.
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Vertical, Color.Blue, outlineSymbol);
            Polygon polygonOne = new Polygon(polygonVertices);
            _graphicOne = new Graphic(polygonOne, fillSymbol);

            // Create a point collection to define outer polygon ring vertices.
            PointCollection outerRingVerticesCollection = new PointCollection(SpatialReferences.WebMercator)
            {
                new MapPoint(-13060, 6711030),
                new MapPoint(-12160, 6710730),
                new MapPoint(-13160, 6709700),
                new MapPoint(-14560, 6710730)
            };

            // Create a point collection to define inner polygon ring vertices ("donut hole").
            PointCollection innerRingVerticesCollection = new PointCollection(SpatialReferences.WebMercator)
            {
                new MapPoint(-13060, 6710910),
                new MapPoint(-14160, 6710630),
                new MapPoint(-13160, 6709900),
                new MapPoint(-12450, 6710660)
            };

            // Create a list to contain the inner and outer ring point collections.
            List<PointCollection> polygonParts = new List<PointCollection>
            {
                outerRingVerticesCollection,
                innerRingVerticesCollection
            };

            // Create a polygon graphic with a green fill.
            fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Horizontal, Color.Green, outlineSymbol);
            _graphicTwo = new Graphic(new Polygon(polygonParts), fillSymbol);

            // Create a graphics overlay in the map view to hold the polygons.
            _polygonsOverlay = new GraphicsOverlay();
            _myMapView.GraphicsOverlays.Add(_polygonsOverlay);

            // Add the polygons to the graphics overlay.
            _polygonsOverlay.Graphics.Add(_graphicOne);
            _polygonsOverlay.Graphics.Add(_graphicTwo);
        }
    }
}