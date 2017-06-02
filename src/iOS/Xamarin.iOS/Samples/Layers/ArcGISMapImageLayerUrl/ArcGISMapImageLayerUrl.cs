// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ArcGISMapImageLayerUrl
{
    [Register("ArcGISMapImageLayerUrl")]
    public class ArcGISMapImageLayerUrl : UIViewController
    {
        // Constant holding offset where the MapView control should start
        private const int yPageOffset = 60;

        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        public ArcGISMapImageLayerUrl()
        {
            Title = "ArcGIS map image layer (URL)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            base.ViewDidLayoutSubviews();
        }

        private void Initialize()
        {
            // Create new Map
            Map myMap = new Map();

            // Create uri to the map image layer
            var serviceUri = new Uri(
               "https://sampleserver5.arcgisonline.com/arcgis/rest/services/Elevation/WorldElevations/MapServer");

            // Create new image layer from the url
            ArcGISMapImageLayer imageLayer = new ArcGISMapImageLayer(serviceUri);

            // Add created layer to the basemaps collection
            myMap.Basemap.BaseLayers.Add(imageLayer);

            // Assign the map to the MapView
            _myMapView.Map = myMap;
        }

        private void CreateLayout()
        {
            // Add MapView to the page
            View.AddSubviews(_myMapView);
        }
    }
}