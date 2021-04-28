// Copyright 2019 Esri.
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
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Xamarin.Forms;
#if __IOS__
using Xamarin.Auth;
using UIKit;

#endif

#if __ANDROID__
using Android.App;
using Application = Xamarin.Forms.Application;
using Xamarin.Auth;
#endif

namespace ArcGISRuntimeXamarin.Samples.OfflineBasemapByReference
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Generate offline map with local basemap",
        category: "Map",
        description: "Use the `OfflineMapTask` to take a web map offline, but instead of downloading an online basemap, use one which is already on the device.",
        instructions: "1. Use the button to start taking the map offline.",
        tags: new[] { "Offline" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("628e8e3521cf45e9a28a12fe10c02c4d")]
    public partial class OfflineBasemapByReference : ContentPage, IOAuthAuthorizeHandler
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

            // Wait for the user to choose whether to use the offline basemap.
            bool useBasemap = await Application.Current.MainPage.DisplayAlert("Basemap choice", "Use the offline basemap?", "Yes", "No");

            if (useBasemap)
            {
                // Configure the offline basemap if the user said yes.
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
                LoadingIndicator.IsVisible = false;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Alert", ex.ToString(), "OK");
            }
        }

        private async void TakeMapOfflineButton_Click(object sender, EventArgs e)
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

            try
            {
                // Show the progress indicator while the job is running.
                BusyIndicator.IsVisible = true;

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
                    await Application.Current.MainPage.DisplayAlert("Alert", "Generate offline map package failed.", "OK");
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
                    await Application.Current.MainPage.DisplayAlert("Alert", errorText, "OK");
                }

                // Display the offline map.
                MyMapView.Map = results.OfflineMap;

                // Apply the original viewpoint for the offline map.
                MyMapView.SetViewpoint(new Viewpoint(_areaOfInterest));

                // Enable map interaction so the user can explore the offline data.
                MyMapView.InteractionOptions.IsEnabled = true;

                // Hide the "Take map offline" button.
                TakeOfflineArea.IsVisible = false;

                // Show a message that the map is offline.
                MessageArea.IsVisible = true;
            }
            catch (TaskCanceledException)
            {
                // Generate offline map task was canceled.
                await Application.Current.MainPage.DisplayAlert("Alert", "Taking map offline was canceled", "OK");
            }
            catch (Exception ex)
            {
                // Exception while taking the map offline.
                await Application.Current.MainPage.DisplayAlert("Alert", ex.Message, "OK");
            }
            finally
            {
                // Hide the activity indicator when the job is done.
                BusyIndicator.IsVisible = false;
            }
        }

        // Show changes in job progress.
        private void OfflineMapJob_ProgressChanged(object sender, EventArgs e)
        {
            // Get the job.
            GenerateOfflineMapJob job = sender as GenerateOfflineMapJob;

            // Dispatch to the UI thread.
            Device.BeginInvokeOnMainThread(() =>
            {
                // Show the percent complete and update the progress bar.
                Percentage.Text = job.Progress > 0 ? job.Progress.ToString() + " %" : string.Empty;
                ProgressBar.Progress = job.Progress / 100.0;
            });
        }

        private void CancelJobButton_Click(object sender, EventArgs e)
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
        private const string AppClientId = @"6wMAmbUEX1rvsOb4";

        // - A URL for redirecting after a successful authorization (this must be a URL configured with the app).
        private const string OAuthRedirectUrl = @"forms-samples-app://auth";

        private void SetOAuthInfo()
        {
            // Register the server information with the AuthenticationManager.
            ServerInfo serverInfo = new ServerInfo(new Uri(ServerUrl))
            {
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit,
                OAuthClientInfo = new OAuthClientInfo(AppClientId, new Uri(OAuthRedirectUrl))
            };

            // Register this server with AuthenticationManager.
            AuthenticationManager.Current.RegisterServer(serverInfo);

            // Use a function in this class to challenge for credentials.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Set the OAuthAuthorizeHandler component (this class) for Android or iOS platforms.
#if __ANDROID__ || __IOS__
            AuthenticationManager.Current.OAuthAuthorizeHandler = this;
#endif
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
#if __ANDROID__ || __IOS__

#if __ANDROID__
            // Get the current Android Activity.
            Activity activity = (Activity)ArcGISRuntime.Droid.MainActivity.Instance;
#endif
#if __IOS__
            // Get the current iOS ViewController.
            UIViewController viewController = null;
            Device.BeginInvokeOnMainThread(() => { viewController = UIApplication.SharedApplication.KeyWindow.RootViewController; });
#endif
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
#if __IOS__
                    // Dismiss the OAuth UI when complete.
                    viewController.DismissViewController(true, null);
#endif

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
#if __ANDROID__
                    activity.FinishActivity(99);
#endif
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
                    _taskCompletionSource?.TrySetCanceled();
#if __ANDROID__
                    activity.FinishActivity(99);
#endif
                }

                // Cancel authentication.
                authenticator.OnCancelled();
            };

            // Present the OAuth UI so the user can enter user name and password.
#if __ANDROID__
            Android.Content.Intent intent = authenticator.GetUI(activity);
            activity.StartActivityForResult(intent, 99);
#endif
#if __IOS__
            // Present the OAuth UI (on the app's UI thread) so the user can enter user name and password.
            Device.BeginInvokeOnMainThread(() => { viewController.PresentViewController(authenticator.GetUI(), true, null); });
#endif

#endif // (If Android or iOS)
            // Return completion source task so the caller can await completion.
            return _taskCompletionSource.Task;
        }

        #endregion
    }

    #endregion
}