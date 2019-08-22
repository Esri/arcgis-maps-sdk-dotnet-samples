// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;

namespace ArcGISRuntime.Samples.SimpleRenderers
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Simple renderer",
        "Symbology",
        "This sample demonstrates how to create a simple renderer and add it to a graphics overlay. Renderers define the symbology for all graphics in a graphics overlay (unless they are overridden by setting the symbol directly on the graphic). Simple renderers can also be defined on feature layers using the same code.",
        "")]
    public class SimpleRenderers : Activity
    {
        // Hold a reference to the map view.
        private MapView _myMapView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Simple renderer";

            // Create the UI, setup the control references and execute initialization.
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new map with basemap layer.
            Map myMap = new Map(Basemap.CreateImageryWithLabels());

            // Create several map points using the WGS84 coordinates (latitude and longitude).
            MapPoint oldFaithfulPoint = new MapPoint(-110.828140, 44.460458, SpatialReferences.Wgs84);
            MapPoint cascadeGeyserPoint = new MapPoint(-110.829004, 44.462438, SpatialReferences.Wgs84);
            MapPoint plumeGeyserPoint = new MapPoint(-110.829381, 44.462735, SpatialReferences.Wgs84);

            // Use the two points farthest apart to create an envelope.
            Envelope initialEnvelope = new Envelope(oldFaithfulPoint, plumeGeyserPoint);

            // Set the map initial viewpoint.
            myMap.InitialViewpoint = new Viewpoint(initialEnvelope);

            // Add the map to the map view.
            _myMapView.Map = myMap;

            // Set the viewpoint to the envelope with padding.
            _myMapView.SetViewpointGeometryAsync(initialEnvelope, 50);

            // Create a graphics overlay.
            GraphicsOverlay myGraphicOverlay = new GraphicsOverlay();

            // Create graphics based upon the map points.
            Graphic oldFaithfulGraphic = new Graphic(oldFaithfulPoint);
            Graphic cascadeGeyserGraphic = new Graphic(cascadeGeyserPoint);
            Graphic plumeGeyserGraphic = new Graphic(plumeGeyserPoint);

            // Add the graphics to the graphics overlay.
            myGraphicOverlay.Graphics.Add(oldFaithfulGraphic);
            myGraphicOverlay.Graphics.Add(cascadeGeyserGraphic);
            myGraphicOverlay.Graphics.Add(plumeGeyserGraphic);

            // Create a simple marker symbol - red, cross, size 12.
            SimpleMarkerSymbol mySymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.Red, 12);

            // Create a simple renderer based on the simple marker symbol.
            SimpleRenderer myRenderer = new SimpleRenderer(mySymbol);

            // Apply the renderer to the graphics overlay (all graphics use the same symbol).
            myGraphicOverlay.Renderer = myRenderer;

            // Add the graphics overlay to the map view.
            _myMapView.GraphicsOverlays.Add(myGraphicOverlay);
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}