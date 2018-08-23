// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Drawing;
using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.RenderSimpleMarkers
{
    [Register("RenderSimpleMarkers")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Render simple markers",
        "Symbology",
        "This sample adds a point graphic to a graphics overlay symbolized with a red circle specified via a SimpleMarkerSymbol.",
        "")]
    public class RenderSimpleMarkers : UIViewController
    {
        // Create and hold a reference to the used MapView.
        private MapView _myMapView;

        public RenderSimpleMarkers()
        {
            Title = "Render simple markers";
        }

        public override void LoadView()
        {
            base.LoadView();

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubviews(_myMapView);

            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization.
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with imagery basemap.
            Map myMap = new Map(Basemap.CreateImagery());

            // Create initial map location and reuse the location for graphic.
            MapPoint centralLocation = new MapPoint(-226773, 6550477, SpatialReferences.WebMercator);

            // Set initial viewpoint.
            myMap.InitialViewpoint = new Viewpoint(centralLocation, 7500);

            // Provide used Map to the MapView.
            _myMapView.Map = myMap;

            // Create overlay to where graphics are shown.
            GraphicsOverlay overlay = new GraphicsOverlay();

            // Add created overlay to the MapView.
            _myMapView.GraphicsOverlays.Add(overlay);

            // Create a simple marker symbol.
            SimpleMarkerSymbol simpleSymbol = new SimpleMarkerSymbol
            {
                Color = Color.Red,
                Size = 10,
                Style = SimpleMarkerSymbolStyle.Circle
            };

            // Add a new graphic with a central point that was created earlier.
            Graphic graphicWithSymbol = new Graphic(centralLocation, simpleSymbol);
            overlay.Graphics.Add(graphicWithSymbol);
        }
    }
}