using System.Text.Json;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Schema;
using AvroCompiler.Core.Specifications;
using static AvroCompiler.Core.Lexicon;
using static AvroCompiler.Core.Exceptions.AvroCompliationErrorLexicon;
namespace AvroCompiler.Core.Parser;

public class AvroArrayParser : IAvroParser<IEnumerable<AvroElement>>
{
    private readonly Field field;
    private readonly ILanguageFeature languageFeature;
    public AvroArrayParser(Field field, ILanguageFeature languageFeature)
    {
        this.field = field;
        this.languageFeature = languageFeature;
    }
    public IEnumerable<AvroElement> Parse()
    {
        JsonElement type = ShouldOr(field.Type, new ArgumentNullException(MissingFieldType()));
        string name = ShouldOr(field.Name, new ArgumentNullException(MissingFieldName()));
        int dimensions = 1;
        string? arrayItemType = null;
        string? itemGenericType = null;
        if (IsArrayType(type, ref dimensions, ref arrayItemType, ref itemGenericType))
        {
            Schema.Type _type = type.Deserialize<Schema.Type>()!;
            yield return new AvroArray(name, dimensions, ShouldOr(arrayItemType, new ArgumentNullException(ExpectingArrayType(name))), itemGenericType, _type.Validations, languageFeature);
        }
    }
}