﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ArcGISRuntime.Converters;
using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Shared.Models;
using ArcGISRuntime.WPF.Viewer;
using Esri.ArcGISRuntime;

namespace ArcGISRuntime
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private static string _runtimeVersion = "";
        private readonly MarkedNet.Marked _markdownRenderer = new MarkedNet.Marked();
        CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private List<SampleInfo> OfflineDataSamples;

        public SettingsWindow()
        {
            InitializeComponent();

            // Set up version info.
            if (String.IsNullOrWhiteSpace(_runtimeVersion))
            {
                var runtimeTypeInfo = typeof(ArcGISRuntimeEnvironment).GetTypeInfo();
                var rtVersion = FileVersionInfo.GetVersionInfo(runtimeTypeInfo.Assembly.Location);
                _runtimeVersion = rtVersion.FileVersion;
            }
            VersionTextField.Text = _runtimeVersion;

            // Set up license info.
            string markdownPath = System.IO.Path.Combine(App.ResourcePath, "Resources", "licenses.md");
            string cssPath = System.IO.Path.Combine(App.ResourcePath, "Resources", "github-markdown.css");
            string licenseContent = System.IO.File.ReadAllText(markdownPath);
            licenseContent = _markdownRenderer.Parse(licenseContent);
            string htmlString = "<!doctype html><head><link rel=\"stylesheet\" href=\"" + cssPath + "\" /></head><body class=\"markdown-body\">" + licenseContent + "</body>";
            LicenseView.NavigateToString(htmlString);

            // Set up offline data.
            OfflineDataSamples = SampleManager.Current.AllSamples.Where(m => m.OfflineDataItems?.Any() ?? false).ToList();

            SampleDataListView.ItemsSource = OfflineDataSamples;
        }

        private void OpenInAgol_Clicked(object sender, RoutedEventArgs e)
        {
            SampleInfo sample = (SampleInfo) ((Button) sender).Tag;

            foreach (var offlineItem in sample.OfflineDataItems)
            {
                string onlinePath = $"https://www.arcgis.com/home/item.html?id={offlineItem}";

                System.Diagnostics.Process.Start(onlinePath);
            }
        }

        private async void DownloadNow_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                SetStatusMessage("Downloading sample data", true);
                SampleInfo sample = (SampleInfo) ((Button) sender).Tag;

                await DataManager.EnsureSampleDataPresent(sample);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
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
                System.Diagnostics.Debug.WriteLine(exception);
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
            System.Diagnostics.Process.Start(DataManager.GetDataFolder());
        }

        private void DeleteData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetStatusMessage("Deleting sample data", true);

                SampleInfo sample = (SampleInfo) ((Button) sender).Tag;

                foreach (string offlineItemId in sample.OfflineDataItems)
                {
                    string offlineDataPath = DataManager.GetDataFolder(offlineItemId);

                    Directory.Delete(offlineDataPath);
                }

                MessageBox.Show($"Offline data deleted for {sample.SampleName}");
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
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
