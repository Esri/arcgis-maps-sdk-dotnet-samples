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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Auth;

namespace ArcGISRuntime.Samples.AuthorMap
{
    [Register("AuthorMap")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Author and save a map",
        "Map",
        "This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). Saving a map to arcgis.com requires an ArcGIS Online login.",
        "1. Pan and zoom to the extent you would like for your map. \n2. Choose a basemap from the list of available basemaps. \n3. Choose one or more operational layers to include. \n4. Click 'Save ...' to apply your changes. \n5. Provide info for the new portal item, such as a Title, Description, and Tags. \n6. Click 'Save Map'. \n7. After successfully logging in to your ArcGIS Online account, the map will be saved to your default folder. \n8. You can make additional changes, update the map, and then re-save to store changes in the portal item.")]
    public class AuthorMap : UIViewController, IOAuthAuthorizeHandler
    {
        // Reference to the MapView used in the app
        private MapView _myMapView;

        private UISegmentedControl _segmentButton = new UISegmentedControl();
        private UIToolbar _toolbar = new UIToolbar();

        // Dictionary of operational layer names and URLs
        private Dictionary<string, string> _operationalLayerUrls = new Dictionary<string, string>
        {
            {"World Elevations", "https://sampleserver5.arcgisonline.com/arcgis/rest/services/Elevation/WorldElevations/MapServer"},
            {"World Cities", "https://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer/" },
            {"US Census Data", "https://sampleserver5.arcgisonline.com/arcgis/rest/services/Census/MapServer"}
        };

        // Use a TaskCompletionSource to track the completion of the authorization
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;

        // Overlay with entry controls for OAuth configuration (client ID and redirect Url)
        private OAuthPropsDialogOverlay _oauthInfoUI;

        // Overlay with entry controls for map item details (title, description, and tags)
        private SaveMapDialogOverlay _mapInfoUI;

        // Progress bar to show that the app is working
        private UIActivityIndicatorView _activityIndicator;

        // Constants for OAuth-related values ...
        // URL of the server to authenticate with
        private string ServerUrl = "https://www.arcgis.com/sharing/rest";

        // TODO: Add Client ID for an app registered with the server
        private string AppClientId = "2Gh53JRzkPtOENQq";

        // TODO: Add URL for redirecting after a successful authorization
        //       Note - this must be a URL configured as a valid Redirect URI with your app
        private string OAuthRedirectUrl = "https://developers.arcgis.com";

        // URL used by the server for authorization
        private string AuthorizeUrl = "https://www.arcgis.com/sharing/oauth2/authorize";

        public AuthorMap()
        {
            Title = "Author and save a map";
        }

        public override void ViewDidLayoutSubviews()
        {
            // correctly handle re-layout events (e.g. rotated phone)
            base.ViewDidLayoutSubviews();
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            _toolbar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 50, View.Bounds.Width, 50);
            _segmentButton.Frame = new CoreGraphics.CGRect(10, _toolbar.Frame.Top + 10, View.Bounds.Width - 20, _toolbar.Frame.Height - 20);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the layout
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with a light gray canvas basemap
            var myMap = new Map(Basemap.CreateLightGrayCanvas());

            // Add the Map to the MapView
            _myMapView.Map = myMap;

            // Prompt the user for OAuth settings
            ShowOAuthPropsUI();
        }

        private void CreateLayout()
        {
            // Create a new MapView control
            _myMapView = new MapView();

            // Create an activity indicator
            var centerRect = new CoreGraphics.CGRect(View.Bounds.Width / 2, View.Bounds.Height / 2, 40, 40);
            _activityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
            _activityIndicator.Frame = centerRect;

            // Define the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Configure segmented button control
            _segmentButton.BackgroundColor = UIColor.White;
            _segmentButton.InsertSegment("Basemap", 0, false);
            _segmentButton.InsertSegment("Layers", 1, false);
            _segmentButton.InsertSegment("New", 2, false);
            _segmentButton.InsertSegment("Save", 3, false);

            // Handle the "click" for each segment (new segment is selected)
            _segmentButton.ValueChanged += SegmentButtonClicked;

            // Add the MapView, progress bar, and UIButton to the page
            View.AddSubviews(_myMapView, _activityIndicator, _toolbar, _segmentButton);
        }

        private void SegmentButtonClicked(object sender, EventArgs e)
        {
            // Get the segmented button control that raised the event
            var buttonControl = sender as UISegmentedControl;

            // Get the selected segment in the control
            var selectedSegmentId = buttonControl.SelectedSegment;

            // Execute the appropriate action for the control
            if (selectedSegmentId == 0)
            {
                // Show basemap choices
                ShowBasemapList();
            }
            else if (selectedSegmentId == 1)
            {
                // Show a list of available operational layers
                ShowLayerList();
            }
            else if (selectedSegmentId == 2)
            {
                // Clear the map from the map view (allow the user to start over and save as a new portal item)
                _myMapView.Map = new Map(Basemap.CreateLightGrayCanvas());
            }
            else if (selectedSegmentId == 3)
            {
                // Show the save map UI
                ShowSaveMapUI();
            }

            // Unselect all segments (user might want to click the same control twice)
            buttonControl.SelectedSegment = -1;
        }

        private void ShowBasemapList()
        {
            // Create a new Alert Controller
            UIAlertController basemapsActionSheet = UIAlertController.Create("Basemaps", "Choose a basemap", UIAlertControllerStyle.ActionSheet);

            // Add actions to apply each basemap type
            basemapsActionSheet.AddAction(UIAlertAction.Create("Topographic", UIAlertActionStyle.Default, (action) => _myMapView.Map.Basemap = Basemap.CreateTopographic()));
            basemapsActionSheet.AddAction(UIAlertAction.Create("Streets", UIAlertActionStyle.Default, (action) => _myMapView.Map.Basemap = Basemap.CreateStreets()));
            basemapsActionSheet.AddAction(UIAlertAction.Create("Imagery", UIAlertActionStyle.Default, (action) => _myMapView.Map.Basemap = Basemap.CreateImagery()));
            basemapsActionSheet.AddAction(UIAlertAction.Create("Oceans", UIAlertActionStyle.Default, (action) => _myMapView.Map.Basemap = Basemap.CreateOceans()));
            basemapsActionSheet.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, (action) => Console.WriteLine("Canceled")));

            // Required for iPad - You must specify a source for the Action Sheet since it is displayed as a popover
            UIPopoverPresentationController presentationPopover = basemapsActionSheet.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the list of basemaps
            PresentViewController(basemapsActionSheet, true, null);
        }

        private void ShowLayerList()
        {
            // Create a new Alert Controller
            UIAlertController layersActionSheet = UIAlertController.Create("Layers", "Choose layers", UIAlertControllerStyle.ActionSheet);

            // Add actions to add or remove each of the available layers
            foreach (var kvp in _operationalLayerUrls)
            {
                layersActionSheet.AddAction(UIAlertAction.Create(kvp.Key, UIAlertActionStyle.Default, (action) => AddOrRemoveLayer(kvp.Key)));
            }

            // Add a choice to cancel
            layersActionSheet.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, (action) => Console.WriteLine("Canceled")));

            // Required for iPad - You must specify a source for the Action Sheet since it is displayed as a popover
            UIPopoverPresentationController presentationPopover = layersActionSheet.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the list of layers to add/remove
            PresentViewController(layersActionSheet, true, null);
        }

        private async void AddOrRemoveLayer(string layerName)
        {
            // See if the layer already exists
            ArcGISMapImageLayer layer = _myMapView.Map.OperationalLayers.FirstOrDefault(l => l.Name == layerName) as ArcGISMapImageLayer;

            // If the layer is in the map, remove it
            if (layer != null)
            {
                _myMapView.Map.OperationalLayers.Remove(layer);
            }
            else
            {
                // Get the URL for this layer
                var layerUrl = _operationalLayerUrls[layerName];
                var layerUri = new Uri(layerUrl);

                // Create a new map image layer
                layer = new ArcGISMapImageLayer(layerUri);
                layer.Name = layerName;
                await layer.LoadAsync();

                // Set it 50% opaque, and add it to the map
                layer.Opacity = 0.5;
                _myMapView.Map.OperationalLayers.Add(layer);
            }
        }

        private void ShowOAuthPropsUI()
        {
            if (_oauthInfoUI != null) { return; }

            // Create a view to show entry controls over the map view

            var ovBounds = new CoreGraphics.CGRect(30, 60, (View.Bounds.Width - 60), (View.Bounds.Height - 120));
            _oauthInfoUI = new OAuthPropsDialogOverlay(ovBounds, 0.75f, UIColor.White, AppClientId, OAuthRedirectUrl);

            // Handle the OnOAuthPropsInfoEntered event to get the info entered by the user
            _oauthInfoUI.OnOAuthPropsInfoEntered += (s, e) =>
            {
                // Store the settings entered and use them to update the AuthenticationManager
                AppClientId = e.ClientId;
                OAuthRedirectUrl = e.RedirectUrl;
                UpdateAuthenticationManager();

                // Hide the OAuth entry
                _oauthInfoUI.Hide();
                _oauthInfoUI = null;
            };

            // Handle the cancel event when the user closes the dialog without choosing to save
            _oauthInfoUI.OnCanceled += (s, e) =>
            {
                _oauthInfoUI.Hide();
                _oauthInfoUI = null;
            };

            // Add the map item info UI view (will display semi-transparent over the map view)
            View.Add(_oauthInfoUI);
        }

        private void ShowSaveMapUI()
        {
            if (_mapInfoUI != null) { return; }

            // Create a view to show map item info entry controls over the map view
            var ovBounds = new CoreGraphics.CGRect(0, 60, View.Bounds.Width, View.Bounds.Height - 60);
            _mapInfoUI = new SaveMapDialogOverlay(ovBounds, 0.75f, UIColor.White, (PortalItem)_myMapView.Map.Item);

            // Handle the OnMapInfoEntered event to get the info entered by the user
            _mapInfoUI.OnMapInfoEntered += MapItemInfoEntered;

            // Handle the cancel event when the user closes the dialog without choosing to save
            _mapInfoUI.OnCanceled += SaveCanceled;

            // Add the map item info UI view (will display semi-transparent over the map view)
            View.Add(_mapInfoUI);
        }

        // Handle the OnMapInfoEntered event from the item input UI
        // MapSavedEventArgs contains the title, description, and tags that were entered
        private async void MapItemInfoEntered(object sender, MapSavedEventArgs e)
        {
            // Get the current map
            var myMap = _myMapView.Map;

            try
            {
                // Show the activity indicator so the user knows work is happening
                _activityIndicator.StartAnimating();

                // Get information entered by the user for the new portal item properties
                var title = e.Title;
                var description = e.Description;
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
                    UIAlertController alert = UIAlertController.Create("Saved map", "Saved " + title + " to ArcGIS Online", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
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
                    UIAlertController alert = UIAlertController.Create("Updated map", "Saved changes to " + title, UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
            }
            catch (Exception)
            {
                // Report save error
                UIAlertController alert = UIAlertController.Create("Error", "Unable to save " + e.Title, UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
            finally
            {
                // Get rid of the item input controls
                _mapInfoUI.Hide();
                _mapInfoUI = null;

                // Hide the progress bar
                _activityIndicator.StopAnimating();
            }
        }

        private void SaveCanceled(object sender, EventArgs e)
        {
            // Remove the item input UI
            _mapInfoUI.Hide();
            _mapInfoUI = null;
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
            catch (OperationCanceledException)
            {
                // user canceled the login
                throw new Exception("Portal log in was canceled.");
            }

            // Get the ArcGIS Online portal (will use credential from login above)
            ArcGISPortal agsOnline = await ArcGISPortal.CreateAsync(new Uri(ServerUrl));

            // Save the current state of the map as a portal item in the user's default folder
            await myMap.SaveAsAsync(agsOnline, null, title, description, tags, img);
        }

        #region OAuth helpers

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
                // Try to cancel any existing authentication process
                _taskCompletionSource.TrySetCanceled();
            }

            // Create a task completion source
            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();

            // Create a new Xamarin.Auth.OAuth2Authenticator using the information passed in
            OAuth2Authenticator auth = new OAuth2Authenticator(
                clientId: AppClientId,
                scope: "",
                authorizeUrl: new Uri(AuthorizeUrl),
                redirectUrl: new Uri(OAuthRedirectUrl))
            {
                ShowErrors = false
            };

            // Allow the user to cancel the OAuth attempt
            auth.AllowCancel = true;

            // Define a handler for the OAuth2Authenticator.Completed event
            auth.Completed += (sender, authArgs) =>
            {
                try
                {
                    // Dismiss the OAuth UI when complete
                    DismissViewController(true, null);

                    // Throw an exception if the user could not be authenticated
                    if (!authArgs.IsAuthenticated)
                    {
                        throw(new Exception("Unable to authenticate user."));
                    }

                    // If authorization was successful, get the user's account
                    Account authenticatedAccount = authArgs.Account;

                    // Set the result (Credential) for the TaskCompletionSource
                    _taskCompletionSource.SetResult(authenticatedAccount.Properties);
                }
                catch (Exception ex)
                {
                    // If authentication failed, set the exception on the TaskCompletionSource
                    _taskCompletionSource.TrySetException(ex);

                    // Cancel authentication
                    auth.OnCancelled();
                }

            };

            // If an error was encountered when authenticating, set the exception on the TaskCompletionSource
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

                // Cancel authentication
                auth.OnCancelled();
            };

            // Present the OAuth UI (on the app's UI thread) so the user can enter user name and password
            InvokeOnMainThread(() =>
            {
                PresentViewController(auth.GetUI(), true, null);
            });

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

        #endregion OAuth helpers
    }

    // View containing "configure OAuth" controls (client id and redirect url inputs with save/cancel buttons)
    public class OAuthPropsDialogOverlay : UIView
    {
        // Event to provide information the user entered when the user dismisses the view
        public event EventHandler<OAuthPropsSavedEventArgs> OnOAuthPropsInfoEntered;

        // Event to report that the entry was canceled
        public event EventHandler OnCanceled;

        // Store the input controls so the values can be read
        private UITextField _clientIdTextField;

        private UITextField _redirectUrlTextField;

        public OAuthPropsDialogOverlay(CoreGraphics.CGRect frame, nfloat transparency, UIColor color, string clientId, string redirectUrl) : base(frame)
        {
            // Create a semi-transparent overlay with the specified background color
            BackgroundColor = color;
            Alpha = transparency;

            // Set size and spacing for controls
            nfloat controlHeight = 25;
            nfloat rowSpace = 7;
            nfloat lessRowSpace = 4;
            nfloat buttonSpace = 15;
            nfloat textViewWidth = 200;
            nfloat buttonWidth = 60;

            // Get the total height and width of the control set (four rows of controls, three sets of space)
            nfloat totalHeight = (6 * controlHeight) + (5 * rowSpace);
            nfloat totalWidth = textViewWidth;

            // Find the start x and y for the control layout
            nfloat controlX = 10;
            nfloat controlY = 10;

            // Label for inputs
            var description = new UILabel(new CoreGraphics.CGRect(controlX, controlY, textViewWidth, controlHeight));
            description.Text = "OAuth Settings";
            description.TextColor = UIColor.Blue;

            // Adjust the Y position for the next control
            controlY = controlY + controlHeight + rowSpace;

            // Client ID text input and label
            var clientIdLabel = new UILabel(new CoreGraphics.CGRect(controlX, controlY, textViewWidth, controlHeight));
            clientIdLabel.Text = "Client ID";

            controlY = controlY + controlHeight + lessRowSpace;

            _clientIdTextField = new UITextField(new CoreGraphics.CGRect(controlX, controlY, textViewWidth, controlHeight));
            _clientIdTextField.Placeholder = "Client ID";
            _clientIdTextField.Text = clientId;
            _clientIdTextField.AutocapitalizationType = UITextAutocapitalizationType.None;
            _clientIdTextField.BackgroundColor = UIColor.LightGray;
            // Allow pressing 'return' to dismiss the keyboard
            _clientIdTextField.ShouldReturn += (textField) => { textField.ResignFirstResponder(); return true; };

            // Adjust the Y position for the next control
            controlY = controlY + controlHeight + rowSpace;

            // Redirect Url text input and label
            var redirectLabel = new UILabel(new CoreGraphics.CGRect(controlX, controlY, textViewWidth, controlHeight));
            redirectLabel.Text = "Redirect URL";

            controlY = controlY + controlHeight + lessRowSpace;

            _redirectUrlTextField = new UITextField(new CoreGraphics.CGRect(controlX, controlY, textViewWidth, controlHeight));
            _redirectUrlTextField.Placeholder = "Redirect URI";
            _redirectUrlTextField.Text = redirectUrl;
            _redirectUrlTextField.AutocapitalizationType = UITextAutocapitalizationType.None;
            _redirectUrlTextField.BackgroundColor = UIColor.LightGray;
            // Allow pressing 'return' to dismiss the keyboard
            _redirectUrlTextField.ShouldReturn += (textField) => { textField.ResignFirstResponder(); return true; };

            // Adjust the Y position for the next control
            controlY = controlY + controlHeight + rowSpace;

            // Button to save the values
            UIButton saveButton = new UIButton(new CoreGraphics.CGRect(controlX, controlY, buttonWidth, controlHeight));
            saveButton.SetTitle("Save", UIControlState.Normal);
            saveButton.SetTitleColor(UIColor.Red, UIControlState.Normal);
            saveButton.TouchUpInside += SaveButtonClick;

            // Adjust the X position for the next control
            controlX = controlX + buttonWidth + buttonSpace;

            // Button to cancel the save
            UIButton cancelButton = new UIButton(new CoreGraphics.CGRect(controlX, controlY, buttonWidth, controlHeight));
            cancelButton.SetTitle("Cancel", UIControlState.Normal);
            cancelButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            cancelButton.TouchUpInside += (s, e) => { OnCanceled.Invoke(this, null); };

            // Add the controls
            AddSubviews(description, clientIdLabel, _clientIdTextField, redirectLabel, _redirectUrlTextField, saveButton, cancelButton);
        }

        // Animate increasing transparency to completely hide the view, then remove it
        public void Hide()
        {
            // Action to make the view transparent
            Action makeTransparentAction = () => Alpha = 0;

            // Action to remove the view
            Action removeViewAction = () => RemoveFromSuperview();

            // Time to complete the animation (seconds)
            double secondsToComplete = 0.75;

            // Animate transparency to zero, then remove the view
            Animate(secondsToComplete, makeTransparentAction, removeViewAction);
        }

        private void SaveButtonClick(object sender, EventArgs e)
        {
            // Get the values entered in the text fields
            var clientId = _clientIdTextField.Text.Trim();
            var redirectUrl = _redirectUrlTextField.Text.Trim();

            // Make sure all required info was entered
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUrl))
            {
                new UIAlertView("Error", "Please enter a client ID and redirect URL for OAuth authentication.", null, "OK", null).Show();
                return;
            }

            // Fire the OnOAuthPropsInfoEntered event and provide the map item values
            if (OnOAuthPropsInfoEntered != null)
            {
                // Create a new OAuthPropsSavedEventArgs to contain the user's values
                var oauthSaveEventArgs = new OAuthPropsSavedEventArgs(clientId, redirectUrl);

                // Raise the event
                OnOAuthPropsInfoEntered(sender, oauthSaveEventArgs);
            }
        }
    }

    // Custom EventArgs implementation to hold OAuth information (client Id and redirect Url)
    public class OAuthPropsSavedEventArgs : EventArgs
    {
        // Client ID property
        public string ClientId { get; set; }

        // Redirect Url property
        public string RedirectUrl { get; set; }

        // Store map item values passed into the constructor
        public OAuthPropsSavedEventArgs(string clientId, string redirectUrl)
        {
            ClientId = clientId;
            RedirectUrl = redirectUrl;
        }
    }

    // View containing "save map" controls (title, description, and tags inputs with save/cancel buttons)
    public class SaveMapDialogOverlay : UIView
    {
        // Event to provide information the user entered when the user dismisses the view
        public event EventHandler<MapSavedEventArgs> OnMapInfoEntered;

        // Event to report that the save was canceled
        public event EventHandler OnCanceled;

        // Store the input controls so the values can be read
        private UITextField _titleTextField;

        private UITextField _descriptionTextField;
        private UITextField _tagsTextField;

        // Store any existing portal item (for "update" versus "save", e.g.)
        private PortalItem _portalItem = null;

        public SaveMapDialogOverlay(CoreGraphics.CGRect frame, nfloat transparency, UIColor color, PortalItem mapItem) : base(frame)
        {
            // Store the current portal item for the map (if any)
            var portalItem = mapItem;

            // Create a semi-transparent overlay with the specified background color
            BackgroundColor = color;
            Alpha = transparency;

            // Set size and spacing for controls
            nfloat controlHeight = 25;
            nfloat rowSpace = 11;
            nfloat buttonSpace = 15;
            nfloat textViewWidth = Frame.Width - 60;
            nfloat buttonWidth = 60;

            // Get the total height and width of the control set (five rows of controls, four sets of space)
            nfloat totalHeight = (5 * controlHeight) + (4 * rowSpace);
            nfloat totalWidth = textViewWidth;

            // Find the center x and y of the view
            nfloat centerX = Frame.Width / 2;
            nfloat centerY = Frame.Height / 2;

            // Find the start x and y for the control layout
            nfloat controlX = centerX - (totalWidth / 2);
            nfloat controlY = centerY - (totalHeight / 2);

            // Label for inputs
            var description = new UILabel(new CoreGraphics.CGRect(controlX, controlY, textViewWidth, controlHeight));
            description.Text = "Portal item info";
            description.TextColor = UIColor.Blue;

            // Adjust the Y position for the next control
            controlY = controlY + controlHeight + rowSpace;

            // Title text input
            _titleTextField = new UITextField(new CoreGraphics.CGRect(controlX, controlY, textViewWidth, controlHeight));
            _titleTextField.Placeholder = "Title";
            _titleTextField.AutocapitalizationType = UITextAutocapitalizationType.None;
            _titleTextField.BackgroundColor = UIColor.LightGray;
            // Allow pressing 'return' to dismiss the keyboard
            _titleTextField.ShouldReturn += (textField) => { textField.ResignFirstResponder(); return true; };

            // Adjust the Y position for the next control
            controlY = controlY + controlHeight + rowSpace;

            // Description text input
            _descriptionTextField = new UITextField(new CoreGraphics.CGRect(controlX, controlY, textViewWidth, controlHeight));
            _descriptionTextField.Placeholder = "Description";
            _descriptionTextField.AutocapitalizationType = UITextAutocapitalizationType.None;
            _descriptionTextField.BackgroundColor = UIColor.LightGray;
            // Allow pressing 'return' to dismiss the keyboard
            _descriptionTextField.ShouldReturn += (textField) => { textField.ResignFirstResponder(); return true; };

            // Adjust the Y position for the next control
            controlY = controlY + controlHeight + rowSpace;

            // Tags text input
            _tagsTextField = new UITextField(new CoreGraphics.CGRect(controlX, controlY, textViewWidth, controlHeight));
            _tagsTextField.Text = "ArcGIS Runtime, Web Map";
            _tagsTextField.AutocapitalizationType = UITextAutocapitalizationType.None;
            _tagsTextField.BackgroundColor = UIColor.LightGray;
            // Allow pressing 'return' to dismiss the keyboard
            _tagsTextField.ShouldReturn += (textField) => { textField.ResignFirstResponder(); return true; };

            // Adjust the Y position for the next control
            controlY = controlY + controlHeight + rowSpace;

            // Button to save the map
            UIButton saveButton = new UIButton(new CoreGraphics.CGRect(controlX, controlY, buttonWidth, controlHeight));
            saveButton.SetTitle("Save", UIControlState.Normal);
            saveButton.SetTitleColor(UIColor.Red, UIControlState.Normal);
            saveButton.TouchUpInside += SaveButtonClick;

            // Adjust the X position for the next control
            controlX = controlX + buttonWidth + buttonSpace;

            // Button to cancel the save
            UIButton cancelButton = new UIButton(new CoreGraphics.CGRect(controlX, controlY, buttonWidth, controlHeight));
            cancelButton.SetTitle("Cancel", UIControlState.Normal);
            cancelButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            cancelButton.TouchUpInside += (s, e) => { OnCanceled.Invoke(this, null); };

            // Add the controls
            AddSubviews(description, _titleTextField, _descriptionTextField, _tagsTextField, saveButton, cancelButton);

            // If there's an existing portal item, configure the dialog for "update" (read-only entries)
            if (_portalItem != null)
            {
                _titleTextField.Text = _portalItem.Title;
                _titleTextField.Enabled = false;

                _descriptionTextField.Text = _portalItem.Description;
                _descriptionTextField.Enabled = false;

                _tagsTextField.Text = string.Join(",", _portalItem.Tags);
                _tagsTextField.Enabled = false;

                // Change the button text
                saveButton.SetTitle("Update", UIControlState.Normal);
            }
        }

        // Animate increasing transparency to completely hide the view, then remove it
        public void Hide()
        {
            // Action to make the view transparent
            Action makeTransparentAction = () => Alpha = 0;

            // Action to remove the view
            Action removeViewAction = () => RemoveFromSuperview();

            // Time to complete the animation (seconds)
            double secondsToComplete = 0.75;

            // Animate transparency to zero, then remove the view
            Animate(secondsToComplete, makeTransparentAction, removeViewAction);
        }

        private void SaveButtonClick(object sender, EventArgs e)
        {
            // Get the values entered in the text fields
            var title = _titleTextField.Text.Trim();
            var description = _descriptionTextField.Text.Trim();
            var tags = _tagsTextField.Text.Split(',');

            // Make sure all required info was entered
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) || tags.Length == 0)
            {
                new UIAlertView("Error", "Please enter a title, description, and some tags to describe the map.", null, "OK", null).Show();
                return;
            }

            // Fire the OnMapInfoEntered event and provide the map item values
            if (OnMapInfoEntered != null)
            {
                // Create a new MapSavedEventArgs to contain the user's values
                var mapSaveEventArgs = new MapSavedEventArgs(title, description, tags);

                // Raise the event
                OnMapInfoEntered(sender, mapSaveEventArgs);
            }
        }
    }

    // Custom EventArgs implementation to hold map item information (title, description, and tags)
    public class MapSavedEventArgs : EventArgs
    {
        // Title property
        public string Title { get; set; }

        // Description property
        public string Description { get; set; }

        // Tags property
        public string[] Tags { get; set; }

        // Store map item values passed into the constructor
        public MapSavedEventArgs(string title, string description, string[] tags)
        {
            Title = title;
            Description = description;
            Tags = tags;
        }
    }
}