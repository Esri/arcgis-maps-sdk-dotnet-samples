// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Auth;

namespace ArcGISRuntime.Samples.SearchPortalMaps
{
    [Register("SearchPortalMaps")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Search for webmap",
        "Map",
        "Find webmap portal items by using a search term.",
        "Enter search terms into the search bar. Once the search is complete, a list is populated with the resultant webmaps. Tap on a webmap to set it to the map view. Scrolling to the bottom of the webmap recycler view will get more results.",
        "keyword", "query", "search", "webmap")]
    public class SearchPortalMaps : UIViewController, IOAuthAuthorizeHandler
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _searchButton;
        private UIBarButtonItem _myMapsButton;
        private bool _myMapsLastClicked;

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

        // Hold a reference to the authenticator.
        private Xamarin.Auth.OAuth2Authenticator _auth;

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
                // For UI popup.
                _myMapsLastClicked = true;

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
            // For UI popup.
            _myMapsLastClicked = false;

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
                presentationPopover.BarButtonItem = _myMapsLastClicked ? _myMapsButton : _searchButton;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Down;
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
                    new UIAlertView("Map Load Error", e.Message, (IUIAlertViewDelegate)null, "OK", null);
                alert.Show();
            }

            // Handle change in the load status (to report load errors).
            webMap.LoadStatusChanged += WebMapLoadStatusChanged;

            _myMapView.Map = webMap;
        }

        private void WebMapLoadStatusChanged(object sender, Esri.ArcGISRuntime.LoadStatusEventArgs e)
        {
            Map map = (Map)sender;

            // Report errors if map failed to load.
            if (e.Status == LoadStatus.Loaded)
            {
                // Unsubscribe from event.
                map.LoadStatusChanged -= WebMapLoadStatusChanged;
            }
            else if (e.Status == LoadStatus.FailedToLoad)
            {
                // Unsubscribe from event.
                map.LoadStatusChanged -= WebMapLoadStatusChanged;

                // Show the error
                Exception err = map.LoadError;
                if (err != null)
                {
                    UIAlertView alert = new UIAlertView("Map Load Error", err.Message, (IUIAlertViewDelegate)null,
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
            View = new UIView { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _searchButton = new UIBarButtonItem();
            _searchButton.Title = "Search maps";

            _myMapsButton = new UIBarButtonItem();
            _myMapsButton.Title = "Get my maps";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                _searchButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _myMapsButton
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

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _searchButton.Clicked += SearchMaps_Clicked;
            _myMapsButton.Clicked += GetMyMaps_Clicked;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _searchButton.Clicked -= SearchMaps_Clicked;
            _myMapsButton.Clicked -= GetMyMaps_Clicked;
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
                UIAlertView alert = new UIAlertView("Login Error", ex.Message, (IUIAlertViewDelegate)null, "OK", null);
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
            // Try to cancel any existing authentication process.
            _taskCompletionSource?.TrySetCanceled();

            // Create a task completion source.
            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();

            // Create a new Xamarin.Auth.OAuth2Authenticator using the information passed in.
            _auth = new OAuth2Authenticator(
                clientId: AppClientId,
                scope: "",
                authorizeUrl: authorizeUri,
                redirectUrl: new Uri(OAuthRedirectUrl))
            {
                ShowErrors = false,
                // Allow the user to cancel the OAuth attempt.
                AllowCancel = true
            };

            // Define a handler for the OAuth2Authenticator.Completed event.
            _auth.Completed += (o, authArgs) =>
            {
                try
                {
                    // Dismiss the OAuth UI when complete.
                    InvokeOnMainThread(() => { UIApplication.SharedApplication.KeyWindow.RootViewController.DismissViewController(true, null); });

                    // Check if the user is authenticated.
                    if (authArgs.IsAuthenticated)
                    {
                        // If authorization was successful, get the user's account.
                        Xamarin.Auth.Account authenticatedAccount = authArgs.Account;

                        // Set the result (Credential) for the TaskCompletionSource.
                        _taskCompletionSource.SetResult(authenticatedAccount.Properties);
                    }
                    else
                    {
                        throw new Exception("Unable to authenticate user.");
                    }
                }
                catch (Exception ex)
                {
                    // If authentication failed, set the exception on the TaskCompletionSource.
                    _taskCompletionSource.TrySetException(ex);

                    // Cancel authentication.
                    _auth.OnCancelled();
                }
            };

            // If an error was encountered when authenticating, set the exception on the TaskCompletionSource.
            _auth.Error += (o, errArgs) =>
            {
                // If the user cancels, the Error event is raised but there is no exception ... best to check first.
                if (errArgs.Exception != null)
                {
                    _taskCompletionSource.TrySetException(errArgs.Exception);
                }
                else
                {
                    // Login canceled: dismiss the OAuth login.
                    _taskCompletionSource?.TrySetCanceled();
                }

                // Cancel authentication.
                _auth.OnCancelled();
                _auth = null;
            };

            // Present the OAuth UI (on the app's UI thread) so the user can enter user name and password.
            InvokeOnMainThread(() => { UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(_auth.GetUI(), true, null); });

            // Return completion source task so the caller can await completion.
            return _taskCompletionSource.Task;
        }

        #endregion OAuth helpers
    }
}