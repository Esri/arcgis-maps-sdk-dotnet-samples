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
        name: "Symbolize shapefile",
        category: "Data",
        description: "Display a shapefile with custom symbology.",
        instructions: "Tap the button to apply a new symbology renderer to the feature layer created from the shapefile. ",
        tags: new[] { "package", "shape file", "shapefile", "symbology", "visualization" })]
    public class SymbolizeShapefile : UIViewController
    {
        // Hold references to UI controls.
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

        private void ChangeRenderer_Clicked(object sender, System.EventArgs e)
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
            // Create the views.
            View = new UIView {BackgroundColor = ApplicationTheme.BackgroundColor};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _changeRendererButton = new UIBarButtonItem();
            _changeRendererButton.Title = "Change renderer";
            _changeRendererButton.Enabled = false;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _changeRendererButton,
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
            _changeRendererButton.Clicked += ChangeRenderer_Clicked;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _changeRendererButton.Clicked -= ChangeRenderer_Clicked;
        }
    }
}