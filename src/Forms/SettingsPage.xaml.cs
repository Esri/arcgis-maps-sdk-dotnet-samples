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
using Esri.ArcGISRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ArcGISRuntime
{
    public partial class SettingsPage : TabbedPage
    {
        private CancellationTokenSource _cancellationTokenSource;
        private List<SampleInfo> OfflineDataSamples;

        public SettingsPage()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Get the ArcGIS Runtime version number.
            string versionNumber = string.Empty;
            try
            {
#if XAMARIN_ANDROID
                versionNumber = typeof(ArcGISRuntimeEnvironment).GetTypeInfo().Assembly.GetName().Version.ToString(2);
#else
                var runtimeTypeInfo = typeof(ArcGISRuntimeEnvironment).GetTypeInfo();
                versionNumber = FileVersionInfo.GetVersionInfo(runtimeTypeInfo.Assembly.Location).FileVersion;
#endif
            }
            // Precise version number cant be used while running in release mode.
            catch (Exception)
            {
                versionNumber = "100.15.0";
            }

            // Set up offline data.
            OfflineDataSamples = SampleManager.Current.AllSamples.Where(m => m.OfflineDataItems?.Any() ?? false).OrderBy(s => s.SampleName).ToList();
            OfflineDataView.ItemsSource = OfflineDataSamples;
            _cancellationTokenSource = new CancellationTokenSource();

            // Get the contents of the markdown files for the "About" and "Licenses" pages.
            var assembly = Assembly.GetExecutingAssembly();
            string aboutString = new StreamReader(assembly.GetManifestResourceStream("ArcGISRuntime.Resources.SettingsPage.about.md")).ReadToEnd();
            string licenseString = new StreamReader(assembly.GetManifestResourceStream("ArcGISRuntime.Resources.SettingsPage.licenses.md")).ReadToEnd();

            // The location of the github markdown css is platform dependent.
            string baseUrl = string.Empty;
#if WINDOWS_UWP
            baseUrl = "ms-appx-web:///";
#elif XAMARIN_ANDROID
            baseUrl = "file:///android_asset";
#elif __IOS__
            baseUrl = Foundation.NSBundle.MainBundle.BundlePath;

            // Need to set the viewport on iOS to scale page correctly.
            string viewportHTML = "<meta name=\"viewport\" content=\"width=" +
                Application.Current.MainPage.Width +
                ", shrink-to-fit=YES\">";
#endif
            string cssPath = $"{baseUrl}/github-markdown.css";
            string htmlStart = $"<!doctype html><head><link rel=\"stylesheet\" href=\"{cssPath}\" />";

            // Load the HTML for the about and license pages.
            string licenseHTML = htmlStart +
#if __IOS__
                viewportHTML +
#endif
                $"</head><body class=\"markdown-body\">{Markdig.Markdown.ToHtml(licenseString)}</body>";
            LicensePage.Source = new HtmlWebViewSource() { Html = licenseHTML };

            string aboutHTML = htmlStart +
#if __IOS__
                viewportHTML +
#endif
                $"</head><body class=\"markdown-body\">{Markdig.Markdown.ToHtml(aboutString)}{versionNumber}</body>";
            AboutPage.Source = new HtmlWebViewSource() { Html = aboutHTML };

            // Add an event handler for hyperlinks in the web views.
            AboutPage.Navigating += HyperlinkClicked;
            LicensePage.Navigating += HyperlinkClicked;
        }

        private async void HyperlinkClicked(object sender, WebNavigatingEventArgs e)
        {
            // Check that user clicked a hyperlink from the readmes.
            if (e.Url.Contains("http"))
            {
                // Cancel that navigation.
                e.Cancel = true;
                try
                {
                    // Open the link in an external browser.
                    await Launcher.OpenAsync(e.Url);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        private async void DownloadClicked(object sender, EventArgs e)
        {
            if (((ImageButton)sender).CommandParameter is SampleInfo sampleInfo)
            {
                try
                {
                    // Enable the cancel button.
                    CancelButton.IsVisible = true;

                    SetStatusMessage($"Downloading: {sampleInfo.SampleName}", true);
                    _ = StatusBar.ProgressTo(0, 10, Easing.Linear);
                    await DataManager.EnsureSampleDataPresent(sampleInfo, _cancellationTokenSource.Token, (info) =>
                    {
                        SetProgress(info.Percentage, info.HasPercentage);
                    });
                }
                catch (OperationCanceledException)
                {
                    await Application.Current.MainPage.DisplayAlert(string.Empty, "Download canceled", "OK");
                }
                catch (Exception exception)
                {
                    Debug.WriteLine(exception);
                    await Application.Current.MainPage.DisplayAlert("Error", "Couldn't download data for that sample", "OK");
                }
                finally
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    SetStatusMessage("Ready", false);
                    CancelButton.IsVisible = false; ;
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
                        try
                        {
                            string offlineDataPath = DataManager.GetDataFolder(offlineItemId);
                            Directory.Delete(offlineDataPath, true);
                        }
                        catch (DirectoryNotFoundException ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
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
                CancellationToken token = _cancellationTokenSource.Token;
                CancelButton.IsVisible = true;
                HashSet<string> itemIds = new HashSet<string>();
                foreach (SampleInfo sample in OfflineDataSamples)
                {
                    SetStatusMessage($"Downloading items for {sample.SampleName}", true);
                    foreach (string itemId in sample.OfflineDataItems)
                    {
                        if (!itemIds.Contains(itemId))
                        {
                            try
                            {
                                _ = StatusBar.ProgressTo(0, 10, Easing.Linear);

                                // Wait for offline data to complete
                                await DataManager.DownloadDataItem(itemId, _cancellationTokenSource.Token,
                                (info) =>
                                {
                                    SetProgress(info.Percentage, info.HasPercentage);
                                });
                            }
                            catch (OperationCanceledException)
                            {
                                await Application.Current.MainPage.DisplayAlert(string.Empty, "Download canceled", "OK");
                                return;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                            itemIds.Add(itemId);
                        }
                    }
                }
                await Application.Current.MainPage.DisplayAlert(string.Empty, "All data downloaded", "OK");
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
                foreach (var d in Directory.GetDirectories(offlineDataPath))
                {
                    try
                    {
                        Directory.Delete(d, true);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"{ex.Message} - {d}");
                    }
                }

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
            StatusBar.IsVisible = isRunning;
            OfflineDataView.IsEnabled = !isRunning;
        }

        public void SetProgress(int percentage, bool hasPercentage)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                StatusBar.IsVisible = hasPercentage;
                IndefiniteSpinner.IsVisible = !hasPercentage;
                if (percentage > 0 && hasPercentage)
                {
                    double progress = percentage / 100.0;
                    StatusBar.ProgressTo(progress, 10, Easing.Linear);
                }
            });
        }
    }
}
