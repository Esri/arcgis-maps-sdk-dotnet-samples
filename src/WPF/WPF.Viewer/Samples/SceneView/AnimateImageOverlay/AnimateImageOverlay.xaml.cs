// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace ArcGIS.WPF.Samples.AnimateImageOverlay
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Animate images with image overlay",
        category: "SceneView",
        description: "Animate a series of images with an image overlay.",
        instructions: "The application loads a map of the Southwestern United States. Click the \"Start\" or \"Stop\" buttons to start or stop the radar animation. Use the drop down menu to select how quickly the animation plays. Move the slider to change the opacity of the image overlay.",
        tags: new[] { "3d", "animation", "drone", "dynamic", "image frame", "image overlay", "real time", "rendering" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("9465e8c02b294c69bdb42de056a23ab1")]
    public partial class AnimateImageOverlay
    {
        // Image overlay for displaying the images from the file system in the scene.
        private ImageOverlay _imageOverlay;

        // Timer for animating images.
        private Timer _timer;

        // Boolean for stopping and starting the animation.
        private bool _animationStopped = false;

        // All of the image frames used for the animation.
        private ImageFrame[] _images;

        // Index of the image currently being displayed.
        private int _imageIndex = 0;

        public AnimateImageOverlay()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // This sample is only supported in x64 on WPF.
            if (!Environment.Is64BitProcess)
            {
                MessageBox.Show("This sample is only supported for WPF in x64. Run the sample viewer in x64 to use this sample.");
            }

            // Create the scene.
            MySceneView.Scene = new Scene(new Basemap(new Uri("https://www.arcgis.com/home/item.html?id=1970c1995b8f44749f4b9b6e81b5ba45")));

            // Create an envelope for the imagery.
            var pointForFrame = new MapPoint(-120.0724273439448, 35.131016955536694, SpatialReferences.Wgs84);
            var pacificEnvelope = new Envelope(pointForFrame, 15.09589635986124, -14.3770441522488);

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
            string[] imagePaths = Directory.GetFiles(imageFolder);

            // The paths need to be sorted alphabetically on some file systems.
            Array.Sort(imagePaths);

            // Create all of the image frames using the filepaths and the envelope.
            _images = imagePaths.Select(path => new ImageFrame(new Uri(path), pacificEnvelope)).ToArray();

            // Create new Timer and set the timeout interval to approximately 15 image frames per second.
            _timer = new Timer(AnimateOverlay);
            _timer.Change(0, 1000 / 15);

            // Populate the combobox for selecting speed.
            SpeedComboBox.ItemsSource = new string[] { "Slow", "Medium", "Fast" };
            SpeedComboBox.SelectedIndex = 0;

            // Set an event to stop the animation when the sample is unloaded.
            Unloaded += (s, e) =>
            {
                _animationStopped = true;
                _timer.Dispose();
            };
        }

        private void AnimateOverlay(object state)
        {
            if (!_animationStopped)
            {
                // Set the image overlay to display the next frame.
                _imageOverlay.ImageFrame = _images[_imageIndex];

                // Increase the index of the image.
                _imageIndex = (_imageIndex + 1) % _images.Length;
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

        private void SpeedSelected(object sender, SelectionChangedEventArgs e)
        {
            int newInterval = 0;
            switch (SpeedComboBox.SelectedItem)
            {
                case "Slow":
                    newInterval = 1000 / 15;
                    break;

                case "Medium":
                    newInterval = 1000 / 30;
                    break;

                case "Fast":
                    newInterval = 1000 / 60;
                    break;
            }

            // Calculate the new time interval using the selected speed.
            _timer?.Change(0, newInterval);
        }
    }
}