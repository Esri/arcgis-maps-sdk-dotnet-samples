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
        private const string ClientId = "b4tmQpgU92eu3XAR";
        // TODO: Add Client Secret for the same app (only needed for the OAuthAuthorizationCode auth type)
        private const string ClientSecret = "";
        // TODO: Add URL registered with the server for redirecting after a successful authorization
        //       Note - this must be an existing URL registered with your app
        private const string RedirectUrl = "https://developers.arcgis.com/";

        // String array to store names of the available basemaps
        private string[] _basemapNames = new string[]
        {
            "Topographic",
            "Streets",
            "Imagery",
            "Ocean"
        };

        // Operational layer URLs
        private Dictionary<string, string> _operationalLayerUrls = new Dictionary<string, string>
        {
            {"World Elevations", "http://sampleserver5.arcgisonline.com/arcgis/rest/services/Elevation/WorldElevations/MapServer"},
            {"Census Data", "http://sampleserver5.arcgisonline.com/arcgis/rest/services/Census/MapServer"}
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
        }

        /// <summary>
        /// Register the server and OAuthAuthorize class with the Authentication Manager.
        /// </summary>
        private void UpdateAuthenticationManager()
        {
            // Register the server information with the AuthenticationManager
            Esri.ArcGISRuntime.Security.ServerInfo serverInfo = new ServerInfo
            {
                ServerUri = new Uri(ServerUrl),
                OAuthClientInfo = new OAuthClientInfo
                {
                    ClientId = ClientId,
                    //ClientSecret = ClientSecret,
                    RedirectUri = new Uri(RedirectUrl)
                },
                // Specify OAuthAuthorizationCode if you need a refresh token (and have specified a valid client secret)
                // Otherwise, use OAuthImplicit
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
            };
            AuthenticationManager.Current.RegisterServer(serverInfo);

            // Use the OAuthAuthorize class in this project to create a new web view that contains the OAuth challenge handler.
            AuthenticationManager.Current.OAuthAuthorizeHandler = new OAuthAuthorize();

            // Create a new ChallengeHandler that uses a method in this class to challenge for credentials
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
        }

        private void ApplyBasemap(string basemapName)
        {
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
            foreach(var item in OperationalLayerListBox.SelectedItems)
            {               
                var layerInfo = (KeyValuePair<string, string>)item;
                var layerUri = new Uri(layerInfo.Value);
                ArcGISMapImageLayer layer = new ArcGISMapImageLayer(layerUri);
                layer.Opacity = 0.5;
                _myMap.OperationalLayers.Add(layer);
            }
        }
        
        private void UpdateMap(object sender, System.Windows.RoutedEventArgs e)
        {
            // Create a new (empty) map
            _myMap = new Map();


            // Apply the selected basemap and operational layers
            ApplyBasemap(BasemapListBox.SelectedValue.ToString());
            AddOperationalLayers();

            // Set the initial viewpoint to the one currently displayed
            _myMap.InitialViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

            // Show the new map
            MyMapView.Map = _myMap;
        }

        private async void SaveMap(object sender, System.Windows.RoutedEventArgs e)
        {
            if(_myMap.PortalItem == null)
            {
                CredentialRequestInfo loginInfo = new CredentialRequestInfo();
                loginInfo.GenerateTokenOptions = new GenerateTokenOptions { TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit };
                loginInfo.ServiceUri = new Uri("http://www.arcgis.com/sharing/rest");
                await AuthenticationManager.Current.GetCredentialAsync(loginInfo, false);

                ArcGISPortal agsOnline = await ArcGISPortal.CreateAsync();

                var tags = new List<string> { "test", "map", "runtime" };
                try
                {
                    await _myMap.SaveAsAsync(agsOnline, null, "Test map", "this is a test ...", tags, null);
                }
                catch(Exception ex)
                {
                    
                }
            }
            else
            {
                await _myMap.SaveAsync();
            }
        }

        /// <summary>
        /// ChallengeHandler function for AuthenticationManager that will be called whenever access to a secured
        /// resource is attempted
        /// </summary>
        /// <param name="info">Contains information about the secure request, including the URL of the resource</param>
        /// <returns></returns>
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
        
        private void OnNavigationCompleted(object sender, EventArgs e)
        {
            Envelope currentExtent = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;
            Envelope currentGeoExtent = GeometryEngine.Project(currentExtent, SpatialReferences.Wgs84) as Envelope;
            XMinTextBox.Text = currentGeoExtent.XMin.ToString("0.####");
            YMinTextBox.Text = currentGeoExtent.YMin.ToString("0.####");
            XMaxTextBox.Text = currentGeoExtent.XMax.ToString("0.####");
            YMaxTextBox.Text = currentGeoExtent.YMax.ToString("0.####");
        }
    }

    /// <summary>
    /// Helper class to display the OAuth authorization challenge using a web browser in a defined window
    /// </summary>
    public class OAuthAuthorize : IOAuthAuthorizeHandler
    {
        private Window _window;
        private TaskCompletionSource<IDictionary<string, string>> _tcs;
        private string _callbackUrl;
        private string _authorizeUrl;

        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            if (_tcs != null || _window != null)
                throw new Exception(); // only one authorization process at a time

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

            _window.Closed += OnWindowClosed;
            webBrowser.Navigate(authorizeUri);

            // Display the Window
            _window.ShowDialog();
        }

        void OnWindowClosed(object sender, EventArgs e)
        {
            if (_window != null && _window.Owner != null)
                _window.Owner.Focus();
            if (_tcs != null && !_tcs.Task.IsCompleted)
                _tcs.SetException(new OperationCanceledException()); // user closed the window
            _tcs = null;
            _window = null;
        }

        // Check if the web browser is redirected to the callback url
        void WebBrowserOnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            const string portalApprovalMarker = "/oauth2/approval";
            var webBrowser = sender as WebBrowser;
            Uri uri = e.Uri;
            if (webBrowser == null || uri == null || _tcs == null || string.IsNullOrEmpty(uri.AbsoluteUri))
                return;

            bool isRedirected = uri.AbsoluteUri.StartsWith(_callbackUrl) ||
                _callbackUrl.Contains(portalApprovalMarker) && uri.AbsoluteUri.Contains(portalApprovalMarker); // Portal OAuth workflow with org defined at runtime --> the redirect uri can change
            if (isRedirected)
            {
                // The web browser is redirected to the callbackUrl ==> close the window, decode the parameters returned as fragments or query, and return these parameters as result of the Task
                e.Cancel = true;
                var tcs = _tcs;
                _tcs = null;
                if (_window != null)
                {
                    _window.Close();
                }
                tcs.SetResult(DecodeParameters(uri));
            }
        }

        /// <summary>
        /// Decodes the parameters returned when the user agent is redirected to the callback url
        /// The parameters can be returned as fragments (e.g. access_token for Browser based app) or as query parameter (e.g. code for Server based app)
        /// </summary>
        /// <param name="uri">The URI.</param>
        private static IDictionary<string, string> DecodeParameters(Uri uri)
        {
            string answer = !string.IsNullOrEmpty(uri.Fragment)
                                ? uri.Fragment.Substring(1)
                                : (!string.IsNullOrEmpty(uri.Query) ? uri.Query.Substring(1) : string.Empty);

            // decode parameters from format key1=value1&key2=value2&...
            return answer.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Split('=')).ToDictionary(pair => pair[0], pair => pair.Length > 1 ? Uri.UnescapeDataString(pair[1]) : null);
        }
    }
}
