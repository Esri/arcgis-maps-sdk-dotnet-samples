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
	/// Demonstrates drawing and editing map graphics.
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

                GraphicsLayer graphicsLayer;
                graphicsLayer = drawShape == DrawShape.Point ? mapView1.Map.Layers["PointGraphicsLayer"] as GraphicsLayer :
                   ((drawShape == DrawShape.Polyline || drawShape == DrawShape.Freehand) ? mapView1.Map.Layers["PolylineGraphicsLayer"] as GraphicsLayer : mapView1.Map.Layers["PolygonGraphicsLayer"] as GraphicsLayer);

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
                            graphicsLayer.Graphics.Add(new Graphic() { Geometry = r });
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
            GraphicsLayer graphicsLayer;
            graphicsLayer = drawShape == DrawShape.Point ? mapView1.Map.Layers["PointGraphicsLayer"] as GraphicsLayer :
               ((drawShape == DrawShape.Polyline || drawShape == DrawShape.Freehand) ? mapView1.Map.Layers["PolylineGraphicsLayer"] as GraphicsLayer : mapView1.Map.Layers["PolygonGraphicsLayer"] as GraphicsLayer);

            var graphic = await graphicsLayer.HitTestAsync(mapView1, e.Position);
            if (graphic != null)
            {
                _editGraphic = graphic;
                _editGraphic.IsSelected = true;
            }
        }



    }
}
