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
using Esri.ArcGISRuntime.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGISRuntime.WinUI.Samples.IntegratedWindowsAuth
{
    // Important:
    //    You must add the "Private Networks" capability to use Integrated Windows Authentication (IWA)
    //    in your UWP project. Add this capability by checking "Private Networks (Client and Server)"
    //    in your project's Package.appxmanifest file.
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Integrated Windows Authentication",
        category: "Security",
        description: "Connect to an IWA secured Portal and search for maps.",
        instructions: "1. Enter the URL to your IWA-secured portal.",
        tags: new[] { "Portal", "Windows", "authentication", "security" })]
    public partial class IntegratedWindowsAuth
    {
        // Note: The Universal Windows Platform handles challenging for Windows credentials.
        //       You do not need to surface your own UI to prompt the user for username, password, and domain.

        // The ArcGIS Online URL for searching public web maps.
        private string _publicPortalUrl = "https://www.arcgis.com";

        // The public and secured portals.
        private ArcGISPortal _iwaSecuredPortal = null;
        private ArcGISPortal _publicPortal = null;

        // Track if the user is looking at search results from the public or secured portal.
        private bool _usingPublicPortal;

        public IntegratedWindowsAuth()
        {
            InitializeComponent();

            // Show the light gray canvas basemap.
            MyMapView.Map = new Map(BasemapStyle.ArcGISLightGray);
        }

        // Search the public portal for web maps and display the results in a list.
        private async void SearchPublicMapsButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create an instance of the public portal.
                _publicPortal = await ArcGISPortal.CreateAsync(new Uri(_publicPortalUrl));

                // Call a function to search the portal.
                _ = SearchPortal(_publicPortal);

                // Set a variable that indicates this is the public portal.
                // When a map is loaded from the results, will need to know which portal it came from.
                _usingPublicPortal = true;
            }
            catch (Exception ex)
            {
                // Report any errors that were encountered.
                MessagesTextBlock.Text = ex.Message;
            }
        }

        // Search the IWA-secured portal for web maps and display the results in a list.
        private async void SearchSecureMapsButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the value entered for the secure portal URL.
                string securedPortalUrl = SecurePortalUrlTextBox.Text.Trim();

                // Make sure a portal URL has been entered in the text box.
                if (string.IsNullOrEmpty(securedPortalUrl))
                {
                    var dialog = new MessageDialog2("Please enter the URL of the secured portal.", "Missing URL");
                    await dialog.ShowAsync();
                    return;
                }

                // Create an instance of the IWA-secured portal, the user may be challenged for access.
                _iwaSecuredPortal = await ArcGISPortal.CreateAsync(new Uri(securedPortalUrl));

                // Call a function to search the portal.
                _ = SearchPortal(_iwaSecuredPortal);

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

        private async Task SearchPortal(ArcGISPortal currentPortal)
        {
            // Clear any existing results.
            MapItemListBox.Items.Clear();

            // Show status message and the progress bar.
            MessagesTextBlock.Text = "Searching for web map items on the portal at " + currentPortal.Uri.AbsoluteUri;
            ProgressStatus.Visibility = Visibility.Visible;
            var messageBuilder = new StringBuilder();

            try
            {
                // Report connection info
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

                // Enable the button to load a selected web map item.
                AddMapItem.IsEnabled = true;
            }
            catch (Exception ex)
            {
                // Report errors searching the portal.
                messageBuilder.AppendLine(ex.Message);
            }
            finally
            {
                // Show messages, hide progress bar.
                MessagesTextBlock.Text = messageBuilder.ToString();
                ProgressStatus.Visibility = Visibility.Collapsed;
            }
        }

        private async void AddMapItemClick(object sender, RoutedEventArgs e)
        {
            // Get a web map from the selected portal item and display it in the map view.
            if (MapItemListBox.SelectedItem == null)
            {
                var dialog = new MessageDialog2("No web map item is selected.");
                _ = dialog.ShowAsync();
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
                var itemId = (MapItemListBox.SelectedItem as ListBoxItem).Tag.ToString();

                // Use the item ID to create a PortalItem from the appropriate portal.
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

                    // Add the MapView to the app.
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