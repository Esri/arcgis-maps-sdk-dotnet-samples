using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.WebMap;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace IWAAuthentication
{
    public partial class MainWindow : Window
    {
        //TODO - Add the URL for your IWA-secured portal
        const string SecuredPortalUrl = "https://<my_portal_host>/gis/sharing"; 

        //TODO - Add the URL for a portal containing public content (ArcGIS Organization, e.g.)
        const string PublicPortalUrl = "http://www.arcgis.com/sharing/rest"; // ArcGIS Online

        // Variables to point to public and secured portals
        ArcGISPortal _iwaSecuredPortal = null;
        ArcGISPortal _publicPortal = null;

        // Flag variable to track if the user is looking at maps from the public or secured portal
        bool _usingPublicPortal;

        // Flag to track if the user has cancelled the login dialog
        bool _cancelledLogin;

        public MainWindow()
        {
            InitializeComponent();

            // Define a challenge handler method for the IdentityManager 
            // (this method handles getting credentials when a secured resource is encountered)
            // Note: for IWA-secured services, your current system credentials will be used by default and you will only
            //       be challenged for resources to which your system account doesn't have access
            IdentityManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // You could add code like the following to use network credentials other than those you used to start the app
            // ...
            //  var otherCredential = new ArcGISNetworkCredential()
            //  {  
            //      Credentials = new NetworkCredential("user", "p@ssWoRd", "MyDomain"),
            //      ServiceUri = SecuredPortalUrl
            //  };
            //  IdentityManager.Current.AddCredential(otherCredential);
        }

        /// <summary>
        /// Prompts the user for a credential (username/password) if access to a secured resource is attempted.
        /// </summary>
        /// <param name="info">Information about a secured resource (its URI, for example)</param>
        /// <returns>A credential to use when attempting access to a secured resource</returns>
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            Credential credential = null;
            try
            { 
                // Showing a new window with login UI (username/password) must occur on the UI thread
                credential = this.Dispatcher.Invoke(new Func<Credential>(() =>
                {
                    Credential cred = null;

                    // Exit if the user clicked "Cancel" in the login window
                    // (otherwise, if the user can't provide credentials for a resource they will continue to be challenged)
                    if (_cancelledLogin) 
                    {
                        _cancelledLogin = false;
                        return null; 
                    }

                    // Create a new login window
                    var win = new LoginWindow();
                    win.Owner = this;

                    // Show the window to get user input (if cancelled, false is returned)
                    _cancelledLogin = (win.ShowDialog() == false);

                    if (!_cancelledLogin)
                    {
                        // Get the credential information provided
                        var username = win.UsernameTextBox.Text;
                        var password = win.PasswordTextBox.Password;
                        var domain = win.DomainTextBox.Text;

                        // Create a new network credential using the user input and the URI of the resource
                        cred = new ArcGISNetworkCredential()
                        {
                            Credentials = new NetworkCredential(username, password, domain),
                            ServiceUri = info.ServiceUri
                        };
                    }

                    // Return the credential
                    return cred;
                })
                );
            }
            catch (Exception ex)
            { 
                Debug.WriteLine("Exception: " + ex.Message); 
            }

            // Add the credential to the IdentityManager
            IdentityManager.Current.AddCredential(credential);

            // Return the credential
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

                // Build a list of items from the results that shows the map title and stores the item ID (with the Tag property)
                var resultItems = from r in items.Results select new ListBoxItem { Tag = r.Id, Content = r.Title };
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
        /// Search the IWA-secured portal for web maps and display the results in a list box.
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
                // Create an instance of the IWA-secured portal
                _iwaSecuredPortal = await ArcGISPortal.CreateAsync(new Uri(SecuredPortalUrl));

                // Report a successful connection
                sb.AppendLine("Connected to the portal on " + _iwaSecuredPortal.Uri.Host);
                sb.AppendLine("Version: " + _iwaSecuredPortal.CurrentVersion);

                // Report the username used for this connection
                if (_iwaSecuredPortal.CurrentUser != null)
                    sb.AppendLine("Connected as: " + _iwaSecuredPortal.CurrentUser.UserName);
                else
                    sb.AppendLine("Anonymous"); // THIS SHOULDN'T HAPPEN!

                // Search the secured portal for web maps
                var items = await _iwaSecuredPortal.SearchItemsAsync(new SearchParameters("type:(\"web map\" NOT \"web mapping application\")"));

                // Build a list of items from the results that shows the map title and stores the item ID (with the Tag property)
                var resultItems = from r in items.Results select new ListBoxItem { Tag = r.Id, Content = r.Title };
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
                    portal = _iwaSecuredPortal;
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
