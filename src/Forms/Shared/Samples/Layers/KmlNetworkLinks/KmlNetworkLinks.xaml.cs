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
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.KmlNetworkLinks
{
	public partial class KmlNetworkLinks : ContentPage
	{
        // Member (aka global) variable to hold a reference to the KML dataset
        KmlDataset _myKmlDataset;

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

                // First clear the operational layers
                MyMap.OperationalLayers.Clear();

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

                // Ensure the KML layer is loaded
                await myKmlLayer.LoadAsync();

                // Add the KML layer to the operational layers
                MyMap.OperationalLayers.Add(myKmlLayer);

                // Define and extent to zoom to (in the case: Frankfurt Germany area)
                Envelope myExtent = new Envelope(647571.091263907, 6310478.90141415, 1350638.62905514, 6604034.01050928, SpatialReferences.WebMercator);

                // Zoom to the extent
                await MyMapView.SetViewpointGeometryAsync(myExtent);

                // Traverse the file and display the refresh intervals for network links
                TraverseNodesUpdateStatus(_myKmlDataset.RootNodes);
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

        private void TraverseNodesUpdateStatus(IReadOnlyList<KmlNode> sublayers)
        {
            // Iterate through each sublayer content
            foreach (KmlNode content in sublayers)
            {
                // Update the UI if the content is a Network Link
                if (typeof(KmlNetworkLink) == content.GetType())
                {
                    // Get a reference to the item as a KmlNetworkLink
                    KmlNetworkLink netLink = content as KmlNetworkLink;

                    // Get the existing text of the label
                    string existingTextBlock = MyNetworkLinkLabel.Text as string;

                    // Update the text with information about the layer
                    existingTextBlock += String.Format("Name: {0} - Refresh Interval: {1}" +
                        Environment.NewLine, netLink.Name, netLink.RefreshInterval);

                    // Update the label with the new text
                    MyNetworkLinkLabel.Text = existingTextBlock;
                }

                // Update the UI if the content is a Network Link
                if (typeof(KmlNetworkLink) == content.GetType())
                {
                    KmlNetworkLink myKmlNetworkLink = (KmlNetworkLink)content;
                    // Recurse on the contents of this sublayer
                    TraverseNodesUpdateStatus(myKmlNetworkLink.ChildNodes);
                }

                // Note: recursion ends when there are no more sublayer contents to visit
            }
        }
    }
}