// Copyright 2019 Esri.
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
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Reflection;
using ArcGISRuntime.WinUI;

namespace ArcGISRuntime
{
    public sealed partial class SettingsPage : UserControl
    {
        private static string _runtimeVersion = "";
        private CancellationTokenSource _cancellationTokenSource;
        private List<SampleInfo> OfflineDataSamples;

        public SettingsPage()
        {
            this.InitializeComponent();
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Set up version info and About section.
            try
            {
                if (string.IsNullOrWhiteSpace(_runtimeVersion))
                {
                    var version = typeof(Esri.ArcGISRuntime.ArcGISRuntimeEnvironment).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
                    _runtimeVersion = version.Version.ToLower();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                _runtimeVersion = "Couldn't find ArcGIS Runtime version.";
            }

            // Set up markdown tabs.
            var root = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            AboutBlock.Text = File.ReadAllText(Path.Combine(root, "Resources", "about.md")) + _runtimeVersion;
            LicensesBlock.Text = File.ReadAllText(Path.Combine(root, "Resources", "licenses.md"));
            
            // Set up offline data.
            OfflineDataSamples = SampleManager.Current.AllSamples.Where(m => m.OfflineDataItems?.Any() ?? false).ToList();
            SampleDataListView.ItemsSource = OfflineDataSamples;
            _cancellationTokenSource = new CancellationTokenSource();
        }
        private async void Download_All_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get a token from a new CancellationTokenSource()
                CancellationToken token = _cancellationTokenSource.Token;

                // Enable the cancel button.
                CancelButton.Visibility = Visibility.Visible;

                // Adjust the UI
                SetStatusMessage("Downloading all...", true);

                // Make a list of tasks for downloading all of the samples.
                HashSet<string> itemIds = new HashSet<string>();
                List<Task> downloadTasks = new List<Task>();

                foreach (SampleInfo sample in OfflineDataSamples)
                {
                    foreach (string itemId in sample.OfflineDataItems)
                    {
                        itemIds.Add(itemId);
                    }
                }

                StatusSpinner.IsIndeterminate = false;
                StatusSpinner.Minimum = 0;
                StatusSpinner.MaxHeight = 100;
                await DataManager.EnsureSampleDataPresent(itemIds, token, (info) =>
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        StatusSpinner.Value = info.Percentage;
                    });
                });

                await new MessageDialog2("All data downloaded").ShowAsync();
            }
            catch (OperationCanceledException)
            {
                await new MessageDialog2("Download canceled").ShowAsync();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                await new MessageDialog2("Download canceled", "Error").ShowAsync();
            }
            finally
            {
                StatusSpinner.IsIndeterminate = true;
                _cancellationTokenSource = new CancellationTokenSource();
                SetStatusMessage("Ready", false);
                CancelButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void Delete_All_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetStatusMessage("Deleting all...", true);

                string offlineDataPath = DataManager.GetDataFolder();

                Directory.Delete(offlineDataPath, true);

                await new MessageDialog2("All data deleted").ShowAsync();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                if (exception.Message.Contains("used by another process"))
                {
                    await new MessageDialog2("Couldn't delete offline data. Data is being used by a sample. Restart the sample viewer and try again.", "Error").ShowAsync();
                }
                else
                {
                    await new MessageDialog2("Couldn't delete the offline data folder", "Error").ShowAsync();
                }
            }
            finally
            {
                SetStatusMessage("Ready", false);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource.Cancel(true);
        }

        private async void Open_AGOL_Click(object sender, RoutedEventArgs e)
        {
            SampleInfo sample = (SampleInfo)((Button)sender).Tag;

            foreach (var offlineItem in sample.OfflineDataItems)
            {
                string onlinePath = $"https://www.arcgis.com/home/item.html?id={offlineItem}";

                await Launcher.LaunchUriAsync(new Uri(onlinePath));
            }
        }

        private Dictionary<SampleInfo, CancellationTokenSource> Downloads = new Dictionary<SampleInfo, CancellationTokenSource>();

        private async void Download_Now_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar progress = null;
            SampleInfo sample = ((Button)sender).Tag as SampleInfo;
            try
            {
                if(Downloads.ContainsKey(sample))
                {
                    Downloads[sample].Cancel();
                    return;
                }
                CancellationTokenSource tcs = new CancellationTokenSource();
                Downloads[sample] = tcs;
                var elm = (sender as Button)?.Content;
                Action<ProgressInfo> onProgress = null;
                if (elm is Grid grid && grid.Children.Count == 2 && grid.Children[1] is ProgressBar bar)
                {
                    progress = bar;
                    progress.Value = 0;
                    progress.Visibility = Visibility.Visible;
                    onProgress = (info) =>
                    {
                        DispatcherQueue.TryEnqueue(() => { progress.Value = info.Percentage; });
                    };
                }
                await DataManager.EnsureSampleDataPresent(sample, tcs.Token, onProgress);
            }
            catch (OperationCanceledException) { }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
                await new MessageDialog2("Couldn't download data for that sample", "Error").ShowAsync();
            }
            finally
            {
                Downloads.Remove(sample);
                if (progress != null)
                    progress.Visibility = Visibility.Collapsed;
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetStatusMessage("Deleting sample data", true);

                SampleInfo sample = (SampleInfo)((Button)sender).Tag;

                foreach (string offlineItemId in sample.OfflineDataItems)
                {
                    string offlineDataPath = DataManager.GetDataFolder(offlineItemId);

                    Directory.Delete(offlineDataPath, true);
                }
                await new MessageDialog2($"Offline data deleted for {sample.SampleName}").ShowAsync();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                if (exception.Message.Contains("used by another process"))
                {
                    await new MessageDialog2("Couldn't delete offline data. Data is being used by a sample. Restart the sample viewer and try again.", "Error").ShowAsync();
                }
                else
                {
                    await new MessageDialog2("Couldn't delete the offline data folder", "Error").ShowAsync();
                }
            }
            finally
            {
                SetStatusMessage("Ready", false);
            }
        }

        private void SetStatusMessage(string message, bool isRunning)
        {
            StatusLabel.Text = message;
            SampleDataListView.IsEnabled = !isRunning;
            StatusSpinner.Visibility = isRunning ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void MarkdownText_LinkClicked(object sender, CommunityToolkit.WinUI.UI.Controls.LinkClickedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(e.Link));
        }
    }
}