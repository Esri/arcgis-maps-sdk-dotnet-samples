// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Xamarin.Forms;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Esri.ArcGISRuntime.Xamarin.Forms;

#if __IOS__
using Xamarin.Auth;
using Xamarin.Forms.Platform.iOS;
using UIKit;
#endif

#if __ANDROID__
using Android.App;
using Application = Xamarin.Forms.Application;
using Xamarin.Auth;
using ArcGISRuntime.Droid;
using System.IO;
using Esri.ArcGISRuntime.Xamarin.Forms;
#endif

namespace ArcGISRuntimeXamarin.Samples.NavigateAR
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Navigate in AR",
        "Augmented reality",
        "Use a route displayed in the real world to navigate.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class RoutePlanner : ContentPage, IOAuthAuthorizeHandler
    {
        // Graphics overlays for showing stops and the calculated route.
        private GraphicsOverlay _routeOverlay;
        private GraphicsOverlay _stopsOverlay;

        // Hold the start and end point.
        private MapPoint _startPoint;
        private MapPoint _endPoint;

        // Routing.
        private RouteTask _routeTask;
        private Route _route;
        private RouteResult _routeResult;
        private RouteParameters _routeParameters;
        private Uri _routingUri = new Uri("https://route.arcgis.com/arcgis/rest/services/World/Route/NAServer/Route_World");

        public RoutePlanner()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Create and add the map.
            MyMapView.Map = new Map(Basemap.CreateImagery());

            MyMapView.PropertyChanged += async (o, e) =>
            {
                // Start the location display on the mapview.
                try
                {
                    // Permission request only needed on Android.
#if XAMARIN_ANDROID
                    // See implementation in MainActivity.cs in the Android platform project.
                    bool permissionGranted = await MainActivity.Instance.AskForLocationPermission();
                    if (!permissionGranted)
                    {
                        throw new Exception("Location permission not granted.");
                    }
#endif

                    if (e.PropertyName == nameof(MyMapView.LocationDisplay) && MyMapView.LocationDisplay != null)
                    {
                        MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
                        await MyMapView.LocationDisplay.DataSource.StartAsync();
                        MyMapView.LocationDisplay.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    await Application.Current.MainPage.DisplayAlert("Couldn't start location", ex.Message, "OK");
                }
            };

            // Enable authentication.
            SetOAuthInfo();
            var credential = await AuthenticationManager.Current.GenerateCredentialAsync(_routingUri);
            AuthenticationManager.Current.AddCredential(credential);

            try
            {
                // Create the route task.
                _routeTask = await RouteTask.CreateAsync(_routingUri);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to start sample", "OK");
                Debug.WriteLine(ex.Message);
            }

            // Create route display overlay and symbology.
            _routeOverlay = new GraphicsOverlay();
            SimpleLineSymbol routeSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Yellow, 1);
            _routeOverlay.Renderer = new SimpleRenderer(routeSymbol);

            // Create stop display overlay.
            _stopsOverlay = new GraphicsOverlay();

            // Add the overlays to the map.
            MyMapView.GraphicsOverlays.Add(_routeOverlay);
            MyMapView.GraphicsOverlays.Add(_stopsOverlay);

            // Wait for the user to place stops.
            MyMapView.GeoViewTapped += MapView_GeoViewTapped;

            // Update the help text.
            HelpLabel.Text = "Tap to set a start point";
        }

        private void MapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            if (_startPoint == null)
            {
                // Place the start point.
                _startPoint = e.Location;
                Graphic startGraphic = new Graphic(_startPoint,
                    new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.Green, 25));
                _stopsOverlay.Graphics.Add(startGraphic);

                // Update help text.
                HelpLabel.Text = "Tap to set an end point";
            }
            else if (_endPoint == null)
            {
                // Place the end point.
                _endPoint = e.Location;
                Graphic endGraphic = new Graphic(_endPoint,
                    new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, System.Drawing.Color.Red, 25));
                _stopsOverlay.Graphics.Add(endGraphic);

                // Update help text.
                HelpLabel.Text = "Solving route";

                // Solve the route.
                SolveRoute();
            }
        }

        private async void SolveRoute()
        {
            try
            {
                // Create route parameters and configure them to enable navigation.
                _routeParameters = await _routeTask.CreateDefaultParametersAsync();
                _routeParameters.ReturnStops = true;
                _routeParameters.ReturnDirections = true;
                _routeParameters.ReturnRoutes = true;

                // Prefer walking directions if available.
                TravelMode walkingMode =
                    _routeTask.RouteTaskInfo.TravelModes.FirstOrDefault(mode => mode.Name.Contains("Walk")) ??
                    _routeTask.RouteTaskInfo.TravelModes.First();
                _routeParameters.TravelMode = walkingMode;

                // Set the stops for the route.
                Stop stop1 = new Stop(_startPoint);
                Stop stop2 = new Stop(_endPoint);
                _routeParameters.SetStops(new[] { stop1, stop2 });

                // Calculate the route.
                _routeResult = await _routeTask.SolveRouteAsync(_routeParameters);

                // Get the first result.
                _route = _routeResult.Routes.First();

                // Create and show a graphic for the route.
                Graphic routeGraphic = new Graphic(_route.RouteGeometry);
                _routeOverlay.Graphics.Add(routeGraphic);

                // Allow the user to start navigating.
                EnableNavigation();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to calculate route.", "OK");
                Debug.WriteLine(ex);
            }
        }

        private void EnableNavigation()
        {
            // Pass the route result to the route viewer.
            RouteViewer.PassedRouteResult = _routeResult;

            // Configure the UI.
            StartARButton.IsEnabled = true;
            HelpLabel.Text = "You're ready to start navigating!";
        }

        private void StartARClicked(object sender, EventArgs e)
        {
            // Stop the current location source.
            MyMapView.LocationDisplay.DataSource.StopAsync();

            // Set the route for the route viewer.
            RouteViewer.PassedRouteResult = _routeResult;

            // Load the routeviewer as a new page on the navigation stack.
            Navigation.PopAsync();
            Navigation.PushAsync(new RouteViewer() { }, true);
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
#if __ANDROID__ || __IOS__
            AuthenticationManager.Current.OAuthAuthorizeHandler = this;
#endif
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
#if __ANDROID__ || __IOS__

#if __ANDROID__
            // Get the current Android Activity.
            Activity activity = (Activity)ArcGISRuntime.Droid.MainActivity.Instance;
#endif
#if __IOS__
            // Get the current iOS ViewController.
            UIViewController viewController = null;
            Device.BeginInvokeOnMainThread(() =>
            {
                viewController = UIApplication.SharedApplication.KeyWindow.RootViewController;
            });
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
                    if (_taskCompletionSource != null)
                    {
                        _taskCompletionSource.TrySetCanceled();
#if __ANDROID__
                        activity.FinishActivity(99);
#endif
                    }
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
            Device.BeginInvokeOnMainThread(() =>
            {
                viewController.PresentViewController(authenticator.GetUI(), true, null);
            });
#endif

#endif // (If Android or iOS)
            // Return completion source task so the caller can await completion.
            return _taskCompletionSource.Task;
        }

        #endregion IOAuthAuthorizationHandler implementation

        #endregion Authentication
    }
}