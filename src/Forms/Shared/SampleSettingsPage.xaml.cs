using System;
using Xamarin.Forms;

namespace ArcGISRuntime
{
    public partial class SampleSettingsPage : TabbedPage
    {
        public SampleSettingsPage()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            LicensePage.Source = "https://www.google.com";
        }
    }
}