using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using sym = Esri.ArcGISRuntime.Symbology;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
	/// Example of how to add Graphics to a GraphicLayer by drawing shapes on the map.
	/// </summary>
	/// <title>Add Graphics Interactively</title>
	/// <category>Graphics Layers</category>
	public sealed partial class AddInteractively : Windows.UI.Xaml.Controls.Page
	{
		private GraphicsLayer _graphicsLayer;

		/// <summary>Construct Add Graphics Interactively sample control</summary>
		public AddInteractively()
		{
			InitializeComponent();

			_graphicsLayer = MyMapView.Map.Layers["GraphicsLayer"] as GraphicsLayer;

			comboDrawShape.DisplayMemberPath = "Item1";
			comboDrawShape.SelectedValuePath = "Item2";
			comboDrawShape.ItemsSource = Enum.GetValues(typeof(DrawShape)).Cast<int>()
				.Select(n => new Tuple<string, int>(Enum.GetName(typeof(DrawShape), n), n));
			comboDrawShape.SelectedIndex = 0;
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			AddGraphicsAsync();
		}

		// Draw graphics infinitely
		private async void AddGraphicsAsync()
		{
			await MyMapView.LayersLoadedAsync();

			while (true)
			{
				// if the map is not in a valid state
				if (MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry.Extent == null)
					break;

				await AddSingleGraphicAsync();
			}
		}

		// Draw and add a single graphic to the graphic layer
		private async Task AddSingleGraphicAsync()
		{
			try
			{
				var drawShape = (DrawShape)comboDrawShape.SelectedValue;

				sym.Symbol symbol = null;
				switch (drawShape)
				{
					case DrawShape.Point:
						symbol = Resources["BluePointSymbol"] as sym.Symbol;
						break;

					case DrawShape.LineSegment:
					case DrawShape.Freehand:
					case DrawShape.Polyline:
						symbol = Resources["GreenLineSymbol"] as sym.Symbol;
						break;

					case DrawShape.Arrow:
					case DrawShape.Circle:
					case DrawShape.Ellipse:
					case DrawShape.Polygon:
					case DrawShape.Rectangle:
					case DrawShape.Triangle:
					case DrawShape.Envelope:
						symbol = Resources["RedFillSymbol"] as sym.Symbol;
						break;
				}

				// wait for user to draw the shape
				var geometry = await MyMapView.Editor.RequestShapeAsync(drawShape, symbol);

				// add the new graphic to the graphic layer
				var graphic = new Graphic(geometry, symbol);
				_graphicsLayer.Graphics.Add(graphic);
			}
			catch (TaskCanceledException)
			{
				// Ignore cancellations from selecting new shape type
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog("Error drawing graphic: " + ex.Message, "Add Graphic Interactively").ShowAsync();
			}
		}

		// Cancel the current shape drawing (if in Editor.RequestShapeAsync) when the shape type has changed
		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (MyMapView.Editor.IsActive)
				MyMapView.Editor.Cancel.Execute(null);
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			_graphicsLayer.Graphics.Clear();
		}
	}
}
