using System.Text.Json;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Schema;
using AvroCompiler.Core.Specifications;
using static AvroCompiler.Core.Lexicon;
using static AvroCompiler.Core.Exceptions.AvroCompliationErrorLexicon;
using AvroCompiler.Core.Storage;

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
            JsonElement type = ShouldOr(field.Type, new ArgumentNullException(MissingFieldType()));
            string name = ShouldOr(field.Name, new ArgumentNullException(MissingFieldName()));
            if (IsEnum(type))
            {
                Schema.Type enumType = ShouldOr(type.Deserialize<Schema.Type>(), new ArgumentException());
                AvroEnumParser avroEnumParser = new AvroEnumParser(enumType, languageFeature);
                foreach (var item in avroEnumParser.Parse())
                {
                    yield return item;
                }
                yield return new AvroField(name, new string[] { name }, this.type?.Validations, languageFeature);
            }
        }
        else if (HasValue(type))
        {
            Schema.Type type = ShouldOr(this.type, new ArgumentNullException(MissingFieldType()));
            string name = ShouldOr(type.Name, new ArgumentNullException(MissingFieldName()));
            if (HasValue(type.Symbols))
            {
                Types.Current.Value.RegisterType(name, HighOrderType.ENUM);
                yield return new AvroEnum(name, MustNeverBeNull(type.Symbols).ToArray(), languageFeature);
            }
        }
        else
        {
            throw new Exception("Unpredicted Case in Enum Parser");
        }
    }
}