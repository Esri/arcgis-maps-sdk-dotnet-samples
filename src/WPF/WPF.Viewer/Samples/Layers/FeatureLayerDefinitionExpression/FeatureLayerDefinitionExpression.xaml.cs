// Copyright 2021 Esri.
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
using Esri.ArcGISRuntime.UI;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.FeatureLayerDefinitionExpression
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Filter by definition expression or display filter",
        category: "Layers",
        description: "Filter features displayed on a map using a definition expression or a display filter.",
        instructions: "Use a definition expression to limit the features requested from the feature layer to those specified by a SQL query. This narrows down the results that are drawn, and removes those features from the layer's attribute table. To filter the results being drawn without modifying the attribute table, hit the button to apply the display filter instead.",
        tags: new[] { "SQL", "definition expression", "display filter", "filter", "limit data", "query", "restrict data", "where clause" })]
    public partial class FeatureLayerDefinitionExpression
    {
        private const string FeatureServerURL = "https://services2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/rest/services/SF_311_Incidents/FeatureServer/0";

        private readonly Viewpoint InitialViewpoint = new Viewpoint(new MapPoint(-122.44014487516885, 37.772296660953138, SpatialReferences.Wgs84), 3350);

        private ManualDisplayFilterDefinition _definition;

        private FeatureLayer _featureLayer;

        public FeatureLayerDefinitionExpression()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create the map.
            MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic) { InitialViewpoint = InitialViewpoint };

            // Add a feature layer to the map.
            ServiceFeatureTable table = new ServiceFeatureTable(new Uri(FeatureServerURL));
            _featureLayer = new FeatureLayer(table);
            MyMapView.Map.OperationalLayers.Add(_featureLayer);

            // Create a display filter and display filter definition.
            // req_type here is one of the published fields
            DisplayFilter damagedTrees = new DisplayFilter(name: $"Damaged Trees", whereClause: "req_type LIKE '%Tree Maintenance%'");
            _definition = new ManualDisplayFilterDefinition(activeFilter: damagedTrees, availableFilters: new[] { damagedTrees });
        }

        private void Expression_Click(object sender, RoutedEventArgs e)
        {
            // Reset the display filter definition.
            _featureLayer.DisplayFilterDefinition = null;

            // Set the definition expression to show specific features only.
            _featureLayer.DefinitionExpression = "req_Type = 'Tree Maintenance or Damage'";
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            // Set the display filter definition on the layer.
            _featureLayer.DisplayFilterDefinition = _definition;

            // Disable the feature layer definition expression
            _featureLayer.DefinitionExpression = string.Empty;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            // Reset the display filter definition.
            _featureLayer.DisplayFilterDefinition = null;

            // Disable the feature layer definition expression
            _featureLayer.DefinitionExpression = string.Empty;
        }

        private void MapDrawStatusChanged(object sender, DrawStatusChangedEventArgs e)
        {
            _ = CountFeatures();
        }

        private async Task CountFeatures()
        {
            try
            {
                // Get the extent of the current viewpoint.
                Envelope extent = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry)?.TargetGeometry?.Extent;

                // Return if no valid extent.
                if (extent == null) return;

                // Update the UI with the count of features in the extent
                long totalIncidentReported = await _featureLayer.FeatureTable.QueryFeatureCountAsync(new QueryParameters() { Geometry = extent });
                IncidentReportSummary.Text = $"Current feature count: {totalIncidentReported}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}