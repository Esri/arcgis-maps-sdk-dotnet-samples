using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        }

        private void Download_All_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Delete_All_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Open_AGOL_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Download_Now_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
