using System.Text.Json.Serialization;

namespace AvroCompiler.Core.Schema;
public class Request
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
     [JsonPropertyName("type")]
    public string? Type { get; set; }
}