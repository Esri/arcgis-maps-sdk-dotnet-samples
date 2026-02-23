// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.LocalServices;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.LocalServerGenerateElevationProfile
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Generate elevation profile with Local Server",
        category: "Local Server",
        description: "Create an elevation profile using a geoprocessing package executed with Local Server.",
        instructions: "The sample loads at the full extent of the raster dataset. Click the \"Draw Polyline\" button and sketch a polyline along where you'd like the elevation profile to be calculated (the polyline can be any shape). Click the \"Save\" button to save the sketch and draw the polyline. Click \"Generate Elevation Profile\" to interpolate the sketched polyline onto the raster surface in 3D. Once ready, the view will automatically zoom onto the newly drawn elevation profile. Click \"Clear Results\" to reset the sample.",
        tags: new[] { "elevation profile", "geoprocessing", "interpolate shape", "local server", "offline", "parameters", "processing", "raster", "raster function", "scene", "service", "terrain" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("db9cd9beedce4e0987c33c198c8dfb45", "259f420250a444b4944a277eec2c4e42", "831cbdc61b1c4cd3bfedd1af91d09d36")]
    public partial class LocalServerGenerateElevationProfile
    {
        // Hold references to the paths for the files used in this sample.
        private readonly string _rasterPath = DataManager.GetDataFolder("db9cd9beedce4e0987c33c198c8dfb45", "Arran_10m_raster.tif");
        private readonly string _geoprocessingPackagePath = DataManager.GetDataFolder("831cbdc61b1c4cd3bfedd1af91d09d36", "create_elevation_profile_model.gpkx");
        private readonly string _aboveSeaLevelJsonPath = DataManager.GetDataFolder("259f420250a444b4944a277eec2c4e42", "raster_functions", "above_sea_level_raster_calculation.json");
        private readonly string _restoreElevationJsonPath = DataManager.GetDataFolder("259f420250a444b4944a277eec2c4e42", "raster_functions", "restore_elevation_raster_calculation.json");
        private readonly string _maskJsonPath = DataManager.GetDataFolder("259f420250a444b4944a277eec2c4e42", "raster_functions", "mask.json");

        // Hold references for the objects used in event handlers.
        private FeatureCollection _featureCollection;
        private FeatureLayer _elevationProfileFeatureLayer;
        private Polyline _polyline;
        private GeoprocessingTask _gpTask;
        private GeoprocessingJob _gpJob;
        private Raster _arranRaster;
        private LocalGeoprocessingService _localGPService;
        private GraphicsOverlay _polylineGraphicsOverlay;
        private GraphicsOverlay _pointsGraphicsOverlay;
        private Viewpoint _rasterExtentViewPoint;
        private RasterLayer _rasterLayer;
        private SpatialReference _rasterLayerSpatialReference;
        private PointCollection _pointCollection;
        private bool _isDrawing;

        public LocalServerGenerateElevationProfile()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Try to start Local Server.
            try
            {
                // LocalServer must not be running when setting the data path.
                if (LocalServer.Instance.Status == LocalServerStatus.Started)
                {
                    await LocalServer.Instance.StopAsync();
                }

                // Set the local data path - must be done before starting. On most systems, this will be C:\EsriSamples\AppData.
                // This path should be kept short to avoid Windows path length limitations.
                string tempDataPathRoot = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.Windows)).FullName;
                string tempDataPath = Path.Combine(tempDataPathRoot, "EsriSamples", "AppData");
                Directory.CreateDirectory(tempDataPath); // CreateDirectory won't overwrite if it already exists.
                LocalServer.Instance.AppDataPath = tempDataPath;

                // Start the local server instance.
                await LocalServer.Instance.StartAsync();

                // Initialize and start a local geoprocessing service instance using our local geoprocessing package.
                _localGPService = new LocalGeoprocessingService(_geoprocessingPackagePath, GeoprocessingServiceType.AsynchronousSubmitWithMapServiceResult);
                await _localGPService.StartAsync();

                // Initialize a geoprocessing task using the local geoprocessing service.
                _gpTask = await GeoprocessingTask.CreateAsync(new Uri(_localGPService.Url + "/CreateElevationProfileModel"));
            }
            catch (Exception ex)
            {
                TypeInfo localServerTypeInfo = typeof(LocalMapService).GetTypeInfo();
                FileVersionInfo localServerVersion = FileVersionInfo.GetVersionInfo(localServerTypeInfo.Assembly.Location);

                MessageBox.Show($"Please ensure that local server {localServerVersion.FileVersion} is installed prior to using the sample. The download link is in the description. Message: {ex.Message}", "Local Server failed to start");
                return;
            }

            // Create a scene with a topographic basemap style.
            MySceneView.Scene = new Scene(BasemapStyle.ArcGISHillshadeDark);

            try
            {
                // Create a new raster from a local file and display it on the scene.
                _arranRaster = new Raster(_rasterPath);
                await DisplayRaster(_arranRaster);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }

            // Set up a new feature collection.
            _featureCollection = new FeatureCollection();

            // Create a graphics overlay for displaying the sketched polyline and add it to the scene view's list of graphics overlays.
            _polylineGraphicsOverlay = new GraphicsOverlay();
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.Black, 3);
            _polylineGraphicsOverlay.Renderer = new SimpleRenderer(lineSymbol);
            MySceneView.GraphicsOverlays.Add(_polylineGraphicsOverlay);

            // Update the UI.
            MyInstructionsTitleTextBlock.Visibility = Visibility.Visible;
            MyInstructionsContentTextBlock.Text = "Draw a polyline on the scene with the 'Draw Polyline' button.";
            MyDrawPolylineButton.IsEnabled = true;
            MyButtonPanel.Visibility = Visibility.Visible;
        }

        private async Task DisplayRaster(Raster raster)
        {
            try
            {
                // Get the masked raster after applying the masking function.
                Raster maskedRaster = ApplyMaskingRasterFunction(raster);

                // Create a raster layer from the masked raster.
                _rasterLayer = new RasterLayer(maskedRaster);

                // Set a hillshade renderer on the raster layer.
                _rasterLayer.Renderer = new HillshadeRenderer(30, 210, 1, SlopeType.None, 1, 1, 8);

                // Load the raster layer.
                await _rasterLayer.LoadAsync();

                // Add the raster layer to the scene.
                MySceneView.Scene.OperationalLayers.Add(_rasterLayer);

                // Create a viewpoint for the raster layers full extent.
                _rasterExtentViewPoint = new Viewpoint(_rasterLayer.FullExtent);

                // Update the scene viewpoint based on the raster layer.
                await MySceneView.SetViewpointAsync(_rasterExtentViewPoint);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
            }
        }

        private Raster ApplyMaskingRasterFunction(Raster originalRaster)
        {
            // Initiate raster for output.
            Raster maskedRaster;

            // Raster function to get pixels above 0m (above sea level).
            string aboveSeaLevelRasterFunctionJson = File.ReadAllText(_aboveSeaLevelJsonPath);
            RasterFunction aboveSeaLevelRasterFunction = RasterFunction.FromJson(aboveSeaLevelRasterFunctionJson);

            // Get the raster arguments and the names of raster variables.
            RasterFunctionArguments aboveSeaLevelRasterArguments = aboveSeaLevelRasterFunction.Arguments;
            IReadOnlyList<string> aboveSeaLevelRasterNames = aboveSeaLevelRasterArguments.GetRasterNames();

            // Apply the raster function to the original raster.
            aboveSeaLevelRasterArguments.SetRaster(aboveSeaLevelRasterNames[0], originalRaster);

            // Apply the raster function to the input raster, this gets a raster composed of 1s and 0s, 1 represents data above sea level.
            Raster aboveSeaLevelRaster = new Raster(aboveSeaLevelRasterFunction);

            // Raster function to restore elevation profiles post above sea level calculations.
            string restoreElevationRasterFunctionJson = File.ReadAllText(_restoreElevationJsonPath);
            RasterFunction restoreElevationRasterFunction = RasterFunction.FromJson(restoreElevationRasterFunctionJson);

            // Get the raster arguments and the names of raster variables.
            RasterFunctionArguments restoreElevationRasterArguments = restoreElevationRasterFunction.Arguments;
            IReadOnlyList<string> restoreElevationRasterNames = restoreElevationRasterArguments.GetRasterNames();

            // Apply the raster function to the original raster and the above sea level raster.
            restoreElevationRasterArguments.SetRaster(restoreElevationRasterNames[0], originalRaster);
            restoreElevationRasterArguments.SetRaster(restoreElevationRasterNames[1], aboveSeaLevelRaster);

            // Create a new raster with elevation values restored above 0.
            Raster restoredElevationRaster = new Raster(restoreElevationRasterFunction);

            // Raster function to mask out values below sea level (pixels with value of 0).
            string maskRasterFunctionJson = File.ReadAllText(_maskJsonPath);
            RasterFunction maskRasterFunction = RasterFunction.FromJson(maskRasterFunctionJson);

            // Get the raster arguments and the names of raster variables.
            RasterFunctionArguments maskRasterArguments = maskRasterFunction.Arguments;
            IReadOnlyList<string> maskRasterNames = maskRasterArguments.GetRasterNames();

            // Apply the raster function to the restored elevation raster.
            maskRasterArguments.SetRaster(maskRasterNames[0], restoredElevationRaster);

            // Create a new raster with values equal to 0 masked out.
            maskedRaster = new Raster(maskRasterFunction);

            return maskedRaster;
        }

        private void GenerateElevationProfileButton_Click(object sender, RoutedEventArgs e)
        {
            _ = GenerateElevationProfileTask();
        }

        private async Task GenerateElevationProfileTask()
        {
            try
            {
                // Update the UI.
                MyGenerateElevationProfileButton.IsEnabled = false;
                MyProgressBar.Visibility = Visibility.Visible;

                // Create the feature collection table from the sketched polyline.
                await CreateFeatureCollectionTableWithPolylineFeature();

                // Create default parameters and get their inputs.
                GeoprocessingParameters defaultParams = await _gpTask.CreateDefaultParametersAsync();
                IDictionary<string, GeoprocessingParameter> inputParameters = defaultParams.Inputs;

                // Ensure the feature collection is loaded before attempting to access the feature table it contains.
                await _featureCollection.LoadAsync();

                // Set the input polyline parameter geoprocessing feature, pointing to polyline.
                inputParameters["Input_Polyline"] = new GeoprocessingFeatures(_featureCollection.Tables[0]);

                // Set the input raster path geoprocessing string, pointing to raster file.
                inputParameters["Input_Raster"] = new GeoprocessingString(_arranRaster.Path);

                // Create geoprocessing job from the geoprocessing parameters to show elevation profile on the scene.
                _gpJob = _gpTask.CreateJob(defaultParams);

                // Add the event handlers to the geoprocessing job.
                _gpJob.StatusChanged += GpJob_StatusChanged;
                _gpJob.ProgressChanged += GpJob_ProgressChanged;

                // Update UI.
                MyProgressBar.Value = 0;
                MyProgressBar.Visibility = Visibility.Visible;
                MyProgressBarLabel.Visibility = Visibility.Visible;
                MyCancelJobButton.Visibility = Visibility.Visible;
                MyGenerateElevationProfileButton.Visibility = Visibility.Collapsed;
                MyDrawPolylineButton.Visibility = Visibility.Collapsed;
                MyClearResultsButton.Visibility = Visibility.Collapsed;

                _gpJob.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
            }
        }

        private void GpJob_ProgressChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                // Update the UI.
                MyProgressBar.Value = _gpJob.Progress;
                MyProgressBarLabel.Text = $"{MyProgressBar.Value}%";
            });
        }

        private void GpJob_StatusChanged(object sender, JobStatus e)
        {
            if (e == JobStatus.Succeeded)
            {
                _ = GeoprocessingJobSucceededTask();
            }
        }

        private async Task GeoprocessingJobSucceededTask()
        {
            try
            {
                // Convert geoprocessor server url to that of a map server and get the job id.
                Uri serviceUrl = _localGPService.Url;
                string mapServerUrl = serviceUrl.ToString().Replace("GPServer", "MapServer/jobs/" + _gpJob.ServerJobId);

                // Create a service geodatabase from the map server url.
                var serviceGeodatabase = new ServiceGeodatabase(new Uri(mapServerUrl));

                // Load the service geodatabase.
                await serviceGeodatabase.LoadAsync();

                // Get the feature table from the service geodatabase.
                FeatureTable featureTable = serviceGeodatabase.GetTable(0);

                // Instantiate the elevation profile feature layer from the feature table.
                _elevationProfileFeatureLayer = new FeatureLayer(featureTable);

                // Load the elevation profile feature layer.
                await _elevationProfileFeatureLayer.LoadAsync();

                // Set the surface placement and renderer for the elevation profile feature layer.
                _elevationProfileFeatureLayer.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
                _elevationProfileFeatureLayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.White, 3));

                Dispatcher.Invoke(() =>
                {
                    // Add the elevation profile feature layer to the scene.
                    MySceneView.Scene.OperationalLayers.Add(_elevationProfileFeatureLayer);

                    // Update the UI.
                    MyInstructionsContentTextBlock.Text = "Elevation Profile drawn.";
                    MyGenerateElevationProfileButton.Visibility = Visibility.Visible;
                    MyDrawPolylineButton.Visibility = Visibility.Visible;
                    MyClearResultsButton.Visibility = Visibility.Visible;
                    MyClearResultsButton.IsEnabled = true;
                    MyCancelJobButton.Visibility = Visibility.Collapsed;
                    MyProgressBar.Visibility = Visibility.Collapsed;
                    MyProgressBarLabel.Visibility = Visibility.Collapsed;
                });

                // Set the viewpoint based on the position of the elevation profile.
                await MySceneView.SetViewpointCameraAsync(CreateCameraFacingElevationProfile(), new TimeSpan(0, 0, 0, 2, 0));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
            }
            finally
            {
                // Remove the event handlers.
                _gpJob.ProgressChanged -= GpJob_ProgressChanged;
                _gpJob.StatusChanged -= GpJob_StatusChanged;
            }
        }

        private Camera CreateCameraFacingElevationProfile()
        {
            // Get the polyline's end point coordinates.
            MapPoint endPoint = _polyline.Parts[0].EndPoint;
            double endPointX = endPoint.X;
            double endPointY = endPoint.Y;

            // Get the polyline's center point coordinates.
            MapPoint centerPoint = _polyline.Extent.GetCenter();
            double centerX = centerPoint.X;
            double centerY = centerPoint.Y;

            // Calculate the position of a point perpendicular to the centre of the polyline.
            double lengthX = endPointX - centerX;
            double lengthY = endPointY - centerY;
            var cameraPositionPoint = new MapPoint(centerX + lengthY * 2, centerY - lengthX * 2, 1200,
              SpatialReferences.WebMercator);

            // Calculate the heading for the camera position so that it points perpendicularly towards the elevation profile.
            double theta;
            double cameraHeadingPerpToProfile;

            // Account for switching opposite and adjacent depending on angle direction from drawn line.
            if (lengthY < 0)
            {
                // Account for a downwards angle.
                theta = (180 / Math.PI) * (Math.Atan((centerX - endPointX) / (centerY - endPointY)));
                cameraHeadingPerpToProfile = theta + 90;
            }
            else
            {
                // Account for an upwards angle.
                theta = (180 / Math.PI) * (Math.Atan((centerY - endPointY) / (centerX - endPointX)));

                // Determine if theta is positive or negative, then account accordingly for calculating the angle back from north
                // and then rotate that value by + or - 90 to get the angle perpendicular to the drawn line.
                double angleFromNorth = (90 - theta);

                // If theta is positive, rotate angle anticlockwise by 90 degrees, else, clockwise by 90 degrees.
                if (theta > 0)
                {
                    cameraHeadingPerpToProfile = angleFromNorth - 90;
                }
                else
                {
                    cameraHeadingPerpToProfile = angleFromNorth + 90;
                }
            }
            // Create a new camera from the calculated camera position point and camera angle perpendicular to profile.
            return new Camera(cameraPositionPoint, cameraHeadingPerpToProfile, 80, 0);
        }

        private async Task CreateFeatureCollectionTableWithPolylineFeature()
        {
            // Create name field for polyline.
            List<Field> polylineField = new List<Field>();
            polylineField.Add(Field.CreateString("Name", "Name of feature", 20));

            // Create a feature collection table.
            FeatureCollectionTable featureCollectionTable = new FeatureCollectionTable(polylineField, GeometryType.Polyline, SpatialReferences.WebMercator, true, false);

            // Load the feature collection table.
            await featureCollectionTable.LoadAsync();

            // Add the feature collection table to the feature collection used to generate the elevation profile.
            _featureCollection.Tables.Add(featureCollectionTable);

            // Create an attributes dictionary for the feature.
            var attributes = new Dictionary<string, object>();
            attributes["ElevationSection"] = polylineField[0].Name;

            // Create a polyline feature and add it to the feature collection table.
            Feature addedFeature = featureCollectionTable.CreateFeature(attributes, _polyline);
            await featureCollectionTable.AddFeatureAsync(addedFeature);
        }

        private void DrawPolylineButton_Click(object sender, RoutedEventArgs e)
        {
            _isDrawing = true;

            // Update the UI.
            MyInstructionsContentTextBlock.Text = "Click on the map to draw a polyline path, click the 'Save' button to save it.";
            MyGenerateElevationProfileButton.Visibility = Visibility.Collapsed;
            MyClearResultsButton.Visibility = Visibility.Collapsed;
            MyDrawPolylineButton.Visibility = Visibility.Collapsed;
            MyDrawPolylineButton.IsEnabled = false;
            MySaveButton.Visibility = Visibility.Visible;

            // Create a temporary graphics overlay to display a point collection on the map.
            _pointsGraphicsOverlay = new GraphicsOverlay();
            MySceneView.GraphicsOverlays.Add(_pointsGraphicsOverlay);

            // Create a cross markery symbol for the tapped points.
            var simpleMarkerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.Black, 10);
            _pointsGraphicsOverlay.Renderer = new SimpleRenderer(simpleMarkerSymbol);

            // Create a point collection with the same spatial reference as the raster layer.
            _rasterLayerSpatialReference = _rasterLayer.SpatialReference;
            _pointCollection = new PointCollection(_rasterLayerSpatialReference);

            MySceneView.GeoViewTapped += MySceneView_GeoViewTapped;
        }

        private void MySceneView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            if (_isDrawing)
            {
                _ = GeoViewTappedTask(e);
            }
        }

        private async Task GeoViewTappedTask(GeoViewInputEventArgs e)
        {
            try
            {
                // Get the location from the tapped screen position.
                MapPoint point = await MySceneView.ScreenToLocationAsync(e.Position);

                // In order to project the tapped point, the point must have a valid spatial reference.
                if (point.SpatialReference == null)
                {
                    MessageBox.Show("Clicked point must contain a valid spatial reference.", "Error");

                    return;
                }

                // Project the point to the spatial reference of the raster layer.
                MapPoint projectedPoint = (MapPoint)point.Project(_rasterLayerSpatialReference);

                // Check that the user has clicked within the extent of the raster.
                if (projectedPoint.Intersects(_rasterLayer.FullExtent))
                {
                    // Add the projected point to the collection of projected points and the graphics overlay displaying tapped points.
                    _pointCollection.Add(projectedPoint);
                    _pointsGraphicsOverlay.Graphics.Add(new Graphic(projectedPoint));
                }
                else
                {
                    MessageBox.Show("Clicked point must be within raster layer extent.", "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void ClearResultsButton_Click(object sender, RoutedEventArgs e)
        {
            _ = ClearResultsTask();
        }

        private async Task ClearResultsTask()
        {
            try
            {
                // Remove all graphics.
                _polylineGraphicsOverlay.Graphics.Clear();

                // Clear the temporary graphics overlay.
                _pointsGraphicsOverlay.Graphics.Clear();

                // If the elevation profile feature layer has been added to the scene, remove it.
                if (MySceneView.Scene.OperationalLayers.Count > 1)
                {
                    // Remove last elevation profile feature layer from the scene.
                    MySceneView.Scene.OperationalLayers.Remove(_elevationProfileFeatureLayer);
                }

                // If more than one graphics overlay is present, remove the temporary points overlay.
                if (MySceneView.GraphicsOverlays.Count > 1)
                {
                    MySceneView.GraphicsOverlays.Remove(_pointsGraphicsOverlay);
                }

                // Update the UI.
                MyGenerateElevationProfileButton.IsEnabled = false;
                MyDrawPolylineButton.IsEnabled = true;
                MyClearResultsButton.IsEnabled = false;
                MyInstructionsContentTextBlock.Visibility = Visibility.Visible;
                MyInstructionsContentTextBlock.Text = "Draw a polyline on the scene with the 'Draw Polyline' button.";

                // Set the viewpoint to the initial raster layer extent view.
                await MySceneView.SetViewpointAsync(_rasterExtentViewPoint);

                // Clear the elevation polyline data from the feature collection.
                _featureCollection.Tables.Clear();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_pointsGraphicsOverlay.Graphics.Count > 1)
            {
                _isDrawing = false;

                // Remove the temporarily created points from the map.
                _pointsGraphicsOverlay.Graphics.Clear();

                // Create a polyline from the clicked points on the scene and add it as a graphic to the graphics overlay.
                _polyline = new Polyline(_pointCollection);
                Graphic graphic = new Graphic(_polyline);
                _polylineGraphicsOverlay.Graphics.Add(graphic);

                // Update the UI.
                MyGenerateElevationProfileButton.Visibility = Visibility.Visible;
                MyGenerateElevationProfileButton.IsEnabled = true;
                MyDrawPolylineButton.Visibility = Visibility.Visible;
                MyDrawPolylineButton.IsEnabled = false;
                MyClearResultsButton.Visibility = Visibility.Visible;
                MyClearResultsButton.IsEnabled = true;
                MyInstructionsContentTextBlock.Text = "Generate an elevation profile along the polyline using the 'Generate Elevation Profile' button.";
                MySaveButton.Visibility = Visibility.Collapsed;

                // Clear the polyline points.
                _pointCollection.Clear();

                MySceneView.GeoViewTapped -= MySceneView_GeoViewTapped;
            }
            else
            {
                MessageBox.Show("More than one point required to draw a polyline", "Warning");
            }
        }

        private void CancelJobButton_Click(object sender, RoutedEventArgs e)
        {
            if (_gpJob != null)
            {
                _ = _gpJob.CancelAsync();

                // Remove the event handlers.
                _gpJob.ProgressChanged -= GpJob_ProgressChanged;
                _gpJob.StatusChanged -= GpJob_StatusChanged;

                // Update the UI.
                MyCancelJobButton.Visibility = Visibility.Collapsed;
                MyProgressBar.Visibility = Visibility.Collapsed;
                MyProgressBarLabel.Visibility = Visibility.Collapsed;
                MyGenerateElevationProfileButton.Visibility = Visibility.Visible;
                MyGenerateElevationProfileButton.IsEnabled = true;
                MyDrawPolylineButton.Visibility = Visibility.Visible;
                MyClearResultsButton.Visibility = Visibility.Visible;
            }
        }
    }
}