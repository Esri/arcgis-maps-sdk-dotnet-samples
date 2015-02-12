using ArcGISRuntime.Samples.StoreViewer.Common;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.StoreViewer
{
    /// <summary>
    /// Application Settings Flyout for deleting sample data from the local data folder
    /// </summary>
    public sealed partial class LocalDataSettingsFlyout : SettingsFlyout
    {
        public static readonly Guid LOCALDATA_SETTINGS_ID = new Guid("{df28d422-a8f5-4b9c-8e69-f82d3e67ee5b}");

        private SampleDataViewModel _vm;

        public LocalDataSettingsFlyout()
        {
            this.InitializeComponent();

            _vm = new SampleDataViewModel();
            this.DataContext = _vm;
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            await _vm.DownloadLocalDataAsync();
        }
    }
}
