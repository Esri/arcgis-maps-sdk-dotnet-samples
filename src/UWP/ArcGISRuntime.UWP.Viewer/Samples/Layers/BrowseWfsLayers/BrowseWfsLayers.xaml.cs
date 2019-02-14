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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.UWP.Samples.BrowseWfsLayers
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Browse a WFS service for layers",
        "Layers",
        "Browse for layers in a WFS service.",
        "")]
    public partial class BrowseWfsLayers
    {
        private WfsServiceInfo info; // TODO - get rid of this - workaround for .NET bug

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

            // Create the service.
            WfsService service = new WfsService(new Uri(ServiceUrl));

            // Load the service.
            await service.LoadAsync();

            // Show the layers in the UI.
            WfsLayerList.ItemsSource = service.ServiceInfo.LayerInfos.Reverse();

            // Update the UI.
            LoadingProgressBar.Visibility = Visibility.Collapsed;
            LoadLayersButton.IsEnabled = true;

            info = service.ServiceInfo; //TODO - get rid of this - workaround for .NET memory bug
        }

        private async void LoadLayers_Clicked(object sender, RoutedEventArgs e)
        {
            // Skip if nothing selected.
            if (WfsLayerList.SelectedItems.Count < 1)
            {
                return;
            }

            // Show the progress bar.
            LoadingProgressBar.Visibility = Visibility.Visible;

            // Clear the existing layers.
            MyMapView.Map.OperationalLayers.Clear();

            try
            {
                // Get the selected layer.
                WfsLayerInfo selectedLayerInfo = (WfsLayerInfo)WfsLayerList.SelectedItems[0];

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

                // Zoom to the extent of the selected layer.
                await MyMapView.SetViewpointGeometryAsync(selectedLayerInfo.Extent, 50);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                await new MessageDialog(exception.ToString(), "Failed to load layer.").ShowAsync();
            }
            finally
            {
                // Hide the progress bar.
                LoadingProgressBar.Visibility = Visibility.Collapsed;
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
        #endregion Random symbology
    }
}
