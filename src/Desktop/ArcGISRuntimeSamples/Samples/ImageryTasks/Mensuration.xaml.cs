using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Imagery;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates the use of the mensuration (measure) operations on an ArcGIS image service. Choose a measure operation and units to be returned in the results.
    /// </summary>
    /// <title>Mensuration</title>
    /// <category>Tasks</category>
    /// <subcategory>Imagery</subcategory>
    public partial class Mensuration : UserControl
    {
        private Symbol _pointSymbol;
        private Symbol _lineSymbol;
        private Symbol _polygonSymbol;
		private GraphicsOverlay _graphicsOverlay;
        private MensurationTask _mensurationTask;

        /// <summary>Construct Mensuration sample control</summary>
        public Mensuration()
        {
            InitializeComponent();

            _pointSymbol = layoutGrid.Resources["PointSymbol"] as Symbol;
            _lineSymbol = layoutGrid.Resources["LineSymbol"] as Symbol;
            _polygonSymbol = layoutGrid.Resources["PolygonSymbol"] as Symbol;

			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];

            comboLinearUnit.ItemsSource = typeof(LinearUnits).GetProperties().Select(p => p.GetValue(null, null))
                .Except(new LinearUnit[] { LinearUnits.NauticalMiles } ).ToList();
            comboLinearUnit.SelectedItem = LinearUnits.Meters;

            comboAngularUnit.ItemsSource = new AngularUnit[] { AngularUnits.Degrees, AngularUnits.Radians };
            comboAngularUnit.SelectedItem = AngularUnits.Degrees;

            comboAreaUnit.ItemsSource = typeof(AreaUnits).GetProperties().Select(p => p.GetValue(null, null)).ToList();
            comboAreaUnit.SelectedItem = AreaUnits.SquareMeters;

            _mensurationTask = new MensurationTask(new Uri(imageServiceLayer.ServiceUri));
        }

        private async void AreaPerimeterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
				var polygon = await RequestUserShape(DrawShape.Polygon, _polygonSymbol) as Polygon;

				// Requesting shape cancelled
				if (polygon == null)
					return;

                var parameters = new MensurationAreaParameters()
                {
                    LinearUnit = comboLinearUnit.SelectedItem as LinearUnit,
                    AreaUnit = comboAreaUnit.SelectedItem as AreaUnit,
                };

                var result = await _mensurationTask.AreaAndPerimeterAsync(polygon, parameters);
                ShowResults(result, ((Button)sender).Tag.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Mensuration Error");
            }
        }

        private async void CentroidButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
				var polygon = await RequestUserShape(DrawShape.Polygon, _polygonSymbol) as Polygon;
				
				// Requesting shape cancelled
				if (polygon == null)
					return;

                var result = await _mensurationTask.CentroidAsync(polygon, new MensurationPointParameters());
                ShowResults(result, ((Button)sender).Tag.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Mensuration Error");
            }
        }

        private async void DistanceAzimuthButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
				var line = await RequestUserShape(DrawShape.LineSegment, _lineSymbol) as Polyline;

				// Requesting shape cancelled
				if (line == null)
					return;

                var parameters = new MensurationLengthParameters()
                {
                    AngularUnit = comboAngularUnit.SelectedItem as AngularUnit,
                    LinearUnit = comboLinearUnit.SelectedItem as LinearUnit
                };

				var result = await _mensurationTask.DistanceAndAngleAsync(
					line.Parts.First().StartPoint,
					line.Parts.First().EndPoint, parameters);
                ShowResults(result, ((Button)sender).Tag.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Mensuration Error");
            }
        }

        private async void HeightBaseToTopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
				var line = await RequestUserShape(DrawShape.LineSegment, _lineSymbol) as Polyline;

				// Requesting shape cancelled
				if (line == null)
					return;

                var parameters = new MensurationHeightParameters()
                {
                    LinearUnit = comboLinearUnit.SelectedItem as LinearUnit
                };

				var result = await _mensurationTask.HeightFromBaseAndTopAsync(
					line.Parts.First().StartPoint, 
					line.Parts.First().EndPoint, parameters);


				ShowResults(result, ((Button)sender).Tag.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Mensuration Error");
            }
        }

        private async void HeightBaseToTopShadowButton_Click(object sender, RoutedEventArgs e)
        {
			try
			{
				var line = await RequestUserShape(DrawShape.LineSegment, _lineSymbol) as Polyline;
				
				// Requesting shape cancelled
				if (line == null)
					return;

				var parameters = new MensurationHeightParameters()
				{
					LinearUnit = comboLinearUnit.SelectedItem as LinearUnit
				};

				var result = await _mensurationTask.HeightFromBaseAndTopShadowAsync(
					line.Parts.First().StartPoint,
					line.Parts.First().EndPoint, parameters);

				ShowResults(result, ((Button)sender).Tag.ToString());
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Mensuration Error");
			}
        }

        private async void HeightTopToTopShadowButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
				var line = await RequestUserShape(DrawShape.LineSegment, _lineSymbol) as Polyline;

				// Requesting shape cancelled
				if (line == null)
					return;

                var parameters = new MensurationHeightParameters()
                {
                    LinearUnit = comboLinearUnit.SelectedItem as LinearUnit
                };

				var result = await _mensurationTask.HeightFromTopAndTopShadowAsync(
					line.Parts.First().StartPoint, 
					line.Parts.First().EndPoint, parameters);

                ShowResults(result, ((Button)sender).Tag.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Mensuration Error");
            }
        }

        private async void PointButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
				var point = await RequestUserShape(DrawShape.Point, _pointSymbol) as MapPoint;

				// Requesting shape cancelled
				if (point == null)
					return;

                var result = await _mensurationTask.PointAsync(point, new MensurationPointParameters());
                ShowResults(result, ((Button)sender).Tag.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Mensuration Error");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
			_graphicsOverlay.Graphics.Clear();
        }

        // Retrieve the given shape type from the user
        private async Task<Geometry> RequestUserShape(DrawShape drawShape, Symbol symbol)
        {
            try
            {
				_graphicsOverlay.Graphics.Clear();

                var shape = await MyMapView.Editor.RequestShapeAsync(drawShape, symbol);

				_graphicsOverlay.Graphics.Add(new Graphic(shape, symbol));
                return shape;
            }
			catch (TaskCanceledException) 
			{
				return null;
			}
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Shape Drawing Error");
                return null;
            }
        }

        // Show results from mensuration task in string format
        private void ShowResults(object result, string caption = "")
        {
            StringBuilder sb = new StringBuilder();

            if (result is MensurationPointResult)
            {
                MensurationPointResult pointResult = (MensurationPointResult)result;

                if (pointResult.Point != null)
                {
                    sb.Append(pointResult.Point);
                    sb.Append("\n");
                }
            }
            else if (result is MensurationHeightResult)
            {
                var heightResult = (MensurationHeightResult)result;

                if (heightResult.Height != null)
                {
                    sb.Append("Height\n");
                    sb.AppendFormat("Value:\t\t{0}\n", heightResult.Height.Value);
                    sb.AppendFormat("Display Value:\t{0}\n", heightResult.Height.DisplayValue);
                    sb.AppendFormat("Uncertainty:\t{0}\n", heightResult.Height.Uncertainty);
                    sb.AppendFormat("Unit:\t\t{0}\n", heightResult.Height.LinearUnit);
                    sb.Append("\n");
                }
            }
            else if (result is MensurationLengthResult)
            {
                var lengthResult = (MensurationLengthResult)result;

                if (lengthResult.Distance != null)
                {
                    sb.Append("Distance\n");
                    sb.AppendFormat("Value:\t\t{0}\n", lengthResult.Distance.Value);
                    sb.AppendFormat("Display Value:\t{0}\n", lengthResult.Distance.DisplayValue);
                    sb.AppendFormat("Uncertainty:\t{0}\n", lengthResult.Distance.Uncertainty);
                    sb.AppendFormat("Unit:\t\t{0}\n", lengthResult.Distance.LinearUnit);
                    sb.Append("\n");
                }
                if (lengthResult.AzimuthAngle != null)
                {
                    sb.Append("Azimuth Angle\n");
                    sb.AppendFormat("Value:\t\t{0}\n", lengthResult.AzimuthAngle.Value);
                    sb.AppendFormat("Display Value:\t{0}\n", lengthResult.AzimuthAngle.DisplayValue);
                    sb.AppendFormat("Uncertainty:\t{0}\n", lengthResult.AzimuthAngle.Uncertainty);
                    sb.AppendFormat("Unit:\t\t{0}\n", lengthResult.AzimuthAngle.AngularUnit);
                    sb.Append("\n");
                }
                if (lengthResult.ElevationAngle != null)
                {
                    sb.Append("Elevation Angle\n");
                    sb.AppendFormat("Value:\t\t{0}\n", lengthResult.ElevationAngle.Value);
                    sb.AppendFormat("Display Value:\t{0}\n", lengthResult.ElevationAngle.DisplayValue);
                    sb.AppendFormat("Uncertainty:\t{0}\n", lengthResult.ElevationAngle.Uncertainty);
                    sb.AppendFormat("Unit:\t\t{0}\n", lengthResult.ElevationAngle.AngularUnit);
                    sb.Append("\n");
                }
            }
            else if (result is MensurationAreaResult)
            {
                var areaResult = (MensurationAreaResult)result;

                if (areaResult.Area != null)
                {
                    sb.Append("Area\n");
                    sb.AppendFormat("Value:\t\t{0}\n", areaResult.Area.Value);
                    sb.AppendFormat("Display Value:\t{0}\n", areaResult.Area.DisplayValue);
                    sb.AppendFormat("Uncertainty:\t{0}\n", areaResult.Area.Uncertainty);
                    sb.AppendFormat("Unit:\t\t{0}\n", areaResult.Area.AreaUnit);
                    sb.Append("\n");
                }

                if (areaResult.Perimeter != null)
                {
                    sb.Append("Perimeter\n");
                    sb.AppendFormat("Value:\t\t{0}\n", areaResult.Perimeter.Value);
                    sb.AppendFormat("Display Value:\t{0}\n", areaResult.Perimeter.DisplayValue);
                    sb.AppendFormat("Uncertainty:\t{0}\n", areaResult.Perimeter.Uncertainty);
                    sb.AppendFormat("Unit:\t\t{0}\n", areaResult.Perimeter.LinearUnit);
                    sb.Append("\n");
                }
            }

            MessageBox.Show(sb.ToString(), caption);
        }
    }
}
