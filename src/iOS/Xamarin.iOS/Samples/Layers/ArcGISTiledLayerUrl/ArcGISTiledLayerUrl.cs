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

namespace ArcGISRuntime.Samples.ArcGISTiledLayerUrl
{
    [Register("ArcGISTiledLayerUrl")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "ArcGIS tiled layer (URL)",
        "Layers",
        "This sample demonstrates how to add an ArcGISTiledLayer as a base layer in a map. The ArcGISTiledLayer comes from an ArcGIS Server sample web service.",
        "")]
    public class ArcGISTiledLayerUrl : UIViewController
    {
        // Create and hold a reference to the MapView.
        private readonly MapView _myMapView = new MapView();

        public ArcGISTiledLayerUrl()
        {
            Title = "ArcGIS tiled layer (URL)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

                // Reposition controls.
                _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, 0, 0);

                base.ViewDidLayoutSubviews();
            }
            catch (NullReferenceException)
            {
            }
        }

        private void Initialize()
        {
            // Create new Map.
            Map map = new Map();

            // Create URI to the tiled service.
            var serviceUri = new Uri("https://services.arcgisonline.com/arcgis/rest/services/World_Topo_Map/MapServer");

            // Create new tiled layer from the URL.
            ArcGISTiledLayer imageLayer = new ArcGISTiledLayer(serviceUri);

            // Add created layer to the basemaps collection.
            map.Basemap.BaseLayers.Add(imageLayer);

            // Assign the map to the MapView.
            _myMapView.Map = map;
        }

        private void CreateLayout()
        {
            // Add MapView to the page.
            View.AddSubviews(_myMapView);
        }
    }
}