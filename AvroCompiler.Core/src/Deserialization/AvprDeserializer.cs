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
            root.Protocol = protocol.GetString();
        }
        if (jsonDocument.RootElement.TryGetProperty("namespace", out JsonElement @namespace))
        {
            root.Namespace = @namespace.GetString();
        }
        if (jsonDocument.RootElement.TryGetProperty("doc", out JsonElement doc))
        {
            root.Doc = doc.GetString();
        }
        if (jsonDocument.RootElement.TryGetProperty("types", out JsonElement types))
        {
            root.Types = types.EnumerateArray().Select(x =>
            {
                Schema.Type type = x.Deserialize<Schema.Type>()!;
                type.RawObject = x;
                return type;
            }).ToList();
        }
        if (jsonDocument.RootElement.TryGetProperty("messages", out JsonElement messages))
        {
            root.Messages = messages.Deserialize<Dictionary<string, Message>>();
        }
        return root;
    }
    public static void FlatenReferences(Root root)
    {
        foreach (var type in root.Types!)
        {
            FlatenReferences(root, type);
        }
    }
    public static void FlatenReferences(Root root, Schema.Type type)
    {
        if (type.Fields != null)
        {
            List<Field> fields = new List<Field>();
            foreach (var field in type.Fields)
            {
                if (field.Type?.ValueKind == JsonValueKind.String)
                {
                    string fieldType = field.Type.Value.GetString()!;
                    if (isReferencedType(fieldType))
                    {
                        Schema.Type? reference = root.Types!.FirstOrDefault(x => x.Name == fieldType) ?? throw new NullReferenceException();
                        FlatenReferences(root, reference);
                        fields.Add(new Field
                        {
                            Aliases = field.Aliases,
                            Name = field.Name,
                            Type = reference.RawObject
                        });
                        continue;
                    }
                }
                fields.Add(field);
            }
            type.RawObject = JsonSerializer.SerializeToElement(new Schema.Type
            {
                Aliases = type.Aliases,
                Fields = fields,
                Name = type.Name,
                Order = type.Order,
                Size = type.Size,
                Symbols = type.Symbols,
                TypeName = type.TypeName
            });
        }
    }
    private static bool isReferencedType(string typeName)
    {
        return !Enum.TryParse<AvroTypes>(typeName.ToUpper(), out AvroTypes avroTypes);
    }
}