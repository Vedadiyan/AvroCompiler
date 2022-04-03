using System.Text.Json;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Schema;
using AvroCompiler.Core.Specifications;
using static AvroCompiler.Core.Lexicon;

namespace AvroCompiler.Core.Parser;

public class AvroUnionParser : IAvroParser<AvroElement?>
{
    private readonly Field field;
    private readonly ILanguageFeature languageFeature;
    public AvroUnionParser(Field field, ILanguageFeature languageFeature)
    {
        this.field = field;
        this.languageFeature = languageFeature;
    }
    public AvroElement? Parse()
    {
        string name = ShouldOr(field.Name, new ArgumentNullException());
        JsonElement type = ShouldOr(field.Type, new ArgumentNullException());
        if (IsUnionType(type))
        {
            IEnumerable<string> types = from t in type.EnumerateArray()
                                        select
                                           t.ValueKind == JsonValueKind.String ?
                                               t.GetString() :
                                               "UNION";
            return new AvroField(name, types.ToArray(), languageFeature);
        }
        return null;
    }
}