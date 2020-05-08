// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ArcGISVectorTiledLayerUrl
{
    [Register("ArcGISVectorTiledLayerUrl")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "ArcGIS vector tiled layer URL",
        category: "Layers",
        description: "Load an ArcGIS Vector Tiled Layer from a URL.",
        instructions: "Use the drop down menu to load different vector tile basemaps.",
        tags: new[] { "tiles", "vector", "vector basemap", "vector tiled layer", "vector tiles" })]
    public class ArcGISVectorTiledLayerUrl : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _chooseLayerButton;

        // Dictionary maps layer names to URLs.
        private readonly Dictionary<string, Uri> _layerUrls = new Dictionary<string, Uri>
        {
            {"Mid-Century", new Uri("https://www.arcgis.com/home/item.html?id=7675d44bb1e4428aa2c30a9b68f97822")},
            {"Colored Pencil", new Uri("https://www.arcgis.com/home/item.html?id=4cf7e1fb9f254dcda9c8fbadb15cf0f8")},
            {"Newspaper", new Uri("https://www.arcgis.com/home/item.html?id=dfb04de5f3144a80bc3f9f336228d24a")},
            {"Nova", new Uri("https://www.arcgis.com/home/item.html?id=75f4dfdff19e445395653121a95a85db")},
            {"World Street Map (Night)", new Uri("https://www.arcgis.com/home/item.html?id=86f556a2d1fd468181855a35e344567f")}
        };

        public ArcGISVectorTiledLayerUrl()
        {
            Title = "ArcGIS vector tiled layer (URL)";
        }

        private void Initialize()
        {
            Map myMap = new Map(SpatialReferences.WebMercator);

            // Create a new ArcGISVectorTiledLayer.
            ArcGISVectorTiledLayer vectorTiledLayer = new ArcGISVectorTiledLayer(_layerUrls.Values.First());

            // Create and use a new basemap.
            myMap.Basemap = new Basemap(vectorTiledLayer);

            // Assign the Map to the MapView.
            _myMapView.Map = myMap;
        }

        private void LayerSelectionButtonClick(object sender, EventArgs e)
        {
            // Create the view controller that will present the list of layers.
            UIAlertController layerSelectionAlert = UIAlertController.Create("Select a vector layer", "", UIAlertControllerStyle.ActionSheet);

            // Add an option for each layer.
            foreach (string item in _layerUrls.Keys)
            {
                // Selecting the layer will call the ChooseLayer function.
                layerSelectionAlert.AddAction(UIAlertAction.Create(item, UIAlertActionStyle.Default, action => ChooseLayer(item)));
            }

            // Fix to prevent crash on iPad.
            var popoverPresentationController = layerSelectionAlert.PopoverPresentationController;
            if (popoverPresentationController != null)
            {
                popoverPresentationController.BarButtonItem = (UIBarButtonItem) sender;
            }

            // Show the alert.
            PresentViewController(layerSelectionAlert, true, null);
        }

        private void ChooseLayer(string layer)
        {
            // Get the layer based on the selection.
            ArcGISVectorTiledLayer vectorTiledLayer = new ArcGISVectorTiledLayer(_layerUrls[layer]);

            // Apply the layer.
            _myMapView.Map = new Map(new Basemap(vectorTiledLayer));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _chooseLayerButton = new UIBarButtonItem();
            _chooseLayerButton.Title = "Choose a layer";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _chooseLayerButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _chooseLayerButton.Clicked += LayerSelectionButtonClick;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _chooseLayerButton.Clicked -= LayerSelectionButtonClick;
        }
    }
}