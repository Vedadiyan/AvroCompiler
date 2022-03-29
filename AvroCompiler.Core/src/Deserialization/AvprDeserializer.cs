using System.Text.Json;
using AvroCompiler.Core.Contants;
using AvroCompiler.Core.Schema;

namespace AvroCompiler.Core.Deserialization;

public class AvprDeserializer
{
    private readonly string jsonString;
    public AvprDeserializer(string jsonString)
    {
        this.jsonString = jsonString;
    }
    public Root GetRoot()
    {
        JsonDocument jsonDocument = JsonSerializer.Deserialize<JsonDocument>(jsonString)!;
        Root root = new Root();
        if (jsonDocument.RootElement.TryGetProperty("protocol", out JsonElement protocol))
        {
            root.protocol = protocol.GetString();
        }
        if (jsonDocument.RootElement.TryGetProperty("namespace", out JsonElement @namespace))
        {
            root.@namespace = @namespace.GetString();
        }
        if (jsonDocument.RootElement.TryGetProperty("doc", out JsonElement doc))
        {
            root.doc = doc.GetString();
        }
        if (jsonDocument.RootElement.TryGetProperty("types", out JsonElement types))
        {
            root.types = types.EnumerateArray().Select(x =>
            {
                Schema.Type type = x.Deserialize<Schema.Type>()!;
                type.RawObject = x;
                return type;
            }).ToList();
        }
        if (jsonDocument.RootElement.TryGetProperty("messages", out JsonElement messages))
        {
            root.messages = messages.Deserialize<Dictionary<string, Message>>();
        }
        return root;
    }
    public static void FlatenReferences(Root root)
    {
        foreach (var type in root.types!)
        {
            FlatenReferences(root, type);
        }
    }
    public static void FlatenReferences(Root root, Schema.Type type)
    {
        if (type.fields != null)
        {
            List<Field> fields = new List<Field>();
            foreach (var field in type.fields)
            {
                if (field.type?.ValueKind == JsonValueKind.String)
                {
                    string fieldType = field.type.Value.GetString()!;
                    if (isReferencedType(fieldType))
                    {
                        Schema.Type? reference = root.types!.FirstOrDefault(x => x.name == fieldType) ?? throw new NullReferenceException();
                        FlatenReferences(root, reference);
                        fields.Add(new Field
                        {
                            aliases = field.aliases,
                            name = field.name,
                            type = reference.RawObject
                        });
                        continue;
                    }
                }
                fields.Add(field);
            }
            type.RawObject = JsonSerializer.SerializeToElement(new Schema.Type
            {
                aliases = type.aliases,
                fields = fields,
                name = type.name,
                order = type.order,
                size = type.size,
                symbols = type.symbols,
                type = type.type
            });
        }
    }
    private static bool isReferencedType(string typeName)
    {
        return !Enum.TryParse<AvroTypes>(typeName.ToUpper(), out AvroTypes avroTypes);
    }
}