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
        // Hold references to the UI controls.
        private MapView _myMapView;
        private UISegmentedControl _styleChoiceButton;

        // Hold the URL to the service, which has satellite imagery covering the state of Minnesota.
        private readonly Uri _wmsUrl = new Uri("https://imageserver.gisdata.mn.gov/cgi-bin/mncomp?SERVICE=WMS&VERSION=1.3.0&REQUEST=GetCapabilities");

        // Hold a list of uniquely-identifying WMS layer names to display.
        private readonly List<string> _wmsLayerNames = new List<string> {"mncomp"};

        // Hold a reference to the layer to enable re-styling.
        private WmsLayer _mnWmsLayer;

        public StyleWmsLayer()
        {
            Title = "Style WMS layers";
        }

        public override void LoadView()
        {
            // Create the views.
            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _styleChoiceButton = new UISegmentedControl("Default", "Contrast stretch")
            {
                BackgroundColor = UIColor.FromWhiteAlpha(0, .7f),
                TintColor = UIColor.White,
                Enabled = false,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Clean up borders of segmented control - avoid corner pixels.
            _styleChoiceButton.ClipsToBounds = true;
            _styleChoiceButton.Layer.CornerRadius = 5;

            _styleChoiceButton.ValueChanged += _styleChoiceButton_ValueChanged;

            // Add the views.
            View = new UIView();
            View.AddSubviews(_myMapView, _styleChoiceButton);

            // Apply constraints.
            _myMapView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;

            _styleChoiceButton.LeadingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.LeadingAnchor).Active = true;
            _styleChoiceButton.TrailingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.TrailingAnchor).Active = true;
            _styleChoiceButton.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8).Active = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        private async void Initialize()
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

                // Enable the UI.
                _styleChoiceButton.Enabled = true;
            }
            catch (Exception ex)
            {
                // Any exceptions in the async void method must be caught, otherwise they will result in a crash.
                Debug.WriteLine(ex.ToString());
            }
        }

        private void _styleChoiceButton_ValueChanged(object sender, EventArgs e)
        {
            int styleSelection = (int) _styleChoiceButton.SelectedSegment;

            try
            {
                // Get the available styles from the first sublayer.
                IReadOnlyList<string> styles = _mnWmsLayer.Sublayers[0].SublayerInfo.Styles;

                // Apply the second style to the first sublayer.
                _mnWmsLayer.Sublayers[0].CurrentStyle = styles[styleSelection];
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