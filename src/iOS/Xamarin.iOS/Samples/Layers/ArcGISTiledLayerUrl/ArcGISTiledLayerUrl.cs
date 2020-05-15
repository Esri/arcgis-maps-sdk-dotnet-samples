// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ArcGISTiledLayerUrl
{
    [Register("ArcGISTiledLayerUrl")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "ArcGIS tiled layer",
        category: "Layers",
        description: "Load an ArcGIS tiled layer from a URL.",
        instructions: "Launch the app to view the \"World Topographic Map\" tile layer as the basemap. ",
        tags: new[] { "basemap", "layers", "raster tiles", "tiled layer", "visualization" })]
    public class ArcGISTiledLayerUrl : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        public ArcGISTiledLayerUrl()
        {
            Title = "ArcGIS tiled layer (URL)";
        }

        private void Initialize()
        {
            // Create new Map.
            Map map = new Map();

            // Create URI to the tiled service.
            Uri serviceUri = new Uri("https://services.arcgisonline.com/arcgis/rest/services/World_Topo_Map/MapServer");

            // Create new tiled layer from the URL.
            ArcGISTiledLayer imageLayer = new ArcGISTiledLayer(serviceUri);

            // Add created layer to the basemaps collection.
            map.Basemap.BaseLayers.Add(imageLayer);

            // Assign the map to the MapView.
            _myMapView.Map = map;
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
    }
}