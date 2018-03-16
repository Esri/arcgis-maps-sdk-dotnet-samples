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
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ArcGISRuntime.Samples.AuthorEditSaveMap
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Author, edit, and save a map",
        "Tutorial",
        "This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). It is also the solution to the [Author, edit, and save maps to your portal tutorial](https://developers.arcgis.com/net/latest/android/guide/author-edit-and-save-maps-to-your-portal.htm). Saving a map to arcgis.com requires an ArcGIS Online login.",
        "1. Pan and zoom to the extent you would like for your map.\n2. Choose a basemap from the list of available basemaps.\n3. Click 'Save ...' and provide info for the new portal item (Title, Description, and Tags).\n4. Click 'Save Map to Portal'.\n5. After successfully logging in to your ArcGIS Online account, the map will be saved to your default folder.\n6. You can make additional changes, update the map, and then re-save to store changes in the portal item.")]
    public class AuthorEditSaveMap : Activity, IOAuthAuthorizeHandler
    {
        // Store the app's map view
        MapView _mapView = new MapView();

        // Store the view model used by the app to display the map
        MapViewModel _mapViewModel;

        // Use a TaskCompletionSource to track the completion of the authorization
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;

        // OAuth-related values ...
        // URL of the server to authenticate with (ArcGIS Online)
        private const string ArcGISOnlineUrl = "https://www.arcgis.com/sharing/rest";

        // Client ID for the app registered with the server (Portal Maps)
        private const string AppClientId = "2Gh53JRzkPtOENQq";

        // Redirect URL after a successful authorization (configured for the Portal Maps application)
        private const string OAuthRedirectUrl = "https://developers.arcgis.com";

        // Store the save dialog and controls for entering the portal item title and description
        private AlertDialog _saveDialog = null;
        private EditText _titleText;
        private EditText _descriptionText;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // (This line is not in the tutorial) Display the name of the sample in the viewer
            Title = "Author and save a map";

            // Create the view model (pass in the app's map view)
            _mapViewModel = new MapViewModel(_mapView);

            // Create the UI
            CreateLayout();

            // Set up AuthenticationManager
            UpdateAuthenticationManager();

            // Assign map from view model Map property
            _mapView.Map = _mapViewModel.Map;

            // Listen for changes on the view model
            _mapViewModel.PropertyChanged += MapViewModel_PropertyChanged;
        }

        private void MapViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Update the map view with the view model's new map
            if (e.PropertyName == "Map" && _mapView != null)
                _mapView.Map = _mapViewModel.Map;
        }

        private void CreateLayout()
        {
            // Create a horizontal layout for the buttons at the top
            var buttonLayout = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Create button to clear the map from the map view (start over)
            var newMapButton = new Button(this);
            newMapButton.Text = "New Map";
            newMapButton.Click += OnNewMapClicked;

            // Create button to show available basemap
            var basemapButton = new Button(this);
            basemapButton.Text = "Basemap";
            basemapButton.Click += OnBasemapsClicked;

            // Create a button to save the map
            var saveMapButton = new Button(this);
            saveMapButton.Text = "Save Map ...";
            saveMapButton.Click += OnSaveMapClicked;

            // Add new map, basemap, layers, and save buttons to the layout
            buttonLayout.AddView(newMapButton);
            buttonLayout.AddView(basemapButton);
            buttonLayout.AddView(saveMapButton);

            // Create a new vertical layout for the app (buttons followed by map view)
            var mainLayout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the button layout
            mainLayout.AddView(buttonLayout);

            // Add the map view to the layout
            mainLayout.AddView(_mapView);

            // Show the layout in the app
            SetContentView(mainLayout);
        }

        #region Basemap Button
        private void OnBasemapsClicked(object sender, EventArgs e)
        {
            // Get the button that raised the click event (change basemap)
            var mapsButton = sender as Button;

            // Create a menu to show basemap choices
            var mapsMenu = new PopupMenu(mapsButton.Context, mapsButton);

            // Handle the menu item click event
            mapsMenu.MenuItemClick += OnBasemapsMenuItemClicked;

            // Create a menu option for each basemap type defined in the view model
            foreach (var choice in _mapViewModel.BasemapChoices)
            {
                mapsMenu.Menu.Add(choice);
            }

            // Show menu in the view
            mapsMenu.Show();
        }

        // Click handler for basemap menu items
        private void OnBasemapsMenuItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Get the title of the selected item
            var selectedBasemapType = e.Item.TitleCondensedFormatted.ToString();

            // Pass the selected basemap name to the view model ChangeBasemap method
            _mapViewModel.ChangeBasemap(selectedBasemapType);
        }
        #endregion

        #region Save Map Button
        private async void OnSaveMapClicked(object sender, EventArgs e)
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

                try 
                {
                    // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                    await AuthenticationManager.Current.GetCredentialAsync(challengeRequest, false);
                }
                catch (System.OperationCanceledException)
                {
                    // user canceled the login
                    throw new Exception("Portal log in was canceled.");
                }

                // See if the map has already been saved
                if (!_mapViewModel.MapIsSaved)
                {
                    // Map has not been saved ... save to portal for the first time
                    ShowSaveMapDialog();
                }
                else
                {
                    // Map has previously been saved as a portal item, update it (title and description will remain the same)
                    _mapViewModel.UpdateMapItem();
                    
                    // Report a successful update
                    AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);
                    dialogBuilder.SetTitle("Map Updated!");
                    dialogBuilder.SetMessage("Changes to map ('" + _mapViewModel.Map.Item.Title + "' were saved to ArcGIS Online");
                    dialogBuilder.Show();
                }
            }
            catch (System.OperationCanceledException)
            {
                // user canceled the login                
            }
        }

        // Prompt for portal item information 
        private void ShowSaveMapDialog()
        {
            // Create a dialog to get map information (title and description)
            AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);

            // Create the layout
            LinearLayout dialogLayout = new LinearLayout(this);
            dialogLayout.Orientation = Orientation.Vertical;

            // Create a text box for entering the map title
            _titleText = new EditText(this);
            _titleText.Hint = "Webmap Title";

            // Create a text box for entering the map description
            _descriptionText = new EditText(this);
            _descriptionText.Hint = "Description";

            // Create a button to dismiss the dialog (and proceed with saving the map)
            Button okButton = new Button(this);
            okButton.Text = "Save Map";

            // Handle the click event for the OK button
            okButton.Click += OnCloseSaveDialog;

            // Add the controls to the dialog
            dialogLayout.AddView(_titleText);
            dialogLayout.AddView(_descriptionText);
            dialogLayout.AddView(okButton);
            dialogBuilder.SetView(dialogLayout);
            dialogBuilder.SetTitle("Save Map");

            // Show the dialog
            _saveDialog = dialogBuilder.Show();
        }

        // Click event for the OK button on the save map dialog
        private async void OnCloseSaveDialog(object sender, EventArgs e)
        {
            if (_saveDialog != null)
            {
                // Get title and description text
                var title = _titleText.Text;
                var description = _descriptionText.Text;

                // Dismiss the dialog
                _saveDialog.Dismiss();

                // Return if the text is null or empty for either
                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
                {
                    return;
                }

                // Get the map view's current extent (viewpoint) to use as the web map initial extent
                Viewpoint initialMapViewpoint = _mapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

                // Export the current map view for the item's thumbnail
                RuntimeImage thumbnailImg = await _mapView.ExportImageAsync();

                // Provide some default tags for the item
                var tags = new[] { "ArcGIS Runtime SDK", "tutorial" };

                try
                {
                    // Call a function on the view model to save the map as a new portal item
                    await _mapViewModel.SaveNewMapAsync(initialMapViewpoint, title, description, tags, thumbnailImg);

                    // Report a successful save
                    AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);
                    dialogBuilder.SetTitle("Map Saved!");
                    dialogBuilder.SetMessage("Your map ('" + title + "' was saved to ArcGIS Online");
                    dialogBuilder.Show();
                }
                catch (Exception ex)
                {
                    // Show the exception message
                    AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);
                    dialogBuilder.SetTitle("Error");
                    dialogBuilder.SetMessage("Unable to save: " + ex.Message);
                    dialogBuilder.Show();
                }
            }
        }
        #endregion

        #region New Map Button
        private void OnNewMapClicked(object sender, EventArgs e)
        {
            _mapViewModel.ResetMap();
        }
        #endregion

        #region OAuth
        private void UpdateAuthenticationManager()
        {
            // Register the server information with the AuthenticationManager
            ServerInfo portalServerInfo = new ServerInfo
            {
                ServerUri = new Uri(ArcGISOnlineUrl),
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
            Xamarin.Auth.OAuth2Authenticator authenticator = new Xamarin.Auth.OAuth2Authenticator(
                clientId: AppClientId,
                scope: "",
                authorizeUrl: authorizeUri,
                redirectUrl: callbackUri)
            { ShowErrors = false };

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
                    // Dismiss the OAuth login
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
                    // Login canceled: dismiss the OAuth login
                    if (_taskCompletionSource != null)
                    {
                        _taskCompletionSource.TrySetCanceled();
                        FinishActivity(99);
                    }
                }

                // Cancel authentication
                authenticator.OnCancelled();
            };

            // Present the OAuth UI so the user can enter user name and password
            var intent = authenticator.GetUI(this);
            StartActivityForResult(intent, 99);
            // Return completion source task so the caller can await completion
            return _taskCompletionSource.Task;
        }
        #endregion
    }

    // The ViewModel class used by the View (AuthorEditSaveMap)
    // Note: When creating an app from the ArcGIS Runtime SDK for .NET project template,
    //       this class will be in a separate file (MapViewModel.cs)
    public class MapViewModel : INotifyPropertyChanged
    {
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

        // Fields to store the current map view and map
        private MapView _mapView;
        private Map _map = new Map(Basemap.CreateTopographicVector());
        
        // Gets or sets the map
        public Map Map
        {
            get { return _map; }
            set { _map = value; OnPropertyChanged(); }
        }

        public MapViewModel(MapView mapView)
        {
            // Store the app's map view when the view model is created
            _mapView = mapView;
        }

        public bool MapIsSaved
        {
            // Return True if the current map has a value for the Item property
            get { return (_map != null && _map.Item != null); }
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
            await _map.SaveAsAsync(agsOnline, null, title, description, tags, img);
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

            // Create a new map with light gray canvas basemap
            Map newMap = new Map(Basemap.CreateLightGrayCanvasVector());

            // Store the new map 
            Map = newMap;
        }
        
        // Raises the PropertyChanged event for a property
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}