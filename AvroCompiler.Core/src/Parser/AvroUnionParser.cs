using System.Text.Json;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Schema;
using AvroCompiler.Core.Specifications;
using static AvroCompiler.Core.Lexicon;
using static AvroCompiler.Core.Exceptions.AvroCompliationErrorLexicon;

namespace AvroCompiler.Core.Parser;

public class AvroUnionParser : IAvroParser<IEnumerable<AvroElement>>
{
    private readonly Field field;
    private readonly ILanguageFeature languageFeature;
    public AvroUnionParser(Field field, ILanguageFeature languageFeature)
    {
        this.field = field;
        this.languageFeature = languageFeature;
    }
    public IEnumerable<AvroElement> Parse()
    {
        JsonElement type = ShouldOr(field.Type, new ArgumentNullException(MissingFieldType()));
        string name = ShouldOr(field.Name, new ArgumentNullException(MissingFieldName()));
        if (IsTypeDefinition(type, out JsonElement typeDefinition))
        {
            if (IsUnionType(typeDefinition))
            {
                yield return new AvroField(name, ShouldOr(typeDefinition.Deserialize<string[]>(), new ArgumentNullException()), languageFeature);
            }
        }
        else
        {
            if (IsUnionType(type))
            {
                int dimensions = 1;
                string? itemType = null;
                string? itemGenericType = null;
                List<string> types = new List<string>();
                foreach (var t in type.EnumerateArray())
                {
                    if (t.ValueKind == JsonValueKind.String)
                    {
                        if (isUnion(types))
                        {
                            types = MakeUnion(types);
                            break;
                        }
                        else
                        {
                            types.Add(ShouldOr(t.GetString(), new ArgumentNullException()));
                        }
                    }
                    else if (t.ValueKind == JsonValueKind.Object)
                    {
                        if (t.TryGetProperty("type", out JsonElement innerType))
                        {
                            if (IsMap(innerType))
                            {
                                if (isUnion(types))
                                {
                                    types = MakeUnion(types);
                                    break;
                                }
                                else
                                {
                                    types.Add("MAP");
                                    if (t.TryGetProperty("values", out JsonElement value))
                                    {
                                        if (value.ValueKind == JsonValueKind.String)
                                        {
                                            itemGenericType = value.GetString();
                                        }
                                        else
                                        {
                                            throw new Exception("");
                                        }
                                    }
                                }
                            }
                            else if (IsArray(innerType))
                            {
                                if (isUnion(types))
                                {
                                    types = MakeUnion(types);
                                    break;
                                }
                                else
                                {
                                    if (IsArrayType(t, ref dimensions, ref itemType, ref itemGenericType))
                                    {
                                        types.Add("ARRAY");
                                    }
                                    else
                                    {
                                        throw new Exception("");
                                    }
                                }
                            }
                            else
                            {
                                if (isUnion(types))
                                {
                                    types = MakeUnion(types);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("");
                        }
                    }
                }
                if (!types.Contains("UNION"))
                {
                    if (types.Contains("MAP"))
                    {
                        yield return new AvroMap(name, ShouldOr(itemGenericType, new ArgumentNullException()), languageFeature);
                    }
                    else if (types.Contains("ARRAY"))
                    {
                        yield return new AvroArray(name, dimensions, ShouldOr(itemType, new ArgumentNullException()), itemGenericType, languageFeature);
                    }
                    else
                    {
                        yield return new AvroField(name, types.ToArray(), languageFeature);
                    }
                }
                else
                {
                    yield return new AvroField(name, types.ToArray(), languageFeature);
                }
            }
        }
    }
    private bool isUnion(List<string> types)
    {
        int uniqueTypes = 0;
        foreach (var i in types)
        {
            if (i != "null")
            {
                uniqueTypes++;
            }
        }
        return uniqueTypes >= 1;
    }
    private List<string> MakeUnion(List<string> types)
    {
        List<string> _types = new List<string>() { "UNION" };
        if (types.Contains("null"))
        {
            _types.Add("NULL");
        }
        return _types;
    }
}