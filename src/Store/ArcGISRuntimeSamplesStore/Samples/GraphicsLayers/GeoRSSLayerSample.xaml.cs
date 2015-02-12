using Esri.ArcGISRuntime.Layers;
using System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Creates a GeoRssLayer based on the United States Geological Survey earthquake feed.
    /// </summary>
    /// <title>GeoRSS Layer</title>
    /// <category>Graphics Layers</category>
	public sealed partial class GeoRssLayerSample : Page
    {
        public GeoRssLayerSample()
        {
            this.InitializeComponent();
        }

        private async void OnLayerUpdateButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                var rssLayer = MyMapView.Map.Layers["RssLayer"] as GeoRssLayer;
                await rssLayer.UpdateAsync();
                await new MessageDialog("Layer updated successfully", "GeoRSS Layer Sample").ShowAsync();
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog(ex.Message, "GeoRSS Layer Sample").ShowAsync();
            }
        }
    }
}
