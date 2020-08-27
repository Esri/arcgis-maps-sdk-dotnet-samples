// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Threading.Tasks;
using ContextThemeWrapper = AndroidX.AppCompat.View.ContextThemeWrapper;

namespace ArcGISRuntimeXamarin.Samples.TokenSecuredChallenge
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "ArcGIS token challenge",
        category: "Security",
        description: "This sample demonstrates how to prompt the user for a username and password to authenticate with ArcGIS Server to access an ArcGIS token-secured service. Accessing secured services requires a login that's been defined on the server.",
        instructions: "When you run the sample, the app will load a map that contains a layer from a secured service. Then, you will be challenged for a user name and password to view that layer. Enter the correct user name (user1) and password (user1). If you authenticate successfully, the secured layer will display, otherwise the map will contain only the public layers.",
        tags: new[] { "authentication", "cloud", "portal", "remember", "security" })]
    public class TokenSecuredChallenge : Activity
    {
        // Public and secured map service URLs.
        private string _publicMapServiceUrl = "https://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer";
        private string _secureMapServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA_secure_user1/MapServer";

        // Public and secured layer names.
        private string _publicLayerName = "World Street Map - Public";
        private string _secureLayerName = "USA - Secure";

        // Use a TaskCompletionSource to store the result of a login task.
        private TaskCompletionSource<Credential> _loginTaskCompletionSource;

        // Hold a reference to the map view.
        private MapView _myMapView;

        // Labels to show layer load status.
        private TextView _publicLayerLabel;
        private TextView _secureLayerLabel;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Token challenge";

            // Call a function to create the user interface.
            CreateLayout();

            // Call a function to initialize the app.
            Initialize();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a label for showing the load status for the public service.
            _publicLayerLabel = new TextView(this)
            {
                Text = _publicLayerName,
                TextSize = 12
            };
            _publicLayerLabel.SetTextColor(Color.Gray);
            layout.AddView(_publicLayerLabel);

            // Create a label to show the load status of the secured layer.
            _secureLayerLabel = new TextView(this)
            {
                Text = _secureLayerName,
                TextSize = 12
            };
            _secureLayerLabel.SetTextColor(Color.Gray);
            layout.AddView(_secureLayerLabel);

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }

        private void Initialize()
        {
            // Define a challenge handler method for the AuthenticationManager.
            // This method handles getting credentials when a secured resource is encountered.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Create the public layer and provide a name.
            ArcGISTiledLayer publicLayer = new ArcGISTiledLayer(new Uri(_publicMapServiceUrl))
            {
                Name = _publicLayerName
            };

            // Create the secured layer and provide a name.
            ArcGISMapImageLayer tokenSecuredLayer = new ArcGISMapImageLayer(new Uri(_secureMapServiceUrl))
            {
                Name = _secureLayerName
            };

            // Track the load status of each layer with a LoadStatusChangedEvent handler.
            publicLayer.LoadStatusChanged += LayerLoadStatusChanged;
            tokenSecuredLayer.LoadStatusChanged += LayerLoadStatusChanged;

            // Create a new map and add the layers.
            Map myMap = new Map();
            myMap.OperationalLayers.Add(publicLayer);
            myMap.OperationalLayers.Add(tokenSecuredLayer);

            // Add the map to the map view.
            _myMapView.Map = myMap;
        }

        // Handle the load status changed event for the public and token-secured layers.
        private void LayerLoadStatusChanged(object sender, Esri.ArcGISRuntime.LoadStatusEventArgs e)
        {
            // Get the layer that triggered the event.
            Layer layer = (Layer)sender;

            // Get the label (TextView) for this layer.
            TextView labelToUpdate = null;
            if (layer.Name == _publicLayerName)
            {
                labelToUpdate = _publicLayerLabel;
            }
            else
            {
                labelToUpdate = _secureLayerLabel;
            }

            // Create the text string and font color to describe the current load status.
            string updateText = layer.Name;
            Color textColor = Color.Gray;

            switch (e.Status)
            {
                case Esri.ArcGISRuntime.LoadStatus.FailedToLoad:
                    updateText = layer.Name + " (Load failed)";
                    textColor = Color.Red;
                    break;

                case Esri.ArcGISRuntime.LoadStatus.Loaded:
                    updateText = layer.Name + " (Loaded)";
                    textColor = Color.Green;
                    break;

                case Esri.ArcGISRuntime.LoadStatus.Loading:
                    updateText = layer.Name + " (Loading ...)";
                    textColor = Color.Gray;
                    break;

                case Esri.ArcGISRuntime.LoadStatus.NotLoaded:
                    updateText = layer.Name + " (Not loaded)";
                    textColor = Color.LightGray;
                    break;
            }

            // Update the layer label on the UI thread.
            RunOnUiThread(() =>
            {
                labelToUpdate.Text = updateText;
                labelToUpdate.SetTextColor(textColor);
            });
        }

        // AuthenticationManager.ChallengeHandler function that prompts the user for login information to create a credential.
        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // See if authentication is already in process
            if (_loginTaskCompletionSource != null) { return null; }

            // Create a new TaskCompletionSource for the login operation.
            // Passing the CredentialRequestInfo object to the constructor will make it available from its AsyncState property.
            _loginTaskCompletionSource = new TaskCompletionSource<Credential>(info);

            // Create a dialog (fragment) with login controls.
            LoginDialogFragment enterLoginDialog = new LoginDialogFragment();

            // Handle the login and the cancel events.
            enterLoginDialog.OnLoginClicked += LoginClicked;
            enterLoginDialog.OnLoginCanceled += (s, e) =>
            {
                _loginTaskCompletionSource.TrySetCanceled();
                _loginTaskCompletionSource = null;
            };

            // Begin a transaction to show a UI fragment (the login dialog).
            FragmentTransaction transax = FragmentManager.BeginTransaction();
            enterLoginDialog.Show(transax, "login");

            // Return the login task, the result will be ready when completed (user provides login info and clicks the "Login" button).
            return await _loginTaskCompletionSource.Task;
        }

        // Handler for the OnLoginClicked event defined in the LoginDialogFragment.
        // OnEnterCredentialsEventArgs contains the username and password the user entered.
        private async void LoginClicked(object sender, OnEnterCredentialsEventArgs e)
        {
            // If no login information is available from the Task, return.
            if (_loginTaskCompletionSource == null || _loginTaskCompletionSource.Task == null || _loginTaskCompletionSource.Task.AsyncState == null)
            {
                return;
            }

            try
            {
                // Get the associated CredentialRequestInfo (will need the URI of the service being accessed).
                CredentialRequestInfo requestInfo = (CredentialRequestInfo)_loginTaskCompletionSource.Task.AsyncState;

                // Create a token credential using the provided username and password.
                TokenCredential userCredentials = await AuthenticationManager.Current.GenerateCredentialAsync
                                            (requestInfo.ServiceUri,
                                             e.Username,
                                             e.Password,
                                             requestInfo.GenerateTokenOptions);

                // Set the result on the task completion source.
                _loginTaskCompletionSource.TrySetResult(userCredentials);
            }
            catch (Exception ex)
            {
                _loginTaskCompletionSource.TrySetException(ex);
            }
            finally
            {
                // Set the task completion source to null to indicate authentication is complete.
                _loginTaskCompletionSource = null;
            }
        }
    }

    // Custom DialogFragment class to show input controls for providing login information (username and password).
    public class LoginDialogFragment : DialogFragment
    {
        // Login entries for the user to complete.
        private EditText _usernameTextbox;
        private EditText _passwordTextbox;

        // Event raised when the login button is clicked.
        public event EventHandler<OnEnterCredentialsEventArgs> OnLoginClicked;

        // Event raised when the login is canceled (Cancel button is clicked).
        public event EventHandler<EventArgs> OnLoginCanceled;

        // Override OnCreateView to create the dialog controls.
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            Context ctx = this.Activity.ApplicationContext;
            ContextThemeWrapper ctxWrapper = new ContextThemeWrapper(ctx, Android.Resource.Style.ThemeMaterialLight);

            // The container for the dialog is a vertical linear layout.
            LinearLayout dialogView = new LinearLayout(ctxWrapper) { Orientation = Orientation.Vertical };

            // Add a text box for entering a username.
            _usernameTextbox = new EditText(ctxWrapper)
            {
                Hint = "Username = user1"
            };
            dialogView.AddView(_usernameTextbox);

            // Add a text box for entering a password.
            _passwordTextbox = new EditText(ctxWrapper)
            {
                Hint = "Password = user1",
                InputType = Android.Text.InputTypes.TextVariationPassword | Android.Text.InputTypes.ClassText
            };
            dialogView.AddView(_passwordTextbox);

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
                string username = _usernameTextbox.Text;
                string password = _passwordTextbox.Text;

                // Make sure all required info was entered.
                if (String.IsNullOrEmpty(username) || String.IsNullOrEmpty(password))
                {
                    throw new Exception("Please enter a username and password.");
                }

                // Create a new OnEnterCredentialsEventArgs object to store the information entered by the user.
                OnEnterCredentialsEventArgs credentialsEnteredArgs = new OnEnterCredentialsEventArgs(username, password);

                // Raise the OnLoginClicked event so the main activity can handle the event and try to authenticate with the credentials.
                OnLoginClicked?.Invoke(this, credentialsEnteredArgs);

                // Close the dialog.
                this.Dismiss();
            }
            catch (Exception ex)
            {
                // Show the exception message (dialog will stay open so user can try again).
                AlertDialog.Builder alertBuilder = new AlertDialog.Builder(this.Activity);
                alertBuilder.SetTitle("Error");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }
        }

        // Click handler for the cancel button.
        private void CancelButtonClick(object sender, EventArgs e)
        {
            // Raise an event to indicate that the login was canceled.
            OnLoginCanceled?.Invoke(this, e);

            // Close the dialog.
            this.Dismiss();
        }
    }

    // Custom EventArgs class for containing login info.
    public class OnEnterCredentialsEventArgs : EventArgs
    {
        public string Username { get; set; }
        public string Password { get; set; }

        // Constructor gets username and password and stores them in properties.
        public OnEnterCredentialsEventArgs(string username, string password) : base()
        {
            Username = username;
            Password = password;
        }
    }
}