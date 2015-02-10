using Esri.ArcGISRuntime.Layers;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Demonstrates adding a Web tiled layer to a Map in XAML, and changing layer properties in code behind.
    /// Also demonstrates including Attribution as per Terms of Use for the various layers. 
    /// </summary>
    /// <title>Web Tiled Layer</title>
    /// <category>Layers</category>
    /// <subcategory>Tiled Layers</subcategory>
    public partial class WebTiledLayerSample : UserControl
    {

        public WebTiledLayerSample()
        {
            InitializeComponent();

            MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
        }

        void MyMapView_SpatialReferenceChanged(object sender, System.EventArgs e)
        {
            cboLayers.SelectedIndex = 0;
        }

        private void cboLayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MyMapView.SpatialReference == null)
                return;

            MyMapView.Map.Layers.Remove("MyWebTiledLayer");

            WebTiledLayer webTiledLayer = new WebTiledLayer() { ID = "MyWebTiledLayer" };

            switch (cboLayers.SelectedIndex)
            {
                //Esri National Geographic
                case 0:
                    webTiledLayer.TemplateUri = "http://{subDomain}.arcgisonline.com/ArcGIS/rest/services/NatGeo_World_Map/MapServer/tile/{level}/{row}/{col}";
                    webTiledLayer.SubDomains = new string[] { "server", "services" };
                    Attribution.Content = Attribution.Resources["NatGeoAttribution"];
                    Attribution.Visibility = Visibility.Visible;
                    break;
                //MapQuest
                case 1:
                    webTiledLayer.TemplateUri = "http://otile1.mqcdn.com/tiles/1.0.0/vx/map/{level}/{col}/{row}.jpg";
                    Attribution.Content = Attribution.Resources["MapQuestAttribution"];
                    Attribution.Visibility = Visibility.Visible;
                    break;
                //OpenCycleMap
                case 2:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tile.opencyclemap.org/cycle/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c" };
                    Attribution.Content = Attribution.Resources["OpenCycleMapAttribution"];
                    Attribution.Visibility = Visibility.Visible;
                    break;
                //Cloudmade Midnight Commander
                case 3:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tile.cloudmade.com/1a1b06b230af4efdbb989ea99e9841af/999/256/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c" };
                    Attribution.Visibility = Visibility.Collapsed; // No attribution info here, so keep dialog collapsed.
                    break;
                //Cloudmade Pale Dawn
                case 4:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tile.cloudmade.com/1a1b06b230af4efdbb989ea99e9841af/998/256/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c" };
                    Attribution.Visibility = Visibility.Collapsed; // No attribution info here, so keep dialog collapsed.
                    break;
                //MapBox Dark
                case 5:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tiles.mapbox.com/v3/examples.map-cnkhv76j/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c", "d" };
                    Attribution.Content = Attribution.Resources["MapboxAttribution"];
                    Attribution.Visibility = Visibility.Visible;
                    break;
                //Mapbox Terrain
                case 6:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tiles.mapbox.com/v3/mapbox.mapbox-warden/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c", "d" };
                    Attribution.Content = Attribution.Resources["MapboxAttribution"];
                    Attribution.Visibility = Visibility.Visible;
                    break;
                //Stamen Terrain
                case 7:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tile.stamen.com/terrain/{level}/{col}/{row}.jpg";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c", "d" };
                    Attribution.Content = Attribution.Resources["StamenOtherAttribution"];
                    Attribution.Visibility = Visibility.Visible;
                    break;
                //Stamen Watercolor
                case 8:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tile.stamen.com/watercolor/{level}/{col}/{row}.jpg";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c", "d" };
                    Attribution.Content = Attribution.Resources["StamenOtherAttribution"];
                    Attribution.Visibility = Visibility.Visible;
                    break;
                //Stamen Toner
                case 9:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tile.stamen.com/toner/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c", "d" };
                    Attribution.Content = Attribution.Resources["StamenTonerAttribution"];
                    Attribution.Visibility = Visibility.Visible;
                    break;
            }

            MyMapView.Map.Layers.Add(webTiledLayer);
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }


}
