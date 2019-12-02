﻿// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using System;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.DisplayAnnotation
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display annotation",
        "Layers",
        "Display annotation from a feature service URL.",
        "")]
    public partial class DisplayAnnotation : ContentPage
    {
        public DisplayAnnotation()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Uris for the river data.
            Uri riverFeatureServiceUri = new Uri("https://services1.arcgis.com/6677msI40mnLuuLr/arcgis/rest/services/East_Lothian_Rivers/FeatureServer/0");
            Uri riverFeatureLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/RiversAnnotation/FeatureServer/0");

            // Create a map.
            Map map = new Map(BasemapType.LightGrayCanvasVector, 55.882436, -2.725610, 13);

            // Create a feature layer from a feature service.
            FeatureLayer riverFeatureLayer = new FeatureLayer(new ServiceFeatureTable(riverFeatureServiceUri));

            // Add the feature layer to the map.
            map.OperationalLayers.Add(riverFeatureLayer);

            // Create an annotation layer from a feature service.
            AnnotationLayer annotationLayer = new AnnotationLayer(riverFeatureLayerUri);

            // Add the annotation layer to the map.
            map.OperationalLayers.Add(annotationLayer);

            // Set the map to the map view.
            MyMapView.Map = map;
        }
    }
}