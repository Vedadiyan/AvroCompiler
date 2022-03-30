using System.Text;
using System.Text.Json;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Deserialization;
using AvroCompiler.Core.Schema;
using AvroCompiler.Core.Specifications;

using static AvroCompiler.Core.Lexicon;

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
        if (HasValue(root))
        {
            preview.Append(languageFeature.GetComments());
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
        StringBuilder finalCode = new StringBuilder();
        foreach(var i in preview.ToString().Split("\r\n")) {
            if(!string.IsNullOrWhiteSpace(i)) {
                finalCode.AppendLine(i);
            }
        }
        return await languageFeature.Format(finalCode.ToString());
    }
    private IEnumerable<AvroElement> parse(Root root)
    {
        ThrowIfOtherwise(HasValue(root.Types));
        foreach (var type in MustNeverBeNull(root.Types))
        {
            ThrowIfOtherwise(HasValue(type.Name), HasValue(type.TypeName));
            switch (type.TypeName)
            {
                case "record":
                case "error":
                case "object":
                    {
                        if (!HasValue(type.Fields)) break;
                        Dictionary<string, AvroElement> fields = new Dictionary<string, AvroElement>();
                        foreach (var field in MustNeverBeNull(type.Fields))
                        {
                            if (!HasValue(field.Name) || !HasValue(field.Type)) continue;
                            if (IsUnionType(field.Type))
                            {
                                string[] typeNames = MustNeverBeNull(field.Type).EnumerateArray().Select(x =>
                                {
                                    if (x.ValueKind == JsonValueKind.String)
                                    {
                                        return x.GetString()!;
                                    }
                                    else
                                    {
                                        return "UNION";
                                    }
                                }).ToArray();
                                fields.Add(
                                    MustNeverBeNull(field.Name),
                                    new AvroField(
                                        MustNeverBeNull(field.Name),
                                        MustNeverBeNull(typeNames),
                                        languageFeature
                                    )
                                );
                            }
                            else if (IsFieldType(field.Type))
                            {
                                fields.Add(
                                    MustNeverBeNull(field.Name),
                                    new AvroField(
                                        MustNeverBeNull(field.Name),
                                        new string[] {
                                                    MustNeverBeNull(
                                                        MustNeverBeNull(field.Type).GetString()
                                                    )
                                        },
                                        languageFeature
                                    )
                                );
                            }
                            else
                            {
                                int dimensions = 1;
                                string? arrayItemType = null;
                                string? itemGenericType = null;
                                if (IsArrayType(field.Type, ref dimensions, ref arrayItemType, ref itemGenericType))
                                {
                                    fields.Add(
                                        MustNeverBeNull(field.Name),
                                        new AvroArray(
                                            MustNeverBeNull(field.Name),
                                            dimensions,
                                            MustNeverBeNull(arrayItemType),
                                            itemGenericType,
                                            languageFeature
                                        )
                                    );
                                }
                                else
                                {
                                    if (MustNeverBeNull(field.Type).TryGetProperty("type", out JsonElement types))
                                    {
                                        if (IsUnionType(types))
                                        {
                                            string[]? typeNames = types.Deserialize<string[]>();
                                            if (HasValue(typeNames)) continue;
                                            fields.Add(
                                                MustNeverBeNull(field.Name),
                                                new AvroField(
                                                    MustNeverBeNull(field.Name),
                                                    MustNeverBeNull(typeNames),
                                                    languageFeature
                                                )
                                            );
                                        }
                                        else if (IsFieldType(types))
                                        {
                                            string typeName = MustNeverBeNull(types.GetString());
                                            if (typeName == "map" && MustNeverBeNull(field.Type).TryGetProperty("values", out JsonElement values))
                                            {
                                                fields.Add(
                                                    MustNeverBeNull(field.Name),
                                                    new AvroMap(
                                                        MustNeverBeNull(field.Name),
                                                        MustNeverBeNull(values.GetString()),
                                                        languageFeature
                                                    )
                                                );
                                            }
                                            else
                                            {
                                                fields.Add(
                                                    MustNeverBeNull(field.Name),
                                                    new AvroField(
                                                        MustNeverBeNull(field.Name),
                                                        new string[] {
                                                        typeName
                                                        },
                                                        languageFeature
                                                    )
                                                );
                                            }
                                        }
                                        else
                                        {
                                            ThrowIfUnpredictable();
                                        }
                                    }
                                }

                            }
                        }
                        yield return new AvroRecord(MustNeverBeNull(type.Name), fields, type.RawObject.GetRawText(), languageFeature);
                        break;
                    }
                case "enum":
                    {
                        if (HasValue(type.Symbols))
                        {
                            yield return new AvroEnum(MustNeverBeNull(type.Name), MustNeverBeNull(type.Symbols).ToArray(), languageFeature);
                        }
                        break;
                    }
                case "fixed":
                    {
                        if (HasValue(type.Size))
                        {
                            yield return new AvroFixed(MustNeverBeNull(type.Name), MustNeverBeNull(type.Size), languageFeature);
                        }
                        break;
                    }
            }

        }

        if (HasValue(root.Messages))
        {
            foreach (var message in MustNeverBeNull(root.Messages))
            {
                yield return
                new AvroMessage(
                    message.Key,
                    MustNeverBeNull(message.Value.Request).ToDictionary(x => MustNeverBeNull(x.Name), x => MustNeverBeNull(x.Type)),
                    MustNeverBeNull(message.Value.Response),
                    MayBeSafelyNull(message.Value.Errors)?.FirstOrDefault(),
                    languageFeature
                );
            }
        }
    }
}