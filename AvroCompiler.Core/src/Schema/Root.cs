using System.Text.Json;
using System.Text.Json.Serialization;

namespace AvroCompiler.Core.Schema;
public class Root
{
    [JsonPropertyName("protocol")]
    public string? Protocol { get; set; }
     [JsonPropertyName("namespace")]
    public string? Namespace { get; set; }
     [JsonPropertyName("doc")]
    public string? Doc { get; set; }
     [JsonPropertyName("types")]
    public List<Schema.Type>? Types { get; set; }
     [JsonPropertyName("messages")]
    public Dictionary<string, Message>? Messages { get; set; }
}