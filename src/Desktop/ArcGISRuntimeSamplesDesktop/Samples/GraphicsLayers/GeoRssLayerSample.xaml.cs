using System;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Creates a GeoRssLayer based on the United States Geological Survey earthquake feed and assigned a SimpleRenderer.
    /// </summary>
    /// <title>GeoRSS Layer</title>
	/// <category>Layers</category>
	/// <subcategory>Graphics Layers</subcategory>
	public partial class GeoRssLayerSample : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeoRssLayerSample"/> class.
        /// </summary>
        public GeoRssLayerSample()
        {
            InitializeComponent();
        }

        private async void OnLayerUpdateButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await rssLayer.UpdateAsync();
                MessageBox.Show("Layer updated successfully", "GeoRSS Layer Sample");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "GeoRSS Layer Sample");
            }
        }
    }
}
