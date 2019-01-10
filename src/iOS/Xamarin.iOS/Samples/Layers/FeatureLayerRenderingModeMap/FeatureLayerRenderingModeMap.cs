// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.FeatureLayerRenderingModeMap
{
    [Register("FeatureLayerRenderingModeMap")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer rendering mode (Map)",
        "Layers",
        "This sample demonstrates how to use load settings to set preferred rendering mode for feature layers, specifically static or dynamic rendering modes.",
        "")]
    public class FeatureLayerRenderingModeMap : UIViewController
    {
        // Hold references to the UI controls.
        private MapView _staticMapView;
        private MapView _dynamicMapView;
        private UIStackView _stackView;

        // Hold references to the two views.
        private Viewpoint _zoomOutPoint;
        private Viewpoint _zoomInPoint;

        public FeatureLayerRenderingModeMap()
        {
            Title = "Feature layer rendering mode (Map)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        private async void Initialize()
        {
            // Viewpoint locations for map view to zoom in and out to.
            _zoomOutPoint = new Viewpoint(new MapPoint(-118.37, 34.46, SpatialReferences.Wgs84), 650000, 0);
            _zoomInPoint = new Viewpoint(new MapPoint(-118.45, 34.395, SpatialReferences.Wgs84), 50000, 90);

            // Configure the maps.
            _dynamicMapView.Map = new Map();
            _staticMapView.Map = new Map();

            // Create service feature table using a point, polyline, and polygon service.
            ServiceFeatureTable pointServiceFeatureTable = new ServiceFeatureTable(new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/0"));
            ServiceFeatureTable polylineServiceFeatureTable = new ServiceFeatureTable(new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/8"));
            ServiceFeatureTable polygonServiceFeatureTable = new ServiceFeatureTable(new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/9"));

            // Create feature layers from service feature tables.
            List<FeatureLayer> featureLayers = new List<FeatureLayer>
            {
                new FeatureLayer(polygonServiceFeatureTable),
                new FeatureLayer(polylineServiceFeatureTable),
                new FeatureLayer(pointServiceFeatureTable)
            };

            // Add each layer to the map as a static layer and a dynamic layer.
            foreach (FeatureLayer layer in featureLayers)
            {
                // Add the static layer to the top map view.
                layer.RenderingMode = FeatureRenderingMode.Static;
                _staticMapView.Map.OperationalLayers.Add(layer);

                // Add the dynamic layer to the bottom map view.
                FeatureLayer dynamicLayer = (FeatureLayer) layer.Clone();
                dynamicLayer.RenderingMode = FeatureRenderingMode.Dynamic;
                _dynamicMapView.Map.OperationalLayers.Add(dynamicLayer);
            }

            try
            {
                // Set the view point of both MapViews.
                await _staticMapView.SetViewpointAsync(_zoomOutPoint);
                await _dynamicMapView.SetViewpointAsync(_zoomOutPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async void OnZoomClick(object sender, EventArgs e)
        {
            try
            {
                // Initiate task to zoom both map views in.  
                Task t1 = _staticMapView.SetViewpointAsync(_zoomInPoint, TimeSpan.FromSeconds(5));
                Task t2 = _dynamicMapView.SetViewpointAsync(_zoomInPoint, TimeSpan.FromSeconds(5));
                await Task.WhenAll(t1, t2);

                // Delay start of next set of zoom tasks.
                await Task.Delay(2000);

                // Initiate task to zoom both map views out. 
                Task t3 = _staticMapView.SetViewpointAsync(_zoomOutPoint, TimeSpan.FromSeconds(5));
                Task t4 = _dynamicMapView.SetViewpointAsync(_zoomOutPoint, TimeSpan.FromSeconds(5));
                await Task.WhenAll(t3, t4);
            }
            catch (Exception ex)
            {
                new UIAlertView("Error", ex.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        public override void LoadView()
        {
            View = new UIView {BackgroundColor = UIColor.White};

            _staticMapView = new MapView();
            _staticMapView.TranslatesAutoresizingMaskIntoConstraints = false;
            _dynamicMapView = new MapView();
            _dynamicMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _stackView = new UIStackView(new UIView[] {_staticMapView, _dynamicMapView});
            _stackView.TranslatesAutoresizingMaskIntoConstraints = false;
            _stackView.Distribution = UIStackViewDistribution.FillEqually;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            UILabel staticLabel = new UILabel
            {
                Text = "Static",
                BackgroundColor = UIColor.FromWhiteAlpha(0f, .6f),
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            UILabel dynamicLabel = new UILabel()
            {
                Text = "Dynamic",
                BackgroundColor = UIColor.FromWhiteAlpha(0f, .6f),
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            View.AddSubviews(_stackView, toolbar, staticLabel, dynamicLabel);

            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem("Zoom", UIBarButtonItemStyle.Plain, OnZoomClick),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _stackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _stackView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),
                _stackView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _stackView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                staticLabel.TopAnchor.ConstraintEqualTo(_staticMapView.TopAnchor),
                staticLabel.HeightAnchor.ConstraintEqualTo(40),
                staticLabel.LeadingAnchor.ConstraintEqualTo(_staticMapView.LeadingAnchor),
                staticLabel.TrailingAnchor.ConstraintEqualTo(_staticMapView.TrailingAnchor),
                dynamicLabel.TopAnchor.ConstraintEqualTo(_dynamicMapView.TopAnchor),
                dynamicLabel.HeightAnchor.ConstraintEqualTo(40),
                dynamicLabel.LeadingAnchor.ConstraintEqualTo(_dynamicMapView.LeadingAnchor),
                dynamicLabel.TrailingAnchor.ConstraintEqualTo(_dynamicMapView.TrailingAnchor)
            });
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);
            if (View.TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                _stackView.Axis = UILayoutConstraintAxis.Horizontal;
            }
            else
            {
                _stackView.Axis = UILayoutConstraintAxis.Vertical;
            }
        }
    }
}