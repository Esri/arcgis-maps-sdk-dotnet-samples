// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.KmlLayerUrl
{
    [Register("KmlLayerUrl")]
    public class KmlLayerUrl : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Hold the Uri for the service
        private Uri _serviceUri = new Uri("http://www.wpc.ncep.noaa.gov/kml/noaa_chart/WPC_Day1_SigWx.kml");

        public KmlLayerUrl()
        {
            Title = "KML layer (URL)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

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

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            base.ViewDidLayoutSubviews();
        }

        private void CreateLayout()
        {
            // Add MapView to the page
            View.AddSubviews(_myMapView);
        }
    }
}