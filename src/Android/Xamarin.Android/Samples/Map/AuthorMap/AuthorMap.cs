// Copyright 2016 Esri.
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
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Auth;

namespace ArcGISRuntime.Samples.AuthorMap
{
    [Activity(Label = "AuthorMap")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Author and save a map",
        "Map",
        "This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). Saving a map to arcgis.com requires an ArcGIS Online login.",
        "1. Pan and zoom to the extent you would like for your map. \n2. Choose a basemap from the list of available basemaps. \n3. Choose one or more operational layers to include. \n4. Click 'Save ...' to apply your changes. \n5. Provide info for the new portal item, such as a Title, Description, and Tags. \n6. Click 'Save Map'. \n7. After successfully logging in to your ArcGIS Online account, the map will be saved to your default folder. \n8. You can make additional changes, update the map, and then re-save to store changes in the portal item.")]
    public class AuthorMap : Activity, IOAuthAuthorizeHandler
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Progress bar to show when the app is working
        ProgressBar _progressBar;

        // Use a TaskCompletionSource to track the completion of the authorization
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;

        // Store the OAuth dialog and controls for updating OAuth configuration
        private AlertDialog _configOAuthDialog = null;
        private EditText _clientIdText;
        private EditText _redirectUrlText;

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

        // Variables for OAuth-related values ...
        // URL of the server to authenticate with
        private string ServerUrl = "https://www.arcgis.com/sharing/rest";

        // TODO: Add Client ID for an app registered with the server
        private string AppClientId = "2Gh53JRzkPtOENQq";

        // TODO: Add URL for redirecting after a successful authorization
        //       Note - this must be a URL configured as a valid Redirect URI with your app
        private string OAuthRedirectUrl = "https://developers.arcgis.com";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Author and save a map";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();

            // Set up AuthenticationManager (prompt user for OAuth config first)
            ShowOAuthConfigDialog();
            // Note: the code above calls UpdateAuthenticationManager()

            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateLightGrayCanvas());

            // Provide used Map to the MapView
            _myMapView.Map = myMap;
        }

        private void CreateLayout()
        {
            // Create a horizontal layout for the buttons at the top
            var buttonLayout = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Create a progress bar (circle) to show when the app is working
            _progressBar = new ProgressBar(this);
            _progressBar.Indeterminate = true;
            _progressBar.Visibility = ViewStates.Invisible;

            // Create button to clear the map from the map view (start over)
            var newMapButton = new Button(this);
            newMapButton.Text = "New";
            newMapButton.Click += OnNewMapClicked;

            // Create button to show available basemap
            var basemapButton = new Button(this);
            basemapButton.Text = "Basemap";
            basemapButton.Click += OnBasemapsClicked;

            // Create a button to show operational layers
            var layersButton = new Button(this);
            layersButton.Text = "Layers";
            layersButton.Click += OnLayersClicked;

            // Create a button to save the map
            var saveMapButton = new Button(this);
            saveMapButton.Text = "Save ...";
            saveMapButton.Click += OnSaveMapClicked;

            // Add progress bar, new map, basemap, layers, and save buttons to the layout
            buttonLayout.AddView(newMapButton);
            buttonLayout.AddView(basemapButton);
            buttonLayout.AddView(layersButton);
            buttonLayout.AddView(saveMapButton);
            buttonLayout.AddView(_progressBar);

            // Create a new vertical layout for the app (buttons followed by map view)
            var mainLayout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the button layout
            mainLayout.AddView(buttonLayout);

            // Add the map view to the layout
            mainLayout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(mainLayout);
        }

        private void OnNewMapClicked(object sender, EventArgs e)
        {
            // Create a new map for the map view (can make changes and save as a new portal item)
            _myMapView.Map = new Map(Basemap.CreateLightGrayCanvas());
        }

        private void OnSaveMapClicked(object sender, EventArgs e)
        {
            // Create a dialog to show save options (title, description, and tags)
            SaveDialogFragment saveMapDialog = new SaveDialogFragment(_myMapView.Map.Item as PortalItem);
            saveMapDialog.OnSaveClicked += SaveMapAsync;

            // Begin a transaction to show a UI fragment (the save dialog)
            FragmentTransaction trans = FragmentManager.BeginTransaction();
            saveMapDialog.Show(trans, "save map");
        }

        private async void SaveMapAsync(object sender, OnSaveMapEventArgs e)
        {
            var alertBuilder = new AlertDialog.Builder(this);

            // Get the current map
            var myMap = _myMapView.Map;

            try
            {
                // Show the progress bar so the user knows work is happening
                _progressBar.Visibility = ViewStates.Visible;

                // Get information entered by the user for the new portal item properties
                var title = e.MapTitle;
                var description = e.MapDescription;
                var tags = e.Tags;

                // Apply the current extent as the map's initial extent
                myMap.InitialViewpoint = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

                // Export the current map view for the item's thumbnail
                RuntimeImage thumbnailImg = await _myMapView.ExportImageAsync();

                // See if the map has already been saved (has an associated portal item)
                if (myMap.Item == null)
                {
                    // Call a function to save the map as a new portal item
                    await SaveNewMapAsync(myMap, title, description, tags, thumbnailImg);

                    // Report a successful save
                    alertBuilder.SetTitle("Map Saved");
                    alertBuilder.SetMessage("Saved '" + title + "' to ArcGIS Online!");
                    alertBuilder.Show();
                }
                else
                {
                    // This is not the initial save, call SaveAsync to save changes to the existing portal item
                    await myMap.SaveAsync();
                    
                    // Get the file stream from the new thumbnail image
                    Stream imageStream = await thumbnailImg.GetEncodedBufferAsync();

                    // Update the item thumbnail
                    (myMap.Item as PortalItem).SetThumbnailWithImage(imageStream);
                    await myMap.SaveAsync();

                    // Report update was successful
                    alertBuilder.SetTitle("Updates Saved");
                    alertBuilder.SetMessage("Saved changes to '" + myMap.Item.Title + "'");
                    alertBuilder.Show();
                }
            }
            catch (Exception ex)
            {
                // Show the exception message 
                alertBuilder.SetTitle("Unable to save map");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }
            finally
            {
                // Hide the progress bar
                _progressBar.Visibility = ViewStates.Invisible;
            }
        }

        private async Task SaveNewMapAsync(Map myMap, string title, string description, string[] tags, RuntimeImage img)
        {
            // Challenge the user for portal credentials (OAuth credential request for arcgis.com)
            CredentialRequestInfo loginInfo = new CredentialRequestInfo();

            // Use the OAuth implicit grant flow
            loginInfo.GenerateTokenOptions = new GenerateTokenOptions
            {
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
            };

            // Indicate the url (portal) to authenticate with (ArcGIS Online)
            loginInfo.ServiceUri = new Uri("https://www.arcgis.com/sharing/rest");

            try
            {
                // Get a reference to the (singleton) AuthenticationManager for the app
                AuthenticationManager thisAuthenticationManager = AuthenticationManager.Current;

                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                await thisAuthenticationManager.GetCredentialAsync(loginInfo, false);
            }
            catch (System.OperationCanceledException)
            {
                // user canceled the login
                throw new Exception("Portal log in was canceled.");
            }

            // Get the ArcGIS Online portal (will use credential from login above)
            ArcGISPortal agsOnline = await ArcGISPortal.CreateAsync();

            // Save the current state of the map as a portal item in the user's default folder
            await myMap.SaveAsAsync(agsOnline, null, title, description, tags, img);
        }

        #region Basemap Button
        private void OnBasemapsClicked(object sender, EventArgs e)
        {
            var mapsButton = sender as Button;

            // Create a menu to show basemaps
            var mapsMenu = new PopupMenu(mapsButton.Context, mapsButton);
            mapsMenu.MenuItemClick += OnBasemapsMenuItemClicked;
            
            // Create a menu option for each basemap type
            foreach (var basemapType in _basemapTypes)
            {
                mapsMenu.Menu.Add(basemapType);
            }

            // Show menu in the view
            mapsMenu.Show();
        }

        private void OnBasemapsMenuItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Get the title of the selected item
            var selectedBasemapType = e.Item.TitleCondensedFormatted.ToString();

            // Apply the chosen basemap
            switch (selectedBasemapType)
            {
                case "Topographic":
                    // Set the basemap to Topographic
                    _myMapView.Map.Basemap = Basemap.CreateTopographic();
                    break;
                case "Streets":
                    // Set the basemap to Streets
                    _myMapView.Map.Basemap = Basemap.CreateStreets();
                    break;
                case "Imagery":
                    // Set the basemap to Imagery
                    _myMapView.Map.Basemap = Basemap.CreateImagery();
                    break;
                case "Oceans":
                    // Set the basemap to Oceans
                    _myMapView.Map.Basemap = Basemap.CreateOceans();
                    break;
            }
        }
        #endregion

        #region Layers Button
        private void OnLayersClicked(object sender, EventArgs e)
        {
            var layerButton = sender as Button;

            // Create menu to show layers
            var layerMenu = new PopupMenu(layerButton.Context, layerButton);
            layerMenu.MenuItemClick += OnLayerMenuItemClicked;

            // Create menu options
            foreach (var layerInfo in _operationalLayerUrls)
            {
                layerMenu.Menu.Add(layerInfo.Key);
            }

            // Show menu in the view
            layerMenu.Show();
        }

        private void OnLayerMenuItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Get the title of the selected item
            var selectedLayerName = e.Item.TitleCondensedFormatted.ToString();

            // See if the layer already exists
            ArcGISMapImageLayer layer = _myMapView.Map.OperationalLayers.FirstOrDefault(l => l.Name == selectedLayerName) as ArcGISMapImageLayer;

            // If the layer is in the map, remove it
            if (layer != null)
            {
                _myMapView.Map.OperationalLayers.Remove(layer);
            }
            else
            {
                // Get the URL for this layer
                var layerUrl = _operationalLayerUrls[selectedLayerName];
                var layerUri = new Uri(layerUrl);

                // Create a new map image layer
                layer = new ArcGISMapImageLayer(layerUri);
                layer.Name = selectedLayerName;

                // Set it 50% opaque, and add it to the map
                layer.Opacity = 0.5;
                _myMapView.Map.OperationalLayers.Add(layer);
            }
        }
        #endregion

        #region OAuth helpers

        // Prompt for portal item information 
        private void ShowOAuthConfigDialog()
        {
            // Create a dialog to get OAuth information (client id, redirect url, etc.)
            AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);

            // Create the layout
            LinearLayout dialogLayout = new LinearLayout(this);
            dialogLayout.Orientation = Orientation.Vertical;

            // Create a text box for entering the client id
            LinearLayout clientIdLayout = new LinearLayout(this);
            clientIdLayout.Orientation = Orientation.Horizontal;
            var clientIdLabel = new TextView(this);
            clientIdLabel.Text = "Client ID:";
            _clientIdText = new EditText(this);
            if (!string.IsNullOrEmpty(AppClientId)) { _clientIdText.Text = AppClientId; }
            clientIdLayout.AddView(clientIdLabel);
            clientIdLayout.AddView(_clientIdText);

            // Create a text box for entering the redirect url
            LinearLayout redirectUrlLayout = new LinearLayout(this);
            redirectUrlLayout.Orientation = Orientation.Horizontal;
            var redirectUrlLabel = new TextView(this);
            redirectUrlLabel.Text = "Redirect:";
            _redirectUrlText = new EditText(this);
            _redirectUrlText.Hint = "https://my.redirect/url";
            if (!string.IsNullOrEmpty(OAuthRedirectUrl)) { _redirectUrlText.Text = OAuthRedirectUrl; }
            redirectUrlLayout.AddView(redirectUrlLabel);
            redirectUrlLayout.AddView(_redirectUrlText);

            // Create a button to dismiss the dialog (and proceed with updating the values)
            Button okButton = new Button(this);
            okButton.Text = "Save";

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
                AppClientId = _clientIdText.Text;
                OAuthRedirectUrl = _redirectUrlText.Text;

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
                    ClientId = AppClientId,
                    RedirectUri = new Uri(OAuthRedirectUrl)
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
            var intent = authenticator.GetUI(this);
            StartActivityForResult(intent, 99);

            // Return completion source task so the caller can await completion
            return _taskCompletionSource.Task;
        }

        private static IDictionary<string, string> DecodeParameters(Uri uri)
        {
            // Create a dictionary of key value pairs returned in an OAuth authorization response URI query string
            var answer = string.Empty;

            // Get the values from the URI fragment or query string
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

            // Parse parameters into key / value pairs
            var keyValueDictionary = new Dictionary<string, string>();
            var keysAndValues = answer.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var kvString in keysAndValues)
            {
                var pair = kvString.Split('=');
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

    // A custom DialogFragment class to show input controls for saving a web map
    public class SaveDialogFragment : DialogFragment
    {
        // Inputs for portal item title, description, and tags
        private EditText _mapTitleTextbox;
        private EditText _mapDescriptionTextbox;
        private EditText _tagsTextbox;

        // Store any existing portal item (for "update" versus "save", e.g.)
        private PortalItem _portalItem = null;

        // Raise an event so the listener can access input values when the form has been completed
        public event EventHandler<OnSaveMapEventArgs> OnSaveClicked;

        public SaveDialogFragment(PortalItem mapItem)
        {
            _portalItem = mapItem;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Dialog to display
            LinearLayout dialogView = null;

            // Get the context for creating the dialog controls
            Android.Content.Context ctx = Activity.ApplicationContext;

            // Set a dialog title
            Dialog.SetTitle("Save Map to Portal");

            try
            {
                base.OnCreateView(inflater, container, savedInstanceState);

                // The container for the dialog is a vertical linear layout
                dialogView = new LinearLayout(ctx);
                dialogView.Orientation = Orientation.Vertical;

                // Add a text box for entering a title for the new web map
                _mapTitleTextbox = new EditText(ctx);
                _mapTitleTextbox.Hint = "Title";
                dialogView.AddView(_mapTitleTextbox);

                // Add a text box for entering a description
                _mapDescriptionTextbox = new EditText(ctx);
                _mapDescriptionTextbox.Hint = "Description";
                dialogView.AddView(_mapDescriptionTextbox);

                // Add a text box for entering tags (populate with some values so the user doesn't have to fill this in)
                _tagsTextbox = new EditText(ctx);
                _tagsTextbox.Text = "ArcGIS Runtime, Web Map";
                dialogView.AddView(_tagsTextbox);

                // Add a button to save the map
                Button saveMapButton = new Button(ctx);
                saveMapButton.Text = "Save";
                saveMapButton.Click += SaveMapButtonClick;
                dialogView.AddView(saveMapButton);

                // If there's an existing portal item, configure the dialog for "update" (read-only entries)
                if (_portalItem != null)
                {
                    _mapTitleTextbox.Text = _portalItem.Title;
                    _mapTitleTextbox.Enabled = false;

                    _mapDescriptionTextbox.Text = _portalItem.Description;
                    _mapDescriptionTextbox.Enabled = false;

                    _tagsTextbox.Text = string.Join(",", _portalItem.Tags);
                    _tagsTextbox.Enabled = false;

                    // Change some of the control text
                    Dialog.SetTitle("Save Changes to Map");
                    saveMapButton.Text = "Update";
                }
            }
            catch (Exception ex)
            {
                // Show the exception message 
                var alertBuilder = new AlertDialog.Builder(Activity);
                alertBuilder.SetTitle("Error");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }

            // Return the new view for display
            return dialogView;
        }

        // A click handler for the save map button
        private void SaveMapButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Get information for the new portal item
                var title = _mapTitleTextbox.Text;
                var description = _mapDescriptionTextbox.Text;
                var tags = _tagsTextbox.Text.Split(',');

                // Make sure all required info was entered
                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) || tags.Length == 0)
                {
                    throw new Exception("Please enter a title, description, and some tags to describe the map.");
                }

                // Create a new OnSaveMapEventArgs object to store the information entered by the user
                var mapSavedArgs = new OnSaveMapEventArgs(title, description, tags);

                // Raise the OnSaveClicked event so the main activity can handle the event and save the map
                OnSaveClicked(this, mapSavedArgs);

                // Close the dialog
                Dismiss();
            }
            catch (Exception ex)
            {
                // Show the exception message (dialog will stay open so user can try again)
                var alertBuilder = new AlertDialog.Builder(Activity);
                alertBuilder.SetTitle("Error");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }
        }
    }

    // Custom EventArgs class for containing portal item properties when saving a map
    public class OnSaveMapEventArgs : EventArgs
    {
        // Portal item title
        public string MapTitle { get; set; }

        // Portal item description
        public string MapDescription { get; set; }

        // Portal item tags
        public string[] Tags { get; set; }

        public OnSaveMapEventArgs(string title, string description, string[] tags) : base()
        {
            MapTitle = title;
            MapDescription = description;
            Tags = tags;
        }
    }
}