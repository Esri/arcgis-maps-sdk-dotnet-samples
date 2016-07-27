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
using System.Threading.Tasks;
using WinUI = Windows.UI;
using Windows.UI.Popups;

namespace ArcGISRuntime.Windows.Samples.AuthorMap
{
    public partial class AuthorMap
    {
        // The map object that will be saved as a portal item
        private Map _myMap;

        // Constants for OAuth-related values ...
        // URL of the server to authenticate with
        private const string ServerUrl = "https://www.arcgis.com/sharing/rest";

        // TODO: Add Client ID for an app registered with the server
        private const string AppClientId = "2Gh53JRzkPtOENQq"; 

        // TODO: Add URL for redirecting after a successful authorization
        //       Note - this must be a URL configured as a valid Redirect URI with your app
        private const string OAuthRedirectUrl = "http://myapps.portalmapapp"; 

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
            BasemapListView.ItemsSource = _basemapNames;
            LayerListView.ItemsSource = _operationalLayerUrls;

            // Show a plain gray map in the map view
            MyMapView.Map = new Map(Basemap.CreateLightGrayCanvas());

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
            foreach (var item in LayerListView.SelectedItems)
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

        private void UpdateMap(object sender, WinUI.Xaml.RoutedEventArgs e)
        {
            // Create a new (empty) map
            if (_myMap == null || _myMap.PortalItem == null)
            {
                _myMap = new Map();
            }

            // Call functions that apply the selected basemap and operational layers
            ApplyBasemap(_basemapName);
            AddOperationalLayers();

            // Use the current extent to set the initial viewpoint for the map
            _myMap.InitialViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

            // Show the new map in the map view
            MyMapView.Map = _myMap;
        }

        private string _basemapName = string.Empty;
        private void BasemapItemClick(object sender, WinUI.Xaml.RoutedEventArgs e)
        {
            // Store the name of the desired basemap when one is selected
            // (will be applied to the map view when "Update Map" is clicked)
            var radioBtn = sender as WinUI.Xaml.Controls.RadioButton;
            _basemapName = radioBtn.Content.ToString();
        }

        private async void SaveMap(object sender, WinUI.Xaml.RoutedEventArgs e)
        {
            // Make sure the map is not null
            if (_myMap == null)
            {
                var dialog = new MessageDialog("Please update the map before saving.", "Map is empty");
                dialog.ShowAsync();
                return;
            }

            // See if the map has already been saved (has an associated portal item)
            if (_myMap.PortalItem == null)
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
                   SaveProgressBar.Visibility = WinUI.Xaml.Visibility.Visible;
                   
                    // Save the current state of the map as a portal item in the user's default folder
                    await _myMap.SaveAsAsync(agsOnline, null, title, description, tags, null);
                    var dialog = new MessageDialog("Saved '" + title + "' to ArcGIS Online!", "Map Saved");
                    dialog.ShowAsync();
                }
                catch (Exception ex)
                {
                    var dialog = new MessageDialog("Unable to save map to ArcGIS Online: " + ex.Message);
                    dialog.ShowAsync();
                }
                finally
                {
                    // Hide the progress bar
                    SaveProgressBar.Visibility = WinUI.Xaml.Visibility.Collapsed;
                }
            }
            else
            {
                // This is an update to the existing portal item
                try
                {
                    // Show the progress bar so the user knows it's working
                    SaveProgressBar.Visibility = WinUI.Xaml.Visibility.Visible;

                    // This is not the initial save, call SaveAsync to save changes to the existing portal item
                    await _myMap.SaveAsync();
                    var dialog = new MessageDialog("Saved changes to '" + _myMap.PortalItem.Title + "'", "Updates Saved");
                    dialog.ShowAsync();
                }
                catch (Exception ex)
                {
                    var dialog = new MessageDialog("Unable to save map updates: " + ex.Message);
                    dialog.ShowAsync();
                }
                finally
                {
                    // Hide the progress bar
                    SaveProgressBar.Visibility = WinUI.Xaml.Visibility.Collapsed;
                }
            }
        }

        private void ClearMap(object sender, WinUI.Xaml.RoutedEventArgs e)
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
            ServerInfo portalServerInfo = new ServerInfo
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

            // Get a reference to the (singleton) AuthenticationManager for the app
            AuthenticationManager thisAuthenticationManager = AuthenticationManager.Current;

            // Register the server information
            thisAuthenticationManager.RegisterServer(portalServerInfo);
            
            // Create a new ChallengeHandler that uses a method in this class to challenge for credentials
            thisAuthenticationManager.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
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
                // Get a reference to the (singleton) AuthenticationManager for the app
                AuthenticationManager thisAuthenticationManager = AuthenticationManager.Current;

                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                var cred = await thisAuthenticationManager.GetCredentialAsync(loginInfo, false);
                authenticated = (cred != null);
            }
            catch (OperationCanceledException)
            {
                // user canceled the login
                var dialog = new MessageDialog("Maps cannot be saved unless logged in to ArcGIS Online.", "Save to Portal");
                dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog("Error logging in: " + ex.Message, "Save to Portal");
                dialog.ShowAsync();
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

                // IOAuthAuthorizeHandler will challenge the user for credentials
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
}
