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
using System;
using System.Collections.Generic;

namespace ArcGIS.WPF.Samples.UniqueValuesAlternateSymbols
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Apply unique values with alternate symbols",
        category: "Symbology",
        description: "Apply a unique value with alternate symbols at different scales.",
        instructions: "Zoom in and out of the map to see alternate symbols at each scale. The symbology changes according to the following scale ranges: 0-5000, 5000-10000, 10000-20000. To go back to the initial viewpoint, press \"Reset Viewpoint\".",
        tags: new[] { "alternate symbols", "scale based rendering", "symbology", "unique value", "unique value renderer" })]
    public partial class UniqueValuesAlternateSymbols
    {
        private Viewpoint _initialViewpoint = new Viewpoint(new MapPoint(-13631205.660131, 4546829.846004, SpatialReferences.WebMercator), 25000);

        public UniqueValuesAlternateSymbols()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create a feature layer using the feature table.
            var featureTable = new ServiceFeatureTable(new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/SF311/FeatureServer/0"));
            var featureLayer = new FeatureLayer(featureTable);

            // Create a symbol for a specific scale range.
            MultilayerPointSymbol triangleMultilayerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Triangle, System.Drawing.Color.Red, 30).ToMultilayerSymbol();
            triangleMultilayerSymbol.ReferenceProperties = new SymbolReferenceProperties(minScale: 5000, maxScale: 0);

            // Create alternate symbols for use at different scale ranges.
            MultilayerPointSymbol blueAlternateSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Square, System.Drawing.Color.Blue, 30).ToMultilayerSymbol();
            blueAlternateSymbol.ReferenceProperties = new SymbolReferenceProperties(minScale: 10000, maxScale: 5000);

            MultilayerPointSymbol yellowAlternateSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, System.Drawing.Color.Yellow, 30).ToMultilayerSymbol();
            yellowAlternateSymbol.ReferenceProperties = new SymbolReferenceProperties(minScale: 20000, maxScale: 10000);

            var alternateSymbols = new List<Symbol> { blueAlternateSymbol, yellowAlternateSymbol };

            // Create a unique value with the triangle symbol and the alternate symbols.
            var uniqueValue = new UniqueValue("unique value", "unique values based on request type", triangleMultilayerSymbol, "Damaged Property", alternateSymbols);

            // Create a unique value renderer.
            var uniqueValueRenderer = new UniqueValueRenderer();
            uniqueValueRenderer.UniqueValues.Add(uniqueValue);
            uniqueValueRenderer.FieldNames.Add("req_type");

            // Set a default symbol for the unique value renderer. This will be use for features that aren't "Damaged Property" or when out of range of the UniqueValue symbols.
            uniqueValueRenderer.DefaultSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, System.Drawing.Color.Purple, 15).ToMultilayerSymbol();

            // Set the unique value renderer on the feature layer.
            featureLayer.Renderer = uniqueValueRenderer;

            // Create the map.
            MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic) { InitialViewpoint = _initialViewpoint };
            MyMapView.Map.OperationalLayers.Add(featureLayer);
        }

        private void ResetViewpointClick(object sender, System.Windows.RoutedEventArgs e)
        {
            MyMapView.SetViewpointAsync(_initialViewpoint, TimeSpan.FromSeconds(5));
        }
    }
}