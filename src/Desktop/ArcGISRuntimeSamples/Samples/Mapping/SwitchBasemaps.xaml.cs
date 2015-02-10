using Esri.ArcGISRuntime.Layers;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Demonstrates changing the basemap layer in a map by switching  between ArcGIS tiled map services layers hosted by ArcGIS Online.
    /// </summary>
    /// <title>Switch Basemaps</title>
	/// <category>Mapping</category>
	public partial class SwitchBasemaps : UserControl
    {
        public SwitchBasemaps()
        {
            InitializeComponent();
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
