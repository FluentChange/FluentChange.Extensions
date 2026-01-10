using System.Text.Json;
using System.Text.Json.Serialization;

namespace FluentChange.Extensions.Azure.Functions.Helper
{
    /// <summary>
    /// Zentrale JSON-Serialisierungsoptionen für einheitliche API-Responses
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Standard-Optionen: camelCase, Nulls ignorieren
        /// </summary>
        public static JsonSerializerOptions DefaultOptions { get; }

        static JsonHelper()
        {
            DefaultOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            };
        }

        public static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, DefaultOptions);
        }

        public static string Serialize(object value, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Serialize(value, options ?? DefaultOptions);
        }

        public static T? Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }

        public static T? Deserialize<T>(string json, JsonSerializerOptions? options)
        {
            return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
        }
    }
}
