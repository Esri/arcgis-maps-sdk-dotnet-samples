// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System.Collections.Generic;
using System.Windows;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;

namespace ArcGISRuntime.WPF.Samples.ChangeSublayerRenderer
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change sublayer renderer",
        "Layers",
        "This sample demonstrates how to change the sub-layer renderer of an ArcGIS map image layer. A unique value renderer is applied to see different population ranges in the counties sub-layer data.",
        "Click the 'Change Sublayer Renderer' button to apply a unique value renderer to the counties sub-layer.",
        "")]
    public partial class ChangeSublayerRenderer
    {
        // ArcGIS map image layer that contains four Census sub-layers.
        private ArcGISMapImageLayer _arcGISMapImageLayer;

        public ChangeSublayerRenderer()
        {
            InitializeComponent();

            // Load the initial datasets in the map.
            Initialize();
        }
        
        private async void Initialize()
        {
            // Create a new map based on the streets base map.
            Map newMap = new Map(Basemap.CreateStreets());

            // Assign the map to the MapView.
            MyMapView.Map = newMap;

            // Create an ArcGIS map image layer based on the Uri to that points to an ArcGIS Server map service that contains four Census sub-layers.
            // NOTE: sub-layer[0] = Census Block Points, sub-layer[1] = Census Block Group, sub-layer[3] = Counties, sub-layer[3] = States. 
            _arcGISMapImageLayer = new ArcGISMapImageLayer(new System.Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer"));

            // Add the ArcGIS map image layer to the map's operation layers collection.
            newMap.OperationalLayers.Add(_arcGISMapImageLayer);

            // Load the ArcGIS map image layer.
            await _arcGISMapImageLayer.LoadAsync();

            // Create an envelope that covers the continental US in the web Mercator spatial reference.
            Envelope continentalUSEnvelope = new Envelope(-14193469.5655232, 2509617.28647268, -7228772.04749191, 6737139.97573925 , SpatialReferences.WebMercator);

            // Zoom the map to the extent of the envelope.
            await MyMapView.SetViewpointGeometryAsync(continentalUSEnvelope);
        }

        private ClassBreaksRenderer CreateClassBreaksRenderer()
        {
            // Define the colors that will be used by the unique value renderer.
            System.Windows.Media.Color gray = System.Windows.Media.Color.FromArgb(255, 153, 153, 153);
            System.Windows.Media.Color blue1 = System.Windows.Media.Color.FromArgb(255, 227, 235, 207);
            System.Windows.Media.Color blue2 = System.Windows.Media.Color.FromArgb(255, 150, 194, 191);
            System.Windows.Media.Color blue3 = System.Windows.Media.Color.FromArgb(255, 97, 166, 181);
            System.Windows.Media.Color blue4 = System.Windows.Media.Color.FromArgb(255, 69, 125, 150);
            System.Windows.Media.Color blue5 = System.Windows.Media.Color.FromArgb(255, 41, 84, 120);

            // Create a gray outline and five fill symbols with different shades of blue.
            SimpleLineSymbol outlineSimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, gray, 1);
            SimpleFillSymbol simpleFileSymbol1 = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, blue1, outlineSimpleLineSymbol);
            SimpleFillSymbol simpleFileSymbol2 = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, blue2, outlineSimpleLineSymbol);
            SimpleFillSymbol simpleFileSymbol3 = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, blue3, outlineSimpleLineSymbol);
            SimpleFillSymbol simpleFileSymbol4 = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, blue4, outlineSimpleLineSymbol);
            SimpleFillSymbol simpleFileSymbol5 = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, blue5, outlineSimpleLineSymbol);

            // Create a list of five class breaks for different population ranges.
            List<ClassBreak> listClassBreaks = new List<ClassBreak>
            {
                new ClassBreak("-99 to 8560", "-99 to 8560", -99, 8560, simpleFileSymbol1),
                new ClassBreak("> 8,560 to 18,109", "> 8,560 to 18,109", 8560, 18109, simpleFileSymbol2),
                new ClassBreak("> 18,109 to 35,501", "> 18,109 to 35,501", 18109, 35501, simpleFileSymbol3),
                new ClassBreak("> 35,501 to 86,100", "> 35,501 to 86,100", 35501, 86100, simpleFileSymbol4),
                new ClassBreak("> 86,100 to 10,110,975", "> 86,100 to 10,110,975", 86100, 10110975, simpleFileSymbol5)
            };

            // Create and return the a class break renderer for use with the POP2007 field in the counties sub-layer.
            return new ClassBreaksRenderer("POP2007", listClassBreaks);
        }

        private void ChangeSublayerRendererButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the counties sub-layer (the 3rd layer) from the ArcGIS map image layer.
            ArcGISMapImageSublayer countiesArcGISMapImageSubLayer = (ArcGISMapImageSublayer)_arcGISMapImageLayer.Sublayers[2];

            // Set the renderer of the ArcGIS map image sub-layer to a class break renderer based on population.
            countiesArcGISMapImageSubLayer.Renderer = CreateClassBreaksRenderer();

            // Disable the button after has been used.
            ChangeSublayerRendererButton.IsEnabled = false;
        }

    }
}