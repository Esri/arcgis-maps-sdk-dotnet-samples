using Esri.ArcGISRuntime.Geometry;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Shows how to set the initial extent of the map (Map.InitialExtent).
    /// </summary>
    /// <title>Set Initial Map Extent</title>
    /// <category>Mapping</category>
	public sealed partial class SetInitialMapExtent : Page
    {
        public SetInitialMapExtent()
        {
            InitializeComponent();

            // Note: uncomment the following to set the initial extent of the map in code.
            //mapView.Map.InitialExtent = new Envelope(-13044000, 3855000, -13040000, 3858000); 
        }
    }
}
