// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using CoreGraphics;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UIKit;

namespace ArcGISRuntime.Samples.StyleWmsLayer
{
    [Register("StyleWmsLayer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Style WMS layers",
        "Layers",
        "This sample demonstrates how to select from the available styles on WMS sublayers. ",
        "Click to select from one of the two pre-set styles.")]
    public class StyleWmsLayer : UIViewController
    {
        // Hold the URL to the service, which has satellite imagery covering the state of Minnesota.
        private Uri _wmsUrl = new Uri("http://geoint.lmic.state.mn.us/cgi-bin/wms?VERSION=1.3.0&SERVICE=WMS&REQUEST=GetCapabilities");

        // Hold a list of uniquely-identifying WMS layer names to display.
        private List<String> _wmsLayerNames = new List<string> { "fsa2017" };

        // Hold a reference to the layer to enable re-styling.
        private WmsLayer _mnWmsLayer;

        // Hold references to the views.
        private MapView _myMapView = new MapView();
        private UIButton _firstStyleButton = new UIButton
        {
            Enabled = false,
            HorizontalAlignment = UIControlContentHorizontalAlignment.Center
        };
        private UIButton _secondStyleButton = new UIButton
        {
            Enabled = false,
            HorizontalAlignment = UIControlContentHorizontalAlignment.Center
        };
        private UILabel _helpLabel = new UILabel
        {
            Text = "Choose a style",
            TextAlignment = UITextAlignment.Center,
            TextColor = UIColor.Black
        };
        private UIToolbar _buttonContainer = new UIToolbar();

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
            // Calculate the top margin.
            nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

            // Setup the visual frame for the MapView.
            _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Update the insets for the map view (to ensure attribution bar is visible, among other reasons).
            _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, 50, 0);

            // Update the toolbar and button positions.
            _buttonContainer.Frame = new CGRect(0, View.Bounds.Height - 50, View.Bounds.Width, 50);
            _firstStyleButton.Frame = new CGRect(10, View.Bounds.Height - 40, View.Bounds.Width / 2 - 10, 30);
            _secondStyleButton.Frame = new CGRect(View.Bounds.Width / 2 + 10, View.Bounds.Height - 40, View.Bounds.Width / 2 - 10, 30);

            // Update the help label position.
            _helpLabel.Frame = new CGRect(0, View.Bounds.Height - 90, View.Bounds.Width, 20);

            base.ViewDidLayoutSubviews();
        }
    }
}