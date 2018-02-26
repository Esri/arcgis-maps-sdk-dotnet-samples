// Copyright 2016 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace IntegratedWindowsAuth
{
    public partial class MainWindow : Window
    {
        //TODO - Add the URL for your IWA-secured portal
        const string SecuredPortalUrl = "https://my.portal.com/gis/sharing/rest";

        //TODO - Add the URL for a portal containing public content (your ArcGIS Online Organization, e.g.)
        const string PublicPortalUrl = "http://www.arcgis.com/sharing/rest";

        //TODO [optional] - Add hard-coded account information (if present, a network credential will be created on app initialize)
        // Note: adding bogus credential info can provide a way to verify unauthorized users will be challenged for a log in
        const string NetworkUsername = "";
        const string NetworkPassword = "";
        const string NetworkDomain = "";

        // Variables to point to public and secured portals
        ArcGISPortal _iwaSecuredPortal = null;
        ArcGISPortal _publicPortal = null;

        // Flag variable to track if the user is looking at maps from the public or secured portal
        bool _usingPublicPortal;

        // Flag to track if the user has canceled the login dialog
        bool _canceledLogin;

        public MainWindow()
        {
            InitializeComponent();

            // Call a function to set up the AuthenticationManager and add a hard-coded credential (if defined)
            Initialize();
        }

        private void Initialize()
        {
            // Define a challenge handler method for the AuthenticationManager 
            // (this method handles getting credentials when a secured resource is encountered)
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Note: for IWA-secured services, your current system credentials will be used by default and you will only
            //       be challenged for resources to which your system account doesn't have access

            // Check for hard-coded username, password, and domain values
            if (!string.IsNullOrEmpty(NetworkUsername) &&
                !string.IsNullOrEmpty(NetworkPassword) &&
                !string.IsNullOrEmpty(NetworkDomain))
            {
                // Create a hard-coded network credential (other than the one that started the app, in other words)
                ArcGISNetworkCredential hardcodedCredential = new ArcGISNetworkCredential
                {
                    Credentials = new System.Net.NetworkCredential(NetworkUsername, NetworkPassword, NetworkDomain),
                    ServiceUri = new Uri(SecuredPortalUrl)
                };

                // Add the credential to the AuthenticationManager and report that a non-default credential is being used
                AuthenticationManager.Current.AddCredential(hardcodedCredential);
                MessagesTextBlock.Text = "Using credentials for user '" + NetworkUsername + "'";
            }
        }
        
        // Prompt the user for a credential if unauthorized access to a secured resource is attempted
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            Credential credential = null;
            try
            {
                // Dispatch to the UI thread to show the login UI
                credential = this.Dispatcher.Invoke(new Func<Credential>(() =>
                {
                    Credential cred = null;

                    // Exit if the user clicked "Cancel" in the login window
                    // (if the user can't provide credentials for a resource they will continue to be challenged)
                    if (_canceledLogin)
                    {
                        _canceledLogin = false;
                        return null;
                    }

                    // Create a new login window
                    var win = new LoginWindow();
                    win.Owner = this;

                    // Show the window to get user input (if canceled, false is returned)
                    _canceledLogin = (win.ShowDialog() == false);

                    if (!_canceledLogin)
                    {
                        // Get the credential information provided
                        var username = win.UsernameTextBox.Text;
                        var password = win.PasswordTextBox.Password;
                        var domain = win.DomainTextBox.Text;

                        // Create a new network credential using the user input and the URI of the resource
                        cred = new ArcGISNetworkCredential()
                        {
                            Credentials = new System.Net.NetworkCredential(username, password, domain),
                            ServiceUri = info.ServiceUri
                        };
                    }

                    // Return the credential
                    return cred;
                }));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex.Message);
            }

            // Add the credential to the AuthenticationManager
            AuthenticationManager.Current.AddCredential(credential);

            // Return the credential
            return credential;
        }

        // Search the public portal for web maps and display the results in a list box
        private async void SearchPublicMapsClick(object sender, RoutedEventArgs e)
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

        // Search the IWA-secured portal for web maps and display the results in a list box
        private async void SearchSecureMapsButtonClick(object sender, RoutedEventArgs e)
        {
            // Set the flag variable to indicate this is the secure portal
            // (if the user wants to load a map, will need to know which portal it came from)
            _usingPublicPortal = false;

            try
            {
                // Create an instance of the IWA-secured portal
                _iwaSecuredPortal = await ArcGISPortal.CreateAsync(new Uri(SecuredPortalUrl));

                // Call a function to search the portal
                SearchPortal(_iwaSecuredPortal);
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
                var items = await currentPortal.FindItemsAsync(new PortalQueryParameters("type:(\"web map\" NOT \"web mapping application\")"));

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

        private async void AddMapItemClick(object sender, RoutedEventArgs e)
        {
            // Get a web map from the selected portal item and display it in the app
            if (this.MapItemListBox.SelectedItem == null) { return; }

            // Clear status messages
            MessagesTextBlock.Text = string.Empty;

            // Store status (or errors) when adding the map
            var statusInfo = new StringBuilder();

            try
            {
                // Clear the current MapView control from the app
                MyMapGrid.Children.Clear();

                // See if using the public or secured portal; get the appropriate object reference
                ArcGISPortal portal = null;
                if (_usingPublicPortal)
                {
                    portal = _publicPortal;
                }
                else
                {
                    portal = _iwaSecuredPortal;
                }

                // Throw an exception if the portal is null
                if (portal == null)
                {
                    throw new Exception("Portal has not been instantiated.");
                }

                // Get the portal item ID from the selected list box item (read it from the Tag property)
                var itemId = (this.MapItemListBox.SelectedItem as ListBoxItem).Tag.ToString();

                // Use the item ID to create an ArcGISPortalItem from the appropriate portal 
                var portalItem = await PortalItem.CreateAsync(portal, itemId);
                
                if (portalItem != null)
                {
                    // Create a Map using the web map (portal item)
                    Map webMap = new Map(portalItem);

                    // Create a new MapView control to display the Map
                    MapView myMapView = new MapView();
                    myMapView.Map = webMap;

                    // Add the MapView to the app
                    MyMapGrid.Children.Add(myMapView);
                }

                // Report success
                statusInfo.AppendLine("Successfully loaded web map from item #" + itemId + " from " + portal.Uri.Host);
            }
            catch (Exception ex)
            {
                // Add an error message
                statusInfo.AppendLine("Error accessing web map: " + ex.Message);
            }
            finally
            {
                // Show messages
                MessagesTextBlock.Text = statusInfo.ToString();
            }
        }

    }
}
