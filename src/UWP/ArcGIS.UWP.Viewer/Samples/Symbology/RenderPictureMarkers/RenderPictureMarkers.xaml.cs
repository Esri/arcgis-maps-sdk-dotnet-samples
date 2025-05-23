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
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace ArcGIS.UWP.Samples.RenderPictureMarkers
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Picture marker symbol",
        category: "Symbology",
        description: "Use pictures for markers.",
        instructions: "When launched, this sample displays a map with picture marker symbols. Pan and zoom to explore the map.",
        tags: new[] { "graphics", "marker", "picture", "symbol", "visualization" })]
    [ArcGIS.Samples.Shared.Attributes.EmbeddedResource(@"PictureMarkerSymbols\pin_star_blue.png")]
    public sealed partial class RenderPictureMarkers
    {
        public RenderPictureMarkers()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(BasemapStyle.ArcGISTopographic);

            // Create and set initial map area
            Envelope initialLocation = new Envelope(
                -229835, 6550763, -222560, 6552021,
                SpatialReferences.WebMercator);
            myMap.InitialViewpoint = new Viewpoint(initialLocation);

            // Assign the map to the MapView
            MyMapView.Map = myMap;

            // Create overlay to where graphics are shown
            GraphicsOverlay overlay = new GraphicsOverlay();

            // Add created overlay to the MapView
            MyMapView.GraphicsOverlays.Add(overlay);

            // Add graphics using different source types
            CreatePictureMarkerSymbolFromUrl(overlay);
            CreatePictureMarkerSymbolFromResources(overlay);
        }

        private void CreatePictureMarkerSymbolFromUrl(GraphicsOverlay overlay)
        {
            // Create uri to the used image
            Uri symbolUri = new Uri(
                "https://static.arcgis.com/images/Symbols/OutdoorRecreation/Camping.png");

            // Create new symbol using asynchronous factory method from uri.
            PictureMarkerSymbol campsiteSymbol = new PictureMarkerSymbol(symbolUri)
            {
                Width = 40,
                Height = 40
            };

            // Create location for the campsite
            MapPoint campsitePoint = new MapPoint(-223560, 6552021, SpatialReferences.WebMercator);

            // Create graphic with the location and symbol
            Graphic campsiteGraphic = new Graphic(campsitePoint, campsiteSymbol);

            // Add graphic to the graphics overlay
            overlay.Graphics.Add(campsiteGraphic);
        }

        private void CreatePictureMarkerSymbolFromResources(GraphicsOverlay overlay)
        {
            // "ms-appx" is a URI scheme that points to the app's package and can be used to open files with "Content" build action.
            Uri resourceUri = new Uri("ms-appx:///Resources/PictureMarkerSymbols/pin_star_blue.png");

            // Create a picture marker symbol from the bundled image.
            PictureMarkerSymbol pinSymbol = new PictureMarkerSymbol(resourceUri);
            pinSymbol.Width = 50;
            pinSymbol.Height = 50;

            // Create location for the pint
            MapPoint pinPoint = new MapPoint(-226773, 6550477, SpatialReferences.WebMercator);

            // Create graphic with the location and symbol
            Graphic pinGraphic = new Graphic(pinPoint, pinSymbol);

            // Add graphic to the graphics overlay
            overlay.Graphics.Add(pinGraphic);
        }
    }
}