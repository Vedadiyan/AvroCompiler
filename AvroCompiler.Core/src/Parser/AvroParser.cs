using System.Text;
using System.Text.Json;
using AvroCompiler.Core.Abstraction;
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
        if (Lexicon.HasValue(root))
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
        Lexicon.ThrowIfOtherwise(Lexicon.HasValue(root.Types));
        foreach (var type in Lexicon.MustNeverBeNull(root.Types))
        {
            Lexicon.ThrowIfOtherwise(Lexicon.HasValue(type.Name), Lexicon.HasValue(type.TypeName));
            switch (type.TypeName)
            {
                case "record":
                case "error":
                case "object":
                    {
                        if (!Lexicon.HasValue(type.Fields))
                        {
                            break;
                        }
                        Dictionary<string, AvroElement> fields = new Dictionary<string, AvroElement>();
                        foreach (var field in Lexicon.MustNeverBeNull(type.Fields))
                        {
                            if (!Lexicon.HasValue(field.Name) || !Lexicon.HasValue(field.Type))
                            {
                                continue;
                            }
                            if (Lexicon.IsUnionType(field.Type))
                            {
                                string[]? typeNames = Lexicon.MustNeverBeNull(field.Type).Deserialize<string[]>();
                                if (Lexicon.HasValue(typeNames))
                                {
                                    fields.Add(
                                        Lexicon.MustNeverBeNull(field.Name),
                                        new AvroField(
                                            Lexicon.MustNeverBeNull(field.Name),
                                            Lexicon.MustNeverBeNull(typeNames),
                                            languageFeature
                                        )
                                    );
                                }
                            }
                            else if (Lexicon.IsFieldType(field.Type))
                            {
                                fields.Add(
                                    Lexicon.MustNeverBeNull(field.Name),
                                    new AvroField(
                                        Lexicon.MustNeverBeNull(field.Name),
                                        new string[] {
                                                    Lexicon.MustNeverBeNull(
                                                        Lexicon.MustNeverBeNull(field.Type).GetString()
                                                    )
                                        },
                                        languageFeature
                                    )
                                );
                            }
                            else
                            {
                                if (Lexicon.IsArrayType(field.Type, out string? __type))
                                {
                                    if (Lexicon.HasValue(__type))
                                    {
                                        fields.Add(
                                            Lexicon.MustNeverBeNull(field.Name),
                                            new AvroArray(
                                                Lexicon.MustNeverBeNull(field.Name),
                                                Lexicon.MustNeverBeNull(__type),
                                                languageFeature
                                            )
                                        );
                                    }
                                }
                                else
                                {
                                    if (Lexicon.MustNeverBeNull(field.Type).TryGetProperty("type", out JsonElement types))
                                    {
                                        if (Lexicon.IsUnionType(types))
                                        {
                                            string[]? typeNames = types.Deserialize<string[]>();
                                            if (Lexicon.HasValue(typeNames))
                                            {
                                                fields.Add(
                                                    Lexicon.MustNeverBeNull(field.Name),
                                                    new AvroField(
                                                        Lexicon.MustNeverBeNull(field.Name),
                                                        Lexicon.MustNeverBeNull(typeNames),
                                                        languageFeature
                                                    )
                                                );

                                            }
                                        }
                                        else if (Lexicon.IsFieldType(types))
                                        {
                                            fields.Add(
                                                Lexicon.MustNeverBeNull(field.Name),
                                                new AvroField(
                                                    Lexicon.MustNeverBeNull(field.Name),
                                                    new string[] {
                                                                Lexicon.MustNeverBeNull(
                                                                    Lexicon.MustNeverBeNull(types).GetString()
                                                                )
                                                    },
                                                    languageFeature
                                                )
                                            );
                                        }
                                        else
                                        {
                                            Lexicon.ThrowIfUnpredictable();
                                        }
                                    }
                                }
                            }
                        }
                        yield return new AvroRecord(Lexicon.MustNeverBeNull(type.Name), fields, type.RawObject.GetRawText(), languageFeature);
                        break;
                    }
                case "enum":
                    {
                        if (Lexicon.HasValue(type.Symbols))
                        {
                            yield return new AvroEnum(Lexicon.MustNeverBeNull(type.Name), Lexicon.MustNeverBeNull(type.Symbols).ToArray(), languageFeature);
                        }
                        break;
                    }
                case "fixed":
                    {
                        if (Lexicon.HasValue(type.Size))
                        {
                            yield return new AvroFixed(Lexicon.MustNeverBeNull(type.Name), Lexicon.MustNeverBeNull(type.Size), languageFeature);
                        }
                        break;
                    }
            }

        }

        if (Lexicon.HasValue(root.Messages))
        {
            foreach (var message in Lexicon.MustNeverBeNull(root.Messages))
            {
                yield return
                new AvroMessage(
                    message.Key,
                    Lexicon.MustNeverBeNull(message.Value.Request).ToDictionary(x => Lexicon.MustNeverBeNull(x.Name), x => Lexicon.MustNeverBeNull(x.Type)),
                    Lexicon.MustNeverBeNull(message.Value.Response),
                    Lexicon.MaybeSafelyNull(message.Value.Errors)?.FirstOrDefault(),
                    languageFeature
                );
            }
        }
    }
}