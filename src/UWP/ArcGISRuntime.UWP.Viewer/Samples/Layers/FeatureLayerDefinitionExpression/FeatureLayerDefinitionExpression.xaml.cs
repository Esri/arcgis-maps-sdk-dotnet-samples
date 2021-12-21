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
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

//using System.Windows.Threading;
using Windows.UI.Xaml;

namespace ArcGISRuntime.UWP.Samples.FeatureLayerDefinitionExpression
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Filter by definition expression or display filter",
        category: "Layers",
        description: "Filter features displayed on a map using a definition expression or a display filter.",
        instructions: "Press the apply expression button to limit the features requested from the feature layer to those specified by the SQL query definition expression. This option not only narrows down the results that are drawn, but also removes those features from the layer's attribute table. To filter the results being drawn without modifying the attribute table, hit the button to apply the filter instead. Click the reset button to remove the definition expression or display filter on the feature layer, which returns all the records.",
        tags: new[] { "SQL", "definition expression", "display filter", "filter", "limit data", "query", "restrict data", "where clause", "Featured" })]
    public partial class FeatureLayerDefinitionExpression
    {
        private const string FeatureServerURL = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/SF311/FeatureServer/0";

        private readonly Viewpoint InitialViewpoint = new Viewpoint(new MapPoint(-122.45044007080793, 37.775915492745874, SpatialReferences.Wgs84), 3350);

        private ManualDisplayFilterDefinition _definition;

        private FeatureLayer _featureLayer;
        private DispatcherTimer _timer;

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
            DisplayFilter damagedTrees = new DisplayFilter($"Damaged Trees", "req_type LIKE '%Tree Maintenance%'");
            _definition = new ManualDisplayFilterDefinition(damagedTrees, new[] { damagedTrees });
        }

        private void Expression_Click(object sender, RoutedEventArgs e)
        {
            // Reset the display filter definition.
            _featureLayer.DisplayFilterDefinition = null;

            // Set the definition expression to show specific features only.
            _featureLayer.DefinitionExpression = "req_Type = 'Tree Maintenance or Damage'";

            CountFeatures();
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            // Disable the feature layer definition expression
            _featureLayer.DefinitionExpression = "";

            // Set the display filter definition on the layer.
            _featureLayer.DisplayFilterDefinition = _definition;

            CountFeatures();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            // Disable the feature layer definition expression
            _featureLayer.DefinitionExpression = "";

            // Reset the display filter definition.
            _featureLayer.DisplayFilterDefinition = null;

            CountFeatures();
        }

        private void OnViewpointChanged(object sender, EventArgs e)
        {
            CountFeatures();
        }

        private void CountFeatures()
        {
            // Use a DispatcherTimer to prevent concurrent feature counts.
            try
            {
                if (_timer == null || !_timer.IsEnabled)
                {
                    if (_timer == null)
                    {
                        _timer = new DispatcherTimer
                        {
                            Interval = TimeSpan.FromSeconds(1),
                        };
                        _timer.Tick += (a, b) =>
                        {
                            _ = QueryFeatureCountAsync();
                            _timer.Stop();
                        };
                    }
                    _timer.Start();
                }
            }
            catch (Exception ex)
            {
                _ = new MessageDialog(ex.Message, ex.GetType().Name).ShowAsync();
            }
        }

        private async Task QueryFeatureCountAsync()
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
                await new MessageDialog(ex.Message, ex.GetType().Name).ShowAsync();
            }
        }
    }
}