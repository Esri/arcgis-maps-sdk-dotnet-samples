// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Shared.Models;
using ArcGISRuntime.WPF.Viewer;
using Esri.ArcGISRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime
{
    public partial class SettingsWindow : Window
    {
        private static string _runtimeVersion = "";
        private readonly MarkedNet.Marked _markdownRenderer = new MarkedNet.Marked();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private List<SampleInfo> OfflineDataSamples;

        public SettingsWindow()
        {
            InitializeComponent();

            // Set up version info.
            if (string.IsNullOrWhiteSpace(_runtimeVersion))
            {
                var runtimeTypeInfo = typeof(ArcGISRuntimeEnvironment).GetTypeInfo();
                var rtVersion = FileVersionInfo.GetVersionInfo(runtimeTypeInfo.Assembly.Location);
                _runtimeVersion = rtVersion.FileVersion;
            }
            VersionTextField.Text = _runtimeVersion;

            // Set up license info.
            string markdownPath = Path.Combine(App.ResourcePath, "Resources", "licenses.md");
            string cssPath = Path.Combine(App.ResourcePath, "Resources", "github-markdown.css");
            string licenseContent = File.ReadAllText(markdownPath);
            licenseContent = _markdownRenderer.Parse(licenseContent);
            string htmlString = "<!doctype html><head><link rel=\"stylesheet\" href=\"" + cssPath + "\" /></head><body class=\"markdown-body\">" + licenseContent + "</body>";

            // Set the html in web browser.
            LicenseBrowser.DocumentText = htmlString;

            // Add an event handler for hyperlink clicks.
            LicenseBrowser.Document.Click += HyperlinkClick;

            // Create an event handler for cancelling the default hyperlink behavior.
            LicenseBrowser.NewWindow += (s, e) => { e.Cancel = true; };

            // Set up offline data.
            OfflineDataSamples = SampleManager.Current.AllSamples.Where(m => m.OfflineDataItems?.Any() ?? false).ToList();

            SampleDataListView.ItemsSource = OfflineDataSamples;
        }

        private void HyperlinkClick(object sender, System.Windows.Forms.HtmlElementEventArgs e)
        {
            // Get the html element that the user clicked on.
            System.Windows.Forms.HtmlElement src = LicenseBrowser.Document?.GetElementFromPoint(e.ClientMousePosition);

            // Check if the element is a hyperlink.
            if (src?.OuterHtml.Contains("http") == true)
            {
                // Parse the url from the hyperlink html.
                string url = src.OuterHtml.Split('\"')[1];

                // Open the url.
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
        }

        private void OpenInAgol_Clicked(object sender, RoutedEventArgs e)
        {
            SampleInfo sample = (SampleInfo)((Button)sender).Tag;

            foreach (var offlineItem in sample.OfflineDataItems)
            {
                string onlinePath = $"https://www.arcgis.com/home/item.html?id={offlineItem}";

                Process.Start(new ProcessStartInfo(onlinePath) { UseShellExecute = true });
            }
        }

        private async void DownloadNow_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                SetStatusMessage("Downloading sample data", true);
                SampleInfo sample = (SampleInfo)((Button)sender).Tag;

                await DataManager.EnsureSampleDataPresent(sample);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                MessageBox.Show("Couldn't download data for that sample");
            }
            finally
            {
                SetStatusMessage("Ready", false);
            }
        }

        private async void DownloadAll_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                CancellationToken token = _cancellationTokenSource.Token;
                CancelButton.Visibility = Visibility.Visible;
                SetStatusMessage("Downloading all...", true);
                HashSet<string> itemIds = new HashSet<string>();
                List<Task> downloadTasks = new List<Task>();
                foreach (SampleInfo sample in OfflineDataSamples)
                {
                    foreach (string itemId in sample.OfflineDataItems)
                    {
                        itemIds.Add(itemId);
                    }
                }

                foreach (var item in itemIds)
                {
                    downloadTasks.Add(DataManager.DownloadDataItem(item, token));
                }

                await Task.WhenAll(downloadTasks);

                MessageBox.Show("All data downloaded");
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Download canceled");
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
                MessageBox.Show("Couldn't download all sample data");
            }
            finally
            {
                _cancellationTokenSource = new CancellationTokenSource();
                SetStatusMessage("Ready", false);
                CancelButton.Visibility = Visibility.Collapsed;
            }
        }

        private void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetStatusMessage("Deleting all...", true);

                string offlineDataPath = DataManager.GetDataFolder();

                Directory.Delete(offlineDataPath, true);

                MessageBox.Show("All data deleted");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                MessageBox.Show("Couldn't delete the offline data folder");
            }
            finally
            {
                SetStatusMessage("Ready", false);
            }
        }

        private void RevealInExplorer_Clicked(object sender, RoutedEventArgs e)
        {
            // Button for this was removed because it doesn't work with
            // folder redirection when packaged with desktop bridge.
            Process.Start(new ProcessStartInfo(DataManager.GetDataFolder()) { UseShellExecute = true });
        }

        private void DeleteData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetStatusMessage("Deleting sample data", true);

                SampleInfo sample = (SampleInfo)((Button)sender).Tag;

                foreach (string offlineItemId in sample.OfflineDataItems)
                {
                    string offlineDataPath = DataManager.GetDataFolder(offlineItemId);

                    Directory.Delete(offlineDataPath);
                }

                MessageBox.Show($"Offline data deleted for {sample.SampleName}");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                MessageBox.Show($"Couldn't delete offline data.");
            }
            finally
            {
                SetStatusMessage("Ready", false);
            }
        }

        private void SetStatusMessage(string message, bool isRunning)
        {
            StatusLabel.Text = message;

            if (isRunning)
            {
                StatusSpinner.Visibility = Visibility.Visible;
                SampleDataListView.IsEnabled = false;
            }
            else
            {
                StatusSpinner.Visibility = Visibility.Collapsed;
                SampleDataListView.IsEnabled = true;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource.Cancel(true);
        }
    }
}