using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ArcGISRuntimeSDKDotNet_PhoneSamples.Resources;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
			DataContext = SampleDatasource.Current;
        }

		private void LongListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var item = ((LongListSelector)sender).SelectedItem as Sample;
			if(item != null)
			{
                Uri uri = new Uri(string.Format("/{0}/{1}/{2}.xaml", 
                        "Samples",
                        item.Category.Replace(" ", string.Empty),
                        item.Name.Replace(" ", string.Empty))
                    , UriKind.Relative);

				NavigationService.Navigate(uri);
			}
		}
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			// Force a collect of memory (WinPhone's Garbage Collector can get a little 
			// too lazy with cleaning up before it runs out of memory, so we force it to
			// clean up the map which after running many samples can cause some memory issues)
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
    }
}