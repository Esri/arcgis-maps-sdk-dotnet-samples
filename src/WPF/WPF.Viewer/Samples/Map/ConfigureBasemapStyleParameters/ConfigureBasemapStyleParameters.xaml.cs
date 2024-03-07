// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace ArcGIS.WPF.Samples.ConfigureBasemapStyleParameters
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Configure basemap style parameters",
        category: "Map",
        description: "Apply basemap style parameters customization for a basemap, such as displaying all labels in a specific language or displaying every label in their corresponding local language.",
        instructions: "This sample showcases the workflow of configuring basemap style parameters by displaying a basemap with labels in different languages and launches with a `Viewpoint` set over Bulgaria, Greece and Turkey, as they use three different alphabets: Cyrillic, Greek, and Latin, respectively. By default, the `BasemapStyleLanguageStrategy` is set to `Local` which displays all labels in their corresponding local language. This can be changed to `Global`, which displays all labels in English. The `SpecificLanguage` setting sets all labels to a selected language and overrides the `BasemapStyleLanguageStrategy` settings.",
        tags: new[] { "basemap style", "language", "language strategy", "map", "point", "viewpoint" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class ConfigureBasemapStyleParameters
    {
        /// <summary>
        /// Get the selected language from the language picker.
        /// </summary>
        private string SelectedLanguage
        {
            get => (LanguagePicker.SelectedItem as ComboBoxItem)?.Content.ToString();
        }

        public ConfigureBasemapStyleParameters()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            MyMapView.Map = new Map();
            SetNewBasemap();

            //  Focus the viewpoint on an area where the different languages are best showcased: Bulgaria / Greece / Turkey
            //  as they use three different alphabets: Cyrillic, Greek, and Latin, respectively.
            //  Thus, showcasing the different functionalities in the most obvious way:
            //  all English, all Greek, all Bulgarian, all Turkish, or each their own.
            MyMapView.SetViewpoint(new Viewpoint(new MapPoint(3144804, 4904598, SpatialReferences.WebMercator), 10000000));

            // Ensure parameter changes are reflected on the basemap.
            GlobalRadioButton.Checked += StrategyRadioButton_Checked;
            LocalRadioButton.Checked += StrategyRadioButton_Checked;
            LanguagePicker.SelectionChanged += LanguagePicker_SelectionChanged;
        }

        private void SetNewBasemap()
        {
            BasemapStyleParameters basemapStyleParameters = new BasemapStyleParameters();

            basemapStyleParameters.LanguageStrategy = (bool)GlobalRadioButton.IsChecked ? BasemapStyleLanguageStrategy.Global : BasemapStyleLanguageStrategy.Local;

            switch (SelectedLanguage)
            {
                // A SpecificLanguage setting overrides the BasemapStyleLanguageStrategy settings when
                // the BasemapStyleParameters.SpecificLanguage is a non-empty string.
                // Setting the specific language back to an empty string allows the strategy to be used.
                case "None":
                    basemapStyleParameters.SpecificLanguage = new CultureInfo("");
                    break;

                case "Bulgarian":
                    basemapStyleParameters.SpecificLanguage = new CultureInfo("bg");
                    break;

                case "Greek":
                    basemapStyleParameters.SpecificLanguage = new CultureInfo("el");
                    break;

                case "Turkish":
                    basemapStyleParameters.SpecificLanguage = new CultureInfo("tr");
                    break;
            }

            MyMapView.Map.Basemap = new Basemap(BasemapStyle.OSMLightGray, basemapStyleParameters);
        }

        private void LanguagePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GlobalRadioButton.IsEnabled = LocalRadioButton.IsEnabled = SelectedLanguage == "None";
            SetNewBasemap();
        }

        private void StrategyRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SetNewBasemap();
        }
    }
}