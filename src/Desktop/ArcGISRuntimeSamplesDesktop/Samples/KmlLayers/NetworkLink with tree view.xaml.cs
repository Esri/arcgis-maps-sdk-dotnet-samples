using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArcGISRuntime.Samples.Desktop.Samples.KmlLayers
{
    /// <summary>
    /// This sample show you how to bind kml to tree view and when Networklink features get updated
    /// </summary>
    /// <title>NetworkLink with TreeView</title>
    /// <category>Layers</category>
    /// <subcategory>Kml Layers</subcategory>
    public partial class NetworkLink_with_tree_view : UserControl
    {

        /// <summary>Construct KML NetworkLink with TreeView sample control</summary>       
        public NetworkLink_with_tree_view()
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

     public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
