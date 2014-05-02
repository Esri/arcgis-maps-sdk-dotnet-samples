using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples
{
    /// <summary>
    /// Application Settings Flyout for deleting sample data from the local data folder
    /// </summary>
    public sealed partial class LocalDataSettingsFlyout : SettingsFlyout
    {
        public LocalDataSettingsFlyout()
        {
            this.InitializeComponent();

            this.Loaded += LocalDataSettingsFlyout_Loaded;
        }

        private void LocalDataSettingsFlyout_Loaded(object sender, RoutedEventArgs e)
        {
            txtSampleDataPath.Text = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        }

        private async void ResetSampleDataButton_Click(object sender, RoutedEventArgs e)
        {
            var localDataFolders = await ApplicationData.Current.LocalFolder.GetFoldersAsync();
            foreach (var folder in localDataFolders)
            {
                try
                {
                    await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                catch { }
            }
        }
    }
}
