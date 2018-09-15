// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.IdentifyGraphics
{
    [Register("IdentifyGraphics")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Identify graphics",
        "GraphicsOverlay",
        "This sample demonstrates how to identify graphics in a graphics overlay. When you tap on a graphic on the map, you will see an alert message displayed.",
        "")]
    public class IdentifyGraphics : UIViewController
    {
        // Create and hold a reference to the MapView.
        private MapView _myMapView;

        // Graphics overlay to host graphics.
        private GraphicsOverlay _polygonOverlay;

        public IdentifyGraphics()
        {
            Title = "Identify graphics";
        }

        public override void LoadView()
        {
            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            View = new UIView();
            View.AddSubviews(_myMapView);

            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Initialize();
        }

        private void Initialize()
        {
            // Create and show a topographic basemap.
            _myMapView.Map = new Map(Basemap.CreateTopographic());

            // Create graphics overlay with graphics.
            CreateOverlay();

            // Respond to taps on the map.
            _myMapView.GeoViewTapped += OnMapViewTapped;
        }

        private void CreateOverlay()
        {
            // Create polygon builder and add polygon corners into it.
            PolygonBuilder builder = new PolygonBuilder(SpatialReferences.WebMercator);
            builder.AddPoint(new MapPoint(-20e5, 20e5));
            builder.AddPoint(new MapPoint(20e5, 20e5));
            builder.AddPoint(new MapPoint(20e5, -20e5));
            builder.AddPoint(new MapPoint(-20e5, -20e5));

            // Get geometry from the builder.
            Polygon polygonGeometry = builder.ToGeometry();

            // Create symbol for the polygon.
            SimpleFillSymbol polygonSymbol = new SimpleFillSymbol(
                SimpleFillSymbolStyle.Solid,
                System.Drawing.Color.Yellow,
                null);

            // Create new graphic.
            Graphic polygonGraphic = new Graphic(polygonGeometry, polygonSymbol);

            // Create overlay to where graphics are shown.
            _polygonOverlay = new GraphicsOverlay();
            _polygonOverlay.Graphics.Add(polygonGraphic);

            // Add created overlay to the MapView.
            _myMapView.GraphicsOverlays.Add(_polygonOverlay);
        }

        private async void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            double tolerance = 10d; // Use larger tolerance for touch.
            int maximumResults = 1; // Only return one graphic  .
            bool onlyReturnPopups = false; // Don't only return popups.

            // Use the following method to identify graphics in a specific graphics overlay.
            IdentifyGraphicsOverlayResult identifyResults = await _myMapView.IdentifyGraphicsOverlayAsync(
                _polygonOverlay,
                e.Position,
                tolerance,
                onlyReturnPopups,
                maximumResults);

            // Check if we got results.
            if (identifyResults.Graphics.Count > 0)
            {
                // Make sure that the UI changes are done in the UI thread.
                InvokeOnMainThread(() =>
                {
                    UIAlertView alert = new UIAlertView("", "Tapped on graphic", (IUIAlertViewDelegate) null, "OK", null);
                    alert.Show();
                });
            }
        }
    }
}