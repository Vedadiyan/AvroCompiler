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
                IEnumerable<string> types = from t in type.EnumerateArray()
                                            select
                                               t.ValueKind == JsonValueKind.String ?
                                                   t.GetString() :
                                                   "UNION";
                yield return new AvroField(name, types.ToArray(), languageFeature);
            }
        }
    }
}