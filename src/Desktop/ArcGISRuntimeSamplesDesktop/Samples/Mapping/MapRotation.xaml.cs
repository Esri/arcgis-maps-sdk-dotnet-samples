using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample shows how to rotate a map using the MapView.Rotation property.  The slider control in the top right corner of the map controls the rotation angle of the map view.
    /// </summary>
    /// <title>Map Rotation</title>
	/// <category>Mapping</category>
	public partial class MapRotation : UserControl
    {
        public MapRotation()
        {
            InitializeComponent();
        }

		private void rotationSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
		{
			MyMapView.SetRotation(e.NewValue);
		}
    }
}
