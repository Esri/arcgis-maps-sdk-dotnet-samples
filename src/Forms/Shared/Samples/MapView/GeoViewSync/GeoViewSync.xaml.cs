// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Xamarin.Forms;
using System;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.GeoViewSync
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "GeoView viewpoint synchronization",
        "MapView",
        "This sample demonstrates how to keep two geo views (MapView/SceneView) in sync with each other.",
        "")]
    public partial class GeoViewSync : ContentPage
    {
        public GeoViewSync()
        {
            InitializeComponent();

            Title = "GeoView viewpoint synchronization";

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Initialize the MapView and SceneView with a basemap
            MyMapView.Map = new Map(Basemap.CreateImageryWithLabels());
            MySceneView.Scene = new Scene(Basemap.CreateImageryWithLabels());

            // Disable 'flick' gesture - this is the most straightforward way to prevent the 'flick'
            //     animation on one view from competing with user interaction on the other
            MySceneView.InteractionOptions = new SceneViewInteractionOptions { IsFlickEnabled = false };
            MyMapView.InteractionOptions = new MapViewInteractionOptions { IsFlickEnabled = false };

            // Subscribe to viewpoint change events for both views - event raised on click+drag
            MyMapView.ViewpointChanged += OnViewpointChanged;
            MySceneView.ViewpointChanged += OnViewpointChanged;
            
            // Subscribe to the navigation completed events - raised on flick
            MyMapView.NavigationCompleted += OnNavigationComplete;
            MySceneView.NavigationCompleted += OnNavigationComplete;
        }

        private void OnNavigationComplete(object sender, EventArgs eventArgs)
        {
            // Get a reference to the MapView or SceneView that raised the event
            GeoView sendingView = (GeoView)sender;

            // Get a reference to the other view
            GeoView otherView;
            if (sendingView is MapView)
            {
                otherView = MySceneView;
            }
            else
            {
                otherView = MyMapView;
            }

            // Update the viewpoint on the other view
            otherView.SetViewpoint(sendingView.GetCurrentViewpoint(ViewpointType.CenterAndScale));
        }

        private void OnViewpointChanged(object sender, EventArgs e)
        {
            // Get the MapView or SceneView that sent the event
            GeoView sendingView = sender as GeoView;

            // Only take action if this geoview is the one that the user is navigating.
            // Viewpoint changed events are fired when SetViewpoint is called; This check prevents a feedback loop
            if (sendingView.IsNavigating)
            {
                // If the MapView sent the event, update the SceneView's viewpoint
                if (sender is MapView)
                {
                    // Get the viewpoint
                    Viewpoint updateViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.CenterAndScale);

                    // Set the viewpoint
                    MySceneView.SetViewpoint(updateViewpoint);
                }
                else // Else, update the MapView's viewpoint
                {
                    // Get the viewpoint
                    Viewpoint updateViewpoint = MySceneView.GetCurrentViewpoint(ViewpointType.CenterAndScale);

                    // Set the viewpoint
                    MyMapView.SetViewpoint(updateViewpoint);
                }
            }
        }
    }
}