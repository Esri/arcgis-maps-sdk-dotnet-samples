using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// This sample demonstrates drawing and editing map graphics in a scene.
	/// </summary>
	/// <title>Draw and Edit Graphics</title>
	/// <category>Scene</category>
	/// <subcategory>Editing</subcategory>
	public partial class DrawAndEditGraphics3d : UserControl
	{
		Graphic _editGraphic = null;

		public DrawAndEditGraphics3d()
		{
			InitializeComponent();

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

			var editCnfg = MySceneView.Editor.EditorConfiguration;
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
				graphicsOverlay = drawShape == DrawShape.Point ? MySceneView.GraphicsOverlays["PointGraphicsOverlay"] as GraphicsOverlay :
						   ((drawShape == DrawShape.Polyline || drawShape == DrawShape.Freehand || drawShape == DrawShape.LineSegment) ?
				  MySceneView.GraphicsOverlays["PolylineGraphicsOverlay"] as GraphicsOverlay : MySceneView.GraphicsOverlays["PolygonGraphicsOverlay"] as GraphicsOverlay);

				var progress = new Progress<GeometryEditStatus>();

				var content = (sender as Button).Content.ToString();
				switch (content)
				{
					case "Draw":
						{
							var r = await MySceneView.Editor.RequestShapeAsync(drawShape, null, progress);
							graphicsOverlay.Graphics.Add(new Graphic() { Geometry = r });
							break;
						}
					case "Edit":
						{
							if (_editGraphic == null)
								return;
							var g = _editGraphic;
							g.IsVisible = false;
							var r = await MySceneView.Editor.EditGeometryAsync(g.Geometry, null, progress);
							resultGeometry = r ?? resultGeometry;
							_editGraphic.Geometry = resultGeometry;
							_editGraphic.IsSelected = false;
							break;
						}
				}

			}
			catch (TaskCanceledException)
			{
				// Ignore TaskCanceledException - usually happens if the editor gets cancelled or restarted
			}
			catch (Exception ex)
			{

				message = ex.Message;
			}
			finally
			{
				if (_editGraphic != null)
				{
					_editGraphic.IsVisible = true;
					_editGraphic = null;
				}
			}
			if (message != null)
				MessageBox.Show(message);
		}

		private async void MySceneView_SceneViewTapped(object sender, MapViewInputEventArgs e)
		{
			if (MySceneView.Editor.IsActive)
				return;

			var drawShape = (DrawShape)DrawShapes.SelectedItem;
			GraphicsOverlay graphicsOverlay;
			graphicsOverlay = drawShape == DrawShape.Point ? MySceneView.GraphicsOverlays["PointGraphicsOverlay"] as GraphicsOverlay :
					   ((drawShape == DrawShape.Polyline || drawShape == DrawShape.Freehand || drawShape == DrawShape.LineSegment) ?
			  MySceneView.GraphicsOverlays["PolylineGraphicsOverlay"] as GraphicsOverlay : MySceneView.GraphicsOverlays["PolygonGraphicsOverlay"] as GraphicsOverlay);


			var graphic = await graphicsOverlay.HitTestAsync(MySceneView, e.Position);

			if (graphic != null)
			{
				//Clear previous selection
				foreach (GraphicsOverlay gOLay in MySceneView.GraphicsOverlays)
				{
					gOLay.ClearSelection();
				}

				//Cancel editing if started
				if (MySceneView.Editor.Cancel.CanExecute(null))
					MySceneView.Editor.Cancel.Execute(null);

				_editGraphic = graphic;
				_editGraphic.IsSelected = true;
			}
		}
		
	}
}
