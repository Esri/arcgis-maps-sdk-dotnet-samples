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
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using System;

namespace ArcGISRuntimeXamarin.Samples.KmlLayerUrl
{
    [Activity]
    public class KmlLayerUrl : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Hold the Uri for the service
        private Uri _serviceUri = new Uri("http://www.wpc.ncep.noaa.gov/kml/noaa_chart/WPC_Day1_SigWx.kml");

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "KML layer (URL)";

            // Create the UI, setup the control references
            CreateLayout();

            // Initialize the sample
            Initialize();
        }

        private void Initialize()
        {
            // Initialize the map with a dark gray basemap
            _myMapView.Map = new Map(Basemap.CreateDarkGrayCanvasVector());

            // Create a KML dataset
            KmlDataset fileDataSource = new KmlDataset(_serviceUri);

            // Create a KML layer from the dataset
            KmlLayer displayLayer = new KmlLayer(fileDataSource);

            // Add the layer to the map
            _myMapView.Map.OperationalLayers.Add(displayLayer);
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