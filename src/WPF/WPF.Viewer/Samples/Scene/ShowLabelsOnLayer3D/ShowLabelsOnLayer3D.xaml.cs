﻿// Copyright 2022 Esri.
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
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Esri.ArcGISRuntime.Mapping.Labeling;
using Esri.ArcGISRuntime.Portal;

namespace ArcGIS.WPF.Samples.ShowLabelsOnLayer3D
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Show labels on layer 3D",
        category: "Scene",
        description: "Display custom labels in a 3D scene.",
        instructions: "Pan and zoom to explore the scene. Notice the labels showing installation dates of features in the 3D gas network.",
        tags: new[] { "portal", "scene", "web scene" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class ShowLabelsOnLayer3D
    {
        // Hold the ID of the portal item, which is a web scene.
        private const string ItemId = "850dfee7d30f4d9da0ebca34a533c169";

        public ShowLabelsOnLayer3D()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Try to load the default portal, which will be ArcGIS Online.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync();

                // Create the portal item.
                PortalItem websceneItem = await PortalItem.CreateAsync(portal, ItemId);

                // Create the scene.
                MySceneView.Scene = new Scene(websceneItem);

                // Load scene to access properties.
                await MySceneView.Scene.LoadAsync();

                // Find the gas layer.
                Layer gasLayer = MySceneView.Scene.OperationalLayers.Single(l => l.Name.Equals("Gas"));

                // Find the main gas sublayer.
                FeatureLayer gasMainLayer = gasLayer.SublayerContents.Single(l => l.Name.Equals("Gas Main")) as FeatureLayer;

                // Clear any existing labels.
                gasMainLayer.LabelDefinitions.Clear();

                // Enable labelling.
                gasMainLayer.LabelsEnabled = true;

                // Create a text symbol for the label definition.
                var textSymbol = new TextSymbol
                {
                    Color = System.Drawing.Color.Orange,
                    HaloColor = System.Drawing.Color.White,
                    HaloWidth = 2,
                    Size = 16
                };

                // Create a label defintion from an arcade label expression and the text symbol.
                var labelDefinition = new LabelDefinition(new ArcadeLabelExpression("Text($feature.INSTALLATIONDATE, `DD MMM YY`)"),
                    textSymbol);

                gasMainLayer.LabelDefinitions.Add(labelDefinition);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample error");
            }
        }
    }
}
