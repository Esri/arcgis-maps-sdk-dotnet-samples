using ArcGISRuntime.Samples.Models;
using System;
using System.Diagnostics;

namespace ArcGISRuntime.Desktop.Viewer
{
    public partial class Description
    {
        public Description()
        {
            InitializeComponent();
        }

        private void OpenTutorial(object sender, System.Windows.RoutedEventArgs e)
        {
            var sampleModel = DataContext as SampleModel;
            Process.Start(new ProcessStartInfo(sampleModel.Link));
            e.Handled = true;
        }
    }
}
