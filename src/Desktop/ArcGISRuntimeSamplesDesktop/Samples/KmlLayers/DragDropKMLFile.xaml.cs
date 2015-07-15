using System;
using System.Windows;
using System.Windows.Controls;
using Esri.ArcGISRuntime.Layers;


namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates how you can add KML or KMZ file from your machine to the map using Drag/Drop. 
    /// </summary>
    /// <title>Drag and Drop</title>
    /// <category>Layers</category>
    /// <subcategory>Kml Layers</subcategory>
    public partial class DragDropKMLFile : UserControl
    {
        public DragDropKMLFile()
        {
            InitializeComponent();
        }

        private async void MyMapView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                KmlLayer kmlLayer = new KmlLayer(new Uri(files[0]));
                await kmlLayer.InitializeAsync();

                //Add the kml layer
                MyMapView.Map.Layers.Add(kmlLayer);

                //Zoom to the kml layer if available
                if (kmlLayer.RootFeature.Viewpoint != null)
                    await MyMapView.SetViewAsync(kmlLayer.RootFeature.Viewpoint);
            }
        }

        private void ResetMapButton_Click(object sender, RoutedEventArgs e)
        {
            MyMapView.Map.Layers.Clear();
            MyMapView.Map.Layers.Add(new ArcGISTiledMapServiceLayer(new Uri("http://services.arcgisonline.com/ArcGIS/rest/services/World_Topo_Map/MapServer")));
        }
    }
}
