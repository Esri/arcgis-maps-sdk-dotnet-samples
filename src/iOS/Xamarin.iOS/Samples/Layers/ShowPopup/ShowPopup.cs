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
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class ShowPopup : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private PopupViewer _popupViewer;

        public ShowPopup()
        {
            Title = "Show popup";
        }

        private void Initialize()
        {
            // Load the map.
            _myMapView.Map = new Map(new Uri("https://runtime.maps.arcgis.com/home/webmap/viewer.html?webmap=e4c6eb667e6c43b896691f10cc2f1580"));
        }

        private async void MapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Get the feature layer from the map.
                FeatureLayer incidentLayer = _myMapView.Map.OperationalLayers.First() as FeatureLayer;

                // Identify the tapped on feature.
                IdentifyLayerResult result = await _myMapView.IdentifyLayerAsync(incidentLayer, e.Position, 12, true);

                if (result != null && result.Popups.Any())
                {
                    // Get the first popup from the identify result.
                    Popup popup = result.Popups.First();

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

            _myMapView = new MapView() { TranslatesAutoresizingMaskIntoConstraints = false };
            _popupViewer = new PopupViewer() { TranslatesAutoresizingMaskIntoConstraints = false };

            // Add the views.
            View.AddSubviews(_myMapView, _popupViewer);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]{
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(_popupViewer.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                _popupViewer.TopAnchor.ConstraintEqualTo(_myMapView.BottomAnchor),
                _popupViewer.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _popupViewer.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _popupViewer.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
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