using System.Text.Json;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Schema;
using AvroCompiler.Core.Specifications;
using static AvroCompiler.Core.Lexicon;
using static AvroCompiler.Core.Exceptions.AvroCompliationErrorLexicon;

namespace AvroCompiler.Core.Parser;

public class AvroMapParser : IAvroParser<IEnumerable<AvroElement>>
{
    private readonly Field field;
    private readonly ILanguageFeature languageFeature;
    public AvroMapParser(Field field, ILanguageFeature languageFeature)
    {
        this.field = field;
        this.languageFeature = languageFeature;
    }
    public IEnumerable<AvroElement> Parse()
    {
        JsonElement type = ShouldOr(field.Type, new ArgumentNullException());
        string name = ShouldOr(field.Name, new ArgumentNullException());
        if (IsTypeDefinition(type, out JsonElement typeDefinition))
        {
            if (IsMap(typeDefinition))
            {
                Schema.Type _type = type.Deserialize<Schema.Type>()!;
                if (type.TryGetProperty("values", out JsonElement values))
                {
                    yield return new AvroMap(name, ShouldOr(values.GetString(), new ArgumentNullException()), _type.Validations, languageFeature);
                }
            }
        }
        else
        {
            if (IsMap(type))
            {
                Schema.Type _type = type.Deserialize<Schema.Type>()!;
                if (type.TryGetProperty("values", out JsonElement values))
                {
                    yield return new AvroMap(name, ShouldOr(values.GetString(), new ArgumentNullException()), _type.Validations, languageFeature);
                }
            }
        }
    }
}