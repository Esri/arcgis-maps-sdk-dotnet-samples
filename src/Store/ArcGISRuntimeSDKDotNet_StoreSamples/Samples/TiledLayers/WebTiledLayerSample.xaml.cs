using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using Windows.System;
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

            MyMapView.SpatialReferenceChanged += MyMapView_SpatialReferenceChanged;
        }

        void MyMapView_SpatialReferenceChanged(object sender, EventArgs e)
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
                    Attribution.Visibility = Visibility.Collapsed; // No attribution info here, so keep dialog collapsed.
                    break;
                //MapQuest
                case 1:
                    webTiledLayer.TemplateUri = "http://otile1.mqcdn.com/tiles/1.0.0/vx/map/{level}/{col}/{row}.jpg";
                    webTiledLayer.CopyrightText = "Map Quest";
                    Attribution.ContentTemplate = Attribution.Resources["MapQuestAttribution"] as DataTemplate;
                    Attribution.Visibility = Visibility.Visible;
                    break;
                //OpenCycleMap
                case 2:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tile.opencyclemap.org/cycle/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c" };
                    webTiledLayer.CopyrightText = "Open Cycle Map";
                    Attribution.ContentTemplate = Attribution.Resources["OpenCycleMapAttribution"] as DataTemplate;
                    Attribution.Visibility = Visibility.Visible;
                    break;
                //Cloudmade Midnight Commander
                case 3:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tile.cloudmade.com/1a1b06b230af4efdbb989ea99e9841af/999/256/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c" };
                    webTiledLayer.CopyrightText = "Cloudmade Midnight Commander";
                    Attribution.Visibility = Visibility.Collapsed; // No attribution info here, so keep dialog collapsed.
                    break;
                //Cloudmade Pale Dawn
                case 4:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tile.cloudmade.com/1a1b06b230af4efdbb989ea99e9841af/998/256/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c" };
                    webTiledLayer.CopyrightText = "Cloudmade Pale Dawn";
                    Attribution.Visibility = Visibility.Collapsed; // No attribution info here, so keep dialog collapsed.
                    break;
                //MapBox Dark
                case 5:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tiles.mapbox.com/v3/examples.map-cnkhv76j/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c", "d" };
                    webTiledLayer.CopyrightText = "Mapbox Dark";
                    Attribution.ContentTemplate = Attribution.Resources["MapboxAttribution"] as DataTemplate;
                    Attribution.Visibility = Visibility.Visible;
                    break;
                //Mapbox Terrain
                case 6:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tiles.mapbox.com/v3/mapbox.mapbox-warden/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c", "d" };
                    webTiledLayer.CopyrightText = "Mapbox Terrain";
                    Attribution.ContentTemplate = Attribution.Resources["MapboxAttribution"] as DataTemplate;
                    Attribution.Visibility = Visibility.Visible;
                    break;
                //Stamen Terrain
                case 7:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tile.stamen.com/terrain/{level}/{col}/{row}.jpg";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c", "d" };
                    webTiledLayer.CopyrightText = "Stamen Terrain";
                    Attribution.ContentTemplate = Attribution.Resources["StamenOtherAttribution"] as DataTemplate;
                    Attribution.Visibility = Visibility.Visible;
                    break;
                //Stamen Watercolor
                case 8:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tile.stamen.com/watercolor/{level}/{col}/{row}.jpg";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c", "d" };
                    webTiledLayer.CopyrightText = "Stamen Watercolor";
                    Attribution.ContentTemplate = Attribution.Resources["StamenOtherAttribution"] as DataTemplate;
                    Attribution.Visibility = Visibility.Visible;
                    break;
                //Stamen Toner
                case 9:
                    webTiledLayer.TemplateUri = "http://{subDomain}.tile.stamen.com/toner/{level}/{col}/{row}.png";
                    webTiledLayer.SubDomains = new string[] { "a", "b", "c", "d" };
                    webTiledLayer.CopyrightText = "Stamen Toner";
                    Attribution.ContentTemplate = Attribution.Resources["StamenTonerAttribution"] as DataTemplate;
                    Attribution.Visibility = Visibility.Visible;
                    break;
            }

            MyMapView.Map.Layers.Add(webTiledLayer);
        }

        private async void Hyperlink_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            // Navigate to specified Uri
            await Launcher.LaunchUriAsync(sender.NavigateUri);
        }
    }
}
