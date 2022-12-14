// Copyright 2019 Esri.
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
using System.Windows;

namespace ArcGIS.WPF.Samples.CertificateAuthenticationWithPKI
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Certificate authentication with PKI",
        category: "Security",
        description: "Access secured portals using a certificate.",
        instructions: "> **NOTE**: You must provide your own ArcGIS Portal with PKI authentication configured.",
        tags: new[] { "PKI", "X509", "authentication", "certificate", "login", "passwordless", "smartcard", "store" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class CertificateAuthenticationWithPKI
    {
        private string _serverUrl = "";

        public CertificateAuthenticationWithPKI()
        {
            InitializeComponent();
        }

        private async Task<Credential> CreateCertCredential(CredentialRequestInfo info)
        {
            // Handle challenges for a secured resource by prompting for a client certificate.
            Esri.ArcGISRuntime.Security.Credential credential = null;

            try
            {
                // Create an X509 store for reading certificates for the current user.
                var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

                // Open the store in read-only mode.
                store.Open(OpenFlags.ReadOnly);

                // Get a list of certificates that are currently valid.
                X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, true);

                // Prompt the user to select a certificate using the built-in certificate selection UI.
                var selection = X509Certificate2UI.SelectFromCollection(certificates, "Select Certificate",
                    "Select the certificate to use for authentication.", X509SelectionFlag.SingleSelection);

                // Make sure the user chose a certificate.
                if (selection.Count > 0)
                {
                    // Create a new CertificateCredential using the chosen certificate.
                    credential = new CertificateCredential(new Uri(_serverUrl), selection[0]);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            // Return the CertificateCredential for the secured portal.
            return await Task.FromResult(credential);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Store the server url for later reference.
                _serverUrl = PortalUrlTextbox.Text;

                // Configure the challenge handler.
                AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCertCredential);

                // Create the portal.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync(new Uri(_serverUrl));

                // Update the UI with the logged in user.
                LoggedInUserName.Text = portal.User.FullName;
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("404"))
                {
                    MessageBox.Show("404: Not Found");
                }
                else if (ex.Message.Contains("403"))
                {
                    MessageBox.Show("403: Not authorized; did you use the right certificate?");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    MessageBox.Show("Couldn't authenticate. See debug output for details.");
                }
            }
            catch (UriFormatException)
            {
                MessageBox.Show("Couldn't authenticate. Enter a valid URL first.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show("Couldn't authenticate. See debug output for details.");
            }
        }
    }
}