// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using CoreGraphics;
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using ArcGISRuntime.Samples.Managers;
using UIKit;

namespace ArcGISRuntime.Samples.Animate3DGraphic
{
    [Register("Animate3DGraphic")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("290f0c571c394461a8b58b6775d0bd63", "e87c154fb9c2487f999143df5b08e9b1", "5a9b60cee9ba41e79640a06bcdf8084d", "12509ffdc684437f8f2656b0129d2c13", "681d6f7694644709a7c830ec57a2d72b")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Animate 3D Graphic",
        "GraphicsOverlay",
        "This sample demonstrates how to animate a graphic's position and follow it using a camera controller.",
        "Click-and-drag to pan the sceneview, orbiting the moving plane. Click 'Camera' to toggle between the default and the orbiting camera controller.\nThe plane's route is shown on the inset map in the bottom left corner of the screen. Click 'Stats' to toggle stats display. Tap 'Mission' to choose from a list of alternative routes. \n\nNote that this is a graphics-intensive sample; performance may be degraded in certain situations (such as using a simulator).",
        "Featured")]
    public class Animate3DGraphic : UIViewController
    {
        // Create and hold references to UI controls.
        private readonly MapView _insetMapView = new MapView();
        private readonly SceneView _mySceneView = new SceneView {AtmosphereEffect = AtmosphereEffect.Realistic};
        private readonly UIToolbar _controlToolbox = new UIToolbar();
        private readonly UIButton _missionControlButton = new UIButton();
        private readonly UIButton _cameraControlButton = new UIButton();
        private readonly UIButton _statsControlButton = new UIButton();
        private readonly UIButton _playButton = new UIButton();
        private readonly UILabel _altitudeLabel = new UILabel();
        private readonly UILabel _headingLabel = new UILabel();
        private readonly UILabel _pitchLabel = new UILabel();
        private readonly UILabel _rollLabel = new UILabel();
        private readonly UILabel _progressLabel = new UILabel();
        private readonly UILabel _altitudeLabelLabel = new UILabel {Text = "Altitude:"};
        private readonly UILabel _headingLabelLabel = new UILabel {Text = "Heading:"};
        private readonly UILabel _pitchLabelLabel = new UILabel {Text = "Pitch:"};
        private readonly UILabel _rollLabelLabel = new UILabel {Text = "Roll:"};
        private readonly UILabel _progressLabelLabel = new UILabel {Text = "Progress:"};
        private readonly UIToolbar _statsFrame = new UIToolbar();

        // List of labels; this simplifies the code for adding and removing the labels from the layout.
        private List<UILabel> _statsLabels;

        // URL to the elevation service - provides terrain elevation.
        private readonly Uri _elevationServiceUrl = new Uri("http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

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

        // Flags for the toggle-able states (controls when stats are shown and when the orbit camera is used).
        private bool _showStats;

        // Flag to control which camera will be used.
        private bool _shouldFollowPlane = true;

        // Set the title of the sample.
        public Animate3DGraphic()
        {
            Title = "Animate 3D Graphic";
        }

        private async void Initialize()
        {
            // Apply appropriate maps to the scene and the inset map view.
            _insetMapView.Map = new Map(Basemap.CreateImagery());
            _insetMapView.IsAttributionTextVisible = false;
            _mySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Apply the elevation source.
            Surface surface = new Surface();
            ElevationSource elevationSource = new ArcGISTiledElevationSource(_elevationServiceUrl);
            surface.ElevationSources.Add(elevationSource);
            _mySceneView.Scene.BaseSurface = surface;

            // Create and add the graphics overlay.
            GraphicsOverlay sceneOverlay = new GraphicsOverlay
            {
                SceneProperties = {SurfacePlacement = SurfacePlacement.Absolute}
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
            GraphicsOverlay insetMapOperlay = new GraphicsOverlay
            {
                Renderer = renderer2D
            };
            _insetMapView.GraphicsOverlays.Add(insetMapOperlay);

            // Create placeholder graphic for showing the mission route in the inset map.
            SimpleLineSymbol routeSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 2);
            _routeGraphic = new Graphic {Symbol = routeSymbol};
            insetMapOperlay.Graphics.Add(_routeGraphic);

            // Create the plane graphic; this is symbolized as a blue triangle because of renderer implemented above.
            Dictionary<string, object> plane2DAttributes = new Dictionary<string, object>
            {
                // Set the angle for the plane graphic.
                ["ANGLE"] = 0f
            };
            // Create the graphic from the attributes and the initial point.
            _plane2D = new Graphic(new MapPoint(0, 0, SpatialReferences.Wgs84), plane2DAttributes);

            // Add the plane graphic to the inset map via the overlay.
            insetMapOperlay.Graphics.Add(_plane2D);

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

            // Call the animation method every time the timer expires (once every 60ms per above).
            _animationTimer.Elapsed += (sender, args) => AnimatePlane();

            // Set the initial mission for when the sample loads.
            await ChangeMission(_missionToItemId.Keys.First());
        }

        private void ShowMissionOptions(object sender, EventArgs eventArgs)
        {
            // Create the view controller that will present the list of missions.
            UIAlertController missionSelectionAlert = UIAlertController.Create("Select a mission", "", UIAlertControllerStyle.ActionSheet);

            // Needed to prevent a crash on iPad.
            UIPopoverPresentationController presentationPopover = missionSelectionAlert.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Add an option for each mission.
            foreach (string item in _missionToItemId.Keys)
            {
                // Selecting the mission will call the ChangeMission method.
                missionSelectionAlert.AddAction(UIAlertAction.Create(item, UIAlertActionStyle.Default, async action => await ChangeMission(item)));
            }

            // Show the alert.
            PresentViewController(missionSelectionAlert, true, null);
        }

        private void ToggleStatsDisplay()
        {
            // Toggle the stats display field.
            _showStats = !_showStats;

            if (_showStats)
            {
                View.AddSubview(_statsFrame);
            }
            else
            {
                _statsFrame.RemoveFromSuperview();
            }

            // Either show or hide each label.
            foreach (UILabel label in _statsLabels)
            {
                if (_showStats)
                {
                    View.AddSubviews(label);
                }
                else
                {
                    label.RemoveFromSuperview();
                }
            }
        }

        private async Task ChangeMission(string mission)
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

            // Update the inset map's scale.
            await _insetMapView.SetViewpointScaleAsync(100000);

            // Update animation parameters.
            _frameCount = _missionData.Length;
            _keyframe = 0;

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
            double missionProgress = _keyframe / (double) _frameCount;

            // This is needed because the event could be running on a non-UI thread.
            InvokeOnMainThread(() =>
            {
                // Update stats display.
                _altitudeLabel.Text = currentFrame.Elevation.ToString("F");
                _headingLabel.Text = currentFrame.Heading.ToString("F");
                _pitchLabel.Text = currentFrame.Pitch.ToString("F");
                _rollLabel.Text = currentFrame.Roll.ToString("F");
                _progressLabel.Text = $"{missionProgress * 100:F}%";
            });

            // Update plane's position.
            _plane3D.Geometry = currentFrame.ToMapPoint();
            _plane3D.Attributes["HEADING"] = currentFrame.Heading;
            _plane3D.Attributes["PITCH"] = currentFrame.Pitch;
            _plane3D.Attributes["ROLL"] = currentFrame.Roll;

            // Update the inset map; plane symbol position.
            _plane2D.Geometry = currentFrame.ToMapPoint();

            // Update inset's viewpoint and heading.
            Viewpoint vp = new Viewpoint(currentFrame.ToMapPoint(), _insetMapView.MapScale,
                360 + (float) currentFrame.Heading);
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

        private void TogglePlayMission()
        {
            if (_playButton.Title(UIControlState.Normal) == "Play")
            {
                // Resume playing.
                _playButton.SetTitle("Pause", UIControlState.Normal);
                _animationTimer.Start();
            }
            else
            {
                // Pause.
                _playButton.SetTitle("Play", UIControlState.Normal);
                _animationTimer.Stop();
            }
        }

        private void ToggleFollowPlane()
        {
            // Update the flag.
            _shouldFollowPlane = !_shouldFollowPlane;

            // Setting the camera controller to null resets it to the default.
            // If should follow is true, the orbit camera controller will be used.
            _mySceneView.CameraController = _shouldFollowPlane ? _orbitCameraController : null;
        }

        public override void ViewDidLoad()
        {
            CreateLayout();
            Initialize();
            base.ViewDidLoad();
        }

        private void CreateLayout()
        {
            // Set the titles for the primary buttons.
            _missionControlButton.SetTitle("Mission", UIControlState.Normal);
            _cameraControlButton.SetTitle("Camera", UIControlState.Normal);
            _statsControlButton.SetTitle("Stats", UIControlState.Normal);
            _playButton.SetTitle("Pause", UIControlState.Normal);
            _missionControlButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _cameraControlButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _statsControlButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _playButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            // Allow for selecting a mission.
            _missionControlButton.TouchUpInside += ShowMissionOptions;

            // Allow for pausing the mission.
            _playButton.TouchUpInside += (sender, args) => TogglePlayMission();

            // Allow for toggling display of statistics.
            _statsControlButton.TouchUpInside += (sender, args) => ToggleStatsDisplay();

            // Allow for toggling the camera.
            _cameraControlButton.TouchUpInside += (sender, args) => ToggleFollowPlane();

            // Populate the list of labels (simplifies the process of adding and removing the labels from the UI).
            _statsLabels = new List<UILabel>
            {
                _altitudeLabel,
                _altitudeLabelLabel,
                _pitchLabel,
                _pitchLabelLabel,
                _rollLabel,
                _rollLabelLabel,
                _headingLabel,
                _headingLabelLabel,
                _progressLabel,
                _progressLabelLabel
            };

            // Add views to page.
            // Only views that are visible by default are included.
            View.AddSubviews(_mySceneView, _insetMapView, _controlToolbox, _missionControlButton, _statsControlButton, _cameraControlButton, _playButton);
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat margin = 5;
                nfloat labelHeight = 20;
                nfloat controlHeight = 30;
                nfloat toolbarHeight = controlHeight + 2 * margin;

                // Update the map frames.
                _mySceneView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _insetMapView.Frame = new CGRect(10, View.Bounds.Height - toolbarHeight - 35 - 100, 100, 100);

                // Update the control layout.
                _controlToolbox.Frame = new CGRect(0, View.Bounds.Height - 40, View.Bounds.Width, 40);
                nfloat controlWidth = (View.Bounds.Width - 20) / 4;
                nfloat controlYOffset = View.Bounds.Height - controlHeight - margin;
                _missionControlButton.Frame = new CGRect(margin, controlYOffset, controlWidth, controlHeight);
                _cameraControlButton.Frame = new CGRect(_missionControlButton.Frame.Right + margin, controlYOffset, controlWidth, controlHeight);
                _statsControlButton.Frame = new CGRect(_cameraControlButton.Frame.Right + margin, controlYOffset, controlWidth, controlHeight);
                _playButton.Frame = new CGRect(_statsControlButton.Frame.Right + margin, controlYOffset, controlWidth, controlHeight);

                // Layout stats display.
                nfloat halfWidth = View.Bounds.Width / 2;
                _altitudeLabel.Frame = new CGRect(halfWidth, topMargin + margin, halfWidth, labelHeight);
                _headingLabel.Frame = new CGRect(halfWidth, topMargin + labelHeight + 2 * margin, halfWidth, labelHeight);
                _pitchLabel.Frame = new CGRect(halfWidth, topMargin + 2 * labelHeight + 3 * margin, halfWidth, labelHeight);
                _rollLabel.Frame = new CGRect(halfWidth, topMargin + 3 * labelHeight + 4 * margin, halfWidth, labelHeight);
                _progressLabel.Frame = new CGRect(halfWidth, topMargin + 4 * labelHeight + 5 * margin, halfWidth, labelHeight);

                // Layout stats display labels.
                _altitudeLabelLabel.Frame = new CGRect(10, topMargin + margin, halfWidth - 10, 20);
                _headingLabelLabel.Frame = new CGRect(10, topMargin + labelHeight + 2 * margin, halfWidth - 10, 20);
                _pitchLabelLabel.Frame = new CGRect(10, topMargin + 2 * labelHeight + 3 * margin, halfWidth - 10, 20);
                _rollLabelLabel.Frame = new CGRect(10, topMargin + 3 * labelHeight + 4 * margin, halfWidth - 10, 20);
                _progressLabelLabel.Frame = new CGRect(10, topMargin + 4 * labelHeight + 5 * margin, halfWidth - 10, 20);

                _statsFrame.Frame = new CGRect(0, topMargin, View.Bounds.Width, 5 * labelHeight + 6 * margin);

                _mySceneView.ViewInsets = new UIEdgeInsets(topMargin, 0, 40, 0);

                base.ViewDidLayoutSubviews();
            }
            catch (NullReferenceException)
            {
            }
        }

        /// <summary>
        /// Private helper class represents a single frame in the animation.
        /// </summary>
        private class MissionFrame
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
            private MissionFrame(string missionLine)
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
                Longitude = Convert.ToDouble(missionFrameParameters[0]);
                Latitude = Convert.ToDouble(missionFrameParameters[1]);
                Elevation = Convert.ToDouble(missionFrameParameters[2]);
                Heading = Convert.ToDouble(missionFrameParameters[3]);
                Pitch = Convert.ToDouble(missionFrameParameters[4]);
                Roll = Convert.ToDouble(missionFrameParameters[5]);
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
    }
}