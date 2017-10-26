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
using Esri.ArcGISRuntime.Portal;
using System;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace ArcGISRuntime.UWP.Samples.KmlNetworkLinks
{
    public partial class KmlNetworkLinks
    {
        // Member (aka global) variable to hold a reference to the KML dataset
        private KmlDataset _myKmlDataset;

        public KmlNetworkLinks()
        {
            InitializeComponent();

            // Initialize the map
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Set up the map with a basemap
                MyMap.Basemap = Basemap.CreateDarkGrayCanvasVector();

                // Create the ArcGIS Portal
                ArcGISPortal myPortal = await ArcGISPortal.CreateAsync();

                // Create the portal item with the known ID
                PortalItem item = await PortalItem.CreateAsync(myPortal, "5d56deb77c0d424799a522d8a13f079e");

                // Get the service Url
                Uri serviceUri = item.ServiceUrl;

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
                var message = new MessageDialog("An error occurred" + ex.ToString(), "Sample error");
                await message.ShowAsync();
            }
        }

        private async void KmlLayer_LoadStatusChanged(object sender, Esri.ArcGISRuntime.LoadStatusEventArgs e)
        {
            if (e.Status == Esri.ArcGISRuntime.LoadStatus.Loaded)
            {
                // This makes sure that we switch to the UI thread before interacting with any UI components
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // Clear the existing text block
                    MyNetworkLinkTextBlock.Text = "";

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
                // Get the type of the node
                Type nodeType = content.GetType();

                // Update the UI if the content is a Network Link
                if (nodeType == typeof(KmlNetworkLink))
                {
                    // Get a reference to the item as a KmlNetworkLink
                    KmlNetworkLink netLink = content as KmlNetworkLink;

                    // Get the existing text of the label
                    string existingLabel = MyNetworkLinkTextBlock.Text as string;

                    // Update the text with information about the layer
                    existingLabel += String.Format("Name: {0} - Refresh Interval: {1}" +
                        Environment.NewLine, netLink.Name, netLink.RefreshInterval);

                    // Update the label with the new text
                    MyNetworkLinkTextBlock.Text = existingLabel;
                }

                // Recur if the node type has children
                if (nodeType == typeof(KmlNetworkLink))
                {
                    // Cast the node to the correct type
                    KmlNetworkLink myKmlNetworkLink = (KmlNetworkLink)content;

                    // Recur on the children of the node
                    TraverseNodesUpdateStatus(myKmlNetworkLink.ChildNodes);
                }
                else if (nodeType == typeof(KmlContainer))
                {
                    // Cast the node to the correct type
                    KmlContainer myKmlNetworkLink = (KmlContainer)content;

                    // Recur on the children of the node
                    TraverseNodesUpdateStatus(myKmlNetworkLink.ChildNodes);
                }
                else if (nodeType == typeof(KmlFolder))
                {
                    // Cast the node to the correct type
                    KmlFolder myKmlNetworkLink = (KmlFolder)content;

                    // Recur on the children of the node
                    TraverseNodesUpdateStatus(myKmlNetworkLink.ChildNodes);
                }
                else if (nodeType == typeof(KmlDocument))
                {
                    // Cast the node to the correct type
                    KmlDocument myKmlNetworkLink = (KmlDocument)content;

                    // Recur on the children of the node
                    TraverseNodesUpdateStatus(myKmlNetworkLink.ChildNodes);
                }
                // Note: recursion ends when there are no more nodes to visit
            }
        }
    }
}