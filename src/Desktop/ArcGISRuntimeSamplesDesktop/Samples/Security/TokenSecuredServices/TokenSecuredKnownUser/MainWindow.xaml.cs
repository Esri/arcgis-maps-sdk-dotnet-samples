using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Security;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace TokenSecuredServices
{
	public partial class MainWindow : Window
	{
		private TaskCompletionSource<Credential> _loginTCS;

		public MainWindow()
		{
			InitializeComponent();

			IdentityManager.Current.ChallengeHandler = new ChallengeHandler(Challenge);
		}

		private async Task<Credential> Challenge(CredentialRequestInfo cri)
		{
				return await Challenge_KnownCredentials(cri);
		}

		// Challenge method that checks for service access with known credentials
		private async Task<Credential> Challenge_KnownCredentials(CredentialRequestInfo cri)
		{
			try
			{
				// Obtain credentials from a secure source
				string username = "user1";
				string password = (cri.ServiceUri.Contains("USA_secure_user1")) ? "user1" : "pass.word1";

				return await IdentityManager.Current.GenerateCredentialAsync(cri.ServiceUri, username, password, cri.GenerateTokenOptions);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Access to " + cri.ServiceUri + " denied. " + ex.Message, "Credential Error");
			}

			return await Task.FromResult<Credential>(null);
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

		public LoginInfo(CredentialRequestInfo cri, string user, string pwd)
		{
			RequestInfo = cri;
			ServiceUrl = new Uri(cri.ServiceUri).GetLeftPart(UriPartial.Path);
			UserName = user;
			Password = pwd;
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
}
