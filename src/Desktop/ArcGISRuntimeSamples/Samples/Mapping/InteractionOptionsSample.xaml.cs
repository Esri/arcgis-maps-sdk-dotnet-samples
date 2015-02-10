using Esri.ArcGISRuntime.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample shows how to change interaction settings that controls the MapView.
    /// </summary>
    /// <title>Interaction Options</title>
    /// <category>Mapping</category>
    public partial class InteractionOptionsSample : UserControl
    {
        public InteractionOptionsSample()
        {
            InitializeComponent();
        }

        private void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.LoadError == null)
                return;

            MessageBox.Show(
                string.Format("Error when loading layer. {0}", e.LoadError.ToString()),
                              "Error loading layer");
        }

        private void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            Debug.WriteLine("MapViewTapped");
        }

        private void MyMapView_MapViewDoubleTapped(object sender, MapViewInputEventArgs e)
        {
            Debug.WriteLine("MapViewDoubleTapped");
        }

        private void MyMapView_MapViewHolding(object sender, MapViewInputEventArgs e)
        {
            Debug.WriteLine("Holding");
        }

        private void rotationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MyMapView.InteractionOptions.RotationOptions.IsEnabled)
            {
                MyMapView.SetRotation(e.NewValue);
            }
        }
    }
}
