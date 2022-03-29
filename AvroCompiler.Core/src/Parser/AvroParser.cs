using System.Text;
using System.Text.Json;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Contants;
using AvroCompiler.Core.Deserialization;
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
        AvprDeserializer avprDeserializer = new AvprDeserializer(avprData);
        Root root = avprDeserializer.GetRoot();
        AvprDeserializer.FlatenReferences(root);
        if (root != null)
        {
            preview.AppendLine(languageFeature.GetNamespace(root.Protocol!));
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
        if (root.Types != null)
        {
            foreach (var type in root.Types)
            {
                if (type.Name != null && type.TypeName != null)
                {
                    switch (type.TypeName)
                    {
                        case "record":
                        case "error":
                        case "object":
                            {
                                if (type.Fields != null)
                                {
                                    Dictionary<string, AvroElement> fields = new Dictionary<string, AvroElement>();
                                    foreach (var field in type.Fields)
                                    {
                                        if (field.Name != null)
                                        {
                                            if (field.Type != null)
                                            {
                                                if (field.Type.Value.ValueKind == JsonValueKind.Array)
                                                {
                                                    string[] typeNames = field.Type.Value.Deserialize<string[]>()!;
                                                    fields.Add(field.Name, new AvroField(field.Name, typeNames, languageFeature));
                                                }
                                                else if (field.Type.Value.ValueKind == JsonValueKind.String)
                                                {
                                                    fields.Add(field.Name, new AvroField(field.Name, new string[] { field.Type.Value.GetString()! }, languageFeature));
                                                }
                                                else
                                                {
                                                    if (field.Type.Value.TryGetProperty("items", out JsonElement items))
                                                    {
                                                        string? __type = items.GetString();
                                                        if (__type != null)
                                                        {
                                                            fields.Add(field.Name, new AvroArray(field.Name, __type, languageFeature));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (field.Type.Value.TryGetProperty("type", out JsonElement types))
                                                        {
                                                            if (types.ValueKind == JsonValueKind.Array)
                                                            {
                                                                string[] typeNames = types.Deserialize<string[]>()!;
                                                                fields.Add(field.Name, new AvroField(field.Name, typeNames, languageFeature));
                                                            }
                                                            else if (types.ValueKind == JsonValueKind.String)
                                                            {
                                                                fields.Add(field.Name, new AvroField(field.Name, new string[] { types.GetString()! }, languageFeature));
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
                                    yield return new AvroRecord(type.Name, fields, type.RawObject.GetRawText(), languageFeature);
                                }
                                break;
                            }
                        case "enum":
                            {
                                if (type.Symbols != null)
                                {
                                    yield return new AvroEnum(type.Name, type.Symbols.ToArray(), languageFeature);
                                }
                                break;
                            }
                        case "fixed":
                            {
                                if (type.Size != null)
                                {
                                    yield return new AvroFixed(type.Name, type.Size.Value, languageFeature);
                                }
                                break;
                            }
                    }
                }
            }
        }
        if (root.Messages != null)
        {
            foreach (var message in root.Messages)
            {
                yield return new AvroMessage(message.Key, message.Value.Request!.ToDictionary(x => x.Name!, x => x.Type)!, message.Value.Response!, message.Value.Errors?.FirstOrDefault(), languageFeature);
            }
        }
    }
}