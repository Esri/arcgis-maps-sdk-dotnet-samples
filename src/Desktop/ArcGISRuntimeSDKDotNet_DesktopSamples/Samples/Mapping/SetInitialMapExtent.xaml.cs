using System.Windows.Controls;
using Esri.ArcGISRuntime.Geometry;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Shows how to set the initial extent of the map (Map.InitialExtent).
    /// </summary>
	/// <category>Mapping</category>
	public partial class SetInitialMapExtent : UserControl
    {
        public SetInitialMapExtent()
        {
            InitializeComponent();

            // Note: uncomment the following to set the initial extent of the map in code.
            // mapView.Map.InitialExtent = new Envelope(-13044000, 3855000, -13040000, 3858000); 
        }
    }
}
