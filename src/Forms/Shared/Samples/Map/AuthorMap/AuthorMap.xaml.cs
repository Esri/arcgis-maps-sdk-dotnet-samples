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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Xamarin.Forms;
using System.IO;


#if __IOS__
using Xamarin.Forms.Platform.iOS;
using Xamarin.Auth;
using UIKit;
#endif

#if __ANDROID__
using Android.App;
using Xamarin.Auth;
using System.IO;
#endif

namespace ArcGISRuntime.Samples.AuthorMap
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Author and save a map",
        "Map",
        "This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). Saving a map to arcgis.com requires an ArcGIS Online login.",
        "1. Pan and zoom to the extent you would like for your map. \n2. Choose a basemap from the list of available basemaps. \n3. Choose one or more operational layers to include. \n4. Click 'Save ...' to apply your changes. \n5. Provide info for the new portal item, such as a Title, Description, and Tags. \n6. Click 'Save Map'. \n7. After successfully logging in to your ArcGIS Online account, the map will be saved to your default folder. \n8. You can make additional changes, update the map, and then re-save to store changes in the portal item.")]
    [ArcGISRuntime.Samples.Shared.Attributes.ClassFile("SaveMapPage.xaml.cs")]
    [ArcGISRuntime.Samples.Shared.Attributes.XamlFiles("SaveMapPage.xaml")]
    public partial class AuthorMap : ContentPage, IOAuthAuthorizeHandler
    {
        // OAuth-related values ...
        // URL of the server to authenticate with (ArcGIS Online)
        private const string ArcGISOnlineUrl = "https://www.arcgis.com/sharing/rest";

        // Client ID for the app registered with the server (Portal Maps)
        public string _appClientId = "2Gh53JRzkPtOENQq";

        // Redirect URL after a successful authorization (configured for the Portal Maps application)
        private string _oAuthRedirectUrl = "https://developers.arcgis.com";

        // String array to store basemap constructor types
        private string[] _basemapTypes = new string[]
        {
            "Topographic",
            "Streets",
            "Imagery",
            "Oceans"
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

            Title = "Author and save a map";

            // call a function to initialize the app (display a map, etc.)
            Initialize();
        }

        private void Initialize()
        {
            // Call a function to create a new map with a light gray canvas basemap
            CreateNewMap();

            // Show the default OAuth settings in the entry controls
            ClientIDEntry.Text = _appClientId;
            RedirectUrlEntry.Text = _oAuthRedirectUrl;

            // Change the style of the layer list view for Android and UWP
            Device.OnPlatform(
                Android: () =>
                {
                    // Black background on Android (transparent by default)
                    LayersList.BackgroundColor = Color.Black;
                    OAuthSettingsGrid.BackgroundColor = Color.Black;
                },
                WinPhone: () =>
                {
                    // Semi-transparent background on Windows with a small margin around the control
                    LayersList.BackgroundColor = Color.FromRgba(255, 255, 255, 0.3);
                    LayersList.Margin = new Thickness(50);
                });
        }

        private void OAuthSettingsCancel(object sender, EventArgs e)
        {
            OAuthSettingsGrid.IsVisible = false;
        }

        private void SaveOAuthSettings(object sender, EventArgs e)
        {
            _appClientId = ClientIDEntry.Text.Trim();
            _oAuthRedirectUrl = RedirectUrlEntry.Text.Trim();

            OAuthSettingsGrid.IsVisible = false;

            // Call a function to set up the AuthenticationManager
            UpdateAuthenticationManager();
        }

        private void LayerSelected(object sender, ItemTappedEventArgs e)
        {
            // return if null
            if (e.Item == null) { return; }

            // Handle the event when a layer item is selected (tapped) in the layer list
            var selectedItem = e.Item.ToString();

            // See if this is one of the layers in the operational layers list 
            if (_operationalLayerUrls.ContainsKey(selectedItem))
            {
                // Get the service URL from the operational layers dictionary
                var value = _operationalLayerUrls[selectedItem];

                // Call a function to add the chosen operational layer
                AddLayer(selectedItem, value);
            }
            else
            {
                // Add the chosen basemap (replace the current one)
                AddBasemap(selectedItem);
            }

            // Hide the layer list
            LayersList.IsVisible = false;
        }

        private void ShowLayerList(object sender, EventArgs e)
        {
            // See which button was used to show the list and fill it accordingly
            var button = sender as Button;
            if (button.Text == "Basemap")
            {
                // Show the basemap list
                LayersList.ItemsSource = _basemapTypes.ToList();
            }
            else if (button.Text == "Layers")
            {
                // Show the operational layers list (names)
                LayersList.ItemsSource = _operationalLayerUrls.Keys;
            }

            // Show the layer list view control
            LayersList.IsVisible = true;
        }

        private async void ShowSaveMapUI(object sender, EventArgs e)
        {
            // Create a SaveMapPage page for getting user input for the new web map item
            var mapInputForm = new SaveMapPage();

            // If an existing map, show the UI for updating the item
            var mapItem = MyMapView.Map.Item;
            if (mapItem != null)
            {
                mapInputForm.ShowForUpdate(mapItem.Title,mapItem.Description, mapItem.Tags.ToArray());
            }

            // Handle the save button click event on the page
            mapInputForm.OnSaveClicked += SaveMapAsync;

            // Navigate to the SaveMapPage UI
            // Note: in each platform's project, there is a custom PageRenderer class called SaveMapPage that provides
            //       platform-specific logic to challenge the user for OAuth credentials for ArcGIS Online when the page launches
            await Navigation.PushAsync(mapInputForm);
        }

        // Event handler to get information entered by the user and save the map
        private async void SaveMapAsync(object sender, SaveMapEventArgs e)
        {
            // Get the current map
            var myMap = MyMapView.Map;

            try
            {
                // Show the progress bar so the user knows work is happening
                SaveMapProgressBar.IsVisible = true;

                // Make sure the user is logged in to ArcGIS Online
                var cred = await EnsureLoggedInAsync();
                AuthenticationManager.Current.AddCredential(cred);

                // Get information entered by the user for the new portal item properties
                var title = e.MapTitle;
                var description = e.MapDescription;
                var tags = e.Tags;

                // Apply the current extent as the map's initial extent
                myMap.InitialViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

                // Export the current map view for the item's thumbnail
                RuntimeImage thumbnailImage = await MyMapView.ExportImageAsync();

                // See if the map has already been saved (has an associated portal item)
                if (myMap.Item == null)
                {
                    // Get the ArcGIS Online portal (will use credential from login above)
                    ArcGISPortal agsOnline = await ArcGISPortal.CreateAsync(new Uri(ArcGISOnlineUrl));

                    // Save the current state of the map as a portal item in the user's default folder
                    await myMap.SaveAsAsync(agsOnline, null, title, description, tags, thumbnailImage);

                    // Report a successful save
                    await DisplayAlert("Map Saved", "Saved '" + title + "' to ArcGIS Online!", "OK");
                }
                else
                {
                    // This is not the initial save, call SaveAsync to save changes to the existing portal item
                    await myMap.SaveAsync();

                    // Get the file stream from the new thumbnail image
                    Stream imageStream = await thumbnailImage.GetEncodedBufferAsync();

                    // Update the item thumbnail
                    (myMap.Item as PortalItem).SetThumbnailWithImage(imageStream);
                    await myMap.SaveAsync();

                    // Report update was successful
                    await DisplayAlert("Updates Saved", "Saved changes to '" + myMap.Item.Title + "'", "OK");
                }
            }
            catch (Exception ex)
            {
                // Show the exception message
                await DisplayAlert("Unable to save map", ex.Message, "OK");
            }
            finally
            {
                // Hide the progress bar
                SaveMapProgressBar.IsVisible = false;
            }
        }

        private async Task<Credential> EnsureLoggedInAsync()
        {
            // Challenge the user for portal credentials (OAuth credential request for arcgis.com)
            Credential cred = null;
            CredentialRequestInfo loginInfo = new CredentialRequestInfo();

            // Use the OAuth implicit grant flow
            loginInfo.GenerateTokenOptions = new GenerateTokenOptions
            {
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
            };

            // Indicate the url (portal) to authenticate with (ArcGIS Online)
            loginInfo.ServiceUri = new Uri(ArcGISOnlineUrl);

            try
            {
                // Get the users credentials for ArcGIS Online (should have logged in when launching the page)
                cred = await AuthenticationManager.Current.GetCredentialAsync(loginInfo, false);
            }
            catch (System.OperationCanceledException)
            {
                // user canceled the login
                throw new Exception("Portal log in was canceled.");
            }

            return cred;
        }

        private void NewMapButtonClick(object sender, EventArgs e)
        {
            // Call a function to create a new map
            CreateNewMap();
        }

        private void CreateNewMap()
        {
            // Create new Map with a light gray canvas basemap
            var myMap = new Map(Basemap.CreateLightGrayCanvas());

            // Add the Map to the MapView
            MyMapView.Map = myMap;
        }

        private void AddBasemap(string basemapName)
        {
            // Apply the chosen basemap
            switch (basemapName)
            {
                case "Topographic":
                    // Set the basemap to Topographic
                    MyMapView.Map.Basemap = Basemap.CreateTopographic();
                    break;
                case "Streets":
                    // Set the basemap to Streets
                    MyMapView.Map.Basemap = Basemap.CreateStreets();
                    break;
                case "Imagery":
                    // Set the basemap to Imagery
                    MyMapView.Map.Basemap = Basemap.CreateImagery();
                    break;
                case "Oceans":
                    // Set the basemap to Oceans
                    MyMapView.Map.Basemap = Basemap.CreateOceans();
                    break;
            }
        }

        private void AddLayer(string layerName, string url)
        {
            // Clear any existing layers
            MyMapView.Map.OperationalLayers.Clear();

            // See if the layer already exists
            ArcGISMapImageLayer layer = MyMapView.Map.OperationalLayers.FirstOrDefault(l => l.Name == layerName) as ArcGISMapImageLayer;

            var layerUri = new Uri(url);

            // Create a new map image layer
            layer = new ArcGISMapImageLayer(layerUri);
            layer.Name = layerName;

            // Set it 50% opaque, and add it to the map
            layer.Opacity = 0.5;
            MyMapView.Map.OperationalLayers.Add(layer);
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

            // Set the OAuthAuthorizeHandler component (this class) for Android or iOS platforms
#if __ANDROID__ || __IOS__
            thisAuthenticationManager.OAuthAuthorizeHandler = this;
#endif
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

        #region IOAuthAuthorizationHandler implementation
        // Use a TaskCompletionSource to track the completion of the authorization
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;

        // IOAuthAuthorizeHandler.AuthorizeAsync implementation
        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            // If the TaskCompletionSource is not null, authorization may already be in progress and should be cancelled
            if (_taskCompletionSource != null)
            {
                // Try to cancel any existing authentication task
                _taskCompletionSource.TrySetCanceled();
            }

            // Create a task completion source
            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();
#if __ANDROID__ || __IOS__

#if __ANDROID__
            // Get the current Android Activity
            var activity = Xamarin.Forms.Forms.Context as Activity; 
#endif
#if __IOS__
            // Get the current iOS ViewController
            var viewController = Xamarin.Forms.Platform.iOS.Platform.GetRenderer(this).ViewController;
#endif
            // Create a new Xamarin.Auth.OAuth2Authenticator using the information passed in
            Xamarin.Auth.OAuth2Authenticator authenticator = new Xamarin.Auth.OAuth2Authenticator(
                clientId: _appClientId,
                scope: "",
                authorizeUrl: authorizeUri,
                redirectUrl: callbackUri)
            {
                ShowErrors = false
            };

            // Allow the user to cancel the OAuth attempt
            authenticator.AllowCancel = true;

            // Define a handler for the OAuth2Authenticator.Completed event
            authenticator.Completed += (sender, authArgs) =>
            {
                try
                {
#if __IOS__
                    // Dismiss the OAuth UI when complete
                    viewController.DismissViewController(true, null);
#endif

                    // Check if the user is authenticated
                    if (authArgs.IsAuthenticated)
                    {
                        // If authorization was successful, get the user's account
                        Xamarin.Auth.Account authenticatedAccount = authArgs.Account;

                        // Set the result (Credential) for the TaskCompletionSource
                        _taskCompletionSource.SetResult(authenticatedAccount.Properties);
                    }
                    else
                    {
                        throw new Exception("Unable to authenticate user.");
                    }
                }
                catch (Exception ex)
                {
                    // If authentication failed, set the exception on the TaskCompletionSource
                    _taskCompletionSource.TrySetException(ex);

                    // Cancel authentication
                    authenticator.OnCancelled();
                }
#if __ANDROID__ 
                finally
                {
                    // Dismiss the OAuth login
                    activity.FinishActivity(99);
                }
#endif
            };

            // If an error was encountered when authenticating, set the exception on the TaskCompletionSource
            authenticator.Error += (sndr, errArgs) =>
            {
                // If the user cancels, the Error event is raised but there is no exception ... best to check first
                if (errArgs.Exception != null)
                {
                    _taskCompletionSource.TrySetException(errArgs.Exception);
                }
                else
                {
                    // Login canceled: dismiss the OAuth login
                    if (_taskCompletionSource != null)
                    {
                        _taskCompletionSource.TrySetCanceled();
#if __ANDROID__ 
                        activity.FinishActivity(99);
#endif
                    }
                }

                // Cancel authentication
                authenticator.OnCancelled();
            };

            // Present the OAuth UI so the user can enter user name and password
#if __ANDROID__
            var intent = authenticator.GetUI(activity);
            activity.StartActivityForResult(intent, 99);
#endif
#if __IOS__
            // Present the OAuth UI (on the app's UI thread) so the user can enter user name and password
            Device.BeginInvokeOnMainThread(() =>
            {
                viewController.PresentViewController(authenticator.GetUI(), true, null);
            });
#endif

#endif 
            // Return completion source task so the caller can await completion
            return _taskCompletionSource.Task;
        }
#endregion 
#endregion 

    }
}
