using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Shared.Models;
using Esri.ArcGISRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ArcGISRuntime
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsWindow : UserControl
    {
        private static string _runtimeVersion = "";
        CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private List<SampleInfo> OfflineDataSamples;

        public SettingsWindow()
        {
            this.InitializeComponent();

            // Set up version info.
            if (String.IsNullOrWhiteSpace(_runtimeVersion))
            {
                var runtimeTypeInfo = typeof(ArcGISRuntimeEnvironment).GetTypeInfo();
                var rtVersion = FileVersionInfo.GetVersionInfo(runtimeTypeInfo.Assembly.Location);
                _runtimeVersion = rtVersion.FileVersion;
            }
            VersionTextField.Text = _runtimeVersion;

            // Set up license info.
            string markdownPath  = "Resources\\licenses.md";
            MarkDownBlock.Text = System.IO.File.ReadAllText(markdownPath);
            MarkDownBlock.Background = new ImageBrush() { Opacity=0 };

            // Set up offline data.
            SampleDataListView.ItemsSource =  SampleManager.Current.AllSamples.Where(m => m.OfflineDataItems?.Any() ?? false).ToList();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private async void Download_All_Click(object sender, RoutedEventArgs e)
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

                await new MessageDialog("All data downloaded").ShowAsync();
            }
            catch (OperationCanceledException)
            {
                await new MessageDialog("Download canceled").ShowAsync();
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
                await new MessageDialog("Download canceled", "Error").ShowAsync();
            }
            finally
            {
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

                await new MessageDialog("All data deleted").ShowAsync();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                await new MessageDialog("Couldn't delete the offline data folder", "Error").ShowAsync();
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

                await Windows.System.Launcher.LaunchUriAsync(new Uri(onlinePath));
            }

        }

        private async void Download_Now_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetStatusMessage("Downloading sample data", true);
                SampleInfo sample = (SampleInfo)((Button)sender).Tag;

                await DataManager.EnsureSampleDataPresent(sample);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
                await new MessageDialog("Couldn't download data for that sample", "Error").ShowAsync();
            }
            finally
            {
                SetStatusMessage("Ready", false);
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
                await new MessageDialog($"Offline data deleted for {sample.SampleName}").ShowAsync();
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
                await new MessageDialog($"Couldn't delete offline data.", "Error").ShowAsync();
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
    }
}
