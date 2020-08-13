// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Popups;
using Esri.ArcGISRuntime.Toolkit.UI.Controls;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Linq;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ShowPopup
{
    [Register("ShowPopup")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Show popup",
        category: "Layers",
        description: "Show predefined popups from a web map.",
        instructions: "Tap on the features to prompt a popup that displays information about the feature.",
        tags: new[] { "feature", "feature layer", "popup", "toolkit", "web map" })]
    public class ShowPopup : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private PopupViewer _popupViewer;
        private UIStackView _stackView;
        private UILabel _instructionsLabel;

        public ShowPopup()
        {
            Title = "Show popup";
        }

        private void Initialize()
        {
            // Load the map.
            _myMapView.Map = new Map(new Uri("https://arcgisruntime.maps.arcgis.com/home/item.html?id=fb788308ea2e4d8682b9c05ef641f273"));
        }

        private async void MapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Get the feature layer from the map.
                FeatureLayer incidentLayer = _myMapView.Map.OperationalLayers.First() as FeatureLayer;

                // Identify the tapped on feature.
                IdentifyLayerResult result = await _myMapView.IdentifyLayerAsync(incidentLayer, e.Position, 12, true);

                if (result?.Popups?.FirstOrDefault() is Popup popup)
                {
                    // Remove the instructions label.
                    _stackView.RemoveArrangedSubview(_instructionsLabel);
                    _instructionsLabel.RemoveFromSuperview();
                    _stackView.AddArrangedSubview(_popupViewer);

                    // Create a new popup manager for the popup.
                    _popupViewer.PopupManager = new PopupManager(popup);

                    QueryParameters queryParams = new QueryParameters
                    {
                        // Set the geometry to selection envelope for selection by geometry.
                        Geometry = new Envelope((MapPoint)popup.GeoElement.Geometry, 6, 6)
                    };

                    // Select the features based on query parameters defined above.
                    await incidentLayer.SelectFeaturesAsync(queryParams, SelectionMode.New);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = ApplicationTheme.BackgroundColor };

            _stackView = new UIStackView { TranslatesAutoresizingMaskIntoConstraints = false, Spacing = 8, Distribution = UIStackViewDistribution.FillEqually };
            if (View.TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                _stackView.Axis = UILayoutConstraintAxis.Horizontal;
            }
            else
            {
                _stackView.Axis = UILayoutConstraintAxis.Vertical;
            }

            _myMapView = new MapView { TranslatesAutoresizingMaskIntoConstraints = false };
            _popupViewer = new PopupViewer { TranslatesAutoresizingMaskIntoConstraints = false };
            _instructionsLabel = new UILabel { TranslatesAutoresizingMaskIntoConstraints = false, Text = "Tap a feature to display its popup." };

            _stackView.AddArrangedSubview(_myMapView);
            _stackView.AddArrangedSubview(_instructionsLabel);

            // Add the views.
            View.AddSubviews(_stackView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]{
                _stackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _stackView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                _stackView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                _stackView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
            });
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);
            if (View.TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                _stackView.Axis = UILayoutConstraintAxis.Horizontal;
            }
            else
            {
                _stackView.Axis = UILayoutConstraintAxis.Vertical;
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.GeoViewTapped += MapViewTapped;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= MapViewTapped;
        }
    }
}