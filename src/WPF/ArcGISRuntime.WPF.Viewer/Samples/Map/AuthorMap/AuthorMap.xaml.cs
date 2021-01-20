// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Shared.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace ArcGISRuntime.WPF.Samples.AuthorMap
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Create and save map",
        category: "Map",
        description: "Create and save a map as an ArcGIS `PortalItem` (i.e. web map).",
        instructions: "1. Select the basemap and layers you'd like to add to your map.",
        tags: new[] { "ArcGIS Online", "OAuth", "portal", "publish", "share", "web map" })]
    public partial class AuthorMap
    {
        // Constants for OAuth-related values ...
        // URL of the server to authenticate with
        private string ServerUrl = "https://www.arcgis.com/sharing/rest";

        // TODO: Add Client ID for an app registered with the server
        private string _appClientId = "lgAdHkYZYlwwfAhC";

        // TODO: Add URL for redirecting after a successful authorization
        //       Note - this must be a URL configured as a valid Redirect URI with your app
        private string _oAuthRedirectUrl = "my-ags-app://auth";

        private string _apiKeyHold;

        // String array to store names of the available basemaps
        private readonly string[] _basemapNames = 
        {
            "Light Gray",
            "Topographic",
            "Streets",
            "Imagery",
            "Ocean"
        };

        // Dictionary of operational layer names and URLs
        private Dictionary<string, string> _operationalLayerUrls = new Dictionary<string, string>
        {
            {"World Elevations", "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Elevation/WorldElevations/MapServer"},
            {"World Cities", "https://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer/" },
            {"US Census Data", "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer"}
        };

        public AuthorMap()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            _apiKeyHold = Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey;

            ApiKeyManager.ArcGISDeveloperApiKey = null;

            this.Unloaded += Sample_Unloaded;

            // Show a plain gray map in the map view
            MyMapView.Map = new Map(Basemap.CreateLightGrayCanvas());

            // Fill the basemap combo box with basemap names
            BasemapListBox.ItemsSource = _basemapNames;

            // Select the first basemap in the list by default
            BasemapListBox.SelectedIndex = 0;

            // Fill the operational layers list box with layer names
            OperationalLayerListBox.ItemsSource = _operationalLayerUrls;

            // Show the OAuth settings in the page
            ClientIdTextBox.Text = _appClientId;
            RedirectUrlTextBox.Text = _oAuthRedirectUrl;

            // Update the extent labels whenever the view point (extent) changes
            MyMapView.ViewpointChanged += (s, evt) => UpdateViewExtentLabels();
        }

        private void Sample_Unloaded(object sender, RoutedEventArgs e)
        {
            ApiKeyManager.ArcGISDeveloperApiKey = _apiKeyHold;
        }

        #region UI event handlers
        private void BasemapSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Call a function to set the basemap to the one selected
            ApplyBasemap(e.AddedItems[0].ToString());
        }

        private void OperationalLayerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Call a function to add the chosen layers to the map
            AddOperationalLayers();
        }

        private void ClearMapClicked(object sender, RoutedEventArgs e)
        {
            // Create a new map (will not have an associated PortalItem)
            MyMapView.Map = new Map(BasemapStyle.ArcGISLightGray);

            // Reset UI to be consistent with map
            BasemapListBox.SelectedIndex = 0;
            OperationalLayerListBox.SelectedIndex = -1;
        }

        private void SaveOAuthSettingsClicked(object sender, RoutedEventArgs e)
        {
            // Store the OAuth information that was entered
            _appClientId = ClientIdTextBox.Text.Trim();
            _oAuthRedirectUrl = RedirectUrlTextBox.Text.Trim();

            // Hide the OAuth settings, show the save map controls
            OAuthSettingsGrid.Visibility = Visibility.Collapsed;
            SaveMapGrid.Visibility = Visibility.Visible;

            // Update authentication manager with the OAuth settings
            UpdateAuthenticationManager();
        }

        private async void SaveMapClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show the progress bar so the user knows work is happening
                SaveProgressBar.Visibility = Visibility.Visible;

                // Get the current map
                Map myMap = MyMapView.Map;

                // Apply the current extent as the map's initial extent
                myMap.InitialViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

                // Get the current map view for the item thumbnail
                RuntimeImage thumbnailImg = await MyMapView.ExportImageAsync();

                // See if the map has already been saved (has an associated portal item)
                if (myMap.Item == null)
                {
                    // Get information for the new portal item
                    string title = TitleTextBox.Text;
                    string description = DescriptionTextBox.Text;
                    string[] tags = TagsTextBox.Text.Split(',');

                    // Make sure all required info was entered
                    if (String.IsNullOrEmpty(title) || String.IsNullOrEmpty(description) || tags.Length == 0)
                    {
                        throw new Exception("Please enter a title, description, and some tags to describe the map.");
                    }

                    // Call a function to save the map as a new portal item
                    await SaveNewMapAsync(MyMapView.Map, title, description, tags, thumbnailImg);

                    // Report a successful save
                    MessageBox.Show("Saved '" + title + "' to ArcGIS Online!", "Map Saved");
                }
                else
                {
                    // This is not the initial save, call SaveAsync to save changes to the existing portal item
                    await myMap.SaveAsync();

                    // Get the file stream from the new thumbnail image
                    Stream imageStream = await thumbnailImg.GetEncodedBufferAsync();

                    // Update the item thumbnail
                    ((PortalItem)myMap.Item).SetThumbnail(imageStream);
                    await myMap.SaveAsync();

                    // Report update was successful
                    MessageBox.Show("Saved changes to '" + myMap.Item.Title + "'", "Updates Saved");
                }
            }
            catch (Exception ex)
            {
                // Report error message
                MessageBox.Show("Error saving map to ArcGIS Online: " + ex.Message);
            }
            finally
            {
                // Hide the progress bar
                SaveProgressBar.Visibility = Visibility.Hidden;
            }            
        }
        #endregion

        private void ApplyBasemap(string basemapName)
        {
            // Set the basemap for the map according to the user's choice in the list box
            Map myMap = MyMapView.Map;
            switch (basemapName)
            {
                case "Light Gray":
                    // Set the basemap to Light Gray Canvas
                    myMap.Basemap = Basemap.CreateLightGrayCanvas();
                    break;
                case "Topographic":
                    // Set the basemap to Topographic
                    myMap.Basemap = Basemap.CreateTopographic();
                    break;
                case "Streets":
                    // Set the basemap to Streets
                    myMap.Basemap = Basemap.CreateStreets();
                    break;
                case "Imagery":
                    // Set the basemap to Imagery
                    myMap.Basemap = Basemap.CreateImagery();
                    break;
                case "Ocean":
                    // Set the basemap to Oceans
                    myMap.Basemap = Basemap.CreateOceans();
                    break;
            }
        }

        private void AddOperationalLayers()
        {
            // Clear all operational layers from the map
            Map myMap = MyMapView.Map;
            myMap.OperationalLayers.Clear();

            // Loop through the selected items in the operational layers list box
            foreach (KeyValuePair<string, string> item in OperationalLayerListBox.SelectedItems)
            {
                // Get the service uri for each selected item 
                KeyValuePair<string, string> layerInfo = item;
                Uri layerUri = new Uri(layerInfo.Value);

                // Create a new map image layer, set it 50% opaque, and add it to the map
                ArcGISMapImageLayer layer = new ArcGISMapImageLayer(layerUri)
                {
                    Opacity = 0.5
                };
                myMap.OperationalLayers.Add(layer);
            }
        }

        private async Task SaveNewMapAsync(Map myMap, string title, string description, string[] tags, RuntimeImage thumb)
        {
            // Challenge the user for portal credentials (OAuth credential request for arcgis.com)
            CredentialRequestInfo loginInfo = new CredentialRequestInfo
            {

                // Use the OAuth implicit grant flow
                GenerateTokenOptions = new GenerateTokenOptions
                {
                    TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
                },

                // Indicate the url (portal) to authenticate with (ArcGIS Online)
                ServiceUri = new Uri("https://www.arcgis.com/sharing/rest")
            };

            try
            {
                // Get a reference to the (singleton) AuthenticationManager for the app
                AuthenticationManager thisAuthenticationManager = AuthenticationManager.Current;

                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                await thisAuthenticationManager.GetCredentialAsync(loginInfo, false);
            }
            catch (OperationCanceledException)
            {
                // user canceled the login
                throw new Exception("Portal log in was canceled.");
            }

            // Get the ArcGIS Online portal (will use credential from login above)
            ArcGISPortal agsOnline = await ArcGISPortal.CreateAsync();
            
            // Save the current state of the map as a portal item in the user's default folder
            await myMap.SaveAsAsync(agsOnline, null, title, description, tags, thumb);                      
        }

        private void UpdateViewExtentLabels()
        {
            // Get the current view point for the map view
            Viewpoint currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
            if (currentViewpoint == null) { return; }

            // Get the current map extent (envelope) from the view point
            Envelope currentExtent = currentViewpoint.TargetGeometry as Envelope;

            // Project the current extent to geographic coordinates (longitude / latitude)
            Envelope currentGeoExtent = (Envelope)GeometryEngine.Project(currentExtent, SpatialReferences.Wgs84);

            // Fill the app text boxes with min / max longitude (x) and latitude (y) to four decimal places
            XMinTextBox.Text = currentGeoExtent.XMin.ToString("0.####");
            YMinTextBox.Text = currentGeoExtent.YMin.ToString("0.####");
            XMaxTextBox.Text = currentGeoExtent.XMax.ToString("0.####");
            YMaxTextBox.Text = currentGeoExtent.YMax.ToString("0.####");
        }

        #region OAuth helpers
        private void UpdateAuthenticationManager()
        {
            // Register the server information with the AuthenticationManager
            ServerInfo portalServerInfo = new ServerInfo
            {
                ServerUri = new Uri(ServerUrl),
                OAuthClientInfo = new OAuthClientInfo
                {
                    ClientId = _appClientId,
                    RedirectUri = new Uri(_oAuthRedirectUrl)
                },
                // Specify OAuthAuthorizationCode if you need a refresh token (and have specified a valid client secret)
                // Otherwise, use OAuthImplicit
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
            };

            // Get a reference to the (singleton) AuthenticationManager for the app
            AuthenticationManager thisAuthenticationManager = AuthenticationManager.Current;

            // Register the server information
            thisAuthenticationManager.RegisterServer(portalServerInfo);

            // Use the OAuthAuthorize class in this project to create a new web view that contains the OAuth challenge handler.
            thisAuthenticationManager.OAuthAuthorizeHandler = new OAuthAuthorize();

            // Create a new ChallengeHandler that uses a method in this class to challenge for credentials
            thisAuthenticationManager.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
        }
        
        // ChallengeHandler function for AuthenticationManager that will be called whenever access to a secured
        // resource is attempted
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            Credential credential = null;

            try
            {
                // IOAuthAuthorizeHandler will challenge the user for OAuth credentials
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);
            }
            catch (Exception)
            {
                // Exception will be reported in calling function
                throw;
            }

            return credential;
        }
        #endregion
    }

    #region Helper class to display the OAuth authorization challenge
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
            Dispatcher dispatcher = Application.Current.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
                AuthorizeOnUIThread(_authorizeUrl);
            else
            {
                Action authorizeOnUIAction = () => AuthorizeOnUIThread(_authorizeUrl);
                dispatcher.BeginInvoke(authorizeOnUIAction);
            }

            // Return the task associated with the TaskCompletionSource
            return _tcs != null ? _tcs.Task : null;
        }

        // Challenge for OAuth credentials on the UI thread
        private void AuthorizeOnUIThread(string authorizeUri)
        {
            // Create a WebBrowser control to display the authorize page
            WebBrowser webBrowser = new WebBrowser();

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
        private void OnWindowClosed(object sender, EventArgs e)
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
        private void WebBrowserOnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            // Check for a response to the callback url
            const string portalApprovalMarker = "/oauth2/approval";
            WebBrowser webBrowser = sender as WebBrowser;
            Uri uri = e.Uri;

            // If no browser, uri, task completion source, or an empty url, return
            if (webBrowser == null || uri == null || _tcs == null || String.IsNullOrEmpty(uri.AbsoluteUri))
                return;

            // Check for redirect
            bool isRedirected = uri.AbsoluteUri.StartsWith(_callbackUrl) ||
                _callbackUrl.Contains(portalApprovalMarker) && uri.AbsoluteUri.Contains(portalApprovalMarker);

            if (isRedirected)
            {
                // If the web browser is redirected to the callbackUrl:
                //    -close the window 
                //    -decode the parameters (returned as fragments or query)
                //    -return these parameters as result of the Task
                e.Cancel = true;
                TaskCompletionSource<IDictionary<string, string>> tcs = _tcs;
                _tcs = null;
                if (_window != null)
                {
                    _window.Close();
                }

                // Call a helper function to decode the response parameters
                IDictionary<string,string> authResponse = DecodeParameters(uri);

                // Set the result for the task completion source
                tcs.SetResult(authResponse);
            }
        }

        private static IDictionary<string, string> DecodeParameters(Uri uri)
        {
            // Create a dictionary of key value pairs returned in an OAuth authorization response URI query string
            string answer = "";

            // Get the values from the URI fragment or query string
            if (!String.IsNullOrEmpty(uri.Fragment))
            {
                answer = uri.Fragment.Substring(1);
            }
            else
            {
                if (!String.IsNullOrEmpty(uri.Query))
                {
                    answer = uri.Query.Substring(1);
                }
            }

            // Parse parameters into key / value pairs
            Dictionary<string,string> keyValueDictionary = new Dictionary<string, string>();
            string[] keysAndValues = answer.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string kvString in keysAndValues)
            {
                string[] pair = kvString.Split('=');
                string key = pair[0];
                string value = "";
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
