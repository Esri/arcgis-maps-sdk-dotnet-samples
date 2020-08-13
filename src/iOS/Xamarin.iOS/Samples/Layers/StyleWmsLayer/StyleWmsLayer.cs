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
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.StyleWmsLayer
{
    [Register("StyleWmsLayer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Style WMS layers",
        category: "Layers",
        description: "Change the style of a Web Map Service (WMS) layer.",
        instructions: "Once the layer loads, the toggle button will be enabled. Tap it to toggle between the first and second styles of the WMS layer.",
        tags: new[] { "WMS", "imagery", "styles", "visualization" })]
    public class StyleWmsLayer : UIViewController
    {
        // Hold references to UI controls.
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

        private async void Initialize()
        {
            try
            {
                // Create a map with spatial reference appropriate for the service.
                Map myMap = new Map(SpatialReference.Create(26915)) {MinScale = 7000000.0};

                // Create a new WMS layer displaying the specified layers from the service.
                // The default styles are chosen by default.
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
                new UIAlertView("Error", ex.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _styleChoiceButton = new UISegmentedControl("Default", "Contrast stretch")
            {
                BackgroundColor = ApplicationTheme.BackgroundColor,
                TintColor = ApplicationTheme.ForegroundColor,
                Enabled = false,
                TranslatesAutoresizingMaskIntoConstraints = false,
                SelectedSegment = 0,
                // Clean up borders of segmented control - avoid corner pixels.
                ClipsToBounds = true,
                Layer = {CornerRadius = 5}
            };

            // Add the views.
            View.AddSubviews(_myMapView, _styleChoiceButton);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),

                _styleChoiceButton.LeadingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.LeadingAnchor),
                _styleChoiceButton.TrailingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.TrailingAnchor),
                _styleChoiceButton.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _styleChoiceButton.ValueChanged += _styleChoiceButton_ValueChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _styleChoiceButton.ValueChanged -= _styleChoiceButton_ValueChanged;
        }
    }
}