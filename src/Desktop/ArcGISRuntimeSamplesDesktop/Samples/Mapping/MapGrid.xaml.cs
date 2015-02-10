using Esri.ArcGISRuntime.Controls;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
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
			DataContext = this;
		}

		private void gridTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			switch (gridTypeCombo.SelectedIndex)
			{
				case 0:
					MyMapView.MapGrid = null;
					break;
				case 1:
					LatLonMapGrid latLongMapGrid = new LatLonMapGrid();
					// Use Geographic positioning for LatLong MapGrid.
					latLongMapGrid.LabelPosition = MapGridLabelPosition.Geographic;
					MyMapView.MapGrid = latLongMapGrid;
					break;
				case 2:
					MgrsMapGrid mgrsGrid = new MgrsMapGrid();
					// Use Screen-aligned TopLeft position for MGRS MapGrid.
					mgrsGrid.LabelPosition = MapGridLabelPosition.TopLeft;
					MyMapView.MapGrid = mgrsGrid;
					break;
				case 3:
					UtmMapGrid utmGrid = new UtmMapGrid();
					MyMapView.MapGrid = utmGrid;
					break;
				case 4:
					UsngMapGrid usngMapGrid = new UsngMapGrid();
					// Use Screen-aligned AllSides option for USNG MapGrid.
					usngMapGrid.LabelPosition = MapGridLabelPosition.AllSides;
					MyMapView.MapGrid = usngMapGrid;
					break;
			}
		}
	}
}
