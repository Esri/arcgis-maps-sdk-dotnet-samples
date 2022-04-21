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
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.RenderMultilayerSymbols
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Render multilayer symbols",
        "Symbology",
        "Shows a multilayer symbol on a map.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
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
            // Create new Map with the light gray basemap.
            Map myMap = new Map(BasemapStyle.ArcGISLightGray);
            MyMapView.Map = myMap;

            // Create overlay to where graphics are shown.
            GraphicsOverlay overlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(overlay);

            // Graphic for labeling multilayer point symbols with marker symbols.
            Graphic textGraphic4Markers = new Graphic(new MapPoint(-150, 50, SpatialReferences.Wgs84),
                new TextSymbol()
                {
                    Text = "Multilayerpoint \n Simple markers",
                    Color = Color.Black,
                    BackgroundColor = Color.White,
                    Size = 20,
                    OutlineColor = Color.Black,
                    OutlineWidth = 1,
                });
            overlay.Graphics.Add(textGraphic4Markers);

            // Creates the multilayer point symbology.
            AddPointGraphicsWithMarkerSymbols(overlay);

            // Graphic for labeling picture marker symbols column.
            Graphic textGraphic4PictureMarkers = new Graphic(new MapPoint(-80, 50, SpatialReferences.Wgs84),
                new TextSymbol()
                {
                    Text = "Multilayerpoint \n Picture markers",
                    Color = Color.Black,
                    BackgroundColor = Color.White,
                    Size = 20,
                    OutlineColor = Color.Black,
                    OutlineWidth = 1
                });
            overlay.Graphics.Add(textGraphic4PictureMarkers);

            try
            {
                // Creates picture marker symbol from a URL.
                AddPointGraphicsWithPictureMarkerSymbolFromUrl(overlay);

                // Creates picture marker symbol from embedded resources.
                AddPointGraphicsWithPictureMarkerSymbolFromResources(overlay);

                // Creates picture marker symbol from bytes.
                AddPointGraphicsWithPictureMarkesFromBytes(overlay);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }

            // Graphic for labeling line marker symbols column.
            Graphic textGraphic4lineSymbols = new Graphic(new MapPoint(0, 50, SpatialReferences.Wgs84),
                new TextSymbol()
                {
                    Text = "Multilayer \n Polyline",
                    Color = Color.Black,
                    BackgroundColor = Color.White,
                    Size = 20,
                    OutlineColor = Color.Black,
                    OutlineWidth = 1
                });
            overlay.Graphics.Add(textGraphic4lineSymbols);

            // Creates the line marker symbols.
            AddLineGraphicsWithMarkerSymbols(overlay);

            // Graphic for labeling multilayer polygons symbols column.
            Graphic textGraphic4fillSymbols = new Graphic(new MapPoint(65, 50, SpatialReferences.Wgs84),
                new TextSymbol()
                {
                    Text = "Multilayer \n Polygon",
                    Color = Color.Black,
                    BackgroundColor = Color.White,
                    Size = 20,
                    OutlineColor = Color.Black,
                    OutlineWidth = 1
                });
            overlay.Graphics.Add(textGraphic4fillSymbols);

            // Creates the polygon marker symbol.
            AddPolygonGraphicsWithMarkerSymbols(overlay);

            // Graphic for labeling the more advance multilayer symbols you can make.
            Graphic textGraphic4advancedSymbols = new Graphic(new MapPoint(130, 50, SpatialReferences.Wgs84),
               new TextSymbol()
               {
                   Text = "More Multilayer \n Symbols",
                   Color = Color.Black,
                   BackgroundColor = Color.White,
                   Size = 20,
                   OutlineColor = Color.Black,
                   OutlineWidth = 1
               });
            overlay.Graphics.Add(textGraphic4advancedSymbols);

            // Creates the more advance multilayer points, polygons, and polylines.
            AdvancePoint(overlay);
            AdvancePolygon(overlay);
            AdvancePolyline(overlay);
        }

        #region Creates a multilayer point symbol.

        private void AddPointGraphicsWithMarkerSymbols(GraphicsOverlay overlay)
        {
            // Graphic for point geometry.
            Graphic pointGraphic;

            // Symbol for different marker styles.
            MultilayerPointSymbol markerSymbol;

            // Define a vector element, a diamond in this case.
            var vec_elem_geom = Geometry.FromJson("{\"rings\":[[[0.0,2.5],[2.5,0.0],[0.0,-2.5],[-2.5,0.0],[0.0,2.5]]]}");
            var vec_elem_fill = new SolidFillSymbolLayer(Color.Red);
            var mlfs = new MultilayerPolygonSymbol(new[] { vec_elem_fill });
            var vmse_diamond = new VectorMarkerSymbolElement(vec_elem_geom, mlfs);
            var vmsl = new VectorMarkerSymbolLayer(new[] { vmse_diamond });
            markerSymbol = new MultilayerPointSymbol(new[] { vmsl });

            // Create point graphics using diamond symbol created above.
            pointGraphic = new Graphic(new MapPoint(-150, 20, SpatialReferences.Wgs84), markerSymbol);
            overlay.Graphics.Add(pointGraphic);

            // Define a vector element, a triangle in this case.
            vec_elem_geom = Geometry.FromJson("{\"rings\":[[[0.0,5.0],[5,-5.0],[-5,-5.0],[0.0,5.0]]]}");
            vec_elem_fill = new SolidFillSymbolLayer(Color.Red);
            mlfs = new MultilayerPolygonSymbol(new[] { vec_elem_fill });
            var vmse_triangle = new VectorMarkerSymbolElement(vec_elem_geom, mlfs);
            vmsl = new VectorMarkerSymbolLayer(new[] { vmse_triangle });
            markerSymbol = new MultilayerPointSymbol(new[] { vmsl });

            // Create point graphics using triangle symbol created above.
            pointGraphic = new Graphic(new MapPoint(-150, 20 - _offset, SpatialReferences.Wgs84), markerSymbol);
            overlay.Graphics.Add(pointGraphic);

            // Define a vector element, a cross in this case.
            vec_elem_geom = Geometry.FromJson("{\"paths\":[[[-1,1],[0,0],[1,-1]],[[1,1],[0,0],[-1,-1]]]}");
            var vec_elem_stroke = new SolidStrokeSymbolLayer(1, Color.Red);
            var mlpls = new MultilayerPolylineSymbol(new[] { vec_elem_stroke });
            var vmse_x = new VectorMarkerSymbolElement(vec_elem_geom, mlpls);
            vmsl = new VectorMarkerSymbolLayer(new[] { vmse_x });
            markerSymbol = new MultilayerPointSymbol(new[] { vmsl });

            // Create point graphics using cross symbol created above.
            pointGraphic = new Graphic(new MapPoint(-150, 20 - 2 * _offset, SpatialReferences.Wgs84), markerSymbol);
            overlay.Graphics.Add(pointGraphic);
        }

        #endregion Creates a multilayer point symbol.

        #region Creates a picture marker symbol from a URL.

        private void AddPointGraphicsWithPictureMarkerSymbolFromUrl(GraphicsOverlay overlay)
        {
            // Create Uri to the used image.
            Uri symbolUri = new Uri(
                "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Recreation/FeatureServer/0/images/e82f744ebb069bb35b234b3fea46deae");

            // Create a new symbol using asynchronous factory method from Uri.
            PictureMarkerSymbolLayer campsiteMarker = new PictureMarkerSymbolLayer(symbolUri)
            {
                Size = 40
            };
            MultilayerPointSymbol campsiteSymbol = new MultilayerPointSymbol(new[] { campsiteMarker });

            // Create location for the campsite.
            MapPoint campsitePoint = new MapPoint(-80, 20, SpatialReferences.Wgs84);

            // Create graphic with the location and symbol.
            Graphic campsiteGraphic = new Graphic(campsitePoint, campsiteSymbol);
            overlay.Graphics.Add(campsiteGraphic);
        }

        #endregion Creates a picture marker symbol from a URL.

        #region Create a picture marker symbol from an embedded resource.

        private void AddPointGraphicsWithPictureMarkerSymbolFromResources(GraphicsOverlay overlay)
        {
            // Get current assembly that contains the image.
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // Get image as a stream from the resources.
            // Picture is defined as EmbeddedResource.
            using (var stream = currentAssembly.GetManifestResourceStream("ArcGISRuntime.Resources.PictureMarkerSymbols.pin_star_blue.png"))
            {
                using (var mem = new MemoryStream())
                {
                    stream.CopyTo(mem);

                    // Create a runtime image from the bytes read from the memory stream.
                    RuntimeImage img = new RuntimeImage(mem.ToArray());

                    // Create new PictureMarkerSymbolLayer from the runtime image object.
                    PictureMarkerSymbolLayer pinMarker = new PictureMarkerSymbolLayer(img) { Size = 50 };

                    // Create a new multilayerpoint symbol with the PictureMarkerSymbolLayer.
                    MultilayerPointSymbol pinSymbol = new MultilayerPointSymbol(new[] { pinMarker });

                    // Create location for the pin.
                    MapPoint pinPoint = new MapPoint(-80, 20 - _offset, SpatialReferences.Wgs84);

                    // Create graphic with the location and symbol.
                    Graphic pinGraphic = new Graphic(pinPoint, pinSymbol);
                    overlay.Graphics.Add(pinGraphic);
                }
            }
        }

        #endregion Create a picture marker symbol from an embedded resource.

        #region Creates a picture marker symbol from bytes.

        private void AddPointGraphicsWithPictureMarkesFromBytes(GraphicsOverlay overlay)
        {
            // Get bytes from FromBase64String.
            var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAABGdBTUEAAYagMeiWXwAAAfJJREFUeJztm79LAnEYhz/n1OAQNBTkIOjg0GBw7QYNLVFgUEOQQW0Ojv4J/glBB11bgVBBQ9CgzgU1NDg4uBzUELQIbfV+Tw5OSbj3hgPvfR94Dn8gL99H7/SEg3QyMx4/Ji/JV/J3zn0mL8iDKAGWySfSJWtkGfOPTZ6S1+QtuRR+MhxgleyTW0gve+Q7QhHCAVxyEelnhTwP7gQBzD6f5nd+mn1y19wIAmxDHv6agwBpONhx8dccBChBHhMBJLJgNpID+GiAJIbYto1KpRLZYrGIpEgkgOM46HQ6ka3X60gK3QUgHA0A4WgACEcDQDgaAMLRANwX5PN51omNMZvNsmbkcjn2jFIp3n867AC1Wo11YmMsFAqsGdVqlT2j2WwiDroLQDgaAMLRABCOBoBwNACEowG4LxgOh+j1eixHoxFrhud57Bn9fh9xYAdwXZd9pjYYDFgz2u02e0ar1UIcdBeAcDQAhKMBIBwNAOFoAAhHAyQxpFwuw7KsyDYaDSSFfgIgHA0AufyYTRAg3t8p882b2WTCd4QxEeAR8vDXHAS4IruQwx15b26ED4JH5DfSzwfGV5L6hAN4GF881UV6eSDXyK/ggemvwU9yE+Prhl2k4+D4QjrkIbmD0OINs34HmGPCCblOWnPuBnlG3vy30D/UTySOabh2IwAAAABJRU5ErkJggg==");

            // Create a runtime image from the bytes.
            RuntimeImage img = new RuntimeImage(bytes);

            // Create new PictureMarkerSymbolLayer from the runtime image object.
            var symLayer = new PictureMarkerSymbolLayer(img) { Size = 30 };

            // Create a new multilayerpoint symbol with the PictureMarkerSymbolLayer.
            MultilayerPointSymbol pictureSymbol = new MultilayerPointSymbol(new[] { symLayer });

            // Create graphic with the location and symbol.
            var pictureFromBytesGraphic = new Graphic(new MapPoint(-80, 20 - 2 * _offset, SpatialReferences.Wgs84), pictureSymbol);

            // Add graphic to the graphics overlay.
            overlay.Graphics.Add(pictureFromBytesGraphic);
        }

        #endregion Creates a picture marker symbol from bytes.

        #region Creates a multilayer polyline symbol.

        private void AddLineGraphicsWithMarkerSymbols(GraphicsOverlay overlay)
        {
            // Graphic for line geometries.
            Graphic lineGraphic;

            // Multilayer polyline symbol for different line styles.
            MultilayerPolylineSymbol lineSymbol;

            // Stroke used by line symbols.
            SolidStrokeSymbolLayer strokeLayer;

            // Create a new stroke for each style.
            strokeLayer = new SolidStrokeSymbolLayer(3, Color.Red);

            // Set capstyle of stroke to rounded.
            strokeLayer.CapStyle = StrokeSymbolLayerCapStyle.Round;

            // Creates a polyine for the multilayer polyline symbol.
            PolylineBuilder polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);
            polylineBuilder.AddPoint(new MapPoint(-30, 20, SpatialReferences.Wgs84));
            polylineBuilder.AddPoint(new MapPoint(30, 20, SpatialReferences.Wgs84));

            // Dash geometric effects that define the dash and dot template used by different line style.
            DashGeometricEffect dashEffect = new DashGeometricEffect();

            // Creates a dash effect similar to SimpleLineSymbolStyle.ShortDashDotDot.
            dashEffect.DashTemplate.Add(4);
            dashEffect.DashTemplate.Add(6);
            dashEffect.DashTemplate.Add(0.5);
            dashEffect.DashTemplate.Add(6);
            dashEffect.DashTemplate.Add(0.5);
            dashEffect.DashTemplate.Add(6);
            strokeLayer.GeometricEffects.Add(dashEffect);

            lineSymbol = new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeLayer });

            // Create a polyline graphic with geometry using symbol created above.
            lineGraphic = new Graphic(polylineBuilder.ToGeometry(), lineSymbol);
            overlay.Graphics.Add(lineGraphic);

            // Creates a new polyline for a different multilayer symbol.
            polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);
            polylineBuilder.AddPoint(new MapPoint(-30, 20 - _offset, SpatialReferences.Wgs84));
            polylineBuilder.AddPoint(new MapPoint(30, 20 - _offset, SpatialReferences.Wgs84));

            // Create a new stroke for the new style.
            strokeLayer = new SolidStrokeSymbolLayer(3, Color.Red);

            // Set capstyle of stroke to rounded.
            strokeLayer.CapStyle = StrokeSymbolLayerCapStyle.Round;

            // Create a new dash effect for the new style.
            dashEffect = new DashGeometricEffect();

            // Creates a dash effect similar to SimpleLineSymbolStyle.ShortDash.
            dashEffect.DashTemplate.Add(4);
            dashEffect.DashTemplate.Add(6);
            strokeLayer.GeometricEffects.Add(dashEffect);

            lineSymbol = new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeLayer });

            // Create a polyline graphic with geometry using symbol created above.
            lineGraphic = new Graphic(polylineBuilder.ToGeometry(), lineSymbol);
            overlay.Graphics.Add(lineGraphic);

            // Creates a new polyline for a different multilayer symbol.
            polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);
            polylineBuilder.AddPoint(new MapPoint(-30, 20 - 2 * _offset, SpatialReferences.Wgs84));
            polylineBuilder.AddPoint(new MapPoint(30, 20 - 2 * _offset, SpatialReferences.Wgs84));

            // Create a new stroke for the new style.
            strokeLayer = new SolidStrokeSymbolLayer(3, Color.Red);

            // Set capstyle of stroke to rounded.
            strokeLayer.CapStyle = StrokeSymbolLayerCapStyle.Round;

            // Create a dash effect for the new style.
            dashEffect = new DashGeometricEffect();

            // Creates a dash effect similar to SimpleLineSymbolStyle.DashDotDot.
            dashEffect.DashTemplate.Add(7);
            dashEffect.DashTemplate.Add(9);
            dashEffect.DashTemplate.Add(0.5);
            dashEffect.DashTemplate.Add(9);
            strokeLayer.GeometricEffects.Add(dashEffect);

            lineSymbol = new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeLayer });

            // Create a polyline graphic with geometry using symbol created above.
            lineGraphic = new Graphic(polylineBuilder.ToGeometry(), lineSymbol);
            overlay.Graphics.Add(lineGraphic);
        }

        #endregion Creates a multilayer polyline symbol.

        private void AddPolygonGraphicsWithMarkerSymbols(GraphicsOverlay overlay)
        {
            // Creates a polygon.
            PolygonBuilder polygonBuilder = new PolygonBuilder(SpatialReferences.Wgs84);
            polygonBuilder.AddPoint(new MapPoint(60, 25, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(70, 25, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(70, 20, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(60, 20, SpatialReferences.Wgs84));

            // Creates a stroke symol layer to be used by hatch patterns.
            SolidStrokeSymbolLayer strokeForHatches = new SolidStrokeSymbolLayer(2, Color.Red);

            // Creates a stroke symbol layer for outline of polygons.
            SolidStrokeSymbolLayer strokeForOutline = new SolidStrokeSymbolLayer(1, Color.Black);

            // Createsaa diagonal cross pattern hatch symbol layers for diagonal cross fill style.
            HatchFillSymbolLayer diagonalStroke1 = new HatchFillSymbolLayer(new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeForHatches }), 45);
            HatchFillSymbolLayer diagonalStroke2 = new HatchFillSymbolLayer(new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeForHatches }), -45);

            // Define separation distance for lines in a hatch pattern.
            diagonalStroke1.Separation = 9;
            diagonalStroke2.Separation = 9;

            // Creates a multilayer polygon symbol with symbol layers.
            MultilayerPolygonSymbol diagonalCrossPolygonSymbol = new MultilayerPolygonSymbol(new List<SymbolLayer>() { diagonalStroke1, diagonalStroke2, strokeForOutline });

            // Creates a polygon graphic with geometry using symbol created above.
            var diagonalCrossGraphic = new Graphic(polygonBuilder.ToGeometry(), diagonalCrossPolygonSymbol);
            overlay.Graphics.Add(diagonalCrossGraphic);

            // Creates a polygon.
            polygonBuilder = new PolygonBuilder(SpatialReferences.Wgs84);
            polygonBuilder.AddPoint(new MapPoint(60, 25 - _offset, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(70, 25 - _offset, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(70, 20 - _offset, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(60, 20 - _offset, SpatialReferences.Wgs84));

            // Creates a forward diagonal pattern hatch symbol layer for forward diagonal fill style.
            HatchFillSymbolLayer forwardDiagonal = new HatchFillSymbolLayer(new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeForHatches }), -45);

            // Define separation distance for lines in a hatch pattern.
            forwardDiagonal.Separation = 9;

            // Creates a multilayer polygon symbol with symbol layers.
            MultilayerPolygonSymbol forwardDiagonalPolygonSymbol = new MultilayerPolygonSymbol(new List<SymbolLayer>() { forwardDiagonal, strokeForOutline });

            // Create a polygon graphic with geometry using symbol created above.
            var forwardDiagonalGraphic = new Graphic(polygonBuilder.ToGeometry(), forwardDiagonalPolygonSymbol);
            overlay.Graphics.Add(forwardDiagonalGraphic);

            // Creates a polygon.
            polygonBuilder = new PolygonBuilder(SpatialReferences.Wgs84);
            polygonBuilder.AddPoint(new MapPoint(60, 25 - 2 * _offset, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(70, 25 - 2 * _offset, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(70, 20 - 2 * _offset, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(60, 20 - 2 * _offset, SpatialReferences.Wgs84));

            // Creates a vertical pattern hatch symbol layer for vertical fill style.
            HatchFillSymbolLayer vertical = new HatchFillSymbolLayer(new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeForHatches }), 90);

            // Define separation distance for lines in a hatch pattern.
            vertical.Separation = 9;

            // Creates a multilayer polygon symbol with symbol layers.
            MultilayerPolygonSymbol verticalPolygonSymbol = new MultilayerPolygonSymbol(new List<SymbolLayer>() { vertical, strokeForOutline });

            // Creates a polygon graphic with geometry using symbol created above.
            var verticalPolygonGraphic = new Graphic(polygonBuilder.ToGeometry(), verticalPolygonSymbol);
            overlay.Graphics.Add(verticalPolygonGraphic);
        }

        #region Creates advance multilayer point symbol.

        private void AdvancePoint(GraphicsOverlay overlay)
        {
            // Create an orange envelope with redish outline.
            SolidFillSymbolLayer orangeFillLayer = new SolidFillSymbolLayer(Color.Orange);
            SolidStrokeSymbolLayer pinkOutline = new SolidStrokeSymbolLayer(2, Color.Blue);
            Envelope orangeSquareGeometry = new Envelope(new MapPoint(-0.5, -0.5, SpatialReferences.Wgs84), new MapPoint(0.5, 0.5, SpatialReferences.Wgs84));
            VectorMarkerSymbolElement orangeSquareVectorElement = new VectorMarkerSymbolElement(orangeSquareGeometry, new MultilayerPolygonSymbol(new List<SymbolLayer>() { orangeFillLayer, pinkOutline }));
            VectorMarkerSymbolLayer orangeSquareVectorMarkerLayer = new VectorMarkerSymbolLayer(new[] { orangeSquareVectorElement });
            orangeSquareVectorMarkerLayer.Size = 11;
            orangeSquareVectorMarkerLayer.Anchor = new SymbolAnchor(-4, -6, SymbolAnchorPlacementMode.Absolute);

            // Creates a black envelope.
            SolidFillSymbolLayer blackFillLayer = new SolidFillSymbolLayer(Color.Black);
            SolidStrokeSymbolLayer orangeOutline = new SolidStrokeSymbolLayer(2, Color.OrangeRed);
            Envelope blackSquareGeometry = new Envelope(new MapPoint(-0.5, -0.5, SpatialReferences.Wgs84), new MapPoint(0.5, 0.5, SpatialReferences.Wgs84));
            VectorMarkerSymbolElement blackSquareVectorElement = new VectorMarkerSymbolElement(blackSquareGeometry, new MultilayerPolygonSymbol(new List<SymbolLayer>() { blackFillLayer, orangeOutline }));
            VectorMarkerSymbolLayer blackSquareVectorMarkerLayer = new VectorMarkerSymbolLayer(new[] { blackSquareVectorElement });
            blackSquareVectorMarkerLayer.Size = 6;
            blackSquareVectorMarkerLayer.Anchor = new SymbolAnchor(2, 1, SymbolAnchorPlacementMode.Absolute);

            // Creates an envelope with no purple outline.
            SolidFillSymbolLayer transparentFillLayer = new SolidFillSymbolLayer(Color.Transparent);
            SolidStrokeSymbolLayer purpleOutline = new SolidStrokeSymbolLayer(2, Color.Purple);
            Envelope purpleSquareGeometry = new Envelope(new MapPoint(-0.5, -0.5, SpatialReferences.Wgs84), new MapPoint(0.5, 0.5, SpatialReferences.Wgs84));
            VectorMarkerSymbolElement purpleSquareVectorElement = new VectorMarkerSymbolElement(purpleSquareGeometry, new MultilayerPolygonSymbol(new List<SymbolLayer>() { transparentFillLayer, purpleOutline }));
            VectorMarkerSymbolLayer purpleSquareVectorMarkerLayer = new VectorMarkerSymbolLayer(new[] { purpleSquareVectorElement });
            purpleSquareVectorMarkerLayer.Size = 14;
            purpleSquareVectorMarkerLayer.Anchor = new SymbolAnchor(4, 2, SymbolAnchorPlacementMode.Absolute);

            // First layer with it's marker graphics and nested symbol layers.
            var hexagonElementGeometry = Geometry.FromJson("{\"rings\":[[[-2.89,5.0],[2.89,5.0],[5.77,0.0],[2.89,-5.0],[-2.89,-5.0],[-5.77,0.0],[-2.89,5.0]]]}");
            SolidFillSymbolLayer yellowFillLayer = new SolidFillSymbolLayer(Color.Yellow);
            SolidStrokeSymbolLayer blackOutline = new SolidStrokeSymbolLayer(2, Color.Black);
            VectorMarkerSymbolElement hexagonVectorElement = new VectorMarkerSymbolElement(hexagonElementGeometry, new MultilayerPolygonSymbol(new List<SymbolLayer>() { yellowFillLayer, blackOutline }));
            VectorMarkerSymbolLayer hexagonVectorMarkerLayer = new VectorMarkerSymbolLayer(new[] { hexagonVectorElement });
            hexagonVectorMarkerLayer.Size = 35;

            // Creates the multilayer point symbol.
            MultilayerPointSymbol pointSymbol = new MultilayerPointSymbol(new List<SymbolLayer> { hexagonVectorMarkerLayer, orangeSquareVectorMarkerLayer, blackSquareVectorMarkerLayer, purpleSquareVectorMarkerLayer });

            // Creates the multilayer point graphic using the symbols created above.
            Graphic advancedPointGraphic = new Graphic(new MapPoint(130, 20, SpatialReferences.Wgs84), pointSymbol);
            overlay.Graphics.Add(advancedPointGraphic);
        }

        #endregion Creates advance multilayer point symbol.

        #region Creates advance multilayer polyline symbol.

        private void AdvancePolyline(GraphicsOverlay overlay)
        {
            // Symbol layers for multilayer polyline.
            SolidStrokeSymbolLayer blackDashes = new SolidStrokeSymbolLayer(1, Color.Black);
            GeometricEffect dashEffect = new DashGeometricEffect(new double[] { 5, 3 });
            blackDashes.GeometricEffects.Add(dashEffect);
            blackDashes.CapStyle = StrokeSymbolLayerCapStyle.Square;

            // Creates the yellow stroke inside.
            SolidStrokeSymbolLayer yellowStroke = new SolidStrokeSymbolLayer(5, Color.Yellow);
            yellowStroke.CapStyle = StrokeSymbolLayerCapStyle.Round;

            // Creates the black outline.
            SolidStrokeSymbolLayer blackOutline = new SolidStrokeSymbolLayer(7, Color.Black);
            blackOutline.CapStyle = StrokeSymbolLayerCapStyle.Round;

            // Creates the multilayer polyline symbol.
            MultilayerPolylineSymbol polylineSymbol = new MultilayerPolylineSymbol(new List<SymbolLayer>() { blackOutline, yellowStroke, blackDashes });
            PolylineBuilder polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);
            polylineBuilder.AddPoint(new MapPoint(120, -25, SpatialReferences.Wgs84));
            polylineBuilder.AddPoint(new MapPoint(140, -25, SpatialReferences.Wgs84));

            // Create the multilayer polyline graphic with geometry using the symbols created above.
            Graphic advancedLineGraphic = new Graphic(polylineBuilder.ToGeometry(), polylineSymbol);
            overlay.Graphics.Add(advancedLineGraphic);
        }

        #endregion Creates advance multilayer polyline symbol.

        #region Creates advance multilayer polygon symbol.

        private void AdvancePolygon(GraphicsOverlay overlay)
        {
            // Creates the black outline.
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

            // Creates a red filling for the polygon.
            SolidFillSymbolLayer redFillLayer = new SolidFillSymbolLayer(Color.Red);

            // Creates the multilayer polygon symbol.
            MultilayerPolygonSymbol polygonSymbol = new MultilayerPolygonSymbol(new List<SymbolLayer>() { redFillLayer, blackOutline, yellowStroke, blackDashes });

            // Creates the polygon.
            PolygonBuilder polygonBuilder = new PolygonBuilder(SpatialReferences.Wgs84);
            polygonBuilder.AddPoint(new MapPoint(120, 0, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(140, 0, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(140, -10, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(120, -10, SpatialReferences.Wgs84));

            // Create a multilayer polygon graphic with geometry using the symbols created above.
            Graphic advancedPolygonGraphic = new Graphic(polygonBuilder.ToGeometry(), polygonSymbol);
            overlay.Graphics.Add(advancedPolygonGraphic);
        }

        #endregion Creates advance multilayer polygon symbol.
    }
}