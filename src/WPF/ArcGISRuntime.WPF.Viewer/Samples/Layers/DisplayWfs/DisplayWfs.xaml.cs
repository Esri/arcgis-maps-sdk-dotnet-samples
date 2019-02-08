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
using System.Windows;
using Color = System.Drawing.Color;

namespace ArcGISRuntime.WPF.Samples.DisplayWfs
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display a WFS layer",
        "Layers",
        "Display a layer from a WFS service, requesting only features for the current extent.",
        "")]
    public partial class DisplayWfs
    {
        private WfsFeatureTable _featureTable;
        public DisplayWfs()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Create the map with topographic basemap.
            MyMapView.Map = new Map(Basemap.CreateTopographic());

            try
            {
                // Create the feature table from URI and layer name.
                _featureTable = new WfsFeatureTable(
                    new Uri("http://qadev000238.esri.com:8070/geoserver/ows?service=wfs&request=GetCapabilities"), 
                    "tiger:tiger_roads");

                // Set the feature request mode to manual - only manual is supported at v100.5.
                _featureTable.FeatureRequestMode = FeatureRequestMode.ManualCache;

                // Load the table.
                await _featureTable.LoadAsync();

                // Create a feature layer to visualize the WFS features.
                FeatureLayer manhattanFeatureLayer = new FeatureLayer(_featureTable);

                // Apply a renderer.
                manhattanFeatureLayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 3));

                // Add the layer to the map.
                MyMapView.Map.OperationalLayers.Add(manhattanFeatureLayer);

                // Use the navigation completed event to populate the table with the features needed for the current extent.
                MyMapView.NavigationCompleted += MapView_NavigationCompleted;
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

            try
            {
                // Populate the table with the query, leaving existing table entries intact.
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
