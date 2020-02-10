using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xamarin.Forms;

namespace ArcGISRuntime
{
    public partial class SampleSettingsPage : TabbedPage
    {
        private static string _runtimeVersion = "";
        private CancellationTokenSource _cancellationTokenSource;
        private List<SampleInfo> OfflineDataSamples;
        private readonly MarkedNet.Marked _markdownRenderer = new MarkedNet.Marked();

        public SampleSettingsPage()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            LicensePage.Source = "https://www.google.com";

            // Set up offline data.
            OfflineDataSamples = SampleManager.Current.AllSamples.Where(m => m.OfflineDataItems?.Any() ?? false).ToList();
            OfflineDataView.ItemsSource = OfflineDataSamples;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void DownloadClicked(object sender, EventArgs e)
        {
            if(((Button)sender).CommandParameter is SampleInfo sampleInfo)
            {
                // Download data for that sample.
            }
        }

        private void AGOLClicked(object sender, EventArgs e)
        {
            if (((Button)sender).CommandParameter is SampleInfo sampleInfo)
            {
                // Open data for that sample in AGOL.
            }
        }

        private void DeleteClicked(object sender, EventArgs e)
        {
            if (((Button)sender).CommandParameter is SampleInfo sampleInfo)
            {
                // Delete data for that sample.
            }
        }
    }
}