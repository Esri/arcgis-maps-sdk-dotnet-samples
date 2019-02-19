// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;
using Xamarin.Auth;

namespace ArcGISRuntime.Samples.SearchPortalMaps
{
    [Register("SearchPortalMaps")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Search a portal for maps",
        "Map",
        "This sample demonstrates searching a portal for web maps and loading them in the map view. You can search ArcGIS Online public web maps using tag values or browse the web maps in your account. OAuth is used to authenticate with ArcGIS Online to access items in your account.",
        "1. When the sample starts, you will be presented with a dialog for entering OAuth settings. If you need to create your own settings, sign in with your developer account and use the [ArcGIS for Developers dashboard](https://developers.arcgis.com/dashboard) to create an Application to store these settings.\n2. Enter values for the following OAuth settings.\n\t1. **Client ID**: a unique alphanumeric string identifier for your application\n\t2. **Redirect URL**: a URL to which a successful OAuth login response will be sent\n3. If you do not enter OAuth settings, you will be able to search public web maps on ArcGIS Online. Browsing the web map items in your ArcGIS Online account will be disabled, however.")]
    public class SearchPortalMaps : UIViewController, IOAuthAuthorizeHandler
    {
        // Hold a reference to the MapView.
        private MapView _myMapView;

        // Use a TaskCompletionSource to track the completion of the authorization.
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;

        // Variables for OAuth configuration and default values.
        // URL of the server to authenticate with.
        private const string ServerUrl = "https://www.arcgis.com/sharing/rest";

        // TODO: Add Client ID for an app registered with the server.
        private const string AppClientId = "2Gh53JRzkPtOENQq";

        // TODO: Add URL for redirecting after a successful authorization.
        //       Note - this must be a URL configured as a valid Redirect URI with your app.
        private const string OAuthRedirectUrl = "https://developers.arcgis.com";

        public SearchPortalMaps()
        {
            Title = "Search a portal for maps";
        }

        private void Initialize()
        {
            // Show a map with basemap by default.
            _myMapView.Map = new Map(Basemap.CreateLightGrayCanvas());

            // Update the authentication settings.
            UpdateAuthenticationManager();
        }

        private async void GetMyMaps_Clicked(object sender, EventArgs e)
        {
            try
            {
                await GetMyMaps();
            }
            catch (Exception ex)
            {
                UIAlertController alertController = UIAlertController.Create("There was a problem.", ex.ToString(),
                    UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                PresentViewController(alertController, true, null);
            }
        }

        private void SearchMaps_Clicked(object sender, EventArgs e)
        {
            // Prompt for the query.
            UIAlertController unionAlert = UIAlertController.Create("Search for maps", "Enter a search term.",
                UIAlertControllerStyle.Alert);
            unionAlert.AddTextField(field => { });
            unionAlert.AddAction(UIAlertAction.Create("Search", UIAlertActionStyle.Default,
                action => SearchTextEntered(unionAlert.TextFields[0].Text)));
            unionAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Show the alert.
            PresentViewController(unionAlert, true, null);
        }

        private async Task GetMyMaps()
        {
            // Call a sub that will force the user to log in to ArcGIS Online (if they haven't already).
            bool loggedIn = await EnsureLoggedInAsync();
            if (!loggedIn)
            {
                return;
            }

            // Connect to the portal (will connect using the provided credentials).
            ArcGISPortal portal = await ArcGISPortal.CreateAsync(new Uri(ServerUrl));

            // Get the user's content (items in the root folder and a collection of sub-folders).
            PortalUserContent myContent = await portal.User.GetContentAsync();

            // Get the web map items in the root folder.
            IEnumerable<PortalItem> mapItems =
                from item in myContent.Items where item.Type == PortalItemType.WebMap select item;

            // Loop through all sub-folders and get web map items, add them to the mapItems collection.
            foreach (PortalFolder folder in myContent.Folders)
            {
                IEnumerable<PortalItem> folderItems = await portal.User.GetContentAsync(folder.FolderId);
                mapItems = mapItems.Concat(
                    from item in folderItems where item.Type == PortalItemType.WebMap select item);
            }

            // Show the map results.
            ShowMapList(mapItems);
        }

        // Handle the SearchTextEntered event from the search input UI.
        // SearchMapsEventArgs contains the search text that was entered.
        private async void SearchTextEntered(string searchText)
        {
            try
            {
                // Connect to the portal (anonymously).
                ArcGISPortal portal = await ArcGISPortal.CreateAsync(new Uri(ServerUrl));

                // Create a query expression that will get public items of type 'web map' with the keyword(s) in the items tags.
                string queryExpression =
                    $"tags:\"{searchText}\" access:public type: (\"web map\" NOT \"web mapping application\")";

                // Create a query parameters object with the expression and a limit of 10 results.
                PortalQueryParameters queryParams = new PortalQueryParameters(queryExpression, 10);

                // Search the portal using the query parameters and await the results.
                PortalQueryResultSet<PortalItem> findResult = await portal.FindItemsAsync(queryParams);

                // Get the items from the query results.
                IEnumerable<PortalItem> mapItems = findResult.Results;

                // Show the map results.
                ShowMapList(mapItems);
            }
            catch (Exception ex)
            {
                // Report search error.
                UIAlertController alert = UIAlertController.Create("Error", ex.Message, UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
        }

        private void ShowMapList(IEnumerable<PortalItem> webmapItems)
        {
            // Create a new Alert Controller.
            UIAlertController mapListActionSheet =
                UIAlertController.Create("Web maps", "Choose a map", UIAlertControllerStyle.ActionSheet);

            // Add actions to load the available web maps.
            foreach (PortalItem item in webmapItems)
            {
                mapListActionSheet.AddAction(UIAlertAction.Create(item.Title, UIAlertActionStyle.Default,
                    async action => await DisplayMap(item.Url)));
            }

            // Add a choice to cancel.
            mapListActionSheet.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel,
                action => Console.WriteLine("Canceled")));

            // Required for iPad - You must specify a source for the Action Sheet since it is displayed as a popover.
            UIPopoverPresentationController presentationPopover = mapListActionSheet.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the list of maps.
            PresentViewController(mapListActionSheet, true, null);
        }

        private async Task DisplayMap(Uri webMapUri)
        {
            Map webMap = new Map(webMapUri);
            try
            {
                await webMap.LoadAsync();
            }
            catch (Esri.ArcGISRuntime.ArcGISRuntimeException e)
            {
                UIAlertView alert =
                    new UIAlertView("Map Load Error", e.Message, (IUIAlertViewDelegate) null, "OK", null);
                alert.Show();
            }

            // Handle change in the load status (to report load errors).
            webMap.LoadStatusChanged += WebMapLoadStatusChanged;

            _myMapView.Map = webMap;
        }

        private void WebMapLoadStatusChanged(object sender, Esri.ArcGISRuntime.LoadStatusEventArgs e)
        {
            // Report errors if map failed to load.
            if (e.Status == LoadStatus.FailedToLoad)
            {
                Map map = (Map) sender;
                Exception err = map.LoadError;
                if (err != null)
                {
                    UIAlertView alert = new UIAlertView("Map Load Error", err.Message, (IUIAlertViewDelegate) null,
                        "OK", null);
                    alert.Show();
                }
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem("Search maps", UIBarButtonItemStyle.Plain, SearchMaps_Clicked),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem("Get my maps", UIBarButtonItemStyle.Plain, GetMyMaps_Clicked)
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor)
            });
        }

        #region OAuth helpers

        private void UpdateAuthenticationManager()
        {
            // Register the server information with the AuthenticationManager.
            ServerInfo portalServerInfo = new ServerInfo
            {
                ServerUri = new Uri(ServerUrl),
                OAuthClientInfo = new OAuthClientInfo
                {
                    ClientId = AppClientId,
                    RedirectUri = new Uri(OAuthRedirectUrl)
                },
                // Specify OAuthAuthorizationCode if you need a refresh token (and have specified a valid client secret).
                // Otherwise, use OAuthImplicit.
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
            };

            // Get a reference to the (singleton) AuthenticationManager for the app.
            AuthenticationManager thisAuthenticationManager = AuthenticationManager.Current;

            // Register the server information.
            thisAuthenticationManager.RegisterServer(portalServerInfo);

            // Assign the method that AuthenticationManager will call to challenge for secured resources.
            thisAuthenticationManager.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Set the OAuth authorization handler to this class (Implements IOAuthAuthorize interface).
            thisAuthenticationManager.OAuthAuthorizeHandler = this;
        }

        private async Task<bool> EnsureLoggedInAsync()
        {
            bool loggedIn = false;

            try
            {
                // Create a challenge request for portal credentials (OAuth credential request for arcgis.com).
                CredentialRequestInfo challengeRequest = new CredentialRequestInfo
                {
                    // Use the OAuth implicit grant flow.
                    GenerateTokenOptions = new GenerateTokenOptions
                    {
                        TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
                    },

                    // Indicate the URL (portal) to authenticate with (ArcGIS Online).
                    ServiceUri = new Uri(ServerUrl)
                };

                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler.
                Credential cred = await AuthenticationManager.Current.GetCredentialAsync(challengeRequest, false);
                loggedIn = cred != null;
            }
            catch (OperationCanceledException)
            {
                // Login was canceled.
                // .. ignore, user can still search public maps without logging in.
            }
            catch (Exception ex)
            {
                // Login failure.
                UIAlertView alert = new UIAlertView("Login Error", ex.Message, (IUIAlertViewDelegate) null, "OK", null);
                alert.Show();
            }

            return loggedIn;
        }

        // ChallengeHandler function for AuthenticationManager, called whenever access to a secured resource is attempted.
        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            OAuthTokenCredential credential = null;

            try
            {
                // Create generate token options if necessary.
                if (info.GenerateTokenOptions == null)
                {
                    info.GenerateTokenOptions = new GenerateTokenOptions();
                }

                // AuthenticationManager will handle challenging the user for credentials.
                credential = await AuthenticationManager.Current.GenerateCredentialAsync
                (
                    info.ServiceUri,
                    info.GenerateTokenOptions
                ) as OAuthTokenCredential;
            }
            catch (TaskCanceledException)
            {
                return credential;
            }
            catch (Exception)
            {
                // Exception will be reported in calling function.
                throw;
            }

            return credential;
        }

        // IOAuthAuthorizeHandler.AuthorizeAsync implementation.
        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            // If the TaskCompletionSource is not null, authorization may already be in progress and should be canceled.
            // Try to cancel any existing authentication task.
            _taskCompletionSource?.TrySetCanceled();

            // Create a task completion source.
            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();

            // Create a new Xamarin.Auth.OAuth2Authenticator using the information passed in.
            Xamarin.Auth.OAuth2Authenticator auth = new OAuth2Authenticator(
                clientId: AppClientId,
                scope: "",
                authorizeUrl: authorizeUri,
                redirectUrl: callbackUri)
            {
                ShowErrors = false,
                // Allow the user to cancel the OAuth attempt.
                AllowCancel = true
            };

            // Define a handler for the OAuth2Authenticator.Completed event.
            auth.Completed += (sender, authArgs) =>
            {
                try
                {
                    // Dismiss the OAuth UI when complete.
                    DismissViewController(true, null);

                    // Throw an exception if the user could not be authenticated.
                    if (!authArgs.IsAuthenticated)
                    {
                        throw new Exception("Unable to authenticate user.");
                    }

                    // If authorization was successful, get the user's account.
                    Xamarin.Auth.Account authenticatedAccount = authArgs.Account;

                    // Set the result (Credential) for the TaskCompletionSource.
                    _taskCompletionSource.SetResult(authenticatedAccount.Properties);
                }
                catch (Exception ex)
                {
                    // If authentication failed, set the exception on the TaskCompletionSource.
                    _taskCompletionSource.TrySetException(ex);

                    // Cancel authentication.
                    auth.OnCancelled();
                }
            };

            // If an error was encountered when authenticating, set the exception on the TaskCompletionSource.
            auth.Error += (sender, errArgs) =>
            {
                if (errArgs.Exception != null)
                {
                    _taskCompletionSource.TrySetException(errArgs.Exception);
                }
                else
                {
                    _taskCompletionSource.TrySetException(new Exception(errArgs.Message));
                }

                // Cancel authentication.
                auth.OnCancelled();
            };

            // Present the OAuth UI (on the app's UI thread) so the user can enter user name and password.
            InvokeOnMainThread(() => { PresentViewController(auth.GetUI(), true, null); });

            // Return completion source task so the caller can await completion.
            return _taskCompletionSource.Task;
        }

        #endregion OAuth helpers
    }
}