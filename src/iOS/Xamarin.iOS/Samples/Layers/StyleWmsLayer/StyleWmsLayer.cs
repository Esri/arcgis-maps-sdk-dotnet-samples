// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using CoreGraphics;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.StyleWmsLayer
{
    [Register("StyleWmsLayer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Style WMS layers",
        "Layers",
        "This sample demonstrates how to select from the available styles on WMS sublayers. ",
        "Click to select from one of the two preset styles.")]
    public class StyleWmsLayer : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _buttonContainer = new UIToolbar();

        private readonly UIButton _firstStyleButton = new UIButton
        {
            Enabled = false,
            HorizontalAlignment = UIControlContentHorizontalAlignment.Center
        };

        private readonly UIButton _secondStyleButton = new UIButton
        {
            Enabled = false,
            HorizontalAlignment = UIControlContentHorizontalAlignment.Center
        };

        private readonly UILabel _helpLabel = new UILabel
        {
            Text = "Choose a style:",
            TextAlignment = UITextAlignment.Center,
            TextColor = UIColor.Black
        };

        // Hold the URL to the service, which has satellite imagery covering the state of Minnesota.
        private readonly Uri _wmsUrl = new Uri("http://geoint.lmic.state.mn.us/cgi-bin/wms?VERSION=1.3.0&SERVICE=WMS&REQUEST=GetCapabilities");

        // Hold a list of uniquely-identifying WMS layer names to display.
        private readonly List<string> _wmsLayerNames = new List<string> {"fsa2017"};

        // Hold a reference to the layer to enable re-styling.
        private WmsLayer _mnWmsLayer;

        public StyleWmsLayer()
        {
            Title = "Style WMS layers";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references.
            CreateLayout();

            // Initialize the map.
            InitializeAsync();
        }

        private void CreateLayout()
        {
            // Add the mapview to the view.
            View.AddSubviews(_myMapView, _buttonContainer, _firstStyleButton, _secondStyleButton, _helpLabel);

            // Update the button text.
            _firstStyleButton.SetTitle("Style 1", UIControlState.Normal);
            _secondStyleButton.SetTitle("Style 2", UIControlState.Normal);

            // Update the colors.
            _firstStyleButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _secondStyleButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            // Subscribe to the button click events.
            _firstStyleButton.TouchUpInside += FirstStyleButton_Clicked;
            _secondStyleButton.TouchUpInside += SecondStyleButton_Clicked;
        }

        private async void InitializeAsync()
        {
            try
            {
                // Apply an imagery basemap to the map.
                Map myMap = new Map(Basemap.CreateImagery());

                // Create a new WMS layer displaying the specified layers from the service.
                // The default styles are chosen by default, which corresponds to 'Style 1' in the UI.
                _mnWmsLayer = new WmsLayer(_wmsUrl, _wmsLayerNames);

                // Wait for the layer to load.
                await _mnWmsLayer.LoadAsync();

                // Center the map on the layer's contents.
                myMap.InitialViewpoint = new Viewpoint(_mnWmsLayer.FullExtent);

                // Add the layer to the map.
                myMap.OperationalLayers.Add(_mnWmsLayer);

                // Add the map to the view.
                _myMapView.Map = myMap;

                // Enable the buttons.
                _firstStyleButton.Enabled = true;
                _secondStyleButton.Enabled = true;
            }
            catch (Exception ex)
            {
                // Any exceptions in the async void method must be caught, otherwise they will result in a crash.
                Debug.WriteLine(ex.ToString());
            }
        }

        private void FirstStyleButton_Clicked(object sender, EventArgs e)
        {
            // Get the available styles from the first sublayer.
            IReadOnlyList<string> styles = _mnWmsLayer.Sublayers[0].SublayerInfo.Styles;

            // Apply the first style to the first sublayer.
            _mnWmsLayer.Sublayers[0].CurrentStyle = styles[0];
        }

        private void SecondStyleButton_Clicked(object sender, EventArgs e)
        {
            // Get the available styles from the first sublayer.
            IReadOnlyList<string> styles = _mnWmsLayer.Sublayers[0].SublayerInfo.Styles;

            // Apply the second style to the first sublayer.
            _mnWmsLayer.Sublayers[0].CurrentStyle = styles[1];
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat controlHeight = 30;
                nfloat margin = 5;
                nfloat toolbarHeight = controlHeight * 2 + margin * 3;

                // Reposition the views.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);
                _buttonContainer.Frame = new CGRect(0, View.Bounds.Height - toolbarHeight, View.Bounds.Width, toolbarHeight);
                _helpLabel.Frame = new CGRect(margin, View.Bounds.Height - 2 * controlHeight - 2 * margin, View.Bounds.Width - 2 * margin, controlHeight);
                _firstStyleButton.Frame = new CGRect(margin, View.Bounds.Height - controlHeight - margin, View.Bounds.Width / 2 - margin, controlHeight);
                _secondStyleButton.Frame = new CGRect(View.Bounds.Width / 2 + margin, View.Bounds.Height - controlHeight - margin, View.Bounds.Width / 2 - 2 * margin, controlHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }
    }
}