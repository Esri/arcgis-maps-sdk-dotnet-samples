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
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _controlToolbar = new UIToolbar();
        private readonly UISegmentedControl _rulePicker = new UISegmentedControl();

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

            // Assign the Map to the MapView
            _myMapView.Map = new Map(SpatialReferences.WebMercator)
            {
                Basemap = Basemap.CreateTopographic()
            };

            // Make the text for the buttons in the UISegmentedControl small to display the names of the rendering rules.
            UIFont myUiFont = UIFont.SystemFontOfSize(7);
            _rulePicker.SetTitleTextAttributes(new UITextAttributes {Font = myUiFont}, UIControlState.Normal);
            _rulePicker.ApportionsSegmentWidthsByContent = true;

            // Wire-up the UISegmentedControl's value change event handler.
            _rulePicker.ValueChanged += _segmentControl_ValueChanged;

            // Add the map view and toolbar to the view.
            View.AddSubviews(_myMapView, _controlToolbar, _rulePicker);

            // Load of the rendering rules of the image service raster and display their names on the buttons in the toolbar.
            await LoadRenderingRules();
        }

        private async Task LoadRenderingRules()
        {
            // Create a new image service raster from the Uri.
            ImageServiceRaster imageServiceRaster = new ImageServiceRaster(_myUri);

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

            // Define an index counter to be used by the UISegmentedControl.
            int counter = 0;

            // Loop through each rendering rule info.
            foreach (RenderingRuleInfo renderingRuleInfo in _renderRuleInfos)
            {
                // Get the name of the rendering rule info.
                string renderingRuleName = renderingRuleInfo.Name;

                // Add the rendering rule info name to the UISegmentedControl.
                _rulePicker.InsertSegment(renderingRuleName, counter, false);

                // Increment the counter for adding segments into the UISegmentedControl.
                counter++;
            }
        }

        private void _segmentControl_ValueChanged(object sender, EventArgs e)
        {
            // Get the index number of the user choice of render rule names.
            nint selectedSegmentId = (sender as UISegmentedControl).SelectedSegment;

            // Get the rendering rule info name from the UISegmentedControl that was chosen by the user.
            string selectedRuleName = (sender as UISegmentedControl).TitleAt(selectedSegmentId);

            // Loop through each rendering rule info in the image service raster.
            foreach (RenderingRuleInfo renderingRuleInfo in _renderRuleInfos)
            {
                // Get the name of the rendering rule info.
                string renderingRuleName = renderingRuleInfo.Name;

                // If the name of the rendering rule info matches what was chosen by the user, proceed.
                if (renderingRuleName == selectedRuleName)
                {
                    // Create a new rendering rule from the rendering rule info.
                    RenderingRule renderingRule = new RenderingRule(renderingRuleInfo);

                    // Create a new image service raster.
                    ImageServiceRaster imageServiceRaster2 = new ImageServiceRaster(_myUri)
                    {
                        // Set the image service raster's rendering rule to the rendering rule created earlier.
                        RenderingRule = renderingRule
                    };

                    // Create a new raster layer from the image service raster.
                    RasterLayer rasterLayer = new RasterLayer(imageServiceRaster2);

                    // Add the raster layer to the operational layers of the  map view.
                    _myMapView.Map.OperationalLayers.Add(rasterLayer);

                    // Stop iterating once match is found.
                    break;
                }
            }
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat controlHeight = 30;
                nfloat margin = 5;
                nfloat toolbarHeight = controlHeight + 2 * margin;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);
                _controlToolbar.Frame = new CGRect(0, View.Bounds.Height - toolbarHeight, View.Bounds.Width, toolbarHeight);
                _rulePicker.Frame = new CGRect(margin, _controlToolbar.Frame.Top + margin, View.Bounds.Width - 2 * margin, controlHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }
    }
}