// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;

namespace ArcGISRuntime.WPF.Samples.BrowseWfsLayers
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Browse a WFS service for layers",
        "Layers",
        "Browse for layers in a WFS service.",
        "")]
    public partial class BrowseWfsLayers
    {
        private WfsServiceInfo info; // TODO - get rid of this - workaround for .NET bug
        private static Random _rand = new Random();

        public BrowseWfsLayers()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Create the map with imagery basemap.
            MyMapView.Map = new Map(Basemap.CreateImagery());

            // Create the service.
            WfsService service = new WfsService(new Uri("http://qadev000238.esri.com:8070/geoserver/ows?service=wfs&request=GetCapabilities"));

            // Load the service.
            await service.LoadAsync();

            // Show the layers in the UI.
            WfsLayerList.ItemsSource = service.ServiceInfo.LayerInfos;

            // Update the UI.
            LoadingProgressBar.Visibility = Visibility.Collapsed;
            LoadLayersButton.IsEnabled = true;

            info = service.ServiceInfo; //TODO - get rid of this - workaround for .NET memory bug
        }

        private async void LoadLayers_Clicked(object sender, RoutedEventArgs e)
        {
            // Show the progress bar.
            LoadingProgressBar.Visibility = Visibility.Visible;

            // Clear the existing layers.
            MyMapView.Map.OperationalLayers.Clear();

            try
            {
                // Get a list of layer infos
                List<WfsLayerInfo> selectedLayers = WfsLayerList.SelectedItems.Cast<WfsLayerInfo>().ToList();

                // Add each layer to the map.
                foreach (WfsLayerInfo selectedLayerInfo in selectedLayers)
                {
                    // Create the feature table.
                    WfsFeatureTable table = new WfsFeatureTable(selectedLayerInfo);
                    
                    // Set the table's feature request mode.
                    table.FeatureRequestMode = FeatureRequestMode.ManualCache;

                    // Set the axis order based on the UI.
                    if (AxisOrderSwapCheckbox.IsChecked == true)
                    {
                        table.AxisOrder = OgcAxisOrder.Swap;
                    }
                    else
                    {
                        table.AxisOrder = OgcAxisOrder.NoSwap;
                    }

                    // Populate the table.
                    await table.PopulateFromServiceAsync(new QueryParameters(), false, null);

                    // Create a layer from the table.
                    FeatureLayer wfsFeatureLayer = new FeatureLayer(table);

                    // Choose a renderer for the table.
                    wfsFeatureLayer.Renderer = GetRandomRendererForTable(table) ?? wfsFeatureLayer.Renderer;

                    // Add the layer to the map.
                    MyMapView.Map.OperationalLayers.Add(wfsFeatureLayer);
                }

                // Zoom to the extent of all selected layers.
                Envelope selectedEnvelope = GeometryEngine.CombineExtents(selectedLayers.Select(layer => layer.Extent));
                await MyMapView.SetViewpointGeometryAsync(selectedEnvelope, 50);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Failed to load layers.");
                Debug.WriteLine(exception);
            }
            finally
            {
                // Hide the progress bar.
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private Renderer GetRandomRendererForTable(FeatureTable table)
        {
            switch (table.GeometryType)
            {
                case GeometryType.Point:
                case GeometryType.Multipoint:
                    return new SimpleRenderer(new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, GetRandomColor(), 2));
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
    }
}
