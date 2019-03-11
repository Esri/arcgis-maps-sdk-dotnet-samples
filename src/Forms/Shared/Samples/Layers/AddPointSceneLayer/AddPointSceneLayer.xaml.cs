﻿// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.Mapping;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.AddPointSceneLayer
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Add a point scene layer",
        "Layers",
        "View a point scene layer from a scene service.",
        "")]
    public partial class AddPointSceneLayer : ContentPage
    {
        // URL for the service with the point scene layer.
        private const string PointSceneServiceUri = "https://tiles.arcgis.com/tiles/V6ZHFr6zdgNZuVG0/arcgis/rest/services/Airports_PointSceneLayer/SceneServer/layers/0";

        public AddPointSceneLayer()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create the scene.
            MySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Create the layer.
            ArcGISSceneLayer pointSceneLayer = new ArcGISSceneLayer(new Uri(PointSceneServiceUri));

            // Show the layer in the scene.
            MySceneView.Scene.OperationalLayers.Add(pointSceneLayer);
        }
    }
}