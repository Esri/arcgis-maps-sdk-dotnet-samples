// Copyright 2022 Esri.
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System.Collections.Generic;
using Esri.ArcGISRuntime.Portal;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Media;

namespace ArcGISRuntime.WinUI.Samples.SymbolStylesFromWebStyles
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Create symbol styles from web styles",
        category: "Symbology",
        description: "Create symbol styles from a style file hosted on a portal.",
        instructions: "The sample displays a map with a set of symbols that represent the categories of the features within the dataset. Pan and zoom on the map and view the legend to explore the appearance and names of the different symbols from the selected symbol style.",
        tags: new[] { "renderer", "symbol", "symbology", "web style" })]
    public partial class SymbolStylesFromWebStyles
    {
        UniqueValueRenderer _renderer;
        public ObservableCollection<SymbolLegendInfo> SymbolLegend = new ObservableCollection<SymbolLegendInfo>();

        public SymbolStylesFromWebStyles()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            MyMapView.Map = new Map(BasemapStyle.ArcGISNavigation);

            Uri webStyleLayerUri = new Uri("https://services.arcgis.com/V6ZHFr6zdgNZuVG0/arcgis/rest/services/LA_County_Points_of_Interest/FeatureServer/0");

            FeatureLayer webStyleLayer = new FeatureLayer(webStyleLayerUri);

            _renderer = new UniqueValueRenderer();
            _renderer.FieldNames.Add("cat2");
            webStyleLayer.Renderer = _renderer;

            MyMapView.Map.OperationalLayers.Add(webStyleLayer);

            MyMapView.Map.ReferenceScale = (100000);

            MapPoint centerPoint = new MapPoint(-118.44186, 34.28301, SpatialReferences.Wgs84);

            MyMapView.Map.InitialViewpoint = new Viewpoint(centerPoint, 7000);

            CreateSymbolStyles();
        }

        private async void CreateSymbolStyles()
        {
            Dictionary<string, List<string>> categories = CreateCategoriesMap();
            ArcGISPortal portal = await ArcGISPortal.CreateAsync();
            SymbolStyle esri2DPointSymbolStyle = await SymbolStyle.OpenAsync("Esri2DPointSymbolsStyle", portal);

            foreach (string symbolName in categories.Keys)
            {
                Symbol symbol = await esri2DPointSymbolStyle.GetSymbolAsync(new List<string>() { symbolName });

                RuntimeImage symbolSwatch = await symbol.CreateSwatchAsync();
                ImageSource source = await RuntimeImageExtensions.ToImageSourceAsync(symbolSwatch);
                SymbolLegend.Add(new SymbolLegendInfo() { Name = symbolName, Source= source});

                foreach (string symbolDescription in categories[symbolName])
                {
                    UniqueValue uniqueValue = new UniqueValue(symbolDescription, symbolName, symbol, new List<string>() { symbolDescription });

                    _renderer.UniqueValues.Add(uniqueValue);
                }
            }
        }

        private Dictionary<string, List<string>> CreateCategoriesMap()
        {
            Dictionary<string, List<string>> categoriesMap = new Dictionary<string, List<string>>();
            categoriesMap.Add("atm", new List<string>() { "Banking and Finance" });
            categoriesMap.Add("beach", new List<string>() { "Beaches and Marinas" });
            categoriesMap.Add("campground", new List<string>() { "Campgrounds" });
            categoriesMap.Add("city-hall", new List<string>() { "City Halls", "Government Offices" });
            categoriesMap.Add("hospital", new List<string>() { "Hospitals and Medical Centers", "Health Screening and Testing", "Health Centers", "Mental Health Centers" });
            categoriesMap.Add("library", new List<string>() { "Libraries" });
            categoriesMap.Add("park", new List<string>() { "Parks and Gardens" });
            categoriesMap.Add("place-of-worship", new List<string>() { "Churches" });
            categoriesMap.Add("police-station", new List<string>() { "Sheriff and Police Stations" });
            categoriesMap.Add("post-office", new List<string>() { "DHL Locations", "Federal Express Locations" });
            categoriesMap.Add("school", new List<string>() { "Public High Schools", "Public Elementary Schools", "Private and Charter Schools" });
            categoriesMap.Add("trail", new List<string>() { "Trails" });

            return categoriesMap;
        }
    }

    public class SymbolLegendInfo
    {
        public string Name { get; set; }
        public ImageSource Source { get; set; }
    }
}