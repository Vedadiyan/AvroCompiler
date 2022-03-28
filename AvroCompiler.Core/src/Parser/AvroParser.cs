using System.Text;
using System.Text.Json;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Contants;
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
    public async Task<string> Parse()
    {
        StringBuilder preview = new StringBuilder();
        Root? root = JsonSerializer.Deserialize<Root>(avprData);
        loadReferences(root!);
        if (root != null)
        {
            preview.AppendLine(languageFeature.GetNamespace(root.protocol!));
            preview.AppendLine(languageFeature.GetImports());
            foreach (var i in parse(root))
            {
                if (i is AvroRecord avroRecord)
                {
                    preview.AppendLine(languageFeature.GetCodec(avroRecord.Name, avroRecord.RawJson, null));
                }
                preview.AppendLine(i.Template());
            }
        }
        return await languageFeature.Format(preview.ToString());
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
                        case "error":
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
                    }
                }
            }
        }
        if (root.messages != null)
        {
            foreach (var message in root.messages)
            {
                yield return new AvroMessage(message.Key, message.Value.request!.ToDictionary(x => x.name!, x => x.type)!, message.Value.response!, message.Value.errors?.FirstOrDefault(), languageFeature);
            }
        }
    }
    private void loadReferences(Root root)
    {
        if (root.types != null)
        {
            Dictionary<string, (Schema.Type Value, JsonElement RawValue)> types = root.types.Select(x =>
            {
                Schema.Type type = x.Deserialize<Schema.Type>() ?? throw new ArgumentNullException();
                return new { Name = type?.name ?? throw new ArgumentNullException(), Value = type, RawValue = x };
            })?.ToDictionary(x => x.Name, x => (x.Value, x.RawValue))!;
            for (int i = 0; i < root.types.Count; i++)
            {
                Schema.Type deserializedType = root.types[i].Deserialize<Schema.Type>()!;
                if (Enum.TryParse<AvroTypes>(deserializedType.type.ToUpper(), out AvroTypes avroTypes) && avroTypes == AvroTypes.RECORD)
                {
                    lazyLoad(types, deserializedType.fields!);
                    root.types[i] = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(deserializedType));
                }
            }


        }
    }
    private void lazyLoad(Dictionary<string, (Schema.Type Value, JsonElement RawValue)> values, List<Field>? fields)
    {
        if (fields != null)
        {
            foreach (var field in fields)
            {
                if (field.type?.ValueKind == JsonValueKind.String)
                {
                    string fieldType = field.type?.GetString()!;
                    if (!Enum.TryParse<AvroTypes>(fieldType, out AvroTypes avroTypes) || avroTypes == AvroTypes.RECORD)
                    {
                        lazyLoad(values, values[fieldType].Value.fields!);
                        field.type = values[fieldType].RawValue;
                    }
                }
            }
        }
    }
}