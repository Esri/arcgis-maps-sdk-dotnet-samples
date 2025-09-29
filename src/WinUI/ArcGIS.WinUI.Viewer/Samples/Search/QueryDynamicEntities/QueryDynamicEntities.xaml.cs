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
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ArcGIS.WinUI.Samples.QueryDynamicEntities
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
        private Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;

        public QueryDynamicEntities()
        {
            InitializeComponent();
            _ = Initialize();
        }

        // Sets up the map, graphics overlays, UI controls, dispatcher queue, and data source for flight tracking
        private async Task Initialize()
        {
            try
            {
                _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

                MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic);
                MyMapView.SetViewpoint(new Viewpoint(_phoenixAirport, 400000));

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

                await SetupDynamicEntityDataSource();
            }
            catch (Exception ex)
            {
                await ShowMessageAsync($"Error initializing sample: {ex.Message}", "Error");
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
                await ShowMessageAsync($"Error setting up data source: {ex.Message}", "Error");
            }
        }

        // Handles query selection from dropdown and initiates appropriate query type
        private async void OnQuerySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = QueryDropdown.SelectedItem as ComboBoxItem;
            if (selectedItem == null)
                return;

            var queryType = selectedItem.Tag as string;
            if (string.IsNullOrEmpty(queryType))
                return;

            if (queryType == "QueryTrackId")
            {
                FlightNumberDialog.Visibility = Visibility.Visible;
                FlightNumberInput.SelectAll();
            }
            else
            {
                await PerformQuery(queryType);
            }
        }

        // Processes flight number input and executes track ID query
        private async void OnFlightNumberQuery(object sender, RoutedEventArgs e)
        {
            var flightNumber = FlightNumberInput.Text?.Trim();
            if (string.IsNullOrWhiteSpace(flightNumber))
            {
                await ShowMessageAsync("Please enter a flight number.", "Input Required");
                return;
            }

            FlightNumberDialog.Visibility = Visibility.Collapsed;
            await PerformQuery("QueryTrackId", flightNumber);
            FlightNumberInput.Text = string.Empty;
        }

        // Cancels flight number input and returns to query selection
        private void OnFlightNumberCancel(object sender, RoutedEventArgs e)
        {
            FlightNumberDialog.Visibility = Visibility.Collapsed;
            FlightNumberInput.Text = string.Empty;
            QueryDropdown.SelectedIndex = -1;
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
                        else
                        {
                            return;
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
                                var flightInfo = new FlightInfo(entity, _dispatcherQueue);
                                _queryResults.Add(flightInfo);
                            }
                        }
                    }
                }

                ResultsPanel.Visibility = Visibility.Visible;

                if (!_queryResults.Any())
                {
                    await ShowMessageAsync("No flights currently match the criteria, but the list will update as flights enter or leave the area.",
                        "Live Tracking Active");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageAsync($"Query failed: {ex.Message}", "Error");
                System.Diagnostics.Debug.WriteLine($"Query exception: {ex}");
            }
        }

        // Centers the map view on the selected flight's current position
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

        // Closes the results panel and resets the UI to the initial state
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

        // Displays a WinUI ContentDialog with the specified message and title
        private async Task ShowMessageAsync(string message, string title)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private class CustomStreamService : DynamicEntityDataSource
        {
            private readonly DynamicEntityDataSourceInfo _info;
            private readonly string _dataPath;
            private readonly TimeSpan _delay;
            private CancellationTokenSource _cancellationTokenSource;
            private Task _dataFeedTask;

            // Initializes the custom stream service with data source info and file path
            public CustomStreamService(DynamicEntityDataSourceInfo info, string dataPath, TimeSpan delay)
            {
                _info = info;
                _dataPath = dataPath;
                _delay = delay;
            }

            // Returns the data source information when loading the service
            protected override Task<DynamicEntityDataSourceInfo> OnLoadAsync() =>
                Task.FromResult(_info);

            // Starts the data feed processing task when connecting to the service
            protected override Task OnConnectAsync(CancellationToken cancellationToken)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _dataFeedTask = ProcessDataFeedAsync(_cancellationTokenSource.Token);
                return Task.CompletedTask;
            }

            // Cancels the data feed task and cleans up resources when disconnecting
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
                _cancellationTokenSource = null;
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
                                            object value = null;

                                            if (property.Value.ValueKind == JsonValueKind.String)
                                            {
                                                value = property.Value.GetString();
                                            }
                                            else if (property.Value.ValueKind == JsonValueKind.Number)
                                            {
                                                if (property.Value.TryGetInt32(out int intValue))
                                                    value = intValue;
                                                else
                                                    value = property.Value.GetDouble();
                                            }
                                            else if (property.Value.ValueKind == JsonValueKind.True)
                                            {
                                                value = true;
                                            }
                                            else if (property.Value.ValueKind == JsonValueKind.False)
                                            {
                                                value = false;
                                            }
                                            else if (property.Value.ValueKind == JsonValueKind.Null)
                                            {
                                                value = null;
                                            }
                                            else
                                            {
                                                value = property.Value.ToString();
                                            }

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
            private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;
            private string _flightNumber;
            private string _aircraft;
            private string _altitude;
            private string _speed;
            private string _heading;
            private string _status;
            private string _arrivalAirport;

            // Initializes flight info with entity and dispatcher queue for thread-safe property updates
            public FlightInfo(DynamicEntity entity, Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue)
            {
                Entity = entity;
                _dispatcherQueue = dispatcherQueue;
                UpdateAttributes();
                Entity.DynamicEntityChanged += OnEntityChanged;
            }

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

            // Raises property changed event using dispatcher queue for thread-safe UI updates
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                if (_dispatcherQueue != null && !_dispatcherQueue.HasThreadAccess)
                {
                    _dispatcherQueue.TryEnqueue(() =>
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    });
                }
                else
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
    }
}