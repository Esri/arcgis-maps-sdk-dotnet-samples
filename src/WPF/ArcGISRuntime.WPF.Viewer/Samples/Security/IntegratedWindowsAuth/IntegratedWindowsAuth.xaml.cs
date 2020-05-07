// Copyright 2018 Esri.
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

namespace ArcGISRuntime.WPF.Samples.IntegratedWindowsAuth
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Integrated Windows Authentication",
        "Security",
        "Connect to an IWA secured Portal and search for maps.",
        "1. Enter the URL to your IWA-secured portal.",
        "Portal", "Windows", "authentication", "security")]
    [ArcGISRuntime.Samples.Shared.Attributes.ClassFile("LoginWindow.xaml.cs")]
    [ArcGISRuntime.Samples.Shared.Attributes.XamlFiles("LoginWindow.xaml")]
    public partial class IntegratedWindowsAuth
    {
        // The ArcGIS Online URL for searching public web maps.
        private string _publicPortalUrl = "http://www.arcgis.com";

        // The public and secured portals.
        ArcGISPortal _iwaSecuredPortal = null;
        ArcGISPortal _publicPortal = null;

        // Track if the user is looking at search results from the public or secured portal.
        bool _usingPublicPortal;

        // Track if the user has canceled the login dialog.
        bool _canceledLogin;

        public IntegratedWindowsAuth()
        {
            InitializeComponent();

            // Define a challenge handler method for the AuthenticationManager.
            // This method handles getting credentials when a secured resource is encountered.
            // Note: for IWA-secured services, the current system credentials will be used by default.
            // The user will only be challenged for resources the current account doesn't have permissions for.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
        }

        // Prompt the user for a credential if unauthorized access to a secured resource is attempted.
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            Credential credential = null;
            try
            {
                // Dispatch to the UI thread to show the login UI.
                credential = Dispatcher.Invoke(new Func<Credential>(() =>
                {
                    Credential cred = null;

                    // Exit if the user clicked "Cancel" in the login window.
                    // The dialog will continue to be challenge the user if incorrect credentials are entered.
                    if (_canceledLogin)
                    {
                        _canceledLogin = false;
                        return null;
                    }

                    // Create a new login window.
                    var win = new LoginWindow();

                    // Show the window to get the user login (if canceled, false is returned).
                    _canceledLogin = (win.ShowDialog() == false);

                    if (!_canceledLogin)
                    {
                        // Get the credential information provided.
                        var username = win.UsernameTextBox.Text;
                        var password = win.PasswordTextBox.Password;
                        var domain = win.DomainTextBox.Text;

                        // Create a new network credential using the user input and the URI of the resource.
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

            // Add the credential to the AuthenticationManager.
            AuthenticationManager.Current.AddCredential(credential);

            // Return the credential.
            return credential;
        }

        // Search the public portal for web maps and display the results in a list.
        private async void SearchPublicMapsClick(object sender, RoutedEventArgs e)
        {
            // Set a variable that indicates this is the public portal.
            // When a map is loaded from the results, will need to know which portal it came from.
            _usingPublicPortal = true;

            try
            {
                // Create an instance of the public portal.
                _publicPortal = await ArcGISPortal.CreateAsync(new Uri(_publicPortalUrl));

                // Call a function to search the portal.
                SearchPortal(_publicPortal);
            }
            catch (Exception ex)
            {
                // Report any errors encountered.
                MessagesTextBlock.Text = ex.Message;
            }
        }

        // Search the IWA-secured portal for web maps and display the results in a list.
        private async void SearchSecureMapsButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the string entered for the secure portal URL.
                string securedPortalUrl = SecurePortalUrlTextBox.Text.Trim();
                
                // Make sure a portal URL has been entered in the text box.
                if (string.IsNullOrEmpty(securedPortalUrl))
                {
                    MessageBox.Show("Please enter the URL of the secured portal.", "Missing URL");
                    return;
                }


                // Check if the current Window credentials should be used or require an explicit login.
                bool requireLogin = RequireLoginCheckBox.IsChecked == true;

                // Create an instance of the IWA-secured portal, the user may be challenged for access.
                _iwaSecuredPortal = await ArcGISPortal.CreateAsync(new Uri(securedPortalUrl), requireLogin);

                // Call a function to search the portal.
                SearchPortal(_iwaSecuredPortal);

                // Set a variable that indicates this is the secure portal.
                // When a map is loaded from the results, will need to know which portal it came from.
                _usingPublicPortal = false;
            }
            catch (Exception ex)
            {
                // Report errors (connecting to the secured portal, for example).
                MessagesTextBlock.Text = ex.Message;
            }
        }

        private async void SearchPortal(ArcGISPortal currentPortal)
        {
            // Remove any existing results from the list.
            MapItemListBox.Items.Clear();

            // Show a status message and progress bar.
            MessagesTextBlock.Text = "Searching for web map items on the portal at " + currentPortal.Uri.AbsoluteUri;
            ProgressStatus.Visibility = Visibility.Visible;
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

                // Build a list of items from the results that shows the map name and stores the item ID (with the Tag property).
                var resultItems = from r in items.Results select new ListBoxItem { Tag = r.ItemId, Content = r.Title };

                // Add the items to the list box.
                foreach (var itm in resultItems)
                {
                    MapItemListBox.Items.Add(itm);
                }

                // Enable the button for adding a web map.
                AddMapItem.IsEnabled = true;
            }
            catch (Exception ex)
            {
                // Report errors searching the portal.
                messageBuilder.AppendLine(ex.Message);
            }
            finally
            {
                // Show messages, hide the progress bar.
                MessagesTextBlock.Text = messageBuilder.ToString();
                ProgressStatus.Visibility = Visibility.Hidden;
            }
        }

        private async void AddMapItemClick(object sender, RoutedEventArgs e)
        {
            // Get a web map from the selected portal item and display it in the map view.
            if (MapItemListBox.SelectedItem == null)
            {
                MessageBox.Show("No web map item is selected.");
                return;
            }

            // Clear status messages.
            MessagesTextBlock.Text = string.Empty;

            // Store status (or errors) when adding the map.
            var statusInfo = new StringBuilder();

            try
            {
                // Clear the current MapView control from the app.
                MyMapGrid.Children.Clear();

                // See if using the public or secured portal; get the appropriate object reference.
                ArcGISPortal portal = null;
                if (_usingPublicPortal)
                {
                    portal = _publicPortal;
                }
                else
                {
                    portal = _iwaSecuredPortal;
                }

                // Throw an exception if the portal is null.
                if (portal == null)
                {
                    throw new Exception("Portal has not been instantiated.");
                }

                // Get the portal item ID from the selected list box item (read it from the Tag property).
                var itemId = (this.MapItemListBox.SelectedItem as ListBoxItem).Tag.ToString();

                // Use the item ID to create a PortalItem from the portal.
                var portalItem = await PortalItem.CreateAsync(portal, itemId);

                if (portalItem != null)
                {
                    // Create a Map using the web map (portal item).
                    Map webMap = new Map(portalItem);

                    // Create a new MapView control to display the Map.
                    MapView myMapView = new MapView
                    {
                        Map = webMap
                    };

                    // Add the MapView to the UI.
                    MyMapGrid.Children.Add(myMapView);
                }

                // Report success.
                statusInfo.AppendLine("Successfully loaded web map from item #" + itemId + " from " + portal.Uri.Host);
            }
            catch (Exception ex)
            {
                // Add an error message.
                statusInfo.AppendLine("Error accessing web map: " + ex.Message);
            }
            finally
            {
                // Show messages.
                MessagesTextBlock.Text = statusInfo.ToString();
            }
        }
    }
}