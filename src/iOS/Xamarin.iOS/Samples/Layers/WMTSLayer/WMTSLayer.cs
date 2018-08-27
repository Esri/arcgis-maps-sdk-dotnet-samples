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
        // Hold references to the UI controls.
        private MapView _myMapView;
        private UISegmentedControl _constructorChoiceButton;

        public WMTSLayer()
        {
            Title = "WMTS layer";
        }

        public override void LoadView()
        {
            base.LoadView();

            // Create the views.
            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;
            _constructorChoiceButton = new UISegmentedControl("URI", "Service Info")
            {
                BackgroundColor = UIColor.FromWhiteAlpha(0, .7f),
                TintColor = UIColor.White,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            // Clean up borders of segmented control - avoid corner pixels.
            _constructorChoiceButton.ClipsToBounds = true;
            _constructorChoiceButton.Layer.CornerRadius = 5;

            _constructorChoiceButton.ValueChanged += _constructorChoiceButton_ValueChanged;; ;

            // Add the views.
            View.AddSubviews(_myMapView, _constructorChoiceButton);

            // Apply constraints.
            _myMapView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;

            _constructorChoiceButton.LeadingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.LeadingAnchor).Active = true;
            _constructorChoiceButton.TrailingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.TrailingAnchor).Active = true;
            _constructorChoiceButton.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8).Active = true;
        }

        public async override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _constructorChoiceButton.SelectedSegment = 0;
            await LoadWMTSLayerAsync(true);
        }

        private async void _constructorChoiceButton_ValueChanged(object sender, EventArgs e)
        {
            //Load the WMTS layer using service info or URL.
            switch(_constructorChoiceButton.SelectedSegment) {
                case 0:
                    await LoadWMTSLayerAsync(true);
                    break;
                case 1:
                    await LoadWMTSLayerAsync(false);
                    break;
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
    }
}