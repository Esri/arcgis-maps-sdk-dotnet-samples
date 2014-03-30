using Esri.ArcGISRuntime.Controls;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample shows how to enable a map grid using the MapView.MapGrid property.  The user is allowed to switch between the standard grid types (Latitude / Longitude, MGRS, UTM, and USNG) by using a combobox in the upper right corner of the screen.
    /// </summary>
    /// <title>Map Grid</title>
	/// <category>Mapping</category>
	public partial class MapGrid : UserControl
    {
        public List<string> GridTypes { get; set; }

        private MapGridLevelCollection _mapGridLevels;

        public MapGrid()
        {
            InitializeComponent();

            GridTypes = new List<string>() { "None", "Lat/Lon", "MGRS", "UTM", "USNG" };

            _mapGridLevels = new MapGridLevelCollection() {
                new MapGridLevel { LineColor = Colors.Green, LineWidth = 2 },
                new MapGridLevel { LineColor = Colors.Yellow, LineWidth = 1 }
            };

            DataContext = this;
        }

        private void gridTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (gridTypeCombo.SelectedIndex)
            {
                case 0:
                    mapView.MapGrid = null;
                    break;
                case 1: 
                    mapView.MapGrid = new LatLonMapGrid(LatLonMapGridLabelStyle.DecimalDegrees, _mapGridLevels);
                    break;
                case 2:
                    mapView.MapGrid = new MgrsMapGrid(_mapGridLevels);
                    break;
                case 3:
                    mapView.MapGrid = new UtmMapGrid(_mapGridLevels);
                    break;
                case 4:
                    mapView.MapGrid = new UsngMapGrid(_mapGridLevels);
                    break;
            }
        }
    }
}
