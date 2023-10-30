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
    [ArcGIS.Samples.Shared.Attributes.OfflineData("576b21169b1f40eb9e4e8b27defcb94c")]
    public partial class CustomFeatureClustering
    {
        private FeatureLayer _webMapLayer;
        private FeatureReduction _honoredWebMapFeatureReduction;
        private ClusteringFeatureReduction _customClusteringFeatureReduction;

        public CustomFeatureClustering()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            var portal = await ArcGISPortal.CreateAsync();
            PortalItem portalItem = await PortalItem.CreateAsync(portal, "576b21169b1f40eb9e4e8b27defcb94c");

            MyMapView.Map = new Map(portalItem);

            await MyMapView.Map.LoadAsync();

            _webMapLayer = MyMapView.Map.OperationalLayers.First() as FeatureLayer;

            await _webMapLayer.FeatureTable.LoadAsync();

            _webMapLayer.FeatureReduction.IsEnabled = false;
            _honoredWebMapFeatureReduction = _webMapLayer.FeatureReduction;

            ClusteringPicker.ItemsSource = new List<string> { "Honor web map clustering", "Custom feature clustering" };
            ClusteringPicker.SelectedIndex = 0;

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
            ClassBreaksRenderer classBreaksRenderer = new ClassBreaksRenderer();
            classBreaksRenderer.FieldName = "Max_Residential_Buildings";
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("1 to 10", "1 to 10", 1, 10, new SimpleMarkerSymbol() { Size = 10, Color = System.Drawing.Color.Yellow }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("10 to 20", "10 to 20", 10, 20, new SimpleMarkerSymbol() { Size = 20, Color = System.Drawing.Color.Orange }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("20 to 30", "20 to 30", 20, 30, new SimpleMarkerSymbol() { Size = 30, Color = System.Drawing.Color.Red }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("30 to 40", "30 to 40", 30, 40, new SimpleMarkerSymbol() { Size = 40, Color = System.Drawing.Color.Maroon }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("40 to 50", "40 to 50", 40, 50, new SimpleMarkerSymbol() { Size = 50, Color = System.Drawing.Color.MediumPurple }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("50 to 60", "50 to 60", 50, 60, new SimpleMarkerSymbol() { Size = 60, Color = System.Drawing.Color.Purple }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("60 to 70", "60 to 70", 60, 70, new SimpleMarkerSymbol() { Size = 70, Color = System.Drawing.Color.Blue }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("70 to 80", "70 to 80", 70, 80, new SimpleMarkerSymbol() { Size = 80, Color = System.Drawing.Color.Green }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("80 to 90", "80 to 90", 80, 90, new SimpleMarkerSymbol() { Size = 90, Color = System.Drawing.Color.Tan }));
            classBreaksRenderer.ClassBreaks.Add(new ClassBreak("90 to 100", "90 to 100", 90, 200, new SimpleMarkerSymbol() { Size = 100, Color = System.Drawing.Color.Magenta }));

            classBreaksRenderer.DefaultSymbol = new SimpleMarkerSymbol() { Size = 30, Color = System.Drawing.Color.Pink };
            _customClusteringFeatureReduction = new ClusteringFeatureReduction(classBreaksRenderer);
            _customClusteringFeatureReduction.AggregateFields.Add(new AggregateField("Max_Residential_Buildings", "Residential_Buildings", AggregateStatisticType.Max));
            _customClusteringFeatureReduction.AggregateFields.Add(new AggregateField("Most_common_number_of_storeys", "Most_common_number_of_storeys", AggregateStatisticType.Mode));
            _customClusteringFeatureReduction.IsEnabled = false;
            _customClusteringFeatureReduction.PopupDefinition = PopupDefinition.FromPopupSource(_customClusteringFeatureReduction);
        }

        #region EventHandlers
        private void ClusteringPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            string clusteringOption = (string)ClusteringPicker.SelectedItem;
            switch (clusteringOption)
            {
                case "Custom feature clustering":

                    if (_customClusteringFeatureReduction == null)
                    {
                        CreateCustomFeatureReduction();
                    }

                    _webMapLayer.FeatureReduction = _customClusteringFeatureReduction;
                    MyMapView.GeoViewTapped -= MyMapView_GeoViewTapped;
                    break;
                default:
                    _webMapLayer.FeatureReduction = _honoredWebMapFeatureReduction;
                    MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
                    break;
            }

            EnableClusteringCheckBox.IsChecked = false;
            CustomFeatureClusteringOptions.IsVisible = false;
        }

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

        private void EnableClusteringCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            _webMapLayer.FeatureReduction.IsEnabled = EnableClusteringCheckBox.IsChecked;
            CustomFeatureClusteringOptions.IsVisible = ClusteringPicker.SelectedIndex != 0;
        }

        private void UpdateClusteringProperties(object sender, EventArgs e)
        {
            ((ClusteringFeatureReduction)_webMapLayer.FeatureReduction).Radius = RadiusSlider.Value;
            ((ClusteringFeatureReduction)_webMapLayer.FeatureReduction).MaxSymbolSize = MaxSymbolSizeSlider.Value;
            ((ClusteringFeatureReduction)_webMapLayer.FeatureReduction).MinSymbolSize = MinSymbolSizeSlider.Value;
            ((ClusteringFeatureReduction)_webMapLayer.FeatureReduction).MaxScale = MaxScaleSlider.Value;
        }

        private void EnablePopupsCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            ((ClusteringFeatureReduction)_webMapLayer.FeatureReduction).IsPopupEnabled = EnablePopupsCheckBox.IsChecked;
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            // Identify the tapped observation.
            IdentifyLayerResult results = await MyMapView.IdentifyLayerAsync(MyMapView.Map.OperationalLayers.First(), e.Position, 3, true);

            // Return if no observations were found.
            if (results.Popups.Count == 0) return;

            // Set the popup and make it visible.
            PopupViewer.Popup = results.Popups.FirstOrDefault();
            PopupBackground.IsVisible = true;
        }
        #endregion
    }
}