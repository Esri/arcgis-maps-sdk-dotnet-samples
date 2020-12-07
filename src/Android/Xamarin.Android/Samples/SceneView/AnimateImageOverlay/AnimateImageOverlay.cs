// Copyright 2020 Esri.
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
using ArcGISRuntime;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace ArcGISRuntimeXamarin.Samples.AnimateImageOverlay
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Animate images with image overlay",
        category: "SceneView",
        description: "Animate a series of images with an image overlay.",
        instructions: "The application loads a map of the Southwestern United States. Tap the \"Start\" or \"Stop\" buttons to start or stop the radar animation. Use the drop down menu to select how quickly the animation plays. Move the slider to change the opacity of the image overlay.",
        tags: new[] { "3d", "animation", "drone", "dynamic", "image frame", "image overlay", "real time", "rendering" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("9465e8c02b294c69bdb42de056a23ab1")]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("AnimateImageOverlay.axml")]
    public class AnimateImageOverlay : Activity
    {
        // Hold references to the UI controls.
        private SceneView _mySceneView;
        private Spinner _speedSpinner;
        private SeekBar _opacitySlider;
        private Button _pauseButton;

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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Animate images with image overlay";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create the scene.
            _mySceneView.Scene = new Scene(new Basemap(new Uri("https://www.arcgis.com/home/item.html?id=1970c1995b8f44749f4b9b6e81b5ba45")));

            // Create an envelope for the imagery.
            var pointForFrame = new MapPoint(-120.0724273439448, 35.131016955536694, SpatialReferences.Wgs84);
            var pacificEnvelope = new Envelope(pointForFrame, 15.09589635986124, -14.3770441522488);

            // Create a camera, looking at the pacific southwest sector.
            var observationPoint = new MapPoint(-116.621, 24.7773, 856977, SpatialReferences.Wgs84);
            var camera = new Camera(observationPoint, 353.994, 48.5495, 0);

            // Set the viewpoint of the scene to this camera.
            var pacificViewpoint = new Viewpoint(observationPoint, camera);
            _mySceneView.Scene.InitialViewpoint = pacificViewpoint;

            // Create an image overlay and add it ot the scene..
            _imageOverlay = new ImageOverlay();
            _mySceneView.ImageOverlays.Add(_imageOverlay);

            // Create an array of the image filepaths.
            var imageFolder = Path.Combine(DataManager.GetDataFolder("9465e8c02b294c69bdb42de056a23ab1"), "PacificSouthWest");
            string[] imagePaths = Directory.GetFiles(imageFolder);

            // The paths need to be sorted alphabetically on some file systems.
            Array.Sort(imagePaths);

            // Create all of the image frames using the filepaths and the envelope.
            _images = imagePaths.Select(path => new ImageFrame(new Uri(path), pacificEnvelope)).Take(120).ToArray();

            // Create new Timer and set the timeout interval to approximately 15 image frames per second.
            _timer = new Timer(AnimateOverlay);
            _timer.Change(0, 1000 / 15);

            // Populate the spinner for selecting speed.
            _speedSpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, new string[] { "Slow", "Medium", "Fast" });
            _speedSpinner.SetSelection(0);
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

        private void StartStopAnimation(object sender, EventArgs e)
        {
            // Stop or start the animation.
            _animationStopped = !_animationStopped;

            // Update the button text to reflect the state of animation.
            ((Button)sender).Text = _animationStopped ? "Start" : "Stop";
        }

        private void SpeedSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            int newInterval = 0;
            switch (_speedSpinner.SelectedItem.ToString())
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

        private void ChangeOpacity(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            // Update the opacity of the image overlay.
            if (_imageOverlay != null) _imageOverlay.Opacity = ((SeekBar)sender).Progress / 100.0;
        }

        private void CreateLayout()
        {
            // Load the layout from the axml resource.
            SetContentView(Resource.Layout.AnimateImageOverlay);

            _mySceneView = FindViewById<SceneView>(Resource.Id.SceneView);
            _pauseButton = FindViewById<Button>(Resource.Id.pauseButton);
            _opacitySlider = FindViewById<SeekBar>(Resource.Id.opacitySlider);
            _speedSpinner = FindViewById<Spinner>(Resource.Id.speedSpinner);

            _pauseButton.Click += StartStopAnimation;
            _opacitySlider.ProgressChanged += ChangeOpacity;
            _speedSpinner.ItemSelected += SpeedSelected;
        }

        // Stop the animation on pause.
        protected override void OnPause()
        {
            base.OnPause();
            _animationStopped = true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unhook event handlers.
            _pauseButton.Click -= StartStopAnimation;
            _opacitySlider.ProgressChanged -= ChangeOpacity;
            _speedSpinner.ItemSelected -= SpeedSelected;

            // Stop the animation and timer.
            _animationStopped = true;
            _timer.Dispose();

            // Remove the sceneview
            (_mySceneView.Parent as ViewGroup).RemoveView(_mySceneView);
            _mySceneView.Dispose();
            _mySceneView = null;
        }
    }
}