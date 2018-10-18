// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using System;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.DisplayKmlNetworkLinks
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display KML network links",
        "Layers",
        "Display a KML file that loads content from a network resource.",
        "")]
    public partial class DisplayKmlNetworkLinks
    {
        public DisplayKmlNetworkLinks()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Set up the basemap.
            MySceneView.Scene = new Scene(Basemap.CreateImageryWithLabels());

            // Create the dataset.
            KmlDataset dataset = new KmlDataset(new Uri("https://www.arcgis.com/sharing/rest/content/items/600748d4464442288f6db8a4ba27dc95/data"));

            // Listen for network link control messages.
            // These should be shown to the user.
            dataset.NetworkLinkControlMessage += Dataset_NetworkLinkControlMessage;

            // Create the layer from the dataset.
            KmlLayer fileLayer = new KmlLayer(dataset);

            // Add the layer to the map.
            MySceneView.Scene.OperationalLayers.Add(fileLayer);

            // Zoom in to center the map on Germany.
            await MySceneView.SetViewpointAsync(new Viewpoint(new MapPoint(8.150526, 50.472421, SpatialReferences.Wgs84), 20000000));
        }

        private void Dataset_NetworkLinkControlMessage(object sender, KmlNetworkLinkControlMessageEventArgs e)
        {
            MessageBox.Show(e.Message, "KML layer message");
        }
    }
}