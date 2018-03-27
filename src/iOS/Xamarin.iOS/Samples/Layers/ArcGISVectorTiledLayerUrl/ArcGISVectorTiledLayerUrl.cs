// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace ArcGISRuntime.Samples.ArcGISVectorTiledLayerUrl
{
    [Register("ArcGISVectorTiledLayerUrl")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "ArcGIS vector tiled layer (URL)",
        "Layers",
        "This sample demonstrates how to create a ArcGISVectorTiledLayer and bind this to a Basemap which is used in the creation of a map.",
        "")]
    public class ArcGISVectorTiledLayerUrl : UIViewController
    {
        // Create the UI controls
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _toolbar = new UIToolbar();
        private readonly UIButton _button = new UIButton();

        private readonly Dictionary<string, Uri> _layerUrls = new Dictionary<string, Uri>()
        {
            {"Mid-Century", new Uri("http://www.arcgis.com/home/item.html?id=7675d44bb1e4428aa2c30a9b68f97822")},
            {"Colored Pencil", new Uri("http://www.arcgis.com/home/item.html?id=4cf7e1fb9f254dcda9c8fbadb15cf0f8")},
            {"Newspaper", new Uri("http://www.arcgis.com/home/item.html?id=dfb04de5f3144a80bc3f9f336228d24a")},
            {"Nova", new Uri("http://www.arcgis.com/home/item.html?id=75f4dfdff19e445395653121a95a85db")},
            {"World Street Map (Night)", new Uri("http://www.arcgis.com/home/item.html?id=86f556a2d1fd468181855a35e344567f")}
        };

        public ArcGISVectorTiledLayerUrl()
        {
            Title = "ArcGIS vector tiled layer (URL)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create a new Map instance with the basemap
            Map myMap = new Map(SpatialReferences.WebMercator);

            // Create a new ArcGISVectorTiledLayer
            ArcGISVectorTiledLayer vectorTiledLayer = new ArcGISVectorTiledLayer(_layerUrls.Values.First());

            // Create and use a new basemap
            myMap.Basemap = new Basemap(vectorTiledLayer);

            // Assign the Map to the MapView
            _myMapView.Map = myMap;
        }

        private void LayerSelectionButtonClick(object sender, EventArgs e)
        {
            // Create the view controller that will present the list of layers
            UIAlertController layerSelectionAlert = UIAlertController.Create("Select a vector layer", "", UIAlertControllerStyle.ActionSheet);

            // Add an option for each layer
            foreach (string item in _layerUrls.Keys)
            {
                // Selecting the layer will call the ChooseLayer function
                layerSelectionAlert.AddAction(UIAlertAction.Create(item, UIAlertActionStyle.Default, action => ChooseLayer(item)));
            }

            // Show the alert
            PresentViewController(layerSelectionAlert, true, null);
        }

        private void ChooseLayer(string layer)
        {
            // Get the layer based on the selection
            ArcGISVectorTiledLayer vectorTiledLayer = new ArcGISVectorTiledLayer(_layerUrls[layer]);

            // Apply the layer
            _myMapView.Map = new Map(new Basemap(vectorTiledLayer));
        }

        private void CreateLayout()
        {
            // Update the button parameters
            _button.SetTitle("Choose Layer", UIControlState.Normal);
            _button.SetTitleColor(UIColor.Blue, UIControlState.Normal);

            // Allow the user to select new layers
            _button.TouchUpInside += LayerSelectionButtonClick;

            // Add views
            View.AddSubviews(_myMapView, _toolbar, _button);
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frames for the views 
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            _toolbar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 50, View.Bounds.Width, 50);
            _button.Frame = new CoreGraphics.CGRect(10, _toolbar.Frame.Top + 10, View.Bounds.Width - 20, 30);

            base.ViewDidLayoutSubviews();
        }
    }
}