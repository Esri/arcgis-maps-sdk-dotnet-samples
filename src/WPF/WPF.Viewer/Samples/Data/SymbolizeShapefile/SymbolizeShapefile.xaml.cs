// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.SymbolizeShapefile
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Symbolize shapefile",
        category: "Data",
        description: "Display a shapefile with custom symbology.",
        instructions: "Click the button to apply a new symbology renderer to the feature layer created from the shapefile. ",
        tags: new[] { "package", "shape file", "shapefile", "symbology", "visualization" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("d98b3e5293834c5f852f13c569930caa")]
    public partial class SymbolizeShapefile
    {
        // Hold reference to the feature layer so that its renderer can be changed when button is pushed
        private FeatureLayer _shapefileFeatureLayer;

        // Hold reference to default renderer to enable switching back
        private Renderer _defaultRenderer;

        // Hold reference to alternate renderer to enable switching
        private SimpleRenderer _alternateRenderer;

        public SymbolizeShapefile()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create the map with topographic basemap
            Map myMap = new Map(BasemapStyle.ArcGISTopographic);

            // Create the point for the map's initial viewpoint
            MapPoint point = new MapPoint(-11662054, 4818336, SpatialReference.Create(3857));

            // Create a viewpoint for the point
            Viewpoint viewpoint = new Viewpoint(point, 200000);

            // Set the initial viewpoint
            myMap.InitialViewpoint = viewpoint;

            // Create a shapefile feature table from the shapefile path
            ShapefileFeatureTable myFeatureTable = new ShapefileFeatureTable(GetShapefilePath());

            // Create a layer from the feature table
            _shapefileFeatureLayer = new FeatureLayer(myFeatureTable);

            // Add the layer to the map
            myMap.OperationalLayers.Add(_shapefileFeatureLayer);

            // Create the symbology for the alternate renderer
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 1.0);
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.Yellow, lineSymbol);

            // Create the alternate renderer
            _alternateRenderer = new SimpleRenderer(fillSymbol);

            try
            {
                // Wait for the layer to load so that it will be assigned a default renderer
                await _shapefileFeatureLayer.LoadAsync();

                // Hold a reference to the default renderer (to enable switching between the renderers)
                _defaultRenderer = _shapefileFeatureLayer.Renderer;

                // Add the map to the mapview
                MyMapView.Map = myMap;

                // Enable changing symbology now that sample is loaded
                MyRendererButton.IsEnabled = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Toggle the renderer
            if (_shapefileFeatureLayer.Renderer == _defaultRenderer)
            {
                _shapefileFeatureLayer.Renderer = _alternateRenderer;
            }
            else
            {
                _shapefileFeatureLayer.Renderer = _defaultRenderer;
            }
        }

        private static string GetShapefilePath()
        {
            return DataManager.GetDataFolder("d98b3e5293834c5f852f13c569930caa", "Subdivisions.shp");
        }
    }
}