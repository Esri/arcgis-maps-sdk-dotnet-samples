// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.SpatialOperations
{
    [Register("SpatialOperations")]
    public class SpatialOperations : UIViewController
    {
        // Map view control to display a map in the app.
        private MapView _myMapView = new MapView();

        // GraphicsOverlay to hold the polygon graphics.
        private GraphicsOverlay _polygonsOverlay;

        // Polygon graphics to run spatial operations on.
        private Graphic _graphicOne;
        private Graphic _graphicTwo;

        // Graphic to display the spatial operation result polygon.
        private Graphic _resultGraphic;

        // Stack view to contain the spatial operation UI.
        private UIStackView _operationToolsView = new UIStackView();
        
        // Picker to display the spatial operation choices.
        private UIPickerView _operationPicker;

        public SpatialOperations()
        {
            Title = "Spatial operations";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI.
            CreateLayout();

            // Create a new map, add polygon graphics, and fill the spatial operations list.
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // Get the height of the map view and the UI tools view (one third / two thirds).
            var operationsToolsHeight = (nfloat)(View.Bounds.Height * 0.33);
            var mapViewHeight = (nfloat)(View.Bounds.Height * 0.67);

            // Place the MapView (top 2/3 of the view).
            _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, mapViewHeight);

            // Place the UI tools (bottom 1/3 of the view).
            _operationToolsView.Frame = new CGRect(0, mapViewHeight, View.Bounds.Width, operationsToolsHeight);
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
            // Lay out the spatial operations UI vertically.
            _operationToolsView.Axis = UILayoutConstraintAxis.Vertical;
           
            // Create a label to prompt for a spatial operation.
            UILabel operationLabel = new UILabel(new CGRect(5, 0, View.Bounds.Width, 30))
            {
                Text = "Choose a spatial operation:",
                TextAlignment = UITextAlignment.Left,
                TextColor = UIColor.Blue
            };

            // Create a picker model with spatial operation choices.
            List<string> operationsList = new List<string> { "", "Difference", "Intersection", "Symmetric difference", "Union" };
            PickerDataModel operationModel = new PickerDataModel(operationsList);

            // Handle the selection change for spatial operations.
            operationModel.ValueChanged += OperationModel_ValueChanged;
            
            // Create a picker to show the spatial operations.
            _operationPicker = new UIPickerView(new CGRect(20, 25, View.Bounds.Width - 20, 100))
            {
                Model = operationModel
            };

            // Create a button to reset the operation result.
            UIButton resetButton = new UIButton(UIButtonType.Plain);
            resetButton.Frame = new CGRect(20,110,View.Bounds.Width-20,30);
            resetButton.SetTitle("Reset operation", UIControlState.Normal);
            resetButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            resetButton.TouchUpInside += ResetButton_TouchUpInside;

            // Add the controls to the tools UI (stack view).
            _operationToolsView.AddSubviews(operationLabel, _operationPicker, resetButton);

            // Add the map view and tools sub-views to the view.
            View.AddSubviews(_myMapView, _operationToolsView);

            // Set the view background color.
            View.BackgroundColor = UIColor.White;
        }

        // Handle selection events in the spatial operations picker.
        private void OperationModel_ValueChanged(object sender, EventArgs e)
        {
            // Get the data model that contains the spatial operation choices.
            PickerDataModel operationsModel = _operationPicker.Model as PickerDataModel;

            // If an operation hasn't been selected, return.
            if (operationsModel.SelectedItem == "") { return; }

            // Remove any currently displayed result.
            _polygonsOverlay.Graphics.Remove(_resultGraphic);

            // Polygon geometry from the input graphics.
            Geometry polygonOne = _graphicOne.Geometry;
            Geometry polygonTwo = _graphicTwo.Geometry;

            // Result polygon for spatial operations.
            Geometry resultPolygon = null;

            // Run the selected spatial operation on the polygon graphics and get the result geometry.
            switch (operationsModel.SelectedItem)
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

        private void ResetButton_TouchUpInside(object sender, EventArgs e)
        {
            // Remove any currently displayed result.
            _polygonsOverlay.Graphics.Remove(_resultGraphic);

            // Clear the selected spatial operation.
            _operationPicker.Select(0, 0, true);
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

    // Class that defines a UIPickerViewModel to display spatial operation choices.
    public class PickerDataModel : UIPickerViewModel
    {
        // Raise an event when the selected operation changes.
        public event EventHandler<EventArgs> ValueChanged;

        // Store the spatial operation choices in a list.
        public List<string> Items { get; private set; }

        // Expose the currently selected operation as a property.
        private int _selectedIndex = 0;
        public string SelectedItem
        {
            get { return Items[_selectedIndex]; }
        }

        // In the constructor, take the list of items (spatial operations) to display.
        public PickerDataModel(List<string> items)
        {
            Items = items;
        }

        // Number of rows to display in the picker (items in the list).
        public override nint GetRowsInComponent(UIPickerView picker, nint component)
        {
            return Items.Count;
        }

        // Text to display in the picker (spatial operation in each row).
        public override string GetTitle(UIPickerView picker, nint row, nint component)
        {
            return Items[(int)row];
        }

        // Number of columns to show in the picker.
        public override nint GetComponentCount(UIPickerView picker)
        {
            return 1;
        }

        // Raise the ValueChanged event when a new value is selected.
        public override void Selected(UIPickerView picker, nint row, nint component)
        {
            _selectedIndex = (int)row;
            if (ValueChanged != null)
            {
                ValueChanged(this, new EventArgs());
            }
        }
    }
}