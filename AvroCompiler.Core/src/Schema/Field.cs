namespace AvroCompiler.Core.Schema;
using System.Text.Json;

public class Field
{
    public string? name { get; set; }
    public JsonElement? type { get; set; }
    public List<string>? aliases { get; set; }
}
