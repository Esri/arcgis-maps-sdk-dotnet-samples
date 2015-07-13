using Esri.ArcGISRuntime.Layers;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Demonstrates adding a local tiled layer from a tile package (.tpk) to a map.
    /// </summary>
    /// <title>ArcGIS Local Tiled Layer</title>
    /// <category>Tiled Layers</category>
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
            var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(name);
            if (file != null)
                return (StorageFile)file;

            // prompt user for tile package file
            var original = await PickSingleFileAsync(".tpk");
            if (original == null)
                return null;

            // Copy target to LocalData
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            var dir = Path.GetDirectoryName(name);
            if (!string.IsNullOrEmpty(dir))
            {
                folder = await Windows.Storage.ApplicationData.Current.LocalFolder
                    .CreateFolderAsync(dir, CreationCollisionOption.OpenIfExists);
            }

            // Make a copy and return
            return await original.CopyAsync(folder, name);
        }

        /// <summary>
        /// FileOpenPicker for the user to select the a target file
        /// </summary>
        /// <returns></returns>
        internal static async Task<StorageFile> PickSingleFileAsync(string ext)
        {
            var filePicker = new FileOpenPicker();
            filePicker.ViewMode = PickerViewMode.List;
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            filePicker.FileTypeFilter.Add(ext);
            return await filePicker.PickSingleFileAsync();
        }
    }
}
