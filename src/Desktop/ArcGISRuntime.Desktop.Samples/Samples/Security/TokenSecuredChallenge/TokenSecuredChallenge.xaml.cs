// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Security;
using System;
using System.ComponentModel;
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
            
			AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(Challenge);
		}        

        // Base Challenge method that dispatches to the UI thread if necessary
        private async Task<Credential> Challenge(CredentialRequestInfo cri)
		{
			if (Dispatcher == null)
			{
				return await ChallengeUI(cri);
			}
			else
			{
				return await Dispatcher.Invoke(() => ChallengeUI(cri));
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
				loginPanel.Visibility = Visibility.Collapsed;
			}
		}

		// Login button handler - checks entered credentials
		private async void btnLogin_Click(object sender, RoutedEventArgs e)
		{
			if (_loginTCS == null || _loginTCS.Task == null || _loginTCS.Task.AsyncState == null)
				return;

			var loginEntry = _loginTCS.Task.AsyncState as LoginInfo;

			try
			{
				var credentials = await AuthenticationManager.Current.GenerateCredentialAsync(new Uri(loginEntry.ServiceUrl),
					loginEntry.UserName, loginEntry.Password, loginEntry.RequestInfo.GenerateTokenOptions);

				_loginTCS.TrySetResult(credentials);
			}
			catch (Exception ex)
			{
				loginEntry.ErrorMessage = ex.Message;
				loginEntry.AttemptCount++;

				if (loginEntry.AttemptCount >= 3)
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
			ServiceUrl = cri.ServiceUri.GetLeftPart(UriPartial.Path);
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
