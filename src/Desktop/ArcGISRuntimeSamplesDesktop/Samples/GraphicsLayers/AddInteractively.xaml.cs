using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Example of how to add Graphics to a GraphicsOverlay by drawing shapes on the map.  The Editor.RequestShapeAsync method is used to manage map drawing and geometry creation.  Symbols for the graphics are defined in XAML.
	/// </summary>
	/// <title>Add Graphics Interactively</title>
	/// <category>Layers</category>
	/// <subcategory>Graphics Layers</subcategory>
	public partial class AddInteractively : UserControl, INotifyPropertyChanged
	{
		/// <summary>INotifyPropertyChanged notification</summary>
		public event PropertyChangedEventHandler PropertyChanged;

		private GraphicsLayer _graphicsLayer;

		private bool _inDrawMode;
		/// <summary>Is In Drawing Mode</summary>
		public bool InDrawMode
		{
			get { return _inDrawMode; }
			set
			{
				_inDrawMode = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("InDrawMode"));
			}
		}

		private DrawShape _currentDrawShape;
		/// <summary>Currently selected shape type</summary>
		public DrawShape CurrentDrawShape
		{
			get { return _currentDrawShape; }
			set
			{
				_currentDrawShape = value;
				if (PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("CurrentDrawShape"));
			}
		}

		/// <summary>Construct Add Graphics Interactively sample control</summary>
		public AddInteractively()
		{
			InitializeComponent();

			DataContext = this;
			_currentDrawShape = DrawShape.Point;
			_graphicsLayer = MyMapView.Map.Layers["graphicsLayer"] as GraphicsLayer;
			_inDrawMode = false;
		}

		// Draw graphics infinitely
		private async Task AddGraphicsAsync()
		{
			await MyMapView.LayersLoadedAsync();

			while (InDrawMode)
			{
				// if the map is not in a valid state - quit and turn drawing mode off
				if (MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry.Extent == null)
				{
					InDrawMode = false;
					break;
				}

				await AddSingleGraphicAsync();
			}
		}

		// Draw and add a single graphic to the graphic layer
		private async Task AddSingleGraphicAsync()
		{
			try
			{
				Symbol symbol = null;
				switch (CurrentDrawShape)
				{
					case DrawShape.Point:
						symbol = LayoutGrid.Resources["BluePointSymbol"] as Symbol;
						break;

					case DrawShape.LineSegment:
					case DrawShape.Freehand:
					case DrawShape.Polyline:
						symbol = LayoutGrid.Resources["GreenLineSymbol"] as Symbol;
						break;

					case DrawShape.Arrow:
					case DrawShape.Circle:
					case DrawShape.Ellipse:
					case DrawShape.Polygon:
					case DrawShape.Rectangle:
					case DrawShape.Triangle:
					case DrawShape.Envelope:
						symbol = LayoutGrid.Resources["RedFillSymbol"] as Symbol;
						break;
				}

				// wait for user to draw the shape
				var geometry = await MyMapView.Editor.RequestShapeAsync(CurrentDrawShape, symbol);

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
				MessageBox.Show("Error drawing graphic: " + ex.Message, "Add Graphic Interactively");
			}
		}

		// Cancel the current shape drawing (if in Editor.RequestShapeAsync) when the shape type has changed
		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (MyMapView.Editor.IsActive)
				MyMapView.Editor.Cancel.Execute(null);
		}

		// Cancel the current shape drawing (if in Editor.RequestShapeAsync)
		//  and initiate new graphic adding if drawing mode is on
		private void ToggleButton_Click(object sender, RoutedEventArgs e)
		{
			if (MyMapView.Editor.IsActive)
				MyMapView.Editor.Cancel.Execute(null);

			if (InDrawMode)
			{
				var _x = AddGraphicsAsync();
			}
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			_graphicsLayer.Graphics.Clear();
		}
	}
}
