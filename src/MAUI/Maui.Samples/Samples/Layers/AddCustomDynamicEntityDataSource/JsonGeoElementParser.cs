using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArcGIS.Samples.AddCustomDynamicEntityDataSource
{
    public class JsonGeoElementParser
    {
        public static Graphic? ParseGraphicFromJson(string json)
        {
            // Deserialize the json into a JsonGeoElement.
            var jsonGeoElement = JsonSerializer.Deserialize<JsonGeoElement>(json);

            // If the jsonGeoElement cannot be parsed return.
            if (jsonGeoElement is null) return null;

            // Get the geometry from the jsonGeoElement.
            JsonGeometry geometry = jsonGeoElement.geometry;

            // Get the spatial reference from the geometry, if this is null return the default spatial reference (4326).
            var spatialReference = SpatialReference.Create(geometry.spatialReference?.wkid ?? 4326);

            // Create a MapPoint from the geometry and spatial reference.
            var point = new MapPoint(geometry.x, geometry.y, geometry.z, spatialReference);

            // Return a graphic with the created  MapPoint and attributes from the jsonGeoElement.
            return new Graphic(point, jsonGeoElement.attributes);
        }

        #region JsonGeoElement

        // Create an object structure used to deserialize each of the observations from the json file used as the custom data source.
        public class JsonGeoElement
        {
            public JsonGeometry? geometry { get; set; }

            [JsonConverter(typeof(DictionaryStringObjectJsonConverter))]
            public Dictionary<string, object?>? attributes { get; set; }
        }

        public class JsonGeometry
        {
            public double x { get; set; }

            public double y { get; set; }

            public double z { get; set; }

            public JsonSpatialReference? spatialReference { get; set; }
        }

        public class JsonSpatialReference
        {
            public int wkid { get; set; }
        }

        public class DictionaryStringObjectJsonConverter : JsonConverter<Dictionary<string, object?>>
        {
            public override bool CanConvert(Type typeToConvert)
            {
                return typeToConvert == typeof(Dictionary<string, object>)
                       || typeToConvert == typeof(Dictionary<string, object?>);
            }

            // Override the default serialization logic to handle the Dictionary<string, object?>.
            public override Dictionary<string, object?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException($"JsonTokenType was of type {reader.TokenType}, only objects are supported");
                }

                var dictionary = new Dictionary<string, object?>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        return dictionary;
                    }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException("JsonTokenType was not PropertyName");
                    }

                    var propertyName = reader.GetString();

                    if (string.IsNullOrWhiteSpace(propertyName))
                    {
                        throw new JsonException("Failed to get property name");
                    }

                    reader.Read();

                    dictionary.Add(propertyName!, ExtractValue(ref reader, options));
                }

                return dictionary;
            }

            public override void Write(
                Utf8JsonWriter writer, Dictionary<string, object?> value, JsonSerializerOptions options)
            {
                // We don't need any custom serialization logic for writing the json.
                // Ideally, this method should not be called at all. It's only called if you
                // supply JsonSerializerOptions that contains this JsonConverter in it's Converters list.
                // Don't do that, you will lose performance because of the cast needed below.
                // Cast to avoid infinite loop: https://github.com/dotnet/docs/issues/19268
                JsonSerializer.Serialize(writer, (IDictionary<string, object?>)value, options);
            }

            private object? ExtractValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.String:
                        if (reader.TryGetDateTime(out var date))
                        {
                            return date;
                        }
                        return reader.GetString();
                    case JsonTokenType.False:
                        return false;
                    case JsonTokenType.True:
                        return true;
                    case JsonTokenType.Null:
                        return null;
                    case JsonTokenType.Number:
                        if (reader.TryGetInt64(out var result))
                        {
                            return result;
                        }
                        return reader.GetDouble();
                    case JsonTokenType.StartObject:
                        return Read(ref reader, null!, options);
                    case JsonTokenType.StartArray:
                        var list = new List<object?>();
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            list.Add(ExtractValue(ref reader, options));
                        }
                        return list;
                    default:
                        throw new JsonException($"'{reader.TokenType}' is not supported");
                }
            }
        }

        #endregion
    }
}
