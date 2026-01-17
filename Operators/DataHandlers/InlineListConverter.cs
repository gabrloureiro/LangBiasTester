using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections;

namespace LangBiasTester.Operators.DataHandlers
{
    public class InlineCollectionConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            // Only apply to types that are arrays or implement IEnumerable (excluding strings)
            return typeToConvert != typeof(string) && typeof(IEnumerable).IsAssignableFrom(typeToConvert);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            // Dynamically create an instance of the converter for the specific type
            return (JsonConverter)Activator.CreateInstance(
                typeof(InlineCollectionConverter<>).MakeGenericType(typeToConvert));
        }

        private class InlineCollectionConverter<T> : JsonConverter<T>
        {
            public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => JsonSerializer.Deserialize<T>(ref reader, options);

            public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            {
                // 1. Serialize to a compact string
                var compactOptions = new JsonSerializerOptions { WriteIndented = false };
                string json = JsonSerializer.Serialize(value, compactOptions);

                // 2. Add spaces after commas for readability
                // Result: [1,2,3] -> [1, 2, 3]
                string spacedJson = json.Replace(",", ", ").Replace("{", "{ ").Replace("}", " }").Replace("[", "[ ").Replace("]", " ]").Replace(":", ": ");

                // 3. Write as raw JSON
                writer.WriteRawValue(spacedJson);
            }
        }
    }
}
