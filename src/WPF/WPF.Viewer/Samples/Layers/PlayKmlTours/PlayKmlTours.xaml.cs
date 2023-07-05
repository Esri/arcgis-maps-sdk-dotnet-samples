// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.PlayKmlTours
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Play KML tour",
        category: "Layers",
        description: "Play tours in KML files.",
        instructions: "The sample will load the KMZ file from ArcGIS Online. When a tour is found, the _Play_ button will be enabled. Use _Play_ and _Pause_ to control the tour. Use the _Reset_ button to return the tour to the unplayed state.",
        tags: new[] { "KML", "animation", "interactive", "narration", "pause", "play", "story", "tour" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("f10b1d37fdd645c9bc9b189fb546307c")]
    public partial class PlayKmlTours
    {
        // The KML tour controller provides player controls for KML tours.
        private readonly KmlTourController _tourController = new KmlTourController();

        // Keep track of play/pause status.
        private bool _tourPlaying;

        public PlayKmlTours()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Load the scene with a basemap and a terrain surface.
            MySceneView.Scene = new Scene(BasemapStyle.ArcGISImageryStandard);
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

                // Listen for changes to the tour status.
                _tourController.Tour.PropertyChanged += Tour_PropertyChanged;

                // Be notified when the sample is left so that the tour can be reset.
                this.Unloaded += Sample_Unloaded;
                Application.Current.Exit += Application_Exit;

                // Enable the play button.
                PlayPauseButton.IsEnabled = true;

                // Hide the status bar.
                LoadingStatusBar.Visibility = Visibility.Collapsed;

                // Create scene interaction options which will be disabled when the tour begins.
                MySceneView.InteractionOptions = new SceneViewInteractionOptions
                {
                    IsEnabled = false
                };
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                MessageBox.Show(e.ToString(), "Error");
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
                if (currentNode is KmlTour tourNode)
                {
                    _tourController.Tour = tourNode;
                    return;
                }

                // If the node is a container, add all of its children to the list of nodes to explore.
                if (currentNode is KmlContainer container)
                {
                    foreach (KmlNode node in container.ChildNodes)
                    {
                        nodesToExplore.Enqueue(node);
                    }
                }

                // Otherwise, continue.
            }
        }

        private void Tour_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Skip for everything except tour status property changes.
            if (e.PropertyName != nameof(KmlTour.TourStatus))
            {
                return;
            }

            // Set the UI based on the current state of the tour.
            switch (_tourController.Tour.TourStatus)
            {
                case KmlTourStatus.Completed:
                    PlayPauseButton.IsEnabled = true;
                    ResetButton.IsEnabled = false;
                    PlayPauseButton.Content = "Play";
                    _tourPlaying = false;
                    MySceneView.InteractionOptions.IsEnabled = true;

                    // Return to the initial viewpoint to visually indicate the tour being over.
                    MySceneView.SetViewpointAsync(MySceneView.Scene.InitialViewpoint);
                    break;

                case KmlTourStatus.Initialized:
                    PlayPauseButton.IsEnabled = true;
                    ResetButton.IsEnabled = false;
                    PlayPauseButton.Content = "Play";
                    _tourPlaying = false;
                    MySceneView.InteractionOptions.IsEnabled = true;
                    break;

                case KmlTourStatus.Playing:
                    ResetButton.IsEnabled = true;
                    PlayPauseButton.Content = "Pause";
                    _tourPlaying = true;
                    MySceneView.InteractionOptions.IsEnabled = false;
                    break;

                case KmlTourStatus.Paused:
                    PlayPauseButton.Content = "Play";
                    _tourPlaying = false;
                    break;
            }
        }

        // Play and pause the tour when the button is pressed.
        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (_tourPlaying)
            {
                _tourController?.Pause();
            }
            else
            {
                _tourController?.Play();
            }
        }

        // Reset the tour when the button is pressed.
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            _tourController?.Reset();
            MySceneView.SetViewpoint(MySceneView.Scene.InitialViewpoint);
            PlayPauseButton.Content = "Play";
        }

        // Reset the tour when the user leaves the sample - avoids a crash.
        private void Sample_Unloaded(object sender, RoutedEventArgs e)
        {
            _tourController?.Pause();
            _tourController?.Reset();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                _tourController?.Pause();
            }
            catch
            {
            }
        }
    }
}