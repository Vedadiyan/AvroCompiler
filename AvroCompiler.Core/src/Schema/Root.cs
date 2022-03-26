using System.Text.Json;

namespace AvroCompiler.Core.Schema;
public class Root
{
    public string? protocol { get; set; }
    public string? @namespace { get; set; }
    public string? doc { get; set; }
    public List<JsonElement>? types { get; set; }
    public Dictionary<string, Message>? messages { get; set; }
}