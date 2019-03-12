// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.UWP.Samples.PlayKmlTours
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Play a KML tour",
        "Layers",
        "Play tours in KML files.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("f10b1d37fdd645c9bc9b189fb546307c")]
    public partial class PlayKmlTours
    {
        // The KML tour controller provides player controls for KML tours.
        private readonly KmlTourController _tourController = new KmlTourController();

        public PlayKmlTours()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Load the scene with a basemap and a terrain surface.
            MySceneView.Scene = new Scene(Basemap.CreateImagery());
            MySceneView.Scene.BaseSurface.ElevationSources.Add(new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer")));

            // Get the URL to the data.
            string filePath = DataManager.GetDataFolder("f10b1d37fdd645c9bc9b189fb546307c", "Esri_tour.kmz");
            Uri kmlUrl = new Uri(filePath);

            // Create the KML dataset and layer.
            KmlDataset dataset = new KmlDataset(kmlUrl);
            KmlLayer layer = new KmlLayer(dataset);

            // Add the layer to the map.
            MySceneView.Scene.OperationalLayers.Add(layer);

            try
            {
                // Load the dataset.
                await dataset.LoadAsync();

                // Find the first KML tour.
                FindKmlTour(dataset.RootNodes);

                // Handle absence of tour gracefully.
                if (_tourController.Tour == null)
                {
                    throw new InvalidOperationException("No tour found. Can't enable touring for a KML file with no tours.");
                }

                // Enable the play button.
                PlayButton.IsEnabled = true;

                // Hide the status bar.
                LoadingStatusBar.Visibility = Visibility.Collapsed;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                await new MessageDialog(e.ToString(), "Error").ShowAsync();
            }
        }

        private void FindKmlTour(IEnumerable<KmlNode> rootNodes)
        {
            // Hold a list of nodes to explore.
            Queue<KmlNode> nodesToExplore = new Queue<KmlNode>();

            // Add each root node to the list.
            foreach (KmlNode rootNode in rootNodes)
            {
                nodesToExplore.Enqueue(rootNode);
            }

            // Keep exploring until a tour is found or there are no more nodes.
            while (nodesToExplore.Any())
            {
                // Remove a node from the queue.
                KmlNode currentNode = nodesToExplore.Dequeue();

                // If the node is a tour, use it.
                if (currentNode is KmlTour)
                {
                    _tourController.Tour = (KmlTour) currentNode;
                    return;
                }

                // If the node is a container, add all of its children to the list of nodes to explore.
                if (currentNode is KmlContainer)
                {
                    KmlContainer container = (KmlContainer) currentNode;
                    foreach (KmlNode node in container.ChildNodes)
                    {
                        nodesToExplore.Enqueue(node);
                    }
                }

                // Otherwise, continue.
            }
        }

        private void Play_Clicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_tourController != null)
            {
                // Play the tour.
                _tourController.Play();

                // Configure the UI.
                PauseButton.IsEnabled = true;
                PlayButton.IsEnabled = false;
                ResetButton.IsEnabled = true;
            }
        }

        private void Pause_Clicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_tourController != null)
            {
                // Pause the tour.
                _tourController.Pause();

                // Configure the UI.
                PlayButton.IsEnabled = true;
                PauseButton.IsEnabled = false;
            }
        }

        private void Reset_Clicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_tourController != null)
            {
                // Reset the tour.
                _tourController.Reset();

                // Configure the UI.
                PlayButton.IsEnabled = true;
                PauseButton.IsEnabled = false;
                ResetButton.IsEnabled = false;
            }
        }
    }
}