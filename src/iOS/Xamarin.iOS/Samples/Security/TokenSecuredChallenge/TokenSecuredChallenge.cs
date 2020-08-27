// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
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
        name: "ArcGIS token challenge",
        category: "Security",
        description: "This sample demonstrates how to prompt the user for a username and password to authenticate with ArcGIS Server to access an ArcGIS token-secured service. Accessing secured services requires a login that's been defined on the server.",
        instructions: "When you run the sample, the app will load a map that contains a layer from a secured service. Then, you will be challenged for a user name and password to view that layer. Enter the correct user name (user1) and password (user1). If you authenticate successfully, the secured layer will display, otherwise the map will contain only the public layers.",
        tags: new[] { "authentication", "cloud", "portal", "remember", "security" })]
    [Register("TokenSecuredChallenge")]
    public class TokenSecuredChallenge : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        // Public and secured map service URLs.
        private const string PublicMapServiceUrl = "https://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer";
        private const string SecureMapServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA_secure_user1/MapServer";

        // Public and secured layer names.
        private const string PublicLayerName = "World Street Map - Public";
        private const string SecureLayerName = "USA - Secure";

        // Use a TaskCompletionSource to store the result of a login task.
        private TaskCompletionSource<Credential> _loginTaskCompletionSource;

        // Labels to show layer load status.
        private UILabel _publicLayerLabel;
        private UILabel _secureLayerLabel;

        // Hold references to the layers.
        ArcGISMapImageLayer _tokenSecuredLayer;
        ArcGISTiledLayer _publicLayer;

        public TokenSecuredChallenge()
        {
            Title = "Token Challenge";
        }

        private void Initialize()
        {
            // Define a challenge handler method for the AuthenticationManager.
            // This method handles getting credentials when a secured resource is encountered.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Create the public layer and provide a name.
            _publicLayer = new ArcGISTiledLayer(new Uri(PublicMapServiceUrl))
            {
                Name = PublicLayerName
            };

            // Create the secured layer and provide a name.
            _tokenSecuredLayer = new ArcGISMapImageLayer(new Uri(SecureMapServiceUrl))
            {
                Name = SecureLayerName
            };

            // Track the load status of each layer with a LoadStatusChangedEvent handler.
            _publicLayer.LoadStatusChanged += LayerLoadStatusChanged;
            _tokenSecuredLayer.LoadStatusChanged += LayerLoadStatusChanged;

            // Create a new map and add the layers.
            Map myMap = new Map();
            myMap.OperationalLayers.Add(_publicLayer);
            myMap.OperationalLayers.Add(_tokenSecuredLayer);

            // Add the map to the map view.
            _myMapView.Map = myMap;
        }

        // Handle the load status changed event for the public and token-secured layers.
        private void LayerLoadStatusChanged(object sender, Esri.ArcGISRuntime.LoadStatusEventArgs e)
        {
            // Get the layer that triggered the event.
            Layer layer = (Layer) sender;

            // Get the label for this layer.
            UILabel labelToUpdate;
            if (layer.Name == PublicLayerName)
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
            BeginInvokeOnMainThread(() =>
            {
                labelToUpdate.Text = updateText;
                labelToUpdate.TextColor = textColor;
            });
        }

        // AuthenticationManager.ChallengeHandler function that prompts the user for login information to create a credential.
        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // Return if authentication is already in process.
            if (_loginTaskCompletionSource != null && !_loginTaskCompletionSource.Task.IsCanceled)
            {
                return null;
            }

            // Create a new TaskCompletionSource for the login operation.
            // Passing the CredentialRequestInfo object to the constructor will make it available from its AsyncState property.
            _loginTaskCompletionSource = new TaskCompletionSource<Credential>(info);

            // Show the login controls on the UI thread.
            // OnLoginInfoEntered event will return the values entered (username and password).
            InvokeOnMainThread(ShowLoginUI);

            // Return the login task, the result will be ready when completed (user provides login info and clicks the "Login" button).
            return await _loginTaskCompletionSource.Task;
        }

        // Handle the OnLoginEntered event from the login UI.
        // LoginEventArgs contains the username and password that were entered.
        private async void LoginEntered(string username, string password)
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
                CredentialRequestInfo requestInfo = (CredentialRequestInfo) _loginTaskCompletionSource.Task.AsyncState;

                // Create a token credential using the provided username and password.
                TokenCredential userCredentials = await AuthenticationManager.Current.GenerateCredentialAsync
                (requestInfo.ServiceUri,
                    username,
                    password,
                    requestInfo.GenerateTokenOptions);

                // Set the result on the task completion source.
                _loginTaskCompletionSource.TrySetResult(userCredentials);
            }
            catch (Exception ex)
            {
                // Unable to create credential, set the exception on the task completion source.
                _loginTaskCompletionSource.TrySetException(ex);
            }
        }

        private void ShowLoginUI()
        {
            // Prompt for the type of convex hull to create.
            UIAlertController loginAlert = UIAlertController.Create("Authenticate", "", UIAlertControllerStyle.Alert);
            loginAlert.AddTextField(field => field.Placeholder = "Username = user1");
            loginAlert.AddTextField(field => field.Placeholder = "Password = user1");
            loginAlert.AddAction(UIAlertAction.Create("Log in", UIAlertActionStyle.Default, action => { LoginEntered(loginAlert.TextFields[0].Text, loginAlert.TextFields[1].Text); }));
            loginAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Show the alert.
            PresentViewController(loginAlert, true, null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = ApplicationTheme.BackgroundColor};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _publicLayerLabel = new UILabel
            {
                Text = "public layer",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _secureLayerLabel = new UILabel
            {
                Text = "secure layer",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, _publicLayerLabel, _secureLayerLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _publicLayerLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _publicLayerLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _publicLayerLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _publicLayerLabel.HeightAnchor.ConstraintEqualTo(40),

                _secureLayerLabel.TopAnchor.ConstraintEqualTo(_publicLayerLabel.BottomAnchor),
                _secureLayerLabel.LeadingAnchor.ConstraintEqualTo(_publicLayerLabel.LeadingAnchor),
                _secureLayerLabel.TrailingAnchor.ConstraintEqualTo(_publicLayerLabel.TrailingAnchor),
                _secureLayerLabel.HeightAnchor.ConstraintEqualTo(40),

                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events, removing any existing subscriptions.
            if (_publicLayer != null)
            {
                _publicLayer.LoadStatusChanged -= LayerLoadStatusChanged;
                _publicLayer.LoadStatusChanged += LayerLoadStatusChanged;
            }

            if (_tokenSecuredLayer != null)
            {
                _tokenSecuredLayer.LoadStatusChanged -= LayerLoadStatusChanged;
                _tokenSecuredLayer.LoadStatusChanged += LayerLoadStatusChanged;
            }
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _publicLayer.LoadStatusChanged -= LayerLoadStatusChanged;
            _tokenSecuredLayer.LoadStatusChanged -= LayerLoadStatusChanged;
        }
    }
}