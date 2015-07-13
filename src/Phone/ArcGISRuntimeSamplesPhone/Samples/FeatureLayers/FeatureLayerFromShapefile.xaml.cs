using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// Demonstrates adding a Feature Layer from a local shapefile to a map.
	/// </summary>
	/// <title>Feature Layer from Shapefile</title>
	/// <category>Feature Layers</category>
	/// <localdata>true</localdata>
	public sealed partial class FeatureLayerFromShapefile : Page
	{
		private StorageFolder _folder;
		private FrameworkElement _featureOverlay;

		public FeatureLayerFromShapefile()
		{
			InitializeComponent();
			_featureOverlay = MyMapView.Overlays.Items[0] as FrameworkElement;
			MyMapView.MapViewTapped += MyMapView_MapViewTapped;

			Initialize();
		}

		private async void Initialize()
		{
			try
			{
				// Loop through the files in the samples-data shapefile folder and add them to the combobox.
				_folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("shapefiles");

				var files = await _folder.GetFilesAsync();

				foreach (var shpfile in files)
				{
					if (shpfile.FileType == ".shp")
					{
						FileListCombo.Items.Add(shpfile);
						FileListCombo.Visibility = (files.Any()) ? Visibility.Visible : Visibility.Collapsed;
					}
				}

			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}

		private async void listFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				var shpfile = e.AddedItems.FirstOrDefault() as StorageFile;
				if (shpfile != null)
				{
					var basename = Path.GetFileNameWithoutExtension(shpfile.Name);
					var files = await _folder.GetFilesAsync();
					foreach (var file in files)
					{
						if (file.Name.StartsWith(basename + ".", StringComparison.CurrentCultureIgnoreCase))
						{
							await file.CopyAsync(ApplicationData.Current.LocalFolder,
								file.Name, NameCollisionOption.ReplaceExisting);
						}
					}

					var shapefile = Path.Combine(ApplicationData.Current.LocalFolder.Path, shpfile.Name);
					await LoadShapefile(shapefile);
				}
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}

		private async Task LoadShapefile(string path)
		{
			try
			{
				_featureOverlay.Visibility = Visibility.Collapsed;

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
					DisplayName = Path.GetFileName(path),
				};

				// Add the feature layer to the map
				MyMapView.Map.Layers.Add(flayer);
				ShapefileInfo.DataContext = flayer;
				ShapefileInfo.Visibility = Visibility.Visible;
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
				ShapefileInfo.Visibility = Visibility.Collapsed;
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
					_featureOverlay.DataContext = features.First()
						.Attributes.Select(kvp => new Tuple<string, object>(kvp.Key, kvp.Value));
					MapView.SetViewOverlayAnchor(_featureOverlay, e.Location);
				}
				else
					_featureOverlay.DataContext = null;
			}
			catch (Exception ex)
			{
				_featureOverlay.DataContext = null;
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
			finally
			{
				_featureOverlay.Visibility = (_featureOverlay.DataContext != null) ? Visibility.Visible : Visibility.Collapsed;
			}
		}
	}
}
