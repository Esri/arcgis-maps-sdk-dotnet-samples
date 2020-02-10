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
using System.Linq;
using System.Threading;
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
            // Verify that the download
            if (((ImageButton)sender).CommandParameter is SampleInfo sampleInfo)
            {
                try
                {
                    //SetStatusMessage("Downloading sample data", true);
                    await DataManager.EnsureSampleDataPresent(sampleInfo);
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine(exception);
                    //await new MessageDialog("Couldn't download data for that sample", "Error").ShowAsync();
                }
                finally
                {
                    //SetStatusMessage("Ready", false);
                }
            }
        }

        private void AGOLClicked(object sender, EventArgs e)
        {
            if (((ImageButton)sender).CommandParameter is SampleInfo sampleInfo)
            {
                // Open data for that sample in AGOL.
            }
        }

        private void DeleteClicked(object sender, EventArgs e)
        {
            if (((ImageButton)sender).CommandParameter is SampleInfo sampleInfo)
            {
                // Delete data for that sample.
            }
        }
        private void DownloadAllClicked(object sender, EventArgs e)
        {
        }
        private void DeleteAllClicked(object sender, EventArgs e)
        {
        }
    }
}