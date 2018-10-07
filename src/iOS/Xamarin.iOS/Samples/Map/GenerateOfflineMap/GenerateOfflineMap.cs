// Copyright 2016 Esri.
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
using CoreGraphics;
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
using UIKit;
using Xamarin.Auth;

namespace ArcGISRuntime.Samples.GenerateOfflineMap
{
    [Register("GenerateOfflineMap")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Generate an offline map",
        "Map",
        "This sample demonstrates how to generate an offline map for a web map in ArcGIS Portal.",
        "When the app starts, a web map is loaded from ArcGIS Online. The red border shows the extent that of the data that will be downloaded for use offline. Click the `Take map offline` button to start the offline map job (you will be prompted for your ArcGIS Online login). The progress bar will show the job's progress. When complete, the offline map will replace the online map in the map view.")]
    public class GenerateOfflineMap : UIViewController, IOAuthAuthorizeHandler
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIActivityIndicatorView _loadingIndicator = new UIActivityIndicatorView();
        private readonly UIToolbar _toolbar = new UIToolbar();
        private readonly UIButton _takeMapOfflineButton = new UIButton();

        // Overlay to show the load status for the map.
        private LoadingMapOverlay _loadingOverlay;

        // The job to generate an offline map.
        private GenerateOfflineMapJob _generateOfflineMapJob;

        // The extent of the data to take offline.
        private Envelope _areaOfInterest = new Envelope(-88.1541, 41.7690, -88.1471, 41.7720, SpatialReferences.Wgs84);

        // The ID for a web map item hosted on the server (water network map of Naperville IL).
        private string WebMapId = "acc027394bc84c2fb04d1ed317aac674";

        public GenerateOfflineMap()
        {
            Title = "Generate an offline map";
        }

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

        private void CreateLayout()
        {
            _takeMapOfflineButton.SetTitle("Generate", UIControlState.Normal);
            _takeMapOfflineButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _takeMapOfflineButton.SetTitleColor(UIColor.Gray, UIControlState.Disabled);
            _takeMapOfflineButton.TouchUpInside += TakeMapOfflineButton_Click;

            // Add the views.
            View.AddSubviews(_myMapView, _toolbar, _takeMapOfflineButton);
        }

        public override void ViewDidLoad()
        {
            CreateLayout();
            Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat controlHeight = 30;
                nfloat margin = 5;
                nfloat toolbarHeight = controlHeight + 2 * margin;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);
                _toolbar.Frame = new CGRect(0, View.Bounds.Height - toolbarHeight, View.Bounds.Width, toolbarHeight);
                _takeMapOfflineButton.Frame = new CGRect(margin, _toolbar.Frame.Top + margin, View.Bounds.Width - 2 * margin, controlHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private async void TakeMapOfflineButton_Click(object sender, EventArgs e)
        {
            // Create a path for the output mobile map.
            var tempPath = $"{Path.GetTempPath()}";
            var packagePath = Path.Combine(tempPath, @"NaperilleWaterNetwork");

            // Replace any existing output from the job.
            if (Directory.Exists(packagePath))
            {
                Directory.Delete(packagePath, true);
            }

            // Create the output directory.
            Directory.CreateDirectory(packagePath);

            try
            {
                // Show the loading overlay while the job is running.
                var bounds = View.Bounds; 
                _loadingOverlay = new LoadingMapOverlay(bounds, true);
                _loadingOverlay.UpdateLabel("Taking map offine ...");
                _loadingOverlay.OnCanceled += (s,evt) => 
                {
                    // The user canceled the job.
                    _generateOfflineMapJob.Cancel();
                };
                this.ParentViewController.View.Add(_loadingOverlay);

                // Create an offline map task with the current (online) map.
                OfflineMapTask takeMapOfflineTask = await OfflineMapTask.CreateAsync(_myMapView.Map);

                // Create the default parameters for the task, pass in the area of interest.
                GenerateOfflineMapParameters parameters = await takeMapOfflineTask.CreateDefaultGenerateOfflineMapParametersAsync(_areaOfInterest);

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
                    var errorBuilder = new System.Text.StringBuilder();
                    foreach (KeyValuePair<Layer, Exception> layerError in results.LayerErrors)
                    {
                        errorBuilder.AppendLine(string.Format("{0} : {1}", layerError.Key.Id, layerError.Value.Message));
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
                _takeMapOfflineButton.SetTitle("Map is offline", UIControlState.Normal);
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
                if (_loadingOverlay != null) { _loadingOverlay.Hide(); }
            }
        }

        // Show changes in job progress.
        private void OfflineMapJob_ProgressChanged(object sender, EventArgs e)
        {
            // Get the job.
            var job = sender as GenerateOfflineMapJob;

            // Dispatch to the UI thread.
            InvokeOnMainThread(() =>
            {
                // Show the percent complete and update the progress bar.
                string percentText = job.Progress > 0 ? job.Progress.ToString() + " %" : string.Empty;
                _loadingOverlay.UpdateProgress(job.Progress);
                _loadingOverlay.UpdateLabel("Taking map offline (" + percentText + ") ...");
            });
        }


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
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            Credential credential = null;

            try
            {
                // IOAuthAuthorizeHandler will challenge the user for OAuth credentials.
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);
            }
            catch (TaskCanceledException) { return credential; }
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
            if (_taskCompletionSource != null)
            {
                // Try to cancel any existing authentication task.
                _taskCompletionSource.TrySetCanceled();
            }

            // Create a task completion source.
            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();

            // Get the current iOS ViewController.
            UIViewController viewController = null;
            InvokeOnMainThread(() =>
            {
                viewController = UIApplication.SharedApplication.KeyWindow.RootViewController;
            });

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
                    if (_taskCompletionSource != null)
                    {
                        _taskCompletionSource.TrySetCanceled();
                    }
                }

                // Cancel authentication.
                authenticator.OnCancelled();
            };


            // Present the OAuth UI (on the app's UI thread) so the user can enter user name and password.
            InvokeOnMainThread(() =>
            {
                viewController.PresentViewController(authenticator.GetUI(), true, null);
            });

            // Return completion source task so the caller can await completion.
            return _taskCompletionSource.Task;
        }
        #endregion

        #endregion
    }

    /// <summary>
    /// View to show over the map while loading.
    /// </summary>
    public sealed class LoadingMapOverlay : UIView
    {
        // Progress bar to show the percent of the job complete.
        UIProgressView _progress;

        // Label to show additional job info.
        UILabel _label;

        // Event to report that the task was canceled.
        public event EventHandler OnCanceled;

        public LoadingMapOverlay(CGRect frame, bool showProgress) : base(frame)
        {
            // Semi-transparent black background.
            BackgroundColor = UIColor.Black;
            Alpha = 0.8f;
            AutoresizingMask = UIViewAutoresizing.All;

            // Find the center for positioning UI elements.
            nfloat centerX = Frame.Width / 2;
            nfloat centerY = Frame.Height / 2;

            // If showing progress, add a progress bar.
            if (showProgress)
            {
                _progress = new UIProgressView(UIProgressViewStyle.Bar);

                _progress.Frame = new CGRect(
                   centerX - _progress.Frame.Width / 2,
                   centerY - _progress.Frame.Height - 20,
                    _progress.Frame.Width,
                    _progress.Frame.Height);
                
                _progress.AutoresizingMask = UIViewAutoresizing.All;
                AddSubview(_progress);
                _progress.Progress = 0.0f;
            }
            else 
            {
                // Otherwise, show an activity indicator (spinner).
                UIActivityIndicatorView activitySpinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
                activitySpinner.Frame = new CGRect(
                    centerX - activitySpinner.Frame.Width / 2,
                    centerY - activitySpinner.Frame.Height - 20,
                    activitySpinner.Frame.Width,
                    activitySpinner.Frame.Height);
                activitySpinner.AutoresizingMask = UIViewAutoresizing.All;
                AddSubview(activitySpinner);
                activitySpinner.StartAnimating();
            }

            // Add a label to describe what's loading.
            _label = new UILabel(new CGRect(
                centerX - (Frame.Width - 20) / 2,
                centerY + 20,
                Frame.Width - 20,
                22
            ))
            {
                BackgroundColor = UIColor.Clear,
                TextColor = UIColor.White,
                Text = "--",
                TextAlignment = UITextAlignment.Center,
                AutoresizingMask = UIViewAutoresizing.All
            };
            AddSubview(_label);

            // Add a button that allows the user to cancel the task.
            UIButton cancelButton = new UIButton(UIButtonType.Plain);
            cancelButton.SetTitle("Cancel", UIControlState.Normal);
            cancelButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            cancelButton.Frame = new CGRect(
                centerX - (Frame.Width - 20) / 2,
                centerY + 50,
                Frame.Width - 20,
                30);
            cancelButton.TouchUpInside += (s, e) => 
            {
                // Raise the OnCanceled event so the task can be canceled.
                OnCanceled?.Invoke(this, null);
            };
            AddSubview(cancelButton);
        }

        // Update the progress bar value.
        public void UpdateProgress(float percentDone)
        {
            float progress = (float)(percentDone / 100.0);
            _progress.SetProgress(progress, true);
        }

        // Update the label text.
        public void UpdateLabel(string labelText)
        {
            _label.Text = labelText;
        }

        // Animate the disapperance of the view.
        public void Hide()
        {
            Animate(
                0.5,
                () => { Alpha = 0; },
                RemoveFromSuperview
            );
        }
    }
}