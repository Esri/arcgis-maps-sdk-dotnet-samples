using Esri.ArcGISRuntime.Layers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Demonstrates changing the basemap layer in a map by switching  between ArcGIS tiled map services layers hosted by ArcGIS Online.
    /// </summary>
    /// <title>Switch Basemaps</title>
    /// <category>Mapping</category>
    public sealed partial class SwitchBasemaps : Page
    {
        public SwitchBasemaps()
        {
            this.InitializeComponent();
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            MyMapView.Map.Layers.RemoveAt(0);

            MyMapView.Map.Layers.Add(new ArcGISTiledMapServiceLayer()
            {
                ServiceUri = ((RadioButton)sender).Tag as string
            });
        }
    }
}
