using Esri.ArcGISRuntime.Layers;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// 
    /// </summary>
    /// <title>Web Tiled Layer</title>
    /// <category>Tiled Layers</category>
    public sealed partial class WebTiledLayerSample : Page
    {
        public WebTiledLayerSample()
        {
            this.InitializeComponent();

            string[] ABCD = new string[] { "a", "b", "c", "d" };
            string[] MQ_SUBDOMAINS = new string[] { "mtile01", "mtile02", "mtile03", "mtile04" };
            string[] ABC = new string[] { "a", "b", "c" };

            List<WebTiledLayerComboItem> items = new List<WebTiledLayerComboItem>()
            {
                new WebTiledLayerComboItem("Stamen Watercolor",
                    "http://{subDomain}.tile.stamen.com/watercolor/{level}/{col}/{row}.jpg",
                    "Stamen Watercolor",
                    ABCD),
                
                new WebTiledLayerComboItem("Stamen Toner",
                    "http://{subDomain}.tile.stamen.com/toner/{level}/{col}/{row}.png",
                    "Stamen Toner",
                    ABCD),
                    
                new WebTiledLayerComboItem("Stamen Terrain",
                    "http://{subDomain}.tile.stamen.com/terrain/{level}/{col}/{row}.jpg",
                    "Stamen Terrain",
                    ABCD),

                new WebTiledLayerComboItem("Apple's OpenStreetMap",
                    "http://gsp2.apple.com/tile?api=1&style=slideshow&layers=default&lang=en_GB&z={level}&x={col}&y={row}&v=9",
                    "Apple's rendering of OSM data",
                    null),

                new WebTiledLayerComboItem("MapBox Terrain",
                    "http://{subDomain}.tiles.mapbox.com/v3/mapbox.mapbox-warden/{level}/{col}/{row}.png",
                    "MapBox Terrain",
                    ABCD),

                new WebTiledLayerComboItem("MapBox Streets",
                    "http://{subDomain}.tiles.mapbox.com/v3/examples.map-vyofok3q/{level}/{col}/{row}.png",
                    "MapBox Streets",
                    ABCD),

                new WebTiledLayerComboItem("MapBox Dark",
                    "http://{subDomain}.tiles.mapbox.com/v3/examples.map-cnkhv76j/{level}/{col}/{row}.png",
                    "MapBox Dark",
                    ABCD),

                new WebTiledLayerComboItem("OpenCycleMap",
                    "http://{subDomain}.tile.opencyclemap.org/cycle/{level}/{col}/{row}.png",
                    "OpenCycleMap",
                    ABC),

                new WebTiledLayerComboItem("MapQuest",
                    "http://{subDomain}.mqcdn.com/tiles/1.0.0/vx/map/{level}/{col}/{row}.jpg",
                    "MapQuest",
                    MQ_SUBDOMAINS)
            };

            webTiledLayerComboBox1.ItemsSource = items;
            webTiledLayerComboBox1.DisplayMemberPath = "Name";
			MyMapView.Loaded += MyMapView_Loaded;
		}

		private void MyMapView_Loaded(object sender, RoutedEventArgs e)
		{
			webTiledLayerComboBox1.SelectedIndex = 0;
		}

        private class WebTiledLayerComboItem
        {
            public WebTiledLayerComboItem(string name, string urlTemplate, string copyrightText, string[] subDomains)
            {
                this.UrlTemplate = urlTemplate;
                this.CopyrightText = copyrightText;
                this.SubDomains = subDomains;
                this.Name = name;
            }

            public string Name { get; private set; }
            public string UrlTemplate { get; private set; }
            public string CopyrightText { get; private set; }
            public string[] SubDomains { get; private set; }
        }

        private void webTiledLayerComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WebTiledLayerComboItem selectedItem = (sender as ComboBox).SelectedItem as WebTiledLayerComboItem;
            if (selectedItem != null)
            {
				if (MyMapView.Map.Layers["MyWebTiledLayer"] != null)
					MyMapView.Map.Layers.Remove("MyWebTiledLayer");

				var myWebTiledLayer = new WebTiledLayer { ID = "MyWebTiledLayer" };
				myWebTiledLayer.CopyrightText = selectedItem.CopyrightText;
				myWebTiledLayer.TemplateUri = selectedItem.UrlTemplate;
				myWebTiledLayer.SubDomains = selectedItem.SubDomains;
				MyMapView.Map.Layers.Add(myWebTiledLayer);
            }
        }
    }
}
