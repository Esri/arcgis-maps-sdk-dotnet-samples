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
using Map = Esri.ArcGISRuntime.Mapping.Map;

namespace ArcGIS.Samples.CustomFeatureClustering
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        "Custom feature clustering",
        "Layers",
        "Add custom feature clustering to a web map or point feature layer to aggregate points into clusters.",
        "")]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("b6a9b95b86ad4e97b3fe4429f45576f0")]
    public partial class CustomFeatureClustering
    {
        private FeatureLayer _layer;
        private ClusteringFeatureReduction _customClusteringFeatureReduction;

        public CustomFeatureClustering()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            var portal = await ArcGISPortal.CreateAsync();
            PortalItem portalItem = await PortalItem.CreateAsync(portal, "b6a9b95b86ad4e97b3fe4429f45576f0");

            MyMapView.Map = new Map(portalItem);

            await MyMapView.SetViewpointAsync(new Viewpoint(47.3786, 8.5342, 80000));

            await MyMapView.Map.LoadAsync();

            _layer = MyMapView.Map.OperationalLayers.First() as FeatureLayer;

            // Hide and nullify an opened popup when user taps screen.
            PopupBackground.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    PopupBackground.IsVisible = false;
                    PopupViewer.Popup = null;
                })
            });
        }

        private void CreateCustomFeatureReduction()
        {
            // Define a class breaks renderer to apply to the custom feature reduction.
            ClassBreaksRenderer classBreaksRenderer = new ClassBreaksRenderer();

            // Define the field to use for the renderer. Note that this field name must match the field name given to an included AggregateField.
            classBreaksRenderer.FieldName = "Building Height (Mode)";
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("0", "0", 0.0, 1.0, new SimpleMarkerSymbol() { Size = 10, Color = System.Drawing.Color.FromArgb(4, 251, 255) }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("1", "1", 1.0, 2.0, new SimpleMarkerSymbol() { Size = 20, Color = System.Drawing.Color.FromArgb(44, 211, 255) }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("2", "2", 2.0, 3.0, new SimpleMarkerSymbol() { Size = 30, Color = System.Drawing.Color.FromArgb(74, 181, 255) }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("3", "3", 3.0, 4.0, new SimpleMarkerSymbol() { Size = 40, Color = System.Drawing.Color.FromArgb(120, 135, 255) }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("4", "4", 4.0, 5.0, new SimpleMarkerSymbol() { Size = 50, Color = System.Drawing.Color.FromArgb(165, 90, 255) }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("5", "5", 5.0, 6.0, new SimpleMarkerSymbol() { Size = 60, Color = System.Drawing.Color.FromArgb(194, 61, 255) }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("6", "6", 6.0, 7.0, new SimpleMarkerSymbol() { Size = 70, Color = System.Drawing.Color.FromArgb(224, 31, 255) }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("7", "7", 7.0, 8.0, new SimpleMarkerSymbol() { Size = 80, Color = System.Drawing.Color.FromArgb(254, 1, 255) }));

            classBreaksRenderer.DefaultSymbol = new SimpleMarkerSymbol() { Color = System.Drawing.Color.Pink };
            _customClusteringFeatureReduction = new ClusteringFeatureReduction(classBreaksRenderer);
            _customClusteringFeatureReduction.AggregateFields.Add(new AggregateField("Residential Buildings (Sum)", "Residential_Buildings", AggregateStatisticType.Sum));
            _customClusteringFeatureReduction.AggregateFields.Add(new AggregateField("Building Height (Mode)", "Most_common_number_of_storeys", AggregateStatisticType.Mode));
            _customClusteringFeatureReduction.IsEnabled = true;

            // Set the popup definition for the custom feature reduction.
            _customClusteringFeatureReduction.PopupDefinition = PopupDefinition.FromPopupSource(_customClusteringFeatureReduction);

            // Default values for Max and Min symbol size are 70 and 12 respectively.
            _customClusteringFeatureReduction.MinSymbolSize = 30;
            _customClusteringFeatureReduction.MaxSymbolSize = 50;

            // Set the feature reduction for the layer.
            _layer.FeatureReduction = _customClusteringFeatureReduction;

            // Set initial radius slider values.
            RadiusSlider.Value = _customClusteringFeatureReduction.Radius;
        }

        #region EventHandlers
        private void DisplayLabelsCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (DisplayLabelsCheckBox.IsChecked)
            {
                var expr = new SimpleLabelExpression("[cluster_count]");
                var labelSymbol = new TextSymbol() { Color = System.Drawing.Color.Black, Size = 15d, FontWeight = Esri.ArcGISRuntime.Symbology.FontWeight.Bold };
                var labelDef = new LabelDefinition(expr, labelSymbol) { Placement = LabelingPlacement.PointCenterCenter };

                _customClusteringFeatureReduction.LabelDefinitions.Add(labelDef);
            }
            else
            {
                _customClusteringFeatureReduction.LabelDefinitions.Clear();
            }
            
        }

        private void UpdateClusteringProperties(object sender, EventArgs e)
        {
            ((ClusteringFeatureReduction)_layer.FeatureReduction).Radius = RadiusSlider.Value;
            ((ClusteringFeatureReduction)_layer.FeatureReduction).MaxScale = MaxScaleSlider.Value;
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            // Identify the tapped observation.
            IdentifyLayerResult result = await MyMapView.IdentifyLayerAsync(MyMapView.Map.OperationalLayers.First(), e.Position, 3, true);

            // Return if no observations were found.
            if (result.Popups.Count == 0) return;

            // Set the popup and make it visible.
            PopupViewer.Popup = result.Popups.FirstOrDefault();
            PopupBackground.IsVisible = true;
        }

        private void DrawClustersButton_Clicked(object sender, EventArgs e)
        {
            CreateCustomFeatureReduction();
            CustomFeatureClusteringOptions.IsVisible = true;
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
            DrawClustersButton.IsVisible = false;
        }
        #endregion
    }
}