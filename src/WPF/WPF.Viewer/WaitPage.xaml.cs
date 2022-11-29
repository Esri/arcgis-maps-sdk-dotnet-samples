// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Threading;
using System.Windows;

namespace ArcGIS.WPF.Viewer
{
    public partial class WaitPage
    {
        private CancellationTokenSource _cancellationTokenSource;

        public WaitPage()
        {
            InitializeComponent();
        }

        public WaitPage(CancellationTokenSource cancellation)
        {
            InitializeComponent();
            _cancellationTokenSource = cancellation;
            this.Visibility = Visibility.Collapsed;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource.Cancel(true);
        }

        public void SetProgress(int percentage, bool hasPercentage, long totalBytes)
        {
            Dispatcher.BeginInvoke((Action)delegate ()
            {
                this.Visibility = Visibility.Visible;
                ProgressBar.IsIndeterminate = !hasPercentage;
                if (percentage > 0 && hasPercentage)
                {
                    ProgressBar.Value = percentage;
                    PercentageLabel.Content = $"{percentage}%";
                }
                else
                {
                    PercentageLabel.Content = $"{totalBytes / 1024}kb";
                }
            });
        }
    }
}