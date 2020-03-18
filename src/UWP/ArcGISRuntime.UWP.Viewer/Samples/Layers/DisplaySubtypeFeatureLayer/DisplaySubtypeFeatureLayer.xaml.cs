﻿// Copyright 2019 Esri.
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
using System.Drawing;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.UWP.Samples.DisplaySubtypeFeatureLayer
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display subtype feature layer",
        "Layers",
        "Displays a composite layer of all the subtype values in a feature class.",
        "",
        "Featured")]
    public partial class DisplaySubtypeFeatureLayer
    {
        // Reference to a sublayer.
        private SubtypeSublayer _sublayer;

        // JSON for labeling features from the sublayer.
        private const string _labelJSON = "{ \"labelExpression\":\"[nominalvoltage]\",\"labelPlacement\":\"esriServerPointLabelPlacementAboveRight\",\"useCodedValues\":true,\"symbol\":{\"angle\":0,\"backgroundColor\":[0,0,0,0],\"borderLineColor\":[0,0,0,0],\"borderLineSize\":0,\"color\":[0,0,255,255],\"font\":{\"decoration\":\"none\",\"size\":10.5,\"style\":\"normal\",\"weight\":\"normal\"},\"haloColor\":[255,255,255,255],\"haloSize\":2,\"horizontalAlignment\":\"center\",\"kerning\":false,\"type\":\"esriTS\",\"verticalAlignment\":\"middle\",\"xoffset\":0,\"yoffset\":0}}";

        // Renderers for the sublayer.
        private Renderer _defaultRenderer;
        private Renderer _customRenderer;

        public DisplaySubtypeFeatureLayer()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Starting viewpoint for the map view.
                Viewpoint _startingViewpoint = new Viewpoint(new Envelope(-9812691.11079696, 5128687.20710657, -9812377.9447607, 5128865.36767282, SpatialReferences.WebMercator));

                // Create the map.
                MyMapView.Map = new Map(Basemap.CreateStreetsNightVector()) { InitialViewpoint = _startingViewpoint };

                // NOTE: This layer supports any ArcGIS Feature Table that define subtypes.
                SubtypeFeatureLayer subtypeFeatureLayer = new SubtypeFeatureLayer(new ServiceFeatureTable(new Uri("https://sampleserver7.arcgisonline.com/arcgis/rest/services/UtilityNetwork/NapervilleElectric/FeatureServer/100")));
                MyMapView.Map.OperationalLayers.Add(subtypeFeatureLayer);

                // Select sublayer to control.
                await subtypeFeatureLayer.LoadAsync();

                // Select the sublayer of street lights by name.
                _sublayer = subtypeFeatureLayer.GetSublayerBySubtypeName("Street Light");

                // Set the label definitions using the JSON.
                _sublayer.LabelDefinitions.Add(LabelDefinition.FromJson(_labelJSON));

                // Enable labels for the sub layer.
                _sublayer.LabelsEnabled = true;

                // Set the data context for data-binding in XAML.
                SublayerInfo.DataContext = _sublayer;

                // Get the default renderer for the sublayer.
                _defaultRenderer = _sublayer.Renderer.Clone();

                // Create a custom renderer for the sublayer.
                _customRenderer = new SimpleRenderer()
                {
                    Symbol = new SimpleMarkerSymbol()
                    {
                        Color = Color.Salmon,
                        Style = SimpleMarkerSymbolStyle.Diamond,
                        Size = 20,
                    }
                };

                // Set a default minimum scale.
                _sublayer.MinScale = 3000;
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.Message, ex.GetType().Name).ShowAsync();
            }
        }

        private void OnChangeRenderer(object sender, RoutedEventArgs e)
        {
            // Check if the current renderer is the custom renderer.
            if (_sublayer.Renderer == _customRenderer)
            {
                _sublayer.Renderer = _defaultRenderer;
            }
            else
            {
                _sublayer.Renderer = _customRenderer;
            }
        }

        private void OnSetMinimumScale(object sender, RoutedEventArgs e)
        {
            // Set the minimum scale of the sublayer.
            // NOTE: You may also update Sublayer.MaxScale
            _sublayer.MinScale = MyMapView.MapScale;
        }
    }
}