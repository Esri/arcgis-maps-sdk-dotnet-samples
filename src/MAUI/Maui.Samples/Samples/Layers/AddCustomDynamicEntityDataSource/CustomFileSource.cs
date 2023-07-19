using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.RealTime;
using System.Diagnostics;
using System.Text.Json;


namespace ArcGIS.Samples.AddCustomDynamicEntityDataSource
{
    public class CustomFileSource : DynamicEntityDataSource
    {
        // Hold a reference to the file stream reader, the process task, and the cancellation token source.
        private Task? _processTask;
        private StreamReader? _streamReader;
        private CancellationTokenSource? _cancellationTokenSource;
        private TaskCompletionSource<object?>? _pauseTask;

        public CustomFileSource(string filePath, string entityIdField, TimeSpan delay)
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
            var fields = await InferSchemaAsync();

            // Open the file for processing.
            Stream stream = await FileSystem.OpenAppPackageFileAsync(FilePath);
            _streamReader = new StreamReader(stream);

            // Create a new DynamicEntityDataSourceInfo using the entity ID field and the fields derived from the attributes of each observation in the custom data source.
            return new DynamicEntityDataSourceInfo(EntityIdField, fields) { SpatialReference = SpatialReferences.Wgs84 };
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
            _pauseTask?.TrySetResult(null);

            if (_processTask is not null) await _processTask;

            _cancellationTokenSource = null;
            _pauseTask = null;
            _processTask = null;
        }

        private async Task ObservationProcessLoopAsync()
        {
            try
            {
                while (!_cancellationTokenSource!.IsCancellationRequested)
                {
                    // If the pause task is not null, wait for it to complete before processing the next observation.
                    if (_pauseTask is not null)
                    {
                        await _pauseTask.Task;
                    }

                    // Process the next observation.
                    var processed = await ProcessNextObservation();

                    // If the observation was not processed, continue to the next observation.
                    if (!processed) continue;

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
            // If we cannot read from the file, throw an exception.
            if (_streamReader is null)
            {
                throw new ArgumentNullException("File stream not available.");
            }

            // Read the next observation from the file.
            var json = await _streamReader.ReadLineAsync();
            if (string.IsNullOrEmpty(json)) return false;

            try
            {
                // Parse the observation from the file using the JsonGeoElementParser.
                // This will return a graphic holding a  MapPoint and a dictionary of attributes.
                var graphic = JsonGeoElementParser.ParseGraphicFromJson(json);
                if (graphic?.Geometry is not MapPoint point) return false;

                // Check to see if the spatial reference of the observation matches the spatial reference of the custom data source.
                // If not, project the observation to the spatial reference of the custom data source.
                if (point.SpatialReference?.Equals(SpatialReference) != true)
                {
                    graphic.Geometry = GeometryEngine.Project(graphic.Geometry, SpatialReference);
                }

                // Add the observation to the dynamic entity layer for presentation on the map.
                AddObservation(graphic.Geometry, graphic.Attributes);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        private async Task<List<Field>> InferSchemaAsync()
        {
            var fields = new List<Field>();

            try
            {
                // Load the file we are using for our custom data source.
                using var stream = await FileSystem.OpenAppPackageFileAsync(FilePath);
                if (stream != null)
                {
                    using var streamReader = new StreamReader(stream);
                    
                    // Read the first line of the file.
                    var json = await streamReader.ReadLineAsync();

                    // If the first line is empty the scheme cannot be inferred.
                    if (string.IsNullOrEmpty(json))
                    {
                        throw new InvalidOperationException("Could not infer schema (empty json line at the start of the file).");
                    }

                    // Parse the first line of the file using the JsonGeoElementParser.
                    var jsonGeoElement = JsonSerializer.Deserialize<JsonGeoElementParser.JsonGeoElement>(json);

                    // If no attributes are present in the JsonGeoElement, the scheme cannot be inferred.
                    if (jsonGeoElement?.attributes is null)
                    {
                        throw new InvalidOperationException("Could not infer schema (parsing error).");
                    }

                    // Loop through the attributes in the JsonGeoElement and create fields for each attribute based on the field type and length.
                    foreach (var attr in jsonGeoElement.attributes)
                    {
                        // Infer the field type and length from the attribute value.
                        var fieldType = Type.GetTypeCode(attr.Value?.GetType()) switch
                        {
                            TypeCode.Boolean or TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Single => FieldType.Int16,
                            TypeCode.Int32 or TypeCode.UInt32 => FieldType.Int32,
                            TypeCode.DateTime => FieldType.Date,
                            TypeCode.Decimal => FieldType.Float32,
                            TypeCode.Double or TypeCode.Int64 or TypeCode.UInt64 => FieldType.Float64,
                            _ => FieldType.Text,
                        };

                        var length = Type.GetTypeCode(attr.Value?.GetType()) switch
                        {
                            TypeCode.Boolean or TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Single => 2,
                            TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Decimal => 4,
                            TypeCode.Double or TypeCode.Int64 or TypeCode.UInt64 => 8,
                            _ => 256,
                        };

                        // Create a field from the attribute type, name and length and add it to the list of fields.
                        var field = new Field(fieldType, attr.Key, string.Empty, length);
                        fields.Add(field);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return fields;
        }

        public async Task StepAsync()
        {
            bool processed;
            do
            {
                processed = await ProcessNextObservation();
            }
            while (!processed);
        }
    }
}
