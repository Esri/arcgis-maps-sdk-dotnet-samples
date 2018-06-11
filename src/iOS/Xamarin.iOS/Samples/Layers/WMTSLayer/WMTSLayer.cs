// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
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
        // Constant holding offset where the MapView control should start
        private const int yPageOffset = 60;

        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Create button
        private UIButton _uriButton;

        // Create button
        private UIButton _infoButton;

        public WMTSLayer()
        {
            Title = "WMTS layer";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references
            CreateLayout();

            // Load the map using Uri to the WMTS service.
            OnUriButtonClicked(null, null);
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Setup the visual frame for button1
            _uriButton.Frame = new CoreGraphics.CGRect(0, yPageOffset, View.Bounds.Width, 40);

            // Setup the visual frame for button2
            _infoButton.Frame = new CoreGraphics.CGRect(0, yPageOffset + 40, View.Bounds.Width, 40);

            base.ViewDidLayoutSubviews();
        }

        private void OnUriButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Define the Uri to the WMTS service (NOTE: iOS applications require the use of Uri's to be https:// and not http://)
                var myUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/WorldTimeZones/MapServer/WMTS");

                // Create a new instance of a WMTS layer using a Uri and provide an Id value
                WmtsLayer myWmtsLayer = new WmtsLayer(myUri, "WorldTimeZones");

                // Create a new map
                Map myMap = new Map();

                // Get the basemap from the map
                Basemap myBasemap = myMap.Basemap;

                // Get the layer collection for the base layers
                LayerCollection myLayerCollection = myBasemap.BaseLayers;

                // Add the WMTS layer to the layer collection of the map
                myLayerCollection.Add(myWmtsLayer);

                // Set the scale so that the map is visible on iOS devices.
                _myMapView.SetViewpointScaleAsync(300000000);

                // Assign the map to the MapView
                _myMapView.Map = myMap;

                // Disable and enable the appropriate buttons.
                _uriButton.Enabled = false;
                _infoButton.Enabled = true;

                
                
            }
            catch (Exception ex)
            {
                // Report error
                UIAlertController alert = UIAlertController.Create("Error", ex.Message, UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
        }

        private async void OnInfoButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Define the Uri to the WMTS service (NOTE: iOS applications require the use of Uri's to be https:// and not http://)
                var myUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/WorldTimeZones/MapServer/WMTS");

                // Define a new instance of the WMTS service
                WmtsService myWmtsService = new WmtsService(myUri);

                // Load the WMTS service 
                await myWmtsService.LoadAsync();

                // Get the service information (i.e. metadata) about the WMTS service
                WmtsServiceInfo myWMTSServiceInfo = myWmtsService.ServiceInfo;

                // Obtain the read only list of WMTS layer info objects
                IReadOnlyList<WmtsLayerInfo> myWmtsLayerInfos = myWMTSServiceInfo.LayerInfos;

                // Create a new instance of a WMTS layer using the first item in the read only list of WMTS layer info objects
                WmtsLayer myWmtsLayer = new WmtsLayer(myWmtsLayerInfos[0]);

                // Create a new map
                Map myMap = new Map();

                // Get the basemap from the map
                Basemap myBasemap = myMap.Basemap;

                // Get the layer collection for the base layers
                LayerCollection myLayerCollection = myBasemap.BaseLayers;

                // Add the WMTS layer to the layer collection of the map
                myLayerCollection.Add(myWmtsLayer);

                // Set the scale so that the map is visible on iOS devices.
                await _myMapView.SetViewpointScaleAsync(300000000);

                // Assign the map to the MapView
                _myMapView.Map = myMap;

                // Disable and enable the appropriate buttons.
                _uriButton.Enabled = true;
                _infoButton.Enabled = false;
            }
            catch (Exception ex)
            {
                // Report error
                UIAlertController alert = UIAlertController.Create("Error", ex.Message, UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
        }

        private void CreateLayout()
        {

            // Create a button for Uri
            _uriButton = new UIButton();
            _uriButton.SetTitle("WMTSLayer via Uri", UIControlState.Normal);
            _uriButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _uriButton.BackgroundColor = UIColor.White;

            // Hook to touch event to do button1
            _uriButton.TouchUpInside += OnUriButtonClicked;

            // Create a button for WmtsLayerInfo
            _infoButton = new UIButton();
            _infoButton.SetTitle("WMTSLayer via WmtsLayerInfo", UIControlState.Normal);
            _infoButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _infoButton.BackgroundColor = UIColor.White;

            // Hook to touch event to do button2
            _infoButton.TouchUpInside += OnInfoButtonClicked;

            // Add MapView to the page
            View.AddSubviews(_myMapView, _uriButton, _infoButton);
        }
    }
}