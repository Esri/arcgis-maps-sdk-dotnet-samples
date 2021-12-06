// Copyright 2021 Esri.
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
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Timers;
using UIKit;

namespace ArcGISRuntime.Samples.Animate3DGraphic
{
    [Register("Animate3DGraphic")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("290f0c571c394461a8b58b6775d0bd63",
        "e87c154fb9c2487f999143df5b08e9b1", "5a9b60cee9ba41e79640a06bcdf8084d", "12509ffdc684437f8f2656b0129d2c13",
        "681d6f7694644709a7c830ec57a2d72b")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Animate 3D graphic",
        category: "GraphicsOverlay",
        description: "An `OrbitGeoElementCameraController` follows a graphic while the graphic's position and rotation are animated.",
        instructions: "Animation Controls:",
        tags: new[] { "Camera", "ModelSceneSymbol", "OrbitGeoElementCameraController", "RendererSceneProperties", "SceneView" })]
    public class Animate3DGraphic : UIViewController
    {
        // Hold references to UI controls.
        private MapView _insetMapView;
        private SceneView _mySceneView;
        private UIBarButtonItem _playButton;
        private UIBarButtonItem _missionButton;
        private UIBarButtonItem _cameraButton;
        private UIBarButtonItem _statsButton;
        private StatsDisplayViewController _statsVC;

        // URL to the elevation service - provides terrain elevation.
        private readonly Uri _elevationServiceUrl =
            new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

        // Graphic for highlighting the route in the inset map.
        private Graphic _routeGraphic;

        // Graphic for highlighting the airplane in the inset map.
        private Graphic _plane2D;

        // Graphic for showing the 3D plane model in the scene.
        private Graphic _plane3D;

        // Camera controller for centering the camera on the airplane.
        private OrbitGeoElementCameraController _orbitCameraController;

        // Timer control enables stopping and starting frame-by-frame animation.
        private Timer _animationTimer;

        // Number of frames in the mission animation.
        private int _frameCount;

        // Index of current frame in the animation.
        private int _keyframe;

        // Dictionary of mission file names and the corresponding portal item IDs.
        private readonly Dictionary<string, string> _missionToItemId = new Dictionary<string, string>
        {
            {"GrandCanyon", "290f0c571c394461a8b58b6775d0bd63"},
            {"Hawaii", "e87c154fb9c2487f999143df5b08e9b1"},
            {"Pyrenees", "5a9b60cee9ba41e79640a06bcdf8084d"},
            {"Snowdon", "12509ffdc684437f8f2656b0129d2c13"}
        };

        // Array of frames for the current mission.
        //    A MissionFrame contains the position of the plane for a single moment in the animation.
        private MissionFrame[] _missionData;

        // Flag to control which camera will be used.
        private bool _shouldFollowPlane = true;

        // Set the title of the sample.
        public Animate3DGraphic()
        {
            Title = "Animate 3D graphic";
        }

        private async void Initialize()
        {
            _statsVC = new StatsDisplayViewController();

            // Apply appropriate maps to the scene and the inset map view.
            _insetMapView.Map = new Map(BasemapStyle.ArcGISImageryStandard);
            _insetMapView.IsAttributionTextVisible = false;
            _mySceneView.Scene = new Scene(BasemapStyle.ArcGISImageryStandard);

            // Apply the elevation source.
            Surface surface = new Surface();
            ElevationSource elevationSource = new ArcGISTiledElevationSource(_elevationServiceUrl);
            surface.ElevationSources.Add(elevationSource);
            _mySceneView.Scene.BaseSurface = surface;

            // Create and add the graphics overlay.
            GraphicsOverlay sceneOverlay = new GraphicsOverlay
            {
                SceneProperties = { SurfacePlacement = SurfacePlacement.Absolute }
            };
            _mySceneView.GraphicsOverlays.Add(sceneOverlay);

            // Create a renderer to handle updating plane's orientation.
            SimpleRenderer renderer3D = new SimpleRenderer();
            RendererSceneProperties renderProperties = renderer3D.SceneProperties;

            // Use expressions to keep the renderer properties updated as parameters of the rendered object.
            renderProperties.HeadingExpression = "[HEADING]";
            renderProperties.PitchExpression = "[PITCH]";
            renderProperties.RollExpression = "[ROLL]";

            // Apply the renderer to the scene view's overlay.
            sceneOverlay.Renderer = renderer3D;

            // Create renderer to symbolize plane and update plane orientation in the inset map.
            SimpleRenderer renderer2D = new SimpleRenderer
            {
                Symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, Color.Blue, 10),
                RotationExpression = "[ANGLE]"
            };

            // Update the inset map with a new GraphicsOverlay based on the renderer.
            GraphicsOverlay insetMapOverlay = new GraphicsOverlay
            {
                Renderer = renderer2D
            };
            _insetMapView.GraphicsOverlays.Add(insetMapOverlay);

            // Create placeholder graphic for showing the mission route in the inset map.
            SimpleLineSymbol routeSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 2);
            _routeGraphic = new Graphic { Symbol = routeSymbol };
            insetMapOverlay.Graphics.Add(_routeGraphic);

            // Create the plane graphic; this is symbolized as a blue triangle because of renderer implemented above.
            Dictionary<string, object> plane2DAttributes = new Dictionary<string, object>
            {
                // Set the angle for the plane graphic.
                ["ANGLE"] = 0f
            };
            // Create the graphic from the attributes and the initial point.
            _plane2D = new Graphic(new MapPoint(0, 0, SpatialReferences.Wgs84), plane2DAttributes);

            // Add the plane graphic to the inset map via the overlay.
            insetMapOverlay.Graphics.Add(_plane2D);

            try
            {
                // Create the model graphic for the plane.
                string modelPath = DataManager.GetDataFolder("681d6f7694644709a7c830ec57a2d72b", "Bristol.dae");

                // Create the scene symbol from the path to the model.
                ModelSceneSymbol plane3DSymbol = await ModelSceneSymbol.CreateAsync(new Uri(modelPath), 1.0);

                // Create the graphic with an initial location and the plane symbol.
                _plane3D = new Graphic(new MapPoint(0, 0, 0, SpatialReferences.Wgs84), plane3DSymbol);

                // Add the plane to the overlay.
                sceneOverlay.Graphics.Add(_plane3D);

                // Create the orbit camera controller to follow the plane.
                _orbitCameraController = new OrbitGeoElementCameraController(_plane3D, 20.0)
                {
                    CameraPitchOffset = 75.0
                };
                _mySceneView.CameraController = _orbitCameraController;

                // Create a timer; this animates the plane.
                // The value is the duration of the timer in milliseconds. This controls the speed of the animation (fps).
                _animationTimer = new Timer(60)
                {
                    AutoReset = true
                };

                // Set the initial mission for when the sample loads.
                ChangeMission(_missionToItemId.Keys.First());
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate)null, "OK", null).Show();
            }
        }

        private void Timer_Elapsed(object sender, EventArgs e) => AnimatePlane();

        private void ShowMissionOptions(object sender, EventArgs eventArgs)
        {
            // Create the view controller that will present the list of missions.
            UIAlertController missionSelectionAlert =
                UIAlertController.Create(null, "Select a mission", UIAlertControllerStyle.ActionSheet);

            // Needed to prevent a crash on iPad.
            UIPopoverPresentationController presentationPopover = missionSelectionAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.BarButtonItem = (UIBarButtonItem)sender;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Add an option for each mission.
            foreach (string item in _missionToItemId.Keys)
            {
                // Selecting the mission will call the ChangeMission method.
                missionSelectionAlert.AddAction(UIAlertAction.Create(item, UIAlertActionStyle.Default, action => ChangeMission(item)));
            }

            // Show the alert.
            PresentViewController(missionSelectionAlert, true, null);
        }

        private void ChangeMission(string mission)
        {
            // Stop animating the current mission.
            _animationTimer.Stop();

            // Get mission data.
            _missionData = GetMissionData(mission);

            // Draw mission route on the inset.
            // Create a collection of points to hold the mission.
            PointCollection points = new PointCollection(SpatialReferences.Wgs84);

            // Add all of the points from the mission to the point collection.
            points.AddPoints(_missionData.Select(m => m.ToMapPoint()));

            // Create a polyline to symbolize the route from the point collection.
            _routeGraphic.Geometry = new Polyline(points);

            // Update animation parameters.
            _frameCount = _missionData.Length;
            _keyframe = 0;

            // Set the _playButton button back to the currently 'playing' state
            _playButton.Title = "Pause";

            // Restart the animation.
            _animationTimer.Start();
        }

        private MissionFrame[] GetMissionData(string mission)
        {
            // Get the path to the file.
            string filePath = GetMissionFilePath(mission);

            // Read the file text.
            string fileContents = File.ReadAllText(filePath);

            // Split the file contents into a list of lines.
            return fileContents.Split('\n')
                // Then for each line, create a MissionFrame object.
                .Select(MissionFrame.Create)
                // Then remove any null MissionFrames.
                .Where(missionPart => missionPart != null)
                // Finally return that list of MissionFrames as an array.
                .ToArray();
        }

        private void AnimatePlane()
        {
            // Get the next position; % prevents going out of bounds even if the keyframe value is
            //     changed unexpectedly (e.g. due to user interaction with the progress slider).
            MissionFrame currentFrame = _missionData[_keyframe];

            // Update the UI.
            double missionProgress = _keyframe / (double)_frameCount;

            // This is needed because the event could be running on a non-UI thread.
            InvokeOnMainThread(() =>
            {
                // Update stats display.
                _statsVC.UpdateStatsDisplay(currentFrame, missionProgress);
            });

            // Update plane's position.
            _plane3D.Geometry = currentFrame.ToMapPoint();
            _plane3D.Attributes["HEADING"] = currentFrame.Heading;
            _plane3D.Attributes["PITCH"] = currentFrame.Pitch;
            _plane3D.Attributes["ROLL"] = currentFrame.Roll;

            // Update the inset map; plane symbol position.
            _plane2D.Geometry = currentFrame.ToMapPoint();

            // Update inset's viewpoint and heading.
            Viewpoint vp = new Viewpoint(currentFrame.ToMapPoint(), 100000, 360 + (float)currentFrame.Heading);
            _insetMapView.SetViewpoint(vp);

            // Update the keyframe. This advances the animation.
            _keyframe++;

            // Restart the animation if it has finished.
            if (_keyframe >= _frameCount)
            {
                _keyframe = 0;
            }
        }

        private string GetMissionFilePath(string mission)
        {
            string itemId = _missionToItemId[mission];
            string filename = mission + ".csv";

            return DataManager.GetDataFolder(itemId, filename);
        }

        private void TogglePlayMission(object sender, EventArgs e)
        {
            if (_playButton.Title == "Play")
            {
                // Resume playing.
                _playButton.Title = "Pause";
                _animationTimer.Start();
            }
            else
            {
                // Pause.
                _playButton.Title = "Play";
                _animationTimer.Stop();
            }
        }

        private void ToggleFollowPlane(object sender, EventArgs e)
        {
            // Update the flag.
            _shouldFollowPlane = !_shouldFollowPlane;

            // Setting the camera controller to null resets it to the default.
            // If should follow is true, the orbit camera controller will be used.
            _mySceneView.CameraController = _shouldFollowPlane ? _orbitCameraController : null;
        }

        private void ToggleStatsDisplay(object sender, EventArgs e)
        {
            _statsVC.ModalPresentationStyle = UIModalPresentationStyle.Popover;
            _statsVC.PreferredContentSize = new CoreGraphics.CGSize(275, 175);
            UIPopoverPresentationController pc = _statsVC.PopoverPresentationController;
            if (pc != null)
            {
                pc.BarButtonItem = (UIBarButtonItem)sender;
                pc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
                pc.Delegate = new PpDelegate();
            }

            PresentViewController(_statsVC, true, null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = ApplicationTheme.BackgroundColor };

            UIToolbar controlToolbox = new UIToolbar();
            controlToolbox.TranslatesAutoresizingMaskIntoConstraints = false;

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            _insetMapView = new MapView();
            _insetMapView.TranslatesAutoresizingMaskIntoConstraints = false;
            _insetMapView.IsAttributionTextVisible = false;

            _playButton = new UIBarButtonItem();
            _playButton.Title = "Pause";
            _playButton.Width = 100;

            _missionButton = new UIBarButtonItem();
            _missionButton.Title = "Mission";

            _cameraButton = new UIBarButtonItem();
            _cameraButton.Title = "Camera";

            _statsButton = new UIBarButtonItem();
            _statsButton.Title = "Stats";

            controlToolbox.Items = new[]
            {
                _missionButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _cameraButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _playButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _statsButton
            };

            // Add the views.
            View.AddSubviews(_mySceneView, _insetMapView, controlToolbox);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                controlToolbox.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                controlToolbox.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                controlToolbox.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(controlToolbox.TopAnchor),

                _insetMapView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, 16),
                _insetMapView.BottomAnchor.ConstraintEqualTo(_mySceneView.BottomAnchor, -40),
                _insetMapView.WidthAnchor.ConstraintEqualTo(96),
                _insetMapView.HeightAnchor.ConstraintEqualTo(96)
            });
        }

        // Force popover to display on iPhone.
        private class PpDelegate : UIPopoverPresentationControllerDelegate
        {
            public override UIModalPresentationStyle GetAdaptivePresentationStyle(
                UIPresentationController forPresentationController) => UIModalPresentationStyle.None;

            public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller,
                UITraitCollection traitCollection) => UIModalPresentationStyle.None;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            if (_animationTimer != null) _animationTimer.Elapsed += Timer_Elapsed;
            _playButton.Clicked += TogglePlayMission;
            _missionButton.Clicked += ShowMissionOptions;
            _cameraButton.Clicked += ToggleFollowPlane;
            _statsButton.Clicked += ToggleStatsDisplay;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            if (_animationTimer != null)
            {
                // Explicitly stop the timer before unsubscribing.
                _animationTimer.Stop();
                _playButton.Title = "Play";
                _animationTimer.Elapsed -= Timer_Elapsed;
            }
            _playButton.Clicked -= TogglePlayMission;
            _missionButton.Clicked -= ShowMissionOptions;
            _cameraButton.Clicked -= ToggleFollowPlane;
            _statsButton.Clicked -= ToggleStatsDisplay;
        }
    }

    /// <summary>
    /// Private helper class represents a single frame in the animation.
    /// </summary>
    public class MissionFrame
    {
        private double Longitude { get; }
        private double Latitude { get; }
        public double Elevation { get; }
        public double Heading { get; }
        public double Pitch { get; }
        public double Roll { get; }

        /// <summary>
        /// Private constructor ensures that only the factory method (Create) can be called..
        /// </summary>
        /// <param name="missionLine">A string describing a single frame in the mission animation.</param>
        public MissionFrame(string missionLine)
        {
            // Split the string into a list of entries (columns).
            // Example line: -156.3666517,20.6255059,999.999908,83.77659,.00009,-47.766567
            string[] missionFrameParameters = missionLine.Split(',');

            // Throw if the line isn't valid.
            if (missionFrameParameters.Length != 6)
            {
                throw new Exception("Invalid mission part definition");
            }

            // Populate the object's properties from the array of parameters.
            Longitude = Convert.ToDouble(missionFrameParameters[0], CultureInfo.InvariantCulture);
            Latitude = Convert.ToDouble(missionFrameParameters[1], CultureInfo.InvariantCulture);
            Elevation = Convert.ToDouble(missionFrameParameters[2], CultureInfo.InvariantCulture);
            Heading = Convert.ToDouble(missionFrameParameters[3], CultureInfo.InvariantCulture);
            Pitch = Convert.ToDouble(missionFrameParameters[4], CultureInfo.InvariantCulture);
            Roll = Convert.ToDouble(missionFrameParameters[5], CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Creates a new MissionFrame.
        /// The private constructor + static construction method allows
        ///     for keeping the exception-handling logic for the constructor
        ///     internal to the class.
        /// </summary>
        /// <param name="missionLine">A string describing a single frame in the mission animation.</param>
        /// <returns>Constructed MissionFrame or null if the line is invalid.</returns>
        public static MissionFrame Create(string missionLine)
        {
            try
            {
                return new MissionFrame(missionLine);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public MapPoint ToMapPoint()
        {
            return new MapPoint(Longitude, Latitude, Elevation, SpatialReferences.Wgs84);
        }
    }

    public class StatsDisplayViewController : UIViewController
    {
        private UILabel _altitudeLabel;
        private UILabel _headingLabel;
        private UILabel _pitchLabel;
        private UILabel _rollLabel;
        private UILabel _progressLabel;

        public StatsDisplayViewController()
        {
            Title = "Mission stats";
        }

        public void UpdateStatsDisplay(MissionFrame currentFrame, double missionProgress)
        {
            // Skip if stats display hasn't been shown yet.
            if (_altitudeLabel == null)
            {
                return;
            }

            _altitudeLabel.Text = $"{currentFrame.Elevation:F} m";
            _headingLabel.Text = $"{currentFrame.Heading:F}\u00B0"; // \u00b0 is the degree symbol
            _pitchLabel.Text = $"{currentFrame.Pitch:F}\u00B0";
            _rollLabel.Text = $"{currentFrame.Roll:F}\u00B0";
            _progressLabel.Text = $"{missionProgress * 100:F}%";
        }

        public override void LoadView()
        {
            UIStackView floatContainer = new UIStackView();
            floatContainer.TranslatesAutoresizingMaskIntoConstraints = false;
            floatContainer.Axis = UILayoutConstraintAxis.Horizontal;
            floatContainer.Alignment = UIStackViewAlignment.Top;
            floatContainer.LayoutMarginsRelativeArrangement = true;
            floatContainer.LayoutMargins = new UIEdgeInsets(8, 8, 8, 8);

            UIStackView statsContainer = new UIStackView();
            statsContainer.TranslatesAutoresizingMaskIntoConstraints = false;
            statsContainer.Axis = UILayoutConstraintAxis.Vertical;
            statsContainer.Spacing = 8;

            _altitudeLabel = new UILabel();
            _headingLabel = new UILabel();
            _pitchLabel = new UILabel();
            _rollLabel = new UILabel();
            _progressLabel = new UILabel();

            statsContainer.AddArrangedSubview(GetStatStack("Altitude (meters):", _altitudeLabel));
            statsContainer.AddArrangedSubview(GetStatStack("Heading:", _headingLabel));
            statsContainer.AddArrangedSubview(GetStatStack("Pitch:", _pitchLabel));
            statsContainer.AddArrangedSubview(GetStatStack("Roll:", _rollLabel));
            statsContainer.AddArrangedSubview(GetStatStack("Progress:", _progressLabel));

            floatContainer.AddArrangedSubview(statsContainer);

            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };
            View.AddSubview(floatContainer);

            floatContainer.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            floatContainer.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            floatContainer.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            floatContainer.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
        }

        private UIView GetStatStack(string label, UILabel parameterField)
        {
            UIStackView statStack = new UIStackView();
            statStack.TranslatesAutoresizingMaskIntoConstraints = false;
            statStack.Distribution = UIStackViewDistribution.EqualSpacing;

            UILabel labellabel = new UILabel();
            labellabel.TranslatesAutoresizingMaskIntoConstraints = false;
            labellabel.TextAlignment = UITextAlignment.Left;
            labellabel.Text = label;

            statStack.AddArrangedSubview(labellabel);

            parameterField.TextAlignment = UITextAlignment.Right;
            parameterField.TranslatesAutoresizingMaskIntoConstraints = false;

            statStack.AddArrangedSubview(parameterField);

            return statStack;
        }
    }
}