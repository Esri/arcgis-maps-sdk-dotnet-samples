// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System.Threading;

namespace ArcGISRuntimeMaui
{
    public partial class WaitPage
    {
        private CancellationTokenSource _cancellationTokenSource;

        public WaitPage(CancellationTokenSource cancellation)
        {
            InitializeComponent();
            _cancellationTokenSource = cancellation;
        }

        private void DownloadCancelled(object sender, System.EventArgs e)
        {
            _cancellationTokenSource.Cancel(true);
        }

        public void SetProgress(int percentage, bool hasPercentage, long totalBytes)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DownloadProgress.IsVisible = hasPercentage;
                IndefiniteSpinner.IsVisible = !hasPercentage;
                if (percentage > 0 && hasPercentage)
                {
                    double progress = percentage / 100.0;
                    DownloadProgress.ProgressTo(progress, 10, Easing.Linear);
                    DownloadLabel.Text = $"Downloading data: {percentage}%";
                }
                else
                {
                    DownloadLabel.Text = $"Downloading data: {totalBytes / 1024}kb";
                }
            });
        }
    }
}