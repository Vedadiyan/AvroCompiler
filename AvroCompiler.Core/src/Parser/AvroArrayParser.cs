using System.Text.Json;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Schema;
using AvroCompiler.Core.Specifications;
using static AvroCompiler.Core.Lexicon;

namespace AvroCompiler.Core.Parser;

public class AvroArrayParser : IAvroParser<AvroElement?>
{
    private readonly Field field;
    private readonly ILanguageFeature languageFeature;
    public AvroArrayParser(Field field, ILanguageFeature languageFeature)
    {
        this.field = field;
        this.languageFeature = languageFeature;
    }
    public AvroElement? Parse()
    {
        JsonElement type = ShouldOr(field.Type, new ArgumentNullException());
        string name = ShouldOr(field.Name, new ArgumentNullException());
        int dimensions = 1;
        string? arrayItemType = null;
        string? itemGenericType = null;
        if (IsArrayType(type, ref dimensions, ref arrayItemType, ref itemGenericType))
        {
            return new AvroArray(name, dimensions, ShouldOr(arrayItemType, new ArgumentNullException()), itemGenericType, languageFeature);
        }
        return null;
    }
}