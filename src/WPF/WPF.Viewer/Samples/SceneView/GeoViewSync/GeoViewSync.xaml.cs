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
using Esri.ArcGISRuntime.UI.Controls;
using System;

namespace ArcGIS.WPF.Samples.GeoViewSync
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "GeoView viewpoint synchronization",
        category: "SceneView",
        description: "Keep the view points of two views (e.g. MapView and SceneView) synchronized with each other.",
        instructions: "Interact with the MapView or SceneView by zooming or panning. The other MapView or SceneView will automatically focus on the same viewpoint.",
        tags: new[] { "3D", "automatic refresh", "event", "event handler", "events", "extent", "interaction", "interactions", "pan", "sync", "synchronize", "zoom" })]
    public partial class GeoViewSync
    {
        public GeoViewSync()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Initialize the MapView and SceneView with a basemap
            MyMapView.Map = new Map(BasemapStyle.ArcGISImagery);
            MySceneView.Scene = new Scene(BasemapStyle.ArcGISImagery);

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
            GeoView sendingView = (GeoView)sender;

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