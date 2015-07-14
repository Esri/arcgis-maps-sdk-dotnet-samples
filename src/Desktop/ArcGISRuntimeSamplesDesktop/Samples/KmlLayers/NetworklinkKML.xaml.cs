using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Esri.ArcGISRuntime.Layers;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates binding kml to tree view and show Networklink features get updated
    /// </summary>
    /// <title>NetworkLink with TreeView</title>
    /// <category>Layers</category>
    /// <subcategory>Kml Layers</subcategory>
    public partial class NetworklinkKML : UserControl
    {
        /// <summary>Construct KML NetworkLink with TreeView sample control</summary>       
        public NetworklinkKML()
        {
            InitializeComponent();
        }

        private void MySceneView_LayerLoaded(object sender, Esri.ArcGISRuntime.Controls.LayerLoadedEventArgs e)
        {
            //Add kml layer to the treeView
            if (e.Layer is KmlLayer)
            {
                ObservableCollection<KmlFeature> kmlFeatureList = new ObservableCollection<KmlFeature>();
                kmlFeatureList.Add((e.Layer as KmlLayer).RootFeature);

                treeView.ItemsSource = kmlFeatureList;
            }
        }
    }
}
