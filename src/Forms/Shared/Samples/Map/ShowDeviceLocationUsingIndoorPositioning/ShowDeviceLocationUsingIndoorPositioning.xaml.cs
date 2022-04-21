// Copyright 2022 Esri.
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
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.ShowDeviceLocationUsingIndoorPositioning
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Show device location using indoor positioning",
        "Map",
        "Show your device's real-time location while inside a building by using signals from indoor positioning beacons.",
        "")]
    public partial class ShowDeviceLocationUsingIndoorPositioning : ContentPage
    {
        // A TaskCompletionSource to store the result of a login task.
        private TaskCompletionSource<Credential> _loginTaskCompletionSrc;

        public ShowDeviceLocationUsingIndoorPositioning()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            Uri uri = new Uri("https://viennardc.maps.arcgis.com");
            Credential token = null;

            //try
            //{
            //    // Set up the AuthenticationManager to use OAuth for secure ArcGIS Online requests.
            //    ArcGISLoginPrompt.SetChallengeHandler();

            //    // Connect to the portal (ArcGIS Online, for example).
            //    ArcGISPortal arcgisPortal = await ArcGISPortal.CreateAsync(uri, true);

            //    // Get a web map portal item using its ID.
            //    // If the item contains layers not shared publicly, the user will be challenged for credentials at this point.
            //    PortalItem portalItem = await PortalItem.CreateAsync(arcgisPortal, "89f88764c29b48218366855d7717d266");

            //    // Create a new map with the portal item and display it in the map view.
            //    // If authentication failed, only the public layers will be displayed.
            //    Map myMap = new Map(portalItem);
            //    MyMapView.Map = myMap;
            //}
            //catch (Exception e)
            //{
            //    await Application.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
            //}

            // Handle the login to the feature service.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(async (info) =>
            {
                try
                {
                    // WARNING: Never hardcode login information in a production application. This is done solely for the sake of the sample.
                    string sampleUser = "tester_viennardc";
                    string samplePass = "password.testing12345";
                    Debug.WriteLine(info.ServiceUri.ToString());
                    token = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri, sampleUser, samplePass, info.GenerateTokenOptions);
                    return token;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;
                }
            });

            // Define a challenge handler method for the AuthenticationManager.
            // This method handles getting credentials when a secured resource is encountered.
            //AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            try
            {
                ArcGISPortal portal = await ArcGISPortal.CreateAsync(uri, true);

                //PortalItem item = await PortalItem.CreateAsync(portal, "3567f024955548b6b727a3ae41089515");

                FeatureLayer layer = new FeatureLayer(new Uri("https://services5.arcgis.com/ZfLcwYu60AXGB1Zb/arcgis/rest/services/Redlands_iPhone_10_WFL1/FeatureServer"));

                //ServiceGeodatabase geodatabase = await ServiceGeodatabase.CreateAsync(new Uri("https://viennardc.maps.arcgis.com/home/item.html?id=89f88764c29b48218366855d7717d266"),);
                //Basemap basemap = new Basemap(new Uri("https://viennardc.maps.arcgis.com/home/item.html?id=89f88764c29b48218366855d7717d266"));
                //await basemap.LoadAsync();

                if (token != null)
                {
                    Debug.WriteLine(token.ServiceUri.UserInfo);
                }

                //Map myMap = new Map(new Uri("https://viennardc.maps.arcgis.com/home/item.html?id=89f88764c29b48218366855d7717d266"));
                MyMapView.Map = new Map();
                MyMapView.Map.OperationalLayers.Add(layer);
                await layer.LoadAsync();
                //await myMap.LoadAsync();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            //try
            //{
            //    // Get the associated CredentialRequestInfo (will need the URI of the service being accessed).
            //    CredentialRequestInfo requestInfo = (CredentialRequestInfo)_loginTaskCompletionSrc.Task.AsyncState;

            //    // Create a token credential using the provided username and password.
            //    TokenCredential userCredentials = await AuthenticationManager.Current.GenerateCredentialAsync
            //                                (requestInfo.ServiceUri,
            //                                 "tester_viennardc",
            //                                 "password.testing12345",
            //                                 requestInfo.GenerateTokenOptions);

            //    // Set the task completion source result with the ArcGIS network credential.
            //    // AuthenticationManager is waiting for this result and will add it to its Credentials collection.
            //    _loginTaskCompletionSrc.TrySetResult(userCredentials);
            //}
            //catch (Exception ex)
            //{
            //    // Unable to create credential, set the exception on the task completion source.
            //    _loginTaskCompletionSrc.TrySetException(ex);
            //}
        }

        // AuthenticationManager.ChallengeHandler function that prompts the user for login information to create a credential.
        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // Create a new TaskCompletionSource for the login operation.
            // Passing the CredentialRequestInfo object to the constructor will make it available from its AsyncState property.
            _loginTaskCompletionSrc = new TaskCompletionSource<Credential>(info);

            // Return the login task, the result will be ready when completed (user provides login info and clicks the "Login" button)
            return await _loginTaskCompletionSrc.Task;
        }
    }
}
