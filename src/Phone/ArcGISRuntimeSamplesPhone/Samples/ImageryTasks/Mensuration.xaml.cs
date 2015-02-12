using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Imagery;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Symbols = Esri.ArcGISRuntime.Symbology;


namespace ArcGISRuntime.Samples.Phone.Samples
{
    /// <summary>
    /// Demonstrates the use of the mensuration (measure) operations on an ArcGIS image service.
    /// </summary>
    /// <title>Mensuration</title>
    /// <category>Imagery Tasks</category>
    public partial class Mensuration : Page
    {
        private Symbols.Symbol _pointSymbol;
		private Symbols.Symbol _lineSymbol;
		private Symbols.Symbol _polygonSymbol;
		private GraphicsOverlay _graphicsOverlay;
        private MensurationTask _mensurationTask;

        /// <summary>Construct Mensuration sample control</summary>
        public Mensuration()
        {
            InitializeComponent();

			_pointSymbol = LayoutRoot.Resources["PointSymbol"] as Symbols.Symbol;
			_lineSymbol = LayoutRoot.Resources["LineSymbol"] as Symbols.Symbol;
			_polygonSymbol = LayoutRoot.Resources["PolygonSymbol"] as Symbols.Symbol;

			_graphicsOverlay = MyMapView.GraphicsOverlays.First();

            comboLinearUnit.ItemsSource = typeof(LinearUnits).GetTypeInfo().DeclaredProperties
				.Select(p => p.GetValue(null, null))
                .Except(new LinearUnit[] { LinearUnits.NauticalMiles } ).ToList();
            comboLinearUnit.SelectedItem = LinearUnits.Meters;

            comboAngularUnit.ItemsSource = new AngularUnit[] { AngularUnits.Degrees, AngularUnits.Radians };
            comboAngularUnit.SelectedItem = AngularUnits.Degrees;

            comboAreaUnit.ItemsSource = typeof(AreaUnits).GetTypeInfo().DeclaredProperties
				.Select(p => p.GetValue(null, null)).ToList();
            comboAreaUnit.SelectedItem = AreaUnits.SquareMeters;

			var imageLayer = MyMapView.Map.Layers["ImageLayer"] as ArcGISTiledMapServiceLayer;
            _mensurationTask = new MensurationTask(new Uri(imageLayer.ServiceUri));
        }

        private async void AreaPerimeterItem_Click(object sender, RoutedEventArgs e)
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
				ShowResults(result, ((MenuFlyoutItem)sender).Text);
            }
            catch (Exception ex)
            {
				var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        private async void CentroidItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
				var polygon = await RequestUserShape(DrawShape.Polygon, _polygonSymbol) as Polygon;

				// Requesting shape cancelled
				if (polygon == null)
					return;

				var result = await _mensurationTask.CentroidAsync(polygon, new MensurationPointParameters());
				ShowResults(result, ((MenuFlyoutItem)sender).Text);
            }
            catch (Exception ex)
            {
				var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        private async void DistanceAzimuthItem_Click(object sender, RoutedEventArgs e)
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
				ShowResults(result, ((MenuFlyoutItem)sender).Text);
            }
            catch (Exception ex)
            {
				var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        private async void HeightBaseToTopItem_Click(object sender, RoutedEventArgs e)
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
				ShowResults(result, ((MenuFlyoutItem)sender).Text);
            }
            catch (Exception ex)
            {
				var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        private async void HeightBaseToTopShadowItem_Click(object sender, RoutedEventArgs e)
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
				ShowResults(result, ((MenuFlyoutItem)sender).Text);
            }
            catch (Exception ex)
            {
				var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        private async void HeightTopToTopShadowItem_Click(object sender, RoutedEventArgs e)
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
				ShowResults(result, ((MenuFlyoutItem)sender).Text);
            }
            catch (Exception ex)
            {
				var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        private async void PointItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
				var point = await RequestUserShape(DrawShape.Point, _pointSymbol) as MapPoint;

				// Requesting shape cancelled
				if (point == null)
					return;

				var result = await _mensurationTask.PointAsync(point, new MensurationPointParameters());
				ShowResults(result, ((MenuFlyoutItem)sender).Text);
            }
            catch (Exception ex)
            {
				var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        private void ClearItem_Click(object sender, RoutedEventArgs e)
        {
			_graphicsOverlay.Graphics.Clear();
        }

        // Retrieve the given shape type from the user
		private async Task<Geometry> RequestUserShape(DrawShape drawShape, Symbols.Symbol symbol)
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
				var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
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

			var _ = new MessageDialog(sb.ToString(), caption).ShowAsync();
        }
	}
}
