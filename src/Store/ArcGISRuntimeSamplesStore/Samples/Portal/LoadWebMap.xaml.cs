using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.WebMap;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// This sample demonstrates loading a WebMap from ArcGIS Online.
    /// </summary>
    /// <title>Load WebMap</title>
    /// <category>Portal</category>
    public partial class LoadWebMap : Page
    {
        private ArcGISPortal _portal;

        /// <summary>Construct Load WebMap sample control</summary>
        public LoadWebMap()
        {
            InitializeComponent();
            Loaded += LoadWebMap_Loaded;
        }

        // Loads UI elements and an initial webmap
        private async void LoadWebMap_Loaded(object sender, RoutedEventArgs e)
        {
            _portal = await ArcGISPortal.CreateAsync();
            var searchParams = new SearchParameters("type: \"web map\" NOT \"web mapping application\"");
            var result = await _portal.ArcGISPortalInfo.SearchHomePageFeaturedContentAsync(searchParams);
            comboWebMap.ItemsSource = result.Results;
        }

        // Loads a webmap
		private async void comboWebMap_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
            string id = string.Empty;
			if (comboWebMap.SelectedIndex >= 0)
			{
				id = comboWebMap.SelectedValue as string;
				await LoadWebMapAsync(id);
			}
        }

		// Load a webmap by the given ID
		private async void LoadByIdButton_Click(object sender, RoutedEventArgs e)
		{
			string id = txtWebmapId.Text;
			if (!string.IsNullOrEmpty(id))
			{
				await LoadWebMapAsync(id);
				comboWebMap.SelectedIndex = -1;
			}
		}
		
		// Loads the given webmap
        private async Task LoadWebMapAsync(string wmId)
        {
            try
            {
                progress.Visibility = Visibility.Visible;

                var item = await ArcGISPortalItem.CreateAsync(_portal, wmId);
                var webmap = await WebMap.FromPortalItemAsync(item);
                var vm = await WebMapViewModel.LoadAsync(webmap, _portal);
                MyMapView.Map = vm.Map;

                detailsPanel.DataContext = item;
            }
            catch (Exception ex)
            {
				infoToggle.IsChecked = false;
				var _ = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}
