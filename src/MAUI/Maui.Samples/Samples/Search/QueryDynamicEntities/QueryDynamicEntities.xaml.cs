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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
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

        // Sets up the map, graphics overlays, UI controls, and data source for flight tracking
        private async Task Initialize()
        {
            try
            {
                MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic);
                await MyMapView.SetViewpointAsync(new Viewpoint(_phoenixAirport, 400000));

                _bufferGraphicsOverlay = new GraphicsOverlay();
                MyMapView.GraphicsOverlays.Add(_bufferGraphicsOverlay);

                _phoenixAirportBuffer = GeometryEngine.BufferGeodetic(
                    _phoenixAirport, 15, LinearUnits.Miles, double.NaN, GeodeticCurveType.Geodesic);

                var bufferSymbol = new SimpleFillSymbol(
                    SimpleFillSymbolStyle.Solid,
                    System.Drawing.Color.FromArgb(40, 255, 0, 0),
                    new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Black, 1));

                _bufferGraphicsOverlay.Graphics.Add(new Graphic(_phoenixAirportBuffer, bufferSymbol));
                _bufferGraphicsOverlay.IsVisible = false;

                _queryResults = new ObservableCollection<FlightInfo>();
                ResultsList.ItemsSource = _queryResults;

                _queryOptions = new List<string>
                {
                    "Within 15 Miles of PHX",
                    "Arriving in PHX",
                    "With Flight Number"
                };
                QueryDropdown.ItemsSource = _queryOptions;
                QueryDropdown.SelectedIndex = -1;
                ResultsPanel.IsVisible = false;
                FlightNumberDialog.IsVisible = false;
                QueryDropdown.IsVisible = true;

                await SetupDynamicEntityDataSource();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error initializing sample: {ex.Message}", "OK");
            }
        }

        // Creates and configures the dynamic entity data source and layer for real-time flight tracking
        private async Task SetupDynamicEntityDataSource()
        {
            try
            {
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

                string dataPath = DataManager.GetDataFolder("c78e297e99ad4572a48cdcd0b54bed30", "phx_air_traffic.json");

                _dynamicEntityDataSource = new CustomStreamService(dataSourceInfo, dataPath, TimeSpan.FromMilliseconds(100));

                _dynamicEntityLayer = new DynamicEntityLayer(_dynamicEntityDataSource)
                {
                    TrackDisplayProperties =
                    {
                        ShowPreviousObservations = true,
                        ShowTrackLine = true,
                        MaximumObservations = 20
                    }
                };

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
                await _dynamicEntityDataSource.ConnectAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error setting up data source: {ex.Message}", "OK");
            }
        }

        // Handles query selection from dropdown and initiates appropriate query type
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

        // Processes flight number input and executes track ID query
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

        // Cancels flight number input and returns to query selection
        private void OnFlightNumberCancel(object sender, EventArgs e)
        {
            FlightNumberDialog.IsVisible = false;
            FlightNumberInput.Text = string.Empty;
            QueryDropdown.SelectedIndex = -1;
            QueryDropdown.IsVisible = true;
        }

        // Toggles visibility of flight detail information in the results list
        private void OnToggleDetails(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is FlightInfo flightInfo)
            {
                flightInfo.IsExpanded = !flightInfo.IsExpanded;
            }
        }

        // Executes the specified query type against the dynamic entity data source
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

                var queryParameters = new DynamicEntityQueryParameters();

                switch (queryType)
                {
                    case "QueryGeometry":
                        queryParameters.Geometry = _phoenixAirportBuffer;
                        queryParameters.SpatialRelationship = SpatialRelationship.Intersects;
                        _bufferGraphicsOverlay.IsVisible = true;
                        ResultsDescription.Text = "Flights within 15 miles of PHX";
                        break;

                    case "QueryAttributes":
                        queryParameters.WhereClause = "status = 'In flight' AND arrival_airport = 'PHX'";
                        ResultsDescription.Text = "Flights arriving in PHX";
                        break;

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

                if (_dynamicEntityLayer?.DataSource != null)
                {
                    results = await _dynamicEntityLayer.DataSource.QueryDynamicEntitiesAsync(queryParameters);
                }

                results = results ?? Enumerable.Empty<DynamicEntity>();

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

        // Centers the map view on the selected flight's current position
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

        // Closes the results panel and resets the UI to the initial state
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

        private class CustomStreamService : DynamicEntityDataSource
        {
            private readonly DynamicEntityDataSourceInfo _info;
            private readonly string _dataPath;
            private readonly TimeSpan _delay;
            private CancellationTokenSource _cancellationTokenSource;
            private Task _dataFeedTask;

            //Initializes the custom stream service with data source info and file path
            public CustomStreamService(DynamicEntityDataSourceInfo info, string dataPath, TimeSpan delay)
            {
                _info = info;
                _dataPath = dataPath;
                _delay = delay;
            }

            //Returns the data source information while loading the service
            protected override Task<DynamicEntityDataSourceInfo> OnLoadAsync() =>
                Task.FromResult(_info);

            //Starts the data feed processing task when connecting to the service
            protected override Task OnConnectAsync(CancellationToken cancellationToken)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _dataFeedTask = ProcessDataFeedAsync(_cancellationTokenSource.Token);
                return Task.CompletedTask;
            }

            //Cancels the data feed processing task and cleans up resources when disconnecting from the service
            protected override async Task OnDisconnectAsync()
            {
                _cancellationTokenSource?.Cancel();

                if (_dataFeedTask != null)
                {
                    try
                    {
                        await _dataFeedTask;
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }

                _cancellationTokenSource?.Dispose();
            }

            // Reads and processes flight data from JSON file line by line with delays
            private async Task ProcessDataFeedAsync(CancellationToken cancellationToken)
            {
                try
                {
                    if (!File.Exists(_dataPath))
                        throw new FileNotFoundException("Flight data file not found.", _dataPath);

                    var lines = await File.ReadAllLinesAsync(_dataPath, cancellationToken);

                    foreach (var line in lines)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        try
                        {
                            using (JsonDocument document = JsonDocument.Parse(line))
                            {
                                var root = document.RootElement;

                                if (root.TryGetProperty("geometry", out JsonElement geometryElement))
                                {
                                    double x = 0, y = 0;

                                    if (geometryElement.TryGetProperty("x", out JsonElement xElement))
                                        x = xElement.GetDouble();

                                    if (geometryElement.TryGetProperty("y", out JsonElement yElement))
                                        y = yElement.GetDouble();

                                    var point = new MapPoint(x, y, SpatialReferences.Wgs84);
                                    var attributes = new Dictionary<string, object>();

                                    if (root.TryGetProperty("attributes", out JsonElement attributesElement))
                                    {
                                        foreach (var property in attributesElement.EnumerateObject())
                                        {
                                            object value = property.Value.ValueKind switch
                                            {
                                                JsonValueKind.String => property.Value.GetString(),
                                                JsonValueKind.Number when property.Value.TryGetInt32(out int intValue) => intValue,
                                                JsonValueKind.Number => property.Value.GetDouble(),
                                                JsonValueKind.True => true,
                                                JsonValueKind.False => false,
                                                JsonValueKind.Null => null,
                                                _ => property.Value.ToString()
                                            };

                                            attributes[property.Name] = value;
                                        }
                                    }
                                    AddObservation(point, attributes);
                                }
                            }
                            await Task.Delay(_delay, cancellationToken);
                        }
                        catch (JsonException ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error parsing JSON line: {ex.Message}");
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Data feed error: {ex.Message}");
                }
            }
        }

        private class FlightInfo : INotifyPropertyChanged
        {
            public DynamicEntity Entity { get; }
            private string _flightNumber;
            private string _aircraft;
            private string _altitude;
            private string _speed;
            private string _heading;
            private string _status;
            private string _arrivalAirport;
            private bool _isExpanded;

            // Initializes flight info with entity and subscribes to change events
            public FlightInfo(DynamicEntity entity)
            {
                Entity = entity;
                UpdateAttributes();
                Entity.DynamicEntityChanged += OnEntityChanged;
            }

            public bool IsExpanded
            {
                get => _isExpanded;
                set
                {
                    if (_isExpanded != value)
                    {
                        _isExpanded = value;
                        OnPropertyChanged();
                        OnPropertyChanged(nameof(ToggleButtonText));
                    }
                }
            }

            public string ToggleButtonText => IsExpanded ? "Hide Details" : "Show Details";

            // Updates field value and raises property changed event if value differs
            private bool SetField<T>(ref T field, T value, [CallerMemberName] string name = null)
            {
                if (EqualityComparer<T>.Default.Equals(field, value)) return false;
                field = value;
                OnPropertyChanged(name);
                return true;
            }

            public string FlightNumber { get => _flightNumber; private set => SetField(ref _flightNumber, value); }
            public string Aircraft { get => _aircraft; private set => SetField(ref _aircraft, value); }
            public string Altitude { get => _altitude; private set => SetField(ref _altitude, value); }
            public string Speed { get => _speed; private set => SetField(ref _speed, value); }
            public string Heading { get => _heading; private set => SetField(ref _heading, value); }
            public string Status { get => _status; private set => SetField(ref _status, value); }
            public string ArrivalAirport { get => _arrivalAirport; private set => SetField(ref _arrivalAirport, value); }

            // Retrieves attribute value from entity or returns default if not found
            private string GetAttribute(string key, string defaultValue)
            {
                if (Entity?.Attributes != null && Entity.Attributes.TryGetValue(key, out var value))
                    return value?.ToString() ?? defaultValue;
                return defaultValue;
            }

            // Formats numeric string to zero decimal places or returns original if not numeric
            private string FormatNumber(string value)
            {
                if (double.TryParse(value, out double number))
                    return number.ToString("F0");
                return value;
            }

            // Refreshes all flight attribute properties from the entity
            private void UpdateAttributes()
            {
                FlightNumber = GetAttribute("flight_number", "N/A");
                Aircraft = GetAttribute("aircraft", "Unknown");
                Altitude = FormatNumber(GetAttribute("altitude_feet", "0"));
                Speed = FormatNumber(GetAttribute("speed", "0"));
                Heading = FormatNumber(GetAttribute("heading", "0"));
                Status = GetAttribute("status", "Unknown");
                ArrivalAirport = GetAttribute("arrival_airport", "N/A");
            }

            // Updates attributes when entity receives new observation data
            private void OnEntityChanged(object sender, DynamicEntityChangedEventArgs e)
            {
                if (e.ReceivedObservation != null)
                {
                    UpdateAttributes();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            // Raises property changed event for data binding updates
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}