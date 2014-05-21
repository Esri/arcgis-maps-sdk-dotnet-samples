using ArcGISRuntimeSDKDotNet_StoreSamples.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples
{
    /// <summary>
    /// Application Settings Flyout for deleting sample data from the local data folder
    /// </summary>
    public sealed partial class LocalDataSettingsFlyout : SettingsFlyout
    {
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
