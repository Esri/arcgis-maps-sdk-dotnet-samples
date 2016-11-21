// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Threading.Tasks;
using UIKit;

namespace TokenSecuredKnownUser
{
    public partial class TokenSecuredKnownUser : UIViewController
    {
        // Constants for the public and secured map service URLs
        private const string PublicMapServiceUrl = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer";
        private const string SecureMapServiceUrl = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA_secure_user1/MapServer";

        // Constants for the public and secured layer names
        private const string PublicLayerName = "World Street Map - Public";
        private const string SecureLayerName = "USA - Secure";

        // Store the map view displayed in the app
        MapView _myMapView;

        // Labels to show layer load status
        UILabel _publicLayerLabel;
        UILabel _secureLayerLabel;

        // Default constructor
        public TokenSecuredKnownUser() : base("TokenSecuredKnownUser", null) { }

        // Constructor overload
        public TokenSecuredKnownUser(IntPtr p) : base(p) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Call a function to create the user interface
            CreateLayout();

            // Call a function to initialize the app
            Initialize();
        }

        private void CreateLayout()
        {
            // Create a label for showing the load status for the public service
            var label1ViewFrame = new CoreGraphics.CGRect(10, 30, View.Bounds.Width - 10, 20);
            _publicLayerLabel = new UILabel(label1ViewFrame);
            _publicLayerLabel.TextColor = UIColor.Gray;
            _publicLayerLabel.Font = _publicLayerLabel.Font.WithSize(12);
            _publicLayerLabel.Text = PublicLayerName;

            // Create a label to show the load status of the secured layer
            var label2ViewFrame = new CoreGraphics.CGRect(10, 55, View.Bounds.Width - 10, 20);
            _secureLayerLabel = new UILabel(label2ViewFrame);
            _secureLayerLabel.TextColor = UIColor.Gray;
            _secureLayerLabel.Font = _secureLayerLabel.Font.WithSize(12);
            _secureLayerLabel.Text = SecureLayerName;

            // Setup the visual frame for the MapView
            var mapViewRect = new CoreGraphics.CGRect(0, 80, View.Bounds.Width, View.Bounds.Height - 80);

            // Create a map view with a basemap
            _myMapView = new MapView();
            _myMapView.Frame = mapViewRect;

            // Add the map view and button to the page
            View.AddSubviews(_publicLayerLabel, _secureLayerLabel, _myMapView);
        }

        private void Initialize()
        {
            // Define a challenge handler method for the AuthenticationManager 
            // (this method handles getting credentials when a secured resource is encountered)
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateKnownCredentials);

            // Create the public layer and provide a name
            var publicLayer = new ArcGISTiledLayer(new Uri(PublicMapServiceUrl));
            publicLayer.Name = PublicLayerName;

            // Create the secured layer and provide a name
            var tokenSecuredLayer = new ArcGISMapImageLayer(new Uri(SecureMapServiceUrl));
            tokenSecuredLayer.Name = SecureLayerName;

            // Track the load status of each layer with a LoadStatusChangedEvent handler
            publicLayer.LoadStatusChanged += LayerLoadStatusChanged;
            tokenSecuredLayer.LoadStatusChanged += LayerLoadStatusChanged;

            // Create a new map and add the layers
            var myMap = new Map();
            myMap.OperationalLayers.Add(publicLayer);
            myMap.OperationalLayers.Add(tokenSecuredLayer);

            // Add the map to the map view
            _myMapView.Map = myMap;
        }

        // Handle the load status changed event for the public and token-secured layers
        private void LayerLoadStatusChanged(object sender, Esri.ArcGISRuntime.LoadStatusEventArgs e)
        {
            // Get the layer that triggered the event
            var layer = sender as Layer;

            // Get the label for this layer
            UILabel labelToUpdate = null;
            if (layer.Name == PublicLayerName)
            {
                labelToUpdate = _publicLayerLabel;
            }
            else
            {
                labelToUpdate = _secureLayerLabel;
            }

            // Create the text string and font color to describe the current load status
            var updateText = layer.Name;
            var textColor = UIColor.Gray;

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

            // Update the layer label on the UI thread
            this.BeginInvokeOnMainThread(() =>
            {
                labelToUpdate.Text = updateText;
                labelToUpdate.TextColor = textColor;
            });
        }

        // Challenge method that checks for service access with known (hard coded) credentials
        private async Task<Credential> CreateKnownCredentials(CredentialRequestInfo info)
        {
            // If this isn't the expected resource, the credential will stay null
            Credential knownCredential = null;

            try
            {
                // Check the URL of the requested resource
                if (info.ServiceUri.AbsoluteUri.ToLower().Contains("usa_secure_user1"))
                {
                    // Username and password is hard-coded for this resource
                    // (Would be better to read them from a secure source)
                    string username = "user1";
                    string password = "user1";

                    // Create a credential for this resource
                    knownCredential = await AuthenticationManager.Current.GenerateCredentialAsync
                                            (info.ServiceUri,
                                             username,
                                             password,
                                             info.GenerateTokenOptions);
                }
                else
                {
                    // Another option would be to prompt the user here if the username and password is not known
                }
            }
            catch (Exception ex)
            {
                // Report error accessing a secured resource
                var alertView = new UIAlertView("Credential Error", "Access to " + info.ServiceUri.AbsoluteUri + " denied. " + ex.Message, null, "Cancel", null);
                alertView.Show();
            }

            // Return the credential
            return knownCredential;
        }

    }
}