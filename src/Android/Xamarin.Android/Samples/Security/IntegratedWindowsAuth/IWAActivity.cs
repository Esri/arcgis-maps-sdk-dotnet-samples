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
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Threading.Tasks;

namespace IntegratedWindowsAuth
{
    [Activity(Label = "IntegratedWindowsAuth", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        //TODO - Add the URL for your IWA-secured portal
        const string SecuredPortalUrl = "https://my.secure.server.com/gis/sharing";

        //TODO - Add the ID for a web map item stored on the secure portal 
        const string WebMapId = "";

        //TODO [optional] - Add hard-coded account information (if present, a network credential will be created on app initialize)
        const string NetworkUsername = "";
        const string NetworkPassword = "";
        const string NetworkDomain = "";

        // Use a TaskCompletionSource to store the result of a login task
        TaskCompletionSource<Credential> _loginTaskCompletionSrc;

        // Store the map view displayed in the app
        MapView _myMapView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            // Call a function to create the UI
            CreateLayout();

            // Call a function to initialize the app
            Initialize();
        }

        private void CreateLayout()
        {
            // Create a simple UI that contains a map view and a button
            var mainLayout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Show the imagery basemap in the map view initially
            var map = new Map(Basemap.CreateImagery());
            _myMapView = new MapView();
            _myMapView.Map = map;

            // Create a button to load a web map and set a click event handler
            Button loadMapButton = new Button(this);
            loadMapButton.Text = "Load secure map";
            loadMapButton.Click += LoadSecureMap;            

            // Add the elements to the layout 
            mainLayout.AddView(loadMapButton);
            mainLayout.AddView(_myMapView);

            // Apply the layout to the app
            SetContentView(mainLayout);
        }

        private void Initialize()
        {
            // Define a challenge handler method for the AuthenticationManager 
            // (this method handles getting credentials when a secured resource is encountered)
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Check for hard-coded user name, password, and domain values
            if (!string.IsNullOrEmpty(NetworkUsername) &&
                !string.IsNullOrEmpty(NetworkPassword) &&
                !string.IsNullOrEmpty(NetworkDomain))
            {
                // Create a hard-coded network credential
                ArcGISNetworkCredential hardcodedCredential = new ArcGISNetworkCredential
                {
                    Credentials = new System.Net.NetworkCredential(NetworkUsername, NetworkPassword, NetworkDomain),
                    ServiceUri = new Uri(SecuredPortalUrl)
                };

                // Add the credential to the AuthenticationManager
                AuthenticationManager.Current.AddCredential(hardcodedCredential);
            }
        }

        // Connect to the portal identified by the SecuredPortalUrl variable and load the web map identified by WebMapId
        private async void LoadSecureMap(object s, EventArgs e)
        {
            // Store messages that describe success or errors connecting to the secured portal and opening the web map
            var messageBuilder = new System.Text.StringBuilder();            

            try
            {
                // See if a credential exists for this portal in the AuthenticationManager
                // If a credential is not found, the user will be prompted for login info
                CredentialRequestInfo info = new CredentialRequestInfo
                {
                    ServiceUri = new Uri(SecuredPortalUrl),
                    AuthenticationType = AuthenticationType.NetworkCredential
                };
                Credential cred = await AuthenticationManager.Current.GetCredentialAsync(info, false);

                // Create an instance of the IWA-secured portal
                ArcGISPortal iwaSecuredPortal = await ArcGISPortal.CreateAsync(new Uri(SecuredPortalUrl));

                // Report a successful connection
                messageBuilder.AppendLine("Connected to the portal on " + iwaSecuredPortal.Uri.Host);         

                // Report the username for this connection
                if (iwaSecuredPortal.User != null)
                {
                    messageBuilder.AppendLine("Connected as: " + iwaSecuredPortal.User.UserName);
                }
                else
                {
                    // This shouldn't happen (if the portal is truly secured)!
                    messageBuilder.AppendLine("Connected anonymously"); 
                }

                // Get the web map (portal item) to display                
                var webMap = await PortalItem.CreateAsync(iwaSecuredPortal, WebMapId);
                if (webMap != null)
                {
                    // Create a new map from the portal item and display it in the map view
                    var map = new Map(webMap);
                    _myMapView.Map = map;
                }
            }
            catch (Exception ex)
            {
                // Report error
                messageBuilder.AppendLine("**-Exception: " + ex.Message);
            }
            finally
            {
                // Show an alert dialog with the status messages
                var alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("Status");
                alertBuilder.SetMessage(messageBuilder.ToString());
                alertBuilder.Show();
            }
        }

        // AuthenticationManager.ChallengeHandler function that prompts the user for login information to create a credential
        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // See if authentication is already in process
            if (_loginTaskCompletionSrc != null) { return null; }

            // Create a new TaskCompletionSource for the login operation
            // (passing the CredentialRequestInfo object to the constructor will make it available from its AsyncState property)
            _loginTaskCompletionSrc = new TaskCompletionSource<Credential>(info);

            // Create a dialog (fragment) with login controls
            LoginDialogFragment enterLoginDialog = new LoginDialogFragment();

            // Handle the login and the cancel events
            enterLoginDialog.OnLoginClicked += LoginClicked;
            enterLoginDialog.OnLoginCanceled += (s, e) =>
            {
                _loginTaskCompletionSrc.TrySetCanceled();
                _loginTaskCompletionSrc = null;
            };

            // Begin a transaction to show a UI fragment (the login dialog)
            FragmentTransaction transax = FragmentManager.BeginTransaction();
            enterLoginDialog.Show(transax, "login");

            // Return the login task, the result will be ready when completed (user provides login info and clicks the "Login" button)
            return await _loginTaskCompletionSrc.Task;
        }

        // Handler for the OnLoginClicked event defined in the LoginDialogFragment
        // OnEnterCredentialsEventArgs contains the username, password, and domain the user entered
        private void LoginClicked(object sender, OnEnterCredentialsEventArgs e)
        {
            // If no login information is available from the Task, return
            if (_loginTaskCompletionSrc == null || _loginTaskCompletionSrc.Task == null || _loginTaskCompletionSrc.Task.AsyncState == null)
            {
                return;
            }

            // Get the CredentialRequestInfo object that was stored with the task
            var credRequestInfo = _loginTaskCompletionSrc.Task.AsyncState as CredentialRequestInfo;

            try
            {
                // Create a new System.Net.NetworkCredential with the user name, password, and domain provided
                var networkCredential = new System.Net.NetworkCredential(e.Username, e.Password, e.Domain);

                // Create a new ArcGISNetworkCredential with the NetworkCredential and URI of the secured resource
                var credential = new ArcGISNetworkCredential
                {
                    Credentials = networkCredential,
                    ServiceUri = credRequestInfo.ServiceUri
                };
                
                // Set the result of the login task with the new ArcGISNetworkCredential
                _loginTaskCompletionSrc.TrySetResult(credential);
            }
            catch (Exception ex)
            {
                _loginTaskCompletionSrc.TrySetException(ex);
            }
            finally
            {
                // Set the task completion source to null to indicate authentication is complete
                _loginTaskCompletionSrc = null;
            }
        }
    }
    
    // Custom DialogFragment class to show input controls for providing network login information (username, password, domain)
    public class LoginDialogFragment : DialogFragment
    {
        // Login entries for the user to complete
        private EditText _usernameTextbox;
        private EditText _passwordTextbox;
        private EditText _domainTextbox;

        // Event raised when the login button is clicked
        public event EventHandler<OnEnterCredentialsEventArgs> OnLoginClicked;

        // Event raised when the login is canceled (Cancel button is clicked)
        public event EventHandler<EventArgs> OnLoginCanceled;

        // Override OnCreateView to create the dialog controls
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var ctx = this.Activity.ApplicationContext;

            // The container for the dialog is a vertical linear layout
            LinearLayout dialogView = new LinearLayout(ctx){ Orientation = Orientation.Vertical};

            // Add a text box for entering a username
            _usernameTextbox = new EditText(ctx);
            _usernameTextbox.Hint = "Username";
            dialogView.AddView(_usernameTextbox);

            // Add a text box for entering a password
            _passwordTextbox = new EditText(ctx);
            _passwordTextbox.Hint = "Password";
            _passwordTextbox.InputType = Android.Text.InputTypes.TextVariationPassword | Android.Text.InputTypes.ClassText;
            dialogView.AddView(_passwordTextbox);

            // Add a text box for entering the domain
            _domainTextbox = new EditText(ctx);
            _domainTextbox.Hint = "Domain";
            dialogView.AddView(_domainTextbox);

            // Use a horizontal layout for the two buttons (login and cancel)
            LinearLayout buttonsRow = new LinearLayout(ctx) { Orientation = Orientation.Horizontal };

            // Create a button to login with these credentials
            Button loginButton = new Button(ctx);
            loginButton.Text = "Login";
            loginButton.Click += LoginButtonClick;
            buttonsRow.AddView(loginButton);

            // Create a button to cancel
            Button cancelButton = new Button(ctx);
            cancelButton.Text = "Cancel";
            cancelButton.Click += CancelButtonClick;
            buttonsRow.AddView(cancelButton);

            dialogView.AddView(buttonsRow);

            // Return the new view for display
            return dialogView;
        }

        // Click handler for the login button
        private void LoginButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Get information for the login
                var username = _usernameTextbox.Text;
                var password = _passwordTextbox.Text;
                var domain = _domainTextbox.Text;

                // Make sure all required info was entered
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(domain))
                {
                    throw new Exception("Please enter a username, password, and domain.");
                }

                // Create a new OnEnterCredentialsEventArgs object to store the information entered by the user
                var credentialsEnteredArgs = new OnEnterCredentialsEventArgs(username, password, domain);

                // Raise the OnLoginClicked event so the main activity can handle the event and try to authenticate with the credentials
                OnLoginClicked(this, credentialsEnteredArgs);

                // Close the dialog
                this.Dismiss();
            }
            catch (Exception ex)
            {
                // Show the exception message (dialog will stay open so user can try again)
                var alertBuilder = new AlertDialog.Builder(this.Activity);
                alertBuilder.SetTitle("Error");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }
        }

        // Click handler for the cancel button
        private void CancelButtonClick(object sender, EventArgs e)
        {
            // Raise an event to indicate that the login was canceled
            OnLoginCanceled(this, e);

            // Close the dialog
            this.Dismiss();
        }
    }

    // Custom EventArgs class for containing login info
    public class OnEnterCredentialsEventArgs : EventArgs
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }

        // Constructor gets username, password, and domain and stores them in properties
        public OnEnterCredentialsEventArgs(string username, string password, string domain) : base()
        {
            Username = username;
            Password = password;
            Domain = domain;
        }
    }

}

