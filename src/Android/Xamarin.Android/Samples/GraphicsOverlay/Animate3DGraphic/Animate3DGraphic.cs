// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using ArcGISRuntimeXamarin.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Debug = System.Diagnostics.Debug;
using Surface = Esri.ArcGISRuntime.Mapping.Surface;

namespace ArcGISRuntimeXamarin.Samples.Animate3DGraphic
{
    [Activity]
    public class Animate3DGraphic : Activity
    {
        // Hold references to UI components so that they can be accessed by the sample programmatically
        //     These will be populated from the AndroidLayout file in CreateLayout
        private Button _missionButton;
        private Button _cameraButton;
        private Button _playButton;
        private Button _statsButton;

        private TextView _altitudeTextView;
        private TextView _headingTextView;
        private TextView _pitchTextView;
        private TextView _rollTextView;

        private SeekBar _missionProgressBar;

        private SceneView _mySceneView;
        private MapView _insetMapView;

        // URL to the elevation service - provides terrain elevation
        private readonly Uri _elevationServiceUrl = new Uri("http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");

        // Graphic for highlighting the route in the inset map
        private Graphic _routeGraphic;

        // Graphic for highlighting the airplane in the inset map
        private Graphic _plane2D;

        // Graphic for showing the 3D plane model in the scene
        private Graphic _plane3D;

        // Camera controller for centering the camera on the airplane
        private OrbitGeoElementCameraController _orbitCameraController;

        // Timer enables frame-by-frame animation
        private Timer _animationTimer;

        // Number of frames in the mission animation
        private int _frameCount;

        // Index of current frame in the animation
        private int _keyframe;

        // Dictionary of mission file names and the corresponding portal item IDs
        private readonly Dictionary<string, string> _missionToItemId = new Dictionary<string, string>
        {
            {"GrandCanyon","290f0c571c394461a8b58b6775d0bd63"},
            {"Hawaii", "e87c154fb9c2487f999143df5b08e9b1"},
            {"Pyrenees", "5a9b60cee9ba41e79640a06bcdf8084d"},
            {"Snowdon", "12509ffdc684437f8f2656b0129d2c13"}
        };

        // Array of frames for the current mission
        private MissionFrame[] _missionData;
        private bool _shouldFollowPlane = true;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Animate 3D Graphic";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Apply appropriate maps to the scene and the inset map view
            _insetMapView.Map = new Map(Basemap.CreateImagery());
            _mySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Apply the elevation source
            Surface surface = new Surface();
            ElevationSource elevationSource = new ArcGISTiledElevationSource(_elevationServiceUrl);
            surface.ElevationSources.Add(elevationSource);
            _mySceneView.Scene.BaseSurface = surface;

            // Create and add the graphics overlay
            GraphicsOverlay sceneOverlay = new GraphicsOverlay
            {
                SceneProperties = { SurfacePlacement = SurfacePlacement.Absolute }
            };
            _mySceneView.GraphicsOverlays.Add(sceneOverlay);

            // Create a renderer to handle updating plane's orientation
            SimpleRenderer renderer3D = new SimpleRenderer();
            RendererSceneProperties renderProperties = renderer3D.SceneProperties;
            // Use expressions to keep the renderer properties updated as parameters of the rendered object
            renderProperties.HeadingExpression = "[HEADING]";
            renderProperties.PitchExpression = "[PITCH]";
            renderProperties.RollExpression = "[ROLL]";
            // Apply the renderer to the scene view's overlay
            sceneOverlay.Renderer = renderer3D;

            // Create renderer to symbolize plane and update plane orientation in the inset map
            SimpleRenderer renderer2D = new SimpleRenderer();
            // Create the symbol that will be used for the plane
            SimpleMarkerSymbol plane2DSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, System.Drawing.Color.Blue, 10);
            // Apply the symbol to the renderer
            renderer2D.Symbol = plane2DSymbol;
            // Apply a rotation expression to the renderer
            renderer2D.RotationExpression = "[ANGLE]";
            // Update the inset map with a new GraphicsOverlay based on the renderer
            GraphicsOverlay insetMapOperlay = new GraphicsOverlay
            {
                Renderer = renderer2D
            };
            _insetMapView.GraphicsOverlays.Add(insetMapOperlay);

            // Create placeholder graphic for showing the mission route in the inset map
            SimpleLineSymbol routeSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 2);
            _routeGraphic = new Graphic { Symbol = routeSymbol };
            insetMapOperlay.Graphics.Add(_routeGraphic);

            // Create the plane graphic; this is symbolized as a blue triangle because of renderer implemented above
            // Create the attribute dictionary
            Dictionary<string, object> plane2DAttributes = new Dictionary<string, object>();
            // Set the angle for the plane graphic
            plane2DAttributes["ANGLE"] = 0f;
            // Create the graphic from the attributes and the initial point
            _plane2D = new Graphic(new MapPoint(0, 0, SpatialReferences.Wgs84), plane2DAttributes);
            // Add the plane graphic to the inset map via the overlay
            insetMapOperlay.Graphics.Add(_plane2D);

            // Create the model graphic for the plane
            // Get the path to the 3D model
            string modelPath = await GetModelPath();
            // Create the scene symbol from the path to the model
            ModelSceneSymbol plane3DSymbol = await ModelSceneSymbol.CreateAsync(new Uri(modelPath), 1.0);
            // Create the graphic with an initial location and the plane symbol
            _plane3D = new Graphic(new MapPoint(0, 0, 0, SpatialReferences.Wgs84), plane3DSymbol);
            // Add the plane to the overlay
            sceneOverlay.Graphics.Add(_plane3D);

            // Create the orbit camera controller to follow the plane
            _orbitCameraController = new OrbitGeoElementCameraController(_plane3D, 20.0)
            {
                CameraPitchOffset = 75.0
            };
            _mySceneView.CameraController = _orbitCameraController;

            // Create a timer; this will enable animating the plane
            _animationTimer = new Timer(60)
            {
                Enabled = true,
                AutoReset = true
            };

            // Move the plane every time the timer expires
            _animationTimer.Elapsed += AnimatePlane;

            // Set the initial mission for when the sample loads
            await ChangeMission(_missionToItemId.Keys.First());
        }

        private async Task ChangeMission(string mission)
        {
            // Stop animating the current mission
            _animationTimer.Stop();

            // Get mission data
            _missionData = await GetMissionData(mission);

            // Draw mission route on the inset
            // Create a collection of points to hold the mission
            PointCollection points = new PointCollection(SpatialReferences.Wgs84);
            // Add all of the points from the mission to the point collection
            points.AddPoints(_missionData.Select(m => m.ToMapPoint()));
            // Create a polyline to symbolize the route from the point collection
            Polyline route = new Polyline(points);
            // Update the route graphic's geometry with the newly created route polyline
            _routeGraphic.Geometry = route;
            // Update the inset map's scale
            await _insetMapView.SetViewpointScaleAsync(100000);

            // Update animation parameters
            _frameCount = _missionData.Length;
            _keyframe = 0;

            // Restart the animation
            _animationTimer.Start();
        }

        private async Task<MissionFrame[]> GetMissionData(string mission)
        {
            // Get the path to the file
            string filePath = await GetMissionFilePath(mission);

            // Read the file text
            string fileContents = File.ReadAllText(filePath);

            // Split the file contents into a list of lines
            return fileContents.Split('\n')
                // Then for each line, create a MissionFrame object
                .Select(MissionFrame.Create)
                // Then remove any null MissionFrames
                .Where(missionPart => missionPart != null)
                // Finally return that list of MissionFrames as an array
                .ToArray();
        }

        private async Task<string> GetMissionFilePath(string mission)
        {
            string itemId = _missionToItemId[mission];
            string filename = mission + ".csv";

            #region offlinedata

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "Animate3DGraphic", filename);

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the map package file
                await DataManager.GetData(itemId, "Animate3DGraphic");
            }
            return filepath;

            #endregion offlinedata
        }

        private void AnimatePlane(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            // Get the next position; % prevents going out of bounds even if the keyframe value is
            //     changed unexpectedly (e.g. due to user interaction with the progress slider).
            MissionFrame currentFrame = _missionData[_keyframe % _frameCount];

            // Update the UI
            double missionProgress = _keyframe / (double)_frameCount;

            // This is needed because the event could be running on a non-UI thread
            RunOnUiThread(() =>
            {
                // Update the progress slider
                _missionProgressBar.Progress = (int)(missionProgress * 100);

                // Update stats display
                _altitudeTextView.Text = currentFrame.Elevation.ToString("F");
                _headingTextView.Text = currentFrame.Heading.ToString("F");
                _pitchTextView.Text = currentFrame.Pitch.ToString("F");
                _rollTextView.Text = currentFrame.Roll.ToString("F");
            });

            // Update plane's position
            _plane3D.Geometry = currentFrame.ToMapPoint();
            _plane3D.Attributes["HEADING"] = currentFrame.Heading;
            _plane3D.Attributes["PITCH"] = currentFrame.Pitch;
            _plane3D.Attributes["ROLL"] = currentFrame.Roll;

            // Update the inset map; plane symbol position
            _plane2D.Geometry = currentFrame.ToMapPoint();
            // Update inset's viewpoint and heading
            Viewpoint vp = new Viewpoint(currentFrame.ToMapPoint(), _insetMapView.MapScale,
                360 + (float)currentFrame.Heading);
            _insetMapView.SetViewpoint(vp);

            // Update the keyframe. This advances the animation
            _keyframe++;

            // Restart the animation if it has finished
            if (_keyframe >= _frameCount)
            {
                _keyframe = 0;
            }
        }

        private async Task<string> GetModelPath()
        {
            #region offlinedata

            // The desired model is expected to be called Bristol.dae
            string filename = "Bristol.dae";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "Animate3DGraphic", filename);

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the map package file
                await DataManager.GetData("681d6f7694644709a7c830ec57a2d72b", "Animate3DGraphic");
            }
            return filepath;

            #endregion offlinedata
        }

        private void MissionPlayPlauseClick(object sender, EventArgs e)
        {
            // Get a reference to the button that sent the event
            Button playButton = (Button)sender;

            // Get the text of the button
            string playtext = playButton.Text.ToString();

            switch (playtext)
            {
                // Resume the animation
                case "Play":
                    playButton.Text = "Pause";
                    _animationTimer.Start();
                    break;
                // Stop the animation
                case "Pause":
                    playButton.Text = "Play";
                    _animationTimer.Stop();
                    break;
            }
        }

        private void MissionProgressOnSeek(object sender, EventArgs e)
        {
            // Get a reference to the slider that sent the event
            SeekBar sliderControl = (SeekBar)sender;
            if (!sliderControl.HasFocus)
            {
                return;
            }

            // Return if the user didn't change the progress
            //    (this event is also fired when the value is changed programmatically)
            if (!sliderControl.IsFocused)
            {
                return;
            }

            // Stop the animation
            _animationTimer.Stop();

            // Get the new mission progress
            double missionProgress = sliderControl.Progress / 100.0;

            // Update the keyframe based on the progress
            _keyframe = (int)(missionProgress * _frameCount);

            // Restart the animation
            _animationTimer.Start();
        }

        private void ToggleFollowPlane()
        {
            _shouldFollowPlane = !_shouldFollowPlane;
            _mySceneView.CameraController = _shouldFollowPlane ? _orbitCameraController : null;
        }

        private void CreateLayout()
        {
            SetContentView(Resource.Layout.Animate3DGraphic);
            _insetMapView = FindViewById<MapView>(Resource.Id.insetMap);
            _mySceneView = FindViewById<SceneView>(Resource.Id.PrimarySceneView);
            _insetMapView.IsAttributionTextVisible = false;
            _altitudeTextView = FindViewById<TextView>(Resource.Id.planeAltitudeText);
            _headingTextView = FindViewById<TextView>(Resource.Id.planeHeadingText);
            _pitchTextView = FindViewById<TextView>(Resource.Id.planePitchText);
            _rollTextView = FindViewById<TextView>(Resource.Id.planeRollText);
            _missionProgressBar = FindViewById<SeekBar>(Resource.Id.missionProgressSeekBar);
            _cameraButton = FindViewById<Button>(Resource.Id.cameraButton);
            _missionButton = FindViewById<Button>(Resource.Id.missionButton);
            _statsButton = FindViewById<Button>(Resource.Id.statsButton);
            _playButton = FindViewById<Button>(Resource.Id.playPauseButton);
            LinearLayout statsLayout = FindViewById<LinearLayout>(Resource.Id.statsPanelLayout);

            _cameraButton.Click += (sender, args) => ToggleFollowPlane();
            _missionProgressBar.ProgressChanged += MissionProgressOnSeek;
            _playButton.Click += MissionPlayPlauseClick;
            _missionButton.Click += MissionButtonOnClick;
            _statsButton.Click += (sender, args) =>
            {
                if (statsLayout.Visibility == ViewStates.Visible)
                {
                    statsLayout.Visibility = ViewStates.Invisible;
                }
                else
                {
                    statsLayout.Visibility = ViewStates.Visible;
                }
            };
        }

        private void MissionButtonOnClick(object o, EventArgs eventArgs)
        {
            string[] missions = _missionToItemId.Keys.ToArray();
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("Select a Mission");
            builder.SetItems(missions, async (sender, args) => { await ChangeMission(missions[args.Which]);});
            builder.Show();
        }

        /// <summary>
        /// Private helper class represents a single frame in the animation
        /// </summary>
        private class MissionFrame
        {
            private double Longitude { get;}
            private double Latitude { get; }
            public double Elevation { get; }
            public double Heading { get; }
            public double Pitch { get; }
            public double Roll { get; }

            /// <summary>
            /// Private constructor ensures that only the factory method (Create) can be called.
            /// </summary>
            /// <param name="missionLine">A string describing a single frame in the mission animation.</param>
            private MissionFrame(string missionLine)
            {
                // Split the string into a list of entries (columns)
                string[] missionFrameParameters = missionLine.Split(',');

                // Throw if the line isn't valid
                if (missionFrameParameters.Length != 6)
                {
                    throw new Exception("Invalid mission part definition");
                }

                // Populate the object's properties from the array of parameters
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
            /// <param name="missionLine">A string describing a single frame in the mission animation</param>
            /// <returns>Constructed MissionFrame or null if the line is invalid</returns>
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
