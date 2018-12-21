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
using CoreGraphics;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.RasterRenderingRule
{
    [Register("RasterRenderingRule")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Raster rendering rule",
        "Layers",
        "This sample demonstrates how to create an `ImageServiceRaster`, fetch the `RenderingRule`s from the service info, and use a `RenderingRule` to create an `ImageServiceRaster` and add it to a raster layer.",
        "")]
    public class RasterRenderingRule : UIViewController
    {
        // Hold references to the UI controls.
        private MapView _myMapView = new MapView();
        private UILabel _selectionLabel;

        // Hold a reference to a read-only list for the various rendering rules of the image service raster.
        private IReadOnlyList<RenderingRuleInfo> _renderRuleInfos;

        // URL for the image server.
        private readonly Uri _myUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/CharlotteLAS/ImageServer");

        public RasterRenderingRule()
        {
            Title = "Raster rendering rule";
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            // Set up the map with basemap.
            _myMapView.Map = new Map(Basemap.CreateTopographic());

            // Load the rendering rules for the raster.
            await LoadRenderingRules();
            
            // Apply the first rendering rule
            SelectRenderingRule(_renderRuleInfos[0]);
        }

        private async Task LoadRenderingRules()
        {
            // Create a new image service raster from the Uri.
            ImageServiceRaster imageServiceRaster = new ImageServiceRaster(_myUri);

            try
            {
                // Load the image service raster.
                await imageServiceRaster.LoadAsync();

                // Get the ArcGIS image service info (metadata) from the image service raster.
                ArcGISImageServiceInfo arcGISImageServiceInfo = imageServiceRaster.ServiceInfo;

                // Get the full extent envelope of the image service raster (the Charlotte, NC area).
                Envelope myEnvelope = arcGISImageServiceInfo.FullExtent;

                // Define a new view point from the full extent envelope.
                Viewpoint viewPoint = new Viewpoint(myEnvelope);

                // Zoom to the area of the full extent envelope of the image service raster.
                await _myMapView.SetViewpointAsync(viewPoint);

                // Get the rendering rule info (i.e. definitions of how the image should be drawn) info from the image service raster.
                _renderRuleInfos = arcGISImageServiceInfo.RenderingRuleInfos;
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private void SelectRenderingRule(RenderingRuleInfo renderingRuleInfo)
        {
            // Create a new rendering rule from the rendering rule info.
            RenderingRule renderingRule = new RenderingRule(renderingRuleInfo);
            
            // Create a new image service raster.
            ImageServiceRaster imageServiceRaster = new ImageServiceRaster(_myUri)
            {
                // Set the image service raster's rendering rule to the rendering rule created earlier.
                RenderingRule = renderingRule
            };

            // Create a new raster layer from the image service raster.
            RasterLayer rasterLayer = new RasterLayer(imageServiceRaster);
            
            // Clear the existing layer from the map.
            _myMapView.Map.OperationalLayers.Clear();

            // Add the raster layer to the operational layers of the  map view.
            _myMapView.Map.OperationalLayers.Add(rasterLayer);
            
            // Update the label.
            _selectionLabel.Text = $"Rule \"{renderingRuleInfo.Name}\" selected.";
        }

        private void ChangeRenderingRule_Clicked(object sender, EventArgs e)
        {
            // Create the alert controller with a title.
            UIAlertController alertController = UIAlertController.Create("Choose a rendering rule", "", UIAlertControllerStyle.Alert);
            
            // Add actions for each rendering rule.
            foreach (RenderingRuleInfo ruleInfo in _renderRuleInfos)
            {
                alertController.AddAction(UIAlertAction.Create(ruleInfo.Name, UIAlertActionStyle.Default, action => SelectRenderingRule(ruleInfo)));
            }
            
            // Show the alert.
            PresentViewController(alertController, true, null);
        }

        public override void LoadView()
        {
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            
            _selectionLabel = new UILabel
            {
                Text = "No rendering rule selected.",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            
            View.AddSubviews(_myMapView, _selectionLabel, toolbar);

            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem("Change rendering rule", UIBarButtonItemStyle.Plain, ChangeRenderingRule_Clicked),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) 
            };
            
            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor).Active = true;

            toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor).Active = true;
            toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            
            _selectionLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _selectionLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _selectionLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _selectionLabel.HeightAnchor.ConstraintEqualTo(40).Active = true;
        }
    }
}