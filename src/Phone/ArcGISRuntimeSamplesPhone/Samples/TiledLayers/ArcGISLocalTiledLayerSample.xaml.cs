using Esri.ArcGISRuntime.Layers;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
    /// <summary>
    /// Demonstrates adding a local tiled layer from a tile package (.tpk) to a map.
    /// </summary>
    /// <title>ArcGIS Local Tiled Layer</title>
    /// <category>Tiled Layers</category>
	/// <localdata>true</localdata>
    public partial class ArcGISLocalTiledLayerSample : Page
    {
        public ArcGISLocalTiledLayerSample()
        {
            InitializeComponent();

			MyMapView.Loaded += MyMapView_Loaded;
        }

        private async void MyMapView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var tpk = await GetSingleFileAsync(@"basemaps\campus.tpk");
                var layer = new ArcGISLocalTiledLayer(tpk) { ID = "local_basemap" };
                MyMapView.Map.Layers.Add(layer);
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }

        /// <summary>
        /// Get a single file from the user
        /// </summary>
        /// <remarks>Copies the selected file to App Local Data before returning</remarks>
        internal static async Task<StorageFile> GetSingleFileAsync(string name)
        {
            // Attempt to open LocalFolder target
			try
			{
				return await ApplicationData.Current.LocalFolder.GetFileAsync(name);
			}
			catch (FileNotFoundException)
			{
				throw new Exception("Local tile package not found. Please download sample data from the main page.");
			}
        }
    }
}
