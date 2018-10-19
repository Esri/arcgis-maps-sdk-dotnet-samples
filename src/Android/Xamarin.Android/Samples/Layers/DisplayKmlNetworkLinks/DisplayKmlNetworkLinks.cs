// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using System;

namespace ArcGISRuntimeXamarin.Samples.DisplayKmlNetworkLinks
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display KML network links",
        "Layers",
        "Display a KML file that loads content from a network resource.",
        "")]
    public class DisplayKmlNetworkLinks : Activity
    {
        // Create and hold a reference to the SceneView.
        private readonly SceneView _mySceneView = new SceneView();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Display KML network links";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Set up the basemap.
            _mySceneView.Scene = new Scene(Basemap.CreateImageryWithLabels());

            // Create the dataset.
            KmlDataset dataset = new KmlDataset(new Uri("https://www.arcgis.com/sharing/rest/content/items/600748d4464442288f6db8a4ba27dc95/data"));

            // Listen for network link control messages.
            // These should be shown to the user.
            dataset.NetworkLinkControlMessage += Dataset_NetworkLinkControlMessage;

            // Create the layer from the dataset.
            KmlLayer fileLayer = new KmlLayer(dataset);

            // Add the layer to the map.
            _mySceneView.Scene.OperationalLayers.Add(fileLayer);

            // Zoom in to center the map on Germany.
            await _mySceneView.SetViewpointAsync(new Viewpoint(new MapPoint(8.150526, 50.472421, SpatialReferences.Wgs84), 20000000));
        }

        private void Dataset_NetworkLinkControlMessage(object sender, KmlNetworkLinkControlMessageEventArgs e)
        {
            // Due to the nature of the threading implementation,
            //     the dispatcher needs to be used to interact with the UI.
            // The dispatcher takes an Action, provided here as a lambda function.
            RunOnUiThread(() =>
            {
                // Display the message to the user.
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetMessage(e.Message).SetTitle("KML layer message").Show();
            });
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) {Orientation = Orientation.Vertical};

            // Add the scene view to the layout.
            layout.AddView(_mySceneView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}