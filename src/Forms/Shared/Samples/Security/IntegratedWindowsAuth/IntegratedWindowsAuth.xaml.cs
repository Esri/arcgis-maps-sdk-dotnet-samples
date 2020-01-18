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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

// Important:
//    You must add the "Private Networks" capability to use Integrated Windows Authentication (IWA)
//    in your UWP project. Add this capability by checking "Private Networks (Client and Server)"
//    in your project's Package.appxmanifest file.

namespace ArcGISRuntime.Samples.IntegratedWindowsAuth
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
           "Integrated Windows Authentication",
           "Security",
           "This sample demonstrates how to use a Windows login to authenticate with a portal that is secured with IWA.",
           "1. Enter the URL to your IWA-secured portal.\n2. Click the button to search for web maps on the secure portal.\n3. You will be prompted for a user name, password, and domain to authenticate with the portal.\n4. If you authenticate successfully, search results will display.",
           "Authentication, Security, Windows")]
    [ArcGISRuntime.Samples.Shared.Attributes.XamlFiles("LoginPage.xaml")]
    [ArcGISRuntime.Samples.Shared.Attributes.ClassFile("LoginPage.xaml.cs")]
    public partial class IntegratedWindowsAuth : ContentPage
    {
        // A TaskCompletionSource to store the result of a login task.
        private TaskCompletionSource<Credential> _loginTaskCompletionSrc;

        // Page for the user to enter login information.
        private LoginPage _loginPage;

        // The ArcGIS Online URL for searching public web maps.
        private string _publicPortalUrl = "http://www.arcgis.com";

        // The public and secured portals.
        ArcGISPortal _iwaSecuredPortal = null;
        ArcGISPortal _publicPortal = null;

        // A dictionary of portal items and their names.
        private Dictionary<string, PortalItem> _webMapPortalItems;

        // Track if the user is looking at search results from the public or secured portal.
        bool _usingPublicPortal;

        public IntegratedWindowsAuth()
        {
            InitializeComponent();

            // Call a function to display a simple map and set up the AuthenticationManager.
            Initialize();
        }

        private void Initialize()
        {
            // Show the light gray canvas basemap.
            MyMapView.Map = new Map(Basemap.CreateLightGrayCanvasVector());

            // Define a challenge handler method for the AuthenticationManager 
            // (this method handles getting credentials when a secured resource is encountered)
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Create the login UI (will display when the user accesses a secured resource)
            _loginPage = new LoginPage();

            // Set up event handlers for when the user completes the login entry or cancels
            _loginPage.OnLoginInfoEntered += LoginInfoEntered;
            _loginPage.OnCanceled += LoginCanceled;
        }

        // Search the IWA-secured portal for web maps and display the results in a list.
        private async void SearchSecureMapsButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Get the value entered for the secure portal URL.
                string securedPortalUrl = PortalUrlEntry.Text.Trim();
#if WINDOWS_UWP
                // Create an instance of the IWA-secured portal, the user may be challenged for access.
                _iwaSecuredPortal = await ArcGISPortal.CreateAsync(new Uri(securedPortalUrl), true);
#else
                // Make sure a portal URL has been entered in the text box.
                if (string.IsNullOrEmpty(securedPortalUrl))
                {
                    await DisplayAlert("Missing URL", "Please enter the URL of the secured portal.", "OK");
                    return;
                }

                // See if a credential exists for this portal in the AuthenticationManager
                // If a credential is not found, the user will be prompted for login info
                CredentialRequestInfo info = new CredentialRequestInfo
                {
                    ServiceUri = new Uri(securedPortalUrl),
                    AuthenticationType = AuthenticationType.NetworkCredential
                };
                Credential cred = await AuthenticationManager.Current.GetCredentialAsync(info, false);

                // Create an instance of the IWA-secured portal, the user may be challenged for access.
                _iwaSecuredPortal = await ArcGISPortal.CreateAsync(new Uri(securedPortalUrl), true);
#endif
                // Call a function to search the portal.
                SearchPortal(_iwaSecuredPortal);

                // Set a variable that indicates this is the secure portal.
                // When a map is loaded from the results, will need to know which portal it came from.
                _usingPublicPortal = false;

                // Report the username for this connection
                if (_iwaSecuredPortal.User != null)
                {
                    MessagesTextBlock.Text = "Connected as: " + _iwaSecuredPortal.User.UserName;
                }
                else
                {
                    // This shouldn't happen (if the portal is truly secured)!
                    MessagesTextBlock.Text = "Connected anonymously";
                }
            }
            catch (TaskCanceledException)
            {
                // Report canceled login.
                MessagesTextBlock.Text = "Login was canceled";
            }
            catch (Exception ex)
            {
                // Report errors (connecting to the secured portal, for example).
                MessagesTextBlock.Text = ex.Message;
            }
            finally
            {
                // Set the task completion source to null so user can attempt another login (if it failed).
                _loginTaskCompletionSrc = null;
            }
        }

        // Search the public portal (ArcGIS Online, for example) for web maps and display the results in a list.
        private async void SearchPublicMapsButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Create an instance of the public portal.
                _publicPortal = await ArcGISPortal.CreateAsync(new Uri(_publicPortalUrl));

                // Call a function to search the portal.
                SearchPortal(_publicPortal);

                // Set a variable that indicates this is the public portal.
                // When a map is loaded from the results, will need to know which portal it came from.
                _usingPublicPortal = true;
            }
            catch (Exception ex)
            {
                // Report errors, if any.
                MessagesTextBlock.Text = ex.Message;
            }
        }

        private async void SearchPortal(ArcGISPortal currentPortal)
        {
            // Clear any existing results.
            WebMapListView.ItemsSource = null;

            // Show status message.
            MessagesTextBlock.Text = "Searching for web map items on the portal at " + currentPortal.Uri.AbsoluteUri;
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
                var resultItems = from r in items.Results select new KeyValuePair<string, PortalItem>(r.Title, r); 

                // Add the items to a dictionary.
                _webMapPortalItems = new Dictionary<string, PortalItem>();
                foreach (var itm in resultItems)
                {
                    _webMapPortalItems.Add(itm.Key, itm.Value);
                }

                // Show the portal item titles in the list view.
                WebMapListView.ItemsSource = _webMapPortalItems.Keys;

                // Enable the button to load a selected web map item.
                LoadWebMapButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                // Report errors searching the portal.
                messageBuilder.AppendLine(ex.Message);
            }
            finally
            {
                // Show messages.
                MessagesTextBlock.Text = messageBuilder.ToString();
            }
        }

        private async void AddMapItemClick(object sender, EventArgs e)
        {
            // Get a web map from the selected portal item and display it in the map view.
            if (WebMapListView.SelectedItem == null)
            {
                await DisplayAlert("Select item", "No web map item is selected.", "OK");
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

                // Get the portal item for the selected item (read it from the dictionary).
                var portalItem = _webMapPortalItems[WebMapListView.SelectedItem.ToString()];

                if (portalItem != null)
                {
                    // Create a Map using the web map (portal item).
                    Map webMap = new Map(portalItem);

                    // Create a new MapView control to display the Map.
                    Esri.ArcGISRuntime.Xamarin.Forms.MapView myMapView = new Esri.ArcGISRuntime.Xamarin.Forms.MapView
                    {
                        Map = webMap
                    };

                    // Add the MapView to the app.
                    MyMapGrid.Children.Add(myMapView);
                }

                // Report success.
                statusInfo.AppendLine("Successfully loaded web map from item #" + portalItem.ItemId + " from " + portal.Uri.Host);
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

        // AuthenticationManager.ChallengeHandler function that prompts the user for login information to create a credential
        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // Ignore challenges for OAuth (might come from secured layers in public web maps, for example).
            if(info.AuthenticationType != AuthenticationType.NetworkCredential)
            {
                Console.WriteLine("Authentication for " + info.ServiceUri.Host + " skipped.");
                return null;
            }

            // Return if authentication is already in process
            if (_loginTaskCompletionSrc != null && !_loginTaskCompletionSrc.Task.IsCanceled) { return null; }

            // Create a new TaskCompletionSource for the login operation
            // (passing the CredentialRequestInfo object to the constructor will make it available from its AsyncState property)
            _loginTaskCompletionSrc = new TaskCompletionSource<Credential>(info);

            // Show the login controls on the UI thread
            // OnLoginInfoEntered event will return the values entered (username, password, and domain)
            Device.BeginInvokeOnMainThread(async () => await Navigation.PushAsync(_loginPage));

            // Return the login task, the result will be ready when completed (user provides login info and clicks the "Login" button)
            return await _loginTaskCompletionSrc.Task;
        }

        // Handle the OnLoginEntered event from the login UI
        // LoginEventArgs contains the username, password, and domain that were entered
        private void LoginInfoEntered(object sender, LoginEventArgs e)
        {
            // Make sure the task completion source has all the information needed
            if (_loginTaskCompletionSrc == null ||
                _loginTaskCompletionSrc.Task == null ||
                _loginTaskCompletionSrc.Task.AsyncState == null)
            {
                return;
            }

            try
            {
                // Get the associated CredentialRequestInfo (will need the URI of the service being accessed)
                CredentialRequestInfo requestInfo = _loginTaskCompletionSrc.Task.AsyncState as CredentialRequestInfo;

                // Create a new network credential using the values entered by the user
                var netCred = new System.Net.NetworkCredential(e.Username, e.Password, e.Domain);

                // Create a new ArcGIS network credential to hold the network credential and service URI
                var arcgisCred = new ArcGISNetworkCredential
                {
                    Credentials = netCred,
                    ServiceUri = requestInfo.ServiceUri
                };

                // Set the task completion source result with the ArcGIS network credential
                // AuthenticationManager is waiting for this result and will add it to its Credentials collection
                _loginTaskCompletionSrc.TrySetResult(arcgisCred);
            }
            catch (Exception ex)
            {
                // Unable to create credential, set the exception on the task completion source
                _loginTaskCompletionSrc.TrySetException(ex);
            }
            finally
            {
                // Dismiss the login controls
                Navigation.PopAsync();
            }
        }

        private void LoginCanceled(object sender, EventArgs e)
        {
            // Dismiss the login controls
            Navigation.PopAsync();

            // Cancel the task completion source task
            _loginTaskCompletionSrc.TrySetCanceled();
        }
    }
}