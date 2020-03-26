// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntimeXamarin.Converters;
using Esri.ArcGISRuntime.ARToolkit;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Xamarin.Forms;
using Forms.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Surface = Esri.ArcGISRuntime.Mapping.Surface;

#if XAMARIN_ANDROID
using ArcGISRuntime.Droid;
#endif

namespace ArcGISRuntimeXamarin.Samples.CollectDataAR
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Collect data in AR",
        "Augmented reality",
        "Tap on real-world objects to collect data.",
        "")]
    public partial class CollectDataAR : ContentPage, IARSample
    {
        // Scene content.
        private ArcGISTiledElevationSource _elevationSource;
        private Surface _elevationSurface;
        private Scene _scene;

        // Track when user is changing between AR and GPS localization.
        private bool _changingScale;

        // Feature table for collected data about trees.
        private ServiceFeatureTable _featureTable = new ServiceFeatureTable(new Uri("https://services2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/rest/services/AR_Tree_Survey/FeatureServer/0"));

        // Graphics for tapped points in the scene.
        private GraphicsOverlay _graphicsOverlay;
        private SimpleMarkerSceneSymbol _tappedPointSymbol = new SimpleMarkerSceneSymbol(SimpleMarkerSceneSymbolStyle.Diamond, System.Drawing.Color.Orange, 0.5, 0.5, 0.5, SceneSymbolAnchorPosition.Center);

        // Custom location data source that enables calibration and returns values relative to mean sea level rather than the WGS84 ellipsoid.
        private ARLocationDataSource _locationDataSource;

        // Calibration state fields.
        private bool _isCalibrating;
        private double _altitudeOffset;

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
                    MyARSceneView.InteractionOptions.IsEnabled = true;
                    CalibrationGrid.IsVisible = true;
                }
                else
                {
                    // Hide the scene when not calibrating.
                    _scene.BaseSurface.Opacity = 0;

                    // Disable scene interaction.
                    MyARSceneView.InteractionOptions.IsEnabled = false;
                    CalibrationGrid.IsVisible = false;
                }
            }
        }

        public CollectDataAR()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Create the custom location data source and configure the AR scene view to use it.
#if XAMARIN_ANDROID
            bool locationGranted = await MainActivity.Instance.LocationPermissionGranted();
            if(!locationGranted)
            {
                return;
            }    
            _locationDataSource = new ARLocationDataSource(Android.App.Application.Context);
            _locationDataSource.AltitudeMode = ARLocationDataSource.AltitudeAdjustmentMode.NmeaParsedMsl;
#elif __IOS__
            _locationDataSource = new ARLocationDataSource();
#endif
            MyARSceneView.LocationDataSource = _locationDataSource;
            await MyARSceneView.StartTrackingAsync(ARLocationTrackingMode.Continuous);

            // Create the scene and show it.
            _scene = new Scene(Basemap.CreateImagery());
            MyARSceneView.Scene = _scene;

            // Create and add the elevation surface.
            _elevationSource = new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));
            _elevationSurface = new Surface();
            _elevationSurface.ElevationSources.Add(_elevationSource);
            MyARSceneView.Scene.BaseSurface = _elevationSurface;

            // Hide the surface in AR.
            _elevationSurface.NavigationConstraint = NavigationConstraint.None;
            _elevationSurface.Opacity = 0;

            // Configure the space and atmosphere effects for AR.
            MyARSceneView.SpaceEffect = SpaceEffect.None;
            MyARSceneView.AtmosphereEffect = AtmosphereEffect.None;

            // Add a graphics overlay for displaying points in AR.
            _graphicsOverlay = new GraphicsOverlay();
            _graphicsOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            _graphicsOverlay.Renderer = new SimpleRenderer(_tappedPointSymbol);
            MyARSceneView.GraphicsOverlays.Add(_graphicsOverlay);

            // Add the exisiting features to the scene.
            FeatureLayer treeLayer = new FeatureLayer(_featureTable);
            treeLayer.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            MyARSceneView.Scene.OperationalLayers.Add(treeLayer);

            // Add the event for the user tapping the screen.
            MyARSceneView.GeoViewTapped += ARViewTapped;

            // Disable scene interaction.
            MyARSceneView.InteractionOptions = new SceneViewInteractionOptions() { IsEnabled = false };

            // Enable the calibrate button.
            CalibrateButton.IsEnabled = true;
        }

        private void ARViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Don't add features when calibrating the AR view.
            if (_isCalibrating)
            {
                return;
            }

            // Try to get the real-world position of that tapped AR plane.
            MapPoint planeLocation = MyARSceneView.ARScreenToLocation(e.Position);

            // Remove any existing graphics.
            _graphicsOverlay.Graphics.Clear();

            // Check if a Map Point was identified.
            if (planeLocation != null)
            {
                // Add a graphic at the tapped location.
                _graphicsOverlay.Graphics.Add(new Graphic(planeLocation, _tappedPointSymbol));
                AddButton.IsEnabled = true;
                HelpLabel.Text = "Placed relative to ARCore plane";
            }
            else
            {
                Application.Current.MainPage.DisplayAlert("Error", "Didn't find anything, try again.", "OK");
                AddButton.IsEnabled = false;
            }
        }

        private void CalibrateButtonPressed(object sender, EventArgs e) { IsCalibrating = !IsCalibrating; }

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
            Camera camera = MyARSceneView.OriginCamera;

            // Calculate the new heading by applying the offset to the old camera's heading.
            double heading = camera.Heading + e.DeltaProgress;

            // Create a new camera by rotating the old camera to the new heading.
            Camera newCamera = camera.RotateTo(heading, camera.Pitch, camera.Roll);

            // Use the new camera as the origin camera.
            MyARSceneView.OriginCamera = newCamera;
        }

        private async void RealScaleValueChanged(object sender, EventArgs e)
        {
            // Prevent this from being called concurrently
            if (_changingScale)
            {
                return;
            }
            _changingScale = true;

            // Disable the associated UI controls while switching.
            RoamingButton.IsEnabled = false;
            LocalButton.IsEnabled = false;

            // Check if using roaming for AR location mode.
            if (((Button)sender).Text == "GPS")
            {
                await MyARSceneView.StopTrackingAsync();

                // Start AR tracking using a continuous GPS signal.
                await MyARSceneView.StartTrackingAsync(ARLocationTrackingMode.Continuous);
                ElevationSlider.IsEnabled = true;
                LocalButton.IsEnabled = true;
            }
            else
            {
                await MyARSceneView.StopTrackingAsync();

                // Start AR tracking without using a GPS signal.
                await MyARSceneView.StartTrackingAsync(ARLocationTrackingMode.Ignore);
                ElevationSlider.IsEnabled = false;
                RoamingButton.IsEnabled = true;
            }
            _changingScale = false;
        }

        private async void AddButtonPressed(object sender, EventArgs e)
        {
            // Check if the user has already tapped a point.
            if (!_graphicsOverlay.Graphics.Any())
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Didn't find anything, try again.", "OK");
                return;
            }

            try
            {
                // Prevent the user from changing the tapped feature.
                MyARSceneView.GeoViewTapped -= ARViewTapped;

                // Prompt the user for the health value of the tree.
                int healthValue = await GetTreeHealthValue();

                // Create a new ArcGIS feature and add it to the feature service.
                await CreateFeature(healthValue);
            }
            // This exception is thrown when the user cancels out of the prompt.
            catch (OperationCanceledException)
            {
                return;
            }
            finally
            {
                // Restore the event listener for adding new features.
                MyARSceneView.GeoViewTapped += ARViewTapped;
            }
        }

        private async Task<int> GetTreeHealthValue()
        {
            // Prompt the user for the health of the tree.
            string health = await ((Page)Parent).DisplayActionSheet("Tree health?", "Cancel", null, "Dead", "Distressed", "Healthy");

            // Return a tree health value based on the users selection.
            switch (health)
            {
                case "Dead": // Dead tree.
                    return 0;

                case "Distressed": // Distressed tree.
                    return 5;

                case "Healthy": // Healthy tree.
                    return 10;

                default:
                    throw new OperationCanceledException();
            }
        }

        private async Task CreateFeature(int healthValue)
        {
            HelpLabel.Text = "Adding feature...";

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

                // Add the newly created feature to the feature table.
                await _featureTable.AddFeatureAsync(newFeature);

                // Apply the edits to the service feature table.
                await _featureTable.ApplyEditsAsync();

                // Reset the user interface.
                HelpLabel.Text = "Tap to create a feature";
                _graphicsOverlay.Graphics.Clear();
                AddButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Application.Current.MainPage.DisplayAlert("Error", "Could not create feature", "OK");
            }
        }

        async void IARSample.StartAugmentedReality()
        {
            // Start device tracking.
            try
            {
                await MyARSceneView.StartTrackingAsync(ARLocationTrackingMode.Continuous);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        void IARSample.StopAugmentedReality()
        {
            MyARSceneView.StopTrackingAsync();
        }
    }
}