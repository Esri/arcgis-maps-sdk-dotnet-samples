// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Labeling;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.ShowLabelsOnLayer3D
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Show labels on layer 3D",
        category: "Scene",
        description: "Display custom labels in a 3D scene.",
        instructions: "Pan and zoom to explore the scene. Notice the labels showing installation dates of features in the 3D gas network.",
        tags: new[] { "3D", "labeling", "scene", "web scene" })]
    public partial class ShowLabelsOnLayer3D
    {
        // Store the link to the web scene.
        private const string ItemUrl = "https://www.arcgis.com/home/item.html?id=850dfee7d30f4d9da0ebca34a533c169";

        public ShowLabelsOnLayer3D()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Set the scene from a web scene.
                MySceneView.Scene = new Scene(new Uri(ItemUrl));

                // Load scene to access properties.
                await MySceneView.Scene.LoadAsync();

                // Find the gas layer, then the gas sublayer.
                Layer gasLayer = MySceneView.Scene.OperationalLayers.Single(l => l.Name.Equals("Gas"));
                FeatureLayer gasMainLayer = gasLayer.SublayerContents.Single(l => l.Name.Equals("Gas Main")) as FeatureLayer;

                gasMainLayer.LabelDefinitions.Clear();

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