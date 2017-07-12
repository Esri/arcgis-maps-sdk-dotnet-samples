// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Windows;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Tasks.Offline;
using ArcGISRuntime.Samples.Managers;
using System.IO;
using Esri.ArcGISRuntime.Tasks;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ArcGISRuntime.WPF.Samples.GenerateOfflineMap
{
    public partial class GenerateOfflineMap
    {
        // Constants for OAuth-related values ...
        // URL of the server to authenticate with (ArcGIS Online)
        private const string ArcGISOnlineUrl = "https://www.arcgis.com/sharing/rest";

        // Client ID for the app registered with the server (Portal Maps)
        private const string AppClientId = "2Gh53JRzkPtOENQq";

        // Redirect URL after a successful authorization (configured for the Portal Maps application)
        private const string OAuthRedirectUrl = "https://developers.arcgis.com";

        // Job that is used to generate the offline map
        private GenerateOfflineMapJob _job;

        public GenerateOfflineMap()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Set authentication handling
                UpdateAuthenticationManager();

                // Load webmap portal item and set it to the view
                ArcGISPortal portal = await ArcGISPortal.CreateAsync();

                // Create a map that is taken offline based on item id
                PortalItem webmapItem = await PortalItem.CreateAsync(portal, "acc027394bc84c2fb04d1ed317aac674");

                // Create map and add it to the view
                Map myMap = new Map(webmapItem);

                // Assign the map to the MapView
                MyMapView.Map = myMap;

                // Disable user interactions on the map
                MyMapView.InteractionOptions = new MapViewInteractionOptions();
                MyMapView.InteractionOptions.IsEnabled = false;

                loadingIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred. " + ex.ToString(), "Sample error");
            }
        }

        private async void OnTakeMapOfflineClicked(object sender, RoutedEventArgs e)
        {
            // Create a path where the data is stored. Using postfix "_mmpk" to indicate that this is a local map folder.
            var packagePath =
                Path.Combine(DataManager.GetDataFolder(), "TemporaryData", "NaperilleWaterNetwork_mmpk");

            try
            {
                // Set UI to indicate that we are doing work
                busyIndicator.Visibility = Visibility.Visible;

                // If the we already have a package with this name, replace with a new one
                if (Directory.Exists(packagePath))
                    Directory.Delete(packagePath, true);
                else
                    Directory.CreateDirectory(packagePath);

                // Convert area of interest frame to an geographical envelope 
                var areaOfInterest = AreaOfInterestAsEnvelope();

                // Create task and set parameters
                OfflineMapTask task = await OfflineMapTask.CreateAsync(MyMapView.Map);
                // Create default parameters
                GenerateOfflineMapParameters parameters =
                    await task.CreateDefaultGenerateOfflineMapParametersAsync(areaOfInterest);
                // Limit the max scale that are taken offline
                parameters.MaxScale = 1800;

                // Override offline package metadata
                var thumbnail = await MyMapView.ExportImageAsync(); // Create image from current MapView
                parameters.ItemInfo.Thumbnail = thumbnail; // Set current image to package thumbnail
                parameters.ItemInfo.Title = parameters.ItemInfo.Title + " Central"; // Override title

                // Create job and set output location and hook progress indication handling
                _job = task.GenerateOfflineMap(parameters, packagePath);
                _job.ProgressChanged += _job_ProgressChanged;

                // Generate all geodatabases, Export all Tile and Vector Tile Packages and create Mobile Map Package
                GenerateOfflineMapResult results = await _job.GetResultAsync();

                // If an job fails, something went wrong when creating the offline package such as writing to the 
                // folder was denied.
                if (_job.Status != JobStatus.Succeeded)
                {
                    MessageBox.Show("Creating offline map package failed.", "Sample error");
                    busyIndicator.Visibility = Visibility.Collapsed;
                }
                // If one or more layers fails, layer errors are populated with corresponding errors.
                if (results.HasErrors)
                {
                    var errorBuilder = new StringBuilder();
                    foreach (var layerError in results.LayerErrors)
                    {
                        errorBuilder.AppendLine(string.Format("{0} : {1}", layerError.Key.Id, layerError.Value.Message));
                    }
                    foreach (var tableError in results.TableErrors)
                    {
                        errorBuilder.AppendLine(string.Format("{0} : {1}", tableError.Key.TableName, tableError.Value.Message));
                    }
                    var errorText = errorBuilder.ToString();
                    MessageBox.Show(errorText, "Errors on taking layers offline");
                }

                // Show new offline map
                MyMapView.Map = results.OfflineMap;

                // Keep MapView in a same position where it was when taking a map offline
                MyMapView.SetViewpoint(new Viewpoint(areaOfInterest));

                // Enable map interaction 
                MyMapView.InteractionOptions.IsEnabled = true;

                areaOfInterestFrame.Visibility = Visibility.Collapsed;
                busyIndicator.Visibility = Visibility.Collapsed;
                takeOfflineArea.Visibility = Visibility.Collapsed;
                offlineArea.Visibility = Visibility.Visible;
            }
            catch (TaskCanceledException)
            {
                busyIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Taking map offline failed.");
                busyIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void _job_ProgressChanged(object sender, EventArgs e)
        {
            var job = sender as GenerateOfflineMapJob;
            // The event might be raised in the background thread so make sure that we do UI changes
            // in the UI thread.
            Dispatcher.Invoke(() =>
            {
                Percentage.Text = job.Progress > 0 ? $"{job.Progress.ToString()} %" : string.Empty;
                progressBar.Value = job.Progress;
            });
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            // Cancel the task and release the UI.
            _job.Cancel();
        }

        private Envelope AreaOfInterestAsEnvelope()
        {
            Point topLeftPoint = areaOfInterestFrame.TransformToVisual(MyMapView)
                              .Transform(new Point(0, 0));
            Point bottomRightPoint = areaOfInterestFrame.TransformToVisual(MyMapView)
                  .Transform(new Point(areaOfInterestFrame.ActualWidth, areaOfInterestFrame.ActualHeight));

            var areaOfInterest = new Envelope(
                MyMapView.ScreenToLocation(topLeftPoint),
                MyMapView.ScreenToLocation(bottomRightPoint));

            return areaOfInterest;
        }

        private void UpdateAuthenticationManager()
        {
            // Define the server information for ArcGIS Online
            ServerInfo portalServerInfo = new ServerInfo();

            // ArcGIS Online URI
            portalServerInfo.ServerUri = new Uri(ArcGISOnlineUrl);

            // Type of token authentication to use
            portalServerInfo.TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit;

            // Define the OAuth information
            OAuthClientInfo oAuthInfo = new OAuthClientInfo
            {
                ClientId = AppClientId,
                RedirectUri = new Uri(OAuthRedirectUrl)
            };
            portalServerInfo.OAuthClientInfo = oAuthInfo;

            // Get a reference to the (singleton) AuthenticationManager for the app
            AuthenticationManager thisAuthenticationManager = AuthenticationManager.Current;

            // Register the ArcGIS Online server information with the AuthenticationManager
            thisAuthenticationManager.RegisterServer(portalServerInfo);

            // Use the OAuthAuthorize class in this project to create a new web view to show the login UI
            thisAuthenticationManager.OAuthAuthorizeHandler = new OAuthAuthorize();

            // Create a new ChallengeHandler that uses a method in this class to challenge for credentials
            thisAuthenticationManager.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
        }

        // ChallengeHandler function that will be called whenever access to a secured resource is attempted
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            Credential credential = null;

            try
            {
                // IOAuthAuthorizeHandler will challenge the user for OAuth credentials
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);
            }
            catch (Exception ex)
            {
                // Exception will be reported in calling function
                throw (ex);
            }

            return credential;
        }
    }

    #region OAuth handler
    public class OAuthAuthorize : IOAuthAuthorizeHandler
    {
        // Window to contain the OAuth UI
        private Window _window;

        // Use a TaskCompletionSource to track the completion of the authorization
        private TaskCompletionSource<IDictionary<string, string>> _tcs;

        // URL for the authorization callback result (the redirect URI configured for your application)
        private string _callbackUrl;

        // URL that handles the OAuth request
        private string _authorizeUrl;

        // Function to handle authorization requests, takes the URIs for the secured service, the authorization endpoint, and the redirect URI
        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            // If the TaskCompletionSource or Window are not null, authorization is in progress
            if (_tcs != null || _window != null)
            {
                // Allow only one authorization process at a time
                throw new Exception();
            }

            // Store the authorization and redirect URLs
            _authorizeUrl = authorizeUri.AbsoluteUri;
            _callbackUrl = callbackUri.AbsoluteUri;

            // Create a task completion source
            _tcs = new TaskCompletionSource<IDictionary<string, string>>();

            // Call a function to show the login controls, make sure it runs on the UI thread for this app
            var dispatcher = Application.Current.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
                AuthorizeOnUIThread(_authorizeUrl);
            else
            {
                var authorizeOnUIAction = new Action((() => AuthorizeOnUIThread(_authorizeUrl)));
                dispatcher.BeginInvoke(authorizeOnUIAction);
            }

            // Return the task associated with the TaskCompletionSource
            return _tcs.Task;
        }

        // Challenge for OAuth credentials on the UI thread
        private void AuthorizeOnUIThread(string authorizeUri)
        {
            // Create a WebBrowser control to display the authorize page
            var webBrowser = new WebBrowser();

            // Handle the navigation event for the browser to check for a response to the redirect URL
            webBrowser.Navigating += WebBrowserOnNavigating;

            // Display the web browser in a new window 
            _window = new Window
            {
                Content = webBrowser,
                Height = 430,
                Width = 395,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            // Set the app's window as the owner of the browser window (if main window closes, so will the browser)
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                _window.Owner = Application.Current.MainWindow;
            }

            // Handle the window closed event then navigate to the authorize url
            _window.Closed += OnWindowClosed;
            webBrowser.Navigate(authorizeUri);

            // Display the Window
            _window.ShowDialog();
        }

        // Handle the browser window closing
        void OnWindowClosed(object sender, EventArgs e)
        {
            // If the browser window closes, return the focus to the main window
            if (_window != null && _window.Owner != null)
            {
                _window.Owner.Focus();
            }

            // If the task wasn't completed, the user must have closed the window without logging in
            if (_tcs != null && !_tcs.Task.IsCompleted)
            {
                // Set the task completion source exception to indicate a canceled operation
                _tcs.SetException(new OperationCanceledException());
            }

            // Set the task completion source and window to null to indicate the authorization process is complete
            _tcs = null;
            _window = null;
        }

        // Handle browser navigation (content changing)
        void WebBrowserOnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            // Check for a response to the callback url
            const string portalApprovalMarker = "/oauth2/approval";
            var webBrowser = sender as WebBrowser;
            Uri uri = e.Uri;

            // If no browser, uri, task completion source, or an empty url, return
            if (webBrowser == null || uri == null || _tcs == null || string.IsNullOrEmpty(uri.AbsoluteUri))
                return;

            // Check for redirect
            bool isRedirected = uri.AbsoluteUri.StartsWith(_callbackUrl) ||
                _callbackUrl.Contains(portalApprovalMarker) && uri.AbsoluteUri.Contains(portalApprovalMarker);

            if (isRedirected)
            {
                // Browser was redirected to the callbackUrl (success!)
                //    -close the window 
                //    -decode the parameters (returned as fragments or query)
                //    -return these parameters as result of the Task
                e.Cancel = true;
                var tcs = _tcs;
                _tcs = null;

                if (_window != null)
                {
                    _window.Close();
                }

                // Call a helper function to decode the response parameters
                var authResponse = DecodeParameters(uri);

                // Set the result for the task completion source
                tcs.SetResult(authResponse);
            }
        }

        private static IDictionary<string, string> DecodeParameters(Uri uri)
        {
            // Create a dictionary of key value pairs returned in an OAuth authorization response URI query string
            var answer = string.Empty;

            // Get the values from the URI fragment or query string
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

            // Parse parameters into key / value pairs
            var keyValueDictionary = new Dictionary<string, string>();
            var keysAndValues = answer.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var kvString in keysAndValues)
            {
                var pair = kvString.Split('=');
                string key = pair[0];
                string value = string.Empty;
                if (key.Length > 1)
                {
                    value = Uri.UnescapeDataString(pair[1]);
                }

                keyValueDictionary.Add(key, value);
            }

            // Return the dictionary of string keys/values
            return keyValueDictionary;
        }
    }
    #endregion
}