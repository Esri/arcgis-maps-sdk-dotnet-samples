﻿// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;

#if ANDROID

using Application = Microsoft.Maui.Controls.Application;

#endif

namespace ArcGIS.Samples.GenerateOfflineMapWithOverrides
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Generate offline map (overrides)",
        category: "Map",
        description: "Take a web map offline with additional options for each layer.",
        instructions: "Modify the overrides parameters:",
        tags: new[] { "LOD", "adjust", "download", "extent", "filter", "offline", "override", "parameters", "reduce", "scale range", "setting" })]
    [ArcGIS.Samples.Shared.Attributes.ClassFile("ConfigureOverridesPage.xaml.cs", "ConfigureOverridesPage.xaml")]
    public partial class GenerateOfflineMapWithOverrides : ContentPage
    {
        // The job to generate an offline map.
        private GenerateOfflineMapJob _generateOfflineMapJob;

        // The extent of the data to take offline.
        private Envelope _areaOfInterest = new Envelope(-88.1541, 41.7690, -88.1471, 41.7720, SpatialReferences.Wgs84);

        // The ID for a web map item hosted on the server (water network map of Naperville IL).
        private const string WebMapId = "acc027394bc84c2fb04d1ed317aac674";

        public GenerateOfflineMapWithOverrides()
        {
            InitializeComponent();

            // Load the web map, show area of interest, restrict map interaction, and set up authorization.
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Create the ArcGIS Online portal.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync();

                // Get the Naperville water web map item using its ID.
                PortalItem webmapItem = await PortalItem.CreateAsync(portal, WebMapId);

                // Create a map from the web map item.
                var onlineMap = new Map(webmapItem);

                // Display the map in the MapView.
                MyMapView.Map = onlineMap;

                // Disable user interactions on the map (no panning or zooming from the initial extent).
                MyMapView.InteractionOptions = new MapViewInteractionOptions
                {
                    IsEnabled = false
                };

                // Create a graphics overlay for the extent graphic and apply a renderer.
                var aoiOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 3);
                var extentOverlay = new GraphicsOverlay
                {
                    Renderer = new SimpleRenderer(aoiOutlineSymbol)
                };
                MyMapView.GraphicsOverlays.Add(extentOverlay);

                // Add a graphic to show the area of interest (extent) that will be taken offline.
                var aoiGraphic = new Graphic(_areaOfInterest);
                extentOverlay.Graphics.Add(aoiGraphic);

                // Hide the map loading progress indicator.
                LoadingIndicator.IsVisible = false;
            }
            catch (Exception ex)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Alert", ex.ToString(), "OK");
            }
        }

        private async void TakeMapOfflineButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Clean up any previous outputs in the temp directory.
                string tempPath = $"{Path.GetTempPath()}";
                string[] outputFolders = Directory.GetDirectories(tempPath, "NapervilleWaterNetwork*");

                // Loop through the folder names and delete them.
                foreach (string dir in outputFolders)
                {
                    try
                    {
                        // Delete the folder.
                        Directory.Delete(dir, true);
                    }
                    catch (Exception)
                    {
                        // Ignore exceptions (files might be locked, for example).
                    }
                }

                // Create a new folder for the output mobile map.
                string packagePath = Path.Combine(tempPath, @"NapervilleWaterNetwork");
                int num = 1;
                while (Directory.Exists(packagePath))
                {
                    packagePath = Path.Combine(tempPath, @"NapervilleWaterNetwork" + num.ToString());
                    num++;
                }

                // Create the output directory.
                Directory.CreateDirectory(packagePath);

                // Create an offline map task with the current (online) map.
                OfflineMapTask takeMapOfflineTask = await OfflineMapTask.CreateAsync(MyMapView.Map);

                // Create the default parameters for the task, pass in the area of interest.
                GenerateOfflineMapParameters parameters = await takeMapOfflineTask.CreateDefaultGenerateOfflineMapParametersAsync(_areaOfInterest);

                // Generate parameter overrides for more in-depth control of the job.
                GenerateOfflineMapParameterOverrides overrides = await takeMapOfflineTask.CreateGenerateOfflineMapParameterOverridesAsync(parameters);

                // Create the UI for configuring the overrides.
                var configurationPage = new ConfigureOverridesPage(overrides, MyMapView.Map);

                // Wait for the user to finish with the configuration, then continue.
                configurationPage.Disappearing += async (o, args) =>
                {
                    try
                    {
                        // Re-show the busy indicator.
                        BusyIndicator.IsVisible = true;

                        // Create the job with the parameters and output location.
                        _generateOfflineMapJob = takeMapOfflineTask.GenerateOfflineMap(parameters, packagePath, overrides);

                        // Handle the progress changed event for the job.
                        _generateOfflineMapJob.ProgressChanged += OfflineMapJob_ProgressChanged;

                        // Await the job to generate geodatabases, export tile packages, and create the mobile map package.
                        GenerateOfflineMapResult results = await _generateOfflineMapJob.GetResultAsync();

                        // Check for job failure (writing the output was denied, e.g.).
                        if (_generateOfflineMapJob.Status != JobStatus.Succeeded)
                        {
                            await Application.Current.Windows[0].Page.DisplayAlert("Alert", "Generate offline map package failed.", "OK");
                            BusyIndicator.IsVisible = false;
                        }

                        // Check for errors with individual layers.
                        if (results.LayerErrors.Any())
                        {
                            // Build a string to show all layer errors.
                            System.Text.StringBuilder errorBuilder = new System.Text.StringBuilder();
                            foreach (KeyValuePair<Layer, Exception> layerError in results.LayerErrors)
                            {
                                errorBuilder.AppendLine($"{layerError.Key.Id} : {layerError.Value.Message}");
                            }

                            // Show layer errors.
                            string errorText = errorBuilder.ToString();
                            await Application.Current.Windows[0].Page.DisplayAlert("Alert", errorText, "OK");
                        }

                        // Display the offline map.
                        MyMapView.Map = results.OfflineMap;

                        // Apply the original viewpoint for the offline map.
                        MyMapView.SetViewpoint(new Viewpoint(_areaOfInterest));

                        // Enable map interaction so the user can explore the offline data.
                        MyMapView.InteractionOptions.IsEnabled = true;

                        // Show a message that the map is offline.
                        await Application.Current.Windows[0].Page.DisplayAlert("Alert", "Map is offline.", "OK");

                        TakeMapOfflineButton.IsEnabled = false;
                    }
                    catch (TaskCanceledException)
                    {
                        // Generate offline map task was canceled.
                        await Application.Current.Windows[0].Page.DisplayAlert("Alert", "Taking map offline was canceled.", "OK");
                    }
                    catch (Exception ex)
                    {
                        // Exception while taking the map offline.
                        await Application.Current.Windows[0].Page.DisplayAlert("Alert", ex.ToString(), "OK");
                    }
                    finally
                    {
                        // Hide the busy indicator.
                        BusyIndicator.IsVisible = false;
                    }
                };

                // Show the configuration UI.
                await Shell.Current.Navigation.PushModalAsync(configurationPage, true);
            }
            catch (Exception ex)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Alert", ex.ToString(), "OK");
            }
        }

        // Show changes in job progress.
        private void OfflineMapJob_ProgressChanged(object sender, EventArgs e)
        {
            // Get the job.
            GenerateOfflineMapJob job = sender as GenerateOfflineMapJob;

            // Dispatch to the UI thread.
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                // Show the percent complete and update the progress bar.
                Percentage.Text = job.Progress > 0 ? job.Progress.ToString() + " %" : string.Empty;
                ProgressBar.Progress = job.Progress / 100.0;
            });
        }

        private void CancelJobButton_Click(object sender, EventArgs e)
        {
            // The user canceled the job.
            _generateOfflineMapJob.CancelAsync();

            // Hide the busy indicator.
            BusyIndicator.IsVisible = false;
        }
    }
}