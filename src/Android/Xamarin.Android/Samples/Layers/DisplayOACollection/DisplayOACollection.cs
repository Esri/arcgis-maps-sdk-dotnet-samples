// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using Color = System.Drawing.Color;

namespace ArcGISRuntimeXamarin.Samples.DisplayOACollection
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display OGC API collection",
        category: "Layers",
        description: "Display an OGC API feature collection and query features while navigating the map view.",
        instructions: "Pan the map and observe how new features are loaded from the OGC API feature service.",
        tags: new[] { "OGC", "OGC API", "feature", "feature layer", "feature table", "service", "table", "web" })]
    public class DisplayOACollection : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private ProgressBar _loadingProgressBar;

        // Hold a reference to the OGC feature collection table.
        private OgcFeatureCollectionTable _featureTable;

        // Constants for the service URL and collection id.
        private const string ServiceUrl = "https://demo.ldproxy.net/daraa";

        // Note that the service defines the collection id which can be accessed via OgcFeatureCollectionInfo.CollectionId.
        private const string CollectionId = "TransportationGroundCrv";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Display OGC API collection";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Create the map with topographic basemap.
            _myMapView.Map = new Map(BasemapStyle.ArcGISTopographic);

            try
            {
                // Create the feature table from URI and collection id.
                _featureTable = new OgcFeatureCollectionTable(new Uri(ServiceUrl), CollectionId);

                // Set the feature request mode to manual (only manual is currently supported).
                // In this mode, you must manually populate the table - panning and zooming won't request features automatically.
                _featureTable.FeatureRequestMode = FeatureRequestMode.ManualCache;

                // Load the table.
                await _featureTable.LoadAsync();

                // Create a feature layer to visualize the OAFeat features.
                FeatureLayer ogcFeatureLayer = new FeatureLayer(_featureTable);

                // Apply a renderer.
                ogcFeatureLayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Blue, 3));

                // Add the layer to the map.
                _myMapView.Map.OperationalLayers.Add(ogcFeatureLayer);

                // Use the navigation completed event to populate the table with the features needed for the current extent.
                _myMapView.NavigationCompleted += MapView_NavigationCompleted;

                // Zoom to a small area within the dataset by default.
                Envelope datasetExtent = _featureTable.Extent;
                if (datasetExtent != null && !datasetExtent.IsEmpty)
                {
                    await _myMapView.SetViewpointGeometryAsync(new Envelope(datasetExtent.GetCenter(), datasetExtent.Width / 3, datasetExtent.Height / 3));
                }
            }
            catch (Exception e)
            {
                new AlertDialog.Builder(this).SetMessage(e.Message).SetTitle("Error").Show();
            }
        }

        private async void MapView_NavigationCompleted(object sender, EventArgs e)
        {
            // Show the loading bar.
            _loadingProgressBar.Visibility = ViewStates.Visible;

            // Get the current extent.
            Envelope currentExtent = _myMapView.VisibleArea.Extent;

            // Create a query based on the current visible extent.
            QueryParameters visibleExtentQuery = new QueryParameters();
            visibleExtentQuery.Geometry = currentExtent;
            visibleExtentQuery.SpatialRelationship = SpatialRelationship.Intersects;

            // Set a limit of 5000 on the number of returned features per request,
            // because the default on some services could be as low as 10.
            visibleExtentQuery.MaxFeatures = 5000;

            try
            {
                // Populate the table with the query, leaving existing table entries intact.
                // Setting outFields to null requests all fields.
                await _featureTable.PopulateFromServiceAsync(visibleExtentQuery, false, null);
            }
            catch (Exception exception)
            {
                new AlertDialog.Builder(this).SetMessage(exception.Message).SetTitle("Couldn't populate table.").Show();
            }
            finally
            {
                // Hide the loading bar.
                _loadingProgressBar.Visibility = ViewStates.Gone;
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add a help label.
            TextView helpLabel = new TextView(this);
            helpLabel.Gravity = GravityFlags.Center;
            helpLabel.Text = "Pan and zoom to see features.";
            layout.AddView(helpLabel);

            // Add a progress bar.
            _loadingProgressBar = new ProgressBar(this);
            _loadingProgressBar.Indeterminate = true;
            _loadingProgressBar.Visibility = ViewStates.Gone;
            layout.AddView(_loadingProgressBar);

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}