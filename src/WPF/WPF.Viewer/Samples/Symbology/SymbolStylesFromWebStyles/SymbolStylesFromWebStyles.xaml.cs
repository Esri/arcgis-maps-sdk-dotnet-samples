// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ArcGIS.WPF.Samples.SymbolStylesFromWebStyles
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Create symbol styles from web styles",
        category: "Symbology",
        description: "Create symbol styles from a style file hosted on a portal.",
        instructions: "The sample displays a map with a set of symbols that represent the categories of the features within the dataset. Pan and zoom on the map and view the legend to explore the appearance and names of the different symbols from the selected symbol style.",
        tags: new[] { "renderer", "symbol", "symbology", "web style" })]
    public partial class SymbolStylesFromWebStyles
    {
        // Hold a reference to the renderer.
        private UniqueValueRenderer _renderer;

        // Hold a reference to the feature layer.
        private FeatureLayer _webStyleLayer;

        // Hold a list of symbol data for the legend.
        private readonly ObservableCollection<SymbolLegendInfo> _symbolLegendCollection = new ObservableCollection<SymbolLegendInfo>();

        public SymbolStylesFromWebStyles()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Create a new basemap and assign it to the map view.
                MyMapView.Map = new Map(BasemapStyle.ArcGISNavigation);

                // URL for a feature layer that contains points of interest in varying categories across LA County.
                Uri webStyleLayerUri = new Uri("https://services.arcgis.com/V6ZHFr6zdgNZuVG0/arcgis/rest/services/LA_County_Points_of_Interest/FeatureServer/0");

                // Create the feature layer from the given URL.
                _webStyleLayer = new FeatureLayer(webStyleLayerUri);

                // Instantiate a UniqueValueRenderer, this will impact specific features based on the values of the specified FieldName(s).
                _renderer = new UniqueValueRenderer();
                _renderer.FieldNames.Add("cat2");

                // The UniqueValueRenderer defines how features of a FeatureLayer are styled.
                // Without an overriding UniqueValueRenderer features will use the web layer's default gray circle style.
                _webStyleLayer.Renderer = _renderer;

                // Add the feature layer to the map view.
                MyMapView.Map.OperationalLayers.Add(_webStyleLayer);

                // Set the scale at which feature symbols and text will appear at their default size.
                MyMapView.Map.ReferenceScale = 100000;

                // Set the the initial view point for the map view.
                MapPoint centerPoint = new MapPoint(-118.44186, 34.28301, SpatialReferences.Wgs84);
                MyMapView.Map.InitialViewpoint = new Viewpoint(centerPoint, 7000);

                // Set the item source for the legend to the ObservableCollection containing legend data.
                LegendItemsControl.ItemsSource = _symbolLegendCollection;

                // Load the symbols from portal and add them to the renderer.
                await CreateSymbolStyles();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CreateSymbolStyles()
        {
            // Create a dictionary of symbol categories and their associated symbol name.
            Dictionary<string, List<string>> symbolCategories = CreateCategoriesMap();

            // Create a portal to enable access to the symbols.
            ArcGISPortal portal = await ArcGISPortal.CreateAsync();

            // Create a SymbolStyle, this is used to return symbols based on provided symbol keys from portal.
            SymbolStyle esri2DPointSymbolStyle = await SymbolStyle.OpenAsync("Esri2DPointSymbolsStyle", portal);

            // Loop through each of the keys in the symbol categories dictionary and retrieve each symbol from portal.
            foreach (string symbolName in symbolCategories.Keys)
            {
                // This call is used to retrieve a single symbol for a given symbol name, if multiple symbol names are provided
                // a multilayer symbol assembled and returned for the given symbol names.
                Symbol symbol = await esri2DPointSymbolStyle.GetSymbolAsync(new List<string> { symbolName });

                // Get the image source for the symbol to populate the legend UI.
                RuntimeImage symbolSwatch = await symbol.CreateSwatchAsync();
                ImageSource imageSource = await RuntimeImageExtensions.ToImageSourceAsync(symbolSwatch);

                // Add the symbol the ObservableCollection containing the symbol legend data.
                _symbolLegendCollection.Add(new SymbolLegendInfo() { Name = symbolName, ImageSource = imageSource });

                // Loop through each of the categories in the symbol categories dictionary for the given symbol name.
                // This needs to be done to ensure that a UniqueValue is created for each symbol category.
                // Numerous categories can have the same matching symbol name, however each category needs their own UniqueValue.
                foreach (string symbolCategory in symbolCategories[symbolName])
                {
                    // Create a UniqueValue for a given symbol category, name and symbol.
                    // If multiple categories are passed in this UniqueValue will only be used in cases where every given category is matched in our data set.
                    // In the data set used in this sample each point of interest is only represented by a single category so we only use a single category in this case.
                    UniqueValue uniqueValue = new UniqueValue(symbolCategory, symbolName, symbol, new List<string> { symbolCategory });

                    // Add the UniqueValue to the renderer.
                    _renderer.UniqueValues.Add(uniqueValue);
                }
            }
        }

        private Dictionary<string, List<string>> CreateCategoriesMap()
        {
            Dictionary<string, List<string>> symbolCategories = new Dictionary<string, List<string>>();
            symbolCategories.Add("atm", new List<string>() { "Banking and Finance" });
            symbolCategories.Add("beach", new List<string>() { "Beaches and Marinas" });
            symbolCategories.Add("campground", new List<string>() { "Campgrounds" });
            symbolCategories.Add("city-hall", new List<string>() { "City Halls", "Government Offices" });
            symbolCategories.Add("hospital", new List<string>() { "Hospitals and Medical Centers", "Health Screening and Testing", "Health Centers", "Mental Health Centers" });
            symbolCategories.Add("library", new List<string>() { "Libraries" });
            symbolCategories.Add("park", new List<string>() { "Parks and Gardens" });
            symbolCategories.Add("place-of-worship", new List<string>() { "Churches" });
            symbolCategories.Add("police-station", new List<string>() { "Sheriff and Police Stations" });
            symbolCategories.Add("post-office", new List<string>() { "DHL Locations", "Federal Express Locations" });
            symbolCategories.Add("school", new List<string>() { "Public High Schools", "Public Elementary Schools", "Private and Charter Schools" });
            symbolCategories.Add("trail", new List<string>() { "Trails" });

            return symbolCategories;
        }

        private void MapViewExtentChanged(object sender, EventArgs e)
        {
            // Set scale symbols to true when we zoom in so the symbols don't take up the entire view.
            _webStyleLayer.ScaleSymbols = MyMapView.MapScale >= 80000;
        }
    }

    public class SymbolLegendInfo
    {
        public string Name { get; set; }
        public ImageSource ImageSource { get; set; }
    }
}