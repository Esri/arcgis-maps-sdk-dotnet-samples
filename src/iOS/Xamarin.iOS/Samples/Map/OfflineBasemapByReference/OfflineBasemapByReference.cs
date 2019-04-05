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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Auth;

namespace ArcGISRuntimeXamarin.Samples.OfflineBasemapByReference
{
    [Register("OfflineBasemapByReference")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Generate offline map with local basemap",
        "Map",
        "Use the OfflineMapTask to take a web map offline while using a basemap previously taken offline.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("628e8e3521cf45e9a28a12fe10c02c4d")]
    public class OfflineBasemapByReference : UIViewController, IOAuthAuthorizeHandler
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private UIActivityIndicatorView _loadingIndicator;
        private UIBarButtonItem _takeMapOfflineButton;
        private UILabel _statusLabel;

        // The job to generate an offline map.
        private GenerateOfflineMapJob _generateOfflineMapJob;

        // The extent of the data to take offline.
        private readonly Envelope _areaOfInterest = new Envelope(-88.1541, 41.7690, -88.1471, 41.7720, SpatialReferences.Wgs84);

        // The ID for a web map item hosted on the server (water network map of Naperville IL).
        private const string WebMapId = "acc027394bc84c2fb04d1ed317aac674";

        public OfflineBasemapByReference()
        {
            Title = "Generate offline map with local basemap";
        }

        private void ConfigureOfflineJobForBasemap(GenerateOfflineMapParameters parameters, Action completionAction)
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

            UIAlertController basemapAlert = UIAlertController.Create("Basemap choice", "Use the offline basemap?", UIAlertControllerStyle.Alert);

            // Configure the directory and move on when the user says 'yes'. 
            basemapAlert.AddAction(UIAlertAction.Create("Yes", UIAlertActionStyle.Default,
                uiAlertAction =>
                {
                    parameters.ReferenceBasemapDirectory = basemapBasePath;
                    completionAction.Invoke();
                }));

            // Do nothing and move on if the user says no.
            basemapAlert.AddAction(UIAlertAction.Create("No", UIAlertActionStyle.Cancel, action => completionAction.Invoke()));

            // Show the alert.
            PresentViewController(basemapAlert, true, null);
        }

        // Note: all code below (except call to ConfigureOfflineJobForBasemap) is identical to code in the Generate offline map sample.

        #region Generate offline map

        private async void Initialize()
        {
            try
            {
                // Start the loading indicator.
                _loadingIndicator.StartAnimating();

                // Call a function to set up the AuthenticationManager for OAuth.
                SetOAuthInfo();

                // Create the ArcGIS Online portal.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync();

                // Get the Naperville water web map item using its ID.
                PortalItem webmapItem = await PortalItem.CreateAsync(portal, WebMapId);

                // Create a map from the web map item.
                Map onlineMap = new Map(webmapItem);

                // Display the map in the MapView.
                _myMapView.Map = onlineMap;

                // Disable user interactions on the map (no panning or zooming from the initial extent).
                _myMapView.InteractionOptions = new MapViewInteractionOptions
                {
                    IsEnabled = false
                };

                // Create a graphics overlay for the extent graphic and apply a renderer.
                SimpleLineSymbol aoiOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 3);
                GraphicsOverlay extentOverlay = new GraphicsOverlay
                {
                    Renderer = new SimpleRenderer(aoiOutlineSymbol)
                };
                _myMapView.GraphicsOverlays.Add(extentOverlay);

                // Add a graphic to show the area of interest (extent) that will be taken offline.
                Graphic aoiGraphic = new Graphic(_areaOfInterest);
                extentOverlay.Graphics.Add(aoiGraphic);

                // Hide the map loading progress indicator.
                _loadingIndicator.StopAnimating();
            }
            catch (Exception ex)
            {
                // Show the exception message to the user.
                UIAlertController messageAlert = UIAlertController.Create("Error", ex.Message, UIAlertControllerStyle.Alert);
                messageAlert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(messageAlert, true, null);
            }
        }

        private async void TakeMapOfflineButton_Click(object sender, EventArgs e)
        {
            // Show the loading indicator.
            _loadingIndicator.StartAnimating();

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
                packagePath = Path.Combine(tempPath, @"NapervilleWaterNetwork" + num);
                num++;
            }

            // Create the output directory.
            Directory.CreateDirectory(packagePath);

            // Show the loading overlay while the job is running.
            _statusLabel.Text = "Taking map offline...";

            // Create an offline map task with the current (online) map.
            OfflineMapTask takeMapOfflineTask = await OfflineMapTask.CreateAsync(_myMapView.Map);

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
                        UIAlertController messageAlert = UIAlertController.Create("Error", "Failed to take the map offline.", UIAlertControllerStyle.Alert);
                        messageAlert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                        PresentViewController(messageAlert, true, null);
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
                        UIAlertController messageAlert = UIAlertController.Create("Error", errorBuilder.ToString(), UIAlertControllerStyle.Alert);
                        messageAlert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                        PresentViewController(messageAlert, true, null);
                    }

                    // Display the offline map.
                    _myMapView.Map = results.OfflineMap;

                    // Apply the original viewpoint for the offline map.
                    _myMapView.SetViewpoint(new Viewpoint(_areaOfInterest));

                    // Enable map interaction so the user can explore the offline data.
                    _myMapView.InteractionOptions.IsEnabled = true;

                    // Change the title and disable the "Take map offline" button.
                    _statusLabel.Text = "Map is offline";
                    _takeMapOfflineButton.Enabled = false;
                }
                catch (TaskCanceledException)
                {
                    // Generate offline map task was canceled.
                    UIAlertController messageAlert = UIAlertController.Create("Canceled", "Taking map offline was canceled", UIAlertControllerStyle.Alert);
                    messageAlert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(messageAlert, true, null);
                }
                catch (Exception ex)
                {
                    // Exception while taking the map offline.
                    UIAlertController messageAlert = UIAlertController.Create("Error", ex.Message, UIAlertControllerStyle.Alert);
                    messageAlert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(messageAlert, true, null);
                }
                finally
                {
                    // Hide the loading overlay when the job is done.
                    _loadingIndicator.StopAnimating();
                }
            });
        }

        // Show changes in job progress.
        private void OfflineMapJob_ProgressChanged(object sender, EventArgs e)
        {
            // Get the job.
            GenerateOfflineMapJob job = sender as GenerateOfflineMapJob;

            // Dispatch to the UI thread.
            InvokeOnMainThread(() =>
            {
                // Show the percent complete and update the progress bar.
                _statusLabel.Text = $"Taking map offline ({job.Progress}%) ...";
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _takeMapOfflineButton = new UIBarButtonItem("Generate offline map", UIBarButtonItemStyle.Plain, TakeMapOfflineButton_Click);

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _takeMapOfflineButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            _statusLabel = new UILabel
            {
                Text = "Use the button to take the map offline.",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _loadingIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
            _loadingIndicator.TranslatesAutoresizingMaskIntoConstraints = false;
            _loadingIndicator.BackgroundColor = UIColor.FromWhiteAlpha(0, .6f);

            // Add the views.
            View.AddSubviews(_myMapView, toolbar, _loadingIndicator, _statusLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                _statusLabel.TopAnchor.ConstraintEqualTo(_myMapView.TopAnchor),
                _statusLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _statusLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _statusLabel.HeightAnchor.ConstraintEqualTo(40),

                _loadingIndicator.TopAnchor.ConstraintEqualTo(_statusLabel.BottomAnchor),
                _loadingIndicator.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _loadingIndicator.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _loadingIndicator.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
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

            // Get the current iOS ViewController.
            UIViewController viewController = null;
            InvokeOnMainThread(() => { viewController = UIApplication.SharedApplication.KeyWindow.RootViewController; });

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
                    // Dismiss the OAuth UI when complete.
                    viewController.DismissViewController(true, null);

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
                }

                // Cancel authentication.
                authenticator.OnCancelled();
            };


            // Present the OAuth UI (on the app's UI thread) so the user can enter user name and password.
            InvokeOnMainThread(() => { viewController.PresentViewController(authenticator.GetUI(), true, null); });

            // Return completion source task so the caller can await completion.
            return _taskCompletionSource.Task;
        }

        #endregion
    }

    #endregion
}