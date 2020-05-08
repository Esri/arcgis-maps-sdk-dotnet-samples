// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esri.ArcGISRuntime;
using Xamarin.Auth;
using ContextThemeWrapper = Android.Support.V7.View.ContextThemeWrapper;

namespace ArcGISRuntime.Samples.SearchPortalMaps
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Search for webmap",
        category: "Map",
        description: "Find webmap portal items by using a search term.",
        instructions: "Enter search terms into the search bar. Once the search is complete, a list is populated with the resultant webmaps. Tap on a webmap to set it to the map view. Scrolling to the bottom of the webmap recycler view will get more results.",
        tags: new[] { "keyword", "query", "search", "webmap" })]
    public class SearchPortalMaps : Activity, IOAuthAuthorizeHandler
    {
        // Hold a reference to the map view
        private MapView _myMapView;

        // Dictionary to hold URIs to web maps
        private Dictionary<string, Uri> _webMapUris;

        // Use a TaskCompletionSource to track the completion of the authorization
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;

        // Store the OAuth dialog and controls for updating OAuth configuration
        private AlertDialog _configOAuthDialog = null;
        private EditText _clientIdText;
        private EditText _redirectUrlText;

        // Variables for OAuth config and default values ...
        // URL of the server to authenticate with
        private const string ServerUrl = "https://www.arcgis.com/sharing/rest";

        // TODO: Add Client ID for an app registered with the server
        private string _appClientId = "2Gh53JRzkPtOENQq";

        // TODO: Add URL for redirecting after a successful authorization
        //       Note - this must be a URL configured as a valid Redirect URI with your app
        private string _oAuthRedirectUrl = "https://developers.arcgis.com";

        // Button layout at the top of the page
        private LinearLayout _buttonPanel;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Search a portal for maps";

            // Set up AuthenticationManager (prompt user for OAuth config first)
            ShowOAuthConfigDialog();
            // Note: the code above calls UpdateAuthenticationManager() to store OAuth configuration

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create a new Map instance
            Map myMap = new Map(Basemap.CreateLightGrayCanvas());

            // Provide default Map to the MapView
            _myMapView.Map = myMap;
        }

        private void SearchMapsClicked(object sender, EventArgs e)
        {
            // Create a dialog to show save options (title, description, and tags)
            SearchMapsDialogFragment searchMapsDialog = new SearchMapsDialogFragment();
            searchMapsDialog.OnSearchClicked += OnSearchMapsClicked;

            // Begin a transaction to show a UI fragment (the search dialog)
            FragmentTransaction trans = FragmentManager.BeginTransaction();
            searchMapsDialog.Show(trans, "search maps");
        }

        private async void MyMapsClicked(object sender, EventArgs e)
        {
            try
            {
                // Get web map portal items in the current user's folder
                IEnumerable<PortalItem> mapItems = null;

                // Call a sub that will force the user to log in to ArcGIS Online (if they haven't already)
                bool loggedIn = await EnsureLoggedInAsync();
                if (!loggedIn) { return; }

                // Connect to the portal (will connect using the provided credentials)
                ArcGISPortal portal = await ArcGISPortal.CreateAsync(new Uri(ServerUrl));

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

                // Show the map results
                ShowMapList(mapItems);
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }

        private async void OnSearchMapsClicked(object sender, OnSearchMapEventArgs e)
        {
            try
            {
                // Get web map portal items from a keyword search
                IEnumerable<PortalItem> mapItems = null;

                // Connect to the portal (anonymously)
                ArcGISPortal portal = await ArcGISPortal.CreateAsync(new Uri(ServerUrl));

                // Create a query expression that will get public items of type 'web map' with the keyword(s) in the items tags
                string queryExpression = $"tags:\"{e.SearchText}\" access:public type: (\"web map\" NOT \"web mapping application\")";
            
                // Create a query parameters object with the expression and a limit of 10 results
                PortalQueryParameters queryParams = new PortalQueryParameters(queryExpression, 10);

                // Search the portal using the query parameters and await the results
                PortalQueryResultSet<PortalItem> findResult = await portal.FindItemsAsync(queryParams);
            
                // Get the items from the query results
                mapItems = findResult.Results;

                // Show the map results
                ShowMapList(mapItems);
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }

        private void ShowMapList(IEnumerable<PortalItem> webmapItems)
        {
            // Create menu to show map results
            PopupMenu mapsMenu = new PopupMenu(this, _buttonPanel);
            mapsMenu.MenuItemClick += OnMapsMenuItemClicked;

            // Create a dictionary of web maps and show the titles in the menu
            _webMapUris = new Dictionary<string, Uri>();
            foreach (PortalItem item in webmapItems)
            {
                if (!_webMapUris.ContainsKey(item.Title)){
                    _webMapUris.Add(item.Title, item.Url);
                    mapsMenu.Menu.Add(item.Title);
                }
            }

            //Show menu in the view
            mapsMenu.Show();
        }

        private void OnMapsMenuItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Get the selected web map item URI from the dictionary
            string mapTitle = e.Item.TitleCondensedFormatted.ToString();
            Uri selectedMapUri = _webMapUris[mapTitle];

            if (selectedMapUri == null) { return; }

            // Create a new map, pass the web map portal item to the constructor
            Map webMap = new Map(selectedMapUri);

            // Handle change in the load status (to report load errors)
            webMap.LoadStatusChanged += WebMapLoadStatusChanged;

            // Show the web map in the map view
            _myMapView.Map = webMap;
        }

        private void WebMapLoadStatusChanged(object sender, Esri.ArcGISRuntime.LoadStatusEventArgs e)
        {
            // Report errors if map failed to load
            if (e.Status == LoadStatus.FailedToLoad)
            {
                Map map = (Map)sender;
                Exception err = map.LoadError;
                if (err != null)
                {
                    AlertDialog.Builder alertBuilder = new AlertDialog.Builder(this);
                    alertBuilder.SetTitle("Map Load Error");
                    alertBuilder.SetMessage(err.Message);
                    alertBuilder.Show();
                }
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout mainLayout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a layout for app buttons
            _buttonPanel = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Create button to show search UI
            Button searchMapsButton = new Button(this)
            {
                Text = "Search Maps"
            };
            searchMapsButton.Click += SearchMapsClicked;

            // Create another button to show maps from user's ArcGIS Online account
            Button myMapsButton = new Button(this)
            {
                Text = "My Maps"
            };
            myMapsButton.Click += MyMapsClicked;

            // Add buttons to the horizontal layout panel
            _buttonPanel.AddView(searchMapsButton);
            _buttonPanel.AddView(myMapsButton);

            // Add button panel to the main layout
            mainLayout.AddView(_buttonPanel);

            // Add the map view to the layout
            _myMapView = new MapView(this);
            mainLayout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(mainLayout);
        }

        #region OAuth helpers
        // Prompt for portal item information 
        private void ShowOAuthConfigDialog()
        {
            // Create a dialog to get OAuth information (client id, redirect url, etc.)
            AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);

            // Create the layout
            LinearLayout dialogLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };

            // Create a text box for entering the client id
            LinearLayout clientIdLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal
            };
            TextView clientIdLabel = new TextView(this)
            {
                Text = "Client ID:"
            };
            _clientIdText = new EditText(this);
            if (!String.IsNullOrEmpty(_appClientId)) { _clientIdText.Text = _appClientId; }
            clientIdLayout.AddView(clientIdLabel);
            clientIdLayout.AddView(_clientIdText);

            // Create a text box for entering the redirect url
            LinearLayout redirectUrlLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal
            };
            
            TextView redirectUrlLabel = new TextView(this)
            {
                Text = "Redirect:"
            };
            
            _redirectUrlText = new EditText(this)
            {
                Hint = "https://my.redirect/url"
            };

            if (!String.IsNullOrEmpty(_oAuthRedirectUrl))
            {
                _redirectUrlText.Text = _oAuthRedirectUrl;
            }

            redirectUrlLayout.AddView(redirectUrlLabel);
            redirectUrlLayout.AddView(_redirectUrlText);

            // Create a button to dismiss the dialog (and proceed with updating the values)
            Button okButton = new Button(this)
            {
                Text = "Save"
            };

            // Handle the click event for the OK button
            okButton.Click += OnCloseOAuthDialog;

            // Add the controls to the dialog
            dialogLayout.AddView(clientIdLayout);
            dialogLayout.AddView(redirectUrlLayout);
            dialogLayout.AddView(okButton);
            dialogBuilder.SetView(dialogLayout);
            dialogBuilder.SetTitle("Configure OAuth");

            // Show the dialog
            _configOAuthDialog = dialogBuilder.Show();
        }

        // Click event for the OK button on the save map dialog
        private void OnCloseOAuthDialog(object sender, EventArgs e)
        {
            if (_configOAuthDialog != null)
            {
                // Get title and description text
                _appClientId = _clientIdText.Text;
                _oAuthRedirectUrl = _redirectUrlText.Text;

                // Dismiss the dialog
                _configOAuthDialog.Dismiss();

                // Update the OAuth settings
                UpdateAuthenticationManager();
            }
        }

        private void UpdateAuthenticationManager()
        {
            // Register the server information with the AuthenticationManager
            ServerInfo portalServerInfo = new ServerInfo
            {
                ServerUri = new Uri(ServerUrl),
                OAuthClientInfo = new OAuthClientInfo
                {
                    ClientId = _appClientId,
                    RedirectUri = new Uri(_oAuthRedirectUrl)
                },
                // Specify OAuthAuthorizationCode if you need a refresh token (and have specified a valid client secret)
                // Otherwise, use OAuthImplicit
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
            };

            // Get a reference to the (singleton) AuthenticationManager for the app
            AuthenticationManager thisAuthenticationManager = AuthenticationManager.Current;

            // Register the server information
            thisAuthenticationManager.RegisterServer(portalServerInfo);

            // Assign the method that AuthenticationManager will call to challenge for secured resources
            thisAuthenticationManager.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Set the OAuth authorization handler to this class (Implements IOAuthAuthorize interface)
            thisAuthenticationManager.OAuthAuthorizeHandler = this;
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
                    ServiceUri = new Uri(ServerUrl)
                };

                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                Credential cred = await AuthenticationManager.Current.GetCredentialAsync(challengeRequest, false);
                loggedIn = cred != null;
            }
            catch (System.OperationCanceledException)
            {
                // Login was canceled
                // .. ignore, user can still search public maps without logging in
            }
            catch (Exception ex)
            {
                // Login failure
                AlertDialog.Builder alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("Login Error");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }

            return loggedIn;
        }

        // ChallengeHandler function for AuthenticationManager, called whenever access to a secured resource is attempted
        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            OAuthTokenCredential credential = null;

            try
            {
                // Create generate token options if necessary
                if (info.GenerateTokenOptions == null)
                {
                    info.GenerateTokenOptions = new GenerateTokenOptions();
                }

                // AuthenticationManager will handle challenging the user for credentials
                credential = await AuthenticationManager.Current.GenerateCredentialAsync
                    (
                            info.ServiceUri,
                            info.GenerateTokenOptions
                    ) as OAuthTokenCredential;
            }
            catch (TaskCanceledException) { return credential; }
            catch (Exception)
            {
                // Exception will be reported in calling function
                throw;
            }

            return credential;
        }

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

            // Create a new Xamarin.Auth.OAuth2Authenticator using the information passed in
            Xamarin.Auth.OAuth2Authenticator authenticator = new OAuth2Authenticator(
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
                    // Check if the user is authenticated
                    if (authArgs.IsAuthenticated)
                    {
                        // If authorization was successful, get the user's account
                        Xamarin.Auth.Account authenticatedAccount = authArgs.Account;

                        // Set the result (Credential) for the TaskCompletionSource
                        _taskCompletionSource.SetResult(authenticatedAccount.Properties);
                    }
                }
                catch (Exception ex)
                {
                    // If authentication failed, set the exception on the TaskCompletionSource
                    _taskCompletionSource.TrySetException(ex);

                    // Cancel authentication
                    authenticator.OnCancelled();
                }
                finally
                {
                    // End the OAuth login activity
                    FinishActivity(99);
                }
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
                    // Login canceled: end the OAuth login activity
                    if (_taskCompletionSource != null)
                    {
                        _taskCompletionSource.TrySetCanceled();
                        FinishActivity(99);
                    }
                }

                // Cancel authentication
                authenticator.OnCancelled();
            };

            // Present the OAuth UI (Activity) so the user can enter user name and password
            Android.Content.Intent intent = authenticator.GetUI(this);
            StartActivityForResult(intent, 99);

            // Return completion source task so the caller can await completion
            return _taskCompletionSource.Task;
        }

        private static IDictionary<string, string> DecodeParameters(Uri uri)
        {
            // Create a dictionary of key value pairs returned in an OAuth authorization response URI query string
            string answer = "";

            // Get the values from the URI fragment or query string
            if (!String.IsNullOrEmpty(uri.Fragment))
            {
                answer = uri.Fragment.Substring(1);
            }
            else
            {
                if (!String.IsNullOrEmpty(uri.Query))
                {
                    answer = uri.Query.Substring(1);
                }
            }

            // Parse parameters into key / value pairs
            Dictionary<string,string> keyValueDictionary = new Dictionary<string, string>();
            string[] keysAndValues = answer.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
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

            // Return the dictionary of string keys/values
            return keyValueDictionary;
        }
        #endregion
    }

    #region UI for entering web map search text
    // A custom DialogFragment class to show input controls for searching for maps
    public class SearchMapsDialogFragment : DialogFragment
    {
        // Inputs for portal item search text
        private EditText _mapSearchTextbox;

        // Raise an event so the listener can access the input search text value when the form has been completed
        public event EventHandler<OnSearchMapEventArgs> OnSearchClicked;

        public SearchMapsDialogFragment() { }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Dialog to display
            LinearLayout dialogView = null;

            // Get the context for creating the dialog controls
            Android.Content.Context ctx = Activity.ApplicationContext;
            Android.Support.V7.View.ContextThemeWrapper ctxWrapper = new ContextThemeWrapper(ctx, Android.Resource.Style.ThemeMaterialLight);

            // Set a dialog title
            Dialog.SetTitle("Search Portal");

            try
            {
                base.OnCreateView(inflater, container, savedInstanceState);

                // The container for the dialog is a vertical linear layout
                dialogView = new LinearLayout(ctxWrapper)
                {
                    Orientation = Orientation.Vertical
                };

                // Add a text box for entering web map search text
                _mapSearchTextbox = new EditText(ctxWrapper)
                {
                    Hint = "Search text"
                };
                dialogView.AddView(_mapSearchTextbox);

                // Add a button to complete search
                Button searchMapsButton = new Button(ctxWrapper)
                {
                    Text = "Search"
                };
                searchMapsButton.Click += SearchMapsButtonClick;
                dialogView.AddView(searchMapsButton);
            }
            catch (Exception ex)
            {
                // Show the exception message 
                AlertDialog.Builder alertBuilder = new AlertDialog.Builder(Activity);
                alertBuilder.SetTitle("Error");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }

            // Return the new view for display
            return dialogView;
        }

        // A click handler for the search button
        private void SearchMapsButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Get information for the new portal item
                string search = _mapSearchTextbox.Text;

                // Create a new OnSaveMapEventArgs object to store the information entered by the user
                OnSearchMapEventArgs mapSearchArgs = new OnSearchMapEventArgs(search);

                // Raise the OnSaveClicked event so the main activity can handle the event and save the map
                OnSearchClicked?.Invoke(this, mapSearchArgs);

                // Close the dialog
                Dismiss();
            }
            catch (Exception ex)
            {
                // Show the exception message (dialog will stay open so user can try again)
                AlertDialog.Builder alertBuilder = new AlertDialog.Builder(Activity);
                alertBuilder.SetTitle("Error");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }
        }
    }

    // Custom EventArgs class for containing map search expression text
    public class OnSearchMapEventArgs : EventArgs
    {
        // Search text
        public string SearchText { get; set; }

        public OnSearchMapEventArgs(string searchText) : base()
        {
            // Store the web map search text
            SearchText = searchText;
        }
    }
    #endregion
}