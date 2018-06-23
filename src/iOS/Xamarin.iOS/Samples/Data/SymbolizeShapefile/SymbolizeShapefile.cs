// Copyright 2017 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using ArcGISRuntime.Samples.Managers;
using CoreGraphics;
using UIKit;

namespace ArcGISRuntime.Samples.SymbolizeShapefile
{
    [Register("SymbolizeShapefile")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("d98b3e5293834c5f852f13c569930caa")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Symbolize a shapefile",
        "Data",
        "This sample demonstrates how to apply a custom renderer to a shapefile displayed by a feature layer.",
        "Click the button to switch renderers. ")]
    public class SymbolizeShapefile : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _toolbar = new UIToolbar();

        private readonly UIButton _myRendererButton = new UIButton
        {
            Enabled = false
        };

        // Hold reference to the feature layer so that its renderer can be changed when button is pushed.
        private FeatureLayer _shapefileFeatureLayer;

        // Hold reference to default renderer to enable switching back.
        private Renderer _defaultRenderer;

        // Hold reference to alternate renderer to enable switching.
        private SimpleRenderer _alternateRenderer;

        public SymbolizeShapefile()
        {
            Title = "Symbolize a shapefile";
        }

        private async void Initialize()
        {
            // Create the point for the map's initial viewpoint.
            MapPoint point = new MapPoint(-11662054, 4818336, SpatialReference.Create(3857));

            // Create and show a map with topographic basemap.
            Map myMap = new Map(Basemap.CreateTopographic())
            {
                InitialViewpoint = new Viewpoint(point, 200000)
            };

            // Get the path to the shapefile.
            string shapefilepath = DataManager.GetDataFolder("d98b3e5293834c5f852f13c569930caa", "Subdivisions.shp");

            // Create a shapefile feature table from the shapefile path.
            ShapefileFeatureTable featureTable = new ShapefileFeatureTable(shapefilepath);

            // Create a layer from the feature table.
            _shapefileFeatureLayer = new FeatureLayer(featureTable);

            // Wait for the layer to load.
            await _shapefileFeatureLayer.LoadAsync();

            // Add the layer to the map.
            myMap.OperationalLayers.Add(_shapefileFeatureLayer);

            // Create the symbology for the alternate renderer.
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 1.0);
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.Yellow, lineSymbol);

            // Create the alternate renderer.
            _alternateRenderer = new SimpleRenderer(fillSymbol);

            // Hold a reference to the default renderer (to enable switching between the renderers).
            _defaultRenderer = _shapefileFeatureLayer.Renderer;

            // Add the map to the mapview.
            _myMapView.Map = myMap;

            // Enable changing symbology now that sample is loaded.
            _myRendererButton.Enabled = true;
        }

        private void CreateLayout()
        {
            // Configure the renderer button.
            _myRendererButton.SetTitle("Change renderer", UIControlState.Normal);
            _myRendererButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            // Subscribe to button press events.
            _myRendererButton.TouchUpInside += Button_Clicked;

            // Add views to the page.
            View.AddSubviews(_myMapView, _toolbar, _myRendererButton);
        }

        private void Button_Clicked(object sender, System.EventArgs e)
        {
            // Toggle the renderer.
            if (_shapefileFeatureLayer.Renderer == _defaultRenderer)
            {
                _shapefileFeatureLayer.Renderer = _alternateRenderer;
            }
            else
            {
                _shapefileFeatureLayer.Renderer = _defaultRenderer;
            }
        }

        public override void ViewDidLoad()
        {
            CreateLayout();
            Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat toolbarHeight = 40;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);
                _toolbar.Frame = new CGRect(0, View.Bounds.Height - 40, View.Bounds.Width, 40);
                _myRendererButton.Frame = new CGRect(0, View.Bounds.Height - 35, View.Bounds.Width, 30);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }
    }
}