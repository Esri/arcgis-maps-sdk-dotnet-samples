// Copyright 2016 Esri.
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
using Esri.ArcGISRuntime.Xamarin.Forms;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.AuthorEditSaveMap
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Author, edit, and save a map",
        "Tutorial",
        "This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). It is also the solution to the [Author, edit, and save maps to your portal tutorial](https://developers.arcgis.com/net/latest/forms/guide/author-edit-and-save-maps-to-your-portal.htm). Saving a map to arcgis.com requires an ArcGIS Online login.",
        "1. Pan and zoom to the extent you would like for your map.\n2. Choose a basemap from the list of available basemaps.\n3. Click 'Save ...' and provide info for the new portal item (Title, Description, and Tags).\n4. Click 'Save Map to Portal'.\n5. After successfully logging in to your ArcGIS Online account, the map will be saved to your default folder.\n6. You can make additional changes, update the map, and then re-save to store changes in the portal item.")]
    [ArcGISRuntime.Samples.Shared.Attributes.ClassFile("SaveMapPage.xaml.cs")]
    [ArcGISRuntime.Samples.Shared.Attributes.XamlFiles("SaveMapPage.xaml")]
    public partial class AuthorEditSaveMap : ContentPage
	{
        private MapViewModel _mapViewModel;

        // Constants for OAuth-related values ...
        // URL of the server to authenticate with (ArcGIS Online)
        private const string ArcGISOnlineUrl = "https://www.arcgis.com/sharing/rest";

        // Client ID for the app registered with the server (Portal Maps)
        public const string AppClientId = "2Gh53JRzkPtOENQq";

        // Redirect URL after a successful authorization (configured for the Portal Maps application)
        private const string OAuthRedirectUrl = "https://developers.arcgis.com";

        public AuthorEditSaveMap ()
		{
            InitializeComponent();

            // Get the view model (defined as a resource in the XAML)
            _mapViewModel = Resources["MapViewModel"] as MapViewModel;

            // Pass the map view to the map view model
            _mapViewModel.AppMapView = MyMapView;

            // Define a click handler for the Basemaps button (show the basemap choice list)
            BasemapsButton.Clicked += (s, e) => BasemapListBox.IsVisible = true;

            // Define a selection handler on the basemap list
            BasemapListBox.ItemTapped += OnBasemapsClicked;

            // Define a click handler for the Save button (show a form for entering portal item info)
            SaveMapButton.Clicked += ShowSaveMapDialog;

            // Define a click handler for the New button
            NewMapButton.Clicked += (s, e) => {
                _mapViewModel.ResetMap();
                BasemapListBox.SelectedItem = _mapViewModel.BasemapChoices[0];
            };

            // Set up the AuthenticationManager to challenge for ArcGIS Online credentials
            UpdateAuthenticationManager();

            // Change the style of the basemap list view for Android and UWP
            Device.OnPlatform(
                Android: () =>
                {
                    // Black background on Android (transparent by default)
                    BasemapListBox.BackgroundColor = Color.Black;
                },
                WinPhone: () =>
                {
                    // Semi-transparent background on Windows with a small margin around the control
                    BasemapListBox.BackgroundColor = Color.FromRgba(255, 255, 255, 0.3);
                    BasemapListBox.Margin = new Thickness(50);
                });

            Title = "Author, edit, and save maps to your portal";
        }

        private void OnBasemapsClicked(object sender, ItemTappedEventArgs e)
        {
            // Get the text (basemap name) selected in the list box
            var basemapName = e.Item.ToString();

            // Pass the basemap name to the view model method to change the basemap
            _mapViewModel.ChangeBasemap(basemapName);
            // Hide the basemap list
            BasemapListBox.IsVisible = false;
        }

        private async void ShowSaveMapDialog(object sender, EventArgs e)
        {
            // Create a SaveMapPage page for getting user input for the new web map item
            var mapInputForm = new ArcGISRuntime.Samples.AuthorEditSaveMap.SaveMapPage();

            // Handle the save button click event on the page
            mapInputForm.OnSaveClicked += SaveMapAsync;

            // Navigate to the SaveMapPage UI
            await Navigation.PushAsync(mapInputForm);

            // Define an OAuth challenge for ArcGIS Online
            var info = new CredentialRequestInfo
            {
                AuthenticationType = AuthenticationType.Token,
                ServiceUri = new Uri(ArcGISOnlineUrl)
            };

            // Get the credential
            try
            {
                Credential cred = await AuthenticationManager.Current.GetCredentialAsync(info, false);

                // Add the credential to the AuthenticationManager
                AuthenticationManager.Current.AddCredential(cred);
            } catch (System.Threading.Tasks.TaskCanceledException)
            {
                // Handle situation where the user closes the login window
                return;
            } catch (System.OperationCanceledException)
            {
                // Handle situation where the user presses 'cancel' in the login UI
                return;
            } catch (Exception)
            {
                // Handle all other exceptions related to canceled login
                return;
            }
        }

        // Event handler to get information entered by the user and save the map
        private async void SaveMapAsync(object sender, ArcGISRuntime.Samples.AuthorEditSaveMap.SaveMapEventArgs e)
        {
            try
            {
                // Get information entered by the user for the new portal item properties
                var title = e.MapTitle;
                var description = e.MapDescription;
                var tags = e.Tags;

                // Get the current extent displayed in the map view
                var currentViewpoint = MyMapView.GetCurrentViewpoint(Esri.ArcGISRuntime.Mapping.ViewpointType.BoundingGeometry);

                // Export the current map view for the item's thumbnail
                RuntimeImage thumbnailImg = await MyMapView.ExportImageAsync();

                // See if the map has already been saved 
                if (!_mapViewModel.MapIsSaved)
                {
                    // Save the map as a portal item
                    await _mapViewModel.SaveNewMapAsync(currentViewpoint, title, description, tags, thumbnailImg);

                    // Report a successful save
                    await DisplayAlert("Map Saved", "Saved '" + title + "' to ArcGIS Online!", "OK");
                }
                else
                {
                    // Map has previously been saved as a portal item, update it (title, description, and tags will remain the same)
                    _mapViewModel.UpdateMapItem();

                    // Report success
                    await DisplayAlert("Map Updated", "Saved changes to '" + title + "'", "OK");
                }
            }
            catch (Exception ex)
            {
                // Show the exception message
                await DisplayAlert("Unable to save map", ex.Message, "OK");
            }
        }

        #region OAuth
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

        // ChallengeHandler function that will be called whenever access to a secured resource is attempted
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            Credential credential = null;

            try
            {
                // IOAuthAuthorizeHandler will challenge the user for OAuth credentials
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);
            }
            catch (TaskCanceledException) { return credential; }
            catch (Exception)
            {
                // Exception will be reported in calling function
                throw;
            }

            return credential;
        }


        #endregion
    }

    // Note: In an ArcGIS Runtime SDK for .NET template project, this class would be in a separate file (MapViewModel.cs)
    // ViewModel that binds to the View class (AuthorEditSaveMap) to provide the Map and other functionality
    public class MapViewModel : INotifyPropertyChanged
    {
        public MapViewModel()
        {
            
        }

        // Store the map view used by the app
        private MapView _mapView;
        public MapView AppMapView
        {
            set { _mapView = value; }
        }

        private Map _map = new Map(Basemap.CreateTopographic());
        
        // Gets or sets the map
        public Map Map
        {
            get { return _map; }
            set { _map = value; OnPropertyChanged(); }
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

            // Export the current map view for the item's thumbnail
            RuntimeImage thumbnailImg = await _mapView.ExportImageAsync();

            // Get the file stream from the new thumbnail image
            Stream imageStream = await thumbnailImg.GetEncodedBufferAsync();

            // Update the item thumbnail
            (_map.Item as PortalItem).SetThumbnailWithImage(imageStream);
            await _map.SaveAsync();
        }

        public void ResetMap()
        {
            // Set the current map to null
            _map = null;

            // Create a new map with topographic basemap
            Map newMap = new Map(Basemap.CreateTopographic());

            // Store the new map 
            Map = newMap;
        }

        // Raises the PropertyChanged event        
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
