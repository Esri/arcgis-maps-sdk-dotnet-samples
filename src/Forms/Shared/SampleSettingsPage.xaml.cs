// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ArcGISRuntime
{
    public partial class SampleSettingsPage : TabbedPage
    {
        private static string _runtimeVersion = "";
        private CancellationTokenSource _cancellationTokenSource;
        private List<SampleInfo> OfflineDataSamples;
        private readonly MarkedNet.Marked _markdownRenderer = new MarkedNet.Marked();

        public SampleSettingsPage()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            //LicensePage.Source = "https://www.google.com";

            // Set up offline data.
            OfflineDataSamples = SampleManager.Current.AllSamples.Where(m => m.OfflineDataItems?.Any() ?? false).ToList();
            OfflineDataView.ItemsSource = OfflineDataSamples;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private async void DownloadClicked(object sender, EventArgs e)
        {
            if (((ImageButton)sender).CommandParameter is SampleInfo sampleInfo)
            {
                try
                {
                    SetStatusMessage($"Downloading data for {sampleInfo.SampleName}", true);
                    await DataManager.EnsureSampleDataPresent(sampleInfo);
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine(exception);
                    await Application.Current.MainPage.DisplayAlert("Error", "Couldn't download data for that sample", "OK");
                }
                finally
                {
                    SetStatusMessage("Ready", false);
                }
            }
        }

        private async void AGOLClicked(object sender, EventArgs e)
        {
            if (((ImageButton)sender).CommandParameter is SampleInfo sampleInfo)
            {
                foreach (var offlineItem in sampleInfo.OfflineDataItems)
                {
                    try
                    {
                        string onlinePath = $"https://www.arcgis.com/home/item.html?id={offlineItem}";
                        await Launcher.OpenAsync(new Uri(onlinePath));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
        }

        private async void DeleteClicked(object sender, EventArgs e)
        {
            if (((ImageButton)sender).CommandParameter is SampleInfo sampleInfo)
            {
                try
                {
                    SetStatusMessage("Deleting sample data", true);

                    foreach (string offlineItemId in sampleInfo.OfflineDataItems)
                    {
                        string offlineDataPath = DataManager.GetDataFolder(offlineItemId);
                        Directory.Delete(offlineDataPath, true);
                    }
                    await Application.Current.MainPage.DisplayAlert("Success", $"Offline data deleted for {sampleInfo.SampleName}", "OK");
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                    await Application.Current.MainPage.DisplayAlert("Error", $"Couldn't delete offline data.", "OK");
                }
                finally
                {
                    SetStatusMessage("Ready", false);
                }
            }
        }
        private async void DownloadAllClicked(object sender, EventArgs e)
        {
            try
            {
                // Get a token from a new CancellationTokenSource()
                CancellationToken token = _cancellationTokenSource.Token;

                // Enable the cancel button.
                CancelButton.IsVisible = true;

                // Adjust the UI
                SetStatusMessage("Downloading all...", true);

                // Make a list of tasks for downloading all of the samples.
                HashSet<string> itemIds = new HashSet<string>();

                foreach (SampleInfo sample in OfflineDataSamples)
                {
                    foreach (string itemId in sample.OfflineDataItems)
                    {
                        itemIds.Add(itemId);
                    }
                }

                // Download every item.
                foreach (var item in itemIds)
                {
                    StatusLabel.Text = $"Downloading item: {item}";
                    await DataManager.DownloadDataItem(item, token);
                }

                await Application.Current.MainPage.DisplayAlert(string.Empty, "All data downloaded", "OK");
            }
            catch (OperationCanceledException)
            {
                await Application.Current.MainPage.DisplayAlert(string.Empty, "Download canceled", "OK");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                await Application.Current.MainPage.DisplayAlert("Error", exception.Message, "OK");
            }
            finally
            {
                _cancellationTokenSource = new CancellationTokenSource();
                SetStatusMessage("Ready", false);
                CancelButton.IsVisible = false; ;
            }
        }
        private async void DeleteAllClicked(object sender, EventArgs e)
        {
            try
            {
                SetStatusMessage("Deleting all...", true);

                string offlineDataPath = DataManager.GetDataFolder();
                Directory.Delete(offlineDataPath, true);

                await Application.Current.MainPage.DisplayAlert("Success", "All data deleted", "OK");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                await Application.Current.MainPage.DisplayAlert("Error", exception.Message, "OK");
            }
            finally
            {
                SetStatusMessage("Ready", false);
            }
        }

        private void CancelClicked(object sender, EventArgs e)
        {
            _cancellationTokenSource.Cancel(true);
        }

        private void SetStatusMessage(string message, bool isRunning)
        {
            StatusLabel.Text = message;
            StatusSpinner.IsVisible = isRunning;
            OfflineDataView.IsEnabled = !isRunning;
        }
    }
}