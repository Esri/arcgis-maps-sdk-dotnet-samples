// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using ArcGISRuntime.Samples.Managers;
using CoreGraphics;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
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
        // Hold references to the UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _changeRendererButton;

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
            string shapefilePath = DataManager.GetDataFolder("d98b3e5293834c5f852f13c569930caa", "Subdivisions.shp");

            // Create a shapefile feature table from the shapefile path.
            ShapefileFeatureTable featureTable = new ShapefileFeatureTable(shapefilePath);

            // Create a layer from the feature table.
            _shapefileFeatureLayer = new FeatureLayer(featureTable);

            // Add the layer to the map.
            myMap.OperationalLayers.Add(_shapefileFeatureLayer);

            // Create the symbology for the alternate renderer.
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 1.0);
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.Yellow, lineSymbol);

            // Create the alternate renderer.
            _alternateRenderer = new SimpleRenderer(fillSymbol);

            try
            {
                // Wait for the layer to load so that it will be assigned a default renderer.
                await _shapefileFeatureLayer.LoadAsync();

                // Hold a reference to the default renderer (to enable switching between the renderers).
                _defaultRenderer = _shapefileFeatureLayer.Renderer;

                // Add the map to the mapview.
                _myMapView.Map = myMap;

                // Enable changing symbology now that sample is loaded.
                _changeRendererButton.Enabled = true;
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
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
            base.ViewDidLoad();
            Initialize();
        }
        
        public override void LoadView()
        {
            View = new UIView {BackgroundColor = UIColor.White};
            
            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;
            
            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            
            View.AddSubviews(_myMapView, toolbar);

            _changeRendererButton =
                new UIBarButtonItem("Change renderer", UIBarButtonItemStyle.Plain, Button_Clicked);
            _changeRendererButton.Enabled = false;

            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _changeRendererButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };
            
            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor).Active = true;

            toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor).Active = true;
            toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }
    }
}