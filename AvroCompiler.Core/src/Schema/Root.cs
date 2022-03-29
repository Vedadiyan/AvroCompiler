using System.Text.Json;
using System.Text.Json.Serialization;

namespace AvroCompiler.Core.Schema;
public class Root
{
    public string? protocol { get; set; }
    public string? @namespace { get; set; }
    public string? doc { get; set; }
    public List<Schema.Type>? types { get; set; }
    public Dictionary<string, Message>? messages { get; set; }
}