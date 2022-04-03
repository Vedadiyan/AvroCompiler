using System.Text.Json;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Schema;
using AvroCompiler.Core.Specifications;
using static AvroCompiler.Core.Lexicon;

namespace AvroCompiler.Core.Parser;

public class AvroEnumParser : IAvroParser<IEnumerable<AvroElement>>
{
    private readonly Field? field;
    private readonly Schema.Type? type;
    private readonly ILanguageFeature languageFeature;
    public AvroEnumParser(Field field, ILanguageFeature languageFeature)
    {
        this.field = field;
        this.languageFeature = languageFeature;
    }
    public AvroEnumParser(Schema.Type type, ILanguageFeature languageFeature)
    {
        this.type = type;
        this.languageFeature = languageFeature;
    }
    public IEnumerable<AvroElement> Parse()
    {
        if (HasValue(field))
        {
            Field field = ShouldOr(this.field, new ArgumentNullException());
            JsonElement type = ShouldOr(field.Type, new ArgumentNullException());
            string name = ShouldOr(field.Name, new ArgumentNullException());
            if (IsEnum(type))
            {
                Schema.Type enumType = ShouldOr(type.Deserialize<Schema.Type>(), new ArgumentException());
                AvroEnumParser avroEnumParser = new AvroEnumParser(enumType, languageFeature);
                foreach (var item in avroEnumParser.Parse())
                {
                    yield return item;
                }
                yield return new AvroField(name, new string[] { name }, languageFeature);
            }
        }
        else if (HasValue(type))
        {
            Schema.Type type = ShouldOr(this.type, new ArgumentNullException());
            string name = ShouldOr(type.Name, new ArgumentNullException());
            if (HasValue(type.Symbols))
            {
                yield return new AvroEnum(name, MustNeverBeNull(type.Symbols).ToArray(), languageFeature);
            }
        }
        else
        {
            throw new Exception("");
        }
    }
}