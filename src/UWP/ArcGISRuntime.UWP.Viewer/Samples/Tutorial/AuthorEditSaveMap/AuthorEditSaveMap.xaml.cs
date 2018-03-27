// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UI;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.UWP.Samples.AuthorEditSaveMap
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Author, edit, and save a map",
        "Tutorial",
        "This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). It is also the solution to the [Author, edit, and save maps to your portal tutorial](https://developers.arcgis.com/net/latest/uwp/guide/author-edit-and-save-maps-to-your-portal.htm). Saving a map to arcgis.com requires an ArcGIS Online login.",
        "1. Pan and zoom to the extent you would like for your map.\n2. Choose a basemap from the list of available basemaps.\n3. Click 'Save ...' and provide info for the new portal item (Title, Description, and Tags).\n4. Click 'Save Map to Portal'.\n5. After successfully logging in to your ArcGIS Online account, the map will be saved to your default folder.\n6. You can make additional changes, update the map, and then re-save to store changes in the portal item.")]
    [ArcGISRuntime.Samples.Shared.Attributes.ClassFile("MapViewModel.cs")]
    public partial class AuthorEditSaveMap
    {
        // Constants for OAuth-related values ...
        // URL of the server to authenticate with (ArcGIS Online)
        private const string ArcGISOnlineUrl = "https://www.arcgis.com/sharing/rest";

        // Client ID for the app registered with the server (Portal Maps)
        private const string AppClientId = "2Gh53JRzkPtOENQq";

        // Redirect URL after a successful authorization (configured for the Portal Maps application)
        private const string OAuthRedirectUrl = "https://developers.arcgis.com";

        // Gets the view-model that provides mapping capabilities to the view
        public MapViewModel ViewModel { get; } = new MapViewModel();

        public AuthorEditSaveMap()
        {
            InitializeComponent();

            // Pass the current map view to the map view model
            ViewModel.AppMapView = MyMapView;

            // Define a handler for selection changed on the basemap list
            BasemapListBox.SelectionChanged += OnBasemapsClicked;

            // Define a handler for the Save Map click
            SaveMapButton.Click += OnSaveMapClick;

            // Call a function to update the authentication manager settings
            UpdateAuthenticationManager();
        }

        private void OnBasemapsClicked(object sender, SelectionChangedEventArgs e)
        {
            // Get the text (basemap name) selected in the list box
            var basemapName = e.AddedItems[0].ToString();

            // Pass the basemap name to the view model method to change the basemap
            ViewModel.ChangeBasemap(basemapName);

            // Hide the basemaps flyout
            flyguy.Hide();
        }

        private async void OnSaveMapClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a challenge request for portal credentials (OAuth credential request for arcgis.com)
                CredentialRequestInfo challengeRequest = new CredentialRequestInfo();

                // Use the OAuth implicit grant flow
                challengeRequest.GenerateTokenOptions = new GenerateTokenOptions
                {
                    TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
                };

                // Indicate the url (portal) to authenticate with (ArcGIS Online)
                challengeRequest.ServiceUri = new Uri("https://www.arcgis.com/sharing/rest");

                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                await AuthenticationManager.Current.GetCredentialAsync(challengeRequest, false);

                // Get information for the new portal item
                var title = TitleTextBox.Text;
                var description = DescriptionTextBox.Text;
                var tags = TagsTextBox.Text.Split(',');

                // Return if the text is null or empty
                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
                {
                    return;
                }

                // Get current map extent (viewpoint) for the map initial extent
                var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

                // Export the current map view to use as the item's thumbnail
                RuntimeImage thumbnailImg = await MyMapView.ExportImageAsync();

                // See if the map has already been saved
                if (!ViewModel.MapIsSaved)
                {
                    // Call the SaveNewMapAsync method on the view model, pass in the required info
                    await ViewModel.SaveNewMapAsync(currentViewpoint, title, description, tags, thumbnailImg);

                    // Report success
                    MessageDialog dialog = new MessageDialog("Map '" + title + "' was saved to the portal.", "Saved Map");
                    await dialog.ShowAsync();
                }
                else
                {
                    // Map has previously been saved as a portal item, update it (title and description will remain the same)
                    ViewModel.UpdateMapItem();

                    // Report success
                    MessageDialog dialog = new MessageDialog("Changes to '" + title + "' were updated to the portal.");
                    await dialog.ShowAsync();
                }
            }
            catch (OperationCanceledException)
            {
                // Report canceled login
                MessageDialog dialog = new MessageDialog("Login to the portal was canceled.", "Save canceled");
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                // Report error
                MessageDialog dialog = new MessageDialog("Error while saving: " + ex.Message, "Cannot save");
                await dialog.ShowAsync();
            }
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

            // Create a new ChallengeHandler that uses a method in this class to challenge for credentials
            thisAuthenticationManager.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
        }

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
}
