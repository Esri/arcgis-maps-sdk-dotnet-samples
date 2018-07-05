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
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.WMTSLayer
{
    [Register("WMTSLayer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "WMTS layer",
        "Layers",
        "This sample demonstrates how to display a WMTS layer on a map via a Uri and WmtsLayerInfo.",
        "")]
    public class WMTSLayer : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _toolbar = new UIToolbar();

        private readonly UILabel _label = new UILabel
        {
            Text = "Construct layer with:",
            TextAlignment = UITextAlignment.Center
        };

        private readonly UIButton _uriButton = new UIButton(UIButtonType.RoundedRect)
        {
            BackgroundColor = UIColor.FromWhiteAlpha(1, .8f),
            Layer = {CornerRadius = 5}
        };

        private readonly UIButton _infoButton = new UIButton(UIButtonType.RoundedRect)
        {
            BackgroundColor = UIColor.FromWhiteAlpha(1, .8f),
            Layer = {CornerRadius = 5}
        };

        public WMTSLayer()
        {
            Title = "WMTS layer";
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references.
            CreateLayout();

            // Load the map using Uri to the WMTS service.
            await LoadWMTSLayerAsync(true);
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat controlHeight = 30;
                nfloat margin = 5;
                nfloat toolbarHeight = controlHeight * 2 + margin * 3;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);
                _toolbar.Frame = new CGRect(0, View.Bounds.Height - 2 * controlHeight - 3 * margin, View.Bounds.Width, 2 * controlHeight + 3 * margin);
                _label.Frame = new CGRect(margin, View.Bounds.Height - 2 * controlHeight - 2 * margin, View.Bounds.Width - 2 * margin, controlHeight);
                _uriButton.Frame = new CGRect(margin, View.Bounds.Height - controlHeight - margin, View.Bounds.Width / 2 - 2 * margin, controlHeight);
                _infoButton.Frame = new CGRect(View.Bounds.Width / 2 + margin, View.Bounds.Height - controlHeight - margin, View.Bounds.Width / 2 - 2 * margin, controlHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private async void UriButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                //Load the WMTS layer using Uri method.
                await LoadWMTSLayerAsync(true);

                // Disable and enable the appropriate buttons.
                _uriButton.Enabled = false;
                _infoButton.Enabled = true;
            }
            catch (Exception ex)
            {
                // Report error.
                UIAlertController alert = UIAlertController.Create("Error", ex.Message, UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
        }

        private async void InfoButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                //Load the WMTS layer using layer info.
                await LoadWMTSLayerAsync(false);

                // Disable and enable the appropriate buttons.
                _uriButton.Enabled = true;
                _infoButton.Enabled = false;
            }
            catch (Exception ex)
            {
                // Report error.
                UIAlertController alert = UIAlertController.Create("Error", ex.Message, UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
        }

        private async Task LoadWMTSLayerAsync(bool uriMode)
        {
            try
            {
                // Create a new map.
                Map myMap = new Map();

                // Get the basemap from the map.
                Basemap myBasemap = myMap.Basemap;

                // Get the layer collection for the base layers.
                LayerCollection myLayerCollection = myBasemap.BaseLayers;

                // Create an instance for the WMTS layer.
                WmtsLayer myWmtsLayer;

                // Define the Uri to the WMTS service.
                Uri wmtsUri = new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/WorldTimeZones/MapServer/WMTS");

                if (uriMode)
                {
                    // Create a WMTS layer using a Uri and provide an Id value.
                    myWmtsLayer = new WmtsLayer(wmtsUri, "WorldTimeZones");
                }
                else
                {
                    // Define a new instance of the WMTS service.
                    WmtsService myWmtsService = new WmtsService(wmtsUri);

                    // Load the WMTS service.
                    await myWmtsService.LoadAsync();

                    // Get the service information (i.e. metadata) about the WMTS service.
                    WmtsServiceInfo myWmtsServiceInfo = myWmtsService.ServiceInfo;

                    // Obtain the read only list of WMTS layer info objects.
                    IReadOnlyList<WmtsLayerInfo> myWmtsLayerInfos = myWmtsServiceInfo.LayerInfos;

                    // Create a WMTS layer using the first item in the read only list of WMTS layer info objects.
                    myWmtsLayer = new WmtsLayer(myWmtsLayerInfos[0]);
                }

                // Add the WMTS layer to the layer collection of the map.
                myLayerCollection.Add(myWmtsLayer);

                // Assign the map to the MapView.
                _myMapView.Map = myMap;

                // Zoom to appropriate level for iOS.
                await _myMapView.SetViewpointScaleAsync(300000000);
            }
            catch (Exception ex)
            {
                UIAlertController alert = UIAlertController.Create("Error", ex.Message, UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
        }

        private void CreateLayout()
        {
            // Create a button for Uri
            _uriButton.SetTitle("URL", UIControlState.Normal);
            _uriButton.SetTitleColor(UIColor.Gray, UIControlState.Disabled);
            _uriButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _uriButton.Enabled = false;

            // Hook to touch event to do button1
            _uriButton.TouchUpInside += UriButton_Clicked;

            // Create a button for WmtsLayerInfo
            _infoButton.SetTitle("WmtsLayerInfo", UIControlState.Normal);
            _infoButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _infoButton.SetTitleColor(UIColor.Gray, UIControlState.Disabled);

            // Hook to touch event to do button2
            _infoButton.TouchUpInside += InfoButton_Clicked;

            // Add controls to the page.
            View.AddSubviews(_myMapView, _toolbar, _label, _uriButton, _infoButton);
        }
    }
}