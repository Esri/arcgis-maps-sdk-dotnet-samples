// Copyright 2018 Esri.
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
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Support.V7.View;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UI.Controls;
using ContextThemeWrapper = Android.Support.V7.View.ContextThemeWrapper;

// *****************************************
// Important: Integrated Windows Authentication does not work with the AndroidClientHandler Http handler. 
// To use IWA successfully, change the project properties to use the Managed handler (HttpClientHandler).
// *****************************************
namespace ArcGISRuntimeXamarin.Samples.IntegratedWindowsAuth
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
           "Integrated Windows Authentication",
           "Security",
           "This sample demonstrates how to use a Windows login to authenticate with a portal that is secured with IWA.",
           "1. Enter the URL to your IWA-secured portal.\n2. Click the button to search for web maps on the secure portal.\n3. You will be prompted for a user name, password, and domain to authenticate with the portal.\n4. If you authenticate successfully, search results will display.",
           "Authentication, Security, Windows")]
    public class IntegratedWindowsAuth : Activity
    {
        // The ArcGIS Online URL for searching public web maps.
        private string _publicPortalUrl = "https://www.arcgis.com";

        // A TaskCompletionSource to store the result of a login task.
        TaskCompletionSource<Credential> _loginTaskCompletionSrc;

        // A map view to display a map in the app.
        MapView _myMapView;

        // Entry box for the iwa-secured portal.
        private EditText _securePortalEditText;

        // Label for messages.
        private TextView _messagesTextView;

        // Button for searching the public portal for web maps.
        private Button _searchPublicPortalButton;

        // Button for searching the private portal for web maps.
        private Button _searchSecurePortalButton;

        // List view to show web map items.
        private ListView _webMapListView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Title = "Integrated Windows Authentication";

            // Call a function to create the user interface.
            CreateLayout();

            // Call a function to initialize the app.
            Initialize();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create the entry for the secure portal URL.
            _securePortalEditText = new EditText(this) { Hint = "IWA-secured portal URL" };
            _securePortalEditText.InputType = InputTypes.TextVariationUri;

            // Hide the keyboard on enter.
            _securePortalEditText.KeyPress += (sender, args) =>
            {
                if (args.Event.Action == KeyEventActions.Down && args.KeyCode == Keycode.Enter)
                {
                    InputMethodManager imm = (InputMethodManager)GetSystemService(InputMethodService);
                    imm.HideSoftInputFromWindow(_securePortalEditText.WindowToken, 0);
                    SearchSecurePortalButton_Click(this, null);
                }
                else
                {
                    args.Handled = false;
                }
            };

            // A label to show errors and other messages.
            _messagesTextView = new TextView(this);

            // Buttons to search a public and private portal.
            _searchPublicPortalButton = new Button(this) { Text = "Search ArcGIS Online" };
            _searchSecurePortalButton = new Button(this) { Text = "Search secure" };

            // Event handlers to perform the appropriate search.
            _searchSecurePortalButton.Click += SearchSecurePortalButton_Click;
            _searchPublicPortalButton.Click += SearchPublicPortalButton_Click;

            // Add the buttons to a horizontal layout.
            LinearLayout buttonLayout = new LinearLayout(this) { Orientation = Orientation.Horizontal };
            buttonLayout.AddView(_searchPublicPortalButton);
            buttonLayout.AddView(_searchSecurePortalButton);

            // Create a list view to show web map item results.
            _webMapListView = new ListView(this)
            {
                ChoiceMode = ChoiceMode.Single
            };

            // Handle item click events to load the selected web map.
            _webMapListView.ItemClick += WebMapListView_ItemSelected;

            // Create a scroll view for the list.
            ScrollView listScroll = new ScrollView(this);
            listScroll.SetMinimumHeight(Resources.DisplayMetrics.HeightPixels / 5);
            listScroll.FillViewport = true;

            // Add the listview to the scroll view.
            listScroll.AddView(_webMapListView);

            // Add the controls to the layout.
            layout.AddView(_securePortalEditText);
            layout.AddView(buttonLayout);
            layout.AddView(listScroll);
            layout.AddView(_messagesTextView);
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }

        private void WebMapListView_ItemSelected(object sender, AdapterView.ItemClickEventArgs e)
        {
            // When a row in the list is selected, get the associated web map item.
            PortalItemAdapter itemAdapter = _webMapListView.Adapter as PortalItemAdapter;
            PortalItem selectedItem = itemAdapter[e.Position];

            // Create a new map from the portal item and display it in the map view.
            Map webMap = new Map(selectedItem);
            _myMapView.Map = webMap;
        }


        private void Initialize()
        {
            // Define a challenge handler method for the AuthenticationManager.
            // This method handles getting credentials when a secured resource is encountered.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Show a default map (light gray canvas).
            Map defaultMap = new Map(Basemap.CreateLightGrayCanvasVector())
            {
                InitialViewpoint = new Viewpoint(0.0, 0.0, 200000000)
            };
            _myMapView.Map = defaultMap;
        }


        async void SearchSecurePortalButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Clear the current results from the list.
                _webMapListView.Adapter = null;

                // Get the value entered for the secure portal URL.
                string securedPortalUrl = _securePortalEditText.Text.Trim();

                // Make sure a portal URL has been entered in the text box.
                if (string.IsNullOrEmpty(securedPortalUrl))
                {
                    _messagesTextView.Text = "Please enter the URL of the secured portal.";
                    return;
                }

                // Create an instance of the IWA-secured portal, the user may be challenged for access.
                var iwaSecuredPortal = await ArcGISPortal.CreateAsync(new Uri(securedPortalUrl), true);

                // Call a function to search the portal.
                SearchPortal(iwaSecuredPortal);

                // Report the username for this connection.
                if (iwaSecuredPortal.User != null)
                {
                    _messagesTextView.Text = "Connected as: " + iwaSecuredPortal.User.UserName;
                }
                else
                {
                    // This shouldn't happen (if the portal is truly secured)!
                    _messagesTextView.Text = "Connected anonymously";
                }
            }
            catch (TaskCanceledException)
            {
                // Report canceled login.
                _messagesTextView.Text = "Login was canceled";
            }
            catch (Exception ex)
            {
                // Report errors (connecting to the secured portal, for example).
                _messagesTextView.Text = ex.Message;
            }
            finally
            {
                // Set the task completion source to null so user can attempt another login (if it failed).
                _loginTaskCompletionSrc = null;
            }
        }

        private async void SearchPublicPortalButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Clear the current results from the list.
                _webMapListView.Adapter = null;

                // Create an instance of the public portal.
                var publicPortal = await ArcGISPortal.CreateAsync(new Uri(_publicPortalUrl));

                // Call a function to search the portal.
                SearchPortal(publicPortal);
            }
            catch (Exception ex)
            {
                // Report errors, if any.
                _messagesTextView.Text = ex.Message;
            }
        }


        private async void SearchPortal(ArcGISPortal currentPortal)
        {
            // Show status message.
            _messagesTextView.Text = "Searching for web map items on the portal at " + currentPortal.Uri.AbsoluteUri;
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

                // Build a list of items from the results.
                var resultItems = from r in items.Results select new KeyValuePair<string, PortalItem>(r.Title, r);

                // Add the items to a dictionary.
                List<PortalItem> webMapItems = new List<PortalItem>();
                foreach (var itm in resultItems)
                {
                    webMapItems.Add(itm.Value);
                }

                // Create an array adapter for the result list.
                PortalItemAdapter adapter = new PortalItemAdapter(this, webMapItems);

                // Apply the adapter to the list view to show the results.
                _webMapListView.Adapter = adapter;

            }
            catch (Exception ex)
            {
                // Report errors searching the portal.
                messageBuilder.AppendLine(ex.Message);
            }
            finally
            {
                // Show messages.
               _messagesTextView.Text = messageBuilder.ToString();
            }
        }

        // AuthenticationManager.ChallengeHandler function that prompts the user for login information to create a credential.
        private  Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // Ignore token or certificate challenges (needs additional code and UI).
            if(info.AuthenticationType != AuthenticationType.NetworkCredential)
            {
                Console.WriteLine("Skipped authentication for " + info.ServiceUri.Host);
                return null;
            }

            // See if authentication is already in progress.
            if (_loginTaskCompletionSrc != null) { return null; }

            // Create a new TaskCompletionSource for the login operation.
            // Passing the CredentialRequestInfo object to the constructor will make it available from its AsyncState property.
            _loginTaskCompletionSrc = new TaskCompletionSource<Credential>(info);

            // Create a dialog (fragment) with login controls.
            LoginDialogFragment enterLoginDialog = new LoginDialogFragment();

            // Handle the login and the cancel events.
            enterLoginDialog.OnLoginClicked += LoginClicked;
            enterLoginDialog.OnLoginCanceled += (s, e) =>
            {
                _loginTaskCompletionSrc?.TrySetCanceled();
                _loginTaskCompletionSrc = null;
            };

            // Begin a transaction to show a UI fragment (the login dialog).
            FragmentTransaction transax = FragmentManager.BeginTransaction();
            enterLoginDialog.Show(transax, "login");

            // Return the login task, the result will be ready when completed (user provides login info and clicks the "Login" button).
            return _loginTaskCompletionSrc.Task;
        }

        // Handler for the OnLoginClicked event defined in the LoginDialogFragment, OnEnterCredentialsEventArgs contains the username, password, and domain the user entered
        private void LoginClicked(object sender, OnEnterCredentialsEventArgs e)
        {
            // If no login information is available from the Task, return.
            if (_loginTaskCompletionSrc == null || _loginTaskCompletionSrc.Task == null || _loginTaskCompletionSrc.Task.AsyncState == null)
            {
                return;
            }

            // Get the CredentialRequestInfo object that was stored with the task.
            var credRequestInfo = _loginTaskCompletionSrc.Task.AsyncState as CredentialRequestInfo;

            try
            {
                // Create a new System.Net.NetworkCredential with the user name, password, and domain provided.
                var networkCredential = new System.Net.NetworkCredential(e.Username, e.Password, e.Domain);

                // Create a new ArcGISNetworkCredential with the NetworkCredential and URI of the secured resource.
                var credential = new ArcGISNetworkCredential
                {
                    Credentials = networkCredential,
                    ServiceUri = credRequestInfo.ServiceUri
                };

                // Set the result of the login task with the new ArcGISNetworkCredential.
                _loginTaskCompletionSrc.TrySetResult(credential);
            }
            catch (Exception ex)
            {
                _loginTaskCompletionSrc.TrySetException(ex);
            }
            finally
            {
                // Set the task completion source to null to indicate authentication is complete.
                _loginTaskCompletionSrc = null;
            }
        }
    }

    // Custom DialogFragment class to show input controls for providing network login information (username, password, domain).
    public class LoginDialogFragment : DialogFragment
    {
        // Login entries for the user to complete.
        private EditText _usernameTextbox;
        private EditText _passwordTextbox;
        private EditText _domainTextbox;

        // Event raised when the login button is clicked.
        public event EventHandler<OnEnterCredentialsEventArgs> OnLoginClicked;

        // Event raised when the login is canceled (Cancel button is clicked).
        public event EventHandler<EventArgs> OnLoginCanceled;

        // Override OnCreateView to create the dialog controls.
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var ctx = this.Activity.ApplicationContext;
            ContextThemeWrapper ctxWrapper = new ContextThemeWrapper(ctx, Android.Resource.Style.ThemeMaterialLight);

            // The container for the dialog is a vertical linear layout.
            LinearLayout dialogView = new LinearLayout(ctxWrapper) { Orientation = Orientation.Vertical };

            // Add a text box for entering a username.
            _usernameTextbox = new EditText(ctxWrapper)
            {
                Hint = "Username"
            };
            dialogView.AddView(_usernameTextbox);

            // Add a text box for entering a password.
            _passwordTextbox = new EditText(ctxWrapper)
            {
                Hint = "Password",
                InputType = Android.Text.InputTypes.TextVariationPassword | Android.Text.InputTypes.ClassText
            };
            dialogView.AddView(_passwordTextbox);

            // Add a text box for entering the domain.
            _domainTextbox = new EditText(ctxWrapper)
            {
                Hint = "Domain"
            };
            dialogView.AddView(_domainTextbox);

            // Use a horizontal layout for the two buttons (login and cancel).
            LinearLayout buttonsRow = new LinearLayout(ctxWrapper) { Orientation = Orientation.Horizontal };

            // Create a button to login with these credentials.
            Button loginButton = new Button(ctxWrapper)
            {
                Text = "Login"
            };
            loginButton.Click += LoginButtonClick;
            buttonsRow.AddView(loginButton);

            // Create a button to cancel.
            Button cancelButton = new Button(ctxWrapper)
            {
                Text = "Cancel"
            };
            cancelButton.Click += CancelButtonClick;
            buttonsRow.AddView(cancelButton);

            dialogView.AddView(buttonsRow);

            // Return the new view for display.
            return dialogView;
        }

        // Click handler for the login button.
        private void LoginButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Get information for the login.
                var username = _usernameTextbox.Text;
                var password = _passwordTextbox.Text;
                var domain = _domainTextbox.Text;

                // Make sure all required info was entered.
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(domain))
                {
                    throw new Exception("Please enter a username, password, and domain.");
                }

                // Create a new OnEnterCredentialsEventArgs object to store the information entered by the user.
                var credentialsEnteredArgs = new OnEnterCredentialsEventArgs(username, password, domain);

                // Raise the OnLoginClicked event so the main activity can handle the event and try to authenticate with the credentials.
                OnLoginClicked(this, credentialsEnteredArgs);

                // Close the dialog.
                this.Dismiss();
            }
            catch (Exception ex)
            {
                // Show the exception message (dialog will stay open so user can try again).
                var alertBuilder = new AlertDialog.Builder(this.Activity);
                alertBuilder.SetTitle("Error");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }
        }

        // Click handler for the cancel button.
        private void CancelButtonClick(object sender, EventArgs e)
        {
            // Raise an event to indicate that the login was canceled.
            OnLoginCanceled(this, e);

            // Close the dialog.
            this.Dismiss();
        }
    }

    // Custom EventArgs class for containing login info.
    public class OnEnterCredentialsEventArgs : EventArgs
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }

        // Constructor gets username, password, and domain and stores them in properties.
        public OnEnterCredentialsEventArgs(string username, string password, string domain) : base()
        {
            Username = username;
            Password = password;
            Domain = domain;
        }
    }

}

// A custom item adapter for showing a list of portal items.
public class PortalItemAdapter : BaseAdapter<PortalItem>
{
    // Store a list of the portal items displayed.
    List<PortalItem> _portalItems;

    // The current activity.
    Activity _context;

    // Take the list of portal items and the current activity in the constructor.
    public PortalItemAdapter(Activity context, List<PortalItem> items) : base()
    {
        _context = context;
        _portalItems = items;
    }

    // Get the ID of the item at a given position (just return the position).
    public override long GetItemId(int position)
    {
        return position;
    }

    // Get the portal item at a given position.
    public override PortalItem this[int position]
    {
        get { return _portalItems[position]; }
    }

    // Get the count of portal items in the list.
    public override int Count
    {
        get { return _portalItems.Count; }
    }

    // Construct a view to display a row in the table.
    public override View GetView(int position, View convertView, ViewGroup parent)
    {
        // Re-use an existing view, if one is available.
        View view = convertView;

        // Create a new view if necessary.
        if (view == null) { view = _context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem1, null); }

        // Set the text for the row with the portal item title.
        view.FindViewById<TextView>(Android.Resource.Id.Text1).Text = _portalItems[position].Title;

        // Return the view for this row.
        return view;
    }
}