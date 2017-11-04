// Copyright 2017 Esri.
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
using System.Collections.Generic;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.KmlNetworkLinks
{
    public partial class KmlNetworkLinks : ContentPage
    {
        // Member (aka global) variable to hold a reference to the KML dataset
        private KmlDataset _myKmlDataset;

        public KmlNetworkLinks()
        {
            InitializeComponent();

            Title = "KML network links";

            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Set up the map with a basemap
                MyMap.Basemap = Basemap.CreateDarkGrayCanvasVector();

                // Get the service Url
                Uri serviceUri = new Uri("http://radar.vlieghinder.nl?networkstart=1169241299");

                // Create a KML Dataset with the Url
                _myKmlDataset = new KmlDataset(serviceUri);

                // Create a KML layer from the KmlDataset
                KmlLayer myKmlLayer = new KmlLayer(_myKmlDataset);

                // Subscribe to status changes
                myKmlLayer.LoadStatusChanged += KmlLayer_LoadStatusChanged;

                // Add the KML layer to the operational layers
                MyMap.OperationalLayers.Add(myKmlLayer);

                // Define and extent to zoom to (in the case: Frankfurt Germany area)
                Envelope myExtent = new Envelope(647571.091263907, 6310478.90141415, 1350638.62905514, 6604034.01050928, SpatialReferences.WebMercator);

                // Zoom to the extent
                await MyMapView.SetViewpointGeometryAsync(myExtent);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Sample error", "An error occurred" + ex.ToString(), "OK");
            }
        }

        private void KmlLayer_LoadStatusChanged(object sender, Esri.ArcGISRuntime.LoadStatusEventArgs e)
        {
            if (e.Status == Esri.ArcGISRuntime.LoadStatus.Loaded)
            {
                // This makes sure that we switch to the UI thread before interacting with any UI components
                Device.BeginInvokeOnMainThread(() =>
                 {
                     // Clear the existing label
                     MyNetworkLinkLabel.Text = "";

                     // Traverse the file and display the refresh intervals for network links
                     TraverseNodesUpdateStatus(_myKmlDataset.RootNodes);
                 });
            }
        }

        private void TraverseNodesUpdateStatus(IReadOnlyList<KmlNode> nodes)
        {
            // Iterate through each node
            foreach (KmlNode content in nodes)
            {
                // Update the UI if the content is a Network Link
                if (content is KmlNetworkLink)
                {
                    // Get a reference to the item as a KmlNetworkLink
                    KmlNetworkLink netLink = (KmlNetworkLink)content;

                    // Get the existing text of the label
                    string existingLabel = MyNetworkLinkLabel.Text as string;

                    // Update the text with information about the layer
                    existingLabel += String.Format("Name: {0} - Refresh Interval: {1}" +
                        Environment.NewLine, netLink.Name, netLink.RefreshInterval);

                    // Update the label with the new text
                    MyNetworkLinkLabel.Text = existingLabel;
                }

                // Recur if the node type has children
                if (content is KmlContainer)
                {
                    // Cast the node to the correct type
                    KmlContainer myKmlNetworkLink = (KmlContainer)content;

                    // Recur on the children of the node
                    TraverseNodesUpdateStatus(myKmlNetworkLink.ChildNodes);
                }
                // Note: This sample does not recursively explore KmlNetworkLinks. Due to how KmlNetworkLinks are loaded,
                //        reliably exploring their children is an advanced topic
                // Note: recursion ends when there are no more nodes to visit
            }
        }
    }
}