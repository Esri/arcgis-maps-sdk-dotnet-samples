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
using Xamarin.Forms;


#if __IOS__
using Xamarin.Forms.Platform.iOS;
using Xamarin.Auth;
using UIKit;
#endif

#if __ANDROID__
using Android.App;
using Xamarin.Auth;
#endif

namespace ArcGISRuntime.Samples.SearchPortalMaps
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Search a portal for maps",
        "Map",
        "This sample demonstrates searching a portal for web maps and loading them in the map view. You can search ArcGIS Online public web maps using tag values or browse the web maps in your account. OAuth is used to authenticate with ArcGIS Online to access items in your account.",
        "1. When the sample starts, you will be presented with a dialog for entering OAuth settings. If you need to create your own settings, sign in with your developer account and use the [ArcGIS for Developers dashboard](https://developers.arcgis.com/dashboard) to create an Application to store these settings.\n2. Enter values for the following OAuth settings.\n\t1. **Client ID**: a unique alphanumeric string identifier for your application\n\t2. **Redirect URL**: a URL to which a successful OAuth login response will be sent\n3. If you do not enter OAuth settings, you will be able to search public web maps on ArcGIS Online. Browsing the web map items in your ArcGIS Online account will be disabled, however.")]
    public partial class SearchPortalMaps : ContentPage, IOAuthAuthorizeHandler
    {
        // Constants for OAuth-related values ...
        // URL of the server to authenticate with (ArcGIS Online)
        private const string ArcGISOnlineUrl = "https://www.arcgis.com/sharing/rest";

        // Client ID for the app registered with the server (Portal Maps)
        public string _appClientId = "2Gh53JRzkPtOENQq";

        // Redirect URL after a successful authorization (configured for the Portal Maps application)
        private string _oAuthRedirectUrl = "https://developers.arcgis.com";

        public SearchPortalMaps()
        {
            InitializeComponent();

            // Display a default map
            DisplayDefaultMap();

            // Show the default OAuth settings in the entry controls
            ClientIDEntry.Text = _appClientId;
            RedirectUrlEntry.Text = _oAuthRedirectUrl;

            // Change the style of the layer list view for Android and UWP
            Device.OnPlatform(
                Android: () =>
                {
                    // Black background on Android (transparent by default)
                    MapsListView.BackgroundColor = Color.Black;
                    SearchMapsUI.BackgroundColor = Color.Black;
                    OAuthSettingsGrid.BackgroundColor = Color.Black;
                },
                WinPhone: () =>
                {
                    // Semi-transparent background on Windows with a small margin around the control
                    MapsListView.BackgroundColor = Color.FromRgba(255, 255, 255, 0.3);
                    MapsListView.Margin = new Thickness(50);
                    SearchMapsUI.BackgroundColor = Color.FromRgba(255, 255, 255, 0.3);
                    SearchMapsUI.Margin = new Thickness(50);
                    OAuthSettingsGrid.BackgroundColor = Color.FromRgba(255, 255, 255, 0.3);
                    OAuthSettingsGrid.Margin = new Thickness(50);
                });
        }

        private void DisplayDefaultMap()
        {
            // Create a new Map instance
            Map myMap = new Map(Basemap.CreateStreets());

            // Provide Map to the MapView
            MyMapView.Map = myMap;
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

        private async void SearchPublicMaps(string searchText)
        {
            // Get web map portal items from a keyword search
            IEnumerable<PortalItem> mapItems = null;
            ArcGISPortal portal;

            // Connect to the portal (anonymously)
            portal = await ArcGISPortal.CreateAsync(new Uri(ArcGISOnlineUrl));

            // Create a query expression that will get public items of type 'web map' with the keyword(s) in the items tags
            var queryExpression = string.Format("tags:\"{0}\" access:public type: (\"web map\" NOT \"web mapping application\")", searchText);
            
            // Create a query parameters object with the expression and a limit of 10 results
            PortalQueryParameters queryParams = new PortalQueryParameters(queryExpression, 10);

            // Search the portal using the query parameters and await the results
            PortalQueryResultSet<PortalItem> findResult = await portal.FindItemsAsync(queryParams);
            
            // Get the items from the query results
            mapItems = findResult.Results;

            // Hide the search controls
            SearchMapsUI.IsVisible = false;

            // Show the list of web maps
            MapsListView.ItemsSource = mapItems;
            MapsListView.IsVisible = true;
        }

        private void ShowSearchUI(object sender, EventArgs e)
        {
            // Show the map search controls
            SearchMapsUI.IsVisible = true;
        }

        private async void GetMyMaps(object sender, EventArgs e)
        {
            // Get web map portal items in the current user's folder or from a keyword search
            IEnumerable<PortalItem> mapItems = null;
            ArcGISPortal portal;

            // Call a sub that will force the user to log in to ArcGIS Online (if they haven't already)
            var loggedIn = await EnsureLoggedInAsync();
            if (!loggedIn) { return; }

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
                mapItems.Concat(from item in folderItems where item.Type == PortalItemType.WebMap select item);
            }

            // Show the list of web maps
            MapsListView.ItemsSource = mapItems;
            MapsListView.IsVisible = true;
        }
        
        private void MapItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // Get the selected web map item in the list box
            PortalItem selectedMap = e.SelectedItem as PortalItem;
            if (selectedMap == null) { return; }

            // Create a new map, pass the web map portal item to the constructor
            Map webMap = new Map(selectedMap);

            // Handle change in the load status (to report load errors)
            webMap.LoadStatusChanged += WebMapLoadStatusChanged;

            // Show the web map in the map view
            MyMapView.Map = webMap;

            // Hide the list of maps
            MapsListView.IsVisible = false;
        }

        private void WebMapLoadStatusChanged(object sender, Esri.ArcGISRuntime.LoadStatusEventArgs e)
        {
            // Get the current status
            var status = e.Status;

            // Report errors if map failed to load
            if (status == Esri.ArcGISRuntime.LoadStatus.FailedToLoad)
            {
                var map = sender as Map;
                var err = map.LoadError;
                if (err != null)
                {
                    DisplayAlert(err.Message, "Map Load Error", "OK");
                }
            }
        }

        private async Task<bool> EnsureLoggedInAsync()
        {
            bool loggedIn = false;

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
                challengeRequest.ServiceUri = new Uri(ArcGISOnlineUrl);

                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                var cred = await AuthenticationManager.Current.GetCredentialAsync(challengeRequest, false);
                loggedIn = cred != null;
            }
            catch (OperationCanceledException)
            {
                // Login was canceled
                // .. ignore, user can still search public maps without logging in
            }
            catch (Exception ex)
            {
                // Login failure
                await DisplayAlert("Login Error", ex.Message, "OK");
            }

            return loggedIn;
        }

        private void CancelSearchClicked(object sender, EventArgs e)
        {
            // Hide the search controls if the cancel button is clicked
            SearchMapsUI.IsVisible = false;
        }

        private void SearchMapsClicked(object sender, EventArgs e)
        {
            // Search ArcGIS Online maps with the text entered
            SearchPublicMaps(SearchTextEntry.Text);
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
