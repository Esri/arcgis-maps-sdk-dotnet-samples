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
using System.IO;


#if __IOS__
using Xamarin.Forms.Platform.iOS;
using Xamarin.Auth;
using UIKit;
#endif

#if __ANDROID__
using Android.App;
using Application = Xamarin.Forms.Application;
using Xamarin.Auth;
#endif

namespace ArcGISRuntime.Samples.AuthorMap
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Create and save map",
        category: "Map",
        description: "Create and save a map as an ArcGIS `PortalItem` (i.e. web map).",
        instructions: "1. Select the basemap and layers you'd like to add to your map.",
        tags: new[] { "ArcGIS Online", "OAuth", "portal", "publish", "share", "web map" })]
    [ArcGISRuntime.Samples.Shared.Attributes.ClassFile("SaveMapPage.xaml.cs")]
    [ArcGISRuntime.Samples.Shared.Attributes.XamlFiles("SaveMapPage.xaml")]
    public partial class AuthorMap : ContentPage, IOAuthAuthorizeHandler
    {
        // OAuth-related values ...
        // URL of the server to authenticate with (ArcGIS Online)
        private const string ArcGISOnlineUrl = "https://www.arcgis.com/sharing/rest";

        // Client ID for the app registered with the server (Portal Maps)
        public static string AppClientId = "6wMAmbUEX1rvsOb4";

        // Redirect URL after a successful authorization (configured for the Portal Maps application)
        private string _oAuthRedirectUrl = "forms-samples-app://auth";

        // String array to store basemap constructor types
        private string[] _basemapTypes = {
            "Topographic",
            "Streets",
            "Imagery",
            "Oceans"
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

            // call a function to initialize the app (display a map, etc.)
            Initialize();
        }

        private void Initialize()
        {
            // Call a function to create a new map with a light gray canvas basemap
            CreateNewMap();

            // Show the default OAuth settings in the entry controls
            ClientIDEntry.Text = AppClientId;
            RedirectUrlEntry.Text = _oAuthRedirectUrl;

            // Change the style of the layer list view for Android and UWP
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    LayersList.BackgroundColor = Color.Black;
                    OAuthSettingsGrid.BackgroundColor = Color.Gray;
                    break;
                case Device.UWP:
                    LayersList.BackgroundColor = Color.FromRgba(255, 255, 255, 0.3);
                    LayersList.Margin = new Thickness(50);
                    break;
            }
        }

        private void OAuthSettingsCancel(object sender, EventArgs e)
        {
            OAuthSettingsGrid.IsVisible = false;
        }

        private void SaveOAuthSettings(object sender, EventArgs e)
        {
            var appClientId = ClientIDEntry.Text.Trim();
            var oAuthRedirectUrl = RedirectUrlEntry.Text.Trim();

            if (!String.IsNullOrWhiteSpace(appClientId) && !String.IsNullOrWhiteSpace(oAuthRedirectUrl))
            {
                AppClientId = appClientId;
                _oAuthRedirectUrl = oAuthRedirectUrl;
            }

            OAuthSettingsGrid.IsVisible = false;

            // Call a function to set up the AuthenticationManager
            UpdateAuthenticationManager();
        }

        private void LayerSelected(object sender, ItemTappedEventArgs e)
        {
            // return if null
            if (e.Item == null) { return; }

            // Handle the event when a layer item is selected (tapped) in the layer list
            string selectedItem = e.Item.ToString();

            // See if this is one of the layers in the operational layers list 
            if (_operationalLayerUrls.ContainsKey(selectedItem))
            {
                // Get the service URL from the operational layers dictionary
                string value = _operationalLayerUrls[selectedItem];

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
            Button button = (Button)sender;
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
            SaveMapPage mapInputForm = new SaveMapPage();

            // If an existing map, show the UI for updating the item
            Item mapItem = MyMapView.Map.Item;
            if (mapItem != null)
            {
                mapInputForm.ShowForUpdate(mapItem.Title,mapItem.Description, mapItem.Tags.ToArray());
            }

            // Handle the save button click event on the page
            mapInputForm.OnSaveClicked += SaveMapAsync;

            // Navigate to the SaveMapPage UI
            await Navigation.PushAsync(mapInputForm);
        }

        // Event handler to get information entered by the user and save the map
        private async void SaveMapAsync(object sender, SaveMapEventArgs e)
        {
            // Get the current map
            Map myMap = MyMapView.Map;

            try
            {
                // Show the progress bar so the user knows work is happening
                SaveMapProgressBar.IsVisible = true;

                // Make sure the user is logged in to ArcGIS Online
                Credential cred = await EnsureLoggedInAsync();
                AuthenticationManager.Current.AddCredential(cred);

                // Get information entered by the user for the new portal item properties
                string title = e.MapTitle;
                string description = e.MapDescription;
                string[] tags = e.Tags;

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
                    await Application.Current.MainPage.DisplayAlert("Map Saved", "Saved '" + title + "' to ArcGIS Online!", "OK");
                }
                else
                {
                    // This is not the initial save, call SaveAsync to save changes to the existing portal item
                    await myMap.SaveAsync();

                    // Get the file stream from the new thumbnail image
                    Stream imageStream = await thumbnailImage.GetEncodedBufferAsync();

                    // Update the item thumbnail
                    ((PortalItem)myMap.Item).SetThumbnail(imageStream);
                    await myMap.SaveAsync();

                    // Report update was successful
                    await Application.Current.MainPage.DisplayAlert("Updates Saved", "Saved changes to '" + myMap.Item.Title + "'", "OK");
                }
            }
            catch (Exception ex)
            {
                // Show the exception message
                await Application.Current.MainPage.DisplayAlert("Unable to save map", ex.Message, "OK");
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
            CredentialRequestInfo loginInfo = new CredentialRequestInfo
            {

                // Use the OAuth implicit grant flow
                GenerateTokenOptions = new GenerateTokenOptions
                {
                    TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
                },

                // Indicate the url (portal) to authenticate with (ArcGIS Online)
                ServiceUri = new Uri(ArcGISOnlineUrl)
            };

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
            Map myMap = new Map(BasemapStyle.ArcGISLightGray);

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
                    MyMapView.Map.Basemap = new Basemap(BasemapStyle.ArcGISTopographic);
                    break;
                case "Streets":
                    // Set the basemap to Streets
                    MyMapView.Map.Basemap = new Basemap(BasemapStyle.ArcGISStreets);
                    break;
                case "Imagery":
                    // Set the basemap to Imagery
                    MyMapView.Map.Basemap = new Basemap(BasemapStyle.ArcGISImageryStandard);
                    break;
                case "Oceans":
                    // Set the basemap to Oceans
                    MyMapView.Map.Basemap = new Basemap(BasemapStyle.ArcGISOceans);
                    break;
            }
        }

        private void AddLayer(string layerName, string url)
        {
            // See if the layer already exists, and remove it if it does
            if (MyMapView.Map.OperationalLayers.FirstOrDefault(l => l.Name == layerName) is ArcGISMapImageLayer layer)
            {
                MyMapView.Map.OperationalLayers.Remove(layer);
            }
            else
            {
                // Otherwise, add the layer
                Uri layerUri = new Uri(url);

                // Create and add a new map image layer
                layer = new ArcGISMapImageLayer(layerUri);
                layer.Name = layerName;
                layer.Opacity = 0.5;
                MyMapView.Map.OperationalLayers.Add(layer);
            }
        }

        #region OAuth
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
                ClientId = AppClientId,
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
            Activity activity = (Activity)ArcGISRuntime.Droid.MainActivity.Instance;
#endif
            // Create a new Xamarin.Auth.OAuth2Authenticator using the information passed in
            Xamarin.Auth.OAuth2Authenticator authenticator = new Xamarin.Auth.OAuth2Authenticator(
                clientId: AppClientId,
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
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        var viewController = UIApplication.SharedApplication.KeyWindow.RootViewController;
                        viewController.DismissViewController(true, null);
                    });
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
                var viewController = UIApplication.SharedApplication.KeyWindow.RootViewController;
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