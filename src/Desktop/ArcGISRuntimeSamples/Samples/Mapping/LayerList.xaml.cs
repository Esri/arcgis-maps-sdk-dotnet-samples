using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Shows how to access layers in the map.
    /// </summary>
    /// <title>Layer List</title>
    /// <category>Mapping</category>
    public sealed partial class LayerList : UserControl, INotifyPropertyChanged
    {
        private Point _startPoint;

		public IEnumerable<Layer> LegendLayers
		{
			get { return MyMapView.Map.Layers.Reverse(); }
		}
		
        public LayerList()
        {
            this.InitializeComponent();
			DataContext = this;
		}

        private void RemoveLayerButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = (sender as FrameworkElement).DataContext as Layer;
            MyMapView.Map.Layers.Remove(layer);
			OnPropertyChanged("LegendLayers");
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
                        int index = MyMapView.Map.Layers.IndexOf(replaceLayer);
                        if (index >= 0)
                        {
                            MyMapView.Map.Layers.Remove(moveLayer);
                            MyMapView.Map.Layers.Insert(index, moveLayer);
                        }
                        else
                        {
                            MyMapView.Map.Layers.Remove(moveLayer);
                            MyMapView.Map.Layers.Add(moveLayer);
                        }

						OnPropertyChanged("LegendLayers");
                    }
                }
            }
        }

		public event PropertyChangedEventHandler PropertyChanged = delegate { };
		private void OnPropertyChanged([CallerMemberName] string name = null)
		{
			PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
