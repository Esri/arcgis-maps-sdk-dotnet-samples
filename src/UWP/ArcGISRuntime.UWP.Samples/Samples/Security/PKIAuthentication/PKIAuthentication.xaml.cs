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
using System;
using System.Linq;
using System.Text;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PKIAuthentication
{
    // Important:
    //    You must add the "Private Networks" capability to use Public Key Infrastructure (PKI) authentication
    //    in your UWP project. Add this capability by checking "Private Networks (Client and Server)"
    //    in your project's Package.appxmanifest file.
    public sealed partial class MainPage : Page
    {
        //TODO - Add the URL for your PKI-secured portal
        private const string SecuredPortalUrl = "https://my.secure.portal.com/gis";

        //TODO - Add the URL for a portal containing public content (ArcGIS Organization, e.g.)
        private const string PublicPortalUrl = "http://esrihax.maps.arcgis.com";

        // Variables to store the encrypted certificate data and friendly name
        private string _certificateString;
        private string _certificateName;

        // Variables to point to public and secured portals
        ArcGISPortal _pkiSecuredPortal = null;
        ArcGISPortal _publicPortal = null;

        // Flag variable to track if web map items are from the public or secured portal
        bool _usingPublicPortal;

        public MainPage()
        {
            InitializeComponent();
        }

        // Click handler for the LoadClientCertButton button that allows the user to choose a certificate file (.pfx)
        private async void ChooseCertificateFile(object sender, RoutedEventArgs e)
        {
            // Create a file picker dialog so the user can select an exported certificate (*.pfx)
            var pfxFilePicker = new FileOpenPicker();
            pfxFilePicker.FileTypeFilter.Add(".pfx");
            pfxFilePicker.CommitButtonText = "Open";

            // Show the dialog and get the selected file (if any)
            StorageFile pfxFile = await pfxFilePicker.PickSingleFileAsync();

            // If a file was selected, store the encrypted data in a string
            if (pfxFile != null)
            {
                // Use the file's display name for the certificate name
                _certificateName = pfxFile.DisplayName;

                // Read the contents of the file
                IBuffer buffer = await FileIO.ReadBufferAsync(pfxFile);
                using (DataReader dataReader = DataReader.FromBuffer(buffer))
                {
                    // Store the contents of the file as an encrypted string
                    // The string will be imported as a certificate when the user enters the password
                    byte[] bytes = new byte[buffer.Length];
                    dataReader.ReadBytes(bytes);
                    _certificateString = Convert.ToBase64String(bytes);
                }

                // Show the certificate password box (and hide the map search controls)
                LoginPanel.Visibility = Visibility.Visible;
                LoadMapPanel.Visibility = Visibility.Collapsed;
            }
        }

        // Load a client certificate for accessing a PKI-secured server 
        private async void LoadClientCertButton_Click(object sender, RoutedEventArgs e)
        {
            // Show the progress bar and a message
            ProgressStatus.Visibility = Visibility.Visible;
            MessagesTextBlock.Text = "Loading certificate ...";

            try
            {
                // Import the certificate by providing: 
                //   -the encoded certificate string, 
                //   -the password (entered by the user)
                //   -certificate options (export, key protection, install)
                //   -a friendly name (the name of the pfx file)
                await CertificateEnrollmentManager.ImportPfxDataAsync(
                    _certificateString,
                    CertPasswordBox.Password,
                    ExportOption.Exportable,
                    KeyProtectionLevel.NoConsent,
                    InstallOptions.None,
                    _certificateName);

                // Report success
                MessagesTextBlock.Text = "Client certificate was successfully imported";
            }
            catch (Exception ex)
            {
                // Report error
                MessagesTextBlock.Text = "Error loading certificate: " + ex.Message;
            }
            finally
            {
                // Hide progress bar and the password controls
                ProgressStatus.Visibility = Visibility.Collapsed;
                HideCertLogin(null, null);
            }
        }

        // Search the public portal for web maps and display the results in a list box.
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

        // Search the PKI-secured portal for web maps and display the results in a list box.
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
        
        // Search the portal for web maps
        private async void SearchPortal(ArcGISPortal currentPortal)
        {
            // Clear existing results and show a status message and progress bar
            MapItemListBox.Items.Clear();
            MessagesTextBlock.Text = "Searching for web map items on the portal at " + currentPortal.Uri.AbsoluteUri;
            ProgressStatus.Visibility = Visibility.Visible;

            // Use a StringBuilder to store messages
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
                ProgressStatus.Visibility = Visibility.Collapsed;
            }
        }

        // Click event handler for the AddMapItem button
        private async void AddMapItem_Click(object sender, RoutedEventArgs e)
        {
            // Get a web map from the selected portal item and display it in the app
            if (MapItemListBox.SelectedItem == null) { return; }

            // Clear status messages
            MessagesTextBlock.Text = string.Empty;
            var messageBuilder = new StringBuilder();

            try
            {
                // See if this is from the public or secured portal, then get the appropriate object reference
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

                // Create a WebMap from the portal item (all items in the list represent web maps)
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

        // Hide the password entry controls for importing a certificate from file
        private void HideCertLogin(object sender, RoutedEventArgs e)
        {
            // Hide the certificate password box (and show the map search controls)
            LoginPanel.Visibility = Visibility.Collapsed;
            LoadMapPanel.Visibility = Visibility.Visible;
        }
    }
}
