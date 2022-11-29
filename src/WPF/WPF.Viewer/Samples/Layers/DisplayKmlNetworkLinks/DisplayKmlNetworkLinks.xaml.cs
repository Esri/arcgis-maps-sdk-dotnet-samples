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

namespace ArcGIS.WPF.Samples.DisplayKmlNetworkLinks
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display KML network links",
        category: "Layers",
        description: "Display a file with a KML network link, including displaying any network link control messages at launch.",
        instructions: "The sample will load the KML file automatically. The data shown should refresh automatically every few seconds. Pan and zoom to explore the map.",
        tags: new[] { "KML", "KMZ", "Keyhole", "Network Link", "Network Link Control", "OGC" })]
    public partial class DisplayKmlNetworkLinks
    {
        public DisplayKmlNetworkLinks()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Set up the basemap.
            MySceneView.Scene = new Scene(BasemapStyle.ArcGISImagery);

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
            MySceneView.SetViewpoint(new Viewpoint(new MapPoint(8.150526, 50.472421, SpatialReferences.Wgs84), 20000000));
        }

        private void Dataset_NetworkLinkControlMessage(object sender, KmlNetworkLinkControlMessageEventArgs e)
        {
            MessageBox.Show(e.Message, "KML layer message");
        }
    }
}