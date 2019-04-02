// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Auth;

namespace ArcGISRuntimeXamarin.Samples.OfflineBasemapByReference
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Generate offline map with local basemap",
        "Map",
        "Use the OfflineMapTask to take a web map offline while using a basemap previously taken offline.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("628e8e3521cf45e9a28a12fe10c02c4d")]
    public class OfflineBasemapByReference : Activity, IOAuthAuthorizeHandler
    {
        // Hold references to the UI controls.
        private MapView _mapView;
        private AlertDialog _alertDialog;
        private ProgressBar _progressIndicator;
        private Button _takeMapOfflineButton;

        // The job to generate an offline map.
        private GenerateOfflineMapJob _generateOfflineMapJob;

        // The extent of the data to take offline.
        private readonly Envelope _areaOfInterest = new Envelope(-88.1541, 41.7690, -88.1471, 41.7720, SpatialReferences.Wgs84);

        // The ID for a web map item hosted on the server (water network map of Naperville IL).
        private const string WebMapId = "acc027394bc84c2fb04d1ed317aac674";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Title = "Generate offline map with local basemap";
            CreateLayout();
            Initialize();
        }

        private void ConfigureOfflineJobForBasemap(GenerateOfflineMapParameters parameters, Action completionHandler)
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

            // Create the dialog for getting the user's choice.
            AlertDialog.Builder basemapAlert = new AlertDialog.Builder(this).SetMessage("Use the offline basemap?").SetTitle("Basemap choice");

            // Add the 'yes' choice. When the user selects it, the basemap directory will be set and the job will continue.
            basemapAlert = basemapAlert.SetPositiveButton("Yes", (o, e) =>
            {
                parameters.ReferenceBasemapDirectory = basemapBasePath;
                completionHandler.Invoke();
            });

            // Add the 'no' choice. When the user selects it, the job will continue using the online map.
            basemapAlert = basemapAlert.SetNegativeButton("No", (o, e) => completionHandler.Invoke());

            // Show the dialog.
            basemapAlert.Show();
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
                _mapView.Map = onlineMap;

                // Disable user interactions on the map (no panning or zooming from the initial extent).
                _mapView.InteractionOptions = new MapViewInteractionOptions
                {
                    IsEnabled = false
                };

                // Create a graphics overlay for the extent graphic and apply a renderer.
                SimpleLineSymbol aoiOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 3);
                GraphicsOverlay extentOverlay = new GraphicsOverlay
                {
                    Renderer = new SimpleRenderer(aoiOutlineSymbol)
                };
                _mapView.GraphicsOverlays.Add(extentOverlay);

                // Add a graphic to show the area of interest (extent) that will be taken offline.
                Graphic aoiGraphic = new Graphic(_areaOfInterest);
                extentOverlay.Graphics.Add(aoiGraphic);
            }
            catch (Exception ex)
            {
                // Show the exception message to the user.
                ShowStatusMessage(ex.Message);
            }
        }

        private async void TakeMapOfflineButton_Click(object sender, EventArgs e)
        {
            // Create a path for the output mobile map.
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

            // Show the progress dialog while the job is running.
            _alertDialog.Show();

            // Create an offline map task with the current (online) map.
            OfflineMapTask takeMapOfflineTask = await OfflineMapTask.CreateAsync(_mapView.Map);

            // Create the default parameters for the task, pass in the area of interest.
            GenerateOfflineMapParameters parameters = await takeMapOfflineTask.CreateDefaultGenerateOfflineMapParametersAsync(_areaOfInterest);

            // Configure basemap settings for the job.
            ConfigureOfflineJobForBasemap(parameters, async () =>
            {
                try
                {
                    // Create the job with the parameters and output location.
                    _generateOfflineMapJob = takeMapOfflineTask.GenerateOfflineMap(parameters, packagePath);

                    // Handle the progress changed event for the job.
                    _generateOfflineMapJob.ProgressChanged += OfflineMapJob_ProgressChanged;

                    // Await the job to generate geodatabases, export tile packages, and create the mobile map package.
                    GenerateOfflineMapResult results = await _generateOfflineMapJob.GetResultAsync();

                    // Check for job failure (writing the output was denied, e.g.).
                    if (_generateOfflineMapJob.Status != JobStatus.Succeeded)
                    {
                        // Report failure to the user.
                        ShowStatusMessage("Failed to take the map offline.");
                    }

                    // Check for errors with individual layers.
                    if (results.LayerErrors.Any())
                    {
                        // Build a string to show all layer errors.
                        StringBuilder errorBuilder = new StringBuilder();
                        foreach (KeyValuePair<Layer, Exception> layerError in results.LayerErrors)
                        {
                            errorBuilder.AppendLine($"{layerError.Key.Id} : {layerError.Value.Message}");
                        }

                        // Show layer errors.
                        ShowStatusMessage(errorBuilder.ToString());
                    }

                    // Display the offline map.
                    _mapView.Map = results.OfflineMap;

                    // Apply the original viewpoint for the offline map.
                    _mapView.SetViewpoint(new Viewpoint(_areaOfInterest));

                    // Enable map interaction so the user can explore the offline data.
                    _mapView.InteractionOptions.IsEnabled = true;

                    // Change the title and disable the "Take map offline" button.
                    _takeMapOfflineButton.Text = "Map is offline";
                    _takeMapOfflineButton.Enabled = false;
                }
                catch (TaskCanceledException)
                {
                    // Generate offline map task was canceled.
                    ShowStatusMessage("Taking map offline was canceled");
                }
                catch (Exception ex)
                {
                    // Exception while taking the map offline.
                    ShowStatusMessage(ex.Message);
                }
                finally
                {
                    // Hide the loading overlay when the job is done.
                    _alertDialog.Dismiss();
                }
            });
        }

        private void ShowStatusMessage(string message)
        {
            // Display the message to the user.
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetMessage(message).SetTitle("Alert").Show();
        }

        // Show changes in job progress.
        private void OfflineMapJob_ProgressChanged(object sender, EventArgs e)
        {
            // Get the job.
            GenerateOfflineMapJob job = sender as GenerateOfflineMapJob;

            // Dispatch to the UI thread.
            RunOnUiThread(() =>
            {
                // Show the percent complete and update the progress bar.
                string percentText = job.Progress > 0 ? job.Progress.ToString() + " %" : string.Empty;
                _progressIndicator.Progress = job.Progress;
                _alertDialog.SetMessage($"Taking map offline ({percentText}) ...");
            });
        }

        private void CreateLayout()
        {
            // Create the layout.
            LinearLayout layout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };

            // Add the generate button.
            _takeMapOfflineButton = new Button(this)
            {
                Text = "Take map offline"
            };
            _takeMapOfflineButton.Click += TakeMapOfflineButton_Click;
            layout.AddView(_takeMapOfflineButton);

            // Add the MapView.
            _mapView = new MapView(this);
            layout.AddView(_mapView);

            // Add the layout to the view.
            SetContentView(layout);

            // Create the progress dialog display.
            _progressIndicator = new ProgressBar(this);
            _progressIndicator.SetProgress(40, true);
            AlertDialog.Builder builder = new AlertDialog.Builder(this).SetView(_progressIndicator);
            builder.SetCancelable(true);
            builder.SetMessage("Generating offline map ...");
            _alertDialog = builder.Create();
            _alertDialog.SetButton("Cancel", (s, e) => { _generateOfflineMapJob.Cancel(); });
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

            // Set the OAuthAuthorizeHandler component (this class) for Android or iOS platforms.
            AuthenticationManager.Current.OAuthAuthorizeHandler = this;
        }

        // ChallengeHandler function that will be called whenever access to a secured resource is attempted.
        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            Credential credential = null;

            try
            {
                // IOAuthAuthorizeHandler will challenge the user for OAuth credentials.
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);
            }
            catch (TaskCanceledException)
            {
                return credential;
            }
            catch (Exception)
            {
                // Exception will be reported in calling function.
                throw;
            }

            return credential;
        }

        #region IOAuthAuthorizationHandler implementation

        // Use a TaskCompletionSource to track the completion of the authorization.
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;

        // IOAuthAuthorizeHandler.AuthorizeAsync implementation.
        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            // If the TaskCompletionSource is not null, authorization may already be in progress and should be canceled.
            // Try to cancel any existing authentication task.
            _taskCompletionSource?.TrySetCanceled();

            // Create a task completion source.
            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();

            // Get the current Android Activity.
            Activity activity = this;

            // Create a new Xamarin.Auth.OAuth2Authenticator using the information passed in.
            OAuth2Authenticator authenticator = new OAuth2Authenticator(
                clientId: AppClientId,
                scope: "",
                authorizeUrl: authorizeUri,
                redirectUrl: callbackUri)
            {
                ShowErrors = false,
                // Allow the user to cancel the OAuth attempt.
                AllowCancel = true
            };

            // Define a handler for the OAuth2Authenticator.Completed event.
            authenticator.Completed += (sender, authArgs) =>
            {
                try
                {
                    // Check if the user is authenticated.
                    if (authArgs.IsAuthenticated)
                    {
                        // If authorization was successful, get the user's account.
                        Xamarin.Auth.Account authenticatedAccount = authArgs.Account;

                        // Set the result (Credential) for the TaskCompletionSource.
                        _taskCompletionSource.SetResult(authenticatedAccount.Properties);
                    }
                    else
                    {
                        throw new Exception("Unable to authenticate user.");
                    }
                }
                catch (Exception ex)
                {
                    // If authentication failed, set the exception on the TaskCompletionSource.
                    _taskCompletionSource.TrySetException(ex);

                    // Cancel authentication.
                    authenticator.OnCancelled();
                }
                finally
                {
                    // Dismiss the OAuth login.
                    activity.FinishActivity(99);
                }
            };

            // If an error was encountered when authenticating, set the exception on the TaskCompletionSource.
            authenticator.Error += (sndr, errArgs) =>
            {
                // If the user cancels, the Error event is raised but there is no exception ... best to check first.
                if (errArgs.Exception != null)
                {
                    _taskCompletionSource.TrySetException(errArgs.Exception);
                }
                else
                {
                    // Login canceled: dismiss the OAuth login.
                    if (_taskCompletionSource != null)
                    {
                        _taskCompletionSource.TrySetCanceled();
                        activity.FinishActivity(99);
                    }
                }

                // Cancel authentication.
                authenticator.OnCancelled();
            };

            // Present the OAuth UI so the user can enter user name and password.
            Android.Content.Intent intent = authenticator.GetUI(activity);
            activity.StartActivityForResult(intent, 99);

            // Return completion source task so the caller can await completion.
            return _taskCompletionSource.Task;
        }

        #endregion

        #endregion
    }
}