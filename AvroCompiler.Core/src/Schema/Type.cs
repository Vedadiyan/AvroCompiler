using System.Text.Json;
using System.Text.Json.Serialization;

namespace AvroCompiler.Core.Schema;
public class Type
{
    [JsonPropertyName("type")]
    public string? TypeName { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("symbols")]
    public List<string>? Symbols { get; set; }
    [JsonPropertyName("order")]
    public string? Order { get; set; }
    [JsonPropertyName("aliases")]
    public List<string>? Aliases { get; set; }
    [JsonPropertyName("size")]
    public int? Size { get; set; }
    [JsonPropertyName("fields")]
    public List<Field>? Fields { get; set; }
    [JsonIgnore]
    public JsonElement RawObject { get; set; }
}