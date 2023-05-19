// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Managers;
using ArcGIS.Samples.Shared.Models;
using Esri.ArcGISRuntime.Security;

namespace ArcGIS.Helpers
{
    internal static class SampleLoader
    {
        private static List<string> _namedUserSamples = new List<string> {
            "AuthorMap",
            "SearchPortalMaps",
            "OAuth",};

        // Used to load a sample from the search or list in a category.
        public static async Task LoadSample(SampleInfo sampleInfo)
        {
            try
            {
                // Set the currently selected sample.
                SampleManager.Current.SelectedSample = sampleInfo;

                // Remove API key if opening named user sample.
                if (_namedUserSamples.Contains(sampleInfo.FormalName))
                {
                    ApiKeyManager.DisableKey();
                }
                else
                {
                    // Ensure the API key is enabled if the sample that is being opened is not a named user sample.
                    ApiKeyManager.EnableKey();
                }

                // Clear any existing credentials from AuthenticationManager.
                foreach (Credential cred in AuthenticationManager.Current.Credentials)
                {
                    AuthenticationManager.Current.RemoveCredential(cred);
                }

                // Clear the challenge handler.
                AuthenticationManager.Current.ChallengeHandler = null;

                // Load offline data before showing the sample.
                if (sampleInfo.OfflineDataItems != null)
                {
                    CancellationTokenSource cancellationSource = new CancellationTokenSource();

                    // Show the wait page.
                    var waitPage = new WaitPage(cancellationSource) { Title = sampleInfo.SampleName };
                    await Shell.Current.Navigation.PushModalAsync(waitPage, false);

                    // Wait for the sample data download.
                    await DataManager.EnsureSampleDataPresent(sampleInfo, cancellationSource.Token, (info) =>
                    {
                        waitPage.SetProgress(info.Percentage, info.HasPercentage, info.TotalBytes);
                    });

                    // Remove the waiting page.
                    await Shell.Current.Navigation.PopModalAsync(false);
                }

                // Get the sample control from the selected sample.
                ContentPage sampleControl = (ContentPage)SampleManager.Current.SampleToControl(sampleInfo);

                // Create the sample display page to show the sample and the metadata.
                SamplePage page = new SamplePage(sampleControl, sampleInfo);

                // Show the sample.
                await Shell.Current.Navigation.PushAsync(page, true);
            }
            catch (OperationCanceledException)
            {
                // Remove the waiting page.
                await Shell.Current.Navigation.PopModalAsync(false);

                await Application.Current.MainPage.DisplayAlert("", "Download cancelled", "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading sample: {ex.Message}");
            }
        }
    }
}