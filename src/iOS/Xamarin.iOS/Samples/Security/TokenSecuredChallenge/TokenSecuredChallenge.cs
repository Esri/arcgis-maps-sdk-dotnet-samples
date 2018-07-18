﻿// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using CoreGraphics;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.TokenSecuredChallenge
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
           "ArcGIS token challenge",
           "Security",
           "This sample demonstrates how to authenticate with ArcGIS Server using ArcGIS Tokens to access a secure service. Accessing secured services requires a login that's been defined on the server.",
           "1. When you run the sample, the app will load a map that contains a layer from a secured service.\n2. You will be challenged for a user name and password to view that layer.\n3. Enter the correct user name (user1) and password (user1).\n4. If you authenticate successfully, the secured layer will display, otherwise the map will contain only the public layers.",
           "Authentication, Security, ArcGIS Token")]
    [Register("TokenSecuredChallenge")]
    public class TokenSecuredChallenge : UIViewController
    {
        // Public and secured map service URLs.
        private string _publicMapServiceUrl = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer";
        private string _secureMapServiceUrl = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA_secure_user1/MapServer";

        // Public and secured layer names.
        private string _publicLayerName = "World Street Map - Public";
        private string _secureLayerName = "USA - Secure";

        // Use a TaskCompletionSource to store the result of a login task.
        private TaskCompletionSource<Credential> _loginTaskCompletionSource;

        // Store the map view displayed in the app.
        private MapView _myMapView;

        // Labels to show layer load status.
        private UILabel _publicLayerLabel;
        private UILabel _secureLayerLabel;

        // View containing login controls to display over the map view.
        private LoginOverlay _loginUI;

        public TokenSecuredChallenge()
        {
            Title = "Token Challenge";
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
            base.ViewDidLayoutSubviews();

            // Setup the visual frame for the MapView and status labels.
            CGRect mapViewFrame = new CGRect(0, 120, View.Bounds.Width, View.Bounds.Height - 120);
            CGRect label1Frame = new CGRect(10, 70, View.Bounds.Width - 10, 20);
            CGRect label2Frame = new CGRect(10, 95, View.Bounds.Width - 10, 20);

            // Apply the layout frames.
            _myMapView.Frame = mapViewFrame;
            _publicLayerLabel.Frame = label1Frame;
            _secureLayerLabel.Frame = label2Frame;
        }

        private void CreateLayout()
        {
            // Create a label for showing the load status for the public service.
            _publicLayerLabel = new UILabel()
            {
                TextColor = UIColor.Gray,
                Text = _publicLayerName,
                Font = _publicLayerLabel.Font.WithSize(12)
            };

            // Create a label to show the load status of the secured layer.
            _secureLayerLabel = new UILabel()
            {
                TextColor = UIColor.Gray,
                Text = _secureLayerName,
                Font = _secureLayerLabel.Font.WithSize(12)
            };
            
            // Create the map view control.
            _myMapView = new MapView();

            // Add the map view and button to the page.
            View.AddSubviews(_publicLayerLabel, _secureLayerLabel, _myMapView);
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

            // Get the label for this layer.
            UILabel labelToUpdate = null;
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
            UIColor textColor = UIColor.Gray;

            switch (e.Status)
            {
                case Esri.ArcGISRuntime.LoadStatus.FailedToLoad:
                    updateText = layer.Name + " (Load failed)";
                    textColor = UIColor.Red;
                    break;
                case Esri.ArcGISRuntime.LoadStatus.Loaded:
                    updateText = layer.Name + " (Loaded)";
                    textColor = UIColor.Green;
                    break;
                case Esri.ArcGISRuntime.LoadStatus.Loading:
                    updateText = layer.Name + " (Loading ...)";
                    textColor = UIColor.Gray;
                    break;
                case Esri.ArcGISRuntime.LoadStatus.NotLoaded:
                    updateText = layer.Name + " (Not loaded)";
                    textColor = UIColor.LightGray;
                    break;
            }

            // Update the layer label on the UI thread.
            this.BeginInvokeOnMainThread(() =>
            {
                labelToUpdate.Text = updateText;
                labelToUpdate.TextColor = textColor;
            });
        }

        // AuthenticationManager.ChallengeHandler function that prompts the user for login information to create a credential.
        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // Return if authentication is already in process.
            if (_loginTaskCompletionSource != null && !_loginTaskCompletionSource.Task.IsCanceled) { return null; }

            // Create a new TaskCompletionSource for the login operation.
            // Passing the CredentialRequestInfo object to the constructor will make it available from its AsyncState property.
            _loginTaskCompletionSource = new TaskCompletionSource<Credential>(info);

            // Show the login controls on the UI thread.
            // OnLoginInfoEntered event will return the values entered (username and password).
            InvokeOnMainThread(() => ShowLoginUI());

            // Return the login task, the result will be ready when completed (user provides login info and clicks the "Login" button).
            return await _loginTaskCompletionSource.Task;
        }

        private void ShowLoginUI()
        {
            // Get the URL for the service being requested.
            CredentialRequestInfo info = (CredentialRequestInfo)_loginTaskCompletionSource.Task.AsyncState;
            string serviceUrl = info.ServiceUri.GetLeftPart(UriPartial.Path);

            // Create a view to show login controls over the map view.
            CGRect ovBounds = new CGRect(0, 80, _myMapView.Bounds.Width, _myMapView.Bounds.Height - 80);
            _loginUI = new LoginOverlay(ovBounds, 0.85f, UIColor.DarkGray, serviceUrl);

            // Handle the login event to get the login entered by the user.
            _loginUI.OnLoginInfoEntered += LoginEntered;

            // Handle the cancel event when the user closes the dialog without entering a login.
            _loginUI.OnCanceled += LoginCanceled;

            // Add the login UI view (will display semi-transparent over the map view)
            View.Add(_loginUI);
        }

        // Handle the OnLoginEntered event from the login UI.
        // LoginEventArgs contains the username and password that were entered.
        private async void LoginEntered(object sender, LoginEventArgs e)
        {
            // Make sure the task completion source has all the information needed.
            if (_loginTaskCompletionSource == null ||
                _loginTaskCompletionSource.Task == null ||
                _loginTaskCompletionSource.Task.AsyncState == null)
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
                // Unable to create credential, set the exception on the task completion source.
                _loginTaskCompletionSource.TrySetException(ex);
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
            _loginTaskCompletionSource.TrySetCanceled();
        }
    }

    // View containing login entries (username and password).
    public class LoginOverlay : UIView
    {
        // Event to provide login information when the user dismisses the view.
        public event EventHandler<LoginEventArgs> OnLoginInfoEntered;

        // Event to report that the login was canceled.
        public event EventHandler OnCanceled;

        // Store the username and password so the values can be read.
        private UITextField _usernameTextField;
        private UITextField _passwordTextField;

        public LoginOverlay(CGRect frame, nfloat transparency, UIColor color, string url) : base(frame)
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

            // Get the total height and width of the control set (five rows of controls, four sets of space).
            nfloat totalHeight = (5 * controlHeight) + (4 * rowSpace);
            nfloat totalWidth = textViewWidth;

            // Find the center x and y of the view.
            nfloat centerX = Frame.Width / 2;
            nfloat centerY = Frame.Height / 2;

            // Find the start x and y for the control layout.
            nfloat controlX = centerX - totalWidth / 2;
            nfloat controlY = centerY - totalHeight / 2;

            // Set a title.
            UILabel titleTextBlock = new UILabel(new CGRect(controlX, controlY, textViewWidth, controlHeight))
            {
                Text = "Login to:"
            };

            // Adjust the Y position for the next control.
            controlY = controlY + controlHeight + rowSpace;

            // Service URL for which the user is logging in.
            UILabel urlTextBlock = new UILabel(new CGRect(controlX, controlY, textViewWidth, controlHeight))
            {
                Text = url,
                TextColor = UIColor.Blue,
                Lines = 2,
                LineBreakMode = UILineBreakMode.CharacterWrap
            };
            urlTextBlock.Font = urlTextBlock.Font.WithSize(10);

            // Adjust the Y position for the next control.
            controlY = controlY + controlHeight + rowSpace;

            // Username text input.
            _usernameTextField = new UITextField(new CGRect(controlX, controlY, textViewWidth, controlHeight))
            {
                Placeholder = "Username = user1",
                AutocapitalizationType = UITextAutocapitalizationType.None,
                BackgroundColor = UIColor.LightGray
            };

            // Adjust the Y position for the next control.
            controlY = controlY + controlHeight + rowSpace;

            // Password text input
            _passwordTextField = new UITextField(new CGRect(controlX, controlY, textViewWidth, controlHeight))
            {
                SecureTextEntry = true,
                Placeholder = "Password = user1",
                AutocapitalizationType = UITextAutocapitalizationType.None,
                BackgroundColor = UIColor.LightGray
            };

            // Adjust the Y position for the next control.
            controlY = controlY + controlHeight + rowSpace;

            // Button to submit the login information.
            UIButton loginButton = new UIButton(new CGRect(controlX, controlY, buttonWidth, controlHeight));
            loginButton.SetTitle("Login", UIControlState.Normal);
            loginButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            loginButton.TouchUpInside += LoginButtonClick;

            // Adjust the X position for the next control.
            controlX = controlX + buttonWidth + buttonSpace;

            // Button to cancel the login.
            UIButton cancelButton = new UIButton(new CGRect(controlX, controlY, buttonWidth, controlHeight));
            cancelButton.SetTitle("Cancel", UIControlState.Normal);
            cancelButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            cancelButton.TouchUpInside += (s, e) => { OnCanceled?.Invoke(this, null); };

            // Add the controls.
            AddSubviews(titleTextBlock, urlTextBlock, _usernameTextField, _passwordTextField, loginButton, cancelButton);
        }

        // Animate increasing transparency to completely hide the view, then remove it.
        public void Hide()
        {
            // Action to make the view transparent.
            Action makeTransparentAction = () => Alpha = 0;

            // Action to remove the view.
            Action removeViewAction = () => RemoveFromSuperview();

            // Time to complete the animation (seconds).
            double secondsToComplete = 0.75;

            // Animate transparency to zero, then remove the view.
            Animate(secondsToComplete, makeTransparentAction, removeViewAction);
        }

        private void LoginButtonClick(object sender, EventArgs e)
        {
            // Get the values entered in the text fields.
            string username = _usernameTextField.Text.Trim();
            string password = _passwordTextField.Text.Trim();

            // Make sure the user entered all values.
            if (String.IsNullOrEmpty(username) ||
                String.IsNullOrEmpty(password))
            {
                new UIAlertView("Login", "Please enter a username and password", (IUIAlertViewDelegate)null, "OK", null).Show();
                return;
            }

            // Fire the OnLoginInfoEntered event and provide the login values.
            if (OnLoginInfoEntered != null)
            {
                // Create a new LoginEventArgs to contain the user's values.
                LoginEventArgs loginEventArgs = new LoginEventArgs(username, password);

                // Raise the event.
                OnLoginInfoEntered(sender, loginEventArgs);
            }
        }
    }

    // Custom EventArgs implementation to hold login information (username and password).
    public class LoginEventArgs : EventArgs
    {
        // User name property.
        public string Username { get; set; }

        // Password property.
        public string Password { get; set; }

        // Store login values passed into the constructor.
        public LoginEventArgs(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}