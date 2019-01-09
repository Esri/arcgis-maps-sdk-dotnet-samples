// Copyright 2017 Esri.
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
        private string _publicMapServiceUrl = "https://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer";
        private string _secureMapServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA_secure_user1/MapServer";

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

        public TokenSecuredChallenge()
        {
            Title = "Token Challenge";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
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
            // Prompt for the type of convex hull to create.
            UIAlertController loginAlert = UIAlertController.Create("Authenticate", "", UIAlertControllerStyle.Alert);
            loginAlert.AddTextField(field => field.Placeholder = "Username = user1");
            loginAlert.AddTextField(field => field.Placeholder = "Password = user1");
            loginAlert.AddAction(UIAlertAction.Create("Log in", UIAlertActionStyle.Default, action => {
                LoginEntered(loginAlert.TextFields[0].Text, loginAlert.TextFields[1].Text);
            }));
            loginAlert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Show the alert.
            PresentViewController(loginAlert, true, null);
        }

        public override void LoadView()
        {
            View = new UIView { BackgroundColor = UIColor.White };

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

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            View.AddSubviews(_myMapView, _publicLayerLabel, _secureLayerLabel);

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
                CredentialRequestInfo requestInfo = (CredentialRequestInfo)_loginTaskCompletionSource.Task.AsyncState;

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
    }
}