using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates adding a Web tiled layer to a map.
    /// </summary>
    /// <title>Web Tiled Layer</title>
    /// <category>Tiled Layers</category>
    public sealed partial class WebTiledLayerSample : Page
    {
        public WebTiledLayerSample()
        {
            this.InitializeComponent();

            MyMapView.Loaded += MyMapView_Loaded;
        }

        private void MyMapView_Loaded(object sender, RoutedEventArgs e)
        {
            cboLayers.SelectedIndex = 0;
        }

        private void cboLayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			if (cboLayers == null)
				return;

			MyMapView.Map.Layers.Remove("MyWebTiledLayer");

			WebTiledLayer webTiledLayer = new WebTiledLayer() { ID = "MyWebTiledLayer" };

            switch (cboLayers.SelectedIndex)
            {
                //Esri National Geographic
                case 0:
                    webTiledLayer.TemplateUri = "http://{subDomain}.arcgisonline.com/ArcGIS/rest/services/NatGeo_World_Map/MapServer/tile/{level}/{row}/{col}";
                    webTiledLayer.SubDomains = new string[] { "server", "services" };
                    webTiledLayer.CopyrightText = "National Geographic, Esri, DeLorme, NAVTEQ, UNEP-WCMC, USGS, NASA, ESA, METI, NRCAN, GEBCO, NOAA, iPC";
                    break;
                //MapQuest
                case 1:
                    webTiledLayer.TemplateUri = "http://mtile01.mqcdn.com/tiles/1.0.0/vx/map/{level}/{col}/{row}.jpg";
                    webTiledLayer.CopyrightText = "Map Quest";
                    break;
                //OpenCycleMap
                case 2:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tile.opencyclemap.org/cycle/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c" };
                    webTiledLayer.CopyrightText = "Open Cycle Map";
                    break;
                //Cloudmade Midnight Commander
                case 3:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tile.cloudmade.com/1a1b06b230af4efdbb989ea99e9841af/999/256/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c" };
                    webTiledLayer.CopyrightText = "Cloudmade Midnight Commander";
                    break;
                //Cloudmade Pale Dawn
                case 4:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tile.cloudmade.com/1a1b06b230af4efdbb989ea99e9841af/998/256/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c" };
                    webTiledLayer.CopyrightText = "Cloudmade Pale Dawn";
                    break;
                //MapBox Dark
                case 5:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tiles.mapbox.com/v3/examples.map-cnkhv76j/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c", "d" };
                    webTiledLayer.CopyrightText = "Mapbox Dark";
                    break;
                //Mapbox Streets
                case 6:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tiles.mapbox.com/v3/examples.map-vyofok3q/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c", "d" };
                    webTiledLayer.CopyrightText = "Mapbox Streets";
                    break;
                //Mapbox Terrain
                case 7:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tiles.mapbox.com/v3/mapbox.mapbox-warden/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c", "d" };
                    webTiledLayer.CopyrightText = "Mapbox Terrain";
                    break;
                //Apple's OpenStreetMap
                case 8:
                    webTiledLayer.TemplateUri = "http://gsp2.apple.com/tile?api=1&style=slideshow&layers=default&lang=en_GB&z={level}&x={col}&y={row}&v=9";
                    webTiledLayer.CopyrightText = "Apple's rendering of OSM data.";
                    break;
                //Stamen Terrain
                case 9:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tile.stamen.com/terrain/{level}/{col}/{row}.jpg";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c", "d" };
                    webTiledLayer.CopyrightText = "Stamen Terrain";
                    break;
                //Stamen Watercolor
                case 10:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tile.stamen.com/watercolor/{level}/{col}/{row}.jpg";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c", "d" };
                    webTiledLayer.CopyrightText = "Stamen Watercolor";
                    break;
                //Stamen Toner
                case 11:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tile.stamen.com/toner/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c", "d" };
                    webTiledLayer.CopyrightText = "Stamen Toner";
                    break;
            }
			MyMapView.Map.Layers.Add(webTiledLayer);
        }
    }
}
