using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.WebMap;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates how to search for, return, and dynamically use basemaps from ArcGIS Online.
    /// </summary>
    /// <title>Basemaps</title>
    /// <category>Portal</category>
    public partial class PortalBasemaps : UserControl
    {
        private WebMapViewModel _currentVM;

        /// <summary>Construct Portal Basemaps sample control</summary>
        public PortalBasemaps()
        {
            InitializeComponent();

            MyMapView.Loaded += MyMapView_Loaded;
        }

        // Initialize the display with a web map and search portal for basemaps
        private async void MyMapView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                progress.Visibility = Visibility.Visible;

                // Load initial webmap
                var portal = await ArcGISPortal.CreateAsync();
                var item = await ArcGISPortalItem.CreateAsync(portal, "3679c136c2694d0b95bb5e6c3f2b480e");
                var webmap = await WebMap.FromPortalItemAsync(item);
                _currentVM = await WebMapViewModel.LoadAsync(webmap, portal);
                MyMapView.Map = _currentVM.Map;

                // Load portal basemaps
                var result = await portal.ArcGISPortalInfo.SearchBasemapGalleryAsync();
                basemapList.ItemsSource = result.Results;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }

        // Switch current maps basemap when the user selects a portal item
        private async void BaseMapButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                progress.Visibility = Visibility.Visible;

                var item = ((Button)sender).DataContext as ArcGISPortalItem;
                var webmap = await WebMap.FromPortalItemAsync(item);
                var basemapVM = await WebMapViewModel.LoadAsync(webmap, _currentVM.ArcGISPortal);
				_currentVM.Basemap = basemapVM.Basemap;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}
