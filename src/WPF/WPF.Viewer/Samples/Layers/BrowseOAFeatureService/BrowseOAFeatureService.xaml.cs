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
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Color = System.Drawing.Color;

namespace ArcGIS.WPF.Samples.BrowseOAFeatureService
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Browse OGC API feature service",
        category: "Layers",
        description: "Browse an OGC API feature service for layers and add them to the map.",
        instructions: "Select a layer to display from the list of layers shown in an OGC API service.",
        tags: new[] { "OGC", "OGC API", "browse", "catalog", "feature", "layers", "service", "web" })]
    public partial class BrowseOAFeatureService
    {
        // URL for the OGC feature service.
        private const string ServiceUrl = "https://demo.ldproxy.net/daraa";

        public BrowseOAFeatureService()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Init the UI.
            ServiceTextBox.Text = ServiceUrl;

            // Create the map with topographic basemap.
            MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic);
            _ = LoadService();
        }

        private async Task LoadService()
        {
            try
            {
                LoadingProgressBar.Visibility = Visibility.Visible;
                LoadLayersButton.IsEnabled = false;
                LoadServiceButton.IsEnabled = false;

                // Create the OGC API - Features service using the landing URL.
                OgcFeatureService service = new OgcFeatureService(new Uri(ServiceTextBox.Text));

                // Load the service.
                await service.LoadAsync();

                // Get the service metadata.
                OgcFeatureServiceInfo serviceInfo = service.ServiceInfo;

                // Get a list of available collections.
                IEnumerable<OgcFeatureCollectionInfo> layerListReversed = serviceInfo.FeatureCollectionInfos;

                // Show the layers in the UI.
                OgcFeatureCollectionList.ItemsSource = layerListReversed;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.Message, "Error loading service");
            }
            finally
            {
                // Update the UI.
                LoadingProgressBar.Visibility = Visibility.Collapsed;
                LoadLayersButton.IsEnabled = true;
                LoadServiceButton.IsEnabled = true;
            }
        }

        private async void LoadLayers_Clicked(object sender, RoutedEventArgs e)
        {
            // Skip if nothing selected.
            if (OgcFeatureCollectionList.SelectedItems.Count < 1)
            {
                return;
            }

            // Show the progress bar.
            LoadingProgressBar.Visibility = Visibility.Visible;

            // Clear the existing layers.
            MyMapView.Map.OperationalLayers.Clear();

            try
            {
                // Get the selected collection.
                OgcFeatureCollectionInfo selectedCollectionInfo = (OgcFeatureCollectionInfo)OgcFeatureCollectionList.SelectedItems[0];

                // Create the OGC feature collection table.
                OgcFeatureCollectionTable table = new OgcFeatureCollectionTable(selectedCollectionInfo);

                // Set the feature request mode to manual (only manual is currently supported).
                // In this mode, you must manually populate the table - panning and zooming won't request features automatically.
                table.FeatureRequestMode = FeatureRequestMode.ManualCache;

                // Populate the OGC feature collection table.
                QueryParameters queryParamaters = new QueryParameters();
                queryParamaters.MaxFeatures = 1000;
                await table.PopulateFromServiceAsync(queryParamaters, false, null);

                // Create a feature layer from the OGC feature collection table.
                FeatureLayer ogcFeatureLayer = new FeatureLayer(table);

                // Choose a renderer for the layer based on the table.
                ogcFeatureLayer.Renderer = GetRendererForTable(table) ?? ogcFeatureLayer.Renderer;

                // Add the layer to the map.
                MyMapView.Map.OperationalLayers.Add(ogcFeatureLayer);

                // Zoom to the extent of the selected collection.
                if (selectedCollectionInfo.Extent is Envelope collectionExtent && !collectionExtent.IsEmpty)
                {
                    await MyMapView.SetViewpointGeometryAsync(collectionExtent, 100);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.Message, "Error loading service");
            }
            finally
            {
                // Hide the progress bar.
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private Renderer GetRendererForTable(FeatureTable table)
        {
            switch (table.GeometryType)
            {
                case GeometryType.Point:
                case GeometryType.Multipoint:
                    return new SimpleRenderer(new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Blue, 5));

                case GeometryType.Polygon:
                case GeometryType.Envelope:
                    return new SimpleRenderer(new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.Blue, null));

                case GeometryType.Polyline:
                    return new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Blue, 1));
            }

            return null;
        }

        private void LoadServiceButton_Click(object sender, RoutedEventArgs e)
        {
            _ = LoadService();
        }
    }
}