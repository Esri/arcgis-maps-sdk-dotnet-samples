// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.GeoViewSync
{
    [Register("GeoViewSync")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "GeoView viewpoint synchronization",
        "MapView",
        "This sample demonstrates how to keep two geo views (MapView/SceneView) in sync with each other.",
        "")]
    public class GeoViewSync : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private SceneView _mySceneView;
        private UIStackView _stackView;

        public GeoViewSync()
        {
            Title = "GeoView viewpoint synchronization";
        }

        private void Initialize()
        {
            // Initialize the MapView and SceneView with basemaps.
            _myMapView.Map = new Map(Basemap.CreateImageryWithLabels());
            _mySceneView.Scene = new Scene(Basemap.CreateImageryWithLabels());

            // Disable 'flick' gesture - this is the most straightforward way to prevent the 'flick'
            //     animation on one view from competing with user interaction on the other.
            _mySceneView.InteractionOptions = new SceneViewInteractionOptions {IsFlickEnabled = false};
            _myMapView.InteractionOptions = new MapViewInteractionOptions {IsFlickEnabled = false};
        }

        private void OnNavigationComplete(object sender, EventArgs eventArgs)
        {
            // Get a reference to the MapView or SceneView that raised the event.
            GeoView sendingView = (GeoView) sender;

            // Get a reference to the other view.
            GeoView otherView;
            if (sendingView is MapView)
            {
                otherView = _mySceneView;
            }
            else
            {
                otherView = _myMapView;
            }

            // Update the viewpoint on the other view.
            otherView.SetViewpoint(sendingView.GetCurrentViewpoint(ViewpointType.CenterAndScale));
        }

        private void OnViewpointChanged(object sender, EventArgs e)
        {
            // Get the MapView or SceneView that sent the event.
            GeoView sendingView = (GeoView) sender;

            // Only take action if this GeoView is the one that the user is navigating.
            // Viewpoint changed events are fired when SetViewpoint is called; This check prevents a feedback loop.
            if (sendingView.IsNavigating)
            {
                // If the MapView sent the event, update the SceneView's viewpoint.
                if (sender is MapView)
                {
                    // Get the viewpoint.
                    Viewpoint updateViewpoint = _myMapView.GetCurrentViewpoint(ViewpointType.CenterAndScale);

                    // Set the viewpoint.
                    _mySceneView.SetViewpoint(updateViewpoint);
                }
                else // Else, update the MapView's viewpoint.
                {
                    // Get the viewpoint.
                    Viewpoint updateViewpoint = _mySceneView.GetCurrentViewpoint(ViewpointType.CenterAndScale);

                    // Set the viewpoint.
                    _myMapView.SetViewpoint(updateViewpoint);
                }
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
            View = new UIView();

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            _stackView = new UIStackView(new UIView[] {_myMapView, _mySceneView});
            _stackView.TranslatesAutoresizingMaskIntoConstraints = false;
            _stackView.Axis = UILayoutConstraintAxis.Vertical;
            _stackView.Distribution = UIStackViewDistribution.FillEqually;

            // Add the views.
            View.AddSubviews(_stackView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _stackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _stackView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _stackView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _stackView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
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

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to viewpoint change events for both views - event raised on click+drag.
            _myMapView.ViewpointChanged += OnViewpointChanged;
            _mySceneView.ViewpointChanged += OnViewpointChanged;

            // Subscribe to the navigation completed events - raised on flick.
            _myMapView.NavigationCompleted += OnNavigationComplete;
            _mySceneView.NavigationCompleted += OnNavigationComplete;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.ViewpointChanged -= OnViewpointChanged;
            _mySceneView.ViewpointChanged -= OnViewpointChanged;
            _myMapView.NavigationCompleted -= OnNavigationComplete;
            _mySceneView.NavigationCompleted -= OnNavigationComplete;
        }
    }
}