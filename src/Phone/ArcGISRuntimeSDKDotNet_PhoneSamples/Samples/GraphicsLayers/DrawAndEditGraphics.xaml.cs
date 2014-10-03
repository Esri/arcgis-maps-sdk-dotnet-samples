using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
    /// <category>Graphics Layers</category>
	public sealed partial class DrawAndEditGraphics : Page
    {
        Graphic _editGraphic = null;

		public DrawAndEditGraphics()
        {
            this.InitializeComponent();
            DrawShapes.ItemsSource = new DrawShape[]
            {
                DrawShape.Freehand, DrawShape.Point, DrawShape.Polygon, DrawShape.Polyline, DrawShape.Arrow, DrawShape.Circle, DrawShape.Ellipse,DrawShape.LineSegment,DrawShape.Rectangle
            };
            DrawShapes.SelectedIndex = 0;
        }

        private async void OnDrawButtonClicked(object sender, RoutedEventArgs e)
        {
            string message = null;
            var resultGeometry = _editGraphic == null ? null : _editGraphic.Geometry;
            var editCnfg = mapView1.Editor.EditorConfiguration;
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
                graphicsOverlay = drawShape == DrawShape.Point ? mapView1.GraphicsOverlays["PointGraphicsOverlay"] as GraphicsOverlay :
                           ((drawShape == DrawShape.Polyline || drawShape == DrawShape.Freehand || drawShape == DrawShape.LineSegment) ?
                  mapView1.GraphicsOverlays["PolylineGraphicsOverlay"] as GraphicsOverlay : mapView1.GraphicsOverlays["PolygonGraphicsOverlay"] as GraphicsOverlay);

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
                            var r = await mapView1.Editor.RequestShapeAsync(drawShape, null, progress);
                            graphicsOverlay.Graphics.Add(new Graphic() { Geometry = r });
                            break;
                        }
                    case "Edit":
                        {
                            if (_editGraphic == null)
                                return;
                            var g = _editGraphic;
                            g.IsVisible = false;
                            var r = await mapView1.Editor.EditGeometryAsync(g.Geometry, null, progress);
                            resultGeometry = r ?? resultGeometry;
                            _editGraphic.Geometry = resultGeometry;
                            _editGraphic.IsSelected = false;
                            _editGraphic.IsVisible = true;
                            _editGraphic = null;
                            break;
                        }
                }

            }
            catch (Exception ex)
            {

                message = ex.Message;
                if (_editGraphic != null)
                {
                    _editGraphic.Geometry = resultGeometry;
                    _editGraphic.IsVisible = true;
                }
            }
            if (message != null)
                await new MessageDialog(message).ShowAsync();

        }

        private async void mapView1_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            var drawShape = (DrawShape)DrawShapes.SelectedItem;
            GraphicsOverlay graphicsOverlay;
            graphicsOverlay = drawShape == DrawShape.Point ? mapView1.GraphicsOverlays["PointGraphicsOverlay"] as GraphicsOverlay :
                       ((drawShape == DrawShape.Polyline || drawShape == DrawShape.Freehand || drawShape == DrawShape.LineSegment) ?
              mapView1.GraphicsOverlays["PolylineGraphicsOverlay"] as GraphicsOverlay : mapView1.GraphicsOverlays["PolygonGraphicsOverlay"] as GraphicsOverlay);

            var graphic = await graphicsOverlay.HitTestAsync(mapView1, e.Position);

            if (graphic != null)
            {
                //Clear prevoius selection
                foreach (GraphicsOverlay gOLay in mapView1.GraphicsOverlays)
                    gOLay.ClearSelection();

                //Cancel editing if started
                if (mapView1.Editor.Cancel.CanExecute(null))
                    mapView1.Editor.Cancel.Execute(null);

                _editGraphic = graphic;
                _editGraphic.IsSelected = true;
            }
        }



    }
}
