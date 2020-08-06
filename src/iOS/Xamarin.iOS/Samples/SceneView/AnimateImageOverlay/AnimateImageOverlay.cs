// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.AnimateImageOverlay
{
    [Register("AnimateImageOverlay")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Animate images with image overlay",
        category: "SceneView",
        description: "Animate a series of images with an image overlay.",
        instructions: "The application loads a map of the Southwestern United States. Tap the \"Start\" or \"Stop\" buttons to start or stop the radar animation. Use the drop down menu to select how quickly the animation plays. Move the slider to change the opacity of the image overlay.",
        tags: new[] { "3d", "animation", "drone", "dynamic", "image frame", "image overlay", "real time", "rendering" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("9465e8c02b294c69bdb42de056a23ab1")]
    public class AnimateImageOverlay : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;
        private UIBarButtonItem _pauseButton;
        private UIBarButtonItem _speedButton;
        private UISlider _opacitySlider;
        private UIToolbar _buttonToolbar;

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
            Title = "Animate images with image overlay";
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

        private void SpeedButtonClick(object sender, EventArgs e)
        {
            // Start the UI for the user choosing the trace type.
            UIAlertController prompt = UIAlertController.Create(null, "Choose trace type", UIAlertControllerStyle.ActionSheet);

            // Add a selection action for every valid trace type.
            foreach (string speed in new string[] { "Slow", "Medium", "Fast"})
            {
                prompt.AddAction(UIAlertAction.Create(speed, UIAlertActionStyle.Default, SpeedSelected));
            }

            // Needed to prevent crash on iPad.
            UIPopoverPresentationController ppc = prompt.PopoverPresentationController;
            if (ppc != null)
            {
                ppc.SourceView = _buttonToolbar;
                ppc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
            }

            PresentViewController(prompt, true, null);
        }

        private void SpeedSelected(UIAlertAction action)
        {
            int newInterval = 0;
            switch (action.Title)
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

            // Update the button text.
            _speedButton.Title = action.Title;
        }

        private void ChangeOpacity(object sender, EventArgs e)
        {
            // Update the opacity of the image overlay.
            if (_imageOverlay != null) _imageOverlay.Opacity = ((UISlider)sender).Value;
        }

        private void StopStartAnimation(object sender, EventArgs e)
        {
            // Stop or start the animation.
            _animationStopped = !_animationStopped;

            // Update the button text to reflect the state of animation.
            ((UIBarButtonItem)sender).Title = _animationStopped ? "Start" : "Stop";
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = ApplicationTheme.BackgroundColor };

            // Create the scene view.
            _mySceneView = new SceneView { TranslatesAutoresizingMaskIntoConstraints = false };

            // Create the toolbars.
            _buttonToolbar = new UIToolbar { TranslatesAutoresizingMaskIntoConstraints = false };

            // Create the opacity items.
            var opacityLabel = new UILabel { TranslatesAutoresizingMaskIntoConstraints = false, Text = "Opacity" };
            _opacitySlider = new UISlider { TranslatesAutoresizingMaskIntoConstraints = false, MinValue = 0, MaxValue = 1, Value = 1 };

            // Add the opacity items to a stackview.
            var stackView = new UIStackView()
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Distribution = UIStackViewDistribution.Fill,
                Alignment = UIStackViewAlignment.Center,
                Axis = UILayoutConstraintAxis.Horizontal,
                Spacing = 10,
                LayoutMargins = new UIEdgeInsets(5, 5, 5, 5),
                LayoutMarginsRelativeArrangement = true,
            };
            stackView.AddArrangedSubview(opacityLabel);
            stackView.AddArrangedSubview(_opacitySlider);

            // Create the buttons.
            _pauseButton = new UIBarButtonItem { Title = "Stop" };
            _speedButton = new UIBarButtonItem { Title = "Slow" };

            // Add the buttons to a toolbar.
            _buttonToolbar.Items = new UIBarButtonItem[] {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _pauseButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _speedButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
            };

            // Add the views.
            View.AddSubviews(_mySceneView, stackView, _buttonToolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]{
                _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(stackView.TopAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                stackView.TopAnchor.ConstraintEqualTo(_mySceneView.BottomAnchor),
                stackView.BottomAnchor.ConstraintEqualTo(_buttonToolbar.TopAnchor),
                stackView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                stackView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                _buttonToolbar.TopAnchor.ConstraintEqualTo(stackView.BottomAnchor),
                _buttonToolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                _buttonToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _buttonToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _pauseButton.Clicked += StopStartAnimation;
            _opacitySlider.TouchUpInside += ChangeOpacity;
            _speedButton.Clicked += SpeedButtonClick;

            // Create new Timer and set the timeout interval to approximately 15 image frames per second.
            _timer = new Timer(AnimateOverlay);
            _timer.Change(0, 1000 / 15);
            _speedButton.Title = "Slow";
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _pauseButton.Clicked -= StopStartAnimation;
            _opacitySlider.TouchUpInside -= ChangeOpacity;
            _speedButton.Clicked -= SpeedButtonClick;

            // Stop the animation when the sample is unloaded.
            _animationStopped = true;
            _pauseButton.Title = "Start";
            _timer.Dispose();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}