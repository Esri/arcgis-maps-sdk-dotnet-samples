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
using System.Collections.ObjectModel;
using Map = Esri.ArcGISRuntime.Mapping.Map;

namespace ArcGIS.Samples.QueryDynamicEntities
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Query dynamic entities",
        category: "Search",
        description: "Find dynamic entities from a data source that match a query.",
        instructions: "Tap the \"Query Flights\" button and select a query to perform from the menu. Once the query is complete, a list of the resulting flights will be displayed. Tap on a flight to see its latest attributes in real-time.",
        tags: new[] { "data", "dynamic", "entity", "live", "query", "real-time", "search", "stream", "track" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("c78e297e99ad4572a48cdcd0b54bed30")]
    public partial class QueryDynamicEntities : ContentPage
    {
        private CustomStreamService _dynamicEntityDataSource;
        private DynamicEntityLayer _dynamicEntityLayer;
        private GraphicsOverlay _bufferGraphicsOverlay;
        private Geometry _phoenixAirportBuffer;
        private ObservableCollection<FlightInfo> _queryResults;
        private readonly MapPoint _phoenixAirport = new MapPoint(-112.0101, 33.4352, SpatialReferences.Wgs84);
        private List<string> _queryOptions;

        public QueryDynamicEntities()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Create the map with a basemap and initial viewpoint.
                MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic);
                await MyMapView.SetViewpointAsync(new Viewpoint(_phoenixAirport, 400000));

                // Create and add a graphics overlay for the buffer around Phoenix Airport (PHX).
                _bufferGraphicsOverlay = new GraphicsOverlay();
                MyMapView.GraphicsOverlays.Add(_bufferGraphicsOverlay);

                _phoenixAirportBuffer = GeometryEngine.BufferGeodetic(
                    _phoenixAirport, 15, LinearUnits.Miles, double.NaN, GeodeticCurveType.Geodesic);

                // Create a semi-transparent fill symbol for the buffer area.
                var bufferSymbol = new SimpleFillSymbol(
                    SimpleFillSymbolStyle.Solid,
                    System.Drawing.Color.FromArgb(40, 255, 0, 0),
                    new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Black, 1));

                //  Add the buffer graphic to the overlay and hide it initially.
                _bufferGraphicsOverlay.Graphics.Add(new Graphic(_phoenixAirportBuffer, bufferSymbol));
                _bufferGraphicsOverlay.IsVisible = false;

                // Initialize the collection for query results and bind it to the results list.
                _queryResults = new ObservableCollection<FlightInfo>();
                ResultsList.ItemsSource = _queryResults;

                // Set up the query options in the dropdown menu.
                _queryOptions = new List<string>
                {
                    "Within 15 Miles of PHX",
                    "Arriving in PHX",
                    "With Flight Number"
                };

                // Bind the query options to the dropdown and set initial visibility states.
                QueryDropdown.ItemsSource = _queryOptions;
                QueryDropdown.SelectedIndex = -1;
                ResultsPanel.IsVisible = false;
                FlightNumberDialog.IsVisible = false;
                QueryDropdown.IsVisible = true;

                // Create and configure the dynamic entity data source and layer.
                await SetupDynamicEntityDataSource();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error initializing sample: {ex.Message}", "OK");
            }
        }

        // Creates and configures the dynamic entity data source and layer for real-time flight tracking.
        private async Task SetupDynamicEntityDataSource()
        {
            try
            {
                // Define the fields expected in the dynamic entity data source.
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

                // Create the data source info with the unique track ID field and spatial reference.
                var dataSourceInfo = new DynamicEntityDataSourceInfo("flight_number", fields)
                {
                    SpatialReference = SpatialReferences.Wgs84
                };

                // Get the path to the JSON data file.
                string dataPath = DataManager.GetDataFolder("c78e297e99ad4572a48cdcd0b54bed30", "phx_air_traffic.json");

                // Initialize the custom stream service with the data source info and file path.
                _dynamicEntityDataSource = new CustomStreamService(dataSourceInfo, dataPath, TimeSpan.FromMilliseconds(100));

                // Create the dynamic entity layer with the data source and configure its display properties.
                _dynamicEntityLayer = new DynamicEntityLayer(_dynamicEntityDataSource)
                {
                    TrackDisplayProperties =
                    {
                        ShowPreviousObservations = true,
                        ShowTrackLine = true,
                        MaximumObservations = 20
                    }
                };

                // Define a simple marker symbol for the dynamic entities (flights).
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

                // Create a simple marker symbol for the flights.
                _dynamicEntityLayer.LabelDefinitions.Add(labelDefinition);
                _dynamicEntityLayer.LabelsEnabled = true;
                MyMapView.Map.OperationalLayers.Add(_dynamicEntityLayer);
                await _dynamicEntityDataSource.ConnectAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error setting up data source: {ex.Message}", "OK");
            }
        }

        // Handles query selection from dropdown and initiates appropriate query type.
        private async void OnQuerySelectionChanged(object sender, EventArgs e)
        {
            if (QueryDropdown.SelectedIndex < 0)
                return;

            var selectedOption = QueryDropdown.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedOption))
                return;

            var queryType = selectedOption switch
            {
                "Within 15 Miles of PHX" => "QueryGeometry",
                "Arriving in PHX" => "QueryAttributes",
                "With Flight Number" => "QueryTrackId",
                _ => null
            };

            if (string.IsNullOrEmpty(queryType))
                return;

            if (queryType == "QueryTrackId")
            {
                QueryDropdown.IsVisible = false;
                FlightNumberDialog.IsVisible = true;
                FlightNumberInput.Focus();
            }
            else
            {
                await PerformQuery(queryType);
            }
        }

        // Processes flight number input and executes track ID query.
        private async void OnFlightNumberQuery(object sender, EventArgs e)
        {
            var flightNumber = FlightNumberInput.Text?.Trim();
            if (string.IsNullOrWhiteSpace(flightNumber))
            {
                await DisplayAlert("Input Required", "Please enter a flight number.", "OK");
                return;
            }

            FlightNumberDialog.IsVisible = false;
            QueryDropdown.IsVisible = false;
            await PerformQuery("QueryTrackId", flightNumber);
            FlightNumberInput.Text = string.Empty;
        }

        // Cancels flight number input and returns to query selection.
        private void OnFlightNumberCancel(object sender, EventArgs e)
        {
            FlightNumberDialog.IsVisible = false;
            FlightNumberInput.Text = string.Empty;
            QueryDropdown.SelectedIndex = -1;
            QueryDropdown.IsVisible = true;
        }

        // Toggles visibility of flight detail information in the results list.
        private void OnToggleDetails(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is FlightInfo flightInfo)
            {
                flightInfo.IsExpanded = !flightInfo.IsExpanded;

                // On iOS, force the CollectionView to update its layout.
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    // Add a small delay to ensure smooth animation.
                    Dispatcher.Dispatch(async () =>
                    {
                        await Task.Delay(10);
                        ResultsList.ScrollTo(flightInfo, position: ScrollToPosition.MakeVisible, animate: true);
                    });
                }
            }
        }

        // Executes the specified query type against the dynamic entity data source.
        private async Task PerformQuery(string queryType, string flightNumber = null)
        {
            try
            {
                _queryResults.Clear();

                if (_dynamicEntityLayer != null)
                {
                    _dynamicEntityLayer.ClearSelection();
                }

                _bufferGraphicsOverlay.IsVisible = false;

                if (_dynamicEntityDataSource == null || _dynamicEntityDataSource.ConnectionStatus != ConnectionStatus.Connected)
                {
                    await DisplayAlert("Not Ready", "Data source is not connected. Please wait for the data to load.", "OK");
                    return;
                }

                // Set up query parameters based on the selected query type.
                var queryParameters = new DynamicEntityQueryParameters();

                switch (queryType)
                {
                    // Query for entities within a 15-mile buffer of Phoenix Airport (PHX).
                    case "QueryGeometry":
                        queryParameters.Geometry = _phoenixAirportBuffer;
                        queryParameters.SpatialRelationship = SpatialRelationship.Intersects;
                        _bufferGraphicsOverlay.IsVisible = true;
                        ResultsDescription.Text = "Flights within 15 miles of PHX";
                        break;

                    // Query for entities with attributes indicating they are arriving at PHX.
                    case "QueryAttributes":
                        queryParameters.WhereClause = "status = 'In flight' AND arrival_airport = 'PHX'";
                        ResultsDescription.Text = "Flights arriving in PHX";
                        break;

                    // Query for a specific entity by its flight number (track ID).
                    case "QueryTrackId":
                        if (!string.IsNullOrEmpty(flightNumber))
                        {
                            queryParameters.TrackIds.Add(flightNumber);
                            ResultsDescription.Text = $"Flight {flightNumber}";
                        }
                        break;

                    default:
                        return;
                }

                IEnumerable<DynamicEntity> results = null;

                // Execute the query against the dynamic entity data source.
                if (_dynamicEntityLayer?.DataSource != null)
                {
                    results = await _dynamicEntityLayer.DataSource.QueryDynamicEntitiesAsync(queryParameters);
                }

                results = results ?? Enumerable.Empty<DynamicEntity>();

                // Process and display the query results.
                if (results.Any())
                {
                    foreach (var entity in results)
                    {
                        if (entity != null)
                        {
                            var flightNum = entity.Attributes["flight_number"]?.ToString();
                            if (!string.IsNullOrEmpty(flightNum))
                            {
                                _dynamicEntityLayer.SelectDynamicEntity(entity);
                                var flightInfo = new FlightInfo(entity);
                                _queryResults.Add(flightInfo);
                            }
                        }
                    }
                }

                // Update UI to show results and hide query options.
                QueryDropdown.IsVisible = false;
                ResultsPanel.IsVisible = true;

                if (!_queryResults.Any())
                {
                    await DisplayAlert("Live Tracking Active",
                        "No flights currently match the criteria, but the list will update as flights enter or leave the area.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Query failed: {ex.Message}", "OK");
                System.Diagnostics.Debug.WriteLine($"Query exception: {ex}");
            }
        }

        // Centers the map view on the selected flight's current position.
        private async void OnResultSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is FlightInfo flight && flight.Entity != null)
            {
                var latestObs = flight.Entity.GetLatestObservation();
                if (latestObs?.Geometry is MapPoint point)
                {
                    await MyMapView.SetViewpointCenterAsync(point, 50000);
                }
            }
        }

        // Closes the results panel and resets the UI to the initial state.
        private void OnCloseResults(object sender, EventArgs e)
        {
            ResultsPanel.IsVisible = false;
            QueryDropdown.IsVisible = true;

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