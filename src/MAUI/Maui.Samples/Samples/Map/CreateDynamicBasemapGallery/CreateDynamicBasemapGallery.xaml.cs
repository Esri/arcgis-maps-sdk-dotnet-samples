// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ArcGIS.Samples.CreateDynamicBasemapGallery
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
        /// <summary>
        /// A class to hold information about basemap styles, allowing for easy binding to UI elements and dynamic basemap creation.
        /// </summary>
        internal class BasemapStyleGalleryItem
        {
            public BasemapStyleInfo BasemapStyleInfo { get; set; }
            public BasemapStyle BasemapStyle { get; set; }
            public ImageSource ImageSource { get; set; }
        }

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
            List<BasemapStyleGalleryItem> items = new List<BasemapStyleGalleryItem>();
            foreach (var styleInfo in service.StylesInfo)
            {
                // Remove all whitespace from the style name.
                string styleName = styleInfo.StyleName.Replace(" ", string.Empty);

                // Get an enum of type BasemapStyle using the formatted style name.
                BasemapStyle style = (BasemapStyle)Enum.Parse(typeof(BasemapStyle), styleName, true);

                try
                {
                    var buffer = await styleInfo.Thumbnail.GetEncodedBufferAsync();
                    byte[] data = new byte[buffer.Length];
                    buffer.Read(data, 0, data.Length);
                    var bitmap = ImageSource.FromStream(() => new MemoryStream(data));

                    // Create a new basemap style gallery item.
                    items.Add(new BasemapStyleGalleryItem
                    {
                        BasemapStyle = style,
                        BasemapStyleInfo = styleInfo,
                        ImageSource = bitmap
                    });
                }
                catch (HttpRequestException e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            BasemapStyleGallery.ItemsSource = items;

            // Listen for basemap style selection events.
            BasemapStyleGallery.SelectionChanged += BasemapStyleGallery_SelectionChanged;

            // Make the dynamic basemap style gallery visible now that initialization is complete.
            LoadingIndicator.IsVisible = false;
            TransparentBackground.IsVisible = false;
            ShowGalleryButton.IsVisible = true;
        }

        private void BasemapStyleGallery_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the selected basemap style info.
            var styleInfo = ((BasemapStyleGalleryItem)BasemapStyleGallery.SelectedItem).BasemapStyleInfo;

            // Set the pickers to the available options for the selected basemap.
            StrategyPicker.ItemsSource = styleInfo.LanguageStrategies.ToList();
            LanguagePicker.ItemsSource = styleInfo.Languages.ToList();
            WorldviewPicker.ItemsSource = styleInfo.Worldviews.ToList();

            // Disable any pickers that have no items.
            LanguagePicker.IsEnabled = LanguagePicker.Items.Count > 0;
            WorldviewPicker.IsEnabled = WorldviewPicker.Items.Count > 0;
            StrategyPicker.IsEnabled = StrategyPicker.Items.Count > 0;
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
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
            BasemapStyle selectedBasemapStyle = ((BasemapStyleGalleryItem)BasemapStyleGallery.SelectedItem).BasemapStyle;

            // Update the map's basemap.
            MyMapView.Map.Basemap = new Basemap(selectedBasemapStyle, basemapStyleParameters);

            // Hide the gallery.
            HideGallery();
        }

        private void ShowGallery_Clicked(object sender, EventArgs e)
        {
            DynamicBasemapStyleGallery.IsVisible = true;
            TransparentBackground.IsVisible = true;
            ShowGalleryButton.IsEnabled = false;
        }

        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            HideGallery();
        }

        private void HideGallery()
        {
            DynamicBasemapStyleGallery.IsVisible = false;
            TransparentBackground.IsVisible = false;
            ShowGalleryButton.IsEnabled = true;
        }
    }
}