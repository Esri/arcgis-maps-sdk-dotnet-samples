using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.WebMap;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        const string SecuredPortalUrl = "https://<my_portal_host>/gis/sharing"; 


        //TODO - Add the URL for a portal containing public content (ArcGIS Organization, e.g.)
        const string PublicPortalUrl = "http://esrihax.maps.arcgis.com/sharing/rest";

        // Variables to point to public and secured portals
        ArcGISPortal _pkiSecuredPortal = null;
        ArcGISPortal _publicPortal = null;

        // Flag variable to track if we're looking at maps from the public or secured portal
        bool _usingPublicPortal;

        public MainWindow()
        {
            InitializeComponent();

            // set up the IdentityManager to prompt the user for a client certificate when a secured service is encountered
            IdentityManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
        }

        /// <summary>
        /// Handle challenges for a secured resource by prompting for a client certificate
        /// </summary>
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            Credential credential = null;

            try
            {
                // Use the X509Store to get available certificates for a particular issuer
                var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);
                var certificates = store.Certificates.Find(X509FindType.FindByIssuerName, "Esri", true);

                // Ask the user to select a certificate to use
                certificates = X509Certificate2UI.SelectFromCollection(certificates, "Select Certificate",
                    "Select the certificate to use for authentication.", X509SelectionFlag.SingleSelection);

                // Create a new CertificateCredential using the chosen certificate
                credential = new CertificateCredential(certificates[0])
                {
                    ServiceUri = SecuredPortalUrl
                };

            }
            catch (Exception ex)
            { 
                Debug.WriteLine("Exception: " + ex.Message); 
            }

            // Return the CertificateCredential for the secured portal
            return credential;
        }

        /// <summary>
        /// Search the public portal for web maps and display the results in a list box.
        /// </summary>
        private async void SearchPublicMapsButton_Click(object sender, RoutedEventArgs e)
        {
            // Set the flag variable to indicate we're working with the public portal
            // (if the user wants to load a map, we'll need to know which portal it came from)
            _usingPublicPortal = true;

            MapItemListBox.Items.Clear();

            // Show status message and the status bar
            MessagesTextBlock.Text = "Searching for web map items on the public portal.";
            ProgressStatus.Visibility = System.Windows.Visibility.Visible;
            var sb = new StringBuilder();

            try
            {
                // Create an instance of the public portal
                _publicPortal = await ArcGISPortal.CreateAsync(new Uri(PublicPortalUrl));

                // Report a successful connection
                sb.AppendLine("Connected to the portal on " + _publicPortal.Uri.Host);
                sb.AppendLine("Version: " + _publicPortal.CurrentVersion);

                // Report the username used for this connection
                if (_publicPortal.CurrentUser != null)
                    sb.AppendLine("Connected as: " + _publicPortal.CurrentUser.UserName);
                else
                    sb.AppendLine("Anonymous"); // connected anonymously

                // Search the public portal for web maps
                var items = await _publicPortal.SearchItemsAsync(new SearchParameters("type:(\"web map\" NOT \"web mapping application\")"));

                // Build a list of items from the results that shows the map name and stores the item ID (with the Tag property)
                var resultItems = from r in items.Results select new ListBoxItem { Tag = r.Id, Content = r.Name };
                // Add the list items
                foreach (var itm in resultItems)
                {
                    MapItemListBox.Items.Add(itm);
                }
            }
            catch (Exception ex)
            {
                // Report errors connecting to or searching the public portal
                sb.AppendLine(ex.Message);
            }
            finally
            {
                // Show messages, hide progress bar
                MessagesTextBlock.Text = sb.ToString();
                ProgressStatus.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        /// <summary>
        /// Search the PKI-secured portal for web maps and display the results in a list box.
        /// </summary>
        private async void SearchSecureMapsButton_Click(object sender, RoutedEventArgs e)
        {
            // Set the flag variable to indicate we're working with the secure portal
            // (if the user wants to load a map, we'll need to know which portal it came from)
            _usingPublicPortal = false;

            MapItemListBox.Items.Clear();

            // Show status message and the status bar
            MessagesTextBlock.Text = "Searching for web map items on the secure portal.";
            ProgressStatus.Visibility = System.Windows.Visibility.Visible;
            var sb = new StringBuilder();

            try
            {
                // Create an instance of the PKI-secured portal
                _pkiSecuredPortal = await ArcGISPortal.CreateAsync(new Uri(SecuredPortalUrl));

                // Report a successful connection
                sb.AppendLine("Connected to the portal on " + _pkiSecuredPortal.Uri.Host);
                sb.AppendLine("Version: " + _pkiSecuredPortal.CurrentVersion);

                // Report the username used for this connection
                if (_pkiSecuredPortal.CurrentUser != null)
                    sb.AppendLine("Connected as: " + _pkiSecuredPortal.CurrentUser.UserName);
                else
                    sb.AppendLine("Anonymous"); // THIS SHOULDN'T HAPPEN!

                // Search the secured portal for web maps
                var items = await _pkiSecuredPortal.SearchItemsAsync(new SearchParameters("type:(\"web map\" NOT \"web mapping application\")"));

                // Build a list of items from the results that shows the map name and stores the item ID (with the Tag property)
                var resultItems = from r in items.Results select new ListBoxItem { Tag = r.Id, Content = r.Name };
                // Add the list items
                foreach (var itm in resultItems)
                {
                    MapItemListBox.Items.Add(itm);
                }
            }
            catch (Exception ex)
            {
                // Report errors connecting to or searching the secured portal
                sb.AppendLine(ex.Message);
            }
            finally
            {
                // Show messages, hide progress bar
                MessagesTextBlock.Text = sb.ToString();
                ProgressStatus.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        /// <summary>
        /// Get a web map from the selected portal item and display it in the app.
        /// </summary>
        private async void AddMapItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.MapItemListBox.SelectedItem == null) { return; }

            // Clear status messages
            MessagesTextBlock.Text = string.Empty;
            var sb = new StringBuilder();

            try
            {
                // Clear the current MapView control from the app
                MyMapGrid.Children.Clear();

                // See if we're using the public or secured portal; get the appropriate object reference
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

                // Get the portal item ID from the selected listbox item (read it from the Tag property)
                var itemId = (this.MapItemListBox.SelectedItem as ListBoxItem).Tag.ToString();
                // Use the item ID to create an ArcGISPortalItem from the appropriate portal 
                var portalItem = await ArcGISPortalItem.CreateAsync(portal, itemId);
                // Create a WebMap from the portal item (all items in the list represent web maps)
                var webMap = await WebMap.FromPortalItemAsync(portalItem);


                if (webMap != null)
                {
                    // Create a WebMapViewModel using the WebMap
                    var myWebMapViewModel = await WebMapViewModel.LoadAsync(webMap, portal);

                    // Create a new MapView control to display the WebMapViewModel's Map; add it to the app
                    var mv = new MapView { Map = myWebMapViewModel.Map };
                    MyMapGrid.Children.Add(mv);
                }

                // Report success
                sb.AppendLine("Successfully loaded web map from item #" + itemId + " from " + portal.Uri.Host);
            }
            catch (Exception ex)
            {
                // Add an error message
                sb.AppendLine("Error accessing web map: " + ex.Message);
            }
            finally
            {
                // Show messages
                MessagesTextBlock.Text = sb.ToString();
            }
        }
    }
}
