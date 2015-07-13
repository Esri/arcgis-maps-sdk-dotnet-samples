using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample shows how to add a FeatureLayer from a local shapefile to the map.
    /// </summary>
    /// <title>Feature Layer from Shapefile</title>
	/// <category>Layers</category>
	/// <subcategory>Feature Layers</subcategory>
	public partial class FeatureLayerFromShapefile : UserControl
    {
		/// <summary>Construct FeatureLayerFromLocalGeodatabase sample control</summary>
        public FeatureLayerFromShapefile()
        {
            InitializeComponent();

            MyMapView.AllowDrop = true;
            MyMapView.DragOver += MyMapView_DragOver;
            MyMapView.Drop += MyMapView_Drop;
			MyMapView.MapViewTapped += MyMapView_MapViewTapped;
        }

        private async void MyMapView_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var f in files.Where(f => f.EndsWith(".shp")))
                    await LoadShapefile(f);
            }
        }

        private void MyMapView_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Any(f => f.EndsWith(".shp")))
                    return;
            }

            e.Handled = true;
        }

		private async void BrowseButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var openFileDialog = new OpenFileDialog();
				openFileDialog.Filter = "Shapefiles (*.shp)|*.shp";
				openFileDialog.Multiselect = false;
				openFileDialog.Title = "Select Shapefile";
				if (openFileDialog.ShowDialog() == true)
				{
					await LoadShapefile(openFileDialog.FileName);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Sample Error");
			}
		}
		
		private async Task LoadShapefile(string path)
        {
            try
            {
				FeatureOverlay.Visibility = Visibility.Collapsed;

				// open shapefile table
				var shapefile = await ShapefileTable.OpenAsync(path);

				// clear existing map and spatial reference
				if (MyMapView.Map.Layers.Any())
				{
					MyMapView.Map.Layers.Clear();
					MyMapView.Map = new Map();
				}

				// create feature layer based on the shapefile
                var flayer = new FeatureLayer(shapefile)
                {
                    ID = shapefile.Name,
                    DisplayName = path,
                };

				// Add the feature layer to the map
				MyMapView.Map.Layers.Add(flayer);
				txtInfo.DataContext = flayer;
				txtInfo.Visibility = Visibility.Visible;
			}
            catch (Exception ex)
            {
                MessageBox.Show("Error creating feature layer: " + ex.Message, "Sample Error");
				txtInfo.Visibility = Visibility.Collapsed;
			}
        }

		private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
		{
			try
			{
				var flayer = MyMapView.Map.Layers.OfType<FeatureLayer>().FirstOrDefault();
				if (flayer == null)
					return;

				var rows = await flayer.HitTestAsync(MyMapView, e.Position);
				if (rows != null && rows.Length > 0)
				{
					var features = await flayer.FeatureTable.QueryAsync(rows);
					FeatureOverlay.DataContext = features.FirstOrDefault();
					MapView.SetViewOverlayAnchor(FeatureOverlay, e.Location);
				}
				else
					FeatureOverlay.DataContext = null;
			}
			catch (Exception ex)
			{
				FeatureOverlay.DataContext = null;
				MessageBox.Show("HitTest Error: " + ex.Message, "Sample Error");
			}
			finally
			{
				FeatureOverlay.Visibility = (FeatureOverlay.DataContext != null) ? Visibility.Visible : Visibility.Collapsed;
			}
		}
	}
}
