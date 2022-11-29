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
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Color = System.Drawing.Color;

namespace ArcGIS.WPF.Samples.DisplayWfs
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display WFS layer",
        category: "Layers",
        description: "Display a layer from a WFS service, requesting only features for the current extent.",
        instructions: "Pan and zoom to see features within the current map extent.",
        tags: new[] { "OGC", "WFS", "browse", "catalog", "feature", "interaction cache", "layers", "service", "web" })]
    public partial class DisplayWfs
    {
        // Hold a reference to the WFS feature table.
        private WfsFeatureTable _featureTable;

        // Constants for the service URL and layer name.
        private const string ServiceUrl = "https://dservices2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/services/Seattle_Downtown_Features/WFSServer?service=wfs&request=getcapabilities";

        // Note that the layer name is defined by the service. The layer name can be accessed via WfsLayerInfo.Name.
        private const string LayerName = "Seattle_Downtown_Features:Buildings";

        public DisplayWfs()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create the map with topographic basemap.
            MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic);

            try
            {
                // Create the feature table from URI and layer name.
                _featureTable = new WfsFeatureTable(new Uri(ServiceUrl), LayerName);

                // Set the feature request mode to manual - only manual is supported at v100.5.
                // In this mode, you must manually populate the table - panning and zooming won't request features automatically.
                _featureTable.FeatureRequestMode = FeatureRequestMode.ManualCache;

                // Load the table.
                await _featureTable.LoadAsync();

                // Create a feature layer to visualize the WFS features.
                FeatureLayer wfsFeatureLayer = new FeatureLayer(_featureTable);

                // Apply a renderer.
                wfsFeatureLayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 3));

                // Add the layer to the map.
                MyMapView.Map.OperationalLayers.Add(wfsFeatureLayer);

                // Use the navigation completed event to populate the table with the features needed for the current extent.
                MyMapView.NavigationCompleted += MapView_NavigationCompleted;

                // Zoom to a small area within the dataset by default.
                MapPoint topLeft = new MapPoint(-122.341581, 47.617207, SpatialReferences.Wgs84);
                MapPoint bottomRight = new MapPoint(-122.332662, 47.613758, SpatialReferences.Wgs84);
                await MyMapView.SetViewpointGeometryAsync(new Envelope(topLeft, bottomRight));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Couldn't load sample.");
                Debug.WriteLine(e);
            }
        }

        private async void MapView_NavigationCompleted(object sender, EventArgs e)
        {
            // Show the loading bar.
            LoadingProgressbar.Visibility = Visibility.Visible;

            // Get the current extent.
            Envelope currentExtent = MyMapView.VisibleArea.Extent;

            // Create a query based on the current visible extent.
            QueryParameters visibleExtentQuery = new QueryParameters();
            visibleExtentQuery.Geometry = currentExtent;
            visibleExtentQuery.SpatialRelationship = SpatialRelationship.Intersects;

            try
            {
                // Populate the table with the query, leaving existing table entries intact.
                // Setting outFields to null requests all features.
                await _featureTable.PopulateFromServiceAsync(visibleExtentQuery, false, null);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Couldn't populate table.");
                Debug.WriteLine(exception);
            }
            finally
            {
                // Hide the loading bar.
                LoadingProgressbar.Visibility = Visibility.Collapsed;
            }
        }
    }
}