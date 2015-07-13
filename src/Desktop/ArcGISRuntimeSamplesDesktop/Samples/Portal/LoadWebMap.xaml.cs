using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.WebMap;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates adding data a WebMap from ArcGIS Online to an application.
    /// </summary>
    /// <title>Load WebMap</title>
    /// <category>Portal</category>
    public partial class LoadWebMap : UserControl
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

            var webmap = result.Results.FirstOrDefault();
            if (webmap != null)
            {
                comboWebMap.SelectedIndex = 0;
                await LoadWebMapAsync(webmap.Id);
            }
        }

        // Loads a webmap on load button click
        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            string id = string.Empty;
            if (comboWebMap.SelectedIndex >= 0)
                id = comboWebMap.SelectedValue as string;
            else
                id = comboWebMap.Text;

            await LoadWebMapAsync(id);
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
                detailsPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                detailsPanel.Visibility = Visibility.Visible;
                MessageBox.Show(ex.Message, "Sample Error");
            }
            finally
            {
                progress.Visibility = Visibility.Hidden;
            }
        }
    }
}
