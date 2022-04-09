namespace AvroCompiler.Core.Schema;
using System.Text.Json.Serialization;

public class Message
{
    [JsonPropertyName("request")]
    public List<Request>? Request { get; set; }
    [JsonPropertyName("response")]
    public string? Response { get; set; }
    [JsonPropertyName("errors")]
    public List<string>? Errors { get; set; }
    [JsonPropertyName("nats")]
    public Dictionary<string, string>? Nats { get; set; }
    [JsonPropertyName("one-way")]
    public bool OneWay { get; set; }
}