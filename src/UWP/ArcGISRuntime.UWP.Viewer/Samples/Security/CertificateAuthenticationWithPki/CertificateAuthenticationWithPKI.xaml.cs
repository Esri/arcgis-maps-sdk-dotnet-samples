﻿// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.UWP.Samples.CertificateAuthenticationWithPKI
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Certificate authentication with PKI",
        "Security",
        "Access secured portals using a certificate.",
        "")]
    public partial class CertificateAuthenticationWithPKI
    {
        private string _serverUrl;

        public CertificateAuthenticationWithPKI()
        {
            InitializeComponent();
        }

        public async Task<Credential> CreateCertCredentialAsync(CredentialRequestInfo info)
        {
            // Handle challenges for a secured resource by prompting for a client certificate.
            Credential credential = null;

            try
            {
                // Create an X509 store for reading certificates for the current user.
                var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

                // Open the store in read-only mode.
                store.Open(OpenFlags.ReadOnly);

                // Get a list of certificates that are currently valid.
                X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, true);

                // Create a dialog for showing the list of certificates.
                ContentDialog dialog = new ContentDialog();
                dialog.CloseButtonText = "Select certificate";

                // Create a list view for rendering the list.
                ListView listview = new ListView();

                // Use a template defined as a resource in XAML.
                listview.ItemTemplate = (DataTemplate) this.Resources["CertificateTemplate"];

                // Display the items in the listview.
                listview.ItemsSource = certificates;

                // Display the listview in the dialog.
                dialog.Content = listview;

                // Display the dialog.
                await dialog.ShowAsync();

                // Make sure the user chose a certificate.
                if (listview.SelectedItems.Count > 0)
                {
                    // Get the chosen certificate.
                    X509Certificate2 cert = (X509Certificate2) listview.SelectedItem;

                    // Create a new CertificateCredential using the chosen certificate.
                    credential = new Esri.ArcGISRuntime.Security.CertificateCredential(cert)
                    {
                        ServiceUri = new Uri(_serverUrl)
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            // Return the CertificateCredential for the secured portal.
            return credential;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Workaround for HTTP client bug affecting System.Net.HttpClient.
                // https://github.com/dotnet/corefx/issues/37598
                var httpClient = new Windows.Web.Http.HttpClient();
                await httpClient.GetStringAsync(new Uri(PortalUrlTextbox.Text));
                // End workaround

                // Store the server URL for later reference.
                _serverUrl = PortalUrlTextbox.Text;

                // Configure the authentication manager.
                AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCertCredentialAsync);

                // Create the portal.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync(new Uri(_serverUrl));

                // Update the UI with the logged in user.
                LoggedInUsername.Text = portal.User.FullName;
            }
            catch (UriFormatException)
            {
                await new MessageDialog("Couldn't authenticate. Enter a valid URL first.").ShowAsync();
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("404"))
                {
                    await new MessageDialog("404: Not Found").ShowAsync();
                }
                else if (ex.Message.Contains("403"))
                {
                    await new MessageDialog("403: Not authorized; did you use the right certificate?").ShowAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    await new MessageDialog("Couldn't authenticate. See debug output for details").ShowAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await new MessageDialog("Error authenticating; see debug output for details.").ShowAsync();
            }
        }
    }
}