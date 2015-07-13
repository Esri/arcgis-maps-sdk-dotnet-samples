using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrographic;
using Esri.ArcGISRuntime.Layers;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop.Symbology.Hydrographic
{
	/// <summary>
	/// This sample demonstrates adding and removing S-57 cells from an HydrographicS57Layer in the Map and accessing the Dataset Identification (DSID) metadata of those S-57 cells. The DSID metadata Provides information regarding the dataset's source and the product specification it is part of.
	/// </summary>
	/// <title>S57 Cell Information</title>
	/// <category>Symbology</category>
	/// <subcategory>Hydrographic</subcategory>
	/// <requiresSymbols>true</requiresSymbols>
	public partial class S57CellInfoSample : UserControl 
	{
		GroupLayer _hydrographicGroupLayer;

		public S57CellInfoSample()
		{
			InitializeComponent();

			// Create default instance of display properties and set that to DataContext for binding
			ZoomToHydrographicLayers();
		}

		// Zoom to combined extent of the group layer that contains all hydrographic layers
		private async void ZoomToHydrographicLayers()
		{
			try
			{
				// wait until all layers are loaded
				await MyMapView.LayersLoadedAsync();
				
				// Get group layer from Map and set list items source
				_hydrographicGroupLayer = MyMapView.Map.Layers.OfType<GroupLayer>().First();
				s57CellList.ItemsSource = _hydrographicGroupLayer.ChildLayers;
				s57CellList.SelectedIndex = 0;

				Envelope extent = _hydrographicGroupLayer.ChildLayers.First().FullExtent;

				// Create combined extent from child hydrographic layers
				foreach (var layer in _hydrographicGroupLayer.ChildLayers)
					extent = extent.Union(layer.FullExtent);

				// Zoom to full extent
				await MyMapView.SetViewAsync(extent);

				// Enable controls
				addCellButton.IsEnabled = true;
				zoomToSelectedButton.IsEnabled = true;
				removeSelectedCellsButton.IsEnabled = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error occurred : " + ex.Message, "Sample error");
			}
		}

		// Show error if loading layers fail
		private void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
		{
			if (e.LoadError == null)
				return;

			MessageBox.Show(
					string.Format("Error when loading layer. {0}", e.LoadError.ToString()),
					"Error loading layer");
		}

		private void AddCellButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			// Create an instance of the open file dialog box.
			OpenFileDialog openFileDialog1 = new OpenFileDialog();

			// Set filter options and filter index.
			openFileDialog1.DefaultExt = ".000";
			openFileDialog1.Filter = "S57 Cells (.000)|*.000";
			openFileDialog1.Multiselect = true;

			// Call the ShowDialog method to show the dialog box.
			bool? userClickedOK = openFileDialog1.ShowDialog();

			// Process input if the user clicked OK.
			if (userClickedOK == true)
			{
				foreach (string fileName in openFileDialog1.FileNames)
				{
					var hydroLayer = new HydrographicS57Layer() 
					{ 
						Path = fileName,
						ID = Path.GetFileNameWithoutExtension(fileName)
					};
					_hydrographicGroupLayer.ChildLayers.Add(hydroLayer);
					s57CellList.SelectedItem = hydroLayer;
				}
			}
		}

		private async void ZoomToSelectedButtom_Click(object sender, RoutedEventArgs e)
		{
			var selectedLayer = s57CellList.SelectedItem as HydrographicS57Layer;
			if (selectedLayer == null)
				return;

			ZoomToCell(await selectedLayer.GetCellAsync(MyMapView));
		}

		private void ZoomToCell(S57Cell currentCell)
		{
			if (currentCell == null)
				return;

			if (currentCell.Extent != null)
			{
				MyMapView.SetView(currentCell.Extent);
			}
		}

		private async void s57CellList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selectedLayer = s57CellList.SelectedItem as HydrographicS57Layer;
			if (selectedLayer == null)
				return;

			cellInfoDisplay.DataContext = await selectedLayer.GetCellAsync(MyMapView);
		}

		private void RemoveSelectedCellsButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var selectedLayer = s57CellList.SelectedItem as HydrographicS57Layer;
			if (selectedLayer == null)
				return;

			_hydrographicGroupLayer.ChildLayers.Remove(selectedLayer);
			cellInfoDisplay.DataContext = null;
		}
	}
}
