// Copyright 2023 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Labeling;
using Esri.ArcGISRuntime.Mapping.Popups;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Reduction;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Toolkit.UI.Controls;
using Esri.ArcGISRuntime.UI.Controls;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGIS.WPF.Samples.ConfigureClusters
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Configure clusters",
        category: "Layers",
        description: "Add client side feature reduction on a point feature layer that is not pre-configured with clustering.",
        instructions: "Tap the `Draw clusters` button to set new feature reduction object on the feature layer. Interact with the controls to customize clustering feature reduction properties. Tap on any clustered aggregate geoelement to see the cluster feature count and aggregate fields in the popup.",
        tags: new[] { "aggregate", "bin", "cluster", "group", "merge", "normalize", "popup", "reduce", "renderer", "summarize" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("aa44e79a4836413c89908e1afdace2ea")]
    public partial class ConfigureClusters
    {
        private FeatureLayer _layer;
        private ClusteringFeatureReduction _clusteringFeatureReduction;

        public ConfigureClusters()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Get the Zurich buildings web map from the default portal.
            var portal = await ArcGISPortal.CreateAsync();
            PortalItem portalItem = await PortalItem.CreateAsync(portal, "aa44e79a4836413c89908e1afdace2ea");

            // Create a new map from the web map.
            MyMapView.Map = new Map(portalItem);

            // Get the Zurich buildings feature layer once the map has finished loading.
            await MyMapView.Map.LoadAsync();
            _layer = (FeatureLayer)MyMapView.Map.OperationalLayers.First();

            // Set the initial viewpoint to Zurich, Switzerland.
            await MyMapView.SetViewpointAsync(new Viewpoint(47.38, 8.53, 8e4));

            // Enable the draw clusters button after the layer finishes loading.
            await _layer.LoadAsync();
            DrawClustersButton.IsEnabled = true;
        }

        private void CreateCustomFeatureReduction()
        {
            // Create a class breaks renderer to apply to the custom feature reduction.
            ClassBreaksRenderer classBreaksRenderer = new ClassBreaksRenderer();

            // Define the field to use for the class breaks renderer.
            // Note that this field name must match the name of an aggregate field contained in the clustering feature reduction's aggregate fields property.
            classBreaksRenderer.FieldName = "Average Building Height";

            // Add a class break for each intended value range and define a symbol to display for features in that range.
            // In this case, the average building height ranges from 0 to 8 storeys.
            // For each cluster of features with a given average building height, a symbol is defined with a specified color.
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("0", "0", 0.0, 1.0, new SimpleMarkerSymbol() { Color = System.Drawing.Color.FromArgb(4, 251, 255) }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("1", "1", 1.0, 2.0, new SimpleMarkerSymbol() { Color = System.Drawing.Color.FromArgb(44, 211, 255) }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("2", "2", 2.0, 3.0, new SimpleMarkerSymbol() { Color = System.Drawing.Color.FromArgb(74, 181, 255) }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("3", "3", 3.0, 4.0, new SimpleMarkerSymbol() { Color = System.Drawing.Color.FromArgb(120, 135, 255) }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("4", "4", 4.0, 5.0, new SimpleMarkerSymbol() { Color = System.Drawing.Color.FromArgb(165, 90, 255) }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("5", "5", 5.0, 6.0, new SimpleMarkerSymbol() { Color = System.Drawing.Color.FromArgb(194, 61, 255) }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("6", "6", 6.0, 7.0, new SimpleMarkerSymbol() { Color = System.Drawing.Color.FromArgb(224, 31, 255) }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("7", "7", 7.0, 8.0, new SimpleMarkerSymbol() { Color = System.Drawing.Color.FromArgb(254, 1, 255) }));

            // Define a default symbol to use for features that do not fall within any of the ranges defined by the class breaks.
            classBreaksRenderer.DefaultSymbol = new SimpleMarkerSymbol() { Color = System.Drawing.Color.Pink };

            // Create a new clustering feature reduction using the class breaks renderer.
            _clusteringFeatureReduction = new ClusteringFeatureReduction(classBreaksRenderer);

            // Set the feature reduction's aggregate fields. Note that the field names must match the names of fields in the feature layer's dataset.
            // The aggregate fields summarize values based on the defined aggregate statistic type.
            _clusteringFeatureReduction.AggregateFields.Add(new AggregateField("Total Residential Buildings", "Residential_Buildings", AggregateStatisticType.Sum));
            _clusteringFeatureReduction.AggregateFields.Add(new AggregateField("Average Building Height", "Most_common_number_of_storeys", AggregateStatisticType.Mode));

            // Enable the feature reduction.
            _clusteringFeatureReduction.IsEnabled = true;

            // Set the popup definition for the custom feature reduction.
            _clusteringFeatureReduction.PopupDefinition = PopupDefinition.FromPopupSource(_clusteringFeatureReduction);

            // Set values for the feature reduction's cluster minimum and maximum symbol sizes.
            // Note that the default values for Max and Min symbol size are 70 and 12 respectively.
            _clusteringFeatureReduction.MinSymbolSize = 5;
            _clusteringFeatureReduction.MaxSymbolSize = 90;

            // Set the feature reduction for the layer.
            _layer.FeatureReduction = _clusteringFeatureReduction;

            // Populate the cluster radius and max scale pickers with default values.
            ClusterRadiusPicker.ItemsSource = new double[] { 30, 45, 60, 75, 90 };
            MaxScalePicker.ItemsSource = new double[] { 0, 1000, 5000, 10000, 50000, 100000, 500000 };

            // Set initial picker values.
            // Note that the default value for cluster radius is 60.
            // Increasing the cluster radius increases the number of features that are grouped together into a cluster.
            ClusterRadiusPicker.SelectedValue = _clusteringFeatureReduction.Radius;

            // Note that the default value for max scale is 0.
            // The max scale value is the maximum scale at which clustering is applied.
            MaxScalePicker.SelectedValue = _clusteringFeatureReduction.MaxScale;
        }

        #region EventHandlers

        private void DisplayLabelsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)(sender as CheckBox).IsChecked)
            {
                // Create a label definition with a simple label expression.
                var simpleLabelExpression = new SimpleLabelExpression("[cluster_count]");
                var textSymbol = new TextSymbol() { Color = System.Drawing.Color.Black, Size = 15d, FontWeight = Esri.ArcGISRuntime.Symbology.FontWeight.Bold };
                var labelDefinition = new LabelDefinition(simpleLabelExpression, textSymbol) { Placement = LabelingPlacement.PointCenterCenter };

                // Add the label definition to the feature reduction.
                _clusteringFeatureReduction.LabelDefinitions.Add(labelDefinition);
            }
            else
            {
                _clusteringFeatureReduction.LabelDefinitions.Clear();
            }
        }

        // When a new picker item is selected, update the feature reduction cluster radius.
        private void ClusterRadiusPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_layer != null)
            {
                ((ClusteringFeatureReduction)_layer.FeatureReduction).Radius = (double)ClusterRadiusPicker.SelectedItem;
            }
        }

        // When a new picker item is selected, update the feature reduction max scale.
        private void MaxScalePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_layer != null)
            {
                ((ClusteringFeatureReduction)_layer.FeatureReduction).MaxScale = (double)MaxScalePicker.SelectedValue;
            }
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Identify the tapped observation.
            IdentifyLayerResult result = await MyMapView.IdentifyLayerAsync(MyMapView.Map.OperationalLayers.First(), e.Position, 3, true);

            // Return if no observations were found.
            if (result.Popups.Count == 0) return;

            // Set the popup and make it visible.
            PopupViewer.Popup = result.Popups.FirstOrDefault();
            PopupBackground.Visibility = Visibility.Visible;
        }

        private void DrawClustersButton_Clicked(object sender, RoutedEventArgs e)
        {
            // Create a new clustering feature reduction.
            CreateCustomFeatureReduction();

            // Show the feature reduction's clustering options.
            ClusteringOptions.Visibility = Visibility.Visible;

            // Add an event handler for tap events on the map view.
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;

            // Hide the draw clusters button.
            DrawClustersButton.Visibility = Visibility.Collapsed;
        }

        // Hide and nullify the opened popup when user left clicks.
        private void PopupBackground_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PopupBackground.Visibility = Visibility.Collapsed;
            PopupViewer.Popup = null;
        }

        #endregion EventHandlers
    }
}