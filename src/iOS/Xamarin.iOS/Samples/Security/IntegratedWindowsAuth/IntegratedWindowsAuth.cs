// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using CoreGraphics;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.IntegratedWindowsAuth
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Integrated Windows Authentication",
        category: "Security",
        description: "Connect to an IWA secured Portal and search for maps.",
        instructions: "1. Enter the URL to your IWA-secured portal.",
        tags: new[] { "Portal", "Windows", "authentication", "security" })]
    [Register("IntegratedWindowsAuth")]
    public class IntegratedWindowsAuth : UIViewController
    {
        // A TaskCompletionSource to store the result of a login task.
        private TaskCompletionSource<Credential> _loginTaskCompletionSrc;

        // The map view to display a map in the app.
        private MapView _myMapView;

        // A table view to show search results (web map portal items).
        private UITableView _webMapTableView = new UITableView();

        // UI controls needed for user input.
        private readonly UIToolbar _toolbar = new UIToolbar();
        private UITextField _securePortalUrlEntry;
        private UIButton _searchSecurePortalButton;
        private UIButton _searchPublicPortalButton;
        private UILabel _messagesLabel;

        // An overlay containing login controls to display over the map view.
        private LoginOverlay _loginUI;

        // The ArcGIS Online URL for searching public web maps.
        private const string PublicPortalUrl = "https://www.arcgis.com";

        public IntegratedWindowsAuth()
        {
            Title = "Integrated Windows Authentication";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Call a function to create the user interface.
            CreateLayout();

            // Call a function to initialize the app.
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                // Margins and control heights for calculating positions in the UI.
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat margin = 5;
                nfloat controlHeight = 30;

                // Position the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _toolbar.Frame = new CGRect(0, topMargin, View.Bounds.Width, controlHeight * 6 + margin * 5);
                _securePortalUrlEntry.Frame = new CGRect(margin, topMargin + margin, View.Bounds.Width - 2 * margin, controlHeight);
                _searchPublicPortalButton.Frame = new CGRect(margin, topMargin + controlHeight + margin, View.Bounds.Width / 2 - 2 * margin, controlHeight);
                _searchSecurePortalButton.Frame = new CGRect(View.Bounds.Width / 2 + margin, topMargin + controlHeight + margin, View.Bounds.Width / 2 - margin, controlHeight);
                _webMapTableView.Frame = new CGRect(margin, topMargin + 2 * controlHeight + 2 * margin, View.Bounds.Width - 2 * margin, controlHeight * 3);
                _messagesLabel.Frame = new CGRect(margin, topMargin + 5 * controlHeight + 4 * margin, View.Bounds.Width - 2 * margin, controlHeight);
                _myMapView.ViewInsets = new UIEdgeInsets(_toolbar.Frame.Bottom, 0, 0, 0);

                base.ViewDidLayoutSubviews();
            }
            catch (NullReferenceException)
            {
                // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            }
        }

        private void CreateLayout()
        {
            // Setup the visual frame for the MapView.
            var mapViewRect = new CGRect(0, 90, View.Bounds.Width, View.Bounds.Height - 90);

            // Create the map view (map will be added in Initialize).
            _myMapView = new MapView
            {
                Frame = mapViewRect
            };

            // Text entry for the secure portal URL.
            _securePortalUrlEntry = new UITextField
            {
                Placeholder = "Enter IWA-secured portal URL",
                BorderStyle = UITextBorderStyle.RoundedRect,
                BackgroundColor = ApplicationTheme.BackgroundColor,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                SpellCheckingType = UITextSpellCheckingType.No,
                AutocorrectionType = UITextAutocorrectionType.No
            };

            // Allow the search bar to dismiss the keyboard.
            _securePortalUrlEntry.ShouldReturn += sender =>
            {
                sender.ResignFirstResponder();
                return true;
            };

            // Button for searching web maps on the secured portal.
            _searchSecurePortalButton = new UIButton();
            _searchSecurePortalButton.SetTitle("Search secure", UIControlState.Normal);
            _searchSecurePortalButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _searchSecurePortalButton.TouchUpInside += SearchSecurePortalButton_Click;

            // Button for searching the public portal.
            _searchPublicPortalButton = new UIButton();
            _searchPublicPortalButton.SetTitle("Search ArcGIS online", UIControlState.Normal);
            _searchPublicPortalButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _searchPublicPortalButton.TouchUpInside += SearchPublicPortalButton_Click;

            // Table view to show web map item results.
            _webMapTableView = new UITableView
            {
                RowHeight = 20
            };

            // A label to display errors and other messages.
            _messagesLabel = new UILabel
            {
                Text = "Search portals for web maps",
                TextAlignment = UITextAlignment.Center
            };
            _messagesLabel.Font = _messagesLabel.Font.WithSize(10.0f);

            // Add the map view and toolbar controls to the page.
            View.AddSubviews(_myMapView, _toolbar, _securePortalUrlEntry, _webMapTableView, _searchSecurePortalButton, _searchPublicPortalButton, _messagesLabel);
            View.BackgroundColor = ApplicationTheme.BackgroundColor;
        }

        private void Initialize()
        {
            // Define a challenge handler method for the AuthenticationManager.
            // This method handles getting credentials when a secured resource is encountered.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Show a default map (light gray canvas).
            Map defaultMap = new Map(BasemapStyle.ArcGISLightGray)
            {
                InitialViewpoint = new Viewpoint(0.0, 0.0, 200000000)
            };
            _myMapView.Map = defaultMap;
        }

        // AuthenticationManager.ChallengeHandler function that prompts the user for login information to create a credential.
        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // Ignore token or certificate authentication challenges (would require more code/UI).
            if (info.AuthenticationType != AuthenticationType.NetworkCredential)
            {
                Console.WriteLine("Skipping authentication for " + info.ServiceUri.Host);
                return null;
            }

            // Return if authentication is already in progress.
            if (_loginTaskCompletionSrc != null && !_loginTaskCompletionSrc.Task.IsCanceled)
            {
                return null;
            }

            // Create a new TaskCompletionSource for the login operation.
            // (passing the CredentialRequestInfo object to the constructor will make it available from its AsyncState property)
            _loginTaskCompletionSrc = new TaskCompletionSource<Credential>(info);

            // Show the login controls on the UI thread, OnLoginInfoEntered event will return the values entered (username, password, and domain)
            InvokeOnMainThread(ShowLoginUI);

            // Return the login task, the result will be ready when completed (user provides login info and clicks the "Login" button).
            return await _loginTaskCompletionSrc.Task;
        }

        private async void SearchSecurePortalButton_Click(object sender, EventArgs e)
        {
            // Clear any previous results.
            _webMapTableView.Source = null;
            _webMapTableView.ReloadData();

            try
            {
                // Get the value entered for the secure portal URL.
                string securedPortalUrl = _securePortalUrlEntry.Text.Trim();

                // Make sure a portal URL has been entered in the text box.
                if (string.IsNullOrEmpty(securedPortalUrl))
                {
                    _messagesLabel.Text = "Please enter the URL of the secured portal.";
                    return;
                }

                // Create an instance of the IWA-secured portal, the user may be challenged for access.
                var iwaSecuredPortal = await ArcGISPortal.CreateAsync(new Uri(securedPortalUrl), true);

                // Call a function to search the portal.
                SearchPortal(iwaSecuredPortal);

                // Report the username for this connection.
                if (iwaSecuredPortal.User != null)
                {
                    _messagesLabel.Text = "Connected as: " + iwaSecuredPortal.User.UserName;
                }
                else
                {
                    // This shouldn't happen (if the portal is truly secured)!
                    _messagesLabel.Text = "Connected anonymously";
                }
            }
            catch (TaskCanceledException)
            {
                // Report canceled login.
                _messagesLabel.Text = "Login was canceled";
            }
            catch (Exception ex)
            {
                // Report errors (connecting to the secured portal, for example).
                _messagesLabel.Text = ex.Message;
            }
            finally
            {
                // Set the task completion source to null so user can attempt another login (if it failed).
                _loginTaskCompletionSrc = null;
            }
        }

        private async void SearchPublicPortalButton_Click(object sender, EventArgs e)
        {
            // Clear any previous results.
            _webMapTableView.Source = null;
            _webMapTableView.ReloadData();

            try
            {
                // Create an instance of the public portal.
                var publicPortal = await ArcGISPortal.CreateAsync(new Uri(PublicPortalUrl));

                // Call a function to search the portal.
                SearchPortal(publicPortal);
            }
            catch (Exception ex)
            {
                // Report errors, if any.
                _messagesLabel.Text = ex.Message;
            }
        }

        private async void SearchPortal(ArcGISPortal currentPortal)
        {
            // Show status message.
            _messagesLabel.Text = "Searching for web map items on the portal at " + currentPortal.Uri.AbsoluteUri;
            var messageBuilder = new StringBuilder();

            try
            {
                // Report connection info.
                messageBuilder.AppendLine("Connected to the portal on " + currentPortal.Uri.Host);

                // Report the user name used for this connection.
                if (currentPortal.User != null)
                {
                    messageBuilder.AppendLine("Connected as: " + currentPortal.User.UserName);
                }
                else
                {
                    // Note: This shouldn't happen for a secure portal!
                    messageBuilder.AppendLine("Anonymous");
                }

                // Search the portal for web maps.
                var items = await currentPortal.FindItemsAsync(new PortalQueryParameters("type:(\"web map\" NOT \"web mapping application\")"));

                // Build a list of items from the results that stores the map name as a key for the item.
                var resultItems = from r in items.Results select new KeyValuePair<string, PortalItem>(r.Title, r);

                // Add the items to a dictionary.
                List<PortalItem> webMapItems = new List<PortalItem>();
                foreach (var itm in resultItems)
                {
                    webMapItems.Add(itm.Value);
                }

                // Show the portal item titles in the list view.
                PortalItemListSource webMapTableSource = new PortalItemListSource(webMapItems);
                webMapTableSource.OnWebMapSelected += WebMapTableSource_OnWebMapSelected;
                _webMapTableView.Source = webMapTableSource;
                _webMapTableView.ReloadData();
            }
            catch (Exception ex)
            {
                // Report errors searching the portal.
                messageBuilder.AppendLine(ex.Message);
            }
            finally
            {
                // Show messages.
                _messagesLabel.Text = messageBuilder.ToString();
            }
        }

        private void WebMapTableSource_OnWebMapSelected(object sender, WebMapSelectedEventArgs e)
        {
            try
            {
                // Get the web map (portal item) that was selected.
                var webMap = e.SelectedWebMapItem;
                if (webMap != null)
                {
                    // Create a new map from the portal item and display it in the map view.
                    var map = new Map(webMap);
                    _myMapView.Map = map;
                }

                _messagesLabel.Text = "Loaded web map from item " + webMap.ItemId;
            }
            catch (Exception ex)
            {
                // Report error.
                _messagesLabel.Text = "Exception: " + ex.Message;
            }
        }

        private void ShowLoginUI()
        {
            // Create a view to show login controls over the map view.
            var ovBounds = _myMapView.Bounds;
            _loginUI = new LoginOverlay(ovBounds, 0.65f, UIColor.DarkGray);

            // Handle the login event to get the login entered by the user.
            _loginUI.OnLoginInfoEntered += LoginEntered;

            // Handle the cancel event when the user closes the dialog without entering a login.
            _loginUI.OnCanceled += LoginCanceled;

            // Add the login UI view (will display semi-transparent over the map view).
            View.Add(_loginUI);
        }

        // Handle the OnLoginEntered event from the login UI, LoginEventArgs contains the username, password, and domain that were entered
        private void LoginEntered(object sender, LoginEventArgs e)
        {
            // Make sure the task completion source has all the information needed.
            if (_loginTaskCompletionSrc == null ||
                _loginTaskCompletionSrc.Task == null ||
                _loginTaskCompletionSrc.Task.AsyncState == null)
            {
                return;
            }

            try
            {
                // Get the associated CredentialRequestInfo (will need the URI of the service being accessed).
                CredentialRequestInfo requestInfo = _loginTaskCompletionSrc.Task.AsyncState as CredentialRequestInfo;

                // Create a new network credential using the values entered by the user.
                var netCred = new System.Net.NetworkCredential(e.Username, e.Password, e.Domain);

                // Create a new ArcGIS network credential to hold the network credential and service URI.
                var arcgisCred = new ArcGISNetworkCredential(requestInfo.ServiceUri, netCred);

                // Set the task completion source result with the ArcGIS network credential.
                // AuthenticationManager is waiting for this result and will add it to its Credentials collection.
                _loginTaskCompletionSrc.TrySetResult(arcgisCred);
            }
            catch (Exception ex)
            {
                // Unable to create credential, set the exception on the task completion source.
                _loginTaskCompletionSrc.TrySetException(ex);
            }
            finally
            {
                // Get rid of the login controls.
                _loginUI.Hide();
                _loginUI = null;
            }
        }

        private void LoginCanceled(object sender, EventArgs e)
        {
            // Remove the login UI.
            _loginUI.Hide();
            _loginUI = null;

            // Cancel the task completion source task.
            _loginTaskCompletionSrc.TrySetCanceled();
        }
    }

    // View containing login controls (username, password, and domain).
    public class LoginOverlay : UIView
    {
        // Event to provide login information when the user dismisses the view.
        public event EventHandler<LoginEventArgs> OnLoginInfoEntered;

        // Event to report that the login was canceled.
        public event EventHandler OnCanceled;

        // Store the username, password, and domain controls so the values can be read.
        private UITextField _usernameTextField;
        private UITextField _passwordTextField;
        private UITextField _domainTextField;

        public LoginOverlay(CGRect frame, nfloat transparency, UIColor color) : base(frame)
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

            // Get the total height and width of the control set (four rows of controls, three sets of space).
            nfloat totalHeight = (4 * controlHeight) + (3 * rowSpace);
            nfloat totalWidth = textViewWidth;

            // Find the center x and y of the view.
            nfloat centerX = Frame.Width / 2;
            nfloat centerY = Frame.Height / 2;

            // Find the start x and y for the control layout.
            nfloat controlX = centerX - (totalWidth / 2);
            nfloat controlY = centerY - (totalHeight / 2);

            // Username text input.
            _usernameTextField = new UITextField(new CGRect(controlX, controlY, textViewWidth, controlHeight))
            {
                Placeholder = "Username",
                AutocapitalizationType = UITextAutocapitalizationType.None,
                BackgroundColor = ApplicationTheme.BackgroundColor,
                TextColor = ApplicationTheme.ForegroundColor
            };

            // Adjust the Y position for the next control.
            controlY = controlY + controlHeight + rowSpace;

            // Password text input.
            _passwordTextField = new UITextField(new CGRect(controlX, controlY, textViewWidth, controlHeight))
            {
                SecureTextEntry = true,
                Placeholder = "Password",
                AutocapitalizationType = UITextAutocapitalizationType.None,
                BackgroundColor = ApplicationTheme.BackgroundColor,
                TextColor = ApplicationTheme.ForegroundColor
            };

            // Adjust the Y position for the next control.
            controlY = controlY + controlHeight + rowSpace;

            // Domain text input.
            _domainTextField = new UITextField(new CGRect(controlX, controlY, textViewWidth, controlHeight))
            {
                Placeholder = "Domain",
                AutocapitalizationType = UITextAutocapitalizationType.None,
                BackgroundColor = ApplicationTheme.BackgroundColor,
                TextColor = ApplicationTheme.ForegroundColor
            };

            // Adjust the Y position for the next control.
            controlY = controlY + controlHeight + rowSpace;

            // Button to submit the login information.
            UIButton loginButton = new UIButton(new CGRect(controlX, controlY, buttonWidth, controlHeight));
            loginButton.SetTitle("Login", UIControlState.Normal);
            loginButton.SetTitleColor(ApplicationTheme.BackgroundColor, UIControlState.Normal);
            loginButton.TouchUpInside += LoginButtonClick;

            // Adjust the X position for the next control.
            controlX = controlX + buttonWidth + buttonSpace;

            // Button to cancel the login.
            UIButton cancelButton = new UIButton(new CGRect(controlX, controlY, buttonWidth, controlHeight));
            cancelButton.SetTitle("Cancel", UIControlState.Normal);
            cancelButton.SetTitleColor(ApplicationTheme.BackgroundColor, UIControlState.Normal);
            cancelButton.TouchUpInside += (s, e) => { OnCanceled.Invoke(this, null); };

            // Add the controls.
            AddSubviews(_usernameTextField, _passwordTextField, _domainTextField, loginButton, cancelButton);
        }

        // Animate increasing transparency to completely hide the view, then remove it.
        public void Hide()
        {
            // Action to make the view transparent.
            Action makeTransparentAction = () => Alpha = 0;

            // Action to remove the view.
            Action removeViewAction = RemoveFromSuperview;

            // Time to complete the animation (seconds).
            const double secondsToComplete = 0.75;

            // Animate transparency to zero, then remove the view.
            Animate(secondsToComplete, makeTransparentAction, removeViewAction);
        }

        private void LoginButtonClick(object sender, EventArgs e)
        {
            // Get the values entered in the text fields.
            string username = _usernameTextField.Text.Trim();
            string password = _passwordTextField.Text.Trim();
            string domain = _domainTextField.Text.Trim();

            // Make sure the user entered all values.
            if (string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(domain))
            {
                new UIAlertView("Login", "Please enter a username, password, and domain", null, "OK", null).Show();
                return;
            }

            // Fire the OnLoginInfoEntered event and provide the login values.
            if (OnLoginInfoEntered != null)
            {
                // Create a new LoginEventArgs to contain the user's values.
                var loginEventArgs = new LoginEventArgs(username, password, domain);

                // Raise the event.
                OnLoginInfoEntered(sender, loginEventArgs);
            }
        }
    }

    // Custom EventArgs implementation to hold login information (username, password, and domain).
    public class LoginEventArgs : EventArgs
    {
        // Username property.
        public string Username { get; set; }

        // Password property.
        public string Password { get; set; }

        // Domain property.
        public string Domain { get; set; }

        // Store login values passed into the constructor.
        public LoginEventArgs(string username, string password, string domain)
        {
            Username = username;
            Password = password;
            Domain = domain;
        }
    }

    // Table view data source that manages a list of web map portal items.
    public class PortalItemListSource : UITableViewSource
    {
        // Event to provide the selected portal item when the user taps a row.
        public event EventHandler<WebMapSelectedEventArgs> OnWebMapSelected;

        // List of portal items to display.
        private readonly List<PortalItem> _webMapItemsList;

        // Used when re-using cells to ensure that a cell of the right type is used.
        private const string CellId = "TableCell";

        public PortalItemListSource(List<PortalItem> items)
        {
            // Set the items.
            _webMapItemsList = items;
        }

        // This method gets a table view cell for the specified index.
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Try to get a re-usable cell (this is for performance). If there are no cells, create a new one.
            UITableViewCell cell = tableView.DequeueReusableCell(CellId);
            if (cell == null)
            {
                cell = new UITableViewCell(UITableViewCellStyle.Default, CellId);
            }

            // Specify the text for the cell.
            cell.TextLabel.Text = _webMapItemsList[indexPath.Row].Title;

            // Ensure that the label fits.
            cell.TextLabel.AdjustsFontSizeToFitWidth = true;

            // Return the cell.
            return cell;
        }

        // This method allows the UITableView to know how many rows to render.
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _webMapItemsList.Count;
        }

        // Called when a row is tapped.
        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            // Get the portal item for the selected row.
            PortalItem webMapItem = _webMapItemsList[indexPath.Row];

            // Raise the selection event with an argument containing the selected portal item.
            var webMapSelectedArgs = new WebMapSelectedEventArgs(webMapItem);
            OnWebMapSelected(this, webMapSelectedArgs);
        }
    }

    // Custom EventArgs implementation to hold the selected web map portal item.
    public class WebMapSelectedEventArgs : EventArgs
    {
        // The selected web map portal item.
        public PortalItem SelectedWebMapItem { get; set; }

        // Take the portal item in the constructor.
        public WebMapSelectedEventArgs(PortalItem webMapItem)
        {
            SelectedWebMapItem = webMapItem;
        }
    }
}