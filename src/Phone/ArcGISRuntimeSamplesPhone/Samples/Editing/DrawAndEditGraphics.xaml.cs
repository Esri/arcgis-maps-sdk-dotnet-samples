using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates drawing and editing map graphics.
	/// </summary>
	/// <title>Draw and Edit Graphics</title>
	/// <category>Editing</category>
	public sealed partial class DrawAndEditGraphics : Page
	{
		Graphic _editGraphic = null;

		public DrawAndEditGraphics()
		{
			this.InitializeComponent();
			DrawShapes.ItemsSource = new DrawShape[]
			{
				DrawShape.Freehand,
				DrawShape.Point,
				DrawShape.Polygon,
				DrawShape.Polyline,
				DrawShape.Arrow,
				DrawShape.Circle,
				DrawShape.Ellipse,
				DrawShape.LineSegment,
				DrawShape.Rectangle
			};
			DrawShapes.SelectedIndex = 0;
		}

		private async void OnDrawButtonClicked(object sender, RoutedEventArgs e)
		{
			string message = null;
			var resultGeometry = _editGraphic == null ? null : _editGraphic.Geometry;

			var editCnfg = MyMapView.Editor.EditorConfiguration;
			editCnfg.AllowAddVertex = AddVertex.IsChecked.HasValue && AddVertex.IsChecked.Value;
			editCnfg.AllowDeleteVertex = DeleteVertex.IsChecked.HasValue && DeleteVertex.IsChecked.Value;
			editCnfg.AllowMoveGeometry = MoveGeometry.IsChecked.HasValue && MoveGeometry.IsChecked.Value;
			editCnfg.AllowMoveVertex = MoveVertex.IsChecked.HasValue && MoveVertex.IsChecked.Value;
			editCnfg.AllowRotateGeometry = Rotate.IsChecked.HasValue && Rotate.IsChecked.Value;
			editCnfg.AllowScaleGeometry = Scale.IsChecked.HasValue && Scale.IsChecked.Value;
			editCnfg.MaintainAspectRatio = MaintainAspectRatio.IsChecked.HasValue && MaintainAspectRatio.IsChecked.Value;
			editCnfg.VertexSymbol =
					new SimpleMarkerSymbol() { Style = SimpleMarkerStyle.Diamond, Color = Colors.Yellow, Size = 15 };

			try
			{
				var drawShape = (DrawShape)DrawShapes.SelectedItem;

				GraphicsOverlay graphicsOverlay;
				graphicsOverlay = drawShape == DrawShape.Point ? MyMapView.GraphicsOverlays["PointGraphicsOverlay"] as GraphicsOverlay :
						   ((drawShape == DrawShape.Polyline || drawShape == DrawShape.Freehand || drawShape == DrawShape.LineSegment) ?
				  MyMapView.GraphicsOverlays["PolylineGraphicsOverlay"] as GraphicsOverlay : MyMapView.GraphicsOverlays["PolygonGraphicsOverlay"] as GraphicsOverlay);

				var progress = new Progress<GeometryEditStatus>();
				progress.ProgressChanged += (a, b) =>
				{
					//if (b.GeometryEditAction == GeometryEditAction..CompletedEdit)
					//    if (_editGraphic != null)
					//        _editGraphic.IsSelected = false;

				};

				var content = (sender as Button).Content.ToString();
				switch (content)
				{
					case "Draw":
						{
							var r = await MyMapView.Editor.RequestShapeAsync(drawShape, null, progress);
							graphicsOverlay.Graphics.Add(new Graphic() { Geometry = r });
							break;
						}
					case "Edit":
						{
							if (_editGraphic == null)
								return;
							var g = _editGraphic;
							g.IsVisible = false;
							var r = await MyMapView.Editor.EditGeometryAsync(g.Geometry, null, progress);
							resultGeometry = r ?? resultGeometry;
							_editGraphic.Geometry = resultGeometry;
							_editGraphic.IsSelected = false;
							_editGraphic.IsVisible = true;
							_editGraphic = null;
							break;
						}
				}
			}
			catch (TaskCanceledException tce)
			{
				// Ignore TaskCanceledException
			}
			catch (Exception ex)
			{
				message = ex.Message;

      }
      finally
      {
        if (_editGraphic != null)
        {
          _editGraphic.Geometry = resultGeometry;
          _editGraphic.IsVisible = true;
        }
      }
			if (message != null)
				await new MessageDialog(message).ShowAsync();
		}

		private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
		{
			if (MyMapView.Editor.IsActive)
				return;

			var drawShape = (DrawShape)DrawShapes.SelectedItem;
			GraphicsOverlay graphicsOverlay;
			graphicsOverlay = drawShape == DrawShape.Point ? MyMapView.GraphicsOverlays["PointGraphicsOverlay"] as GraphicsOverlay :
					   ((drawShape == DrawShape.Polyline || drawShape == DrawShape.Freehand || drawShape == DrawShape.LineSegment) ?
			  MyMapView.GraphicsOverlays["PolylineGraphicsOverlay"] as GraphicsOverlay : MyMapView.GraphicsOverlays["PolygonGraphicsOverlay"] as GraphicsOverlay);


			var graphic = await graphicsOverlay.HitTestAsync(MyMapView, e.Position);

			if (graphic != null)
			{
				//Clear previous selection
				foreach (GraphicsOverlay gOLay in MyMapView.GraphicsOverlays)
				{
					gOLay.ClearSelection();
				}

				//Cancel editing if started
				if (MyMapView.Editor.Cancel.CanExecute(null))
					MyMapView.Editor.Cancel.Execute(null);

				_editGraphic = graphic;
				_editGraphic.IsSelected = true;
				EditButton.IsEnabled = true;
			}
		}
	}
}
