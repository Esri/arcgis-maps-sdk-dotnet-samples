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

namespace ArcGISRuntimeXamarin.Samples.TokenSecuredKnownUser
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "ArcGIS token with a known user",
        "Security",
        "This sample demonstrates how to authenticate with ArcGIS Server using ArcGIS Tokens to access a secure service. Accessing secured services requires a login that's been defined on the server.",
        "1. When you run the sample, the app will load a map that contains a layer from a secured service.\n2. You will NOT be challenged for a user name and password to view that layer because that info has been hard-coded into the app.\n3. If the credentials in the code are correct, the secured layer will display, otherwise the map will contain only the public layers.",
        "Authentication, Security, ArcGIS Token")]
    [Register("TokenSecuredKnownUser")]
    public class TokenSecuredKnownUser : UIViewController
    {
        // Public and secured map service URLs.
        private string _publicMapServiceUrl = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer";
        private string _secureMapServiceUrl = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA_secure_user1/MapServer";

        // Public and secured layer names.
        private string _publicLayerName = "World Street Map - Public";
        private string _secureLayerName = "USA - Secure";

        // Store the map view displayed in the app.
        private MapView _myMapView;

        // Labels to show layer load status.
        private UILabel _publicLayerLabel;
        private UILabel _secureLayerLabel;

        public TokenSecuredKnownUser()
        {
            Title = "Token known user";
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
            _publicLayerLabel = new UILabel()
            {
                TextColor = UIColor.Gray,
                Text = _publicLayerName
            };

            // Create a label to show the load status of the secured layer.
            _secureLayerLabel = new UILabel()
            {
                TextColor = UIColor.Gray,
                Text = _secureLayerName
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
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateKnownCredentials);

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

        // Challenge method that checks for service access with known (hard coded) credentials.
        private async Task<Credential> CreateKnownCredentials(CredentialRequestInfo info)
        {
            // If this isn't the expected resource, the credential will stay null.
            Credential knownCredential = null;

            try
            {
                // Check the URL of the requested resource.
                if (info.ServiceUri.AbsoluteUri.ToLower().Contains("usa_secure_user1"))
                {
                    // Username and password is hard-coded for this resource (would be better to read them from a secure source).
                    string username = "user1";
                    string password = "user1";

                    // Create a credential for this resource.
                    knownCredential = await AuthenticationManager.Current.GenerateCredentialAsync
                    (info.ServiceUri,
                        username,
                        password,
                        info.GenerateTokenOptions);
                }
                else
                {
                    // Another option would be to prompt the user here if the username and password is not known.
                }
            }
            catch (Exception ex)
            {
                // Report error accessing a secured resource.
                new UIAlertView("Credential Error",
                    $"Access to {info.ServiceUri.AbsoluteUri} denied. {ex.Message}",
                    (IUIAlertViewDelegate) null, "Cancel", null).Show();
            }

            // Return the credential.
            return knownCredential;
        }
    }
}