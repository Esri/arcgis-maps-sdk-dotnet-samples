// Copyright 2017 Esri.
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
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;

namespace ArcGISRuntimeXamarin.Samples.KmlNetworkLinks
{
    [Activity]
    public class KmlNetworkLinks : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Create a new instance of the map
        private Map _myMap = new Map();

        // Create a text view to hold the network link names and refresh intervals
        private TextView _myLabel;

        // Member (aka global) variable to hold a reference to the KML dataset
        private KmlDataset _myKmlDataset;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "KML network links";

            // Create the UI, setup the control references
            CreateLayout();

            // Initialize the sample
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Set the map view's map property
                _myMapView.Map = _myMap;

                // Set up the map with a basemap
                _myMap.Basemap = Basemap.CreateDarkGrayCanvasVector();

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
                _myMap.OperationalLayers.Add(myKmlLayer);

                // Define and extent to zoom to (in the case: Frankfurt Germany area)
                Envelope myExtent = new Envelope(647571.091263907, 6310478.90141415, 1350638.62905514, 6604034.01050928, SpatialReferences.WebMercator);

                // Zoom to the extent
                await _myMapView.SetViewpointGeometryAsync(myExtent);
            }
            catch (Exception ex)
            {
                var builder = new AlertDialog.Builder(this);
                builder.SetMessage("An error occurred" + ex.ToString()).SetTitle("Sample error").Show();
            }
        }

        private void KmlLayer_LoadStatusChanged(object sender, Esri.ArcGISRuntime.LoadStatusEventArgs e)
        {
            if (e.Status == Esri.ArcGISRuntime.LoadStatus.Loaded)
            {
                // This makes sure that we switch to the UI thread before interacting with any UI components
                RunOnUiThread(() =>
                {
                    // Clear the existing text view
                    _myLabel.Text = "";

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
                    string existingLabel = _myLabel.Text as string;

                    // Update the text with information about the layer
                    existingLabel += String.Format("Name: {0} - Refresh Interval: {1}" +
                        System.Environment.NewLine, netLink.Name, netLink.RefreshInterval);

                    // Update the label with the new text
                    _myLabel.Text = existingLabel;
                }

                // Recur if the node type has children
                if (content is KmlNetworkLink)
                {
                    // Cast the node to the correct type
                    KmlNetworkLink myKmlNetworkLink = (KmlNetworkLink)content;

                    // Recur on the children of the node
                    TraverseNodesUpdateStatus(myKmlNetworkLink.ChildNodes);
                }
                else if (content is KmlContainer)
                {
                    // Cast the node to the correct type
                    KmlContainer myKmlNetworkLink = (KmlContainer)content;

                    // Recur on the children of the node
                    TraverseNodesUpdateStatus(myKmlNetworkLink.ChildNodes);
                }
                // Note: recursion ends when there are no more nodes to visit
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a text view for the KML network links name and refresh interval
            _myLabel = new TextView(this);
            layout.AddView(_myLabel);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}