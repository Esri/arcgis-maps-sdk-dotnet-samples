using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.WebMap;
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

namespace IWAAuthentication
{
	public sealed partial class MainPage : Page
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

		// Variable to store the result of a login task
		TaskCompletionSource<Credential> _loginTaskCompletionSrc;

		public MainPage()
		{
			this.InitializeComponent();

            // Define a challenge handler method for the IdentityManager 
            // (this method handles getting credentials when a secured resource is encountered)
            // Note: unlike a WPF app, your current system credentials will NOT be used by default in a Store app and
            //       you will be (initially) challenged even for resources to which your system account has access.
            //       Once you provide your credentials, you will not be challenged again for them
			IdentityManager.Current.ChallengeHandler = new ChallengeHandler(Challenge);

            // Note: you could add code like the following to define network credentials for your app rather than prompting the user
            // ...
            //  var otherCredential = new ArcGISNetworkCredential()
            //  {
            //      Credentials = new NetworkCredential("user", "p@ssWoRd", "MyDomain"),
            //      ServiceUri = SecuredPortalUrl
            //  };
            //  IdentityManager.Current.AddCredential(otherCredential);
		}

		/// <summary>
        /// Base Challenge method that dispatches to the UI thread if necessary to create a credential
		/// </summary>
        /// <param name="info">Information about a secured resource (its URI, for example)</param>
        /// <returns>A task that returns a credential for attempting access to a secured resource</returns>
		private async Task<Credential> Challenge(CredentialRequestInfo info)
		{
			// Get the dispatcher for the UI thread
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

        /// <summary> 
        /// Challenge method that prompts the user for network credential information (username / password / domain)
        /// </summary>
        /// <param name="info">Information about a secured resource (its URI, for example)</param>
        /// <returns>A task that returns a credential for attempting access to a secured resource</returns>
		private async Task<Credential> ChallengeUI(CredentialRequestInfo info)
		{
			try
			{
                // Create a new instance of the LoginInfo helper to store info needed to create a credential
                var loginInfo = new LoginInfo(info);

                // Set the login panel data context with the helper class
                // (two-way binding will provide access to the data entered by the user)
                LoginPanel.DataContext = loginInfo;

                // Show the login UI; hide the load map UI
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
                // The user is done logging in (or cancelled); hide the login UI, show the load map UI
                LoginPanel.Visibility = Visibility.Collapsed;
                LoadMapPanel.Visibility = Visibility.Visible;
			}
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
			ProgressStatus.Visibility = Windows.UI.Xaml.Visibility.Visible;
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
				ProgressStatus.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
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
			ProgressStatus.Visibility = Windows.UI.Xaml.Visibility.Visible;
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
            catch (TaskCanceledException tce)
            {
                sb.AppendLine(tce.Message);
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
				ProgressStatus.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
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

        /// <summary>
        /// Click handler for the login button. Uses the provided info to create a network credential.
        /// </summary>
		private void LoginButton_Click(object sender, RoutedEventArgs e)
		{
            // If no login information is available from the Task, return
			if (_loginTaskCompletionSrc == null || _loginTaskCompletionSrc.Task == null || _loginTaskCompletionSrc.Task.AsyncState == null)
				return;

            // Get the login info (helper class) that was stored with the task
			var loginInfo = _loginTaskCompletionSrc.Task.AsyncState as LoginInfo;

			try
			{
                // Create a new System.Net.NetworkCredential with the username, password, and domain provided
                var networkCredential = new NetworkCredential(loginInfo.UserName, loginInfo.Password, loginInfo.Domain);
                // Create a new ArcGISNetworkCredential with the NetworkCredential and URI of the secured resource
                var credential = new ArcGISNetworkCredential
                {
                    Credentials = networkCredential,
                    ServiceUri = loginInfo.ServiceUrl
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

        /// <summary>
        /// Click handler for the Cancel login button
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Set the login task status to canceled
            _loginTaskCompletionSrc.TrySetCanceled();
        }
	}

    /// <summary>
    /// Helper class to contain login information: Username, Password, Domain, Resource URL
    /// Implements INotifyPropertyChanged to support data binding
    /// </summary>
	public class LoginInfo : INotifyPropertyChanged
	{
        /// <summary>
        /// Esri.ArcGISRuntime.Security.CredentialRequestInfo with information about a credential challenge
        /// </summary>
		private CredentialRequestInfo _requestInfo;
		public CredentialRequestInfo RequestInfo
		{
			get { return _requestInfo; }
			set { _requestInfo = value; OnPropertyChanged(); }
		}

        /// <summary>
        /// URL of the secure resource
        /// </summary>
		private string _serviceUrl;
		public string ServiceUrl
		{
			get { return _serviceUrl; }
			set { _serviceUrl = value; OnPropertyChanged(); }
		}

        /// <summary>
        /// Username for the credential
        /// </summary>
		private string _userName;
		public string UserName
		{
			get { return _userName; }
			set { _userName = value; OnPropertyChanged(); }
		}

        /// <summary>
        /// Password for the credential
        /// </summary>
		private string _password;
		public string Password
		{
			get { return _password; }
			set { _password = value; OnPropertyChanged(); }
		}

        /// <summary>
        /// Domain for the network credential
        /// </summary>
		private string _domain;
		public string Domain
		{
			get { return _domain; }
			set { _domain = value; OnPropertyChanged(); }
		}

        /// <summary>
        /// Login error messages
        /// </summary>
		private string _errorMessage;
		public string ErrorMessage
		{
			get { return _errorMessage; }
			set { _errorMessage = value; OnPropertyChanged(); }
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cri">information about a credential challenge</param>
		public LoginInfo(CredentialRequestInfo cri)
		{
            // Store the request info
			RequestInfo = cri;
            // Build the service URL from the request info
			ServiceUrl = new Uri(cri.ServiceUri).GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Query, UriFormat.UriEscaped);
            // Login info is empty by default, will be populated by the user
			UserName = string.Empty;
			Password = string.Empty;
			Domain = string.Empty;
			ErrorMessage = string.Empty;
		}

        /// <summary>
        /// Raise an event when properties change to make sure data bindings are updated
        /// </summary>
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
