namespace AvroCompiler.Core.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

public class Field
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("type")]
    public JsonElement? Type { get; set; }
    [JsonPropertyName("aliases")]
    public List<string>? Aliases { get; set; }
}
