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
using Esri.ArcGISRuntime.UI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
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

        // Variable to store the result of a login task
        TaskCompletionSource<Credential> _loginTaskCompletionSrc;

        public MainPage()
        {
            this.InitializeComponent();

            // Call a function to set up the AuthenticationManager and add a hard-coded credential (if defined)
            Initialize();
        }

        private void Initialize()
        {
            // Define a challenge handler method for the AuthenticationManager 
            // (this method handles getting credentials when a secured resource is encountered)
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Note: unlike a WPF app, your current system credentials will NOT be used by default in a Store app and
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

        // Function that prompts the user for login information to create a credential
        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // Prompting the user must happen on the UI thread, use Dispatcher if necessary
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            // If no dispatcher, call the ChallengeUI method directly to get user input
            if (dispatcher == null)
            {
                return await ChallengeUI(info);
            }
            else
            {
                // Use the dispatcher to show the login panel on the UI thread, then await completion of the task
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        // Call the method that shows the login panel and creates a credential 
                        await ChallengeUI(info);
                    }
                    catch (TaskCanceledException)
                    {
                        // The user clicked the "Cancel" button, login panel will close
                    }
                });

                // return the task
                return await _loginTaskCompletionSrc.Task;
            }
        }

        // Challenge method that prompts the user for network credential information (user name / password / domain)
        private async Task<Credential> ChallengeUI(CredentialRequestInfo info)
        {
            try
            {
                // Create a new instance of LoginInfo (defined in this project) to store credential info
                var loginInfo = new LoginInfo(info);

                // Set the login panel data context with the LoginInfo object
                // (two-way binding will provide access to the data entered by the user)
                LoginPanel.DataContext = loginInfo;

                // Show the login UI and hide the load map UI
                LoginPanel.Visibility = Visibility.Visible;
                LoadMapPanel.Visibility = Visibility.Collapsed;

                // Create a new TaskCompletionSource for the login operation
                // (passing the loginInfo helper to the constructor will make it available from the Task's AsyncState property) 
                _loginTaskCompletionSrc = new TaskCompletionSource<Credential>(loginInfo);

                // Return the login task, result will be ready when completed (user provides login info and clicks the "Login" button)
                return await _loginTaskCompletionSrc.Task;
            }
            finally
            {
                // The user is done logging in (or canceled); hide the login UI, show the load map UI
                LoginPanel.Visibility = Visibility.Collapsed;
                LoadMapPanel.Visibility = Visibility.Visible;
            }
        }

        private async void SearchPublicMapsButtonClick(object sender, RoutedEventArgs e)
        {
            // Set the flag variable to indicate this is the public portal
            // (if the user wants to load a map, will need to know which portal it came from)
            _usingPublicPortal = true;

            MapItemListBox.Items.Clear();

            // Show status message and the status bar
            MessagesTextBlock.Text = "Searching for web map items on the public portal.";
            ProgressStatus.Visibility = Visibility.Visible;
            // Store information about the portal connection
            var connectionInfo = new StringBuilder();

            try
            {
                // Create an instance of the public portal
                _publicPortal = await ArcGISPortal.CreateAsync(new Uri(PublicPortalUrl));

                // Report a successful connection
                connectionInfo.AppendLine("Connected to the portal on " + _publicPortal.Uri.Host);
                connectionInfo.AppendLine("Version: " + _publicPortal.CurrentVersion);

                // Report the user name used for this connection
                if (_publicPortal.CurrentUser != null)
                {
                    connectionInfo.AppendLine("Connected as: " + _publicPortal.CurrentUser.UserName);
                }
                else
                {
                    connectionInfo.AppendLine("Anonymous");
                }

                // Search the public portal for web maps
                // (exclude the term "web mapping application" since it also contains the string "web map")
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
                connectionInfo.AppendLine(ex.Message);
            }
            finally
            {
                // Show messages, hide progress bar
                MessagesTextBlock.Text = connectionInfo.ToString();
                ProgressStatus.Visibility = Visibility.Collapsed;
            }
        }

        private async void SearchSecureMapsButtonClick(object sender, RoutedEventArgs e)
        {
            // Set the flag variable to indicate this is the secure portal
            // (if the user wants to load a map, will need to know which portal it came from)
            _usingPublicPortal = false;

            // Clear any current items in the list
            MapItemListBox.Items.Clear();

            // Show status message and the status bar
            MessagesTextBlock.Text = "Searching for web map items on the secure portal.";
            ProgressStatus.Visibility = Visibility.Visible;

            // Store connection information to report 
            var connectionInfo = new StringBuilder();

            try
            {
                // Create an instance of the IWA-secured portal
                _iwaSecuredPortal = await ArcGISPortal.CreateAsync(new Uri(SecuredPortalUrl));

                // Report a successful connection
                connectionInfo.AppendLine("Connected to the portal on " + _iwaSecuredPortal.Uri.Host);
                connectionInfo.AppendLine("Version: " + _iwaSecuredPortal.CurrentVersion);

                // Report the user name used for this connection
                if (_iwaSecuredPortal.CurrentUser != null)
                {
                    connectionInfo.AppendLine("Connected as: " + _iwaSecuredPortal.CurrentUser.UserName);
                }
                else
                {
                    // This shouldn't happen, need to authentication to connect
                    connectionInfo.AppendLine("Anonymous?!");
                }

                // Search the secured portal for web maps
                // (exclude the term "web mapping application" since it also contains the string "web map")
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
                connectionInfo.AppendLine(ex.Message);
            }
            finally
            {
                // Show messages, hide progress bar
                MessagesTextBlock.Text = connectionInfo.ToString();
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
                var portalItem = await ArcGISPortalItem.CreateAsync(portal, itemId);

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

        private void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            // If no login information is available from the Task, return
            if (_loginTaskCompletionSrc == null || _loginTaskCompletionSrc.Task == null || _loginTaskCompletionSrc.Task.AsyncState == null)
                return;

            // Get the login info (helper class) that was stored with the task
            var loginInfo = _loginTaskCompletionSrc.Task.AsyncState as LoginInfo;

            try
            {
                // Create a new System.Net.NetworkCredential with the user name, password, and domain provided
                var networkCredential = new NetworkCredential(loginInfo.UserName, loginInfo.Password, loginInfo.Domain);

                // Create a new ArcGISNetworkCredential with the NetworkCredential and URI of the secured resource
                var credential = new ArcGISNetworkCredential
                {
                    Credentials = networkCredential,
                    ServiceUri = new Uri(loginInfo.ServiceUrl)
                };

                // Set the result of the login task with the new ArcGISNetworkCredential
                _loginTaskCompletionSrc.TrySetResult(credential);
            }
            catch (Exception ex)
            {
                // Report login exceptions at the bottom of the dialog
                loginInfo.ErrorMessage = ex.Message;
            }
        }
        
        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            // Set the login task status to canceled
            _loginTaskCompletionSrc.TrySetCanceled();
        }
    }

    // A helper class to hold information about a network credential
    public class LoginInfo : INotifyPropertyChanged
    {
        // Esri.ArcGISRuntime.Security.CredentialRequestInfo with information about a credential challenge
        private CredentialRequestInfo _requestInfo;
        public CredentialRequestInfo RequestInfo
        {
            get { return _requestInfo; }
            set { _requestInfo = value; OnPropertyChanged(); }
        }
        
        // URL of the secure resource
        private string _serviceUrl;
        public string ServiceUrl
        {
            get { return _serviceUrl; }
            set { _serviceUrl = value; OnPropertyChanged(); }
        }
        
        // User name for the credential
        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; OnPropertyChanged(); }
        }

        // Password for the credential
        private string _password;
        public string Password
        {
            get { return _password; }
            set { _password = value; OnPropertyChanged(); }
        }

        // Domain for the network credential
        private string _domain;
        public string Domain
        {
            get { return _domain; }
            set { _domain = value; OnPropertyChanged(); }
        }

        // Login error messages
        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; OnPropertyChanged(); }
        }
        
        public LoginInfo(CredentialRequestInfo requestInfo)
        {
            // Store the request info
            RequestInfo = requestInfo;

            // Build the service URL from the request info
            ServiceUrl = requestInfo.ServiceUri.AbsoluteUri; 
            
            // Login info is empty by default, will be populated by the user
            UserName = string.Empty;
            Password = string.Empty;
            Domain = string.Empty;
            ErrorMessage = string.Empty;
        }

        // Raise an event when properties change to make sure data bindings are updated
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
