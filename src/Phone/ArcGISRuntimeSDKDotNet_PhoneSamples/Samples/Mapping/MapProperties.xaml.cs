using Microsoft.Phone.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Mapping</category>
	public partial class MapProperties : PhoneApplicationPage
    {
        public MapProperties()
        {
            InitializeComponent();
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ((ToggleButton)sender).Content = "Less...";
            WKTTextBlock.Visibility = Visibility.Visible;
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ((ToggleButton)sender).Content = "More...";
            WKTTextBlock.Visibility = Visibility.Collapsed;
        }
    }
}