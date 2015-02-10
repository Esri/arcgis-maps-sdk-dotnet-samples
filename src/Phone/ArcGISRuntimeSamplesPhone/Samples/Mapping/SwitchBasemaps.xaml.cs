using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace ArcGISRuntime.Samples.Phone.Samples
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

        private void basemapLayerComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mapView1 != null && mapView1.Map.Layers.Count > 0)
            {
                mapView1.Map.Layers.RemoveAt(0);

                mapView1.Map.Layers.Add(new ArcGISTiledMapServiceLayer()
                {
                    ServiceUri = ((sender as ComboBox).SelectedItem as ComboBoxItem).Tag as string
                });
            }
        }
    }
}
