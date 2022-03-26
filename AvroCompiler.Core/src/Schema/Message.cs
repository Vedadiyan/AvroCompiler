namespace AvroCompiler.Core.Schema;
using System.Text.Json.Serialization;

public class Message
{
    public List<Request>? request { get; set; }
    public string? response { get; set; }
    public List<string>? errors { get; set; }
    [JsonPropertyName("one-way")]
    public bool OneWay { get; set; }
}