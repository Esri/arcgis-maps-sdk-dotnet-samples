// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TokenSecuredChallenge
{
    public sealed partial class MainPage : Page
    {
        // Constants for the public and secured map service URLs
        private const string PublicMapServiceUrl = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer";
        private const string SecureMapServiceUrl = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA_secure_user1/MapServer";

        // Constants for the public and secured layer names
        private const string PublicLayerName = "World Street Map - Public";
        private const string SecureLayerName = "USA - Secure";

        // Task completion source to track a login attempt
        private TaskCompletionSource<Credential> _loginTaskCompletionSource;

        public MainPage()
        {
            this.InitializeComponent();

            // Define a method to challenge the user for credentials when a secured resource is encountered
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(Challenge);

            // Call a function to create a map and add public and secure layers
            Initialize();
        }

        private void Initialize()
        {
            // Create the public layer and provide a name
            var publicLayer = new ArcGISTiledLayer(new Uri(PublicMapServiceUrl));
            publicLayer.Name = PublicLayerName;

            // Set the data context for the public layer stack panel controls (to report name and load status)
            PublicLayerPanel.DataContext = publicLayer;

            // Create the secured layer and provide a name
            var tokenSecuredLayer = new ArcGISMapImageLayer(new Uri(SecureMapServiceUrl));
            tokenSecuredLayer.Name = SecureLayerName;

            // Set the data context for the secure layer stack panel controls (to report name and load status)
            SecureLayerPanel.DataContext = tokenSecuredLayer;

            // Create a new map and add the layers
            var myMap = new Map();
            myMap.OperationalLayers.Add(publicLayer);
            myMap.OperationalLayers.Add(tokenSecuredLayer);

            // Add the map to the map view
            MyMapView.Map = myMap;
        }

        private async Task<Credential> Challenge(CredentialRequestInfo info)
        {
            // Call code to get user credentials
            // Make sure it runs on the UI thread
            if (this.Dispatcher == null)
            {
                // No current dispatcher, code is already running on the UI thread
                return await GetUserCredentialsFromUI(info);
            }
            else
            {
                // Use the dispatcher to invoke the challenge UI
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        await GetUserCredentialsFromUI(info);
                    }
                    catch
                    {
                        // Login was canceled or unsuccessful, dialog will close
                    }
                });
            }

            // Use the task completion source to return the results
            return await _loginTaskCompletionSource.Task;
        }

        private async Task<Credential> GetUserCredentialsFromUI(CredentialRequestInfo info)
        {
            // Show the login UI
            try
            {
                // Create a new LoginInfo to store the entered username and password
                // Pass the CredentialRequestInfo object so the resource URI can be stored
                var loginInputInfo = new LoginInfo(info);

                // Set the login UI data context with the LoginInfo
                loginPanel.DataContext = loginInputInfo;

                // Show the login UI
                loginPanel.Visibility = Visibility.Visible;

                // Create a new task completion source to return the user's login when complete
                // Set the login UI data context (LoginInfo object) as the AsyncState so it can be retrieved later
                _loginTaskCompletionSource = new TaskCompletionSource<Credential>(loginPanel.DataContext);

                // Return the task from the completion source
                // When the login button on the UI is clicked, the info will be returned for creating the credential
                return await _loginTaskCompletionSource.Task;
            }
            finally
            {
                // Hide the login UI
                loginPanel.Visibility = Visibility.Collapsed;
            }
        }

        // Handle the click event for the login button on the login UI
        private async void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            // Make sure there's a task completion source for the login operation
            if (_loginTaskCompletionSource == null || _loginTaskCompletionSource.Task == null || _loginTaskCompletionSource.Task.AsyncState == null)
            {
                return;
            }

            // Get the login info from the task completion source
            var loginEntry = _loginTaskCompletionSource.Task.AsyncState as LoginInfo;

            try
            {
                // Create a token credential using the provided username and password
                TokenCredential userCredentials = await AuthenticationManager.Current.GenerateCredentialAsync
                                            (new Uri(loginEntry.ServiceUrl),
                                             loginEntry.UserName,
                                             loginEntry.Password,
                                             loginEntry.RequestInfo.GenerateTokenOptions);

                // Set the result on the task completion source
                _loginTaskCompletionSource.TrySetResult(userCredentials);
            }
            catch (Exception ex)
            {
                // Show exceptions on the login UI
                loginEntry.ErrorMessage = ex.Message;

                // Increment the login attempt count
                loginEntry.AttemptCount++;

                // Set an exception on the login task completion source after three login attempts
                if (loginEntry.AttemptCount >= 3)
                {
                    // This causes the login attempt to fail
                    _loginTaskCompletionSource.TrySetException(new Exception("Exceeded the number of allowed login attempts"));
                }
            }
        }
    }

    // Helper class to contain login information
    internal class LoginInfo : INotifyPropertyChanged
    {
        // Information about the current request for credentials 
        private CredentialRequestInfo _requestInfo;
        public CredentialRequestInfo RequestInfo
        {
            get { return _requestInfo; }
            set
            {
                _requestInfo = value;
                OnPropertyChanged();
            }
        }

        // URI for the service that is requesting credentials
        private string _serviceUrl;
        public string ServiceUrl
        {
            get { return _serviceUrl; }
            set
            {
                _serviceUrl = value;
                OnPropertyChanged();
            }
        }

        // Username entered by the user
        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        // Password entered by the user
        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        // Last error message encountered while creating credentials
        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        // Number of login attempts
        private int _attemptCount;
        public int AttemptCount
        {
            get { return _attemptCount; }
            set
            {
                _attemptCount = value;
                OnPropertyChanged();
            }
        }

        // Store the credential request information when the class is constructed
        public LoginInfo(CredentialRequestInfo info)
        {
            RequestInfo = info;
            ServiceUrl = info.ServiceUri.AbsoluteUri;
            ErrorMessage = string.Empty;
            AttemptCount = 0;
        }

        // Raise a property changed event so bound controls can update
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
