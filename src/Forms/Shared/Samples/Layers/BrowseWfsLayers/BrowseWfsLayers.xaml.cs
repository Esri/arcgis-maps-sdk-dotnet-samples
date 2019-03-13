// Copyright 2019 Esri.
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using Color = System.Drawing.Color;

namespace ArcGISRuntimeXamarin.Samples.BrowseWfsLayers
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Browse a WFS service for layers",
        "Layers",
        "Browse for layers in a WFS service.",
        "")]
    public partial class BrowseWfsLayers : ContentPage
    {
        // URL to the WFS service.
        private const string ServiceUrl = "https://dservices2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/services/Seattle_Downtown_Features/WFSServer?service=wfs&request=getcapabilities";

        public BrowseWfsLayers()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Create the map with imagery basemap.
            MyMapView.Map = new Map(Basemap.CreateImagery());

            // Create the WFS service.
            WfsService service = new WfsService(new Uri(ServiceUrl));

            // Load the WFS service.
            await service.LoadAsync();

            // Get the service metadata.
            WfsServiceInfo serviceInfo = service.ServiceInfo;

            // Get a reversed list of available layers.
            IEnumerable<WfsLayerInfo> layerListReversed = serviceInfo.LayerInfos.Reverse();

            // Show the layers in the UI.
            WfsLayerList.ItemsSource = layerListReversed;

            // Update the UI.
            LoadingProgressBar.IsVisible = false;
            LoadLayersButton.IsEnabled = true;
        }

        private async void LoadLayers_Clicked(object sender, EventArgs e)
        {
            // Show the progress bar.
            LoadingProgressBar.IsVisible = true;

            // Clear the existing layers.
            MyMapView.Map.OperationalLayers.Clear();

            try
            {
                // Add the layer to the map.
                WfsLayerInfo selectedLayerInfo = (WfsLayerInfo) WfsLayerList.SelectedItem;

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
                wfsFeatureLayer.Renderer = GetRandomRendererForTable(table) ?? wfsFeatureLayer.Renderer;

                // Add the layer to the map.
                MyMapView.Map.OperationalLayers.Add(wfsFeatureLayer);

                // Zoom to the extent of the layer.
                await MyMapView.SetViewpointGeometryAsync(selectedLayerInfo.Extent, 50);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                await ((Page)Parent).DisplayAlert("Failed to load layer", exception.ToString(), "OK");
            }
            finally
            {
                // Hide the progress bar.
                LoadingProgressBar.IsVisible = false;
            }
        }

        #region Random symbology

        // Random number generator used to generate random symbology.
        private static readonly Random _rand = new Random();

        private Renderer GetRandomRendererForTable(FeatureTable table)
        {
            switch (table.GeometryType)
            {
                case GeometryType.Point:
                case GeometryType.Multipoint:
                    return new SimpleRenderer(new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, GetRandomColor(), 4));
                case GeometryType.Polygon:
                case GeometryType.Envelope:
                    return new SimpleRenderer(new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, GetRandomColor(180), null));
                case GeometryType.Polyline:
                    return new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, GetRandomColor(), 1));
            }

            return null;
        }

        private Color GetRandomColor(int alpha = 255)
        {
            return Color.FromArgb(alpha, _rand.Next(0, 255), _rand.Next(0, 255), _rand.Next(0, 255));
        }

        #endregion Random symbology
    }
}