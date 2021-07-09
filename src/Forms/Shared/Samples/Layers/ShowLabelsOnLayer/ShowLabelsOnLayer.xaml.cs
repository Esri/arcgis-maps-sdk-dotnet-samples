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
using Esri.ArcGISRuntime.Mapping.Labeling;
using Esri.ArcGISRuntime.Symbology;
using System;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.ShowLabelsOnLayer
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Show labels on layers",
        category: "Layers",
        description: "Display custom labels on a feature layer.",
        instructions: "Pan and zoom around the United States. Labels for congressional districts will be shown in red for Republican districts and blue for Democrat districts. Notice how labels pop into view as you zoom in.",
        tags: new[] { "arcade", "attribute", "deconfliction", "label", "labeling", "string", "symbol", "text", "visualization" })]
    public partial class ShowLabelsOnLayer : ContentPage
    {
        public ShowLabelsOnLayer()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Create a map with a light gray canvas basemap.
            Map sampleMap = new Map(BasemapStyle.ArcGISLightGray);

            // Assign the map to the MapView.
            MyMapView.Map = sampleMap;

            // Define the URL string for the feature layer.
            string layerUrl = "https://services.arcgis.com/P3ePLMYs2RVChkJx/arcgis/rest/services/USA_115th_Congressional_Districts/FeatureServer/0";

            // Create a service feature table from the URL.
            ServiceFeatureTable featureTable = new ServiceFeatureTable(new Uri(layerUrl));

            // Create a feature layer from the service feature table.
            FeatureLayer districtFeatureLabel = new FeatureLayer(featureTable);

            // Add the feature layer to the operations layers collection of the map.
            sampleMap.OperationalLayers.Add(districtFeatureLabel);

            try
            {
                // Load the feature layer - this way we can obtain it's extent.
                await districtFeatureLabel.LoadAsync();

                // Zoom the map view to the extent of the feature layer.
                await MyMapView.SetViewpointCenterAsync(new MapPoint(-10846309.950860, 4683272.219411, SpatialReferences.WebMercator), 20000000);

                // create label definitions for each party.
                LabelDefinition republicanLabelDefinition = MakeLabelDefinition("Republican", Color.Red);
                LabelDefinition democratLabelDefinition = MakeLabelDefinition("Democrat", Color.Blue);

                // Add the label definition to the feature layer's label definition collection.
                districtFeatureLabel.LabelDefinitions.Add(republicanLabelDefinition);
                districtFeatureLabel.LabelDefinitions.Add(democratLabelDefinition);

                // Enable the visibility of labels to be seen.
                districtFeatureLabel.LabelsEnabled = true;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private LabelDefinition MakeLabelDefinition(string partyName, Color color)
        {
            // Create a text symbol for styling the label.
            TextSymbol textSymbol = new TextSymbol
            {
                Size = 12,
                Color = color,
                HaloColor = Color.White,
                HaloWidth = 2,
            };

            // Create a label expression using an Arcade expression script.
            LabelExpression arcadeLabelExpression = new ArcadeLabelExpression("$feature.NAME + \" (\" + left($feature.PARTY,1) + \")\\nDistrict \" + $feature.CDFIPS");

            return new LabelDefinition(arcadeLabelExpression, textSymbol)
            {
                Placement = Esri.ArcGISRuntime.ArcGISServices.LabelingPlacement.PolygonAlwaysHorizontal,
                WhereClause = $"PARTY = '{partyName}'",
            };
        }
    }
}