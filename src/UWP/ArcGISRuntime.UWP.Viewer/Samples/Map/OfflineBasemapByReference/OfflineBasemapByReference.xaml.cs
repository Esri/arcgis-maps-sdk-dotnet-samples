﻿// Copyright 2019 Esri.
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
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace ArcGISRuntime.UWP.Samples.OfflineBasemapByReference
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Generate offline map with local basemap",
        "Map",
        "Use the OfflineMapTask to take a web map offline while using a basemap previously taken offline.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("628e8e3521cf45e9a28a12fe10c02c4d")]
    public partial class OfflineBasemapByReference
    {
        // The job to generate an offline map.
        private GenerateOfflineMapJob _generateOfflineMapJob;

        // The extent of the data to take offline.
        private readonly Envelope _areaOfInterest = new Envelope(-88.1541, 41.7690, -88.1471, 41.7720, SpatialReferences.Wgs84);

        // The ID for a web map item hosted on the server (water network map of Naperville IL).
        private const string WebMapId = "acc027394bc84c2fb04d1ed317aac674";

        public OfflineBasemapByReference()
        {
            InitializeComponent();
            Initialize();
        }

        private async Task ConfigureOfflineJobForBasemap(GenerateOfflineMapParameters parameters)
        {
            // Don't give the user a choice if there is no basemap specified.
            if (String.IsNullOrWhiteSpace(parameters.ReferenceBasemapFilename))
            {
                return;
            }

            // Get the path to the basemap directory.
            string basemapBasePath = DataManager.GetDataFolder("628e8e3521cf45e9a28a12fe10c02c4d");

            // Get the full path to the basemap by combining the name specified in the web map (ReferenceBasemapFilename)
            //  with the offline basemap directory.
            string basemapFullPath = Path.Combine(basemapBasePath, parameters.ReferenceBasemapFilename);

            // If the offline basemap doesn't exist, proceed without it.
            if (!File.Exists(basemapFullPath))
            {
                return;
            }

            // Create a dialog for getting the user's basemap choice.
            MessageDialog dialog = new MessageDialog("Use the offline basemap?", "Basemap choice");

            // If the user selects OK, parameters.ReferenceBasemapDirectory will be set.
            dialog.Commands.Add(new UICommand("Yes", command => parameters.ReferenceBasemapDirectory = basemapBasePath));

            // Do nothing otherwise.
            dialog.Commands.Add(new UICommand("No"));

            // Wait for the user to choose.
            await dialog.ShowAsync();
        }

        // Note: all code below (except call to ConfigureOfflineJobForBasemap) is identical to code in the Generate offline map sample.

        #region Generate offline map

        private async void Initialize()
        {
            try
            {
                // Call a function to set up the AuthenticationManager for OAuth.
                SetOAuthInfo();

                // Create the ArcGIS Online portal.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync();

                // Get the Naperville water web map item using its ID.
                PortalItem webmapItem = await PortalItem.CreateAsync(portal, WebMapId);

                // Create a map from the web map item.
                Map onlineMap = new Map(webmapItem);

                // Display the map in the MapView.
                MyMapView.Map = onlineMap;

                // Disable user interactions on the map (no panning or zooming from the initial extent).
                MyMapView.InteractionOptions = new MapViewInteractionOptions
                {
                    IsEnabled = false
                };

                // Create a graphics overlay for the extent graphic and apply a renderer.
                SimpleLineSymbol aoiOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 3);
                GraphicsOverlay extentOverlay = new GraphicsOverlay
                {
                    Renderer = new SimpleRenderer(aoiOutlineSymbol)
                };
                MyMapView.GraphicsOverlays.Add(extentOverlay);

                // Add a graphic to show the area of interest (extent) that will be taken offline.
                Graphic aoiGraphic = new Graphic(_areaOfInterest);
                extentOverlay.Graphics.Add(aoiGraphic);

                // Hide the map loading progress indicator.
                LoadingIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                // When the map view unloads, try to clean up existing output data folders.
                MyMapView.Unloaded += (s, e) =>
                {
                    // Find output mobile map folders in the temp directory.
                    string[] outputFolders = Directory.GetDirectories(Environment.ExpandEnvironmentVariables("%TEMP%"), "NapervilleWaterNetwork*");

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
                };
            }
            catch (Exception ex)
            {
                MessageDialog dialog = new MessageDialog(ex.ToString(), "Error loading map");
                await dialog.ShowAsync();
            }
        }

        private async void TakeMapOfflineButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Create a new folder for the output mobile map.
            string packagePath = Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), @"NapervilleWaterNetwork");
            int num = 1;
            while (Directory.Exists(packagePath))
            {
                packagePath = Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), @"NapervilleWaterNetwork" + num.ToString());
                num++;
            }

            // Create the output directory.
            Directory.CreateDirectory(packagePath);

            try
            {
                // Show the progress indicator while the job is running.
                BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Visible;

                // Create an offline map task with the current (online) map.
                OfflineMapTask takeMapOfflineTask = await OfflineMapTask.CreateAsync(MyMapView.Map);

                // Create the default parameters for the task, pass in the area of interest.
                GenerateOfflineMapParameters parameters = await takeMapOfflineTask.CreateDefaultGenerateOfflineMapParametersAsync(_areaOfInterest);

                // Configure basemap settings for the job.
                await ConfigureOfflineJobForBasemap(parameters);

                // Create the job with the parameters and output location.
                _generateOfflineMapJob = takeMapOfflineTask.GenerateOfflineMap(parameters, packagePath);

                // Handle the progress changed event for the job.
                _generateOfflineMapJob.ProgressChanged += OfflineMapJob_ProgressChanged;

                // Await the job to generate geodatabases, export tile packages, and create the mobile map package.
                GenerateOfflineMapResult results = await _generateOfflineMapJob.GetResultAsync();

                // Check for job failure (writing the output was denied, e.g.).
                if (_generateOfflineMapJob.Status != JobStatus.Succeeded)
                {
                    MessageDialog dialog = new MessageDialog("Generate offline map package failed.", "Job status");
                    await dialog.ShowAsync();
                    BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
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
                    MessageDialog dialog = new MessageDialog(errorText, "Layer errors");
                    await dialog.ShowAsync();
                }

                // Display the offline map.
                MyMapView.Map = results.OfflineMap;

                // Apply the original viewpoint for the offline map.
                MyMapView.SetViewpoint(new Viewpoint(_areaOfInterest));

                // Enable map interaction so the user can explore the offline data.
                MyMapView.InteractionOptions.IsEnabled = true;

                // Hide the "Take map offline" button.
                TakeOfflineArea.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                // Show a message that the map is offline.
                MessageArea.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            catch (TaskCanceledException)
            {
                // Generate offline map task was canceled.
                MessageDialog dialog = new MessageDialog("Taking map offline was canceled");
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                // Exception while taking the map offline.
                MessageDialog dialog = new MessageDialog(ex.Message, "Offline map error");
                await dialog.ShowAsync();
            }
            finally
            {
                // Hide the activity indicator when the job is done.
                BusyIndicator.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        // Show changes in job progress.
        private async void OfflineMapJob_ProgressChanged(object sender, EventArgs e)
        {
            // Get the job.
            GenerateOfflineMapJob job = sender as GenerateOfflineMapJob;

            // Dispatch to the UI thread.
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                // Show the percent complete and update the progress bar.
                Percentage.Text = job.Progress > 0 ? job.Progress.ToString() + " %" : string.Empty;
                ProgressBar.Value = job.Progress;
            });
        }

        private void CancelJobButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // The user canceled the job.
            _generateOfflineMapJob.Cancel();
        }

        #endregion Generate offline map

        #region Authentication

        // Constants for OAuth-related values.
        // - The URL of the portal to authenticate with (ArcGIS Online).
        private const string ServerUrl = "https://www.arcgis.com/sharing/rest";

        // - The Client ID for an app registered with the server (the ID below is for a public app created by the ArcGIS Runtime team).
        private const string AppClientId = @"lgAdHkYZYlwwfAhC";

        // - An optional client secret for the app (only needed for the OAuthAuthorizationCode authorization type).
        private const string ClientSecret = "";

        // - A URL for redirecting after a successful authorization (this must be a URL configured with the app).
        private const string OAuthRedirectUrl = @"my-ags-app://auth";

        private void SetOAuthInfo()
        {
            // Register the server information with the AuthenticationManager.
            ServerInfo serverInfo = new ServerInfo
            {
                ServerUri = new Uri(ServerUrl),
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit,
                OAuthClientInfo = new OAuthClientInfo
                {
                    ClientId = AppClientId,
                    RedirectUri = new Uri(OAuthRedirectUrl)
                }
            };

            // Register this server with AuthenticationManager.
            AuthenticationManager.Current.RegisterServer(serverInfo);

            // Use a function in this class to challenge for credentials.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Note: In a WPF app, you need to associate a custom IOAuthAuthorizeHandler component with the AuthenticationManager to 
            //     handle showing OAuth login controls (AuthenticationManager.Current.OAuthAuthorizeHandler = new MyOAuthAuthorize();).
            //     The UWP AuthenticationManager, however, uses a built-in IOAuthAuthorizeHandler (based on WebAuthenticationBroker).
        }

        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // ChallengeHandler function for AuthenticationManager that will be called whenever a secured resource is accessed.
            Credential credential = null;

            try
            {
                // AuthenticationManager will handle challenging the user for credentials.
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);
            }
            catch (Exception)
            {
                // Exception will be reported in calling function.
                throw;
            }

            return credential;
        }
    }

    #endregion
}