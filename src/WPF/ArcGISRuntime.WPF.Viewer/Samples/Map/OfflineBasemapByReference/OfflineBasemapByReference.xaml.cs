// Copyright 2019 Esri.
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using ArcGISRuntime.Samples.Managers;

namespace ArcGISRuntime.WPF.Samples.OfflineBasemapByReference
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Generate offline map with local basemap",
        category: "Map",
        description: "Use the `OfflineMapTask` to take a web map offline, but instead of downloading an online basemap, use one which is already on the device.",
        instructions: "1. Use the button to start taking the map offline.",
        tags: new[] { "Offline" })]
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

        private void ConfigureOfflineJobForBasemap(GenerateOfflineMapParameters parameters)
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

            // Get the user's choice.
            MessageBoxResult userChoice = MessageBox.Show("Use the offline basemap?", "Basemap choice", MessageBoxButton.YesNo);

            // If the user approves, use the offline basemap.
            if (userChoice == MessageBoxResult.Yes)
            {
                parameters.ReferenceBasemapDirectory = basemapBasePath;
            }
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
                LoadingIndicator.Visibility = Visibility.Collapsed;

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
                MessageBox.Show(ex.ToString(), "Error loading map");
            }
        }

        private async void TakeMapOfflineButton_Click(object sender, RoutedEventArgs e)
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
                BusyIndicator.Visibility = Visibility.Visible;

                // Create an offline map task with the current (online) map.
                OfflineMapTask takeMapOfflineTask = await OfflineMapTask.CreateAsync(MyMapView.Map);

                // Create the default parameters for the task, pass in the area of interest.
                GenerateOfflineMapParameters parameters = await takeMapOfflineTask.CreateDefaultGenerateOfflineMapParametersAsync(_areaOfInterest);

                // Configure basemap settings for the job.
                ConfigureOfflineJobForBasemap(parameters);

                // Create the job with the parameters and output location.
                _generateOfflineMapJob = takeMapOfflineTask.GenerateOfflineMap(parameters, packagePath);

                // Handle the progress changed event for the job.
                _generateOfflineMapJob.ProgressChanged += OfflineMapJob_ProgressChanged;

                // Await the job to generate geodatabases, export tile packages, and create the mobile map package.
                GenerateOfflineMapResult results = await _generateOfflineMapJob.GetResultAsync();

                // Check for job failure (writing the output was denied, e.g.).
                if (_generateOfflineMapJob.Status != JobStatus.Succeeded)
                {
                    MessageBox.Show("Generate offline map package failed.", "Job status");
                    BusyIndicator.Visibility = Visibility.Collapsed;
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
                    MessageBox.Show(errorText, "Layer errors");
                }

                // Display the offline map.
                MyMapView.Map = results.OfflineMap;

                // Apply the original viewpoint for the offline map.
                MyMapView.SetViewpoint(new Viewpoint(_areaOfInterest));

                // Enable map interaction so the user can explore the offline data.
                MyMapView.InteractionOptions.IsEnabled = true;

                // Show a message that the map is offline.
                MessageArea.Visibility = Visibility.Visible;

                // Hide the controls.
                TakeOfflineArea.Visibility = Visibility.Collapsed;
            }
            catch (TaskCanceledException)
            {
                // Generate offline map task was canceled.
                MessageBox.Show("Taking map offline was canceled");
            }
            catch (Exception ex)
            {
                // Exception while taking the map offline.
                MessageBox.Show(ex.Message, "Offline map error");
            }
            finally
            {
                // Hide the activity indicator when the job is done.
                BusyIndicator.Visibility = Visibility.Collapsed;
            }
        }

        // Show changes in job progress.
        private void OfflineMapJob_ProgressChanged(object sender, EventArgs e)
        {
            // Get the job.
            GenerateOfflineMapJob job = sender as GenerateOfflineMapJob;

            // Dispatch to the UI thread.
            Dispatcher.Invoke(() =>
            {
                // Show the percent complete and update the progress bar.
                Percentage.Text = job.Progress > 0 ? job.Progress.ToString() + " %" : string.Empty;
                ProgressBar.Value = job.Progress;
            });
        }

        private void CancelJobButton_Click(object sender, RoutedEventArgs e)
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

        // - A URL for redirecting after a successful authorization (this must be a URL configured with the app).
        private const string OAuthRedirectUrl = @"my-ags-app://auth";

        private void SetOAuthInfo()
        {
            // Register the server information with the AuthenticationManager, including the OAuth settings.
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

            // Use the custom OAuthAuthorize class (defined in this module) to handle OAuth communication.
            AuthenticationManager.Current.OAuthAuthorizeHandler = new OAuthAuthorize();

            // Use a function in this class to challenge for credentials.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
        }

        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // ChallengeHandler function for AuthenticationManager that will be called whenever a secured resource is accessed.
            Credential credential;

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

        private class OAuthAuthorize : IOAuthAuthorizeHandler
        {
            // A window to contain the OAuth UI.
            private Window _authWindow;

            // A TaskCompletionSource to track the completion of the authorization.
            private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;

            // URL for the authorization callback result (the redirect URI configured for the application).
            private string _callbackUrl;

            // URL that handles the OAuth request.
            private string _authorizeUrl;

            // A function to handle authorization requests. It takes the URIs for the secured service, the authorization endpoint, and the redirect URI.
            public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
            {
                // If the TaskCompletionSource.Task has not completed, authorization is in progress.
                if (_taskCompletionSource != null || _authWindow != null)
                {
                    // Allow only one authorization process at a time.
                    throw new Exception("Authorization is in progress");
                }

                // Store the authorization and redirect URLs.
                _authorizeUrl = authorizeUri.AbsoluteUri;
                _callbackUrl = callbackUri.AbsoluteUri;

                // Create a task completion source to track completion.
                _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();

                // Call a function to show the login controls, make sure it runs on the UI thread.
                Dispatcher dispatcher = Application.Current.Dispatcher;
                if (dispatcher == null || dispatcher.CheckAccess())
                    AuthorizeOnUIThread(_authorizeUrl);
                else
                {
                    Action authorizeOnUIAction = () => AuthorizeOnUIThread(_authorizeUrl);
                    dispatcher.BeginInvoke(authorizeOnUIAction);
                }

                // Return the task associated with the TaskCompletionSource.
                return _taskCompletionSource.Task;
            }

            // A function to challenge for OAuth credentials on the UI thread.
            private void AuthorizeOnUIThread(string authorizeUri)
            {
                // Create a WebBrowser control to display the authorize page.
                WebBrowser authBrowser = new WebBrowser();

                // Handle the navigating event for the browser to check for a response sent to the redirect URL.
                authBrowser.Navigating += WebBrowserOnNavigating;

                // Display the web browser in a new window.
                _authWindow = new Window
                {
                    Content = authBrowser,
                    Height = 420,
                    Width = 350,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                // Set the app's window as the owner of the browser window (if main window closes, so will the browser).
                if (Application.Current != null && Application.Current.MainWindow != null)
                {
                    _authWindow.Owner = Application.Current.MainWindow;
                }

                // Handle the window closed event then navigate to the authorize url.
                _authWindow.Closed += OnWindowClosed;
                authBrowser.Navigate(authorizeUri);

                // Display the Window.
                _authWindow?.ShowDialog();
            }

            private void OnWindowClosed(object sender, EventArgs e)
            {
                // If the browser window closes, return the focus to the main window.
                _authWindow?.Owner?.Focus();

                // If the task wasn't completed, the user must have closed the window without logging in.
                if (_taskCompletionSource != null && !_taskCompletionSource.Task.IsCompleted)
                {
                    // Set the task completion to indicate a canceled operation.
                    _taskCompletionSource.TrySetCanceled();
                }

                _taskCompletionSource = null;
                _authWindow = null;
            }

            // Handle browser navigation (page content changing).
            private void WebBrowserOnNavigating(object sender, NavigatingCancelEventArgs e)
            {
                // Check for a response to the callback url.
                WebBrowser webBrowser = sender as WebBrowser;
                Uri uri = e.Uri;

                // If no browser, uri, or an empty url return.
                if (webBrowser == null || uri == null || _taskCompletionSource == null || String.IsNullOrEmpty(uri.AbsoluteUri))
                {
                    return;
                }

                // Check if the new content is from the callback url.
                bool isRedirected = uri.AbsoluteUri.StartsWith(_callbackUrl);

                if (isRedirected)
                {
                    // Cancel the event to prevent it from being handled elsewhere.
                    e.Cancel = true;

                    // Get a local copy of the task completion source.
                    TaskCompletionSource<IDictionary<string, string>> tcs = _taskCompletionSource;
                    _taskCompletionSource = null;

                    // Close the window.
                    _authWindow?.Close();

                    // Call a helper function to decode the response parameters (which includes the authorization key).
                    IDictionary<string, string> authResponse = DecodeParameters(uri);

                    // Set the result for the task completion source.
                    tcs.SetResult(authResponse);
                }
            }

            // A helper function that decodes values from a querystring into a dictionary of keys and values.
            private static IDictionary<string, string> DecodeParameters(Uri uri)
            {
                // Create a dictionary of key value pairs returned in an OAuth authorization response URI query string.
                string answer = "";

                // Get the values from the URI fragment or query string.
                if (!string.IsNullOrEmpty(uri.Fragment))
                {
                    answer = uri.Fragment.Substring(1);
                }
                else
                {
                    if (!string.IsNullOrEmpty(uri.Query))
                    {
                        answer = uri.Query.Substring(1);
                    }
                }

                // Parse parameters into key / value pairs.
                Dictionary<string, string> keyValueDictionary = new Dictionary<string, string>();
                string[] keysAndValues = answer.Split(new[] {'&'}, StringSplitOptions.RemoveEmptyEntries);
                foreach (string kvString in keysAndValues)
                {
                    string[] pair = kvString.Split('=');
                    string key = pair[0];
                    string value = string.Empty;
                    if (key.Length > 1)
                    {
                        value = Uri.UnescapeDataString(pair[1]);
                    }

                    keyValueDictionary.Add(key, value);
                }

                // Return the dictionary of string keys/values.
                return keyValueDictionary;
            }
        }

        #endregion
    }
}