﻿// Copyright 2016 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace ArcGISRuntime.WPF.Samples.RenderPictureMarkers
{

    public partial class RenderPictureMarkers
    {
        public RenderPictureMarkers()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateTopographic());

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
            await CreatePictureMarkerSymbolFromUrl(overlay);
            await CreatePictureMarkerSymbolFromResources(overlay);
        }

        private async Task CreatePictureMarkerSymbolFromUrl(GraphicsOverlay overlay)
        {
            // Create uri to the used image
            var symbolUri = new Uri(
                "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Recreation/FeatureServer/0/images/e82f744ebb069bb35b234b3fea46deae");

            // Create new symbol using asynchronous factory method from uri
            PictureMarkerSymbol campsiteSymbol = new PictureMarkerSymbol(symbolUri);

            // Optionally set the size (if not set, the size in pixels of the image will be used)
            campsiteSymbol.Height = 18;
            campsiteSymbol.Width = 18;

            // Create location for the campsite
            MapPoint campsitePoint = new MapPoint(-223560, 6552021, SpatialReferences.WebMercator);

            // Create graphic with the location and symbol
            Graphic campsiteGraphic = new Graphic(campsitePoint, campsiteSymbol);

            // Add graphic to the graphics overlay
            overlay.Graphics.Add(campsiteGraphic);
        }

        private async Task CreatePictureMarkerSymbolFromResources(GraphicsOverlay overlay)
        {
            // Get current assembly that contains the image
            var currentAssembly = Assembly.GetExecutingAssembly();

            // Get image as a stream from the resources
            // Picture is defined as EmbeddedResource and DoNotCopy
            var resourceStream = currentAssembly.GetManifestResourceStream(
                "ArcGISRuntime.WPF.Resources.PictureMarkerSymbols.pin_star_blue.png");

            // Create new symbol using asynchronous factory method from stream
            PictureMarkerSymbol pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);

            // Create location for the pint
            MapPoint pinPoint = new MapPoint(-226773, 6550477, SpatialReferences.WebMercator);

            // Create graphic with the location and symbol
            Graphic pinGraphic = new Graphic(pinPoint, pinSymbol);

            // Add graphic to the graphics overlay
            overlay.Graphics.Add(pinGraphic);
        }
    }
}
