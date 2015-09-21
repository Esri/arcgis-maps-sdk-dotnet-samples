using Esri.ArcGISRuntime.Security;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TokenSecuredChallenge
{
	public sealed partial class MainPage : Page
	{
		private TaskCompletionSource<Credential> _loginTCS;

		public MainPage()
		{
			this.InitializeComponent();

			IdentityManager.Current.ChallengeHandler = new ChallengeHandler(Challenge);
	
			this.NavigationCacheMode = NavigationCacheMode.Required;
		}

		/// <summary>
		/// Invoked when this page is about to be displayed in a Frame.
		/// </summary>
		/// <param name="e">Event data that describes how this page was reached.
		/// This parameter is typically used to configure the page.</param>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			// TODO: Prepare page for display here.

			// TODO: If your application contains multiple pages, ensure that you are
			// handling the hardware Back button by registering for the
			// Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
			// If you are using the NavigationHelper provided by some templates,
			// this event is handled for you.
		}

		// Base Challenge method that dispatches to the UI thread if necessary
		private async Task<Credential> Challenge(CredentialRequestInfo cri)
		{
			var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

			if (dispatcher == null)
			{
				return await ChallengeUI(cri);
			}
			else
			{
				await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
				{
					await ChallengeUI(cri);
				});

				return await _loginTCS.Task;
			}
		}

		// Challenge method that prompts for username / password
		private async Task<Credential> ChallengeUI(CredentialRequestInfo cri)
		{
			try
			{
				loginPanel.DataContext = new LoginInfo(cri);
				_loginTCS = new TaskCompletionSource<Credential>(loginPanel.DataContext);

				loginPanel.Visibility = Visibility.Visible;

				return await _loginTCS.Task;
			}
			finally
			{
				LoginFlyout.Hide();
			}
		}

		// Login button handler - checks entered credentials
		private async void btnLogin_Click(object sender, RoutedEventArgs e)
		{
			if (_loginTCS == null || _loginTCS.Task == null || _loginTCS.Task.AsyncState == null)
				return;

			var loginInfo = _loginTCS.Task.AsyncState as LoginInfo;

			try
			{
				var credentials = await IdentityManager.Current.GenerateCredentialAsync(loginInfo.ServiceUrl,
					loginInfo.UserName, loginInfo.Password, loginInfo.RequestInfo.GenerateTokenOptions);

				_loginTCS.TrySetResult(credentials);
			}
			catch (Exception ex)
			{
				loginInfo.ErrorMessage = ex.Message;
				loginInfo.AttemptCount++;

				if (loginInfo.AttemptCount >= 3)
				{
					_loginTCS.TrySetException(ex);
				}
			}
		}
	}

	// Helper class to contain login information
	internal class LoginInfo : INotifyPropertyChanged
	{
		private CredentialRequestInfo _requestInfo;
		public CredentialRequestInfo RequestInfo
		{
			get { return _requestInfo; }
			set { _requestInfo = value; OnPropertyChanged(); }
		}

		private string _serviceUrl;
		public string ServiceUrl
		{
			get { return _serviceUrl; }
			set { _serviceUrl = value; OnPropertyChanged(); }
		}

		private string _userName;
		public string UserName
		{
			get { return _userName; }
			set { _userName = value; OnPropertyChanged(); }
		}

		private string _password;
		public string Password
		{
			get { return _password; }
			set { _password = value; OnPropertyChanged(); }
		}

		private string _errorMessage;
		public string ErrorMessage
		{
			get { return _errorMessage; }
			set { _errorMessage = value; OnPropertyChanged(); }
		}

		private int _attemptCount;
		public int AttemptCount
		{
			get { return _attemptCount; }
			set { _attemptCount = value; OnPropertyChanged(); }
		}

		public LoginInfo(CredentialRequestInfo cri)
		{
			RequestInfo = cri;
			ServiceUrl = new Uri(cri.ServiceUri).GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Query, UriFormat.UriEscaped);
			ErrorMessage = string.Empty;
			AttemptCount = 0;
		}

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

	public class ValueToForegroundColorConverter : IValueConverter
	{

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			SolidColorBrush brush;
			if (value.ToString() == "Initializing")
				brush = new SolidColorBrush(Colors.Red);
			else if (value.ToString() == "Initialized")
				brush = new SolidColorBrush(Colors.Green);
			else
				brush = new SolidColorBrush(Colors.Black);

			return brush;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}
