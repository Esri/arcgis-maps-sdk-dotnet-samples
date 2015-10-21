using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.WebMap;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OAuthAuthorization
{
	public sealed partial class MainPage : Page, IChallengeHandler
	{
		//TODO - Add your server url, client id, and web map id to load. User name and password credentials
		// are entered when the IdentityManager provides a challenge. 
		private const string MyServerUrl = "https://www.arcgis.com/sharing/rest";
		private const string ThisClientId = "<ENTER CLIENT ID>";
		private const string MyInitialWebMapId = "<ENTER WEBMAP ID>";
		private const string StandardAppDesktopRedirectUri = "urn:ietf:wg:oauth:2.0:oob";

		// String to keep track of the currently-selected RadioButton
		private string _selectedRadio;

		public MainPage()
		{
			this.InitializeComponent();

			// Set the Web Map text box to the specified web map
			WebMapTextBox.Text = MyInitialWebMapId;

			// Check the WebBrokerRadio button and wire up event handlers
			WebBrokerRadio.IsChecked = true;
			WebBrokerRadio.Checked += WebBrokerRadio_Checked;
			WebBrowserRadio.Checked +=WebBrokerRadio_Checked;
			
			// Set to the currently-selected RadioButton
			_selectedRadio = WebBrokerRadio.Content.ToString();
			
			UpdateIdentityManager();

			// Check whether a refresh token has already been generated and stored. If so, credentials will be passed using 
			// that information and the user is automatically logged in. 
			CheckUseRefreshToken();
		}

		/// <summary>
		/// Handle when user changes from Web Broker to Web Browser for OAuth.
		/// </summary>
		private void WebBrokerRadio_Checked(object sender, RoutedEventArgs e)
		{
			var newRadio = (sender as RadioButton).Content.ToString();

			if (newRadio.ToLower() == _selectedRadio.ToLower())
				return;

			_selectedRadio = newRadio;

			UpdateIdentityManager();
		}

		/// <summary>
		/// Register the server and OAuthAuthorize class with the Identity Manager.
		/// </summary>
		private void UpdateIdentityManager()
		{
			// Register the server information with the IdentityManager.
			IdentityManager.Current.RegisterServer(
			   new ServerInfo
			   {
				   ServerUri = MyServerUrl,
				   OAuthClientInfo = new OAuthClientInfo
				   {
					   ClientId = ThisClientId,
					   RedirectUri = StandardAppDesktopRedirectUri
				   },
				   TokenAuthenticationType = TokenAuthenticationType.OAuthAuthorizationCode
			   }
			);

			// Use the OAuthAuthorize class in this project to create a new web view that contains the OAuth challenge handler.
			if (WebBrokerRadio.IsChecked.Value)
				IdentityManager.Current.OAuthAuthorizeHandler = new OAuthAuthorize();
			else if (WebBrowserRadio.IsChecked.Value)
				IdentityManager.Current.OAuthAuthorizeHandler = new OAuthAuthorizeWebView();

			IdentityManager.Current.ChallengeHandler = this;

			// -1 = maximum token expiration. 
			IdentityManager.Current.TokenValidity = -1;
		}

		/// <summary>
		/// Removes any existing credentials in the IdentityManager and then attempts to authenticate the user.
		/// </summary>
		private async void SignInClick(object sender, RoutedEventArgs e)
		{
			RemovePreviousCredentials();

			try
			{
				await CreateCredentialAsync(new CredentialRequestInfo()
							{
								ServiceUri = MyServerUrl,
								AuthenticationType = AuthenticationType.Token
							});
			}
			catch (Exception ex)
			{
				ShowDialog(ex.Message);
			}
		}

		/// <summary>
		/// Generate the credentials with the existing credentials or challenge the user to enter credentials.
		/// </summary>
		public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
		{
			OAuthTokenCredential credential = null;

			try
			{
				// Portal "single sign in" check when trying to load webmap
				if (info.Response != null && info.ServiceUri.StartsWith(MyServerUrl))
				{
					Credential existingCredential = IdentityManager.Current.FindCredential(info.ServiceUri);
					if (existingCredential != null)
					{
						// Already logged in and current user does not have access
						throw new Exception("Current logged in user does not have access.");
					}
				}

				// Modify generate token options if necessary
				if (info.GenerateTokenOptions == null)
					info.GenerateTokenOptions = new GenerateTokenOptions();

				credential = await IdentityManager.Current.GenerateCredentialAsync(
							info.ServiceUri, info.GenerateTokenOptions
							) as OAuthTokenCredential;

				// Switch to UI thread to change control state and content
				await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
				() =>
				{
					SignInButton.IsEnabled = false;
					SignOutButton.IsEnabled = true;
					LoadWebMapButton.IsEnabled = true;
					LoggedInUserName.Text = string.Format("Logged in as: {0}", credential.UserName);
					LoggedInUserName.Visibility = Visibility.Visible;
				});

				if (credential.OAuthRefreshToken != null)
					StoreRefreshToken(credential.OAuthRefreshToken);

				// Not a challenge, user initiated directly
				if (info.Response == null)
					IdentityManager.Current.AddCredential(credential);
			}
			catch (Exception ex)
			{
				throw (ex); // Exception will be reported in the calling function
			}

			return credential;
		}

		/// <summary>
		/// Signs the user out by removing the credentials from the Identity Manager and deleting the refresh token
		/// </summary>
		private void SignOutClick(object sender, RoutedEventArgs e)
		{
			SignOut();
		}

		private void SignOut()
		{
			try
			{
				RemovePreviousCredentials();
				DeleteRefreshToken();

				SignOutButton.IsEnabled = false;
				SignInButton.IsEnabled = true;
				LoadWebMapButton.IsEnabled = false;
				LoggedInUserName.Visibility = Visibility.Collapsed;
				MyMapView.Map = null;

			}
			catch (Exception ex)
			{
				ShowDialog(ex.Message);
			}
		}

		/// <summary>
		/// Remove any credentials stored in the Identity Manager
		/// </summary>
		private void RemovePreviousCredentials()
		{
			// Remove old credentials
			var credentials = IdentityManager.Current.Credentials.ToArray();
			foreach (Credential crd in credentials)
				IdentityManager.Current.RemoveCredential(crd);
		}

		/// <summary>
		/// Load the specified web map. 
		/// </summary>
		private async void LoadWebMapButtonOnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				var portal = await ArcGISPortal.CreateAsync(new Uri(MyServerUrl));
				var portalItem = await ArcGISPortalItem.CreateAsync(portal, WebMapTextBox.Text);

				WebMap webMap = await WebMap.FromPortalItemAsync(portalItem);

				if (webMap != null)
				{
					var myWebMapViewModel = await WebMapViewModel.LoadAsync(webMap, portal);
					MyMapView.Map = myWebMapViewModel.Map;
				}
			}
			catch (Exception ex)
			{
				ShowDialog(ex.Message);
			}
		}

		async void ShowDialog(string msg)
		{
			MessageDialog dlg = new MessageDialog(msg);
			await dlg.ShowAsync();
		}

		/// <summary>
		/// Check whether a refresh token exists. If yes, then add the credentials associated with the refresh token to the Identity Manager. If not,
		/// return to the app and wait for the user to click the Sign In button. 
		/// </summary>
		private async void CheckUseRefreshToken()
		{
			try
			{
				string refreshToken = await LoadRefreshToken();
				if (string.IsNullOrEmpty(refreshToken))
					return;

				OAuthTokenCredential credential = new OAuthTokenCredential();
				credential.OAuthRefreshToken = refreshToken;
				credential.ServiceUri = MyServerUrl;

				credential.GenerateTokenOptions = new GenerateTokenOptions
				{
					TokenAuthenticationType = TokenAuthenticationType.OAuthAuthorizationCode,
				};

				await credential.RefreshTokenAsync();

				IdentityManager.Current.AddCredential(credential);

				SignOutButton.IsEnabled = true;
				SignInButton.IsEnabled = false;
				LoadWebMapButton.IsEnabled = true;
				LoggedInUserName.Text = string.Format("Logged in as: {0}", credential.UserName);
				LoggedInUserName.Visibility = Visibility.Visible;
			}
			catch (Exception ex)
			{
				ShowDialog(ex.Message);
			}
		}

		// Secure storage of refresh token on client
		Windows.Storage.ApplicationDataContainer _localSettings =
			   Windows.Storage.ApplicationData.Current.LocalSettings;
		DataProtectionProvider _provider = new DataProtectionProvider("LOCAL=user");


		/// <summary>
		/// Load the refresh token.
		/// </summary>
		private async Task<string> LoadRefreshToken()
		{
			string protectedString =
				Windows.Storage.ApplicationData.Current.LocalSettings.Values["refreshToken"] as string;

			if (string.IsNullOrEmpty(protectedString))
				return null;

			IBuffer buffUnProtected =
				await _provider.UnprotectAsync(CryptographicBuffer.DecodeFromBase64String(protectedString));

			return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffUnProtected);
		}

		/// <summary>
		/// Store the refresh token to the local settings
		/// </summary>
		private async void StoreRefreshToken(string refreshToken)
		{
			// Encode the refresh token to a buffer
			IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(refreshToken, BinaryStringEncoding.Utf8);

			// Encrypt the refresh token
			IBuffer buffProtected = await _provider.ProtectAsync(buffMsg);

			_localSettings.Values["refreshToken"] = CryptographicBuffer.EncodeToBase64String(buffProtected);
		}

		/// <summary>
		/// Delete the refresh token when the user explicitly clicks Sign Out. 
		/// </summary>
		private void DeleteRefreshToken()
		{
			Windows.Storage.ApplicationData.Current.LocalSettings.Values["refreshToken"] = null;
		}

	}
}
