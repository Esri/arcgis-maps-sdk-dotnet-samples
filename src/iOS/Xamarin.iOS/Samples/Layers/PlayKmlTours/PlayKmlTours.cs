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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.PlayKmlTours
{
    [Register("PlayKmlTours")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Play KML Tour",
        "Layers",
        "Play tours in KML files.",
        "The sample will load the KMZ file from ArcGIS Online. When a tour is found, the _Play_ button will be enabled. Use _Play_ and _Pause_ to control the tour. When you're ready to show the tour, use the reset button to return the tour to the unplayed state.",
        "KML", "animation", "interactive", "narration", "pause", "play", "story", "tour")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("f10b1d37fdd645c9bc9b189fb546307c")]
    public class PlayKmlTours : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;
        private UIBarButtonItem _playButton;
        private UIBarButtonItem _pauseButton;
        private UIBarButtonItem _resetButton;
        private UIActivityIndicatorView _loadingIndicator;

        // The KML tour controller provides player controls for KML tours.
        private readonly KmlTourController _tourController = new KmlTourController();

        public PlayKmlTours()
        {
            Title = "Play a KML tour";
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

                // Enable the play button.
                _playButton.Enabled = true;

                // Hide the activity indicator.
                _loadingIndicator.StopAnimating();
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
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
                    _tourController.Tour.PropertyChanged += Tour_PropertyChanged;
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

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            _playButton = new UIBarButtonItem(UIBarButtonSystemItem.Play) {Enabled = false};
            _pauseButton = new UIBarButtonItem(UIBarButtonSystemItem.Pause) {Enabled = false};
            _resetButton = new UIBarButtonItem(UIBarButtonSystemItem.Refresh) {Enabled = false};

            toolbar.Items = new UIBarButtonItem[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _playButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _pauseButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _resetButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
            };

            UILabel helpLabel = new UILabel
            {
                Text = "Use the buttons to play the tour. Contains audio. 🎧",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _loadingIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge)
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                HidesWhenStopped = true,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f)
            };
            _loadingIndicator.StartAnimating();

            // Add the views.
            View.AddSubviews(_mySceneView, toolbar, helpLabel, _loadingIndicator);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                helpLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                helpLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                helpLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                helpLabel.HeightAnchor.ConstraintEqualTo(40),
                _loadingIndicator.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _loadingIndicator.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _loadingIndicator.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _loadingIndicator.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _playButton.Clicked += Play_Clicked;
            _pauseButton.Clicked += Pause_Clicked;
            _resetButton.Clicked += Reset_Clicked;

            // Subscribe to tour events, removing any existing subscription.
            if (_tourController.Tour != null)
            {
                _tourController.Tour.PropertyChanged -= Tour_PropertyChanged;
                _tourController.Tour.PropertyChanged += Tour_PropertyChanged;
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            // Reset the tour controller when the sample closes - avoids a crash.
            _tourController?.Pause();
            _tourController?.Reset();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            if (_tourController.Tour != null) _tourController.Tour.PropertyChanged -= Tour_PropertyChanged;
            _playButton.Clicked -= Play_Clicked;
            _pauseButton.Clicked -= Pause_Clicked;
            _resetButton.Clicked -= Reset_Clicked;
        }
    }
}