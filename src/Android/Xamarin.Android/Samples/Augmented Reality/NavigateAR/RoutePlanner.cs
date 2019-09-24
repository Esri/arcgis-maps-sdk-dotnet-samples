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
using System.Linq;
using System.Threading.Tasks;
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
using Xamarin.Auth;

namespace ArcGISRuntimeXamarin.Samples.NavigateAR
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Navigate in AR",
        "Augmented reality",
        "Use a route displayed in the real world to navigate.",
        "")]
    public class NavigateAR_RoutePlanner : AppCompatActivity, IOAuthAuthorizeHandler
    {
        // Hold references to the UI controls.
        private MapView _mapView;
        private TextView _helpLabel;
        private Button _navigateButton;

        private GraphicsOverlay _routeOverlay;
        private GraphicsOverlay _stopsOverlay;

        private MapPoint _startPoint;
        private MapPoint _endPoint;

        private RouteTask _routeTask;
        private Route _route;
        private RouteResult _routeResult;
        private RouteParameters _routeParameters;

        // Auth
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;
        private const string ServerUrl = "https://www.arcgis.com/sharing/rest";
        private const string AppClientId = @"lgAdHkYZYlwwfAhC";
        private const string OAuthRedirectUrl = @"my-ags-app://auth";

        private readonly string[] _requestedPermissions = { Manifest.Permission.AccessFineLocation };
        private const int requestCode = 35;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Navigate in AR";

            CreateLayout();

            RequestPermissions();
        }

        private void CreateLayout()
        {
            SetContentView(ArcGISRuntime.Resource.Layout.NavigateARRoutePlanner);

            _helpLabel = FindViewById<TextView>(ArcGISRuntime.Resource.Id.helpLabel);
            _mapView = FindViewById<MapView>(ArcGISRuntime.Resource.Id.mapView);
            _navigateButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.navigateButton);
        }

        private async void Initialize()
        {
            _mapView.Map = new Map(Basemap.CreateImagery());

            try
            {
                _mapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
                await _mapView.LocationDisplay.DataSource.StartAsync();
                _mapView.LocationDisplay.IsEnabled = true;

                SetOAuthInfo();

                _routeTask = await RouteTask.CreateAsync(new System.Uri("https://route.arcgis.com/arcgis/rest/services/World/Route/NAServer/Route_World"));

                _routeOverlay = new GraphicsOverlay();
                SimpleLineSymbol routeSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Yellow, 1);
                _routeOverlay.Renderer = new SimpleRenderer(routeSymbol);

                _stopsOverlay = new GraphicsOverlay();
                SimpleMarkerSymbol stopSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 5);
                _stopsOverlay.Renderer = new SimpleRenderer(stopSymbol);

                _mapView.GraphicsOverlays.Add(_routeOverlay);
                _mapView.GraphicsOverlays.Add(_stopsOverlay);

                _mapView.GeoViewTapped += _mapView_GeoViewTapped;

                _helpLabel.Text = "Tap to set a start point";
            }
            catch (System.Exception ex)
            {
                ShowMessage("Failed to start sample", "Error");
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void _mapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            if (_startPoint == null)
            {
                _startPoint = e.Location;

                Graphic startGraphic = new Graphic(_startPoint);
                _stopsOverlay.Graphics.Add(startGraphic);
            }
            else if (_endPoint == null)
            {
                _endPoint = e.Location;

                Graphic endGraphic = new Graphic(_endPoint);
                _stopsOverlay.Graphics.Add(endGraphic);

                SolveRoute();
            }
        }

        private void EnableNavigation()
        {
            RouteViewerAR.PassedRoute = _route;
            RouteViewerAR.PassedRouteParameters = _routeParameters;
            RouteViewerAR.PassedRouteResult = _routeResult;
            RouteViewerAR.PassedRouteTask = _routeTask;

            _navigateButton.Click += _navigateButton_Click;
            _navigateButton.Visibility = Android.Views.ViewStates.Visible;

            _helpLabel.Text = "You're ready to start navigating!";
        }

        private void _navigateButton_Click(object sender, System.EventArgs e)
        {
            Intent myIntent = new Intent(this, typeof(RouteViewerAR));
            StartActivity(myIntent);
        }

        private async void SolveRoute()
        {
            _helpLabel.Text = "Solving route";

            try {

                _routeParameters = await _routeTask.CreateDefaultParametersAsync();

                _routeParameters.ReturnStops = true;
                _routeParameters.ReturnDirections = true;
                _routeParameters.ReturnRoutes = true;

                TravelMode walkingMode = _routeTask.RouteTaskInfo.TravelModes.FirstOrDefault(mode => mode.Name.Contains("Walk")) ?? _routeTask.RouteTaskInfo.TravelModes.First();
                _routeParameters.TravelMode = walkingMode;

                Stop stop1 = new Stop(_startPoint);
                Stop stop2 = new Stop(_endPoint);

                _routeParameters.SetStops(new[] { stop1, stop2 });

                _routeResult = await _routeTask.SolveRouteAsync(_routeParameters);

                _route = _routeResult.Routes.First();

                Graphic routeGraphic = new Graphic(_route.RouteGeometry);
                _routeOverlay.Graphics.Add(routeGraphic);

                EnableNavigation();
            }
            catch (System.Exception ex)
            {
                ShowMessage("Failed to calculate route", "Error");
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
                ActivityCompat.RequestPermissions(this, _requestedPermissions, NavigateAR_RoutePlanner.requestCode);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == NavigateAR_RoutePlanner.requestCode && grantResults[0] == Permission.Granted)
            {
                Initialize();
            } else
            {
                Toast.MakeText(this, "Location permissions needed for this sample", ToastLength.Short).Show();
            }
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void ShowMessage(string message, string title)
        {
            new Android.Support.V7.App.AlertDialog.Builder(this).SetMessage(message).SetTitle(title).Show();
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
            catch (TaskCanceledException) { return credential; }
            catch (Exception)
            {
                // Exception will be reported in calling function.
                throw;
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
            Xamarin.Auth.OAuth2Authenticator authenticator = new OAuth2Authenticator(
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
            authenticator.Error += (sndr, errArgs) =>
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
