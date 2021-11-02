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
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.RenderSimpleSymbols
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Symbols - 2D simple symbols",
        category: "Symbology",
        description: "Shows simple 2D symbols on a map.",
        instructions: "The sample loads graphics with a predefined simple 2d symbols on a map",
        tags: new[] { "graphics", "marker", "picture", "symbol", "fill", "line", "visualization" })]
    [ArcGISRuntime.Samples.Shared.Attributes.EmbeddedResource(@"PictureMarkerSymbols\pin_star_blue.png")]

    public partial class RenderSimpleSymbols
    {
        double x = -100.0;
        double y = 50.0;
        public RenderSimpleSymbols()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(BasemapStyle.ArcGISLightGrayBase);

            // Provide used Map to the MapView
            MyMapView.Map = myMap;

            // Create overlay to where graphics are shown
            GraphicsOverlay overlay = new GraphicsOverlay();

            #region "Add Simple Marker Symbols"
            // Add created overlay to the MapView
            MyMapView.GraphicsOverlays.Add(overlay);

            //Graphic for labeling simple marker symbols column
            Graphic textGraphic4Markers = new Graphic(new MapPoint(x, y, SpatialReferences.Wgs84),
                new TextSymbol()
                {
                    Text = "Marker",
                    Color = System.Drawing.Color.Red,
                    BackgroundColor = System.Drawing.Color.Yellow,
                    Size = 25,
                    OutlineColor= Color.Green,
                    OutlineWidth=1
                });
            overlay.Graphics.Add(textGraphic4Markers);
            AddPointGraphicsWithMarkerSymbols(overlay);

            #endregion "Add Simple Marker Symbols"

            #region "Add Picture Marker Symbols"
            //Graphic for labeling picture marker symbols column
            x = -60.0;
            y = 50.0;
            Graphic textGraphic4PictureMarkers = new Graphic(new MapPoint(x, y, SpatialReferences.Wgs84),
                new TextSymbol()
                {
                    Text = "Picture",
                    Color = System.Drawing.Color.Red,
                    BackgroundColor = System.Drawing.Color.Yellow,
                    Size = 25,
                    FontStyle=Esri.ArcGISRuntime.Symbology.FontStyle.Italic,
                    FontWeight=Esri.ArcGISRuntime.Symbology.FontWeight.Bold
                    
                });
            overlay.Graphics.Add(textGraphic4PictureMarkers);

            // Add picture graphics using different source types

            // #1: Using url
            AddPointGraphicsWithPictureMarkerSymbolFromUrl(overlay);

            try
            {
                //#2: Using embedded resorce
                await AddPointGraphicsWithPictureMarkerSymbolFromResources(overlay);

                //#3: Using byte data
                await AddPointGraphicsWithPictureMarkesFromBytes(overlay);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }

            #endregion "Add Picture Marker Symbols"

            #region "Add Line Symbols"
            //Graphic for labeling line marker symbols column
            x = -20.0;
            y = 50.0;
            Graphic textGraphic4lineSymbols = new Graphic(new MapPoint(x, y, SpatialReferences.Wgs84),
                new TextSymbol()
                {
                    Text = "Line",
                    Color = System.Drawing.Color.Red,
                    HorizontalAlignment = Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Right,
                    BackgroundColor = System.Drawing.Color.Yellow,
                    Size = 25,
                    FontDecoration=FontDecoration.Underline                    
                });
            overlay.Graphics.Add(textGraphic4lineSymbols);

            AddLineGraphicsWithMarkerSymbols(overlay);
            #endregion "Add Line Symbols"

            #region "Add Polygon Symbols"
            //Graphic for labeling line marker symbols column
            x = -10.0;
            y = 50.0;
            Graphic textGraphic4fillSymbols = new Graphic(new MapPoint(x, y, SpatialReferences.Wgs84),
                new TextSymbol()
                {
                    Text = "Fill",
                    HorizontalAlignment=Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Left,
                    Color = System.Drawing.Color.Red,
                    BackgroundColor = System.Drawing.Color.Yellow,
                    Size = 25,
                    HaloColor=Color.Green,
                    HaloWidth=1.5
                });
            overlay.Graphics.Add(textGraphic4fillSymbols);

            AddPolygonGraphicsWithMarkerSymbols(overlay);
            #endregion "Add Polygon Symbols"

        }

        private void AddPointGraphicsWithMarkerSymbols(GraphicsOverlay overlay)
        {
            //graphic for point geometries
            Graphic pointGraphic;

            //symbol for different marker styles
            SimpleMarkerSymbol markerSymbol;

            // Loop through each symbol style and create symbol and graphic
            foreach (SimpleMarkerSymbolStyle markerstyle in Enum.GetValues(typeof(SimpleMarkerSymbolStyle)))
            {
                // decrement y so all markers appear in one column
                y -= 10.0;

                // Create a simple symbol using each symbol style
                markerSymbol = new SimpleMarkerSymbol()
                {
                    Color = System.Drawing.Color.Red,
                    Size = 10,
                    Style = markerstyle
                };

                // Create point graphics at x, y using symbol created above.
                pointGraphic = new Graphic(new MapPoint(x, y, SpatialReferences.Wgs84), markerSymbol);

                // Add new graphic to overlay
                overlay.Graphics.Add(pointGraphic);
            }
        }

        private void AddPointGraphicsWithPictureMarkerSymbolFromUrl(GraphicsOverlay overlay)
        {
            // Create uri to the used image
            Uri symbolUri = new Uri(
                "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Recreation/FeatureServer/0/images/e82f744ebb069bb35b234b3fea46deae");

            // Create new symbol using asynchronous factory method from uri.
            PictureMarkerSymbol campsiteSymbol = new PictureMarkerSymbol(symbolUri)
            {
                Width = 40,
                Height = 40
            };

            // Create location for the campsite
            // decrement y so all pictures appear in one column
            y -= 10.0;
            MapPoint campsitePoint = new MapPoint(x, y, SpatialReferences.Wgs84);

            // Create graphic with the location and symbol
            Graphic campsiteGraphic = new Graphic(campsitePoint, campsiteSymbol);

            // Add graphic to the graphics overlay
            overlay.Graphics.Add(campsiteGraphic);
        }

        private async Task AddPointGraphicsWithPictureMarkerSymbolFromResources(GraphicsOverlay overlay)
        {
            // Get current assembly that contains the image
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // Get image as a stream from the resources
            // Picture is defined as EmbeddedResource and DoNotCopy
            Stream resourceStream = currentAssembly.GetManifestResourceStream(
                "ArcGISRuntime.Resources.PictureMarkerSymbols.pin_star_blue.png");

            // Create new symbol using asynchronous factory method from stream
            PictureMarkerSymbol pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);
            pinSymbol.Width = 50;
            pinSymbol.Height = 50;

            // Create location for the pin
            // decrement y so all pictures appear in one column
            y -= 10.0;
            MapPoint pinPoint = new MapPoint(x, y, SpatialReferences.Wgs84);

            // Create graphic with the location and symbol
            Graphic pinGraphic = new Graphic(pinPoint, pinSymbol);

            // Add graphic to the graphics overlay
            overlay.Graphics.Add(pinGraphic);
        }

        private async Task AddPointGraphicsWithPictureMarkesFromBytes(GraphicsOverlay overlay)
        {
            var bytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAABGdBTUEAAYagMeiWXwAAAfJJREFUeJztm79LAnEYhz/n1OAQNBTkIOjg0GBw7QYNLVFgUEOQQW0Ojv4J/glBB11bgVBBQ9CgzgU1NDg4uBzUELQIbfV+Tw5OSbj3hgPvfR94Dn8gL99H7/SEg3QyMx4/Ji/JV/J3zn0mL8iDKAGWySfSJWtkGfOPTZ6S1+QtuRR+MhxgleyTW0gve+Q7QhHCAVxyEelnhTwP7gQBzD6f5nd+mn1y19wIAmxDHv6agwBpONhx8dccBChBHhMBJLJgNpID+GiAJIbYto1KpRLZYrGIpEgkgOM46HQ6ka3X60gK3QUgHA0A4WgACEcDQDgaAMLRANwX5PN51omNMZvNsmbkcjn2jFIp3n867AC1Wo11YmMsFAqsGdVqlT2j2WwiDroLQDgaAMLRABCOBoBwNACEowG4LxgOh+j1eixHoxFrhud57Bn9fh9xYAdwXZd9pjYYDFgz2u02e0ar1UIcdBeAcDQAhKMBIBwNAOFoAAhHAyQxpFwuw7KsyDYaDSSFfgIgHA0AufyYTRAg3t8p882b2WTCd4QxEeAR8vDXHAS4IruQwx15b26ED4JH5DfSzwfGV5L6hAN4GF881UV6eSDXyK/ggemvwU9yE+Prhl2k4+D4QjrkIbmD0OINs34HmGPCCblOWnPuBnlG3vy30D/UTySOabh2IwAAAABJRU5ErkJggg==");
            y -= 10;
            using (var ms = new MemoryStream(bytes))
            {
                var pms = await PictureMarkerSymbol.CreateAsync(ms);
                pms.Width = 30;
                pms.Height = 30;
                // Create graphic with the location and symbol
                var pictureFromBytesGraphic = new Graphic(new MapPoint(x, y, SpatialReferences.Wgs84), pms);
                // Add graphic to the graphics overlay
                overlay.Graphics.Add(pictureFromBytesGraphic);
            }
        }


        private void AddLineGraphicsWithMarkerSymbols(GraphicsOverlay overlay)
        {
            x = -30.0;
            y = 40.0;
            //graphic for line geometries
            Graphic lineGraphic;

            //symbol for different line styles
            SimpleLineSymbol lineSymbol;

            //Loop through each line symbol style and create symbol and graphic
            foreach (SimpleLineSymbolStyle linestyle in Enum.GetValues(typeof(SimpleLineSymbolStyle)))
            {
                // Create a simple symbol using each symbol style
                lineSymbol = new SimpleLineSymbol()
                {
                    Color = System.Drawing.Color.Red,
                    Width = 5,
                    Style = linestyle
                };

                // Create point graphics at x, y using symbol created above.
                PolylineBuilder polylineBuilder = new PolylineBuilder(SpatialReferences.Wgs84);
                polylineBuilder.AddPoint(new MapPoint(x - 10, y, SpatialReferences.Wgs84));
                polylineBuilder.AddPoint(new MapPoint(x + 10, y, SpatialReferences.Wgs84));
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

            //graphic for polygon geometries
            Graphic polygonGraphic;

            //symbol for different fill styles
            SimpleFillSymbol polygonSymbol;

            //Loop through each line symbol style and create symbol and graphic
            foreach (SimpleFillSymbolStyle fillstyle in Enum.GetValues(typeof(SimpleFillSymbolStyle)))
            {
                // Create a simple symbol using each symbol style
                polygonSymbol = new SimpleFillSymbol()
                {
                    Color = System.Drawing.Color.Red,
                    //Outline = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Black, Width = 2),
                    Style = fillstyle                   
                };

                // Create polygon graphics starting at x, y defined above.
                PolygonBuilder polygonBuilder = new PolygonBuilder(SpatialReferences.Wgs84);
                polygonBuilder.AddPoint(new MapPoint(x, y, SpatialReferences.Wgs84));
                polygonBuilder.AddPoint(new MapPoint(x + 10, y, SpatialReferences.Wgs84));
                polygonBuilder.AddPoint(new MapPoint(x + 10, y - 5, SpatialReferences.Wgs84));
                polygonBuilder.AddPoint(new MapPoint(x, y - 5, SpatialReferences.Wgs84));
                polygonGraphic = new Graphic(polygonBuilder.ToGeometry(), polygonSymbol);

                // Add new graphic to overlay
                overlay.Graphics.Add(polygonGraphic);
               
                // decrement both y so all lines appear in one column
                y -= 10;

            }
        }
    }
}
