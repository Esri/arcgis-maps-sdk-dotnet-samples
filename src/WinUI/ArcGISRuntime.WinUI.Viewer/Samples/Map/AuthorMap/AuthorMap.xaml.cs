// Copyright 2021 Esri.
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
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArcGISRuntime.WinUI.Samples.AuthorMap
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

        // String array to store names of the available basemaps
        private readonly string[] _basemapNames = {
            "Light Gray",
            "Topographic",
            "Streets",
            "Imagery",
            "Ocean"
        };

        // Dictionary of operational layer names and URLs
        private readonly Dictionary<string, string> _operationalLayerUrls = new Dictionary<string, string>
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
            // Create the map
            MyMapView.Map = new Map();

            // Update the UI with basemaps and layers
            BasemapListBox.ItemsSource = _basemapNames;
            BasemapListBox.SelectedIndex = 0;
            OperationalLayerListBox.ItemsSource = _operationalLayerUrls;

            // Show the OAuth settings in the page
            ClientIdTextBox.Text = _appClientId;
            RedirectUrlTextBox.Text = _oAuthRedirectUrl;
            
            // Update the extent labels whenever the view point (extent) changes
            MyMapView.ViewpointChanged += (s, evt) => UpdateViewExtentLabels();
        }

        #region UI event handlers
        private void LayerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Call a function to add operational layers to the map
            AddOperationalLayers();
        }

        private async void SaveMapClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Don't attempt to save if the OAuth settings weren't provided
                if(String.IsNullOrEmpty(_appClientId) || String.IsNullOrEmpty(_oAuthRedirectUrl))
                {
                    MessageDialog dialog = new MessageDialog("OAuth settings were not provided.", "Cannot Save");
                    await dialog.ShowAsync();
                    return;
                }

                // Show the progress bar so the user knows work is happening
                SaveProgressBar.Visibility = Visibility.Visible;

                // Get the current map
                Map myMap = MyMapView.Map;

                // Apply the current extent as the map's initial extent
                myMap.InitialViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

                // Export the current map view to use as the item's thumbnail
                RuntimeImage thumbnailImg = await MyMapView.ExportImageAsync();

                // See if the map has already been saved (has an associated portal item)
                if (myMap.Item == null)
                {
                    // Get information for the new portal item
                    string title = TitleTextBox.Text;
                    string description = DescriptionTextBox.Text;
                    string tagText = TagsTextBox.Text;

                    // Make sure all required info was entered
                    if (String.IsNullOrEmpty(title) || String.IsNullOrEmpty(description) || String.IsNullOrEmpty(tagText))
                    {
                        throw new Exception("Please enter a title, description, and some tags to describe the map.");
                    }

                    // Call a function to save the map as a new portal item
                    await SaveNewMapAsync(MyMapView.Map, title, description, tagText.Split(','), thumbnailImg);

                    // Report a successful save
                    MessageDialog messageDialog = new MessageDialog("Saved '" + title + "' to ArcGIS Online!", "Map Saved");
                    await messageDialog.ShowAsync();
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
                    MessageDialog messageDialog = new MessageDialog("Saved changes to '" + myMap.Item.Title + "'", "Updates Saved");
                    await messageDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                // Report error message
                MessageDialog messageDialog = new MessageDialog("Error saving map to ArcGIS Online: " + ex.Message);
                await messageDialog.ShowAsync();
            }
            finally
            {
                // Hide the progress bar
                SaveProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void ClearMapClicked(object sender, RoutedEventArgs e)
        {
            // Create a new map (will not have an associated PortalItem)
            MyMapView.Map = new Map(Basemap.CreateLightGrayCanvas());

            // Reset the basemap selection in the UI
            BasemapListBox.SelectedIndex = 0;

            // Reset the layer selection in the UI;
            OperationalLayerListBox.SelectedIndex = -1;

            // Reset the extent labels
            UpdateViewExtentLabels();
        }

        #endregion UI event handlers

        private void ApplyBasemap(string basemapName)
        {
            // Get the current map
            Map myMap = MyMapView.Map;

            // Set the basemap for the map according to the user's choice in the list box
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
            // Clear the operational layers from the map
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

        private async Task SaveNewMapAsync(Map myMap, string title, string description, string[] tags, RuntimeImage img)
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
            await myMap.SaveAsAsync(agsOnline, null, title, description, tags, img);
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

        private void SaveOAuthSettingsClicked(object sender, RoutedEventArgs e)
        {
            // Settings were provided, update the configuration settings for OAuth authorization
            _appClientId = ClientIdTextBox.Text.Trim();
            _oAuthRedirectUrl = RedirectUrlTextBox.Text.Trim();

            // Update authentication manager with the OAuth settings
            UpdateAuthenticationManager();

            // Update the UI
            SaveMapGrid.Visibility = Visibility.Visible;
            OAuthSettingsGrid.Visibility = Visibility.Collapsed;
        }

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

            // Create a new ChallengeHandler that uses a method in this class to challenge for credentials
            thisAuthenticationManager.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
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
            catch (Exception)
            {
                // Exception will be reported in calling function
                throw;
            }

            return credential;
        }

        #endregion OAuth helpers

        private void BasemapListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the name of the desired basemap 
            string name = e.AddedItems[0].ToString();

            // Apply the basemap to the current map
            ApplyBasemap(name);
        }
    }
}