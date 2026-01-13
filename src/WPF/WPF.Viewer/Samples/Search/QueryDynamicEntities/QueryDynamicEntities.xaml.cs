// Copyright 2025 Esri.
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
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Labeling;
using Esri.ArcGISRuntime.RealTime;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGIS.WPF.Samples.QueryDynamicEntities
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Query dynamic entities",
        category: "Search",
        description: "Find dynamic entities from a data source that match a query.",
        instructions: "Tap the \"Query Flights\" button and select a query to perform from the menu. Once the query is complete, a list of the resulting flights will be displayed. Tap on a flight to see its latest attributes in real-time.",
        tags: new[] { "data", "dynamic", "entity", "live", "query", "real-time", "search", "stream", "track" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("c78e297e99ad4572a48cdcd0b54bed30")]
    public partial class QueryDynamicEntities
    {
        private CustomStreamService _dynamicEntityDataSource;
        private DynamicEntityLayer _dynamicEntityLayer;
        private GraphicsOverlay _bufferGraphicsOverlay;
        private Esri.ArcGISRuntime.Geometry.Geometry _phoenixAirportBuffer;
        private ObservableCollection<FlightInfo> _queryResults;
        private readonly MapPoint _phoenixAirport = new MapPoint(-112.0101, 33.4352, SpatialReferences.Wgs84);

        public QueryDynamicEntities()
        {
            InitializeComponent();
            _ = Initialize();
        }

        // Sets up the map, graphics overlay, and data source for flight tracking.
        private async Task Initialize()
        {
            try
            {
                // Create the map with a basemap and initial viewpoint.
                MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic);
                MyMapView.SetViewpoint(new Viewpoint(_phoenixAirport, 400000));

                // Create a graphics overlay for the airport buffer.
                _bufferGraphicsOverlay = new GraphicsOverlay();
                MyMapView.GraphicsOverlays.Add(_bufferGraphicsOverlay);

                // Create a 15-mile geodetic buffer around Phoenix Airport (PHX).
                _phoenixAirportBuffer = GeometryEngine.BufferGeodetic(
                    _phoenixAirport, 15, LinearUnits.Miles, double.NaN, GeodeticCurveType.Geodesic);

                // Create a semi-transparent fill symbol for the buffer and add it to the overlay.
                var bufferSymbol = new SimpleFillSymbol(
                    SimpleFillSymbolStyle.Solid,
                    System.Drawing.Color.FromArgb(40, 255, 0, 0),
                    new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Black, 1));

                // Add the buffer graphic to the overlay and hide it initially.
                _bufferGraphicsOverlay.Graphics.Add(new Graphic(_phoenixAirportBuffer, bufferSymbol));
                _bufferGraphicsOverlay.IsVisible = false;

                // Initialize the collection for query results and bind it to the results list.
                _queryResults = new ObservableCollection<FlightInfo>();
                ResultsList.ItemsSource = _queryResults;

                // Hide the results panel and flight number dialog initially.
                ResultsPanel.Visibility = Visibility.Collapsed;
                FlightNumberDialog.Visibility = Visibility.Collapsed;

                // Set up the dynamic entity data source and layer.
                await SetupDynamicEntityDataSource();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing sample: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Creates and configures the dynamic entity data source and layer for real-time flight tracking.
        private async Task SetupDynamicEntityDataSource()
        {
            try
            {
                // Define the fields for the dynamic entity data source.
                var fields = new List<Field>
                {
                    new Field(FieldType.Text, "flight_number", "", 50),
                    new Field(FieldType.Text, "aircraft", "", 50),
                    new Field(FieldType.Float64, "altitude_feet", "", 0),
                    new Field(FieldType.Text, "arrival_airport", "", 10),
                    new Field(FieldType.Float64, "heading", "", 0),
                    new Field(FieldType.Float64, "speed", "", 0),
                    new Field(FieldType.Text, "status", "", 50)
                };

                var dataSourceInfo = new DynamicEntityDataSourceInfo("flight_number", fields)
                {
                    SpatialReference = SpatialReferences.Wgs84
                };

                // Get the path to the flight data JSON file.
                string dataPath = DataManager.GetDataFolder("c78e297e99ad4572a48cdcd0b54bed30", "phx_air_traffic.json");
                _dynamicEntityDataSource = new CustomStreamService(dataSourceInfo, dataPath, TimeSpan.FromMilliseconds(100));

                // Create and configure the dynamic entity layer.
                _dynamicEntityLayer = new DynamicEntityLayer(_dynamicEntityDataSource)
                {
                    TrackDisplayProperties =
                    {
                        ShowPreviousObservations = true,
                        ShowTrackLine = true,
                        MaximumObservations = 20
                    }
                };

                // Label dynamic entity with its flight number above the point.
                var labelDefinition = new LabelDefinition(
                    new SimpleLabelExpression("[flight_number]"),
                    new TextSymbol
                    {
                        Color = System.Drawing.Color.Red,
                        Size = 12,
                        HaloColor = System.Drawing.Color.White,
                        HaloWidth = 2
                    })
                {
                    Placement = LabelingPlacement.PointAboveCenter
                };

                _dynamicEntityLayer.LabelDefinitions.Add(labelDefinition);
                _dynamicEntityLayer.LabelsEnabled = true;
                MyMapView.Map.OperationalLayers.Add(_dynamicEntityLayer);

                // Load the dynamic entity layer.
                await _dynamicEntityDataSource.ConnectAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up data source: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Handles query selection from dropdown and initiates appropriate query type.
        private async void OnQuerySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = QueryDropdown.SelectedItem as ComboBoxItem;
            if (selectedItem == null)
                return;

            // Determine the query type based on the selected item's tag.
            var queryType = selectedItem.Tag as string;
            if (string.IsNullOrEmpty(queryType))
                return;

            // If querying by track ID, show the flight number input dialog. For other query types, perform the query directly.
            if (queryType == "QueryTrackId")
            {
                FlightNumberDialog.Visibility = Visibility.Visible;
                FlightNumberInput.Focus();
                FlightNumberInput.SelectAll();
            }
            else
            {
                await PerformQuery(queryType);
            }
        }

        // Processes flight number input and executes track ID query.
        private async void OnFlightNumberQuery(object sender, RoutedEventArgs e)
        {
            var flightNumber = FlightNumberInput.Text?.Trim();
            if (string.IsNullOrWhiteSpace(flightNumber))
            {
                MessageBox.Show("Enter a flight number.", "Input Required",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            FlightNumberDialog.Visibility = Visibility.Collapsed;
            await PerformQuery("QueryTrackId", flightNumber);
            FlightNumberInput.Clear();
        }

        // Cancels flight number input and returns to query selection.
        private void OnFlightNumberCancel(object sender, RoutedEventArgs e)
        {
            FlightNumberDialog.Visibility = Visibility.Collapsed;
            FlightNumberInput.Clear();
            QueryDropdown.SelectedIndex = -1;
        }

        // Queries dynamic entities that intersect with the specified geometry and returns the results of the query.
        private async Task<IReadOnlyList<DynamicEntity>> QueryDynamicEntitiesAsync(Geometry geometry)
        {
            // Sets the parameters' geometry and spatial relationship to query within the buffer.
            var parameters = new DynamicEntityQueryParameters
            {
                Geometry = geometry,
                SpatialRelationship = SpatialRelationship.Intersects
            };

            // Performs a dynamic entities query on the data source.
            var queryResult = await _dynamicEntityDataSource.QueryDynamicEntitiesAsync(parameters);

            // Gets the dynamic entities from the query result.
            return queryResult?.ToList();
        }

        // Queries dynamic entities that match the specified attribute where clause and returns the results of the query.
        private async Task<IReadOnlyList<DynamicEntity>> QueryDynamicEntitiesAsync(string whereClause)
        {
            // Sets the parameters' where clause to query the entities' attributes.
            var parameters = new DynamicEntityQueryParameters
            {
                WhereClause = whereClause
            };

            // Performs a dynamic entities query on the data source.
            var queryResult = await _dynamicEntityDataSource.QueryDynamicEntitiesAsync(parameters);

            // Gets the dynamic entities from the query result.
            return queryResult?.ToList();
        }

        // Queries dynamic entities with the specified track IDs and returns the results of the query.
        private async Task<IReadOnlyList<DynamicEntity>> QueryDynamicEntitiesAsync(IEnumerable<string> trackIds)
        {
            // Performs a dynamic entities query on the data source.
            // Use this method when querying only by track IDs.
            var queryResult = await _dynamicEntityDataSource.QueryDynamicEntitiesAsync(trackIds);

            // Gets the dynamic entities from the query result.
            return queryResult?.ToList();
        }

        // Executes the specified query type on the data source.
        private async Task PerformQuery(string queryType, string flightNumber = null)
        {
            _queryResults.Clear();
            _dynamicEntityLayer?.ClearSelection();
            _bufferGraphicsOverlay.IsVisible = false;

            // Holds the dynamic entities returned from the query.
            IReadOnlyList<DynamicEntity> results = Array.Empty<DynamicEntity>();

            switch (queryType)
            {
                case "QueryGeometry":
                    results = await QueryDynamicEntitiesAsync(_phoenixAirportBuffer);
                    // Shows the buffer graphic when querying by geometry.
                    _bufferGraphicsOverlay.IsVisible = true;
                    ResultsDescription.Text = "Flights within 15 miles of PHX";
                    break;

                case "QueryAttributes":
                    results = await QueryDynamicEntitiesAsync("status = 'In flight' AND arrival_airport = 'PHX'");
                    ResultsDescription.Text = "Flights arriving in PHX";
                    break;

                case "QueryTrackId":
                    if (!string.IsNullOrWhiteSpace(flightNumber))
                    {
                        results = await QueryDynamicEntitiesAsync(new[] { flightNumber });
                        ResultsDescription.Text = $"Flight {flightNumber}";
                    }
                    break;
            }

            foreach (var result in results)
            {
                _dynamicEntityLayer.SelectDynamicEntity(result);
                _queryResults.Add(new FlightInfo(result));
            }

            ResultsPanel.Visibility = Visibility.Visible;
        }

        // Centers the map view on the selected flight's current position.
        private void OnResultSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResultsList.SelectedItem is FlightInfo flight && flight.Entity != null)
            {
                var latestObs = flight.Entity.GetLatestObservation();
                if (latestObs?.Geometry is MapPoint point)
                {
                    _ = MyMapView.SetViewpointCenterAsync(point, 50000);
                }
            }
        }

        // Closes the results panel and resets the UI to the initial state.
        private void OnCloseResults(object sender, RoutedEventArgs e)
        {
            ResultsPanel.Visibility = Visibility.Collapsed;

            _queryResults.Clear();

            if (_dynamicEntityLayer != null)
            {
                _dynamicEntityLayer.ClearSelection();
            }

            _bufferGraphicsOverlay.IsVisible = false;
            QueryDropdown.SelectedIndex = -1;
        }
    }
}