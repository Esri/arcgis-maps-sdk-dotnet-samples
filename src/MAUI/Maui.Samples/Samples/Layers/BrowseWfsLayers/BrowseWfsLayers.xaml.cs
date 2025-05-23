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
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.Symbology;
using System.Diagnostics;

using Color = System.Drawing.Color;

namespace ArcGIS.Samples.BrowseWfsLayers
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Browse WFS layers",
        category: "Layers",
        description: "Browse a WFS service for layers and add them to the map.",
        instructions: "A list of layers in the WFS service will be shown. Select a layer to display.",
        tags: new[] { "OGC", "WFS", "browse", "catalog", "feature", "layers", "service", "web" })]
    public partial class BrowseWfsLayers : ContentPage
    {
        // URL to the WFS service.
        private const string ServiceUrl = "https://dservices2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/services/Seattle_Downtown_Features/WFSServer?service=wfs&request=getcapabilities";

        public BrowseWfsLayers()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Update the UI.
            ServiceTextBox.Text = ServiceUrl;

            // Create the map with imagery basemap.
            MyMapView.Map = new Map(BasemapStyle.ArcGISImageryStandard);

            _ = LoadService();
        }

        private async Task LoadService()
        {
            try
            {
                LoadingProgressBar.IsVisible = true;
                LoadLayersButton.IsEnabled = false;
                LoadServiceButton.IsEnabled = false;

                // Create the WFS service.
                WfsService service = new WfsService(new Uri(ServiceTextBox.Text));

                // Load the WFS service.
                await service.LoadAsync();

                // Get the service metadata.
                WfsServiceInfo serviceInfo = service.ServiceInfo;

                // Get a reversed list of available layers.
                IEnumerable<WfsLayerInfo> layerListReversed = serviceInfo.LayerInfos.Reverse();

                // Show the layers in the UI.
                WfsLayerList.ItemsSource = layerListReversed;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Application.Current.Windows[0].Page.DisplayAlert(ex.Message, "Error loading service", "OK");
            }
            finally
            {
                // Update the UI.
                LoadingProgressBar.IsVisible = false;
                LoadLayersButton.IsEnabled = true;
                LoadServiceButton.IsEnabled = true;
            }
        }

        private async void LoadLayers_Clicked(object sender, EventArgs e)
        {
            // Show the progress bar.
            LoadingProgressBar.IsVisible = true;

            // Clear the existing layers.
            MyMapView.Map.OperationalLayers.Clear();

            try
            {
                // Get the selected WFS layer.
                WfsLayerInfo selectedLayerInfo = (WfsLayerInfo)WfsLayerList.SelectedItem;

                // Create the WFS feature table.
                WfsFeatureTable table = new WfsFeatureTable(selectedLayerInfo);

                // Set the feature request mode to manual - only manual is supported at v100.5.
                // In this mode, you must manually populate the table - panning and zooming won't request features automatically.
                table.FeatureRequestMode = FeatureRequestMode.ManualCache;

                // Set the axis order based on the UI.
                if (AxisOrderSwapCheckbox.IsToggled)
                {
                    table.AxisOrder = OgcAxisOrder.Swap;
                }
                else
                {
                    table.AxisOrder = OgcAxisOrder.NoSwap;
                }

                // Populate the WFS table.
                await table.PopulateFromServiceAsync(new QueryParameters(), false, null);

                // Create a feature layer from the WFS table.
                FeatureLayer wfsFeatureLayer = new FeatureLayer(table);

                // Choose a renderer for the layer based on the table.
                wfsFeatureLayer.Renderer = GetRendererForTable(table) ?? wfsFeatureLayer.Renderer;

                // Add the layer to the map.
                MyMapView.Map.OperationalLayers.Add(wfsFeatureLayer);

                // Zoom to the extent of the selected layer.
                await MyMapView.SetViewpointGeometryAsync(selectedLayerInfo.Extent);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Application.Current.Windows[0].Page.DisplayAlert(ex.Message, "Error loading service", "OK");
            }
            finally
            {
                // Hide the progress bar.
                LoadingProgressBar.IsVisible = false;
            }
        }

        private Renderer GetRendererForTable(FeatureTable table)
        {
            switch (table.GeometryType)
            {
                case GeometryType.Point:
                case GeometryType.Multipoint:
                    return new SimpleRenderer(new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Blue, 4));

                case GeometryType.Polygon:
                case GeometryType.Envelope:
                    return new SimpleRenderer(new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.Blue, null));

                case GeometryType.Polyline:
                    return new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Blue, 1));
            }

            return null;
        }

        private void LoadServiceButton_Click(object sender, EventArgs e)
        {
            _ = LoadService();
        }
    }
}