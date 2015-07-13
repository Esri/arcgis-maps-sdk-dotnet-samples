using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using Esri.ArcGISRuntime.Tasks.Offline;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates how to download a local tile cache from an online service with the ExportTiles operation enabled.
	/// </summary>
	/// <title>Export Tile Cache</title>
	/// <category>Offline</category>
	public partial class ExportTileCache : Windows.UI.Xaml.Controls.Page
	{
		//private const string ONLINE_BASEMAP_URL = "https://tiledbasemaps.arcgis.com/arcgis/rest/services/World_Street_Map/MapServer";
		private const string ONLINE_BASEMAP_TOKEN_URL = ""; //"https://www.arcgis.com/sharing/rest/generatetoken";
		private const string USERNAME = "<Organizational Account UserName>";
		private const string PASSWORD = "<Organizational Account Password>";

		private const string ONLINE_BASEMAP_URL = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer";
		private const string ONLINE_LAYER_ID = "OnlineBasemap";
		private const string LOCAL_LAYER_ID = "LocalTiles";
		private const string AOI_LAYER_ID = "AOI";
		private const string TILE_CACHE_FOLDER = "ExportTileCacheSample";

		private ArcGISTiledMapServiceLayer _onlineTiledLayer;
		private GraphicsOverlay _aoiOverlay;
		private ExportTileCacheTask _exportTilesTask;
		private GenerateTileCacheParameters _genOptions;

		/// <summary>Construct Export Tile Cache sample control</summary>
		public ExportTileCache()
		{
			InitializeComponent();

			var extentWGS84 = new Envelope(-123.77, 36.80, -119.77, 38.42, SpatialReferences.Wgs84);
			MyMapView.Map.InitialViewpoint = new Esri.ArcGISRuntime.Controls.Viewpoint(extentWGS84);
			MyMapView.Map.SpatialReference = SpatialReferences.WebMercator;

			MyMapView.Loaded += MyMapView_Loaded;
		}

		// Load the online basemap and dependent UI
		private async void MyMapView_Loaded(object sender, RoutedEventArgs e)
		{
			await InitializeOnlineBasemap();

			_aoiOverlay = new GraphicsOverlay() { Renderer = LayoutRoot.Resources["AOIRenderer"] as Renderer };
			MyMapView.GraphicsOverlays.Add(_aoiOverlay);

			if (_onlineTiledLayer.ServiceInfo != null)
			{
				sliderLOD.Minimum = 0;
				sliderLOD.Maximum = _onlineTiledLayer.ServiceInfo.TileInfo.Lods.Count - 1;
				sliderLOD.Value = sliderLOD.Maximum;
			}

			_exportTilesTask = new ExportTileCacheTask(new Uri(_onlineTiledLayer.ServiceUri));
		}

		// Create the online basemap layer (with token credentials) and add it to the map
		private async Task InitializeOnlineBasemap()
		{
			try
			{
				_onlineTiledLayer = new ArcGISTiledMapServiceLayer(new Uri(ONLINE_BASEMAP_URL));
				_onlineTiledLayer.ID = _onlineTiledLayer.DisplayName = ONLINE_LAYER_ID;

				// Generate token credentials if using tiledbasemaps.arcgis.com
				if (!string.IsNullOrEmpty(ONLINE_BASEMAP_TOKEN_URL))
				{
					// Set credentials and token for online basemap
					var options = new GenerateTokenOptions()
					{
						Referer = new Uri(_onlineTiledLayer.ServiceUri)
					};

					var cred = await IdentityManager.Current.GenerateCredentialAsync(ONLINE_BASEMAP_TOKEN_URL, USERNAME, PASSWORD);

					if (cred != null && !string.IsNullOrEmpty(cred.Token))
					{
						_onlineTiledLayer.Token = cred.Token;
						IdentityManager.Current.AddCredential(cred);
					}
				}

				await _onlineTiledLayer.InitializeAsync();
				MyMapView.Map.Layers.Add(_onlineTiledLayer);
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}

		// Estimate local tile cache size / space
		private async void EstimateCacheSizeButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// Get current viewpoints extent from the MapView
				var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
				var viewpointExtent = currentViewpoint.TargetGeometry.Extent;
				panelTOC.Visibility = Visibility.Collapsed;
				panelExport.Visibility = Visibility.Collapsed;
				progress.Visibility = Visibility.Visible;

				_aoiOverlay.Graphics.Clear();
				_aoiOverlay.Graphics.Add(new Graphic(viewpointExtent));

				_genOptions = new GenerateTileCacheParameters()
				{
					Format = ExportTileCacheFormat.TilePackage,
					MinScale = _onlineTiledLayer.ServiceInfo.TileInfo.Lods[(int)sliderLOD.Value].Scale,
					MaxScale = _onlineTiledLayer.ServiceInfo.TileInfo.Lods[0].Scale,
					GeometryFilter = GeometryEngine.Project(viewpointExtent, SpatialReferences.Wgs84)
				};

				var job = await _exportTilesTask.EstimateTileCacheSizeAsync(_genOptions,
					(result, ex) =>  // Callback for when estimate operation has completed
					{
						if (ex == null) // Check whether operation completed with errors
						{
							txtExportSize.Text = string.Format("Tiles: {0} - Size (kb): {1:0}", result.TileCount, result.Size / 1024);
							panelExport.Visibility = Visibility.Visible;
							panelTOC.Visibility = Visibility.Collapsed;
						}
						else
						{
							var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
						}
						progress.Visibility = Visibility.Collapsed;
					},
					TimeSpan.FromSeconds(1), // Check the operation every five seconds
					CancellationToken.None,
					new Progress<ExportTileCacheJob>((j) =>  // Callback for status updates
					{
						Debug.WriteLine(getTileCacheGenerationStatusMessage(j));
					}));
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}

		}

		// Download the tile cache
		private async void ExportTilesButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				panelTOC.Visibility = Visibility.Collapsed;
				progress.Visibility = Visibility.Visible;

				var downloadOptions = new DownloadTileCacheParameters(ApplicationData.Current.TemporaryFolder)
				{
					OverwriteExistingFiles = true
				};

				var localTiledLayer = MyMapView.Map.Layers.FirstOrDefault(lyr => lyr.ID == LOCAL_LAYER_ID);
				if (localTiledLayer != null)
					MyMapView.Map.Layers.Remove(localTiledLayer);

				var result = await _exportTilesTask.GenerateTileCacheAndDownloadAsync(
					_genOptions, downloadOptions, TimeSpan.FromSeconds(5), CancellationToken.None,
					new Progress<ExportTileCacheJob>((job) => // Callback for reporting status during tile cache generation
					{
						Debug.WriteLine(getTileCacheGenerationStatusMessage(job));
					}),
					new Progress<ExportTileCacheDownloadProgress>((downloadProgress) => // Callback for reporting status during tile cache download
					{
						Debug.WriteLine(getDownloadStatusMessage(downloadProgress));
					}));

				localTiledLayer = new ArcGISLocalTiledLayer(result.OutputPath) { ID = LOCAL_LAYER_ID };
				MyMapView.Map.Layers.Insert(1, localTiledLayer);

				_onlineTiledLayer.IsVisible = false;

				if (MyMapView.Scale < _genOptions.MinScale)
				{
					// Get current viewpoints extent from the MapView
					var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
					var viewpointExtent = currentViewpoint.TargetGeometry.Extent;
					await MyMapView.SetViewAsync(viewpointExtent.GetCenter(), _genOptions.MinScale);
				}
					
				panelTOC.Visibility = Visibility.Visible;
				panelExport.Visibility = Visibility.Collapsed;
				AppBarOptions.IsEnabled = true;

			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
			finally
			{
				progress.Visibility = Visibility.Collapsed;
			}
		}

		private void ShowAoiExtentCheckBox_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var chkbox = sender as CheckBox;
				var graphic = _aoiOverlay.Graphics.FirstOrDefault();
				if (chkbox != null && graphic != null)
				{
					graphic.IsVisible = (bool)chkbox.IsChecked;
				}
			}
			catch (Exception)
			{
			}
		}

		private static string getTileCacheGenerationStatusMessage(ExportTileCacheJob job)
		{
			if (job.Messages == null)
				return "";

			var text = string.Format("Job Status: {0}\n\nMessages:\n=====================\n", job.Status);
			foreach (GPMessage message in job.Messages)
			{
				text += string.Format("Message type: {0}\nMessage: {1}\n--------------------\n",
					message.MessageType, message.Description);
			}
			return text;
		}

		private static string getDownloadStatusMessage(ExportTileCacheDownloadProgress downloadProgress)
		{
			return string.Format("Downloading file {0} of {1}...\n{2:P0} complete\n" +
				"Bytes read: {3}", downloadProgress.FilesDownloaded, downloadProgress.TotalFiles, downloadProgress.ProgressPercentage,
				downloadProgress.CurrentFileBytesReceived);
		}

		private void btnResetMap_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var localTiledLayer = MyMapView.Map.Layers.FirstOrDefault(lyr => lyr.ID == LOCAL_LAYER_ID);
				if (localTiledLayer != null)
					MyMapView.Map.Layers.Remove(localTiledLayer);

				var extentWGS84 = new Envelope(-123.77, 36.80, -119.77, 38.42, SpatialReferences.Wgs84);
				MyMapView.SetView(extentWGS84);

			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
			finally
			{
				_aoiOverlay.IsVisible = false;
				_onlineTiledLayer.IsVisible = true;
			}
		}

		private void btnRemoveLocalLayer_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var localTiledLayer = MyMapView.Map.Layers.FirstOrDefault(lyr => lyr.ID == LOCAL_LAYER_ID);
				if (localTiledLayer != null)
					MyMapView.Map.Layers.Remove(localTiledLayer);
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
			finally
			{
				_aoiOverlay.IsVisible = false;
				_onlineTiledLayer.IsVisible = true;
			}
		}
	}
}
