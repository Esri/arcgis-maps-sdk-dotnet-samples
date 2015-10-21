using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.WebMap;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OAuthAuthentication
{
	public partial class MainWindow : Window, IChallengeHandler
	{
		//TODO - Add your server url, client id, and web map id to load. User name and password credentials
		// are entered when the IdentityManager provides a challenge. 
		private const string MyServerUrl = "https://www.arcgis.com/sharing/rest";
		private const string ThisClientId = "<ENTER CLIENT ID>";
		private const string MyInitialWebMapId = "<ENTER WEBMAP ID>";
		private const string StandardAppDesktopRedirectUri = "urn:ietf:wg:oauth:2.0:oob";

		public MainWindow()
		{
			InitializeComponent();

			// Set the Web Map text box to the specified web map
			WebMapID.Text = MyInitialWebMapId;

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

			// Use the OAuthAuthorize class in this project to create a new web browser that contains the OAuth challenge handler.
			IdentityManager.Current.OAuthAuthorizeHandler = new OAuthAuthentication.OAuthAuthorize();
			IdentityManager.Current.ChallengeHandler = this;

			// Specify -1 to generate a permanent refresh token.
			IdentityManager.Current.TokenValidity = -1;

			// Check whether a refresh token has already been generated and stored. If so, credentials will be passed using 
			// that information and the user is automatically logged in. 
			CheckUseRefreshToken();
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
				MessageBox.Show(ex.Message, "Sample Error");
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

				// Identity Manager will handle challenging the user for credentials
				credential = await IdentityManager.Current.GenerateCredentialAsync(
							info.ServiceUri, info.GenerateTokenOptions
							) as OAuthTokenCredential;

				// Switch to UI thread to change control state and content
				this.Dispatcher.Invoke(
				(Action)(() =>
				{
					SignOutButton.IsEnabled = true;
					SignInButton.IsEnabled = false;
					LoadWebMapButton.IsEnabled = true;
					LoggedInUserName.Text = string.Format("Logged in as: {0}", credential.UserName);
					LoggedInUserName.Visibility = Visibility.Visible;
				}));

				// Store the RefreshToken
				if (credential.OAuthRefreshToken != null)
					StoreRefreshToken(credential.OAuthRefreshToken);

				// Not a challenge, user initiated directly
				if (info.Response == null)
					IdentityManager.Current.AddCredential(credential);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			return credential;
		}

		/// <summary>
		/// Signs the user out by removing the credentials from the Identity Manager and deleting the refresh token
		/// </summary>
		private void SignOutClick(object sender, RoutedEventArgs e)
		{
			RemovePreviousCredentials();
			DeleteRefreshToken();

			SignOutButton.IsEnabled = false;
			SignInButton.IsEnabled = true;
			LoadWebMapButton.IsEnabled = false;
			LoggedInUserName.Visibility = Visibility.Collapsed;
			MyMapView.Map = null;

		}

		/// <summary>
		/// Remove any credentials stored in the Identity Manager
		/// </summary>
		private void RemovePreviousCredentials()
		{
			var credentials = IdentityManager.Current.Credentials.ToArray();
			foreach (Credential cred in credentials)
				IdentityManager.Current.RemoveCredential(cred);
		}

		/// <summary>
		/// Load the specified web map. 
		/// </summary>
		private async void LoadWebMapButtonOnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				var portal = await ArcGISPortal.CreateAsync(new Uri(MyServerUrl));
				var portalItem = await ArcGISPortalItem.CreateAsync(portal, WebMapID.Text);

				WebMap webMap = await WebMap.FromPortalItemAsync(portalItem);

				if (webMap != null)
				{
					var myWebMapViewModel = await WebMapViewModel.LoadAsync(webMap, portal);
					MyMapView.Map = myWebMapViewModel.Map;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		#region RefreshToken

		/// <summary>
		/// Check whether a refresh token exists. If yes, then add the credentials associated with the refresh token to the Identity Manager. If not,
		/// return to the app and wait for the user to click the Sign In button. 
		/// </summary>
		private async Task CheckUseRefreshToken()
		{
			try
			{
				// Try loading the refresh token. If there is no refresh token, return to the main app. 
				string refreshToken = LoadRefreshToken();
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

				SignInButton.IsEnabled = false;
				SignOutButton.IsEnabled = true;
				LoadWebMapButton.IsEnabled = true;
				LoggedInUserName.Text = string.Format("Logged in as: {0}", credential.UserName);
				LoggedInUserName.Visibility = Visibility.Visible;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		// "salt" used as additional input for encoding
		private static byte[] salt = { 1, 2, 3, 4, 5 };

		/// <summary>
		/// Store the refresh token. In this case the token is stored to the project settings, but would normally
		/// be stored on the client in a secure location. 
		/// </summary>
		private static void StoreRefreshToken(string refreshToken)
		{
			OAuthAuthentication.Properties.Settings.Default.RefreshToken =
				Convert.ToBase64String(ProtectedData.Protect(
					Encoding.Unicode.GetBytes(refreshToken),
					salt, DataProtectionScope.CurrentUser));

			OAuthAuthentication.Properties.Settings.Default.Save();
		}

		/// <summary>
		/// Load the refresh token.
		/// </summary>
		private static string LoadRefreshToken()
		{
			// Property set in Setting.settings file
			var protectedString = OAuthAuthentication.Properties.Settings.Default.RefreshToken;

			return !string.IsNullOrEmpty(protectedString)
					   ? Encoding.Unicode.GetString(ProtectedData.Unprotect(Convert.FromBase64String(protectedString),
																			salt,
																			DataProtectionScope.CurrentUser)) : null;
		}

		/// <summary>
		/// Delete the refresh token when the user explicitly clicks Sign Out. 
		/// </summary>
		private static void DeleteRefreshToken()
		{
			OAuthAuthentication.Properties.Settings.Default.RefreshToken = null;
			OAuthAuthentication.Properties.Settings.Default.Save();
		}

		#endregion
	}
}
