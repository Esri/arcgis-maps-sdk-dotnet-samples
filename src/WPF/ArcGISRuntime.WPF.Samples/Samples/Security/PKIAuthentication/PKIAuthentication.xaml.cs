// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PKIAuthentication
{
    public partial class MainWindow : Window
    {
        //TODO - Add the URL for your PKI-secured portal
        const string SecuredPortalUrl = ""; 

        //TODO - Add the URL for a portal containing public content (ArcGIS Organization, e.g.)
        const string PublicPortalUrl = "http://esrihax.maps.arcgis.com";

        // Variables to point to public and secured portals
        ArcGISPortal _pkiSecuredPortal = null;
        ArcGISPortal _publicPortal = null;

        // Flag variable to track if maps are from the public or secured portal
        bool _usingPublicPortal;

        public MainWindow()
        {
            InitializeComponent();

            // Set up the AuthenticationManager to prompt the user for a client certificate when a secured service is encountered
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
        }

        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // Handle challenges for a secured resource by prompting for a client certificate
            Credential credential = null;

            // TODO: Remove the following workaround once issue #232 is addressed
            credential = AuthenticationManager.Current.Credentials.FirstOrDefault();

            if (credential != null)
            {
                if (credential.ServiceUri.AbsoluteUri.StartsWith(SecuredPortalUrl))
                {
                    // Return the CertificateCredential for the secured portal
                    return credential;
                }
            }
            // END: workaround             

            try
            {
                // Use the X509Store to get a collection of available certificates
                var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);
                var certificates = store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, true);

                // Prompt the user to select a certificate
                var selection = X509Certificate2UI.SelectFromCollection(certificates, "Select Certificate",
                    "Select the certificate to use for authentication.", X509SelectionFlag.SingleSelection);

                // Make sure the user chose one
                if (selection.Count > 0)
                {
                    // Create a new CertificateCredential using the chosen certificate
                    credential = new Esri.ArcGISRuntime.Security.CertificateCredential(selection[0])
                    {
                        ServiceUri = new Uri(SecuredPortalUrl)
                    };

                    // Add the credential to the authentication manager
                    AuthenticationManager.Current.AddCredential(credential);
                }
            }
            catch (Exception ex)
            { 
                Debug.WriteLine("Exception: " + ex.Message); 
            }

            // Return the CertificateCredential for the secured portal
            return credential;
        }

        // Search the public portal for web maps and display the results in a list box
        private async void SearchPublicMapsButton_Click(object sender, RoutedEventArgs e)
        {
            // Set the flag variable to indicate this is the public portal
            // (if the user wants to load a map, will need to know which portal it came from)
            _usingPublicPortal = true;

            try
            {
                // Create an instance of the public portal
                _publicPortal = await ArcGISPortal.CreateAsync(new Uri(PublicPortalUrl));

                // Call a function to search the portal
                SearchPortal(_publicPortal);
            }
            catch (Exception ex)
            {
                // Report errors connecting to the secured portal
                MessagesTextBlock.Text = ex.Message;
            }
        }

        // Search the PKI-secured portal for web maps and display the results in a list box
        private async void SearchSecureMapsButton_Click(object sender, RoutedEventArgs e)
        {
            // Set the flag variable to indicate this is the secure portal
            // (if the user wants to load a map, will need to know which portal it came from)
            _usingPublicPortal = false;
            
            try
            {
                // Create an instance of the PKI-secured portal
                _pkiSecuredPortal = await ArcGISPortal.CreateAsync(new Uri(SecuredPortalUrl));

                // Call a function to search the portal
                SearchPortal(_pkiSecuredPortal);
            }
            catch (Exception ex)
            {
                // Report errors connecting to the secured portal
                MessagesTextBlock.Text = ex.Message;
            }
        }

        private async void SearchPortal(ArcGISPortal currentPortal)
        {           
            MapItemListBox.Items.Clear();

            // Show status message and the status bar
            MessagesTextBlock.Text = "Searching for web map items on the portal at " + currentPortal.Uri.AbsoluteUri;
            ProgressStatus.Visibility = Visibility.Visible;
            var messageBuilder = new StringBuilder();

            try
            {                
                // Report connection info
                messageBuilder.AppendLine("Connected to the portal on " + currentPortal.Uri.Host);

                // Report the user name used for this connection
                if (currentPortal.User != null)
                {
                    messageBuilder.AppendLine("Connected as: " + currentPortal.User.UserName);
                }
                else
                {
                    // (this shouldn't happen for a secure portal)
                    messageBuilder.AppendLine("Anonymous");
                }

                // Search the portal for web maps
                var searchParams = new PortalQueryParameters("type:(\"web map\" NOT \"web mapping application\")");
                var items = await currentPortal.FindItemsAsync(searchParams);

                // Build a list of items from the results that shows the map name and stores the item ID (with the Tag property)
                var resultItems = from r in items.Results select new ListBoxItem { Tag = r.ItemId, Content = r.Title };

                // Add the list items
                foreach (var itm in resultItems)
                {
                    MapItemListBox.Items.Add(itm);
                }
            }
            catch (Exception ex)
            {
                // Report errors searching the portal
                messageBuilder.AppendLine(ex.Message);
            }
            finally
            {
                // Show messages, hide progress bar
                MessagesTextBlock.Text = messageBuilder.ToString();
                ProgressStatus.Visibility = Visibility.Hidden;
            }
        }

        private async void AddMapItem_Click(object sender, RoutedEventArgs e)
        {
            // Get a web map from the selected portal item and display it in the app
            if (MapItemListBox.SelectedItem == null) { return; }

            // Clear status messages
            MessagesTextBlock.Text = string.Empty;

            var messageBuilder = new StringBuilder();

            try
            {
                // See if this is the public or secured portal and get the appropriate object reference
                ArcGISPortal portal = null;
                if (_usingPublicPortal)
                {
                    portal = _publicPortal;
                }
                else
                {
                    portal = _pkiSecuredPortal;
                }

                // Throw an exception if the portal is null
                if (portal == null)
                {
                    throw new Exception("Portal has not been instantiated.");
                }

                // Get the portal item ID from the selected list box item (read it from the Tag property)
                var itemId = (MapItemListBox.SelectedItem as ListBoxItem).Tag.ToString();

                // Use the item ID to create an ArcGISPortalItem from the appropriate portal 
                var portalItem = await PortalItem.CreateAsync(portal, itemId);

                // Create a Map from the portal item (all items in the list represent web maps)
                var webMap = new Map(portalItem);

                // Display the Map in the map view
                MyMapView.Map = webMap;

                // Report success
                messageBuilder.AppendLine("Successfully loaded web map from item #" + itemId + " from " + portal.Uri.Host);
            }
            catch (Exception ex)
            {
                // Add an error message
                messageBuilder.AppendLine("Error accessing web map: " + ex.Message);
            }
            finally
            {
                // Show messages
                MessagesTextBlock.Text = messageBuilder.ToString();
            }
        }
    }
}
