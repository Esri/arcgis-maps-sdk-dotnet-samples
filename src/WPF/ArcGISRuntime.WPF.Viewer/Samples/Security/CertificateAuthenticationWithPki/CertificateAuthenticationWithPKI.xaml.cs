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
using System.Security.Cryptography.X509Certificates;

namespace ArcGISRuntime.WPF.Samples.CertificateAuthenticationWithPKI
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Certificate authentication with PKI",
        "Security",
        "Access secured portals using a certificate.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class CertificateAuthenticationWithPKI
    {
        public CertificateAuthenticationWithPKI()
        {
            InitializeComponent();
        }

        public Credential CreateCertCredential(CredentialRequestInfo info)
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

                // Prompt the user to select a certificate using the built-in certificate selection UI.
                var selection = X509Certificate2UI.SelectFromCollection(certificates, "Select Certificate",
                    "Select the certificate to use for authentication.", X509SelectionFlag.SingleSelection);

                // Make sure the user chose a certificate.
                if (selection.Count > 0)
                {
                    // Create a new CertificateCredential using the chosen certificate.
                    credential = new Esri.ArcGISRuntime.Security.CertificateCredential(selection[0])
                    {
                        ServiceUri = new Uri(PortalUrlTextbox.Text)
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

        private async void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                // Explicitly create the credential.
                Credential cred = CreateCertCredential(null);

                // Create the portal with the credential.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync(new Uri(PortalUrlTextbox.Text), null, cred, new System.Threading.CancellationToken());

                // Update the UI with the logged in user's name.
                LoggedInUserName.Text = portal.User.FullName;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}