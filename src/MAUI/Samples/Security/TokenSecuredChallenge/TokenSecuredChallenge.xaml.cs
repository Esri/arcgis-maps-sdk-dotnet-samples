// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using System.Globalization;
using Color = System.Drawing.Color;

namespace ArcGISRuntime.Samples.TokenSecuredChallenge
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "ArcGIS token challenge",
        category: "Security",
        description: "This sample demonstrates how to prompt the user for a username and password to authenticate with ArcGIS Server to access an ArcGIS token-secured service. Accessing secured services requires a login that's been defined on the server.",
        instructions: "When you run the sample, the app will load a map that contains a layer from a secured service. Then, you will be challenged for a user name and password to view that layer. Enter the correct user name (user1) and password (user1). If you authenticate successfully, the secured layer will display, otherwise the map will contain only the public layers.",
        tags: new[] { "authentication", "cloud", "portal", "remember", "security" })]
    [ArcGISRuntime.Samples.Shared.Attributes.ClassFile("LoginPage.xaml.cs")]
    [ArcGISRuntime.Samples.Shared.Attributes.XamlFiles("LoginPage.xaml")]
    public partial class TokenSecuredChallenge : ContentPage
    {
        // Public and secured map service URLs.
        private string _publicMapServiceUrl = "https://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer";
        private string _secureMapServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA_secure_user1/MapServer";

        // Public and secured layer names.
        private string _publicLayerName = "World Street Map - Public";
        private string _secureLayerName = "USA - Secure";

        // A TaskCompletionSource to store the result of a login task.
        private TaskCompletionSource<Credential> _loginTaskCompletionSrc;

        // Page for the user to enter login information.
        private LoginPage _loginPage;

        public TokenSecuredChallenge()
        {
            InitializeComponent();

            // Call a function to display a simple map and set up the AuthenticationManager.
            Initialize();
        }

        private void Initialize()
        {
            // Define a challenge handler method for the AuthenticationManager.
            // This method handles getting credentials when a secured resource is encountered.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Create the public layer and provide a name.
            ArcGISTiledLayer publicLayer = new ArcGISTiledLayer(new Uri(_publicMapServiceUrl))
            {
                Name = _publicLayerName
            };

            // Create the secured layer and provide a name.
            ArcGISMapImageLayer tokenSecuredLayer = new ArcGISMapImageLayer(new Uri(_secureMapServiceUrl))
            {
                Name = _secureLayerName
            };

            // Bind the layer to UI controls to show the load status.
            PublicLayerStatusPanel.BindingContext = publicLayer;
            SecureLayerStatusPanel.BindingContext = tokenSecuredLayer;

            // Create a new map and add the layers.
            Map myMap = new Map();
            myMap.OperationalLayers.Add(publicLayer);
            myMap.OperationalLayers.Add(tokenSecuredLayer);

            // Add the map to the map view.
            MyMapView.Map = myMap;

            // Create the login UI (will display when the user accesses a secured resource).
            _loginPage = new LoginPage();

            // Set up event handlers for when the user completes the login entry or cancels.
            _loginPage.OnLoginInfoEntered += LoginInfoEntered;
            _loginPage.OnCanceled += LoginCanceled;
        }

        // AuthenticationManager.ChallengeHandler function that prompts the user for login information to create a credential.
        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // Return if authentication is already in process.
            if (_loginTaskCompletionSrc != null && !_loginTaskCompletionSrc.Task.IsCanceled) { return null; }

            // Create a new TaskCompletionSource for the login operation.
            // Passing the CredentialRequestInfo object to the constructor will make it available from its AsyncState property.
            _loginTaskCompletionSrc = new TaskCompletionSource<Credential>(info);

            // Provide a title for the login form (show which service needs credentials).
#if WINDOWS_UWP
            // UWP doesn't have ServiceUri.GetLeftPart (could use ServiceUri.AbsoluteUri for all, but why not use a compilation condition?)
            _loginPage.TitleText = "Login for " + info.ServiceUri.AbsoluteUri;
#else
            _loginPage.TitleText = "Login for " + info.ServiceUri.GetLeftPart(UriPartial.Path);
#endif
            // Show the login controls on the UI thread.
            // OnLoginInfoEntered event will return the values entered (username and password).
            Device.BeginInvokeOnMainThread(async () => await Navigation.PushAsync(_loginPage));

            // Return the login task, the result will be ready when completed (user provides login info and clicks the "Login" button)
            return await _loginTaskCompletionSrc.Task;
        }

        // Handle the OnLoginEntered event from the login UI.
        // LoginEventArgs contains the username and password that were entered.
        private async void LoginInfoEntered(object sender, LoginEventArgs e)
        {
            // Make sure the task completion source has all the information needed.
            if (_loginTaskCompletionSrc == null ||
                _loginTaskCompletionSrc.Task == null ||
                _loginTaskCompletionSrc.Task.AsyncState == null)
            {
                return;
            }

            try
            {
                // Get the associated CredentialRequestInfo (will need the URI of the service being accessed).
                CredentialRequestInfo requestInfo = (CredentialRequestInfo)_loginTaskCompletionSrc.Task.AsyncState;

                // Create a token credential using the provided username and password.
                TokenCredential userCredentials = await AuthenticationManager.Current.GenerateCredentialAsync
                                            (requestInfo.ServiceUri,
                                             e.Username,
                                             e.Password,
                                             requestInfo.GenerateTokenOptions);

                // Set the task completion source result with the ArcGIS network credential.
                // AuthenticationManager is waiting for this result and will add it to its Credentials collection.
                _loginTaskCompletionSrc.TrySetResult(userCredentials);
            }
            catch (Exception ex)
            {
                // Unable to create credential, set the exception on the task completion source.
                _loginTaskCompletionSrc.TrySetException(ex);
            }
            finally
            {
                // Dismiss the login controls.
                await Navigation.PopAsync();
            }
        }

        private void LoginCanceled(object sender, EventArgs e)
        {
            // Dismiss the login controls.
            Navigation.PopAsync();

            // Cancel the task completion source task.
            _loginTaskCompletionSrc.TrySetCanceled();
        }
    }

    // Value converter class to return a color for the current load status
    // Note: to make this class accessible as a static resource in the shared form (TokenChallengePage.xaml)
    //       the assembly name for each platform had to be changed to the same value ("TokenChallengeArcGISRuntimeMaui")
    //       in order to provide a consistent XML namespace value. Another option would be to place such code in
    //       a PCL project rather than a shared project (the shared project would still be needed for the ArcGIS
    //       Runtime code).
    public class LoadStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Default to gray as the text color.
            Color statusColor = Color.Gray;

            // Check the provided load status value.
            switch ((int)value)
            {
                // Green for loaded, red for not loaded (or failure to load), gray if still loading.
                case (int)Esri.ArcGISRuntime.LoadStatus.Loaded:
                    statusColor = Color.Green;
                    break;

                case (int)Esri.ArcGISRuntime.LoadStatus.Loading:
                    statusColor = Color.Gray;
                    break;

                case (int)Esri.ArcGISRuntime.LoadStatus.FailedToLoad:
                    statusColor = Color.Red;
                    break;

                case (int)Esri.ArcGISRuntime.LoadStatus.NotLoaded:
                    statusColor = Color.Red;
                    break;
            }

            return statusColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // No need to convert the other way.
            throw new NotImplementedException();
        }
    }
}