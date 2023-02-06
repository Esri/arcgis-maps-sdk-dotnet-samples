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
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ArcGIS.WPF.Samples.IntegratedWindowsAuth
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Integrated Windows Authentication",
        category: "Security",
        description: "Connect to an IWA secured Portal and search for maps.",
        instructions: "1. Enter the URL to your IWA-secured portal.",
        tags: new[] { "Portal", "Windows", "authentication", "security" })]
    public partial class IntegratedWindowsAuth
    {
        // An Integrated Windows Authentication secured portal.
        private ArcGISPortal _iwaSecuredPortal = null;

        public IntegratedWindowsAuth()
        {
            InitializeComponent();
        }

        // Search the IWA-secured portal for web maps and display the results in a list.
        private async void SearchSecureMapsButtonClick(object sender, RoutedEventArgs e)
        {
            var messageBuilder = new StringBuilder();

            // Indicate through UI that authentication is being attempted.
            SearchSecureMapsButton.IsEnabled = false;
            ProgressStatus.Visibility = Visibility.Visible;

            try
            {
                // Get the string entered for the secure portal and format as a URI.
                string securedPortalUrl = SecurePortalUrlTextBox.Text.Trim();
                var securedPortal = new Uri(securedPortalUrl);

                // Show status message and the progress bar.
                AuthenticationMessages.Text = "Attempting authentication and searching for web maps at " + securedPortal.AbsoluteUri;
                ProgressStatus.Visibility = Visibility.Visible;

                // Create an instance of the IWA-secured portal.
                _iwaSecuredPortal = await ArcGISPortal.CreateAsync(securedPortal);

                // Report the user name used for this connection.
                if (_iwaSecuredPortal.User != null)
                {
                    messageBuilder.AppendLine("Connected as: " + _iwaSecuredPortal.User.UserName);
                }
                else
                {
                    // Note: This shouldn't happen for a secure portal!
                    messageBuilder.AppendLine("Anonymous");
                }

                // Report connection info.
                messageBuilder.AppendLine("Connected to the portal on " + _iwaSecuredPortal.Uri.Host);

                // Search the portal for web maps.
                var items = await _iwaSecuredPortal.FindItemsAsync(new PortalQueryParameters("type:(\"web map\" NOT \"web mapping application\")"));

                // Add map names to the list box.
                var resultItems = from r in items.Results select new ListBoxItem { Tag = r.ItemId, Content = r.Title };
                foreach (var itm in resultItems)
                {
                    MapItemListBox.Items.Add(itm);
                }

                // Make the ListBox visible now that it has been populated.
                MapItemListBox.Visibility = Visibility.Visible;

                // Update UI to reflect authenticated state.
                AuthenticationBorder.Visibility = Visibility.Collapsed;
                PostAuthenticationBorder.Visibility = Visibility.Visible;
                PostAuthenticationMessages.Text = messageBuilder.ToString();

                // Load the first portal item by default (calls ListBoxSelectedIndexChanged).
                MapItemListBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                // Report errors.
                messageBuilder.AppendLine(ex.Message);
                AuthenticationMessages.Text = messageBuilder.ToString();
            }
            finally
            {
                // Hide progress bar.
                ProgressStatus.Visibility = Visibility.Collapsed;
            }
        }

        private async void ListBoxSelectedIndexChanged(object sender, RoutedEventArgs e)
        {
            // Clear status messages.
            PostAuthenticationMessages.Text = string.Empty;

            // Store status (or errors) when adding the map.
            var statusInfo = new StringBuilder();

            try
            {
                // Clear the current Map.
                MyMapView.Map = null;

                // Get the portal item ID from the selected list box item (read it from the Tag property).
                var itemId = (this.MapItemListBox.SelectedItem as ListBoxItem).Tag.ToString();

                // Use the item ID to create a PortalItem from the portal.
                var portalItem = await PortalItem.CreateAsync(_iwaSecuredPortal, itemId);

                // Create a Map using the web map (portal item).
                MyMapView.Map = new Map(portalItem);

                // Report success.
                statusInfo.AppendLine("Successfully loaded web map from item #" + itemId + " from " + _iwaSecuredPortal.Uri.Host);
            }
            catch (Exception ex)
            {
                // Add an error message.
                statusInfo.AppendLine("Error accessing web map: " + ex.Message);
            }
            finally
            {
                // Show messages.
                PostAuthenticationMessages.Text = statusInfo.ToString();
            }
        }

        // Enable the search button if the entered URL is in the correct format.
        private void SecurePortalUrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchSecureMapsButton.IsEnabled = Uri.IsWellFormedUriString(SecurePortalUrlTextBox.Text.ToString().Trim(), UriKind.Absolute);
        }
    }
}