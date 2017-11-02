// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;
using Esri.ArcGISRuntime.Xamarin.Forms;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.GeoViewSync
{
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

            // Subscribe to viewpoint change events for both views
            MyMapView.ViewpointChanged += MySceneView_ViewpointChanged;
            MySceneView.ViewpointChanged += MySceneView_ViewpointChanged;
        }

        private void MySceneView_ViewpointChanged(object sender, EventArgs e)
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