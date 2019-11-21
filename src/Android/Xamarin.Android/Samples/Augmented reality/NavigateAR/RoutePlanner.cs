// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISRuntimeXamarin.Samples.NavigateAR
{
    [Activity(ConfigurationChanges =
        ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Navigate in AR",
        "Augmented reality",
        "Use a route displayed in the real world to navigate.",
        "")]
    public class NavigateARRoutePlanner : AppCompatActivity, IOAuthAuthorizeHandler
    {
        // Hold references to the UI controls.
        private MapView _mapView;
        private TextView _helpLabel;
        private Button _navigateButton;

        // Overlays for showing the stops and calculated route.
        private GraphicsOverlay _routeOverlay;
        private GraphicsOverlay _stopsOverlay;

        // Start and end point for the route.
        private MapPoint _startPoint;
        private MapPoint _endPoint;

        // Objects for route calculation.
        private RouteTask _routeTask;
        private Route _route;
        private RouteResult _routeResult;
        private RouteParameters _routeParameters;

        // URL to the routing service; requires login.
        private readonly Uri _routingUri =
            new Uri("https://route.arcgis.com/arcgis/rest/services/World/Route/NAServer/Route_World");

        // Auth.
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;
        private const string ServerUrl = "https://www.arcgis.com/sharing/rest";
        private const string AppClientId = @"lgAdHkYZYlwwfAhC";
        private const string OAuthRedirectUrl = @"my-ags-app://auth";

        // Permissions and permission request.
        private readonly string[] _requestedPermissions = { Manifest.Permission.AccessFineLocation };
        private const int RequestCode = 35;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Navigate in AR";

            CreateLayout();

            RequestPermissions();
        }

        private void CreateLayout()
        {
            // Create and show the layout.
            SetContentView(ArcGISRuntime.Resource.Layout.NavigateARRoutePlanner);

            // Set up control references.
            _helpLabel = FindViewById<TextView>(ArcGISRuntime.Resource.Id.helpLabel);
            _mapView = FindViewById<MapView>(ArcGISRuntime.Resource.Id.mapView);
            _navigateButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.navigateButton);
        }

        private async void Initialize()
        {
            // Create and show a map.
            _mapView.Map = new Map(Basemap.CreateImagery());

            try
            {
                // Enable location display.
                _mapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
                await _mapView.LocationDisplay.DataSource.StartAsync();
                _mapView.LocationDisplay.IsEnabled = true;

                // Configure authentication.
                SetOAuthInfo();
                var credential = await AuthenticationManager.Current.GenerateCredentialAsync(_routingUri);
                AuthenticationManager.Current.AddCredential(credential);

                // Create the route task.
                _routeTask = await RouteTask.CreateAsync(_routingUri);

                // Create route display overlay and symbology.
                _routeOverlay = new GraphicsOverlay();
                SimpleLineSymbol routeSymbol =
                    new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Yellow, 1);
                _routeOverlay.Renderer = new SimpleRenderer(routeSymbol);

                // Create stop display overlay.
                _stopsOverlay = new GraphicsOverlay();

                // Add the overlays to the map.
                _mapView.GraphicsOverlays.Add(_routeOverlay);
                _mapView.GraphicsOverlays.Add(_stopsOverlay);

                // Enable tap-to-place stops.
                _mapView.GeoViewTapped += MapView_GeoViewTapped;

                // Update the UI.
                _helpLabel.Text = "Tap to set a start point";
            }
            catch (Exception ex)
            {
                new Android.Support.V7.App.AlertDialog.Builder(this).SetMessage("Failed to start sample")
                    .SetTitle("Error").Show();
                System.Diagnostics.Debug.WriteLine(ex);
            }
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
                _helpLabel.Text = "Tap to set an end point";
            }
            else if (_endPoint == null)
            {
                // Place the end point.
                _endPoint = e.Location;
                Graphic endGraphic = new Graphic(_endPoint,
                    new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, System.Drawing.Color.Red, 25));
                _stopsOverlay.Graphics.Add(endGraphic);

                // Update help text.
                _helpLabel.Text = "Solving route";

                // Solve the route.
                SolveRoute();
            }
        }

        private void EnableNavigation()
        {
            // Pass the route result to the route viewer.
            RouteViewerAR.PassedRouteResult = _routeResult;

            // Configure the UI.
            _navigateButton.Click += NavigateButton_Click;
            _navigateButton.Visibility = Android.Views.ViewStates.Visible;
            _helpLabel.Text = "You're ready to start navigating!";
        }

        private void NavigateButton_Click(object sender, EventArgs e)
        {
            // Start the AR navigation activity.
            Intent myIntent = new Intent(this, typeof(RouteViewerAR));
            StartActivity(myIntent);
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
                new Android.Support.V7.App.AlertDialog.Builder(this).SetMessage("Failed to calculate route.")
                    .SetTitle("Error").Show();
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void RequestPermissions()
        {
            if (ContextCompat.CheckSelfPermission(this, _requestedPermissions[0]) == Permission.Granted)
            {
                Initialize();
            }
            else
            {
                ActivityCompat.RequestPermissions(this, _requestedPermissions, NavigateARRoutePlanner.RequestCode);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == NavigateARRoutePlanner.RequestCode && grantResults[0] == Permission.Granted)
            {
                Initialize();
            }
            else
            {
                Toast.MakeText(this, "Location permissions needed for this sample", ToastLength.Short).Show();
            }

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        #region OAuth

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

            // Set the OAuthAuthorizeHandler component (this class).
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
            catch (TaskCanceledException)
            {
                return credential;
            }

            return credential;
        }

        // IOAuthAuthorizeHandler.AuthorizeAsync implementation.
        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            // If the TaskCompletionSource is not null, authorization is in progress.
            if (_taskCompletionSource != null)
            {
                // Allow only one authorization process at a time.
                throw new Exception();
            }

            // Create a task completion source.
            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();

            // Create a new Xamarin.Auth.OAuth2Authenticator using the information passed in.
            Xamarin.Auth.OAuth2Authenticator authenticator = new Xamarin.Auth.OAuth2Authenticator(
                clientId: AppClientId,
                scope: "",
                authorizeUrl: authorizeUri,
                redirectUrl: callbackUri)
            {
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
                }
                catch (Exception ex)
                {
                    // If authentication failed, set the exception on the TaskCompletionSource.
                    _taskCompletionSource.SetException(ex);
                }
                finally
                {
                    // End the OAuth login activity.
                    this.FinishActivity(99);
                }
            };

            // If an error was encountered when authenticating, set the exception on the TaskCompletionSource.
            authenticator.Error += (sender, errArgs) =>
            {
                // If the user cancels, the Error event is raised but there is no exception ... best to check first.
                if (errArgs.Exception != null)
                {
                    _taskCompletionSource.SetException(errArgs.Exception);
                }
                else
                {
                    // Login canceled: end the OAuth login activity.
                    if (_taskCompletionSource != null)
                    {
                        _taskCompletionSource.TrySetCanceled();
                        this.FinishActivity(99);
                    }
                }
            };

            // Present the OAuth UI (Activity) so the user can enter user name and password.
            Intent intent = authenticator.GetUI(this);
            this.StartActivityForResult(intent, 99);

            // Return completion source task so the caller can await completion.
            return _taskCompletionSource.Task;
        }

        #endregion OAuth
    }
}