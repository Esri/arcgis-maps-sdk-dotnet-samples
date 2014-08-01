using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample demonstrates use of the GeometryEngine.GeodesicEllipse to calculate a geodesic ellipse. Also shows is how to calculate a geodesic sector using GeometryEngine.GeodesicSector to create a gedesic sector emanating from point. To use the sample, fill in the ellipse properties and click the 'Geodesic Ellipse' button, then click a center point on the map. The click point and a geodesic ellipse and sector centered at the point will be displayed.
    /// </summary>
    /// <title>Geodesic Ellipse</title>
	/// <category>Geometry</category>
	public partial class GeodesicEllipse : UserControl
    {
        private Symbol _pinSymbol;
        private Symbol _sectorSymbol;

        /// <summary>Construct Geodesic Ellipse sample control</summary>
        public GeodesicEllipse()
        {
            InitializeComponent();

            _pinSymbol = layoutGrid.Resources["PointSymbol"] as Symbol;
            _sectorSymbol = layoutGrid.Resources["SectorSymbol"] as Symbol;
        }

        private async void EllipseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                while (MyMapView.Extent != null)
                {
                    // Accept user point
                    var point = await MyMapView.Editor.RequestPointAsync();

                    // create the geodesic ellipse
                    var radius1 = (double)comboRadius1.SelectedItem;
                    var radius2 = (double)comboRadius2.SelectedItem;
                    var axis = sliderAxis.Value;
                    var maxLength = (double)comboSegmentLength.SelectedItem;
                    var param = new GeodesicEllipseParameters(point, radius1, radius2, LinearUnits.Miles)
                    { 
                        AxisDirection = axis, 
                        MaxPointCount = 10000, 
                        MaxSegmentLength = maxLength
                    };
                    var ellipse = GeometryEngine.GeodesicEllipse(param);

                    //show geometries on map
					graphicsOverlay.Graphics.Clear();
					graphicsOverlay.Graphics.Add(new Graphic(point, _pinSymbol));
					graphicsOverlay.Graphics.Add(new Graphic(ellipse));

                    // geodesic sector
                    if ((bool)chkSector.IsChecked)
                    {
                        var sectorParams = new GeodesicSectorParameters(point, radius1, radius2, LinearUnits.Miles)
                        {
                            AxisDirection = axis,
                            MaxPointCount = 10000,
                            MaxSegmentLength = maxLength,
                            SectorAngle = sliderSectorAxis.Value,
                            StartDirection = sliderSectorStart.Value
                        };
                        var sector = GeometryEngine.GeodesicSector(sectorParams);

						graphicsOverlay.Graphics.Add(new Graphic(sector, _sectorSymbol));
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Geometry Engine Failed!");
            }
        }
    }
}
