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
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.UWP.Samples.AuthorEditSaveMap
{

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
            this.InitializeComponent();

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
                    dialog.ShowAsync();
                }
                else
                {
                    // Map has previously been saved as a portal item, update it (title and description will remain the same)
                    ViewModel.UpdateMapItem();

                    // Report success
                    MessageDialog dialog = new MessageDialog("Changes to '" + title + "' were updated to the portal.");
                    dialog.ShowAsync();
                }
            }
            catch (OperationCanceledException)
            {
                // Report canceled login
                MessageDialog dialog = new MessageDialog("Login to the portal was canceled.", "Save canceled");
                dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                // Report error
                MessageDialog dialog = new MessageDialog("Error while saving: " + ex.Message, "Cannot save");
                dialog.ShowAsync();
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
    
    // Provides map data to an application
    // Note: in a ArcGIS Runtime for .NET template project, this class will be in a separate file: "MapViewModel.cs"
    public class MapViewModel : INotifyPropertyChanged
    {        
        // Store the map view used by the app
        private MapView _mapView;
        public MapView AppMapView
        {
            set { _mapView = value; }
        }

        // String array to store basemap constructor types
        private string[] _basemapTypes = new string[]
        {
            "Topographic",
            "Topographic Vector",
            "Streets",
            "Streets Vector",
            "Imagery",
            "Oceans"
        };

        // Read-only property to return the available basemap names
        public string[] BasemapChoices
        {
            get { return _basemapTypes; }
        }

        public MapViewModel()
        {
            // Default constructor
        }

        // Create a default map with the vector streets basemap
        private Map _map = new Map(Basemap.CreateStreetsVector());
        
        // Gets or sets the map        
        public Map Map
        {
            get { return _map; }
            set { _map = value; OnPropertyChanged(); }
        }

        public void ChangeBasemap(string basemap)
        {
            // Apply the selected basemap to the map
            switch (basemap)
            {
                case "Topographic":
                    // Set the basemap to Topographic
                    _map.Basemap = Basemap.CreateTopographic();
                    break;
                case "Topographic Vector":
                    // Set the basemap to Topographic (vector)
                    _map.Basemap = Basemap.CreateTopographicVector();
                    break;
                case "Streets":
                    // Set the basemap to Streets
                    _map.Basemap = Basemap.CreateStreets();
                    break;
                case "Streets Vector":
                    // Set the basemap to Streets (vector)
                    _map.Basemap = Basemap.CreateStreetsVector();
                    break;
                case "Imagery":
                    // Set the basemap to Imagery
                    _map.Basemap = Basemap.CreateImagery();
                    break;
                case "Oceans":
                    // Set the basemap to Oceans
                    _map.Basemap = Basemap.CreateOceans();
                    break;
            }
        }

        // Save the current map to ArcGIS Online. The initial extent, title, description, and tags are passed in.
        public async Task SaveNewMapAsync(Viewpoint initialViewpoint, string title, string description, string[] tags, RuntimeImage img)
        {
            // Get the ArcGIS Online portal 
            ArcGISPortal agsOnline = await ArcGISPortal.CreateAsync(new Uri("https://www.arcgis.com/sharing/rest"));

            // Set the map's initial viewpoint using the extent (viewpoint) passed in
            _map.InitialViewpoint = initialViewpoint;

            // Save the current state of the map as a portal item in the user's default folder
            await _map.SaveAsAsync(agsOnline, null, title, description, tags, img, false);
        }

        public bool MapIsSaved
        {
            // Return True if the current map has a value for the Item property
            get { return (_map != null && _map.Item != null); }
        }

        public async void UpdateMapItem()
        {
            // Save the map
            await _map.SaveAsync();
            
            // Export the current map view for the item thumbnail
            RuntimeImage thumbnailImg = await _mapView.ExportImageAsync();

            // Get the file stream from the new thumbnail image
            Stream imageStream = await thumbnailImg.GetEncodedBufferAsync();

            // Update the item thumbnail
            (_map.Item as PortalItem).SetThumbnailWithImage(imageStream);
            await _map.SaveAsync();
        }

        // Raises the PropertyChanged event for a property
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var propertyChangedHandler = PropertyChanged;
            if (propertyChangedHandler != null)
                propertyChangedHandler(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
