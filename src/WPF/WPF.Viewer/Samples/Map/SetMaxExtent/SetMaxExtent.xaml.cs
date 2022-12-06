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

namespace ArcGIS.WPF.Samples.SetMaxExtent
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Set max extent",
        category: "Map",
        description: "Limit the view of a map to a particular area.",
        instructions: "The application loads with a map whose maximum extent has been set to the borders of Colorado. Note that you won't be able to pan far from the Colorado border or zoom out beyond the minimum scale set by the max extent. Use the control to disable the max extent to freely pan/zoom around the map.",
        tags: new[] { "extent", "limit panning", "map", "mapview", "max extent", "zoom" })]
    public partial class SetMaxExtent
    {
        // Hold a reference to the extent envelope.
        private Envelope _extentEnvelope;

        public SetMaxExtent()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create a new Map.
            Map myMap = new Map(BasemapStyle.ArcGISStreets);

            // Create an envelope for the map extent with points on Colorado's Northwest and Southeast corners.
            MapPoint northEastPoint = new MapPoint(-12139393.2109, 5012444.0468, SpatialReferences.WebMercator);
            MapPoint southWestPoint = new MapPoint(-11359277.5124, 4438148.7816, SpatialReferences.WebMercator);
            _extentEnvelope = new Envelope(northEastPoint, southWestPoint);

            // Create a graphics overlay of the map's max extent.
            GraphicsOverlay overlay = new GraphicsOverlay();

            // Create a simple red dashed line renderer.
            overlay.Renderer = new SimpleRenderer(new SimpleLineSymbol(style: SimpleLineSymbolStyle.Dash, color: System.Drawing.Color.Red, width: 5));

            // Set the graphic's geometry to the max extent of the map.
            overlay.Graphics.Add(new Graphic(_extentEnvelope));

            // Add the graphics overlay to the MapView.
            MyMapView.GraphicsOverlays.Add(overlay);

            // Create an initial viewpoint from the extent.
            Viewpoint initialViewpoint = new Viewpoint(_extentEnvelope);
            myMap.InitialViewpoint = initialViewpoint;

            // Assign the map to the MapView.
            MyMapView.Map = myMap;

            // Enable the max extent.
            MaxExtentCheckBox.IsChecked = true;
        }

        private void MaxExtentCheckBoxChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_extentEnvelope != null)
            {
                // Set the map max extent to null to disable the max extent or to the extentEnvelope to enable the max extent.
                MyMapView.Map.MaxExtent = MaxExtentCheckBox.IsChecked == true ? _extentEnvelope : null;
            }
        }
    }
}