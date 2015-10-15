//Copyright 2015 Esri.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Portal;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Desktop.Samples.MapFromPortal
{
    public partial class MapFromPortal
    {
        private ArcGISPortal _portal;

        /// <summary>Construct Load Map sample control</summary>
        public MapFromPortal()
        {
            InitializeComponent();

            Loaded += LoadMap_Loaded;
        }

        // Loads UI elements and an initial Map
        private async void LoadMap_Loaded(object sender, RoutedEventArgs e)
        {
            _portal = await ArcGISPortal.CreateAsync();

            var searchParams = new SearchParameters("type: \"web map\" NOT \"web mapping application\"");
            var result = await _portal.ArcGISPortalInfo.SearchHomePageFeaturedContentAsync(searchParams);
            comboMap.ItemsSource = result.Results;

            var map = result.Results.FirstOrDefault();
            if (map != null)
            {
                comboMap.SelectedIndex = 0;
                await LoadMapAsync(map.Id);
            }
        }

        // Loads a webmap on load button click
        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            string id = string.Empty;
            if (comboMap.SelectedIndex >= 0)
                id = comboMap.SelectedValue as string;
            else
                id = comboMap.Text;

            await LoadMapAsync(id);
        }

        // Loads the given webmap
        private async Task LoadMapAsync(string wmId)
        {
            try
            {
                progress.Visibility = Visibility.Visible;

                var item = await ArcGISPortalItem.CreateAsync(_portal, wmId);
                var map = new Esri.ArcGISRuntime.Map(item);
                MyMapView.Map = map;
 
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