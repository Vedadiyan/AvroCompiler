using System.Text;
using System.Text.Json;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Schema;
using AvroCompiler.Core.Specifications;

namespace AvroCompiler.Core.Parser;
public class AvroSchemaParser
{
    private readonly ILanguageFeature languageFeature;
    private readonly string avprData;
    public AvroSchemaParser(Stream stream, ILanguageFeature languageFeature)
    {
        using (StreamReader streamReader = new StreamReader(stream))
        {
            avprData = streamReader.ReadToEnd();
        }
        this.languageFeature = languageFeature;
    }
    public string Parse()
    {
        StringBuilder stringBuilder = new StringBuilder();
        Root? root = JsonSerializer.Deserialize<Root>(avprData);
        if (root != null)
        {
            stringBuilder.AppendLine(languageFeature.GetNamespace(root.protocol!));
            stringBuilder.AppendLine(languageFeature.GetImports());
            foreach (var i in parse(root))
            {
                if (i is AvroRecord avroRecord)
                {
                    stringBuilder.AppendLine(languageFeature.GetCodec(avroRecord.Name, avroRecord.RawJson, null));
                }
                stringBuilder.AppendLine(i.Template());
            }
        }
        return stringBuilder.ToString();
    }
    private IEnumerable<AvroElement> parse(Root root)
    {
        if (root.types != null)
        {
            foreach (var _type in root.types)
            {
                Schema.Type type = _type.Deserialize<Schema.Type>()!;
                if (type.name != null && type.type != null)
                {
                    switch (type.type)
                    {
                        case "record":
                        case "object":
                            {
                                if (type.fields != null)
                                {
                                    Dictionary<string, AvroElement> fields = new Dictionary<string, AvroElement>();
                                    foreach (var field in type.fields)
                                    {
                                        if (field.name != null)
                                        {
                                            if (field.type != null)
                                            {
                                                if (field.type.Value.ValueKind == JsonValueKind.Array)
                                                {
                                                    string[] typeNames = field.type.Value.Deserialize<string[]>()!;
                                                    fields.Add(field.name, new AvroField(field.name, typeNames, languageFeature));
                                                }
                                                else if (field.type.Value.ValueKind == JsonValueKind.String)
                                                {
                                                    fields.Add(field.name, new AvroField(field.name, new string[] { field.type.Value.GetString()! }, languageFeature));
                                                }
                                                else
                                                {
                                                    if (field.type.Value.TryGetProperty("items", out JsonElement items))
                                                    {
                                                        string? __type = items.GetString();
                                                        if (__type != null)
                                                        {
                                                            fields.Add(field.name, new AvroArray(field.name, __type, languageFeature));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (field.type.Value.TryGetProperty("type", out JsonElement types))
                                                        {
                                                            if (types.ValueKind == JsonValueKind.Array)
                                                            {
                                                                string[] typeNames = types.Deserialize<string[]>()!;
                                                                fields.Add(field.name, new AvroField(field.name, typeNames, languageFeature));
                                                            }
                                                            else if (types.ValueKind == JsonValueKind.String)
                                                            {
                                                                fields.Add(field.name, new AvroField(field.name, new string[] { types.GetString()! }, languageFeature));
                                                            }
                                                            else
                                                            {
                                                                throw new ArgumentException();
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    yield return new AvroRecord(type.name, fields, _type.GetRawText(), languageFeature);
                                }
                                break;
                            }
                        case "enum":
                            {
                                if (type.symbols != null)
                                {
                                    yield return new AvroEnum(type.name, type.symbols.ToArray(), languageFeature);
                                }
                                break;
                            }
                        case "fixed":
                            {
                                if (type.size != null)
                                {
                                    yield return new AvroFixed(type.name, type.size.Value, languageFeature);
                                }
                                break;
                            }
                        case "message":
                            {
                                yield return new AvroMessage(type.name, new Dictionary<string, string>{}, "", languageFeature);
                                break;
                            }
                    }
                }
            }
        }
    }
}