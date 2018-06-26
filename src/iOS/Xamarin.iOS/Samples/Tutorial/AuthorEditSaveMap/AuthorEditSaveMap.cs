// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CoreGraphics;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.AuthorEditSaveMap
{
    [Register("AuthorEditSaveMap")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Author, edit, and save a map",
        "Tutorial",
        "This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). It is also the solution to the [Author, edit, and save maps to your portal tutorial](https://developers.arcgis.com/net/latest/ios/guide/author-edit-and-save-maps-to-your-portal.htm). Saving a map to arcgis.com requires an ArcGIS Online login.",
        "1. Pan and zoom to the extent you would like for your map.\n2. Choose a basemap from the list of available basemaps.\n3. Click 'Save ...' and provide info for the new portal item (Title, Description, and Tags).\n4. Click 'Save Map to Portal'.\n5. After successfully logging in to your ArcGIS Online account, the map will be saved to your default folder.\n6. You can make additional changes, update the map, and then re-save to store changes in the portal item.")]
    public class AuthorEditSaveMap : UIViewController, IOAuthAuthorizeHandler
    {
        // View model that stores the map.
        private readonly MapViewModel _mapViewModel;

        // UI controls that need to be referenced.
        private readonly UISegmentedControl _segmentButton = new UISegmentedControl("New", "Basemap", "Save");
        private readonly UIToolbar _toolbar = new UIToolbar();
        private readonly MapView _mapView = new MapView();

        // Overlay with entry controls for map item details (title, description, and tags).
        private SaveMapDialogOverlay _mapInfoUi;

        // Constants for OAuth-related values.
        // URL of the server to authenticate with (ArcGIS Online).
        private const string ArcGISOnlineUrl = "https://www.arcgis.com/sharing/rest";

        // Client ID for the app registered with the server (Portal Maps).
        private const string AppClientId = "2Gh53JRzkPtOENQq";

        // Redirect URL after a successful authorization (configured for the Portal Maps application).
        private const string OAuthRedirectUrl = "https://developers.arcgis.com";

        // Use a TaskCompletionSource to track the completion of the authorization.
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;

        public AuthorEditSaveMap()
        {
            Title = "Author edit and save maps";

            // Create a new view model and pass the map view control.
            _mapViewModel = new MapViewModel
            {
                AppMapView = _mapView
            };

            // Listen for changes on the view model.
            _mapViewModel.PropertyChanged += MapViewModel_PropertyChanged;
        }

        public override void ViewDidLoad()
        {
            // Called on initial page load.
            base.ViewDidLoad();

            // Call the function that creates the UI.
            CreateLayout();

            // Set up the AuthenticationManager.
            UpdateAuthenticationManager();

            // Use the map from the view-model.
            _mapViewModel.ResetMap();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

                // Reposition controls.
                _mapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _toolbar.Frame = new CGRect(0, View.Bounds.Height - 40, View.Bounds.Width, 40);
                _segmentButton.Frame = new CGRect(5, _toolbar.Frame.Top + 5, View.Bounds.Width - 10, 30);

                // Update insets.
                _mapView.ViewInsets = new UIEdgeInsets(topMargin, 0, 40, 0);
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private void CreateLayout()
        {
            // Handle the "click" for each segment (new segment is selected).
            _segmentButton.ValueChanged += SegmentButtonClicked;

            // Add the controls to the view.
            View.AddSubviews(_mapView, _toolbar, _segmentButton);
        }

        private void MapViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Update the map view with the view model's new map.
            if (e.PropertyName == nameof(MapViewModel.Map) && _mapView != null)
            {
                _mapView.Map = _mapViewModel.Map;
            }
        }

        #region OAuth

        private void UpdateAuthenticationManager()
        {
            // Register the server information with the AuthenticationManager.
            ServerInfo portalServerInfo = new ServerInfo
            {
                ServerUri = new Uri(ArcGISOnlineUrl),
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

        // IOAuthAuthorizeHandler.AuthorizeAsync implementation.
        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            // If the TaskCompletionSource is not null, authorization may already be in progress and should be canceled.
            // Try to cancel any existing authentication task.
            _taskCompletionSource?.TrySetCanceled();

            // Create a task completion source.
            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();

            // Create a new Xamarin.Auth.OAuth2Authenticator using the information passed in.
            Xamarin.Auth.OAuth2Authenticator authenticator = new Xamarin.Auth.OAuth2Authenticator(
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
            authenticator.Completed += (sender, authArgs) =>
            {
                try
                {
                    // Dismiss the OAuth UI when complete.
                    DismissViewController(true, null);

                    // Throw an exception if the user could not be authenticated.
                    if (!authArgs.IsAuthenticated)
                    {
                        throw new OperationCanceledException("Unable to authenticate user.");
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
                    authenticator.OnCancelled();
                }
                finally
                {
                    DismissViewController(false, null);
                }
            };

            // If an error was encountered when authenticating, set the exception on the TaskCompletionSource.
            authenticator.Error += (sender, errArgs) =>
            {
                // If the user cancels, the Error event is raised but there is no exception ... best to check first.
                if (errArgs.Exception != null)
                {
                    _taskCompletionSource.TrySetException(errArgs.Exception);
                }
                else
                {
                    // Login canceled: dismiss the OAuth login.
                    if (_taskCompletionSource != null)
                    {
                        _taskCompletionSource.TrySetCanceled();
                        DismissViewController(true, null);
                    }
                }

                // Cancel authentication.
                authenticator.OnCancelled();
            };

            // Present the OAuth UI so the user can enter user name and password.
            InvokeOnMainThread(() => { PresentViewController(authenticator.GetUI(), true, null); });

            // Return completion source task so the caller can await completion.
            return _taskCompletionSource?.Task;
        }

        // ChallengeHandler function that will be called whenever access to a secured resource is attempted.
        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            Credential credential = null;

            try
            {
                // IOAuthAuthorizeHandler will challenge the user for OAuth credentials.
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);
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

        #endregion OAuth

        #region UI Event Handlers

        private async void SegmentButtonClicked(object sender, EventArgs e)
        {
            // Get the segmented button control that raised the event.
            var buttonControl = sender as UISegmentedControl;

            switch (buttonControl.SelectedSegment)
            {
                case 0:
                    // Create a new map.
                    _mapViewModel.ResetMap();
                    break;
                case 1:
                    // Show basemap choices.
                    ShowBasemapList();
                    break;
                case 2:
                    // Create a challenge request for portal credentials (OAuth credential request for arcgis.com).
                    CredentialRequestInfo challengeRequest = new CredentialRequestInfo();

                    // Use the OAuth implicit grant flow.
                    challengeRequest.GenerateTokenOptions = new GenerateTokenOptions
                    {
                        TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
                    };

                    // Indicate the URL (portal) to authenticate with (ArcGIS Online).
                    challengeRequest.ServiceUri = new Uri("https://www.arcgis.com/sharing/rest");

                    try
                    {
                        // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler.
                        await AuthenticationManager.Current.GetCredentialAsync(challengeRequest, false);
                    }
                    catch (Exception)
                    {
                        // user canceled the login.
                        buttonControl.SelectedSegment = -1;
                        return;
                    }

                    // Show the save map UI.
                    if (_mapInfoUi != null)
                    {
                        buttonControl.SelectedSegment = -1;
                        return;
                    }

                    // Create a view to show map item info entry controls over the map view.
                    var ovBounds = _mapView.Bounds;
                    ovBounds.Height = ovBounds.Height + 60;
                    _mapInfoUi = new SaveMapDialogOverlay(ovBounds, 0.75f, UIColor.White, _mapView.Map.Item);

                    // Handle the OnMapInfoEntered event to get the info entered by the user.
                    _mapInfoUi.OnMapInfoEntered += MapItemInfoEntered;

                    // Handle the cancel event when the user closes the dialog without choosing to save.
                    _mapInfoUi.OnCanceled += SaveCanceled;

                    // Add the map item info UI view (will display semi-transparent over the map view).
                    View.Add(_mapInfoUi);
                    break;
            }

            // Deselect all segments (user might want to click the same control twice).
            buttonControl.SelectedSegment = -1;
        }

        private void ShowBasemapList()
        {
            // Create a new Alert Controller.
            UIAlertController basemapsActionSheet = UIAlertController.Create("Basemaps", "Choose a basemap", UIAlertControllerStyle.ActionSheet);

            // Create an action that will apply a selected basemap.
            Action<UIAlertAction> changeBasemapAction = action => _mapViewModel.ChangeBasemap(action.Title);

            // Add items to the action sheet to apply each basemap type.
            foreach (string bm in _mapViewModel.BasemapChoices)
            {
                UIAlertAction actionItem = UIAlertAction.Create(bm, UIAlertActionStyle.Default, changeBasemapAction);
                basemapsActionSheet.AddAction(actionItem);
            }

            // Required for iPad - You must specify a source for the Action Sheet since it is displayed as a popover.
            UIPopoverPresentationController presentationPopover = basemapsActionSheet.PopoverPresentationController;
            if (presentationPopover != null)
            {
                presentationPopover.SourceView = View;
                presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Display the list of basemaps.
            PresentViewController(basemapsActionSheet, true, null);
        }

        private void SaveCanceled(object sender, EventArgs e)
        {
            // Remove the item input UI.
            _mapInfoUi.Hide();
            _mapInfoUi = null;
        }

        // Handle the OnMapInfoEntered event from the item input UI.
        // MapSavedEventArgs contains the title, description, and tags that were entered.
        private async void MapItemInfoEntered(object sender, MapSavedEventArgs e)
        {
            try
            {
                // Get information entered by the user for the new portal item properties.
                string title = e.Title;
                string description = e.Description;
                string[] tags = e.Tags;

                // Get the current extent.
                var currentViewpoint = _mapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

                // Export the current map view for the item's thumbnail.
                RuntimeImage thumbnailImg = await _mapView.ExportImageAsync();

                // See if the map has already been saved (has an associated portal item).
                if (!_mapViewModel.MapIsSaved)
                {
                    // Call a method on MapViewModel to save the map as a new portal item.
                    await _mapViewModel.SaveNewMapAsync(currentViewpoint, title, description, tags, thumbnailImg);

                    // Report a successful save.
                    UIAlertController alert = UIAlertController.Create("Saved map", "Saved " + title + " to ArcGIS Online", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
                else
                {
                    // Map has previously been saved as a portal item, update it (title, description, and tags will remain the same).
                    _mapViewModel.UpdateMapItem();

                    // Report success.
                    UIAlertController alert = UIAlertController.Create("Updated map", "Saved changes to " + title, UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
            }
            catch (Exception ex)
            {
                // Report save error.
                UIAlertController alert = UIAlertController.Create("Error", ex.Message, UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
            finally
            {
                // Get rid of the item input controls.
                if (_mapInfoUi != null)
                {
                    _mapInfoUi.Hide();
                    _mapInfoUi = null;
                }
            }
        }

        #endregion UI Event Handlers
    }

    // Custom EventArgs implementation to hold map item information (title, description, and tags).
    public class MapSavedEventArgs : EventArgs
    {
        public string Title { get; }
        public string Description { get; }
        public string[] Tags { get; }

        // Store map item values passed into the constructor.
        public MapSavedEventArgs(string title, string description, string[] tags)
        {
            Title = title;
            Description = description;
            Tags = tags;
        }
    }

    // View containing "save map" controls (title, description, and tags inputs with save/cancel buttons).
    public class SaveMapDialogOverlay : UIView
    {
        // Event to provide information the user entered when the view closes.
        public event EventHandler<MapSavedEventArgs> OnMapInfoEntered;

        // Event to report that the save was canceled.
        public event EventHandler OnCanceled;

        // Store the input controls so the values can be read.
        private readonly UITextField _titleTextField;
        private readonly UITextField _descriptionTextField;
        private readonly UITextField _tagsTextField;

        public SaveMapDialogOverlay(CGRect frame, nfloat transparency, UIColor color, Item mapItem) : base(frame)
        {
            // Store any existing portal item (for "update" versus "save", e.g.).
            var portalItem = mapItem as PortalItem;

            // Create a semi-transparent overlay with the specified background color.
            BackgroundColor = color;
            Alpha = transparency;

            // Set size and spacing for controls.
            nfloat controlHeight = 25;
            nfloat rowSpace = 11;
            nfloat buttonSpace = 15;
            nfloat textViewWidth = Frame.Width - 60;
            nfloat buttonWidth = 60;

            // Get the total height and width of the control set (four rows of controls, three sets of space).
            nfloat totalHeight = 4 * controlHeight + 3 * rowSpace;
            nfloat totalWidth = textViewWidth;

            // Find the center x and y of the view.
            nfloat centerX = Frame.Width / 2;
            nfloat centerY = Frame.Height / 2;

            // Find the start x and y for the control layout.
            nfloat controlX = centerX - totalWidth / 2;
            nfloat controlY = centerY - totalHeight / 2;

            // Title text input.
            _titleTextField = new UITextField(new CGRect(controlX, controlY, textViewWidth, controlHeight))
            {
                Placeholder = "Title",
                AutocapitalizationType = UITextAutocapitalizationType.None,
                BackgroundColor = UIColor.LightGray,
                LeftView = new UIView(new CGRect(0, 0, 5, 20)),
                LeftViewMode = UITextFieldViewMode.Always
            };

            // Allow pressing 'return' to dismiss the keyboard.
            _titleTextField.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            // Adjust the Y position for the next control.
            controlY = controlY + controlHeight + rowSpace;

            // Description text input.
            _descriptionTextField = new UITextField(new CGRect(controlX, controlY, textViewWidth, controlHeight))
            {
                Placeholder = "Description",
                AutocapitalizationType = UITextAutocapitalizationType.None,
                BackgroundColor = UIColor.LightGray,
                LeftView = new UIView(new CGRect(0, 0, 5, 20)),
                LeftViewMode = UITextFieldViewMode.Always
            };

            // Allow pressing 'return' to dismiss the keyboard.
            _descriptionTextField.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            // Adjust the Y position for the next control.
            controlY = controlY + controlHeight + rowSpace;

            // Tags text input.
            _tagsTextField = new UITextField(new CGRect(controlX, controlY, textViewWidth, controlHeight))
            {
                Text = "ArcGIS Runtime, Web Map",
                AutocapitalizationType = UITextAutocapitalizationType.None,
                BackgroundColor = UIColor.LightGray,
                LeftView = new UIView(new CGRect(0, 0, 5, 20)),
                LeftViewMode = UITextFieldViewMode.Always
            };

            // Allow pressing 'return' to dismiss the keyboard.
            _tagsTextField.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            // Adjust the Y position for the next control.
            controlY = controlY + controlHeight + rowSpace;

            // Button to save the map.
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
            cancelButton.TouchUpInside += (s, e) => OnCanceled.Invoke(this, null);

            // Add the controls.
            AddSubviews(_titleTextField, _descriptionTextField, _tagsTextField, saveButton, cancelButton);

            // If there's an existing portal item, configure the dialog for "update" (read-only entries).
            if (portalItem != null)
            {
                _titleTextField.Text = portalItem.Title;
                _titleTextField.Enabled = false;

                _descriptionTextField.Text = portalItem.Description;
                _descriptionTextField.Enabled = false;

                _tagsTextField.Text = string.Join(",", portalItem.Tags);
                _tagsTextField.Enabled = false;

                // Change the button text.
                saveButton.SetTitle("Update", UIControlState.Normal);
            }
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
            string title = _titleTextField.Text.Trim();
            string description = _descriptionTextField.Text.Trim();
            string[] tags = _tagsTextField.Text.Split(',');

            // Make sure all required info was entered.
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) || tags.Length == 0)
            {
                new UIAlertView("Error", "Please enter a title, description, and some tags to describe the map.", (IUIAlertViewDelegate) null, "OK", null).Show();
                return;
            }

            // Fire the OnMapInfoEntered event and provide the map item values.
            if (OnMapInfoEntered != null)
            {
                // Create a new MapSavedEventArgs to contain the user's values.
                var mapSaveEventArgs = new MapSavedEventArgs(title, description, tags);

                // Raise the event.
                OnMapInfoEntered(sender, mapSaveEventArgs);
            }
        }
    }

    // ViewModel class that is bound to the View (AuthorEditSaveMap).
    // Note: in a ArcGIS Runtime for .NET template project, this class will be in a separate file: "MapViewModel.cs".
    public class MapViewModel : INotifyPropertyChanged
    {
        // Store the map view used by the app.
        private MapView _mapView;

        public MapView AppMapView
        {
            set { _mapView = value; }
        }

        private Map _map = new Map(Basemap.CreateStreetsVector());

        // Gets or sets the map.
        public Map Map
        {
            get { return _map; }
            set
            {
                _map = value;
                OnPropertyChanged();
            }
        }

        // String array to store basemap constructor types.

        // Read-only property to return the available basemap names.
        public string[] BasemapChoices { get; } =
        {
            "Topographic",
            "Topographic Vector",
            "Streets",
            "Streets Vector",
            "Imagery",
            "Oceans"
        };

        public void ChangeBasemap(string basemap)
        {
            // Apply the selected basemap to the map.
            switch (basemap)
            {
                case "Topographic":
                    // Set the basemap to Topographic.
                    _map.Basemap = Basemap.CreateTopographic();
                    break;

                case "Topographic Vector":
                    // Set the basemap to Topographic (vector).
                    _map.Basemap = Basemap.CreateTopographicVector();
                    break;

                case "Streets":
                    // Set the basemap to Streets.
                    _map.Basemap = Basemap.CreateStreets();
                    break;

                case "Streets Vector":
                    // Set the basemap to Streets (vector).
                    _map.Basemap = Basemap.CreateStreetsVector();
                    break;

                case "Imagery":
                    // Set the basemap to Imagery.
                    _map.Basemap = Basemap.CreateImagery();
                    break;

                case "Oceans":
                    // Set the basemap to Oceans.
                    _map.Basemap = Basemap.CreateOceans();
                    break;
            }
        }

        // Save the current map to ArcGIS Online. The initial extent, title, description, and tags are passed in.
        public async Task SaveNewMapAsync(Viewpoint initialViewpoint, string title, string description, string[] tags, RuntimeImage img)
        {
            // Get the ArcGIS Online portal.
            ArcGISPortal agsOnline = await ArcGISPortal.CreateAsync(new Uri("https://www.arcgis.com/sharing/rest"));

            // Set the map's initial viewpoint using the extent (viewpoint) passed in.
            _map.InitialViewpoint = initialViewpoint;

            // Save the current state of the map as a portal item in the user's default folder.
            await _map.SaveAsAsync(agsOnline, null, title, description, tags, img);
        }

        public bool MapIsSaved => _map?.Item != null;

        public async void UpdateMapItem()
        {
            // Save the map.
            await _map.SaveAsync();

            // Export the current map view for the item's thumbnail.
            RuntimeImage thumbnailImg = await _mapView.ExportImageAsync();

            // Get the file stream from the new thumbnail image.
            Stream imageStream = await thumbnailImg.GetEncodedBufferAsync();

            // Update the item thumbnail.
            (_map.Item as PortalItem).SetThumbnailWithImage(imageStream);
            await _map.SaveAsync();
        }

        public void ResetMap()
        {
            // Set the current map to null.
            _map = null;

            // Create a new map with gray canvas basemap.
            Map = new Map(Basemap.CreateLightGrayCanvasVector());
        }

        // Raises the "MapViewModel.PropertyChanged" event.
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var propertyChangedHandler = PropertyChanged;
            propertyChangedHandler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}