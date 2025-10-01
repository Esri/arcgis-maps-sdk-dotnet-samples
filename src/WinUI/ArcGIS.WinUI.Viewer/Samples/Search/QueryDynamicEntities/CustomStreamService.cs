// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.RealTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ArcGIS.WinUI.Samples.QueryDynamicEntities
{
    public class CustomStreamService : DynamicEntityDataSource
    {
        private readonly DynamicEntityDataSourceInfo _info;
        private readonly string _dataPath;
        private readonly TimeSpan _delay;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _dataFeedTask;

        // Initializes the custom stream service with data source info and file path.
        public CustomStreamService(DynamicEntityDataSourceInfo info, string dataPath, TimeSpan delay)
        {
            _info = info;
            _dataPath = dataPath;
            _delay = delay;
        }

        // Returns the data source information when loading the service.
        protected override Task<DynamicEntityDataSourceInfo> OnLoadAsync() =>
            Task.FromResult(_info);

        // Starts the data feed processing task when connecting to the service.
        protected override Task OnConnectAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _dataFeedTask = ProcessDataFeedAsync(_cancellationTokenSource.Token);
            return Task.CompletedTask;
        }

        // Cancels the data feed task and cleans up resources when disconnecting.
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

        // Reads and processes the flight data feed from the specified file asynchronously.
        private async Task ProcessDataFeedAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!File.Exists(_dataPath))
                    throw new FileNotFoundException("Flight data file not found.", _dataPath);

                // Read all lines off the UI thread. Each line should contain one JSON object.
                var lines = await File.ReadAllLinesAsync(_dataPath, cancellationToken);

                foreach (var line in lines)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        // Parse a single JSON line (newline-delimited JSON format).
                        using (JsonDocument document = JsonDocument.Parse(line))
                        {
                            var root = document.RootElement;

                            // Geometry section (expected: { "geometry": { "x": <double>, "y": <double> } })
                            if (root.TryGetProperty("geometry", out JsonElement geometryElement))
                            {
                                double x = 0, y = 0;

                                // Extract X coordinate (defaults remain 0 if missing or invalid).
                                if (geometryElement.TryGetProperty("x", out JsonElement xElement))
                                    x = xElement.GetDouble();

                                // Extract Y coordinate.
                                if (geometryElement.TryGetProperty("y", out JsonElement yElement))
                                    y = yElement.GetDouble();

                                // Create geometry in WGS84.
                                var point = new MapPoint(x, y, SpatialReferences.Wgs84);

                                // Collect attribute key/value pairs.
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
                                // Add the observation to the data source.
                                AddObservation(point, attributes);
                            }
                        }
                        // Await a delay to throttle the feed rate.
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
}