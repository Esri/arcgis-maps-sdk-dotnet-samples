// Copyright 2016 Esri.
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ArcGISRuntime.Desktop.Samples.AuthorMap
{
    public partial class AuthorMap
    {
        private Map _myMap;

        // Constants for OAuth-related values ...
        // URL of the server to authenticate with
        private const string ServerUrl = "https://www.arcgis.com/sharing/rest";
        // TODO: Add Client ID for an app registered with the server
        private const string AppClientId = "b4tmQpgU92eu3XAR";
        // TODO: Add URL registered with the server for redirecting after a successful authorization
        //       Note - this must be an existing URL registered with your app
        private const string OAuthRedirectUrl = "https://developers.arcgis.com/";

        // String array to store names of the available basemaps
        private string[] _basemapNames = new string[]
        {
            "Topographic",
            "Streets",
            "Imagery",
            "Ocean"
        };

        // Dictionary of operational layer names and URLs
        private Dictionary<string, string> _operationalLayerUrls = new Dictionary<string, string>
        {
            {"World Elevations", "http://sampleserver5.arcgisonline.com/arcgis/rest/services/Elevation/WorldElevations/MapServer"},
            {"World Cities", "http://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer/" },
            {"US Census Data", "http://sampleserver5.arcgisonline.com/arcgis/rest/services/Census/MapServer"}
        };

        public AuthorMap()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Show a plain gray map in the map view
            MyMapView.Map = new Map(Basemap.CreateLightGrayCanvas());

            // Fill the basemap combo box with basemap names
            BasemapListBox.ItemsSource = _basemapNames;
            // Select the first basemap in the list by default
            BasemapListBox.SelectedIndex = 0;

            // Fill the operational layers list box with layer names
            OperationalLayerListBox.ItemsSource = _operationalLayerUrls;

            // Setup the AuthenticationManager to challenge for credentials
            UpdateAuthenticationManager();
            
            // Update the extent labels whenever the view point (extent) changes
            MyMapView.ViewpointChanged += (s, evt) => UpdateViewExtentLabels();
        }            

        private void ApplyBasemap(string basemapName)
        {
            // Set the basemap for the map according to the user's choice in the list box
            switch (basemapName)
            {
                case "Topographic":
                    // Set the basemap to Topographic
                    _myMap.Basemap = Basemap.CreateTopographic();
                    break;
                case "Streets":
                    // Set the basemap to Streets
                    _myMap.Basemap = Basemap.CreateStreets();
                    break;
                case "Imagery":
                    // Set the basemap to Imagery
                    _myMap.Basemap = Basemap.CreateImagery();
                    break;
                case "Ocean":
                    // Set the basemap to Oceans
                    _myMap.Basemap = Basemap.CreateOceans();
                    break;
                default:
                    break;
            }
        }

        private void AddOperationalLayers()
        {
            // Loop through the selected items in the operational layers list box
            foreach(var item in OperationalLayerListBox.SelectedItems)
            {               
                // Get the service uri for each selected item 
                var layerInfo = (KeyValuePair<string, string>)item;
                var layerUri = new Uri(layerInfo.Value);
                // Create a new map image layer, set it 50% opaque, and add it to the map
                ArcGISMapImageLayer layer = new ArcGISMapImageLayer(layerUri);
                layer.Opacity = 0.5;
                _myMap.OperationalLayers.Add(layer);
            }
        }
        
        private void UpdateMap(object sender, System.Windows.RoutedEventArgs e)
        {
            // Create a new (empty) map
            if (_myMap == null || _myMap.PortalItem == null)
            {
                _myMap = new Map();
            }

            // Call functions that apply the selected basemap and operational layers
            ApplyBasemap(BasemapListBox.SelectedValue.ToString());
            AddOperationalLayers();

            // Use the current extent to set the initial viewpoint for the map
            _myMap.InitialViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

            // Show the new map in the map view
            MyMapView.Map = _myMap;
        }

        private async void SaveMap(object sender, System.Windows.RoutedEventArgs e)
        {
            // Make sure the map is not null
            if(_myMap == null)
            {
                MessageBox.Show("Please update the map before saving.", "Map is empty");
                return;
            }

            // See if the map has already been saved (has an associated portal item)
            if(_myMap.PortalItem == null)
            {
                // This is the initial save for this map

                // Call a function that will challenge the user for ArcGIS Online credentials
                var isLoggedIn = await EnsureLoginToArcGISAsync();
                // If the user could not log in (or canceled the login), exit
                if (!isLoggedIn) { return; }

                // Get the ArcGIS Online portal
                ArcGISPortal agsOnline = await ArcGISPortal.CreateAsync();

                // Get information for the new portal item
                var title = TitleTextBox.Text;
                var description = DescriptionTextBox.Text;
                var tags = TagsTextBox.Text.Split(',');

                try
                {
                    // Show the progress bar so the user knows it's working
                    SaveProgressBar.Visibility = Visibility.Visible;

                    // Save the current state of the map as a portal item in the user's default folder
                    await _myMap.SaveAsAsync(agsOnline, null, title, description, tags, null);
                    MessageBox.Show("Saved '" + title + "' to ArcGIS Online!", "Map Saved");
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Unable to save map to ArcGIS Online: " + ex.Message);                    
                }
                finally
                {
                    // Hide the progress bar
                    SaveProgressBar.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                try
                {
                    // Show the progress bar so the user knows it's working
                    SaveProgressBar.Visibility = Visibility.Visible;

                    // This is not the initial save, call SaveAsync to save changes to the existing portal item
                    await _myMap.SaveAsync();
                    MessageBox.Show("Saved changes to '" + _myMap.PortalItem.Title + "'", "Updates Saved");
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Unable to save map updates: " + ex.Message);
                }
                finally
                {
                    // Hide the progress bar
                    SaveProgressBar.Visibility = Visibility.Hidden;
                }
            }
        }

        private void ClearMap(object sender, RoutedEventArgs e)
        {
            // Set the map to null
            _myMap = null;

            // Show a plain gray map in the map view
            MyMapView.Map = new Map(Basemap.CreateLightGrayCanvas());
        }
        
        private void UpdateViewExtentLabels()
        {
            // Get the current view point for the map view
            Viewpoint currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
            if (currentViewpoint == null) { return; }

            // Get the current map extent (envelope) from the view point
            Envelope currentExtent = currentViewpoint.TargetGeometry as Envelope;
            // Project the current extent to geographic coordinates (longitude / latitude)
            Envelope currentGeoExtent = GeometryEngine.Project(currentExtent, SpatialReferences.Wgs84) as Envelope;

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
            Esri.ArcGISRuntime.Security.ServerInfo portalServerInfo = new ServerInfo
            {
                ServerUri = new Uri(ServerUrl),
                OAuthClientInfo = new OAuthClientInfo
                {
                    ClientId = AppClientId,
                    RedirectUri = new Uri(OAuthRedirectUrl)
                },
                // Specify OAuthAuthorizationCode if you need a refresh token (and have specified a valid client secret)
                // Otherwise, use OAuthImplicit
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
            };
            AuthenticationManager.Current.RegisterServer(portalServerInfo);

            // Use the OAuthAuthorize class in this project to create a new web view that contains the OAuth challenge handler.
            AuthenticationManager.Current.OAuthAuthorizeHandler = new OAuthAuthorize();

            // Create a new ChallengeHandler that uses a method in this class to challenge for credentials
            AuthenticationManager.Current.ChallengeHandler = new Esri.ArcGISRuntime.Security.ChallengeHandler(CreateCredentialAsync);
        }

        private async Task<bool> EnsureLoginToArcGISAsync()
        {
            var authenticated = false;

            // Create an OAuth credential request for arcgis.com
            CredentialRequestInfo loginInfo = new CredentialRequestInfo();

            // Use the OAuth implicit grant flow
            loginInfo.GenerateTokenOptions = new GenerateTokenOptions
            {
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
            };

            // Indicate the url (portal) to authenticate with (ArcGIS Online)
            loginInfo.ServiceUri = new Uri("http://www.arcgis.com/sharing/rest");

            try
            {
                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                await AuthenticationManager.Current.GetCredentialAsync(loginInfo, false);
                authenticated = true;
            }
            catch(OperationCanceledException)
            {
                // user canceled the login
                MessageBox.Show("Maps cannot be saved unless logged in to ArcGIS Online.");
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error logging in: " + ex.Message);
            }

            return authenticated;
        }

        // ChallengeHandler function for AuthenticationManager that will be called whenever access to a secured
        // resource is attempted
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            OAuthTokenCredential credential = null;

            try
            {
                // Create generate token options if necessary
                if (info.GenerateTokenOptions == null)
                {
                    info.GenerateTokenOptions = new GenerateTokenOptions();
                }

                // AuthenticationManager will handle challenging the user for credentials
                credential = await AuthenticationManager.Current.GenerateCredentialAsync
                    (
                            info.ServiceUri,
                            info.GenerateTokenOptions
                    ) as OAuthTokenCredential;
            }
            catch (Exception ex)
            {
                // Exception will be reported in calling function
                throw (ex);
            }

            return credential;
        }
        #endregion
    }

    #region Helper class to display the OAuth authorization challenge
    public class OAuthAuthorize : IOAuthAuthorizeHandler
    {
        private Window _window;
        private TaskCompletionSource<IDictionary<string, string>> _tcs;
        private string _callbackUrl;
        private string _authorizeUrl;

        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            if (_tcs != null || _window != null)
            {
                // Allow only one authorization process at a time
                throw new Exception(); 
            }

            _authorizeUrl = authorizeUri.AbsoluteUri;
            _callbackUrl = callbackUri.AbsoluteUri;
            _tcs = new TaskCompletionSource<IDictionary<string, string>>();
            var tcs = _tcs;

            var dispatcher = Application.Current.Dispatcher;

            if (dispatcher == null || dispatcher.CheckAccess())
                AuthorizeOnUIThread(_authorizeUrl);
            else
            {
                dispatcher.BeginInvoke((Action)(() => AuthorizeOnUIThread(_authorizeUrl)));
            }

            return tcs.Task;
        }

        // Challenge for OAuth credentials on the UI thread
        private void AuthorizeOnUIThread(string authorizeUri)
        {
            // Set an embedded WebBrowser that displays the authorize page
            var webBrowser = new WebBrowser();
            webBrowser.Navigating += WebBrowserOnNavigating;

            // Display the web browser in a window (default behavior, may be customized by an application)
            _window = new Window
            {
                Content = webBrowser,
                Height = 480,
                Width = 480,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current != null && Application.Current.MainWindow != null
                            ? Application.Current.MainWindow
                            : null
            };

            // Handle the window closed event and navigate to the authorize url
            _window.Closed += OnWindowClosed;
            webBrowser.Navigate(authorizeUri);

            // Display the Window
            _window.ShowDialog();
        }

        void OnWindowClosed(object sender, EventArgs e)
        {
            if (_window != null && _window.Owner != null)
            {
                _window.Owner.Focus();
            }

            if (_tcs != null && !_tcs.Task.IsCompleted)
            {
                // The user closed the window
                _tcs.SetException(new OperationCanceledException());
            }

            _tcs = null;
            _window = null;
        }

        // Check if the web browser is redirected to the callback url
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
                // If the web browser is redirected to the callbackUrl:
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

                // Call a function to decode the response parameters
                tcs.SetResult(DecodeParameters(uri));
            }
        }

        // Decodes the parameters returned when the user agent is redirected to the callback url
        private static IDictionary<string, string> DecodeParameters(Uri uri)
        {
            string answer = !string.IsNullOrEmpty(uri.Fragment)
                                ? uri.Fragment.Substring(1)
                                : (!string.IsNullOrEmpty(uri.Query) ? uri.Query.Substring(1) : string.Empty);

            // decode parameters from format key1=value1&key2=value2&...
            return answer.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Split('=')).ToDictionary(pair => pair[0], pair => pair.Length > 1 ? Uri.UnescapeDataString(pair[1]) : null);
        }
    }
    #endregion
}
