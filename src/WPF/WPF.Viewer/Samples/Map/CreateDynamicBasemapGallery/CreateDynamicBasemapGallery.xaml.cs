// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGIS.WPF.Samples.CreateDynamicBasemapGallery
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Create dynamic basemap gallery",
        category: "Map",
        description: "Implement a basemap gallery that automatically retrieves the latest customization options from the basemap styles service.",
        instructions: "When launched, this sample displays a map containing a button that, when pressed, displays a gallery of all styles available in the basemap styles service. Selecting a style results in the drop-down menus at the base of the gallery becoming enabled or disabled. A disabled menu indicates that the customization cannot be applied to the selected style. Once a style and any desired customizations have been selected, pressing `Load` will update the basemap in the map view.",
        tags: new[] { "basemap", "languages", "service", "style" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class CreateDynamicBasemapGallery
    {
        public CreateDynamicBasemapGallery()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a new map with the ArcGIS Navigation basemap style.
            MyMapView.Map = new Map(BasemapStyle.ArcGISNavigation);

            // Create a new basemap styles service, pulling in the available styles and their information.
            BasemapStylesServiceInfo service = await BasemapStylesServiceInfo.CreateAsync();

            // Populate the basemap style gallery.
            BasemapStyleGallery.ItemsSource = service.StylesInfo;

            // Listen for basemap style selection events.
            BasemapStyleGallery.SelectionChanged += BasemapStyleGallery_SelectionChanged;
        }

        private void BasemapStyleGallery_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected basemap style info.
            var styleInfo = (BasemapStyleInfo)BasemapStyleGallery.SelectedItem;

            // Set the pickers to the available options for the selected basemap.
            StrategyPicker.ItemsSource = styleInfo.LanguageStrategies;
            LanguagePicker.ItemsSource = styleInfo.Languages;
            WorldviewPicker.ItemsSource = styleInfo.Worldviews;

            // Disable any pickers that have no items.
            LanguagePicker.IsEnabled = LanguagePicker.Items.Count > 0;
            WorldviewPicker.IsEnabled = WorldviewPicker.Items.Count > 0;
            StrategyPicker.IsEnabled = StrategyPicker.Items.Count > 0;
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            // Return if no basemap style is selected.
            if (BasemapStyleGallery.SelectedItem == null) return;

            // Create a new basemap style parameters object.
            var basemapStyleParameters = new BasemapStyleParameters();

            // Set the language to the selected language.
            if (LanguagePicker.SelectedItem != null)
                basemapStyleParameters.SpecificLanguage = (LanguagePicker.SelectedItem as BasemapStyleLanguageInfo).CultureInfo;

            // Set the worldview to the selected worldview.
            if (WorldviewPicker.SelectedItem != null)
                basemapStyleParameters.Worldview = (WorldviewPicker.SelectedItem as Worldview);

            // Set the strategy to the selected strategy.
            if (StrategyPicker.SelectedItem != null)
                basemapStyleParameters.LanguageStrategy = (BasemapStyleLanguageStrategy)StrategyPicker.SelectedItem;

            // Determine the basemap style of the currently selected basemap.
            BasemapStyle selectedBasemapStyle = ((BasemapStyleInfo)BasemapStyleGallery.SelectedItem).Style;

            // Update the map's basemap.
            MyMapView.Map.Basemap = new Basemap(selectedBasemapStyle, basemapStyleParameters);
        }
    }
}