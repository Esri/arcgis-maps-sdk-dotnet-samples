// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Drawing;
using ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.SpatialOperations
{
    [Register("SpatialOperations")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Perform spatial operations",
        category: "Geometry",
        description: "Find the union, intersection, or difference of two geometries.",
        instructions: "The sample provides an option to select a spatial operation. When an operation is selected, the resulting geometry is shown in red. The 'reset operation' button undoes the action and allow selecting a different operation.",
        tags: new[] { "analysis", "combine", "difference", "geometry", "intersection", "merge", "polygon", "union" })]
    public class SpatialOperations : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UISegmentedControl _operationChoiceButton;

        // GraphicsOverlay to hold the polygon graphics.
        private GraphicsOverlay _polygonsOverlay;

        // Polygon graphics to run spatial operations on.
        private Graphic _graphicOne;
        private Graphic _graphicTwo;

        // Graphic to display the spatial operation result polygon.
        private Graphic _resultGraphic;

        public SpatialOperations()
        {
            Title = "Spatial operations";
        }

        private void Initialize()
        {
            // Create and show a map with a gray canvas basemap and an initial location centered on London, UK.
            _myMapView.Map = new Map(BasemapStyle.ArcGISLightGray);
            _myMapView.Map.InitialViewpoint = new Viewpoint(51.5017, -0.12714, 20000);

            // Create and add two overlapping polygon graphics to operate on.
            CreatePolygonsOverlay();
        }

        private void _operationChoiceButton_ValueChanged(object sender, EventArgs e)
        {
            // Remove any currently displayed result.
            _polygonsOverlay.Graphics.Remove(_resultGraphic);

            // Polygon geometry from the input graphics.
            Geometry polygonOne = _graphicOne.Geometry;
            Geometry polygonTwo = _graphicTwo.Geometry;

            // Result polygon for spatial operations.
            Geometry resultPolygon = null;

            // Run the selected spatial operation on the polygon graphics and get the result geometry.
            switch (((UISegmentedControl) sender).SelectedSegment)
            {
                case 0:
                    resultPolygon = GeometryEngine.Difference(polygonOne, polygonTwo);
                    break;
                case 1:
                    resultPolygon = GeometryEngine.Intersection(polygonOne, polygonTwo);
                    break;
                case 2:
                    resultPolygon = GeometryEngine.SymmetricDifference(polygonOne, polygonTwo);
                    break;
                case 3:
                    resultPolygon = GeometryEngine.Union(polygonOne, polygonTwo);
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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _operationChoiceButton = new UISegmentedControl("Difference", "Intersection", "Symm. diff.", "Union")
            {
                BackgroundColor = ApplicationTheme.BackgroundColor,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Clean up borders of segmented control - avoid corner pixels.
            _operationChoiceButton.ClipsToBounds = true;
            _operationChoiceButton.Layer.CornerRadius = 5;

            // Add the views.
            View.AddSubviews(_myMapView, _operationChoiceButton);

            // Lay out views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),

                _operationChoiceButton.LeadingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.LeadingAnchor),
                _operationChoiceButton.TrailingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.TrailingAnchor),
                _operationChoiceButton.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _operationChoiceButton.ValueChanged += _operationChoiceButton_ValueChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _operationChoiceButton.ValueChanged -= _operationChoiceButton_ValueChanged;
        }
    }
}