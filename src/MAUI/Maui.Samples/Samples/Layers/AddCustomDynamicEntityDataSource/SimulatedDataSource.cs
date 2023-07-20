using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.RealTime;
using System.Diagnostics;
using System.Text.Json;


namespace ArcGIS.Samples.AddCustomDynamicEntityDataSource
{
    public class SimulatedDataSource : DynamicEntityDataSource
    {
        // Hold a reference to the file stream reader, the process task, and the cancellation token source.
        private Task? _processTask;
        private StreamReader? _streamReader;
        private CancellationTokenSource? _cancellationTokenSource;
        private List<Field> _fields;

        public SimulatedDataSource(string filePath, string entityIdField, TimeSpan delay)
        {
            FilePath = filePath;
            EntityIdField = entityIdField;
            Delay = delay;
        }

        #region Properties

        // Expose the file path, entity ID field, and delay length as properties.
        public string FilePath { get; }
        public string EntityIdField { get; }
        public TimeSpan Delay { get; }
        public SpatialReference SpatialReference { get; set; } = SpatialReferences.Wgs84;

        #endregion

        protected override async Task<DynamicEntityDataSourceInfo> OnLoadAsync()
        {
            // Derive schema from the first row in the custom data source.
            _fields = GetSchema();

            // Open the file for processing.
            Stream stream = await FileSystem.OpenAppPackageFileAsync(FilePath);
            _streamReader = new StreamReader(stream);

            // Create a new DynamicEntityDataSourceInfo using the entity ID field and the fields derived from the attributes of each observation in the custom data source.
            return new DynamicEntityDataSourceInfo(EntityIdField, _fields) { SpatialReference = SpatialReferences.Wgs84 };
        }

        protected override Task OnConnectAsync(CancellationToken cancellationToken)
        {
            // On connecting to the custom data source begin processing the file. 
            _cancellationTokenSource = new();
            _processTask = Task.Run(() => ObservationProcessLoopAsync(), _cancellationTokenSource.Token);
            return Task.CompletedTask;
        }

        protected override async Task OnDisconnectAsync()
        {
            // On disconnecting from the custom data source, stop processing the file.
            _cancellationTokenSource?.Cancel();

            if (_processTask is not null) await _processTask;

            _cancellationTokenSource = null;
            _processTask = null;
        }

        private async Task ObservationProcessLoopAsync()
        {
            try
            {
                while (!_cancellationTokenSource!.IsCancellationRequested)
                {
                    // Process the next observation.
                    var processed = await ProcessNextObservation();

                    // If the observation was not processed, continue to the next observation.
                    if (!processed) continue;

                    // If the end of the file has been reached, break out of the loop.
                    if (_streamReader.EndOfStream) _break;

                    // If there is no delay, yield to the UI thread otherwise delay for the specified amount of time.
                    if (Delay == TimeSpan.Zero)
                    {
                        await Task.Yield();
                    }
                    else
                    {
                        await Task.Delay(Delay, _cancellationTokenSource.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private async Task<bool> ProcessNextObservation()
        {
            _ = _streamReader ?? throw new ArgumentNullException("File stream not available.");
            
            // Read the next observation.
            var json = await _streamReader.ReadLineAsync();

            // If there is no json to read or the schema is not available, return false.
            if (string.IsNullOrEmpty(json) || _fields is null) return false;

            try
            {
                JsonElement jsonElement = JsonSerializer.Deserialize<JsonElement>(json);

                // Create a new MapPoint from the x and y coordinates of the observation.
                MapPoint? point = null;
                if (jsonElement.TryGetProperty("geometry", out JsonElement jsonGeometry))
                {
                    point = new MapPoint(
                        jsonGeometry.GetProperty("x").GetDouble(),
                        jsonGeometry.GetProperty("y").GetDouble(),
                        SpatialReferences.Wgs84);
                }

                // Get the dictionary of attributes from the observation using the field names as keys.
                Dictionary<string, object?> attributes = new();
                if (jsonElement.TryGetProperty("attributes", out JsonElement jsonAttributes))
                {
                    foreach (var field in _fields)
                    {
                        if (jsonAttributes.TryGetProperty(field.Name, out JsonElement prop))
                        {
                            object? value = null;
                            if (prop.ValueKind != JsonValueKind.Null)
                            {
                                if (prop.ValueKind == JsonValueKind.Number && field.FieldType == FieldType.Float64)
                                {
                                    value = prop.GetDouble();
                                }
                                else if (prop.ValueKind == JsonValueKind.Number && field.FieldType == FieldType.Int32)
                                {
                                    value = prop.GetInt32();
                                }
                                else if (prop.ValueKind == JsonValueKind.String)
                                {
                                    value = prop.GetString();
                                }
                            }
                            attributes.Add(field.Name, value);
                        }
                    }
                }

                // Add the observation to the custom data source.
                AddObservation(point, attributes);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
                return false;
            }
        }

        private static List<Field> GetSchema()
        {
            // Return a list of fields matching the attributes of each observation in the custom data source.
            return new List<Field>()
            {
                new Field(FieldType.Text, "MMSI", string.Empty, 256),
                new Field(FieldType.Float64, "BaseDateTime", string.Empty, 8),
                new Field(FieldType.Float64, "LAT", string.Empty, 8),
                new Field(FieldType.Float64, "LONG", string.Empty, 8),
                new Field(FieldType.Float64, "SOG", string.Empty, 8),
                new Field(FieldType.Float64, "COG", string.Empty, 8),
                new Field(FieldType.Float64, "Heading", string.Empty, 8),
                new Field(FieldType.Text, "VesselName", string.Empty, 256),
                new Field(FieldType.Text, "IMO", string.Empty, 256),
                new Field(FieldType.Text, "CallSign", string.Empty, 256),
                new Field(FieldType.Text, "VesselType", string.Empty, 256),
                new Field(FieldType.Text, "Status", string.Empty, 256),
                new Field(FieldType.Float64, "Length", string.Empty, 8),
                new Field(FieldType.Float64, "Width", string.Empty, 8),
                new Field(FieldType.Text, "Cargo", string.Empty, 256),
                new Field(FieldType.Text, "globalid", string.Empty, 256)
            };
        }
    }
}
