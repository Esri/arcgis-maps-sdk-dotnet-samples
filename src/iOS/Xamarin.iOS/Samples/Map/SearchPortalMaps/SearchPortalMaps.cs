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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
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
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UISegmentedControl _segmentButton = new UISegmentedControl();
        private readonly UIToolbar _toolbar = new UIToolbar();
        private OAuthPropsDialogOverlay _oauthInfoUi;
        private SearchMapsDialogOverlay _searchMapsUi;

        // Use a TaskCompletionSource to track the completion of the authorization.
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;

        // Variables for OAuth configuration and default values.
        // URL of the server to authenticate with.
        private const string ServerUrl = "https://www.arcgis.com/sharing/rest";

        // TODO: Add Client ID for an app registered with the server.
        private string _appClientId = "2Gh53JRzkPtOENQq";

        // TODO: Add URL for redirecting after a successful authorization.
        //       Note - this must be a URL configured as a valid Redirect URI with your app.
        private string _oAuthRedirectUrl = "https://developers.arcgis.com";

        public SearchPortalMaps()
        {
            Title = "Search a portal for maps";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization.
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat controlHeight = 30;
                nfloat margin = 5;
                nfloat toolbarHeight = controlHeight + 2 * margin;

                // Reposition the views.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);
                _toolbar.Frame = new CGRect(0, View.Bounds.Height - toolbarHeight, View.Bounds.Width, toolbarHeight);
                _segmentButton.Frame = new CGRect(margin, _toolbar.Frame.Top + margin, View.Bounds.Width - (2 * margin), controlHeight);

                if (_searchMapsUi != null)
                {
                    _searchMapsUi.Frame = new CGRect(0, topMargin, View.Bounds.Width, View.Bounds.Height);
                }

                if (_oauthInfoUi != null)
                {
                    _oauthInfoUi.Frame = new CGRect(0, topMargin, View.Bounds.Width, View.Bounds.Height);
                }

                base.ViewDidLayoutSubviews();
            }
            catch (NullReferenceException)
            {
            }
        }

        private void Initialize()
        {
            // Show a map with basemap by default.
            _myMapView.Map = new Map(Basemap.CreateLightGrayCanvas());

            // Prompt the user for OAuth settings.
            ShowOAuthPropsUi();
        }

        private void ShowOAuthPropsUi()
        {
            if (_oauthInfoUi != null)
            {
                return;
            }

            nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

            // Create a view to show entry controls over the map view.
            var ovBounds = new CGRect(0, topMargin, View.Bounds.Width, View.Bounds.Height);
            _oauthInfoUi = new OAuthPropsDialogOverlay(ovBounds, 0.75f, UIColor.White, _appClientId, _oAuthRedirectUrl);

            // Handle the OnOAuthPropsInfoEntered event to get the info entered by the user.
            _oauthInfoUi.OnOAuthPropsInfoEntered += (s, e) =>
            {
                // Store the settings entered and use them to update the AuthenticationManager.
                _appClientId = e.ClientId;
                _oAuthRedirectUrl = e.RedirectUrl;
                UpdateAuthenticationManager();

                // Hide the OAuth entry
                _oauthInfoUi.Hide();
                _oauthInfoUi = null;
            };

            // Handle the cancel event when the user closes the dialog without choosing to save.
            _oauthInfoUi.OnCanceled += (s, e) =>
            {
                _oauthInfoUi.Hide();
                _oauthInfoUi = null;
            };

            // Add the map item info UI view (will display semi-transparent over the map view).
            View.Add(_oauthInfoUi);
        }

        private void CreateLayout()
        {
            // Configure segmented button control.
            _segmentButton.BackgroundColor = UIColor.White;
            _segmentButton.InsertSegment("Search Maps", 0, false);
            _segmentButton.InsertSegment("My Maps", 1, false);

            // Handle the "click" for each segment (new segment is selected).
            _segmentButton.ValueChanged += SegmentButtonClicked;

            // Add the MapView and segmented button to the page.
            View.AddSubviews(_myMapView, _toolbar, _segmentButton);
        }

        private void SegmentButtonClicked(object sender, EventArgs e)
        {
            // Get the segmented button control that raised the event.
            var buttonControl = sender as UISegmentedControl;

            switch (buttonControl.SelectedSegment)
            {
                case 0:
                    // Show search UI.
                    ShowSearchMapUi();
                    break;
                case 1:
                    // Authenticate user on ArcGIS Online, then show their maps.
                    GetMyMaps();
                    break;
            }

            // Deselect all segments (user might want to click the same control twice).
            buttonControl.SelectedSegment = -1;
        }

        private void ShowSearchMapUi()
        {
            if (_searchMapsUi != null)
            {
                return;
            }

            nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

            // Create a view to show map item info entry controls over the map view.
            var ovBounds = new CGRect(0, topMargin, View.Bounds.Width, View.Bounds.Height);
            _searchMapsUi = new SearchMapsDialogOverlay(ovBounds, 0.75f, UIColor.White);

            // Handle the OnSearchMapsTextEntered event to get the info entered by the user.
            _searchMapsUi.OnSearchMapsTextEntered += SearchTextEntered;

            // Handle the cancel event when the user closes the dialog without choosing to search..
            _searchMapsUi.OnCanceled += SearchCanceled;

            // Add the search UI view (will display semi-transparent over the map view).
            View.Add(_searchMapsUi);
        }

        private async void GetMyMaps()
        {
            // Call a sub that will force the user to log in to ArcGIS Online (if they haven't already).
            bool loggedIn = await EnsureLoggedInAsync();
            if (!loggedIn)
            {
                return;
            }

            // Connect to the portal (will connect using the provided credentials).
            var portal = await ArcGISPortal.CreateAsync(new Uri(ServerUrl));

            // Get the user's content (items in the root folder and a collection of sub-folders).
            PortalUserContent myContent = await portal.User.GetContentAsync();

            // Get the web map items in the root folder.
            IEnumerable<PortalItem> mapItems = from item in myContent.Items where item.Type == PortalItemType.WebMap select item;

            // Loop through all sub-folders and get web map items, add them to the mapItems collection.
            foreach (PortalFolder folder in myContent.Folders)
            {
                IEnumerable<PortalItem> folderItems = await portal.User.GetContentAsync(folder.FolderId);
                mapItems = mapItems.Concat(from item in folderItems where item.Type == PortalItemType.WebMap select item);
            }

            // Show the map results.
            ShowMapList(mapItems);
        }

        // Handle the SearchTextEntered event from the search input UI.
        // SearchMapsEventArgs contains the search text that was entered.
        private async void SearchTextEntered(object sender, SearchMapsEventArgs e)
        {
            try
            {
                // Connect to the portal (anonymously).
                var portal = await ArcGISPortal.CreateAsync(new Uri(ServerUrl));

                // Create a query expression that will get public items of type 'web map' with the keyword(s) in the items tags.
                string queryExpression = $"tags:\"{e.SearchText}\" access:public type: (\"web map\" NOT \"web mapping application\")";

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
            finally
            {
                // Get rid of the search input controls.
                _searchMapsUi.Hide();
                _searchMapsUi = null;
            }
        }

        private void ShowMapList(IEnumerable<PortalItem> webmapItems)
        {
            // Create a new Alert Controller.
            UIAlertController mapListActionSheet = UIAlertController.Create("Web maps", "Choose a map", UIAlertControllerStyle.ActionSheet);

            // Add actions to load the available web maps.
            foreach (var item in webmapItems)
            {
                mapListActionSheet.AddAction(UIAlertAction.Create(item.Title, UIAlertActionStyle.Default, action => DisplayMap(item.Url)));
            }

            // Add a choice to cancel.
            mapListActionSheet.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, action => Console.WriteLine("Canceled")));

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

        private async void DisplayMap(Uri webMapUri)
        {
            var webMap = new Map(webMapUri);
            try
            {
                await webMap.LoadAsync();
            }
            catch (Esri.ArcGISRuntime.ArcGISRuntimeException e)
            {
                var alert = new UIAlertView("Map Load Error", e.Message, (IUIAlertViewDelegate) null, "OK", null);
                alert.Show();
            }

            // Handle change in the load status (to report load errors).
            webMap.LoadStatusChanged += WebMapLoadStatusChanged;

            _myMapView.Map = webMap;
        }

        private void WebMapLoadStatusChanged(object sender, Esri.ArcGISRuntime.LoadStatusEventArgs e)
        {
            // Get the current status.
            var status = e.Status;

            // Report errors if map failed to load.
            if (status == Esri.ArcGISRuntime.LoadStatus.FailedToLoad)
            {
                var map = sender as Map;
                var err = map.LoadError;
                if (err != null)
                {
                    var alert = new UIAlertView("Map Load Error", err.Message, (IUIAlertViewDelegate) null, "OK", null);
                    alert.Show();
                }
            }
        }

        private void SearchCanceled(object sender, EventArgs e)
        {
            // Remove the search input UI.
            _searchMapsUi.Hide();
            _searchMapsUi = null;
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
                    ClientId = _appClientId,
                    RedirectUri = new Uri(_oAuthRedirectUrl)
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
                CredentialRequestInfo challengeRequest = new CredentialRequestInfo();

                // Use the OAuth implicit grant flow.
                challengeRequest.GenerateTokenOptions = new GenerateTokenOptions
                {
                    TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
                };

                // Indicate the URL (portal) to authenticate with (ArcGIS Online).
                challengeRequest.ServiceUri = new Uri(ServerUrl);

                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler.
                var cred = await AuthenticationManager.Current.GetCredentialAsync(challengeRequest, false);
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
                var alert = new UIAlertView("Login Error", ex.Message, (IUIAlertViewDelegate) null, "OK", null);
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
                clientId: _appClientId,
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
            auth.Error += (sndr, errArgs) =>
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

        private static IDictionary<string, string> DecodeParameters(Uri uri)
        {
            // Create a dictionary of key value pairs returned in an OAuth authorization response URI query string.
            string answer = string.Empty;

            // Get the values from the URI fragment or query string.
            if (!string.IsNullOrEmpty(uri.Fragment))
            {
                answer = uri.Fragment.Substring(1);
            }
            else
            {
                if (!string.IsNullOrEmpty(uri.Query))
                {
                    answer = uri.Query.Substring(1);
                }
            }

            // Parse parameters into key / value pairs.
            Dictionary<string, string> keyValueDictionary = new Dictionary<string, string>();
            string[] keysAndValues = answer.Split(new[] {'&'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string kvString in keysAndValues)
            {
                string[] pair = kvString.Split('=');
                string key = pair[0];
                string value = string.Empty;
                if (key.Length > 1)
                {
                    value = Uri.UnescapeDataString(pair[1]);
                }

                keyValueDictionary.Add(key, value);
            }

            // Return the dictionary of string keys/values.
            return keyValueDictionary;
        }

        #endregion OAuth helpers
    }

    #region UI for entering OAuth configuration settings

    // View containing "configure OAuth" controls (client id and redirect URL inputs with save/cancel buttons).
    public class OAuthPropsDialogOverlay : UIView
    {
        // Event to provide information the user entered when the user dismisses the view.
        public event EventHandler<OAuthPropsSavedEventArgs> OnOAuthPropsInfoEntered;

        // Event to report that the entry was canceled.
        public event EventHandler OnCanceled;

        // Store the input controls so the values can be read.
        private readonly UITextField _clientIdTextField;
        private readonly UITextField _redirectUrlTextField;

        public OAuthPropsDialogOverlay(CGRect frame, nfloat transparency, UIColor color, string clientId, string redirectUrl) : base(frame)
        {
            // Create a semi-transparent overlay with the specified background color.
            BackgroundColor = color;
            Alpha = transparency;

            // Set size and spacing for controls.
            nfloat controlHeight = 25;
            nfloat rowSpace = 11;
            nfloat lessRowSpace = 4;
            nfloat buttonSpace = 15;
            nfloat textViewWidth = Frame.Width - 60;
            nfloat buttonWidth = 60;

            // Find the start x and y for the control layout.
            nfloat controlX = 5;
            nfloat controlY = 5;

            // Label for inputs.
            var description = new UILabel(new CGRect(controlX, controlY, textViewWidth, controlHeight))
            {
                Text = "OAuth settings",
                TextColor = UIColor.Black
            };

            // Adjust the Y position for the next control.
            controlY = controlY + controlHeight + rowSpace;

            // Client ID text input and label.
            var clientIdLabel = new UILabel(new CGRect(controlX, controlY, textViewWidth, controlHeight))
            {
                Text = "Client ID"
            };

            controlY = controlY + controlHeight + lessRowSpace;

            _clientIdTextField = new UITextField(new CGRect(controlX, controlY, textViewWidth, controlHeight))
            {
                Placeholder = "Client ID",
                Text = clientId,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                BackgroundColor = UIColor.LightGray,
                LeftView = new UIView(new CGRect(0, 0, 5, 20)),
                LeftViewMode = UITextFieldViewMode.Always
            };
            // Allow pressing 'return' to dismiss the keyboard.
            _clientIdTextField.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            // Adjust the Y position for the next control.
            controlY = controlY + controlHeight + rowSpace;

            // Redirect URL text input and label.
            var redirectLabel = new UILabel(new CGRect(controlX, controlY, textViewWidth, controlHeight))
            {
                Text = "Redirect URL"
            };

            controlY = controlY + controlHeight + lessRowSpace;

            _redirectUrlTextField = new UITextField(new CGRect(controlX, controlY, textViewWidth, controlHeight))
            {
                Placeholder = "Redirect URI",
                Text = redirectUrl,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                BackgroundColor = UIColor.LightGray,
                LeftView = new UIView(new CGRect(0, 0, 5, 20)),
                LeftViewMode = UITextFieldViewMode.Always
            };
            // Allow pressing 'return' to dismiss the keyboard.
            _redirectUrlTextField.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            // Adjust the Y position for the next control.
            controlY = controlY + controlHeight + rowSpace;

            // Button to save the values
            UIButton saveButton = new UIButton(new CGRect(controlX, controlY, buttonWidth, controlHeight));
            saveButton.SetTitle("Save", UIControlState.Normal);
            saveButton.SetTitleColor(TintColor, UIControlState.Normal);
            saveButton.TouchUpInside += SaveButtonClick;

            // Adjust the X position for the next control.
            controlX = controlX + buttonWidth + buttonSpace;

            // Button to cancel the save.
            UIButton cancelButton = new UIButton(new CGRect(controlX, controlY, buttonWidth, controlHeight));
            cancelButton.SetTitle("Cancel", UIControlState.Normal);
            cancelButton.SetTitleColor(UIColor.Red, UIControlState.Normal);
            cancelButton.TouchUpInside += (s, e) => { OnCanceled.Invoke(this, null); };

            // Add the controls.
            AddSubviews(description, clientIdLabel, _clientIdTextField, redirectLabel, _redirectUrlTextField, saveButton, cancelButton);
        }

        // Animate increasing transparency to completely hide the view, then remove it.
        public void Hide()
        {
            // Action to make the view transparent.
            Action makeTransparentAction = () => Alpha = 0;

            // Action to remove the view.
            Action removeViewAction = RemoveFromSuperview;

            // Time to complete the animation (seconds).
            double secondsToComplete = 0.75;

            // Animate transparency to zero, then remove the view.
            Animate(secondsToComplete, makeTransparentAction, removeViewAction);
        }

        private void SaveButtonClick(object sender, EventArgs e)
        {
            // Get the values entered in the text fields.
            string clientId = _clientIdTextField.Text.Trim();
            string redirectUrl = _redirectUrlTextField.Text.Trim();

            // Make sure all required info was entered.
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUrl))
            {
                var alert = new UIAlertView("Error", "Please enter a client ID and redirect URL for OAuth authentication.", (IUIAlertViewDelegate) null, "OK", null);
                alert.Show();
                return;
            }

            // Fire the OnOAuthPropsInfoEntered event and provide the map item values.
            if (OnOAuthPropsInfoEntered != null)
            {
                // Create a new OAuthPropsSavedEventArgs to contain the user's values.
                var oauthSaveEventArgs = new OAuthPropsSavedEventArgs(clientId, redirectUrl);

                // Raise the event
                OnOAuthPropsInfoEntered(sender, oauthSaveEventArgs);
            }
        }
    }

    // Custom EventArgs implementation to hold OAuth information (client Id and redirect URL).
    public class OAuthPropsSavedEventArgs : EventArgs
    {
        public string ClientId { get; }
        public string RedirectUrl { get; }

        // Store map item values passed into the constructor
        public OAuthPropsSavedEventArgs(string clientId, string redirectUrl)
        {
            ClientId = clientId;
            RedirectUrl = redirectUrl;
        }
    }

    #endregion UI for entering OAuth configuration settings

    #region UI for entering web map search text

    // View containing "search map" controls (search text input and search/cancel buttons).
    public class SearchMapsDialogOverlay : UIView
    {
        // Event to provide information the user entered when the user dismisses the view.
        public event EventHandler<SearchMapsEventArgs> OnSearchMapsTextEntered;

        // Event to report that the search was canceled.
        public event EventHandler OnCanceled;

        // Store the input control so the value can be read.
        private readonly UITextField _searchTextField;

        public SearchMapsDialogOverlay(CGRect frame, nfloat transparency, UIColor color) : base(frame)
        {
            // Create a semi-transparent overlay with the specified background color.
            BackgroundColor = color;
            Alpha = transparency;

            // Set size and spacing for controls.
            nfloat controlHeight = 25;
            nfloat rowSpace = 11;
            nfloat buttonSpace = 15;
            nfloat textViewWidth = Frame.Width - 60;
            nfloat buttonWidth = 60;

            // Find the start x and y for the control layout.
            nfloat controlX = 5;
            nfloat controlY = 5;

            // Label for inputs.
            var description = new UILabel(new CGRect(controlX, controlY, textViewWidth, controlHeight))
            {
                Text = "Search web maps",
                TextColor = UIColor.Black
            };

            // Adjust the Y position for the next control.
            controlY = controlY + controlHeight + rowSpace;

            // Title text input.
            _searchTextField = new UITextField(new CGRect(controlX, controlY, textViewWidth, controlHeight))
            {
                Placeholder = "Search text",
                AutocapitalizationType = UITextAutocapitalizationType.None,
                BackgroundColor = UIColor.LightGray,
                LeftView = new UIView(new CGRect(0, 0, 5, 20)),
                LeftViewMode = UITextFieldViewMode.Always
            };

            // Allow pressing 'return' to dismiss the keyboard.
            _searchTextField.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            // Hide the keyboard when "Enter" is clicked.
            _searchTextField.ShouldReturn += input =>
            {
                input.ResignFirstResponder();
                return true;
            };

            // Adjust the Y position for the next control.
            controlY = controlY + controlHeight + rowSpace;

            // Button to pass the text to the search.
            UIButton saveButton = new UIButton(new CGRect(controlX, controlY, buttonWidth, controlHeight));
            saveButton.SetTitle("Search", UIControlState.Normal);
            saveButton.SetTitleColor(TintColor, UIControlState.Normal);
            saveButton.TouchUpInside += SearchButtonClick;

            // Adjust the X position for the next control (space between buttons).
            controlX = controlX + buttonWidth + buttonSpace;

            // Button to cancel the search.
            UIButton cancelButton = new UIButton(new CGRect(controlX, controlY, buttonWidth, controlHeight));
            cancelButton.SetTitle("Cancel", UIControlState.Normal);
            cancelButton.SetTitleColor(UIColor.Red, UIControlState.Normal);
            cancelButton.TouchUpInside += (s, e) => { OnCanceled.Invoke(this, null); };

            // Add the controls.
            AddSubviews(description, _searchTextField, saveButton, cancelButton);
        }

        // Animate increasing transparency to completely hide the view, then remove it.
        public void Hide()
        {
            // Action to make the view transparent.
            Action makeTransparentAction = () => Alpha = 0;

            // Action to remove the view.
            Action removeViewAction = RemoveFromSuperview;

            // Time to complete the animation (seconds).
            double secondsToComplete = 0.75;

            // Animate transparency to zero, then remove the view.
            Animate(secondsToComplete, makeTransparentAction, removeViewAction);
        }

        private void SearchButtonClick(object sender, EventArgs e)
        {
            // Get the search text entered.
            string searchText = _searchTextField.Text.Trim();

            // Fire the OnMapInfoEntered event and provide the map item values.
            if (OnSearchMapsTextEntered != null)
            {
                // Create a new MapSavedEventArgs to contain the user's values.
                var mapSaveEventArgs = new SearchMapsEventArgs(searchText);

                // Raise the event.
                OnSearchMapsTextEntered(sender, mapSaveEventArgs);
            }
        }
    }

    // Custom EventArgs implementation to hold web map search text.
    public class SearchMapsEventArgs : EventArgs
    {
        public string SearchText { get; }

        // Store map item values passed into the constructor.
        public SearchMapsEventArgs(string searchText)
        {
            SearchText = searchText;
        }
    }

    #endregion UI for entering web map search text
}