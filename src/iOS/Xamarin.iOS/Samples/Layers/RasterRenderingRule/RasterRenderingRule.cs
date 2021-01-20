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
        name: "Raster rendering rule",
        category: "Layers",
        description: "Display a raster on a map and apply different rendering rules to that raster.",
        instructions: "Run the sample and use the drop-down menu at the top to select a rendering rule.",
        tags: new[] { "raster", "rendering rules", "visualization" })]
    public class RasterRenderingRule : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UILabel _selectionLabel;
        private UIBarButtonItem _changeRuleButton;

        // Hold a reference to a read-only list for the various rendering rules of the image service raster.
        private IReadOnlyList<RenderingRuleInfo> _renderRuleInfos;

        // URL for the image server.
        private readonly Uri _myUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/CharlotteLAS/ImageServer");

        public RasterRenderingRule()
        {
            Title = "Raster rendering rule";
        }

        private async void Initialize()
        {
            // Set up the map with basemap.
            _myMapView.Map = new Map(BasemapStyle.ArcGISTopographic);

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

            _changeRuleButton = new UIBarButtonItem();
            _changeRuleButton.Title = "Change rendering rule";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _changeRuleButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

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

            // Add the views.
            View.AddSubviews(_myMapView, _selectionLabel, toolbar);

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

                _selectionLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _selectionLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _selectionLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _selectionLabel.HeightAnchor.ConstraintEqualTo(40)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _changeRuleButton.Clicked += ChangeRenderingRule_Clicked;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _changeRuleButton.Clicked -= ChangeRenderingRule_Clicked;
        }
    }
}