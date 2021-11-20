// Copyright 2016 Esri.
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
        name: "Symbols - 2D Multilayer symbols",
        category: "Symbology",
        description: "Shows multilayer symbols on a map.",
        instructions: "The sample creates equivalents of pre-defined 2d symbols as multilayer symbols and uses them for graphics added to the map",
        tags: new[] { "graphics", "multilayerpoint", "multilayerpolygon", "multilayerpolyline", "symbol", "vectormarkersymbollayer", "picturemarkersymbollayer", "solidstrokesymbollayer","solidfillsymbollayer", "visualization" })]
    [ArcGISRuntime.Samples.Shared.Attributes.EmbeddedResource(@"PictureMarkerSymbols\pin_star_blue.png")]

    public partial class RenderMultilayerSymbols
    {
        double x = -150.0;
        double y = 50.0;
        public RenderMultilayerSymbols()
        {
            InitializeComponent();
            Initialize();
        }
        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateLightGrayCanvasVector());

            // Provide used Map to the MapView
            MyMapView.Map = myMap;

            // Create overlay to where graphics are shown
            GraphicsOverlay overlay = new GraphicsOverlay();

            // Add created overlay to the MapView
            MyMapView.GraphicsOverlays.Add(overlay);

            #region "Add Simple Marker Symbols"
            //Graphic for labeling multilayer point symbols with marker symbols
            Graphic textGraphic4Markers = new Graphic(new MapPoint(x, y, SpatialReferences.Wgs84),
                new TextSymbol()
                {
                    Text = "Multilayerpoint \n Simple markers",
                    Color = System.Drawing.Color.Red,
                    BackgroundColor = System.Drawing.Color.Yellow,
                    Size = 25,
                    OutlineColor = Color.Green,
                    OutlineWidth = 1
                });
            overlay.Graphics.Add(textGraphic4Markers);
            AddPointGraphicsWithMarkerSymbols(overlay);

            #endregion "Add Simple Marker Symbols"

            #region "Add Picture Marker Symbols"
            //Graphic for labeling picture marker symbols column
            x = -80.0;
            y = 50.0;
            Graphic textGraphic4PictureMarkers = new Graphic(new MapPoint(x, y, SpatialReferences.Wgs84),
                new TextSymbol()
                {
                    Text = "Multilayerpoint \n Picture markers",
                    Color = System.Drawing.Color.Red,
                    BackgroundColor = System.Drawing.Color.Yellow,
                    Size = 25,
                    FontStyle = Esri.ArcGISRuntime.Symbology.FontStyle.Italic,
                    FontWeight = Esri.ArcGISRuntime.Symbology.FontWeight.Bold

                });
            overlay.Graphics.Add(textGraphic4PictureMarkers);

            // Add picture graphics using different source types

            // #1: Using url
            AddPointGraphicsWithPictureMarkerSymbolFromUrl(overlay);

            try
            {
                //#2: Using embedded resorce
                AddPointGraphicsWithPictureMarkerSymbolFromResources(overlay);

                //#3: Using byte data
                AddPointGraphicsWithPictureMarkesFromBytes(overlay);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }

            #endregion "Add Picture Marker Symbols"


            #region "Add Line Symbols"
            //Graphic for labeling line marker symbols column
            x = 20.0;
            y = 50.0;
            Graphic textGraphic4lineSymbols = new Graphic(new MapPoint(x, y, SpatialReferences.Wgs84),
                new TextSymbol()
                {
                    Text = "MultilayerPolyline",
                    Color = System.Drawing.Color.Red,
                    HorizontalAlignment = Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Right,
                    BackgroundColor = System.Drawing.Color.Yellow,
                    Size = 25,
                    FontDecoration = FontDecoration.Underline
                });
            overlay.Graphics.Add(textGraphic4lineSymbols);

            AddLineGraphicsWithMarkerSymbols(overlay);
            #endregion "Add Line Symbols"

            #region "Add Polygon Symbols"
            //Graphic for labeling line marker symbols column
            x = 40.0;
            y = 50.0;
            Graphic textGraphic4fillSymbols = new Graphic(new MapPoint(x, y, SpatialReferences.Wgs84),
                new TextSymbol()
                {
                    Text = "MultilayerPolygon",
                    HorizontalAlignment = Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Left,
                    Color = System.Drawing.Color.Red,
                    BackgroundColor = System.Drawing.Color.Yellow,
                    Size = 25,
                    HaloColor = Color.Green,
                    HaloWidth = 1.5
                });
            overlay.Graphics.Add(textGraphic4fillSymbols);

            AddPolygonGraphicsWithMarkerSymbols(overlay);
            #endregion "Add Polygon Symbols"

            #region "Add more complex multilayer symbols"
            //Graphic for labeling line marker symbols column
            x = 100.0;
            y = 50.0;
            Graphic textGraphic4advancedSymbols = new Graphic(new MapPoint(x, y, SpatialReferences.Wgs84),
               new TextSymbol()
               {
                   Text = "More Multilayer Symbols",
                   HorizontalAlignment = Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Left,
                   Color = System.Drawing.Color.Yellow,
                   BackgroundColor = System.Drawing.Color.Black,
                   Size = 25,
                   HaloColor = Color.Green,
                   HaloWidth = 1.5
               });
            overlay.Graphics.Add(textGraphic4advancedSymbols);
            AddSomeAdvancedSymbolsForPointLinePolygon(overlay);
            #endregion "Add more complex multilayer symbols"
        }

        private void AddPointGraphicsWithMarkerSymbols(GraphicsOverlay overlay)
        {
            //graphic for point geometries
            Graphic pointGraphic;

            //symbol for different marker styles
            MultilayerPointSymbol markerSymbol;

            // Loop through each simple symbol marker style and create an equivalent multilayer symbol and assign to a graphic.
            foreach (SimpleMarkerSymbolStyle markerstyle in Enum.GetValues(typeof(SimpleMarkerSymbolStyle)))
            {
                // decrement y so all markers appear in one column
                y -= 10.0;

                // Create a multilayer point symbol for each symbol style
                switch (markerstyle)
                {
                    case SimpleMarkerSymbolStyle.Circle:
                        var vec_elem_geom = Geometry.FromJson("{\"curveRings\" : [[[0.0,5.0],[0.0,5.0],{\"c\":[[0.0,5.0],[0.0,-5.0]]}]] }");
                        var vec_elem_fill = new SolidFillSymbolLayer(Color.Red);
                        var mlps = new MultilayerPointSymbol(new[] { vec_elem_fill });
                        var vmse_circle = new VectorMarkerSymbolElement(vec_elem_geom, mlps);

                        var vmsl = new VectorMarkerSymbolLayer(new[] { vmse_circle });
                        markerSymbol = new MultilayerPointSymbol(new[] { vmsl });
                        // Create point graphics at x, y using symbol created above.
                        pointGraphic = new Graphic(new MapPoint(x, y, SpatialReferences.Wgs84), markerSymbol);
                        // Add new graphic to overlay
                        overlay.Graphics.Add(pointGraphic);
                        break;
                    case SimpleMarkerSymbolStyle.Square:
                        //define fist vector element , a square in this case.
                        vec_elem_geom = Geometry.FromJson("{\"rings\":[[[0.0,5.0],[5.0,5.0],[5.0,0.0],[0.0,0.0],[0.0,5.0]]]}");
                        vec_elem_fill = new SolidFillSymbolLayer(Color.Red);
                        var mlfs = new MultilayerPolygonSymbol(new[] { vec_elem_fill });
                        var vmse_square = new VectorMarkerSymbolElement(vec_elem_geom, mlfs);
                        vmsl = new VectorMarkerSymbolLayer(new[] { vmse_square });
                        markerSymbol = new MultilayerPointSymbol(new[] { vmsl });
                        // Create point graphics at x, y using symbol created above.
                        pointGraphic = new Graphic(new MapPoint(x, y, SpatialReferences.Wgs84), markerSymbol);
                        // Add new graphic to overlay
                        overlay.Graphics.Add(pointGraphic);
                        break;
                    case SimpleMarkerSymbolStyle.Diamond:
                        //define fist vector element , a square in this case.
                        vec_elem_geom = Geometry.FromJson("{\"rings\":[[[0.0,2.5],[2.5,0.0],[0.0,-2.5],[-2.5,0.0],[0.0,2.5]]]}");
                        vec_elem_fill = new SolidFillSymbolLayer(Color.Red);
                        mlfs = new MultilayerPolygonSymbol(new[] { vec_elem_fill });
                        var vmse_diamond = new VectorMarkerSymbolElement(vec_elem_geom, mlfs);
                        vmsl = new VectorMarkerSymbolLayer(new[] { vmse_diamond });
                        markerSymbol = new MultilayerPointSymbol(new[] { vmsl });
                        // Create point graphics at x, y using symbol created above.
                        pointGraphic = new Graphic(new MapPoint(x, y, SpatialReferences.Wgs84), markerSymbol);
                        // Add new graphic to overlay
                        overlay.Graphics.Add(pointGraphic);
                        break;
                    case SimpleMarkerSymbolStyle.Triangle:
                        //define fist vector element , a triangle in this case.
                        vec_elem_geom = Geometry.FromJson("{\"rings\":[[[0.0,5.0],[5,-5.0],[-5,-5.0],[0.0,5.0]]]}");
                        vec_elem_fill = new SolidFillSymbolLayer(Color.Red);
                        mlfs = new MultilayerPolygonSymbol(new[] { vec_elem_fill });
                        var vmse_triangle = new VectorMarkerSymbolElement(vec_elem_geom, mlfs);
                        vmsl = new VectorMarkerSymbolLayer(new[] { vmse_triangle });
                        markerSymbol = new MultilayerPointSymbol(new[] { vmsl });
                        // Create point graphics at x, y using symbol created above.
                        pointGraphic = new Graphic(new MapPoint(x, y, SpatialReferences.Wgs84), markerSymbol);
                        // Add new graphic to overlay
                        overlay.Graphics.Add(pointGraphic);
                        break;
                    case SimpleMarkerSymbolStyle.Cross:
                        //define fist vector element , a cross in this case.
                        vec_elem_geom = Geometry.FromJson("{\"rings\":[[[-1,5],[1,5],[1,-5],[-1,-5]],[[-5,1],[5,1],[5,-1],[-5,-1]]]}");
                        vec_elem_fill = new SolidFillSymbolLayer(Color.Red);
                        mlfs = new MultilayerPolygonSymbol(new[] { vec_elem_fill });
                        var vmse_cross = new VectorMarkerSymbolElement(vec_elem_geom, mlfs);
                        vmsl = new VectorMarkerSymbolLayer(new[] { vmse_cross });
                        markerSymbol = new MultilayerPointSymbol(new[] { vmsl });
                        // Create point graphics at x, y using symbol created above.
                        pointGraphic = new Graphic(new MapPoint(x, y, SpatialReferences.Wgs84), markerSymbol);
                        // Add new graphic to overlay
                        overlay.Graphics.Add(pointGraphic);
                        break;
                    case SimpleMarkerSymbolStyle.X:
                        //define fist vector element , a cross in this case.
                        vec_elem_geom = Geometry.FromJson("{\"paths\":[[[-1,1],[0,0],[1,-1]],[[1,1],[0,0],[-1,-1]]]}");
                        var vec_elem_stroke = new SolidStrokeSymbolLayer(1, Color.Red);
                        var mlpls = new MultilayerPolylineSymbol(new[] { vec_elem_stroke });
                        var vmse_x = new VectorMarkerSymbolElement(vec_elem_geom, mlpls);
                        vmsl = new VectorMarkerSymbolLayer(new[] { vmse_x });
                        markerSymbol = new MultilayerPointSymbol(new[] { vmsl });
                        // Create point graphics at x, y using symbol created above.
                        pointGraphic = new Graphic(new MapPoint(x, y, SpatialReferences.Wgs84), markerSymbol);
                        // Add new graphic to overlay
                        overlay.Graphics.Add(pointGraphic);
                        break;
                }

            }
        }

        private void AddPointGraphicsWithPictureMarkerSymbolFromUrl(GraphicsOverlay overlay)
        {
            // Create uri to the used image
            Uri symbolUri = new Uri(
                "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Recreation/FeatureServer/0/images/e82f744ebb069bb35b234b3fea46deae");

            // Create new symbol using asynchronous factory method from uri.
            PictureMarkerSymbolLayer campsiteMarker = new PictureMarkerSymbolLayer(symbolUri)
            {
                Size = 40
            };

            MultilayerPointSymbol campsiteSymbol = new MultilayerPointSymbol(new[] { campsiteMarker });

            // Create location for the campsite
            // decrement y so all pictures appear in one column
            y -= 10.0;
            MapPoint campsitePoint = new MapPoint(x, y, SpatialReferences.Wgs84);

            // Create graphic with the location and symbol
            Graphic campsiteGraphic = new Graphic(campsitePoint, campsiteSymbol);

            // Add graphic to the graphics overlay
            overlay.Graphics.Add(campsiteGraphic);
        }

        private void AddPointGraphicsWithPictureMarkerSymbolFromResources(GraphicsOverlay overlay)
        {
            // Get current assembly that contains the image
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // Get image as a stream from the resources
            // Picture is defined as EmbeddedResource
            using (var stream = currentAssembly.GetManifestResourceStream("ArcGISRuntime.Resources.PictureMarkerSymbols.pin_star_blue.png"))
            {
                using (var mem = new MemoryStream())
                {
                    stream.CopyTo(mem);

                    // Create a runtime image from the bytes read from the memory stream
                    RuntimeImage img = new RuntimeImage(mem.ToArray());

                    // Create new PictureMarkerSymbolLayer from the runtime image object
                    PictureMarkerSymbolLayer pinMarker = new PictureMarkerSymbolLayer(img) { Size = 50 };

                    // Create a new multilayerpoint symbol with the PictureMarkerSymbolLayer
                    MultilayerPointSymbol pinSymbol = new MultilayerPointSymbol(new[] { pinMarker });

                    // Create location for the pin
                    // decrement y so all pictures appear in one column
                    y -= 10.0;
                    MapPoint pinPoint = new MapPoint(x, y, SpatialReferences.Wgs84);

                    // Create graphic with the location and symbol
                    Graphic pinGraphic = new Graphic(pinPoint, pinSymbol);

                    // Add graphic to the graphics overlay
                    overlay.Graphics.Add(pinGraphic);
                }
            }
        }

        private void AddPointGraphicsWithPictureMarkesFromBytes(GraphicsOverlay overlay)
        {
            y -= 10;

            // get bytes from FromBase64String
            var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAABGdBTUEAAYagMeiWXwAAAfJJREFUeJztm79LAnEYhz/n1OAQNBTkIOjg0GBw7QYNLVFgUEOQQW0Ojv4J/glBB11bgVBBQ9CgzgU1NDg4uBzUELQIbfV+Tw5OSbj3hgPvfR94Dn8gL99H7/SEg3QyMx4/Ji/JV/J3zn0mL8iDKAGWySfSJWtkGfOPTZ6S1+QtuRR+MhxgleyTW0gve+Q7QhHCAVxyEelnhTwP7gQBzD6f5nd+mn1y19wIAmxDHv6agwBpONhx8dccBChBHhMBJLJgNpID+GiAJIbYto1KpRLZYrGIpEgkgOM46HQ6ka3X60gK3QUgHA0A4WgACEcDQDgaAMLRANwX5PN51omNMZvNsmbkcjn2jFIp3n867AC1Wo11YmMsFAqsGdVqlT2j2WwiDroLQDgaAMLRABCOBoBwNACEowG4LxgOh+j1eixHoxFrhud57Bn9fh9xYAdwXZd9pjYYDFgz2u02e0ar1UIcdBeAcDQAhKMBIBwNAOFoAAhHAyQxpFwuw7KsyDYaDSSFfgIgHA0AufyYTRAg3t8p882b2WTCd4QxEeAR8vDXHAS4IruQwx15b26ED4JH5DfSzwfGV5L6hAN4GF881UV6eSDXyK/ggemvwU9yE+Prhl2k4+D4QjrkIbmD0OINs34HmGPCCblOWnPuBnlG3vy30D/UTySOabh2IwAAAABJRU5ErkJggg==");

            // Create a runtime image from the bytes
            RuntimeImage img = new RuntimeImage(bytes);

            // Create new PictureMarkerSymbolLayer from the runtime image object
            var symLayer = new PictureMarkerSymbolLayer(img) { Size = 30 };

            // Create a new multilayerpoint symbol with the PictureMarkerSymbolLayer
            MultilayerPointSymbol pictureSymbol = new MultilayerPointSymbol(new[] { symLayer });

            // Create graphic with the location and symbol
            var pictureFromBytesGraphic = new Graphic(new MapPoint(x, y, SpatialReferences.Wgs84), pictureSymbol);

            // Add graphic to the graphics overlay
            overlay.Graphics.Add(pictureFromBytesGraphic);

        }

        private void AddLineGraphicsWithMarkerSymbols(GraphicsOverlay overlay)
        {
            y = 40.0;
            //graphic for line geometries
            Graphic lineGraphic;

            // Multilayer polyline symbol for different line styles
            MultilayerPolylineSymbol lineSymbol;

            // Dash geometric effects that define the dash and dot template used by different line style
            DashGeometricEffect dashEffect = new DashGeometricEffect();

            // Stroke used by line symbols
            SolidStrokeSymbolLayer strokeLayer;

            //Loop through each simple line symbol style and create an equivalent multilayer polyline symbol and assign it to a graphic
            foreach (SimpleLineSymbolStyle linestyle in Enum.GetValues(typeof(SimpleLineSymbolStyle)))
            {
                // Create point graphics at x, y using symbol created above.
                PolylineBuilder polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);
                polylineBuilder.AddPoint(new MapPoint(x - 60, y, SpatialReferences.Wgs84));
                polylineBuilder.AddPoint(new MapPoint(x + 10, y, SpatialReferences.Wgs84));

                //create a new stroke for each style
                strokeLayer = new SolidStrokeSymbolLayer(3, Color.Red);

                // set captyle of stroke is rounded. With this style endings of the stroke are rounded
                strokeLayer.CapStyle = StrokeSymbolLayerCapStyle.Round;

                // Create a dash effect for each style
                dashEffect = new DashGeometricEffect();
                switch (linestyle)
                {
                    case SimpleLineSymbolStyle.Dash:
                        dashEffect.DashTemplate.Add(7); //dash
                        dashEffect.DashTemplate.Add(11); //gap
                        strokeLayer.GeometricEffects.Add(dashEffect);
                        break;
                    case SimpleLineSymbolStyle.DashDot:
                        dashEffect.DashTemplate.Add(7);//dash
                        dashEffect.DashTemplate.Add(9);//gap
                        dashEffect.DashTemplate.Add(0.5);//dot
                        dashEffect.DashTemplate.Add(9);//gap
                        strokeLayer.GeometricEffects.Add(dashEffect);
                        break;
                    case SimpleLineSymbolStyle.DashDotDot:
                        dashEffect.DashTemplate.Add(7);//dash
                        dashEffect.DashTemplate.Add(9);//gap
                        dashEffect.DashTemplate.Add(0.5);//dot
                        dashEffect.DashTemplate.Add(9);//gap
                        dashEffect.DashTemplate.Add(0.5);//dot
                        dashEffect.DashTemplate.Add(9);//gap
                        strokeLayer.GeometricEffects.Add(dashEffect);
                        break;
                    case SimpleLineSymbolStyle.Dot:
                        dashEffect.DashTemplate.Add(0.5);
                        dashEffect.DashTemplate.Add(9);
                        strokeLayer.GeometricEffects.Add(dashEffect);
                        break;
                    case SimpleLineSymbolStyle.LongDash:
                        dashEffect.DashTemplate.Add(11);//dash
                        dashEffect.DashTemplate.Add(7);//gap
                        strokeLayer.GeometricEffects.Add(dashEffect);
                        break;
                    case SimpleLineSymbolStyle.LongDashDot:
                        dashEffect.DashTemplate.Add(11);//dash
                        dashEffect.DashTemplate.Add(7);//gap
                        dashEffect.DashTemplate.Add(0.5);//dot
                        dashEffect.DashTemplate.Add(7);//gap
                        strokeLayer.GeometricEffects.Add(dashEffect);
                        break;
                    case SimpleLineSymbolStyle.ShortDash:
                        dashEffect.DashTemplate.Add(4);//dash
                        dashEffect.DashTemplate.Add(6);//gap
                        strokeLayer.GeometricEffects.Add(dashEffect);
                        break;
                    case SimpleLineSymbolStyle.ShortDashDot:
                        dashEffect.DashTemplate.Add(4);//dash
                        dashEffect.DashTemplate.Add(6);//gap
                        dashEffect.DashTemplate.Add(0.5);//dot
                        dashEffect.DashTemplate.Add(6);//gap
                        strokeLayer.GeometricEffects.Add(dashEffect);
                        break;
                    case SimpleLineSymbolStyle.ShortDashDotDot:
                        dashEffect.DashTemplate.Add(4);//dash
                        dashEffect.DashTemplate.Add(6);//gap
                        dashEffect.DashTemplate.Add(0.5);//dot
                        dashEffect.DashTemplate.Add(6);//gap
                        dashEffect.DashTemplate.Add(0.5);//dot
                        dashEffect.DashTemplate.Add(6);//gap
                        strokeLayer.GeometricEffects.Add(dashEffect);
                        break;
                    case SimpleLineSymbolStyle.ShortDot:
                        dashEffect.DashTemplate.Add(0.5);//dot
                        dashEffect.DashTemplate.Add(6);//gat
                        strokeLayer.GeometricEffects.Add(dashEffect);
                        break;
                    case SimpleLineSymbolStyle.Solid:
                        break;
                }
                if (linestyle == SimpleLineSymbolStyle.Null)
                    lineSymbol = new MultilayerPolylineSymbol(new List<SymbolLayer>());
                else
                    lineSymbol = new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeLayer });

                // Create a polyline graphic with geometry and symbol                        
                lineGraphic = new Graphic(polylineBuilder.ToGeometry(), lineSymbol);

                // Add new graphic to overlay
                overlay.Graphics.Add(lineGraphic);

                // decrement both y so all lines appear in one column
                y -= 5.0;

            }
        }

        private void AddPolygonGraphicsWithMarkerSymbols(GraphicsOverlay overlay)
        {
            y = 40;

            //Loop through each simple fill symbol style and create a multilayer polygon symbol and assign it a graphic
            foreach (SimpleFillSymbolStyle fillstyle in Enum.GetValues(typeof(SimpleFillSymbolStyle)))
            {
                // Create polygon graphics starting at x, y defined above.
                PolygonBuilder polygonBuilder = new PolygonBuilder(SpatialReferences.Wgs84);
                polygonBuilder.AddPoint(new MapPoint(x + 20, y, SpatialReferences.Wgs84));
                polygonBuilder.AddPoint(new MapPoint(x + 30, y, SpatialReferences.Wgs84));
                polygonBuilder.AddPoint(new MapPoint(x + 30, y - 5, SpatialReferences.Wgs84));
                polygonBuilder.AddPoint(new MapPoint(x + 20, y - 5, SpatialReferences.Wgs84));

                // Create a stroke symol layer to be used by hatch patterns
                SolidStrokeSymbolLayer strokeForHatches = new SolidStrokeSymbolLayer(2, Color.Red);

                // Create a stroke symbol layer for outline of polygons
                SolidStrokeSymbolLayer strokeForOutline = new SolidStrokeSymbolLayer(1, Color.Black);

                switch (fillstyle)
                {
                    case SimpleFillSymbolStyle.BackwardDiagonal:
                        // Create a hatch symbol layer for BackwardDiagonal fill style
                        HatchFillSymbolLayer backwardDiagonal = new HatchFillSymbolLayer(new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeForHatches }), -45);

                        // Define separation distance for lines in a hatch pattern
                        backwardDiagonal.Separation = 9;

                        // Create multilayer polygon symbol with symbol layers
                        MultilayerPolygonSymbol backwardDiagonalPolygonSymbol = new MultilayerPolygonSymbol(new List<SymbolLayer>() { backwardDiagonal, strokeForOutline });

                        // Create a polygon graphic with geometry and symbol                        
                        Graphic backwardDiagonalPolygonGraphic = new Graphic(polygonBuilder.ToGeometry(), backwardDiagonalPolygonSymbol);

                        // Add new graphic to overlay
                        overlay.Graphics.Add(backwardDiagonalPolygonGraphic);
                        break;
                    case SimpleFillSymbolStyle.Cross:
                        // Create cross pattern hatch symbol layers for cross fill style
                        HatchFillSymbolLayer vertical = new HatchFillSymbolLayer(new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeForHatches }), 90);

                        // Define separation distance for lines in a hatch pattern
                        vertical.Separation = 9;

                        // Define separation distance for lines in a hatch pattern
                        HatchFillSymbolLayer horizontal = new HatchFillSymbolLayer(new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeForHatches }), 0);
                        horizontal.Separation = 9;

                        // Create multilayer polygon symbol with symbol layers
                        MultilayerPolygonSymbol crossPolygonSymbol = new MultilayerPolygonSymbol(new List<SymbolLayer>() { vertical, horizontal, strokeForOutline });

                        // Create a polygon graphic with geometry and symbol                        
                        Graphic crossPolygonGraphic = new Graphic(polygonBuilder.ToGeometry(), crossPolygonSymbol);

                        // Add new graphic to overlay
                        overlay.Graphics.Add(crossPolygonGraphic);
                        break;
                    case SimpleFillSymbolStyle.DiagonalCross:

                        // Create diagonal cross pattern hatch symbol layers for diagonal cross fill style
                        HatchFillSymbolLayer diagonalStroke1 = new HatchFillSymbolLayer(new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeForHatches }), 45);
                        HatchFillSymbolLayer diagonalStroke2 = new HatchFillSymbolLayer(new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeForHatches }), -45);

                        // Define separation distance for lines in a hatch pattern
                        diagonalStroke1.Separation = 9;

                        // Define separation distance for lines in a hatch pattern
                        diagonalStroke2.Separation = 9;

                        // Create multilayer polygon symbol with symbol layers
                        MultilayerPolygonSymbol diagonalCrossPolygonSymbol = new MultilayerPolygonSymbol(new List<SymbolLayer>() { diagonalStroke1, diagonalStroke2, strokeForOutline });

                        // Create a polygon graphic with geometry and symbol
                        var diagonalCrossGraphic = new Graphic(polygonBuilder.ToGeometry(), diagonalCrossPolygonSymbol);

                        // Add new graphic to overlay
                        overlay.Graphics.Add(diagonalCrossGraphic);
                        break;
                    case SimpleFillSymbolStyle.ForwardDiagonal:
                        // Create forward diagonal pattern hatch symbol layer for forward diagonal fill style
                        HatchFillSymbolLayer forwardDiagonal = new HatchFillSymbolLayer(new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeForHatches }), -45);

                        // Define separation distance for lines in a hatch pattern
                        forwardDiagonal.Separation = 9;

                        // Create multilayer polygon symbol with symbol layers
                        MultilayerPolygonSymbol forwardDiagonalPolygonSymbol = new MultilayerPolygonSymbol(new List<SymbolLayer>() { forwardDiagonal, strokeForOutline });

                        // Create a polygon graphic with geometry and symbol
                        var forwardDiagonalGraphic = new Graphic(polygonBuilder.ToGeometry(), forwardDiagonalPolygonSymbol);

                        // Add new graphic to overlay
                        overlay.Graphics.Add(forwardDiagonalGraphic);
                        break;
                    case SimpleFillSymbolStyle.Horizontal:
                        // Create horizontal pattern hatch symbol layer for horizontal fill style
                        horizontal = new HatchFillSymbolLayer(new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeForHatches }), 0);

                        // Define separation distance for lines in a hatch pattern
                        horizontal.Separation = 9;

                        // Create multilayer polygon symbol with symbol layers
                        MultilayerPolygonSymbol horizontalPolygonSymbol = new MultilayerPolygonSymbol(new List<SymbolLayer>() { horizontal, strokeForOutline });

                        // Create a polygon graphic with geometry and symbol
                        var horizontalPolygonGraphic = new Graphic(polygonBuilder.ToGeometry(), horizontalPolygonSymbol);

                        // Add new graphic to overlay
                        overlay.Graphics.Add(horizontalPolygonGraphic);
                        break;
                    case SimpleFillSymbolStyle.Null:
                        // Create a multilayer polygon symbol with null fill. It only has an outline.
                        MultilayerPolygonSymbol nullFillPolygonSymbol = new MultilayerPolygonSymbol(new List<SymbolLayer>() { strokeForOutline });

                        // Create a polygon graphic with geometry and symbol
                        var nullFillPolygonGraphic = new Graphic(polygonBuilder.ToGeometry(), nullFillPolygonSymbol);

                        // Add new graphic to overlay
                        overlay.Graphics.Add(nullFillPolygonGraphic);
                        break;
                    case SimpleFillSymbolStyle.Solid:

                        //Create a solid fill symbol layer
                        SolidFillSymbolLayer solidFillSymbolLayer = new SolidFillSymbolLayer(Color.Red);

                        // Create multilayer polygon symbol with symbol layers
                        MultilayerPolygonSymbol solidPolygonSymbol = new MultilayerPolygonSymbol(new List<SymbolLayer>() { solidFillSymbolLayer, strokeForOutline });

                        // Create a polygon graphic with geometry and symbol
                        var solidFillPolygonGraphic = new Graphic(polygonBuilder.ToGeometry(), solidPolygonSymbol);

                        // Add new graphic to overlay
                        overlay.Graphics.Add(solidFillPolygonGraphic);
                        break;
                    case SimpleFillSymbolStyle.Vertical:
                        // Create vertical pattern hatch symbol layer for vertical fill style
                        vertical = new HatchFillSymbolLayer(new MultilayerPolylineSymbol(new List<SymbolLayer>() { strokeForHatches }), 90);

                        // Define separation distance for lines in a hatch pattern
                        vertical.Separation = 9;

                        // Create multilayer polygon symbol with symbol layers
                        MultilayerPolygonSymbol verticalPolygonSymbol = new MultilayerPolygonSymbol(new List<SymbolLayer>() { vertical, strokeForOutline });

                        // Create a polygon graphic with geometry and symbol
                        var verticalPolygonGraphic = new Graphic(polygonBuilder.ToGeometry(), verticalPolygonSymbol);

                        // Add new graphic to overlay
                        overlay.Graphics.Add(verticalPolygonGraphic);
                        break;
                }

                // decrement both y so all lines appear in one column
                y -= 9;

            }
        }

        private void AddSomeAdvancedSymbolsForPointLinePolygon(GraphicsOverlay overlay)
        {
            y = 40;
            MultilayerPointSymbol ml_pointSymbol;
            MultilayerPolylineSymbol ml_polylineSymbol;
            MultilayerPolygonSymbol ml_polygonSymbol;

            //Symbol layers for Multilayerpoint

            //0th layer with three marker elements and three vector marker layers

            //Orange envelope with Redish outline
            SolidFillSymbolLayer orange_Fill_Layer = new SolidFillSymbolLayer(Color.Orange);
            SolidStrokeSymbolLayer pink_outline = new SolidStrokeSymbolLayer(2, Color.Blue);
            var orange_Square_Geometry = new Envelope(new MapPoint(-0.5, -0.5, SpatialReferences.Wgs84), new MapPoint(0.5, 0.5, SpatialReferences.Wgs84));
            VectorMarkerSymbolElement orange_Square_Vector_Element = new VectorMarkerSymbolElement(orange_Square_Geometry, new MultilayerPolygonSymbol(new List<SymbolLayer>() { orange_Fill_Layer, pink_outline }));
            VectorMarkerSymbolLayer orange_Square_VectorMarkerLayer = new VectorMarkerSymbolLayer(new[] { orange_Square_Vector_Element });
            orange_Square_VectorMarkerLayer.Size = 11;
            orange_Square_VectorMarkerLayer.Anchor = new SymbolAnchor(-4, -6, SymbolAnchorPlacementMode.Absolute);

            // black envelope
            SolidFillSymbolLayer black_Fill_Layer = new SolidFillSymbolLayer(Color.Black);
            SolidStrokeSymbolLayer orange_outline = new SolidStrokeSymbolLayer(2, Color.OrangeRed);
            var black_Square_Geometry = new Envelope(new MapPoint(-0.5, -0.5, SpatialReferences.Wgs84), new MapPoint(0.5, 0.5, SpatialReferences.Wgs84));
            VectorMarkerSymbolElement black_Square_Vector_Element = new VectorMarkerSymbolElement(black_Square_Geometry, new MultilayerPolygonSymbol(new List<SymbolLayer>() { black_Fill_Layer, orange_outline }));
            VectorMarkerSymbolLayer black_Square_VectorMarkerLayer = new VectorMarkerSymbolLayer(new[] { black_Square_Vector_Element });
            black_Square_VectorMarkerLayer.Size = 6;
            black_Square_VectorMarkerLayer.Anchor = new SymbolAnchor(2, 1, SymbolAnchorPlacementMode.Absolute);

            // envelope with no purple outline
            SolidFillSymbolLayer transparent_Fill_Layer = new SolidFillSymbolLayer(Color.Transparent);
            SolidStrokeSymbolLayer purple_Outline = new SolidStrokeSymbolLayer(2, Color.Purple);
            var purple_Square_Geometry = new Envelope(new MapPoint(-0.5, -0.5, SpatialReferences.Wgs84), new MapPoint(0.5, 0.5, SpatialReferences.Wgs84));
            VectorMarkerSymbolElement purple_Square_Vector_Element = new VectorMarkerSymbolElement(purple_Square_Geometry, new MultilayerPolygonSymbol(new List<SymbolLayer>() { transparent_Fill_Layer, purple_Outline }));
            VectorMarkerSymbolLayer purple_Square_VectorMarkerLayer = new VectorMarkerSymbolLayer(new[] { purple_Square_Vector_Element });
            purple_Square_VectorMarkerLayer.Size = 14;
            purple_Square_VectorMarkerLayer.Anchor = new SymbolAnchor(4, 2, SymbolAnchorPlacementMode.Absolute);


            //1st layer with it's marker graphics and nested symbol layers
            var hexagon_Element_Geometry = Geometry.FromJson("{\"rings\":[[[-2.89,5.0],[2.89,5.0],[5.77,0.0],[2.89,-5.0],[-2.89,-5.0],[-5.77,0.0],[-2.89,5.0]]]}");
            SolidFillSymbolLayer yellow_Fill_Layer = new SolidFillSymbolLayer(Color.Yellow);
            SolidStrokeSymbolLayer black_Outline = new SolidStrokeSymbolLayer(2, Color.Black);
            VectorMarkerSymbolElement hexagon_Vector_Element = new VectorMarkerSymbolElement(hexagon_Element_Geometry, new MultilayerPolygonSymbol(new List<SymbolLayer>() { yellow_Fill_Layer, black_Outline }));
            VectorMarkerSymbolLayer hexagon_VectorMarkerLayer = new VectorMarkerSymbolLayer(new[] { hexagon_Vector_Element });
            hexagon_VectorMarkerLayer.Size = 35;

            ml_pointSymbol = new MultilayerPointSymbol(new List<SymbolLayer> { hexagon_VectorMarkerLayer, orange_Square_VectorMarkerLayer, black_Square_VectorMarkerLayer, purple_Square_VectorMarkerLayer });

            Graphic advanced_point_graphic = new Graphic(new MapPoint(x + 25, y, SpatialReferences.Wgs84), ml_pointSymbol);

            overlay.Graphics.Add(advanced_point_graphic);

            // decrement both y so all lines appear in one column
            y -= 10.0;

            //Symbol layers for Multilayerpolyline
            SolidStrokeSymbolLayer black_dashes = new SolidStrokeSymbolLayer(1, Color.Black);
            GeometricEffect dash_Effect = new DashGeometricEffect(new double[] { 5, 3 });
            black_dashes.GeometricEffects.Add(dash_Effect);
            black_dashes.CapStyle = StrokeSymbolLayerCapStyle.Square;

            //yellow stroke inside
            SolidStrokeSymbolLayer yellow_stroke = new SolidStrokeSymbolLayer(5, Color.Yellow);
            yellow_stroke.CapStyle = StrokeSymbolLayerCapStyle.Round;

            //black outline
            SolidStrokeSymbolLayer black_outline = new SolidStrokeSymbolLayer(7, Color.Black);
            black_outline.CapStyle = StrokeSymbolLayerCapStyle.Round;

            ml_polylineSymbol = new MultilayerPolylineSymbol(new List<SymbolLayer>() { black_outline, yellow_stroke, black_dashes });
            PolylineBuilder polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);
            polylineBuilder.AddPoint(new MapPoint(x + 40, y, SpatialReferences.Wgs84));
            polylineBuilder.AddPoint(new MapPoint(x + 10, y, SpatialReferences.Wgs84));

            // Create a polyline graphic with geometry and symbol                        
            Graphic advanced_line_graphic = new Graphic(polylineBuilder.ToGeometry(), ml_polylineSymbol);

            // Add new graphic to overlay
            overlay.Graphics.Add(advanced_line_graphic);

            // decrement both y so all lines appear in one column
            y -= 10.0;

            SolidFillSymbolLayer red_Fill_Layer = new SolidFillSymbolLayer(Color.Red);
            ml_polygonSymbol = new MultilayerPolygonSymbol(new List<SymbolLayer>() { red_Fill_Layer, black_outline, yellow_stroke, black_dashes });
            PolygonBuilder polygonBuilder = new PolygonBuilder(SpatialReferences.Wgs84);
            polygonBuilder.AddPoint(new MapPoint(x + 20, y, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(x + 30, y, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(x + 30, y - 5, SpatialReferences.Wgs84));
            polygonBuilder.AddPoint(new MapPoint(x + 20, y - 5, SpatialReferences.Wgs84));
            // Create a polyline graphic with geometry and symbol                        
            Graphic advanced_polygon_graphic = new Graphic(polygonBuilder.ToGeometry(), ml_polygonSymbol);

            // Add new graphic to overlay
            overlay.Graphics.Add(advanced_polygon_graphic);
        }
    }
}
