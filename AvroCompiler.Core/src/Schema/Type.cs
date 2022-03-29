using System.Text.Json;
using System.Text.Json.Serialization;

namespace AvroCompiler.Core.Schema;
public class Type
{
    public string? type { get; set; }
    public string? name { get; set; }
    public List<string>? symbols { get; set; }
    public string? order { get; set; }
    public List<string>? aliases { get; set; }
    public int? size { get; set; }
    public List<Field>? fields { get; set; }
    [JsonIgnore]
    public JsonElement RawObject { get; set; }
}