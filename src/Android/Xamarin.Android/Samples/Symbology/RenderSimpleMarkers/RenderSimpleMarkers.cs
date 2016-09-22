// Copyright 2016 Esri.
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
using System.Drawing;

namespace ArcGISRuntimeXamarin.Samples.RenderSimpleMarkers
{
    [Activity]
    public class RenderSimpleMarkers : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Title = "Render simple markers";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateImagery());

            // Create initial map location and reuse the location for graphic
            MapPoint centralLocation = new MapPoint(-226773, 6550477, SpatialReferences.WebMercator);
            Viewpoint initialViewpoint = new Viewpoint(centralLocation, 7500);

            // Set initial viewpoint
            myMap.InitialViewpoint = initialViewpoint;

            // Provide used Map to the MapView
            _myMapView.Map = myMap;

            // Create overlay to where graphics are shown
            GraphicsOverlay overlay = new GraphicsOverlay();

            // Add created overlay to the MapView
            _myMapView.GraphicsOverlays.Add(overlay);

            // Create a simple marker symbol
            SimpleMarkerSymbol simpleSymbol = new SimpleMarkerSymbol()
            {
                Color = Color.Red,
                Size = 10,
                Style = SimpleMarkerSymbolStyle.Circle
            };

            // Add a new graphic with a central point that was created earlier
            Graphic graphicWithSymbol = new Graphic(centralLocation, simpleSymbol);
            overlay.Graphics.Add(graphicWithSymbol);
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}