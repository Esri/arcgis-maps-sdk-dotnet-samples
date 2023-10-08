// Copyright 2023 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Labeling;
using Esri.ArcGISRuntime.RealTime;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;
using System.Text;
using System.Windows;

namespace ArcGIS.WPF.Samples.AddCustomDynamicEntityDataSource
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Add custom dynamic entity data source",
        category: "Layers",
        description: "Create a custom dynamic entity data source and display it using a dynamic entity layer.",
        instructions: "Run the sample to view the map and the dynamic entity layer displaying the latest observation from the custom data source. Tap on a dynamic entity to view its attributes in a callout.",
        tags: new[] { "data", "dynamic", "entity", "label", "labeling", "live", "real-time", "stream", "track" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("a8a942c228af4fac96baa78ad60f511f")]
    public partial class AddCustomDynamicEntityDataSource
    {
        // Path to AIS Traffic Data json file.
        private readonly string _localJsonFile = DataManager.GetDataFolder("a8a942c228af4fac96baa78ad60f511f", "AIS_MarineCadastre_SelectedVessels_CustomDataSource.jsonl");

        public AddCustomDynamicEntityDataSource()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create a new map with the navigation basemap style.
            MyMapView.Map = new Map(BasemapStyle.ArcGISOceans);

            // Set the initial viewpoint.
            MyMapView.SetViewpoint(new Viewpoint(47.984, -123.657, 3e6));

            // Create a new custom file source.
            // This takes the path to the simulation file, field name that will be used as the entity id, and the delay between each observation that is processed.
            // In this example we are using a json file as our custom data source.
            // This field value should be a unique identifier for each entity.
            // Adjusting the value for the delay will change the speed at which the entities and their observations are displayed.
            var customSource = new SimulatedDataSource(_localJsonFile, "MMSI", TimeSpan.FromMilliseconds(10));

            // Create the dynamic entity layer using the custom data source.
            var dynamicEntityLayer = new DynamicEntityLayer(customSource);

            // Set up the track display properties.
            SetupTrackDisplayProperties(dynamicEntityLayer);

            // Set up the dynamic entity labeling.
            SetupLabeling(dynamicEntityLayer);

            // Add the dynamic entity layer to the map.
            MyMapView.Map.OperationalLayers.Add(dynamicEntityLayer);
        }

        private void SetupTrackDisplayProperties(DynamicEntityLayer layer)
        {
            // Set up the track display properties, these properties will be used to configure the appearance of the track line and previous observations.
            layer.TrackDisplayProperties.ShowPreviousObservations = true;
            layer.TrackDisplayProperties.ShowTrackLine = true;
            layer.TrackDisplayProperties.MaximumObservations = 20;
        }

        private void SetupLabeling(DynamicEntityLayer layer)
        {
            // Define the label expression to be used, in this case we will use the "VesselName" for each of the dynamic entities.
            var simpleLabelExpression = new SimpleLabelExpression("[VesselName]");

            // Set the text symbol color and size for the labels.
            var labelSymbol = new TextSymbol() { Color = System.Drawing.Color.Red, Size = 12d };

            // Set the label position.
            var labelDef = new LabelDefinition(simpleLabelExpression, labelSymbol) { Placement = LabelingPlacement.PointAboveCenter };

            // Add the label definition to the dynamic entity layer and enable labels.
            layer.LabelDefinitions.Add(labelDef);
            layer.LabelsEnabled = true;
        }

        private async void GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            e.Handled = true;
            try
            {
                MyMapView.DismissCallout();

                // If no dynamic entity layer is present in the map, return.
                var layer = MyMapView.Map?.OperationalLayers.OfType<DynamicEntityLayer>().FirstOrDefault();
                if (layer is null) return;

                // Identify the tapped observation.
                IdentifyLayerResult results = await MyMapView.IdentifyLayerAsync(layer, e.Position, 2d, false);
                DynamicEntityObservation observation = results.GeoElements.FirstOrDefault() as DynamicEntityObservation;
                if (observation is null) return;

                // Get the dynamic entity from the observation.
                var dynamicEntity = observation.GetDynamicEntity();
                if (dynamicEntity is null) return;

                // Build a string for observation attributes.
                var stringBuilder = new StringBuilder();
                foreach (var attribute in new string[] { "VesselName", "CallSign", "COG", "SOG" })
                {
                    var value = dynamicEntity.Attributes[attribute].ToString();

                    // Account for when an attribue has an empty value.
                    if (!string.IsNullOrEmpty(value))
                    {
                        stringBuilder.AppendLine(attribute + ": " + value);
                    }
                }

                // The standard callout takes care of moving when the dynamic entity changes.
                var calloutDef = new CalloutDefinition(stringBuilder.ToString().TrimEnd());
                if (layer.Renderer?.GetSymbol(dynamicEntity) is Symbol symbol)
                {
                    await calloutDef.SetIconFromSymbolAsync(symbol);
                }

                // Show the callout for the tapped dynamic entity.
                MyMapView.ShowCalloutForGeoElement(dynamicEntity, e.Position, calloutDef);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error identifying dynamic entity.");
            }
        }
    }
}