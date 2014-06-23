using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using Esri.ArcGISRuntime.Tasks.Offline;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates how to download a local tile cache from an online service with the ExportTiles operation enabled.
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
        private GraphicsLayer _aoiLayer;
        private ExportTileCacheTask _exportTilesTask;
        private GenerateTileCacheParameters _genOptions;

        /// <summary>Construct Export Tile Cache sample control</summary>
        public ExportTileCache()
        {
            InitializeComponent();

            var extentWGS84 = new Envelope(-123.77, 36.80, -119.77, 38.42, SpatialReferences.Wgs84);
            mapView.Map.InitialExtent = GeometryEngine.Project(extentWGS84, SpatialReferences.WebMercator) as Envelope;

            mapView.Loaded += mapView_Loaded;
        }

        // Load the online basemap and dependent UI
        private async void mapView_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeOnlineBasemap();

            _aoiLayer = new GraphicsLayer() { ID = AOI_LAYER_ID, Renderer = LayoutRoot.Resources["AOIRenderer"] as Renderer };
            mapView.Map.Layers.Add(_aoiLayer);

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
                    var options = new IdentityManager.GenerateTokenOptions()
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
                mapView.Map.Layers.Add(_onlineTiledLayer);
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        // Estimate local tile cache size / space
        private async void EstimateCacheSizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                panelExport.Visibility = Visibility.Collapsed;
                progress.Visibility = Visibility.Visible;

                _aoiLayer.Graphics.Clear();
                _aoiLayer.Graphics.Add(new Graphic(mapView.Extent));

                _genOptions = new GenerateTileCacheParameters()
                {
                    Format = ExportTileCacheFormat.TilePackage,
                    MinScale = _onlineTiledLayer.ServiceInfo.TileInfo.Lods[(int)sliderLOD.Value].Scale,
                    MaxScale = _onlineTiledLayer.ServiceInfo.TileInfo.Lods[0].Scale,
                    GeometryFilter = GeometryEngine.Project(mapView.Extent, SpatialReferences.Wgs84)
                };

                var job = await _exportTilesTask.EstimateTileCacheSizeAsync(_genOptions);

                // Poll for the results async
                while (job.Status != GPJobStatus.Cancelled && job.Status != GPJobStatus.Deleted
                    && job.Status != GPJobStatus.Succeeded && job.Status != GPJobStatus.TimedOut
                    && job.Status != GPJobStatus.Failed)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    await _exportTilesTask.CheckEstimateTileCacheSizeJobStatusAsync(job);
                }

                if (job.Status == GPJobStatus.Succeeded)
                {
                    var result = await _exportTilesTask.CheckEstimateTileCacheSizeJobStatusAsync(job);
                    txtExportSize.Text = string.Format("Tiles: {0} - Size (kb): {1:0}", result.TileCount, result.Size / 1024);
                    panelExport.Visibility = Visibility.Visible;
                    panelTOC.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
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

                var result = await _exportTilesTask.GenerateTileCacheAndDownloadAsync(
                    _genOptions, downloadOptions, TimeSpan.FromSeconds(5), CancellationToken.None);

                var localTiledLayer = mapView.Map.Layers.FirstOrDefault(lyr => lyr.ID == LOCAL_LAYER_ID);
                if (localTiledLayer != null)
                    mapView.Map.Layers.Remove(localTiledLayer);

                localTiledLayer = new ArcGISLocalTiledLayer(result.OutputPath) { ID = LOCAL_LAYER_ID };
                mapView.Map.Layers.Insert(1, localTiledLayer);

                _onlineTiledLayer.IsVisible = false;

                if (mapView.Scale < _genOptions.MinScale)
                    await mapView.SetViewAsync(mapView.Extent.GetCenter(), _genOptions.MinScale);

                panelTOC.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}
