// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Debug = System.Diagnostics.Debug;

namespace ArcGISRuntimeXamarin.Samples.PlayKmlTours
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Play KML tour",
        "Layers",
        "Play tours in KML files.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("f10b1d37fdd645c9bc9b189fb546307c")]
    public class PlayKmlTours : Activity
    {
        // Hold references to the UI controls.
        private SceneView _mySceneView;
        private Button _playButton;
        private Button _pauseButton;
        private Button _resetButton;

        // The KML tour controller provides player controls for KML tours.
        private readonly KmlTourController _tourController = new KmlTourController();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Play a KML tour";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Load the scene with a basemap and a terrain surface.
            _mySceneView.Scene = new Scene(Basemap.CreateImagery());
            _mySceneView.Scene.BaseSurface.ElevationSources.Add(new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer")));

            // Get the URL to the data.
            string filePath = DataManager.GetDataFolder("f10b1d37fdd645c9bc9b189fb546307c", "Esri_tour.kmz");
            Uri kmlUrl = new Uri(filePath);

            // Create the KML dataset and layer.
            KmlDataset dataset = new KmlDataset(kmlUrl);
            KmlLayer layer = new KmlLayer(dataset);

            // Add the layer to the map.
            _mySceneView.Scene.OperationalLayers.Add(layer);

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

                // Enable the play button.
                _playButton.Enabled = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
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
                case KmlTourStatus.Initialized:
                    _playButton.Enabled = true;
                    _pauseButton.Enabled = false;
                    break;
                case KmlTourStatus.Paused:
                    _playButton.Enabled = true;
                    _pauseButton.Enabled = false;
                    _resetButton.Enabled = true;
                    break;
                case KmlTourStatus.Playing:
                    _resetButton.Enabled = true;
                    _playButton.Enabled = false;
                    _pauseButton.Enabled = true;
                    break;
            }
        }

        // Play the tour when the button is pressed.
        private void Play_Clicked(object sender, EventArgs e) => _tourController?.Play();

        // Pause the tour when the button is pressed.
        private void Pause_Clicked(object sender, EventArgs e) => _tourController?.Pause();

        // Reset the tour when the button is pressed.
        private void Reset_Clicked(object sender, EventArgs e) => _tourController?.Reset();

        protected override void OnStop()
        {
            base.OnStop();

            // Reset the tour controller when the sample closes - avoids a crash.
            _tourController?.Pause();
            _tourController?.Reset();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) {Orientation = Orientation.Vertical};

            // Add a help label.
            TextView helpLabel = new TextView(this);
            helpLabel.Text = "Use the buttons to play the tour. Contains audio. 🎧";
            layout.AddView(helpLabel);

            // Add a row with buttons.
            _playButton = new Button(this) {Text = "Play", Enabled = false};
            _playButton.Click += Play_Clicked;
            _pauseButton = new Button(this) {Text = "Pause", Enabled = false};
            _pauseButton.Click += Pause_Clicked;
            _resetButton = new Button(this) {Text = "Reset", Enabled = false};
            _resetButton.Click += Reset_Clicked;

            LinearLayout rowLayout = new LinearLayout(this) {Orientation = Orientation.Horizontal};

            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent,
                1.0f
            );
            _playButton.LayoutParameters = param;
            _pauseButton.LayoutParameters = param;
            _resetButton.LayoutParameters = param;

            rowLayout.AddView(_playButton);
            rowLayout.AddView(_pauseButton);
            rowLayout.AddView(_resetButton);

            layout.AddView(rowLayout);

            // Add the scene view to the layout.
            _mySceneView = new SceneView(this);
            layout.AddView(_mySceneView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}