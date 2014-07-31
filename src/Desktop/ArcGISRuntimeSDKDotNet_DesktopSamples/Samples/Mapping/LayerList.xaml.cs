using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Shows how to access layers in the map.
    /// </summary>
    /// <title>Layer List</title>
    /// <category>Mapping</category>
    public sealed partial class LayerList : UserControl
    {
        private Point _startPoint;

        public LayerList()
        {
            this.InitializeComponent();

			mapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-13279586, 4010136, -12786147, 4280850, SpatialReferences.WebMercator));
        }

        private void RemoveLayerButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = (sender as FrameworkElement).DataContext as Layer;
            mapView.Map.Layers.Remove(layer);
        }

        private void legend_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void legend_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.OriginalSource is Thumb)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    var layer = ((FrameworkElement)e.OriginalSource).DataContext as Layer;
                    if (layer == null)
                        return;

                    DataObject data = new DataObject("legendLayerFormat", layer);
                    DragDropEffects de = DragDrop.DoDragDrop(legend, data, DragDropEffects.Move);
                }
            }
        }

        private void legend_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("legendLayerFormat") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void legend_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("legendLayerFormat"))
            {
                Layer moveLayer = e.Data.GetData("legendLayerFormat") as Layer;

                var lvItem = legend.ContainerFromElement((FrameworkElement)e.OriginalSource) as ListViewItem;
                if (lvItem != null)
                {
                    Layer replaceLayer = lvItem.DataContext as Layer;
                    if (replaceLayer != null)
                    {
                        int index = mapView.Map.Layers.IndexOf(replaceLayer);
                        if (index >= 0)
                        {
                            mapView.Map.Layers.Remove(moveLayer);
                            mapView.Map.Layers.Insert(index, moveLayer);
                        }
                        else
                        {
                            mapView.Map.Layers.Remove(moveLayer);
                            mapView.Map.Layers.Add(moveLayer);
                        }
                    }
                }
            }
        }
    }
}
