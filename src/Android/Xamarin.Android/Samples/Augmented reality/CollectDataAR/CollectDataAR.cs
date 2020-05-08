// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using ArcGISRuntimeXamarin.Samples.ARToolkit.Controls;
using Esri.ArcGISRuntime.ARToolkit;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Java.Nio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Surface = Esri.ArcGISRuntime.Mapping.Surface;

namespace ArcGISRuntimeXamarin.Samples.CollectDataAR
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Collect data in AR",
        category: "Augmented reality",
        description: "Tap on real-world objects to collect data.",
        instructions: "Before you start, go through the on-screen calibration process to ensure accurate positioning of recorded features.",
        tags: new[] { "attachment", "augmented reality", "capture", "collection", "collector", "data", "field", "field worker", "full-scale", "mixed reality", "survey", "world-scale" })]
    public class CollectDataAR : Activity, IDialogInterfaceOnCancelListener
    {
        // Hold references to UI controls.
        private ARSceneView _arView;
        private TextView _helpLabel;
        private Button _calibrateButton;
        private Button _addButton;
        private Button _roamingButton;
        private Button _localButton;
        private View _calibrationView;
        private JoystickSeekBar _headingSlider;
        private JoystickSeekBar _altitudeSlider;

        // Scene content.
        private ArcGISTiledElevationSource _elevationSource;
        private Surface _elevationSurface;
        private Scene _scene;

        // Track when user is changing between AR and GPS localization.
        private bool _changingScale;

        // Create a new copmletion source for the prompt.
        private TaskCompletionSource<int> _healthCompletionSource;

        // Feature table for collected data about trees.
        private ServiceFeatureTable _featureTable = new ServiceFeatureTable(new Uri("https://services2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/rest/services/AR_Tree_Survey/FeatureServer/0"));

        // Graphics for tapped points in the scene.
        private GraphicsOverlay _graphicsOverlay;
        private SimpleMarkerSceneSymbol _tappedPointSymbol = new SimpleMarkerSceneSymbol(SimpleMarkerSceneSymbolStyle.Diamond, System.Drawing.Color.Orange, 0.5, 0.5, 0.5, SceneSymbolAnchorPosition.Center);

        // Custom location data source that enables calibration and returns values relative to mean sea level rather than the WGS84 ellipsoid.
        private MSLAdjustedARLocationDataSource _locationDataSource;

        // Calibration state fields.
        private bool _isCalibrating;
        private double _altitudeOffset;

        // Permissions and permission request.
        private readonly string[] _requestedPermissions = { Manifest.Permission.AccessFineLocation };
        private const int requestCode = 35;

        private void RequestPermissions()
        {
            if (ContextCompat.CheckSelfPermission(this, _requestedPermissions[0]) == Permission.Granted)
            {
                Initialize();
            }
            else
            {
                ActivityCompat.RequestPermissions(this, _requestedPermissions, CollectDataAR.requestCode);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == CollectDataAR.requestCode && grantResults[0] == Permission.Granted)
            {
                Initialize();
            }
            else
            {
                Toast.MakeText(this, "Location permissions needed for this sample", ToastLength.Short).Show();
            }
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private bool IsCalibrating
        {
            get
            {
                return _isCalibrating;
            }
            set
            {
                _isCalibrating = value;
                if (_isCalibrating)
                {
                    // Show the surface semitransparent for calibration.
                    _scene.BaseSurface.Opacity = 0.5;

                    // Enable scene interaction.
                    _arView.InteractionOptions.IsEnabled = true;
                    _calibrationView.Visibility = ViewStates.Visible;
                }
                else
                {
                    // Hide the scene when not calibrating.
                    _scene.BaseSurface.Opacity = 0;

                    // Disable scene interaction.
                    _arView.InteractionOptions.IsEnabled = false;
                    _calibrationView.Visibility = ViewStates.Gone;
                }
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Title = "Collect data in AR";

            // Create the layout.
            CreateLayout();

            // Request permissions for location.
            RequestPermissions();
        }

        private void CreateLayout()
        {
            // Load the view.
            SetContentView(ArcGISRuntime.Resource.Layout.CollectDataAR);

            // Set up control references.
            _arView = FindViewById<ARSceneView>(ArcGISRuntime.Resource.Id.arView);
            _helpLabel = FindViewById<TextView>(ArcGISRuntime.Resource.Id.helpLabel);
            _calibrateButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.calibrateButton);
            _addButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.addTreeButton);
            _roamingButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.roamingButton);
            _localButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.localButton);
            _calibrationView = FindViewById(ArcGISRuntime.Resource.Id.calibrationView);
            _headingSlider = FindViewById<JoystickSeekBar>(ArcGISRuntime.Resource.Id.headingJoystick);
            _altitudeSlider = FindViewById<JoystickSeekBar>(ArcGISRuntime.Resource.Id.altitudeJoystick);

            // Disable plane rendering and visualization.
            _arView.ArSceneView.PlaneRenderer.Enabled = false;
            _arView.ArSceneView.PlaneRenderer.Visible = false;

            // Configure button click events.
            _addButton.Click += AddButtonPressed;
            _calibrateButton.Click += (o, e) => IsCalibrating = !IsCalibrating;
            _roamingButton.Click += (o, e) => RealScaleValueChanged(true);
            _localButton.Click += (o, e) => RealScaleValueChanged(false);

            // Configure calibration sliders.
            _headingSlider.DeltaProgressChanged += HeadingSlider_DeltaProgressChanged;
            _altitudeSlider.DeltaProgressChanged += AltitudeSlider_DeltaProgressChanged;
        }

        private void AltitudeSlider_DeltaProgressChanged(object sender, DeltaChangedEventArgs e)
        {
            // Add the new value to the existing altitude offset.
            _altitudeOffset += e.DeltaProgress;

            // Update the altitude offset on the custom location data source.
            _locationDataSource.AltitudeOffset = _altitudeOffset;
        }

        private void HeadingSlider_DeltaProgressChanged(object sender, DeltaChangedEventArgs e)
        {
            // Get the old camera.
            Camera camera = _arView.OriginCamera;

            // Calculate the new heading by applying the offset to the old camera's heading.
            double heading = camera.Heading + e.DeltaProgress;

            // Create a new camera by rotating the old camera to the new heading.
            Camera newCamera = camera.RotateTo(heading, camera.Pitch, camera.Roll);

            // Use the new camera as the origin camera.
            _arView.OriginCamera = newCamera;
        }

        private async void RealScaleValueChanged(bool roaming)
        {
            // Prevent this from being called concurrently
            if (_changingScale)
            {
                return;
            }
            _changingScale = true;

            // Disable the associated UI controls while switching.
            _roamingButton.Enabled = false;
            _localButton.Enabled = false;

            // Check if using roaming for AR location mode.
            if (roaming)
            {
                await _arView.StopTrackingAsync();

                // Start AR tracking using a continuous GPS signal.
                await _arView.StartTrackingAsync(ARLocationTrackingMode.Continuous);
                _altitudeSlider.Enabled = true;
                _localButton.Enabled = true;
            }
            else
            {
                await _arView.StopTrackingAsync();

                // Start AR tracking without using a GPS signal.
                await _arView.StartTrackingAsync(ARLocationTrackingMode.Ignore);
                _altitudeSlider.Enabled = false;
                _roamingButton.Enabled = true;
            }
            _changingScale = false;
        }

        private void Initialize()
        {
            Toast.MakeText(this,
                "Calibrate your location before collecting data!",
                ToastLength.Long).Show();

            // Create the custom location data source and configure the AR scene view to use it.
            _locationDataSource = new MSLAdjustedARLocationDataSource(this);
            _locationDataSource.AltitudeMode = MSLAdjustedARLocationDataSource.AltitudeAdjustmentMode.NmeaParsedMsl;
            _arView.LocationDataSource = _locationDataSource;

            // Create the scene and show it.
            _scene = new Scene(Basemap.CreateImagery());
            _arView.Scene = _scene;

            // Create and add the elevation surface.
            _elevationSource = new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));
            _elevationSurface = new Surface();
            _elevationSurface.ElevationSources.Add(_elevationSource);
            _arView.Scene.BaseSurface = _elevationSurface;

            // Hide the surface in AR.
            _elevationSurface.NavigationConstraint = NavigationConstraint.None;
            _elevationSurface.Opacity = 0;

            // Configure the space and atmosphere effects for AR.
            _arView.SpaceEffect = SpaceEffect.None;
            _arView.AtmosphereEffect = AtmosphereEffect.None;

            // Add a graphics overlay for displaying points in AR.
            _graphicsOverlay = new GraphicsOverlay();
            _graphicsOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            _graphicsOverlay.Renderer = new SimpleRenderer(_tappedPointSymbol);
            _arView.GraphicsOverlays.Add(_graphicsOverlay);

            // Add the exisiting features to the scene.
            FeatureLayer treeLayer = new FeatureLayer(_featureTable);
            treeLayer.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            _arView.Scene.OperationalLayers.Add(treeLayer);

            // Add the event for the user tapping the screen.
            _arView.GeoViewTapped += arViewTapped;

            // Disable scene interaction.
            _arView.InteractionOptions = new SceneViewInteractionOptions() { IsEnabled = false };

            // Enable the calibrate button.
            _calibrateButton.Enabled = true;
        }

        private void arViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Don't add features when calibrating the AR view.
            if (_isCalibrating)
            {
                return;
            }

            // Try to get the real-world position of that tapped AR plane.
            MapPoint planeLocation = _arView.ARScreenToLocation(e.Position);

            // Remove any existing graphics.
            _graphicsOverlay.Graphics.Clear();

            // Check if a Map Point was identified.
            if (planeLocation != null)
            {
                // Add a graphic at the tapped location.
                _graphicsOverlay.Graphics.Add(new Graphic(planeLocation));
                _addButton.Enabled = true;
                _helpLabel.Text = "Placed relative to ARCore plane";
            }
            else
            {
                ShowMessage("Didn't find anything, try again.", "Error");
                _addButton.Enabled = false;
            }
        }

        private async void AddButtonPressed(object sender, EventArgs e)
        {
            // Check if the user has already tapped a point.
            if (!_graphicsOverlay.Graphics.Any())
            {
                ShowMessage("Didn't find anything, try again.", "Error");
                return;
            }
            try
            {
                // Prevent the user from changing the tapped feature.
                _arView.GeoViewTapped -= arViewTapped;

                // Prompt the user for the health value of the tree.
                int healthValue = await GetTreeHealthValue();

                // Get the camera image for the frame.
                Image capturedImage = _arView.ArSceneView.ArFrame.AcquireCameraImage();

                if (capturedImage != null)
                {
                    // Create a new ArcGIS feature and add it to the feature service.
                    await CreateFeature(capturedImage, healthValue);
                }
                else
                {
                    ShowMessage("Didn't get image for tap.", "Error");
                }
            }
            // This exception is thrown when the user cancels out of the prompt.
            catch (TaskCanceledException)
            {
                return;
            }
            finally
            {
                // Restore the event listener for adding new features.
                _arView.GeoViewTapped += arViewTapped;
            }
        }

        private async Task<int> GetTreeHealthValue()
        {
            // Create UI for tree health selection.
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("How healthy is this tree?");
            builder.SetItems(new string[] { "Dead", "Distressed", "Healthy" }, Choose_Click);
            builder.SetOnCancelListener(this);
            builder.Show();

            // Get the selected terminal.
            _healthCompletionSource = new TaskCompletionSource<int>();
            int selectedIndex = await _healthCompletionSource.Task;

            // Return a tree health value based on the users selection.
            switch (selectedIndex)
            {
                case 0: // Dead tree.
                    return 0;

                case 1: // Distressed tree.
                    return 5;

                case 2: // Healthy tree.
                    return 10;

                default:
                    return 0;
            }
        }

        private void Choose_Click(object sender, DialogClickEventArgs e)
        {
            _healthCompletionSource.TrySetResult(e.Which);
        }

        public void OnCancel(IDialogInterface dialog)
        {
            _healthCompletionSource.TrySetCanceled();
        }

        private async Task CreateFeature(Image capturedImage, int healthValue)
        {
            _helpLabel.Text = "Adding feature...";

            try
            {
                // Get the geometry of the feature.
                MapPoint featurePoint = _graphicsOverlay.Graphics.First().Geometry as MapPoint;

                // Create attributes for the feature using the user selected health value.
                IEnumerable<KeyValuePair<string, object>> featureAttributes = new Dictionary<string, object>() { { "Health", (short)healthValue }, { "Height", 3.2 }, { "Diameter", 1.2 } };

                // Ensure that the feature table is loaded.
                if (_featureTable.LoadStatus != Esri.ArcGISRuntime.LoadStatus.Loaded)
                {
                    await _featureTable.LoadAsync();
                }

                // Create the new feature
                ArcGISFeature newFeature = _featureTable.CreateFeature(featureAttributes, featurePoint) as ArcGISFeature;

                // Convert the Image from ARCore into a JPEG byte array.
                byte[] attachmentData = await ConvertImageToJPEG(capturedImage);

                // Add the attachment.
                // The contentType string is the MIME type for JPEG files, image/jpeg.
                await newFeature.AddAttachmentAsync("tree.jpg", "image/jpeg", attachmentData);

                // Add the newly created feature to the feature table.
                await _featureTable.AddFeatureAsync(newFeature);

                // Apply the edits to the service feature table.
                await _featureTable.ApplyEditsAsync();

                // Reset the user interface.
                _helpLabel.Text = "Tap to create a feature";
                _graphicsOverlay.Graphics.Clear();
                _addButton.Enabled = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ShowMessage("Could not create feature", "Error");
            }
        }

        // This method uses code from this StackOverflow thread. https://stackoverflow.com/a/51521388
        private async Task<byte[]> ConvertImageToJPEG(Image capturedImage)
        {
            // Convert the image into a byte array.
            ByteBuffer yBuffer = capturedImage.GetPlanes()[0].Buffer;
            ByteBuffer uBuffer = capturedImage.GetPlanes()[1].Buffer;
            ByteBuffer vBuffer = capturedImage.GetPlanes()[2].Buffer;

            int ySize = yBuffer.Remaining();
            int uSize = uBuffer.Remaining();
            int vSize = vBuffer.Remaining();

            // Make a byte array large enough to store the buffers from all three planes.
            byte[] nv21ByteArray = new byte[ySize + uSize + vSize];

            // Load the byte array using the byte buffers.
            yBuffer.Get(nv21ByteArray, 0, ySize);
            vBuffer.Get(nv21ByteArray, ySize, vSize);
            uBuffer.Get(nv21ByteArray, ySize + vSize, uSize);

            // Rotate the NV21 byte array 90 degrees to the right.
            byte[] rotatedNV21 = RotateNV21ImageRight(nv21ByteArray, capturedImage.Width, capturedImage.Height);

            // Create a YuvImage using the NV21 image
            Android.Graphics.YuvImage yuv = new Android.Graphics.YuvImage(rotatedNV21, Android.Graphics.ImageFormatType.Nv21, capturedImage.Height, capturedImage.Width, null);

            // Convert the YuvImage into a jpeg.
            System.IO.Stream jpegStream = new System.IO.MemoryStream();
            await yuv.CompressToJpegAsync(new Android.Graphics.Rect(0, 0, yuv.Width, yuv.Height), 100, jpegStream);

            // Store the jpeg data in a byte array.
            jpegStream.Position = 0;
            byte[] jpegArray = new byte[jpegStream.Length];
            jpegStream.Read(jpegArray, 0, jpegArray.Length);
            return jpegArray;
        }

        // This method uses code modified from this Stack Overflow thread. https://stackoverflow.com/a/31425229
        private byte[] RotateNV21ImageRight(byte[] input, int width, int height)
        {
            byte[] output = new byte[input.Length];
            int frameSize = width * height;

            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    int yIn = j * width + i;
                    int uIn = frameSize + (j >> 1) * width + (i & ~1);
                    int vIn = uIn + 1;

                    int iOut = height - j - 1;

                    int yOut = i * height + iOut;
                    int uOut = frameSize + (i >> 1) * height + (iOut & ~1);
                    int vOut = uOut + 1;

                    output[yOut] = (byte)(0xff & input[yIn]);
                    output[uOut] = (byte)(0xff & input[uIn]);
                    output[vOut] = (byte)(0xff & input[vIn]);
                }
            }
            return output;
        }

        protected override async void OnPause()
        {
            base.OnPause();
            await _arView.StopTrackingAsync();
        }

        protected override async void OnResume()
        {
            base.OnResume();

            // Resume AR tracking.
            await _arView.StartTrackingAsync(ARLocationTrackingMode.Continuous);
        }

        private void ShowMessage(string message, string title, bool closeApp = false)
        {
            // Show a message and then exit after if needed.
            AlertDialog dialog = new AlertDialog.Builder(this).SetMessage(message).SetTitle(title).Create();
            if (closeApp)
            {
                dialog.SetButton("OK", (o, e) =>
                {
                    Finish();
                });
            }
            dialog.Show();
        }
    }
}