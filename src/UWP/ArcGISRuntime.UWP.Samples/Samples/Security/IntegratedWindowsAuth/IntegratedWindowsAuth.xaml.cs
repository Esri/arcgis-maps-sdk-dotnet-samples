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
using System.Linq;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace IntegratedWindowsAuth
{
    // Important:
    //    You must add the "Private Networks" capability to use Integrated Windows Authentication (IWA)
    //    in your UWP project. Add this capability by checking "Private Networks (Client and Server)"
    //    in your project's Package.appxmanifest file.
    public sealed partial class MainPage : Page
    {
        // Note: The Universal Windows Platform handles challenging for Windows credentials.
        //       You do not need to surface your own UI to prompt the user for username, password, and domain.

        //TODO - Add the URL for your IWA-secured portal
        const string SecuredPortalUrl = "https://portaliwaqa.ags.esri.com/gis/sharing";

        //TODO - Add the URL for a portal containing public content (your ArcGIS Online Organization, e.g.)
        const string PublicPortalUrl = "http://www.arcgis.com/sharing/rest";

        //TODO [optional] - Add hard-coded account information (if present, a network credential will be created on app initialize)
        const string NetworkUsername = "";
        const string NetworkPassword = "";
        const string NetworkDomain = "";

        // Variables to point to public and secured portals
        ArcGISPortal _iwaSecuredPortal = null;
        ArcGISPortal _publicPortal = null;

        // Flag variable to track if the user is looking at maps from the public or secured portal
        bool _usingPublicPortal;

        public MainPage()
        {
            this.InitializeComponent();

            // Call a function to add a hard-coded credential (if defined)
            Initialize();
        }

        private void Initialize()
        {
            // Note: unlike a WPF app, your current system credentials will NOT be used by default in a UWP app and
            //       you will be (initially) challenged even for resources to which your system account has access.
            //       Once you provide your credentials, you will not be challenged again for them

            // Check for hard-coded user name, password, and domain values
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

        // Search the public portal for web maps and display the results in a list box
        private async void SearchPublicMapsButtonClick(object sender, RoutedEventArgs e)
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
                ProgressStatus.Visibility = Visibility.Collapsed;
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
