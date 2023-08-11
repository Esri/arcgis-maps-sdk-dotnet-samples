// Copyright 2022 Esri.
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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ArcGIS.WPF.Samples.RenderMultilayerSymbols
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Render multilayer symbols",
        category: "Symbology",
        description: "Show different kinds of multilayer symbols on a map similar to some pre-defined 2D simple symbol styles.",
        instructions: "The sample loads multilayer symbols for points, polylines, and polygons.",
        tags: new[] { "graphic", "marker", "multilayer", "picture", "symbol" })]
    public partial class RenderMultilayerSymbols
    {
        // This is used to keep consistent distance from each symbol on the column.
        private double _offset = 20.0;

        public RenderMultilayerSymbols()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            MyMapView.Map = new Map(BasemapStyle.ArcGISLightGray);
            var overlay = new GraphicsOverlay();

            var textGraphicForMarkers = new Graphic(new MapPoint(-150, 50, SpatialReferences.Wgs84),
                new TextSymbol()
                {
                    Text = "MultilayerPoint\nsimple markers",
                    Color = Color.Black,
                    BackgroundColor = Color.White,
                    Size = 20,
                    OutlineColor = Color.Black,
                    OutlineWidth = 1,
                });
            overlay.Graphics.Add(textGraphicForMarkers);

            AddPointGraphicsWithMarkerSymbols(overlay);

            var textGraphicForPictureMarkers = new Graphic(new MapPoint(-80, 50, SpatialReferences.Wgs84),
                new TextSymbol()
                {
                    Text = "MultilayerPoint\npicture markers",
                    Color = Color.Black,
                    BackgroundColor = Color.White,
                    Size = 20,
                    OutlineColor = Color.Black,
                    OutlineWidth = 1
                });
            overlay.Graphics.Add(textGraphicForPictureMarkers);

            // Create picture marker symbol from a Uri.
            AddPointGraphicsWithPictureMarkerSymbolFromUri(overlay);

            // Create picture marker symbol from embedded resources.
            AddPointGraphicsWithPictureMarkerSymbolFromResources(overlay);

            var textGraphicForLineSymbols = new Graphic(new MapPoint(0, 50, SpatialReferences.Wgs84),
                new TextSymbol()
                {
                    Text = "Multilayer\nPolyline",
                    Color = Color.Black,
                    BackgroundColor = Color.White,
                    Size = 20,
                    OutlineColor = Color.Black,
                    OutlineWidth = 1
                });
            overlay.Graphics.Add(textGraphicForLineSymbols);

            // Create the line marker symbols.
            AddLineGraphicsWithMarkerSymbols(overlay);

            var textGraphicForFillSymbols = new Graphic(new MapPoint(65, 50, SpatialReferences.Wgs84),
                new TextSymbol()
                {
                    Text = "Multilayer\nPolygon",
                    Color = Color.Black,
                    BackgroundColor = Color.White,
                    Size = 20,
                    OutlineColor = Color.Black,
                    OutlineWidth = 1
                });
            overlay.Graphics.Add(textGraphicForFillSymbols);

            // Create the polygon marker symbol.
            AddPolygonGraphicsWithMarkerSymbols(overlay);

            var textGraphicForComplexSymbols = new Graphic(new MapPoint(130, 50, SpatialReferences.Wgs84),
               new TextSymbol()
               {
                   Text = "More Multilayer\nSymbols",
                   Color = Color.Black,
                   BackgroundColor = Color.White,
                   Size = 20,
                   OutlineColor = Color.Black,
                   OutlineWidth = 1
               });
            overlay.Graphics.Add(textGraphicForComplexSymbols);

            // Create the more complex multilayer points, polygons, and polylines.
            AddComplexPointGraphic(overlay);
            AddComplexPolygonGraphic(overlay);
            AddComplexPolylineGraphic(overlay);

            MyMapView.GraphicsOverlays.Add(overlay);
        }

        #region Create a multilayer point symbol.

        private void AddPointGraphicsWithMarkerSymbols(GraphicsOverlay overlay)
        {
            MultilayerPointSymbol markerSymbol;

            // Define a vector element, a diamond in this case.
            Geometry vectorElementGeometry = Geometry.FromJson("{\"rings\":[[[0.0,2.5],[2.5,0.0],[0.0,-2.5],[-2.5,0.0],[0.0,2.5]]]}");
            var vectorElementFill = new SolidFillSymbolLayer(Color.Red);
            var multiLayerPolygonSymbol = new MultilayerPolygonSymbol(new[] { vectorElementFill });
            var vectorMarkerDiamond = new VectorMarkerSymbolElement(vectorElementGeometry, multiLayerPolygonSymbol);
            var vectorMarkerSymbol = new VectorMarkerSymbolLayer(new[] { vectorMarkerDiamond });
            markerSymbol = new MultilayerPointSymbol(new[] { vectorMarkerSymbol });

            // Create point graphics using the diamond symbol created above.
            var diamondGraphic = new Graphic(new MapPoint(-150, 20, SpatialReferences.Wgs84), markerSymbol);
            overlay.Graphics.Add(diamondGraphic);

            // Define a vector element, a triangle in this case.
            vectorElementGeometry = Geometry.FromJson("{\"rings\":[[[0.0,5.0],[5,-5.0],[-5,-5.0],[0.0,5.0]]]}");
            vectorElementFill = new SolidFillSymbolLayer(Color.Red);
            multiLayerPolygonSymbol = new MultilayerPolygonSymbol(new[] { vectorElementFill });
            var vectorMarkerTriangle = new VectorMarkerSymbolElement(vectorElementGeometry, multiLayerPolygonSymbol);
            vectorMarkerSymbol = new VectorMarkerSymbolLayer(new[] { vectorMarkerTriangle });
            markerSymbol = new MultilayerPointSymbol(new[] { vectorMarkerSymbol });

            // Create point graphics using the triangle symbol created above.
            var triangleGraphic = new Graphic(new MapPoint(-150, 20 - _offset, SpatialReferences.Wgs84), markerSymbol);
            overlay.Graphics.Add(triangleGraphic);

            // Define a vector element, a cross in this case.
            vectorElementGeometry = Geometry.FromJson("{\"paths\":[[[-1,1],[0,0],[1,-1]],[[1,1],[0,0],[-1,-1]]]}");
            var vectorElementStroke = new SolidStrokeSymbolLayer(1, Color.Red);
            var multilayerPolylineSymbol = new MultilayerPolylineSymbol(new[] { vectorElementStroke });
            var vectorMarkerX = new VectorMarkerSymbolElement(vectorElementGeometry, multilayerPolylineSymbol);
            vectorMarkerSymbol = new VectorMarkerSymbolLayer(new[] { vectorMarkerX });
            markerSymbol = new MultilayerPointSymbol(new[] { vectorMarkerSymbol });

            // Create point graphics using the cross symbol created above.
            var crossGraphic = new Graphic(new MapPoint(-150, 20 - 2 * _offset, SpatialReferences.Wgs84), markerSymbol);
            overlay.Graphics.Add(crossGraphic);
        }

        #endregion Create a multilayer point symbol.

        #region Create a picture marker symbol from a Uri.

        private void AddPointGraphicsWithPictureMarkerSymbolFromUri(GraphicsOverlay overlay)
        {
            var symbolUri = new Uri("https://static.arcgis.com/images/Symbols/OutdoorRecreation/Camping.png");

            // Create a new symbol using asynchronous factory method from Uri.
            var campsiteMarker = new PictureMarkerSymbolLayer(symbolUri)
            {
                Size = 40
            };
            var campsiteSymbol = new MultilayerPointSymbol(new[] { campsiteMarker });

            var campsitePoint = new MapPoint(-80, 20, SpatialReferences.Wgs84);

            // Create graphic with the location and symbol.
            var campsiteGraphic = new Graphic(campsitePoint, campsiteSymbol);
            overlay.Graphics.Add(campsiteGraphic);
        }

        #endregion Create a picture marker symbol from a Uri.

        #region Create a picture marker symbol from an embedded resource.

        private void AddPointGraphicsWithPictureMarkerSymbolFromResources(GraphicsOverlay overlay)
        {
            // Get current assembly that contains the image.
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // Get the resource name of the blue pin image
            string resourceStreamName = this.GetType().Assembly.GetManifestResourceNames().Single(str => str.EndsWith("pin_star_blue.png"));

            // Get image as a stream from the resources.
            // Picture is defined as EmbeddedResource.
            using (var stream = currentAssembly.GetManifestResourceStream(resourceStreamName))
            {
                using (var mem = new MemoryStream())
                {
                    stream.CopyTo(mem);

                    // Create a runtime image from the bytes read from the memory stream.
                    var img = new RuntimeImage(mem.ToArray());

                    // Create new PictureMarkerSymbolLayer from the runtime image object.
                    var pinMarker = new PictureMarkerSymbolLayer(img) { Size = 50 };

                    // Create a new multilayerpoint symbol with the PictureMarkerSymbolLayer.
                    var pinSymbol = new MultilayerPointSymbol(new[] { pinMarker });

                    // Create location for the pin.
                    var pinPoint = new MapPoint(-80, 20 - _offset, SpatialReferences.Wgs84);

                    // Create graphic with the location and symbol.
                    var pinGraphic = new Graphic(pinPoint, pinSymbol);
                    overlay.Graphics.Add(pinGraphic);
                }
            }
        }

        #endregion Create a picture marker symbol from an embedded resource.

        #region Create a multilayer polyline symbol.

        private void AddLineGraphicsWithMarkerSymbols(GraphicsOverlay overlay)
        {
            // Multilayer polyline symbol for different line styles.
            MultilayerPolylineSymbol lineSymbol;

            // Stroke used by line symbols.
            SolidStrokeSymbolLayer strokeLayer;
            strokeLayer = new SolidStrokeSymbolLayer(3, Color.Red);
            strokeLayer.CapStyle = StrokeSymbolLayerCapStyle.Round;

            // Create a polyline for the multilayer polyline symbol.
            var polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);
            polylineBuilder.AddPoint(new MapPoint(-30, 20, SpatialReferences.Wgs84));
            polylineBuilder.AddPoint(new MapPoint(30, 20, SpatialReferences.Wgs84));

            // Dash geometric effects that define the dash and dot template used by different line style.
            var dashEffect = new DashGeometricEffect();

            // Create a dash effect similar to SimpleLineSymbolStyle.ShortDashDotDot.
            dashEffect.DashTemplate.Add(4);
            dashEffect.DashTemplate.Add(6);
            dashEffect.DashTemplate.Add(0.5);
            dashEffect.DashTemplate.Add(6);
            dashEffect.DashTemplate.Add(0.5);
            dashEffect.DashTemplate.Add(6);
            strokeLayer.GeometricEffects.Add(dashEffect);

            lineSymbol = new MultilayerPolylineSymbol(new List<SymbolLayer> { strokeLayer });

            // Create a polyline graphic with geometry using the symbol created above.
            var shortDashDotGraphic = new Graphic(polylineBuilder.ToGeometry(), lineSymbol);
            overlay.Graphics.Add(shortDashDotGraphic);

            polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);
            polylineBuilder.AddPoint(new MapPoint(-30, 20 - _offset, SpatialReferences.Wgs84));
            polylineBuilder.AddPoint(new MapPoint(30, 20 - _offset, SpatialReferences.Wgs84));

            strokeLayer = new SolidStrokeSymbolLayer(3, Color.Red);
            strokeLayer.CapStyle = StrokeSymbolLayerCapStyle.Round;
            dashEffect = new DashGeometricEffect();

            // Create a dash effect similar to SimpleLineSymbolStyle.ShortDash.
            dashEffect.DashTemplate.Add(4);
            dashEffect.DashTemplate.Add(6);
            strokeLayer.GeometricEffects.Add(dashEffect);

            lineSymbol = new MultilayerPolylineSymbol(new List<SymbolLayer> { strokeLayer });

            // Create a polyline graphic with geometry using the symbol created above.
            var shortDashGraphic = new Graphic(polylineBuilder.ToGeometry(), lineSymbol);
            overlay.Graphics.Add(shortDashGraphic);

            polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);
            polylineBuilder.AddPoint(new MapPoint(-30, 20 - 2 * _offset, SpatialReferences.Wgs84));
            polylineBuilder.AddPoint(new MapPoint(30, 20 - 2 * _offset, SpatialReferences.Wgs84));

            strokeLayer = new SolidStrokeSymbolLayer(3, Color.Red);
            strokeLayer.CapStyle = StrokeSymbolLayerCapStyle.Round;
            dashEffect = new DashGeometricEffect();

            // Create a dash effect similar to SimpleLineSymbolStyle.DashDotDot.
            dashEffect.DashTemplate.Add(7);
            dashEffect.DashTemplate.Add(9);
            dashEffect.DashTemplate.Add(0.5);
            dashEffect.DashTemplate.Add(9);
            strokeLayer.GeometricEffects.Add(dashEffect);

            lineSymbol = new MultilayerPolylineSymbol(new List<SymbolLayer> { strokeLayer });

            // Create a polyline graphic with geometry using the symbol created above.
            var dashDotGraphic = new Graphic(polylineBuilder.ToGeometry(), lineSymbol);
            overlay.Graphics.Add(dashDotGraphic);
        }

        #endregion Create a multilayer polyline symbol.

        #region Create a multilayer polygon symbol.

        private void AddPolygonGraphicsWithMarkerSymbols(GraphicsOverlay overlay)
        {
            var polygonBuilder = new PolygonBuilder(SpatialReferences.Wgs84);
            polygonBuilder.AddPoint(new MapPoint(60, 25, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(70, 25, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(70, 20, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(60, 20, SpatialReferences.Wgs84));

            // Create a stroke symbol layer to be used by hatch patterns.
            var strokeForHatches = new SolidStrokeSymbolLayer(2, Color.Red);
            var strokeForOutline = new SolidStrokeSymbolLayer(1, Color.Black);

            // Create a diagonal cross pattern hatch symbol layers for diagonal cross fill style.
            var diagonalStroke1 = new HatchFillSymbolLayer(new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeForHatches }), 45);
            var diagonalStroke2 = new HatchFillSymbolLayer(new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeForHatches }), -45);

            diagonalStroke1.Separation = 9;
            diagonalStroke2.Separation = 9;

            // Create a multilayer polygon symbol with symbol layers.
            var diagonalCrossPolygonSymbol = new MultilayerPolygonSymbol(new List<SymbolLayer>() { diagonalStroke1, diagonalStroke2, strokeForOutline });

            // Create a polygon graphic with geometry using the symbol created above.
            var diagonalCrossGraphic = new Graphic(polygonBuilder.ToGeometry(), diagonalCrossPolygonSymbol);
            overlay.Graphics.Add(diagonalCrossGraphic);

            polygonBuilder = new PolygonBuilder(SpatialReferences.Wgs84);
            polygonBuilder.AddPoint(new MapPoint(60, 25 - _offset, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(70, 25 - _offset, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(70, 20 - _offset, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(60, 20 - _offset, SpatialReferences.Wgs84));

            // Create a forward diagonal pattern hatch symbol layer for forward diagonal fill style.
            var forwardDiagonal = new HatchFillSymbolLayer(new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeForHatches }), -45);
            forwardDiagonal.Separation = 9;

            // Create a multilayer polygon symbol with symbol layers.
            var forwardDiagonalPolygonSymbol = new MultilayerPolygonSymbol(new List<SymbolLayer>() { forwardDiagonal, strokeForOutline });

            // Create a polygon graphic with geometry using the symbol created above.
            var forwardDiagonalGraphic = new Graphic(polygonBuilder.ToGeometry(), forwardDiagonalPolygonSymbol);
            overlay.Graphics.Add(forwardDiagonalGraphic);

            polygonBuilder = new PolygonBuilder(SpatialReferences.Wgs84);
            polygonBuilder.AddPoint(new MapPoint(60, 25 - 2 * _offset, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(70, 25 - 2 * _offset, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(70, 20 - 2 * _offset, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(60, 20 - 2 * _offset, SpatialReferences.Wgs84));

            // Create a vertical pattern hatch symbol layer for vertical fill style.
            var vertical = new HatchFillSymbolLayer(new MultilayerPolylineSymbol(new List<SymbolLayer> { strokeForHatches }), 90);
            vertical.Separation = 9;

            // Create a multilayer polygon symbol with symbol layers.
            var verticalPolygonSymbol = new MultilayerPolygonSymbol(new List<SymbolLayer> { vertical, strokeForOutline });

            // Create a polygon graphic with geometry using the symbol created above.
            var verticalPolygonGraphic = new Graphic(polygonBuilder.ToGeometry(), verticalPolygonSymbol);
            overlay.Graphics.Add(verticalPolygonGraphic);
        }

        #endregion Create a multilayer polygon symbol.

        #region Create complex multilayer point symbol.

        private void AddComplexPointGraphic(GraphicsOverlay overlay)
        {
            // Create an orange envelope with reddish outline.
            var orangeFillLayer = new SolidFillSymbolLayer(Color.Orange);
            var pinkOutline = new SolidStrokeSymbolLayer(2, Color.Blue);
            var orangeSquareGeometry = new Envelope(new MapPoint(-0.5, -0.5, SpatialReferences.Wgs84), new MapPoint(0.5, 0.5, SpatialReferences.Wgs84));
            var orangeSquareVectorElement = new VectorMarkerSymbolElement(orangeSquareGeometry, new MultilayerPolygonSymbol(new List<SymbolLayer>() { orangeFillLayer, pinkOutline }));
            var orangeSquareVectorMarkerLayer = new VectorMarkerSymbolLayer(new[] { orangeSquareVectorElement });
            orangeSquareVectorMarkerLayer.Size = 11;
            orangeSquareVectorMarkerLayer.Anchor = new SymbolAnchor(-4, -6, SymbolAnchorPlacementMode.Absolute);

            // Create a black envelope.
            var blackFillLayer = new SolidFillSymbolLayer(Color.Black);
            var orangeOutline = new SolidStrokeSymbolLayer(2, Color.OrangeRed);
            var blackSquareGeometry = new Envelope(new MapPoint(-0.5, -0.5, SpatialReferences.Wgs84), new MapPoint(0.5, 0.5, SpatialReferences.Wgs84));
            var blackSquareVectorElement = new VectorMarkerSymbolElement(blackSquareGeometry, new MultilayerPolygonSymbol(new List<SymbolLayer>() { blackFillLayer, orangeOutline }));
            var blackSquareVectorMarkerLayer = new VectorMarkerSymbolLayer(new[] { blackSquareVectorElement });
            blackSquareVectorMarkerLayer.Size = 6;
            blackSquareVectorMarkerLayer.Anchor = new SymbolAnchor(2, 1, SymbolAnchorPlacementMode.Absolute);

            // Create an envelope with no purple outline.
            var transparentFillLayer = new SolidFillSymbolLayer(Color.Transparent);
            var purpleOutline = new SolidStrokeSymbolLayer(2, Color.Purple);
            var purpleSquareGeometry = new Envelope(new MapPoint(-0.5, -0.5, SpatialReferences.Wgs84), new MapPoint(0.5, 0.5, SpatialReferences.Wgs84));
            var purpleSquareVectorElement = new VectorMarkerSymbolElement(purpleSquareGeometry, new MultilayerPolygonSymbol(new List<SymbolLayer>() { transparentFillLayer, purpleOutline }));
            var purpleSquareVectorMarkerLayer = new VectorMarkerSymbolLayer(new[] { purpleSquareVectorElement });
            purpleSquareVectorMarkerLayer.Size = 14;
            purpleSquareVectorMarkerLayer.Anchor = new SymbolAnchor(4, 2, SymbolAnchorPlacementMode.Absolute);

            // First layer with its marker graphics and nested symbol layers.
            Geometry hexagonElementGeometry = Geometry.FromJson("{\"rings\":[[[-2.89,5.0],[2.89,5.0],[5.77,0.0],[2.89,-5.0],[-2.89,-5.0],[-5.77,0.0],[-2.89,5.0]]]}");
            var yellowFillLayer = new SolidFillSymbolLayer(Color.Yellow);
            var blackOutline = new SolidStrokeSymbolLayer(2, Color.Black);
            var hexagonVectorElement = new VectorMarkerSymbolElement(hexagonElementGeometry, new MultilayerPolygonSymbol(new List<SymbolLayer>() { yellowFillLayer, blackOutline }));
            var hexagonVectorMarkerLayer = new VectorMarkerSymbolLayer(new[] { hexagonVectorElement });
            hexagonVectorMarkerLayer.Size = 35;

            // Create the multilayer point symbol.
            var pointSymbol = new MultilayerPointSymbol(new List<SymbolLayer> { hexagonVectorMarkerLayer, orangeSquareVectorMarkerLayer, blackSquareVectorMarkerLayer, purpleSquareVectorMarkerLayer });

            // Create the multilayer point graphic using the symbols created above.
            var complexPointGraphic = new Graphic(new MapPoint(130, 20, SpatialReferences.Wgs84), pointSymbol);
            overlay.Graphics.Add(complexPointGraphic);
        }

        #endregion Create complex multilayer point symbol.

        #region Create complex multilayer polyline symbol.

        private void AddComplexPolylineGraphic(GraphicsOverlay overlay)
        {
            // Symbol layers for multilayer polyline.
            var blackDashes = new SolidStrokeSymbolLayer(1, Color.Black);
            var dashEffect = new DashGeometricEffect(new double[] { 5, 3 });
            blackDashes.GeometricEffects.Add(dashEffect);
            blackDashes.CapStyle = StrokeSymbolLayerCapStyle.Square;

            // Create the yellow stroke inside.
            var yellowStroke = new SolidStrokeSymbolLayer(5, Color.Yellow);
            yellowStroke.CapStyle = StrokeSymbolLayerCapStyle.Round;

            // Create the black outline.
            var blackOutline = new SolidStrokeSymbolLayer(7, Color.Black);
            blackOutline.CapStyle = StrokeSymbolLayerCapStyle.Round;

            // Create the multilayer polyline symbol.
            var polylineSymbol = new MultilayerPolylineSymbol(new List<SymbolLayer> { blackOutline, yellowStroke, blackDashes });
            var polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);
            polylineBuilder.AddPoint(new MapPoint(120, -25, SpatialReferences.Wgs84));
            polylineBuilder.AddPoint(new MapPoint(140, -25, SpatialReferences.Wgs84));

            // Create the multilayer polyline graphic with geometry using the symbols created above.
            var complexLineGraphic = new Graphic(polylineBuilder.ToGeometry(), polylineSymbol);
            overlay.Graphics.Add(complexLineGraphic);
        }

        #endregion Create complex multilayer polyline symbol.

        #region Create complex multilayer polygon symbol.

        private void AddComplexPolygonGraphic(GraphicsOverlay overlay)
        {
            // Create the black outline.
            SolidStrokeSymbolLayer blackOutline = new SolidStrokeSymbolLayer(7, Color.Black);
            blackOutline.CapStyle = StrokeSymbolLayerCapStyle.Round;

            // Create the yellow stroke inside.
            SolidStrokeSymbolLayer yellowStroke = new SolidStrokeSymbolLayer(5, Color.Yellow);
            yellowStroke.CapStyle = StrokeSymbolLayerCapStyle.Round;

            // Symbol layers for multilayer polyline.
            SolidStrokeSymbolLayer blackDashes = new SolidStrokeSymbolLayer(1, Color.Black);
            GeometricEffect dashEffect = new DashGeometricEffect(new double[] { 5, 3 });
            blackDashes.GeometricEffects.Add(dashEffect);
            blackDashes.CapStyle = StrokeSymbolLayerCapStyle.Square;

            // Create a red filling for the polygon.
            SolidFillSymbolLayer redFillLayer = new SolidFillSymbolLayer(Color.Red);

            // Create the multilayer polygon symbol.
            MultilayerPolygonSymbol polygonSymbol = new MultilayerPolygonSymbol(new List<SymbolLayer> { redFillLayer, blackOutline, yellowStroke, blackDashes });

            // Create the polygon.
            PolygonBuilder polygonBuilder = new PolygonBuilder(SpatialReferences.Wgs84);
            polygonBuilder.AddPoint(new MapPoint(120, 0, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(140, 0, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(140, -10, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(120, -10, SpatialReferences.Wgs84));

            // Create a multilayer polygon graphic with geometry using the symbols created above.
            Graphic complexPolygonGraphic = new Graphic(polygonBuilder.ToGeometry(), polygonSymbol);
            overlay.Graphics.Add(complexPolygonGraphic);
        }

        #endregion Create complex multilayer polygon symbol.
    }
}