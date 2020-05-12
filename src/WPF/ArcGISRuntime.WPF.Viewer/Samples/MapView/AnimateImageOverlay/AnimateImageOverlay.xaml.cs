// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.WPF.Samples.AnimateImageOverlay
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Animate images with image overlay",
        "MapView",
        "Animate a series of images with an image overlay.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("9465e8c02b294c69bdb42de056a23ab1")]
    public partial class AnimateImageOverlay
    {
        // Image overlay for displaying the images from the file system in the scene.
        private ImageOverlay _imageOverlay;

        // Envelope for geographic location of the images.
        private Envelope _pacificEnvelope;

        // Timer for animating images.
        private Timer _timer;

        // Boolean for stopping and starting the animation.
        private bool _animationStopped = false;

        // File paths for all of the images.
        private string[] _imagePaths;

        // Index of the image currently being displayed.
        private int _imageIndex = 0;

        public AnimateImageOverlay()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create the scene.
            MySceneView.Scene = new Scene(new Basemap(new Uri("https://www.arcgis.com/home/item.html?id=1970c1995b8f44749f4b9b6e81b5ba45")));

            // Create an envelope for the imagery.
            var pointForFrame = new MapPoint(-120.0724273439448, 35.131016955536694, SpatialReferences.Wgs84);
            _pacificEnvelope = new Envelope(pointForFrame, 15.09589635986124, -14.3770441522488);

            // Create a camera, looking at the pacific southwest sector.
            var observationPoint = new MapPoint(-116.621, 24.7773, 856977, SpatialReferences.Wgs84);
            var camera = new Camera(observationPoint, 353.994, 48.5495, 0);

            // Set the viewpoint of the scene to this camera.
            var pacificViewpoint = new Viewpoint(observationPoint, camera);
            MySceneView.Scene.InitialViewpoint = pacificViewpoint;

            // Create an image overlay and add it ot the scene..
            _imageOverlay = new ImageOverlay();
            MySceneView.ImageOverlays.Add(_imageOverlay);

            // Create an array of the image filepaths.
            var imageFolder = Path.Combine(DataManager.GetDataFolder("9465e8c02b294c69bdb42de056a23ab1"), "PacificSouthWest");
            _imagePaths = Directory.GetFiles(imageFolder);

            // Create new Timer and set the timeout interval to approximately 15 frames a second.
            _timer = new Timer(AnimateOverlay);
            _timer.Change(0, 1000 / 15);

            // Populate the combobox for selecting FPS.
            FPSComboBox.ItemsSource = new int[] { 15, 30, 60 };
            FPSComboBox.SelectedIndex = 0;
        }

        private void AnimateOverlay(object state)
        {
            if (!_animationStopped)
            {
                // Create a new image frame, using the image filepath and the envelope.
                _imageOverlay.ImageFrame = new ImageFrame(new Uri(_imagePaths[_imageIndex]), _pacificEnvelope);

                // Increase the index of the image path.
                _imageIndex = (_imageIndex + 1) % _imagePaths.Length;
            }
        }

        private void StartStopAnimation(object sender, RoutedEventArgs e)
        {
            // Stop or start the animation.
            _animationStopped = !_animationStopped;

            // Update the button text to reflect the state of animation.
            ((Button)sender).Content = _animationStopped ? "Start" : "Stop";
        }

        private void ChangeOpacity(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Update the opacity of the image overlay.
            if (_imageOverlay != null) _imageOverlay.Opacity = e.NewValue;
        }

        private void FPSSelected(object sender, SelectionChangedEventArgs e)
        {
            // Calculate the new time interval using the selected frames per second.
            int newInterval = 1000 / (int)FPSComboBox.SelectedItem;
            _timer?.Change(0, newInterval);
        }
    }
}