using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Security;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample shows how to add the ArcGIS Traffic service to a map.
    /// </summary>
    /// <title>Traffic</title>
    /// <category>ArcGIS Online Services</category>
    public partial class Traffic : UserControl
    {
        public Traffic()
        {
            InitializeComponent();

            IdentityManager.Current.ChallengeMethod = PortalSecurity.Challenge;

            mapView.Map.InitialExtent = new Envelope(-13230693.582, 3941779.273, -12928937.030, 4095486.517, SpatialReferences.WebMercator);
        }
    }
}
