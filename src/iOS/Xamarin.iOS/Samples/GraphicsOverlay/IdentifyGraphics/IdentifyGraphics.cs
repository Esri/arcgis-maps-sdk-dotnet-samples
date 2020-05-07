// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
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
        "Display an alert message when a graphic is clicked.",
        "Select a graphic to identify it. You will see an alert message displayed.",
        "graphics", "identify")]
    public class IdentifyGraphics : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        // Graphics overlay to host graphics.
        private GraphicsOverlay _polygonOverlay;

        public IdentifyGraphics()
        {
            Title = "Identify graphics";
        }

        private void Initialize()
        {
            // Create and show a topographic basemap.
            _myMapView.Map = new Map(Basemap.CreateTopographic());

            // Create graphics overlay with graphics.
            CreateOverlay();
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
            try
            {
                // Use the following method to identify graphics in a specific graphics overlay.
                IdentifyGraphicsOverlayResult identifyResults = await _myMapView.IdentifyGraphicsOverlayAsync(
                    graphicsOverlay: _polygonOverlay,
                    screenPoint: e.Position,
                    tolerance: 10d,
                    returnPopupsOnly: false,
                    maximumResults: 1);

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
            catch (Exception ex)
            {
                new UIAlertView(title: "Error", message: ex.ToString(), del: (IUIAlertViewDelegate) null, cancelButtonTitle: "OK", otherButtons: null).Show();
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_myMapView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.GeoViewTapped += OnMapViewTapped;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= OnMapViewTapped;
        }
    }
}