// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Esri.ArcGISRuntime;

namespace ArcGISRuntime.UWP.Samples.SearchPortalMaps
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Search for webmap",
        "Map",
        "Find webmap portal items by using a search term.",
        "Enter search terms into the search bar. Once the search is complete, a list is populated with the resultant webmaps. Tap on a webmap to set it to the map view. Scrolling to the bottom of the webmap recycler view will get more results.",
        "keyword", "query", "search", "webmap")]
    public partial class SearchPortalMaps
    {
        // Variables for OAuth with default values ...
        // URL of the server to authenticate with (ArcGIS Online)
        private const string ArcGISOnlineUrl = "https://www.arcgis.com/sharing/rest";

        // Client ID for the app registered with the server (Portal Maps)
        private string _appClientId = "2Gh53JRzkPtOENQq";

        // Redirect URL after a successful authorization (configured for the Portal Maps application)
        private string _oAuthRedirectUrl = "https://developers.arcgis.com";

        // Constructor for sample class
        public SearchPortalMaps()
        {
            InitializeComponent();

            // Show the OAuth settings in the page
            ClientIdTextBox.Text = _appClientId;
            RedirectUrlTextBox.Text = _oAuthRedirectUrl;

            // Display a default map
            DisplayDefaultMap();
        }

        private void DisplayDefaultMap()
        {
            // Create a new Map instance
            Map myMap = new Map(Basemap.CreateLightGrayCanvas());

            // Provide Map to the MapView
            MyMapView.Map = myMap;
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get web map portal items in the current user's folder or from a keyword search
                IEnumerable<PortalItem> mapItems;
                ArcGISPortal portal;

                // See if the user wants to search public web map items
                if (SearchPublicMaps.IsChecked == true)
                {
                    // Connect to the portal (anonymously)
                    portal = await ArcGISPortal.CreateAsync();

                    // Create a query expression that will get public items of type 'web map' with the keyword(s) in the items tags
                    string queryExpression = string.Format("tags:\"{0}\" access:public type: (\"web map\" NOT \"web mapping application\")", SearchText.Text);

                    // Create a query parameters object with the expression and a limit of 10 results
                    PortalQueryParameters queryParams = new PortalQueryParameters(queryExpression, 10);

                    // Search the portal using the query parameters and await the results
                    PortalQueryResultSet<PortalItem> findResult = await portal.FindItemsAsync(queryParams);

                    // Get the items from the query results
                    mapItems = findResult.Results;
                }
                else
                {
                    // Call a sub that will force the user to log in to ArcGIS Online (if they haven't already)
                    bool loggedIn = await EnsureLoggedInAsync();
                    if (!loggedIn)
                    {
                        return;
                    }

                    // Connect to the portal (will connect using the provided credentials)
                    portal = await ArcGISPortal.CreateAsync(new Uri(ArcGISOnlineUrl));

                    // Get the user's content (items in the root folder and a collection of sub-folders)
                    PortalUserContent myContent = await portal.User.GetContentAsync();

                    // Get the web map items in the root folder
                    mapItems = from item in myContent.Items where item.Type == PortalItemType.WebMap select item;

                    // Loop through all sub-folders and get web map items, add them to the mapItems collection
                    foreach (PortalFolder folder in myContent.Folders)
                    {
                        IEnumerable<PortalItem> folderItems = await portal.User.GetContentAsync(folder.FolderId);
                        mapItems = mapItems.Concat(from item in folderItems where item.Type == PortalItemType.WebMap select item);
                    }
                }

                // Show the web map portal items in the list box
                MapListBox.ItemsSource = mapItems;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.ToString(), "Error").ShowAsync();
            }
        }

        private void LoadMapButtonClick(object sender, RoutedEventArgs e)
        {
            // Get the selected web map item in the list box
            PortalItem selectedMap = MapListBox.SelectedItem as PortalItem;
            if (selectedMap == null)
            {
                return;
            }

            // Create a new map, pass the web map portal item to the constructor
            Map webMap = new Map(selectedMap);

            // Handle change in the load status (to report load errors)
            webMap.LoadStatusChanged += WebMapLoadStatusChanged;

            // Show the web map in the map view
            MyMapView.Map = webMap;
        }

        private async void WebMapLoadStatusChanged(object sender, LoadStatusEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                // Report errors if map failed to load
                if (e.Status == LoadStatus.FailedToLoad)
                {
                    Map map = (Map) sender;
                    Exception err = map.LoadError;
                    if (err != null)
                    {
                        await new MessageDialog(err.Message, "Map load error").ShowAsync();
                    }
                }
            });
        }

        private void RadioButtonUnchecked(object sender, RoutedEventArgs e)
        {
            // When the search/user radio buttons are unchecked, clear the list box
            MapListBox.ItemsSource = null;

            // Set the map to the default (if necessary)
            if (MyMapView.Map.Item != null)
            {
                DisplayDefaultMap();
            }
        }

        private async Task<bool> EnsureLoggedInAsync()
        {
            bool loggedIn = false;
            try
            {
                // Create a challenge request for portal credentials (OAuth credential request for arcgis.com)
                CredentialRequestInfo challengeRequest = new CredentialRequestInfo
                {
                    // Use the OAuth implicit grant flow
                    GenerateTokenOptions = new GenerateTokenOptions
                    {
                        TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
                    },

                    // Indicate the url (portal) to authenticate with (ArcGIS Online)
                    ServiceUri = new Uri(ArcGISOnlineUrl)
                };

                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                Credential cred = await AuthenticationManager.Current.GetCredentialAsync(challengeRequest, false);
                loggedIn = cred != null;
            }
            catch (OperationCanceledException)
            {
                // TODO: handle login canceled
            }
            catch (Exception)
            {
                // TODO: handle login failure
            }

            return loggedIn;
        }

        private void SaveOAuthSettingsClicked(object sender, RoutedEventArgs e)
        {
            // Settings were provided, update the configuration settings for OAuth authorization
            _appClientId = ClientIdTextBox.Text.Trim();
            _oAuthRedirectUrl = RedirectUrlTextBox.Text.Trim();

            // Update authentication manager with the OAuth settings
            UpdateAuthenticationManager();

            // Hide the OAuth input, show the search UI
            OAuthSettingsGrid.Visibility = Visibility.Collapsed;
            SearchUI.Visibility = Visibility.Visible;
        }

        private async void CancelOAuthSettingsClicked(object sender, RoutedEventArgs e)
        {
            // Warn that browsing user's ArcGIS Online maps won't be available without OAuth settings
            string warning = "Without OAuth settings, you will not be able to browse maps from your ArcGIS Online account.";
            await new MessageDialog(warning, "No OAuth settings").ShowAsync();

            // Disable browsing maps from your ArcGIS Online account
            BrowseMyMaps.IsEnabled = false;

            // Hide the OAuth input, show the search UI
            OAuthSettingsGrid.Visibility = Visibility.Collapsed;
            SearchUI.Visibility = Visibility.Visible;
        }

        private void UpdateAuthenticationManager()
        {
            // Define the server information for ArcGIS Online
            ServerInfo portalServerInfo = new ServerInfo
            {
                // ArcGIS Online URI
                ServerUri = new Uri(ArcGISOnlineUrl),

                // Type of token authentication to use
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
            };

            // Define the OAuth information
            OAuthClientInfo oAuthInfo = new OAuthClientInfo
            {
                ClientId = _appClientId,
                RedirectUri = new Uri(_oAuthRedirectUrl)
            };
            portalServerInfo.OAuthClientInfo = oAuthInfo;

            // Get a reference to the (singleton) AuthenticationManager for the app
            AuthenticationManager thisAuthenticationManager = AuthenticationManager.Current;

            // Register the ArcGIS Online server information with the AuthenticationManager
            thisAuthenticationManager.RegisterServer(portalServerInfo);

            // Create a new ChallengeHandler that uses a method in this class to challenge for credentials
            thisAuthenticationManager.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
        }

        // ChallengeHandler function that will be called whenever access to a secured resource is attempted
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            Credential credential = null;

            try
            {
                // User will be challenged for OAuth credentials
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);
            }
            catch (Exception)
            {
                // Exception will be reported in calling function
                throw;
            }

            return credential;
        }
    }
}