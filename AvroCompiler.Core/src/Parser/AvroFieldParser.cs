using System.Text.Json;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Schema;
using AvroCompiler.Core.Specifications;
using static AvroCompiler.Core.Lexicon;
using static AvroCompiler.Core.Exceptions.AvroCompliationErrorLexicon;

namespace AvroCompiler.Core.Parser;

public class AvroFieldParser : IAvroParser<IEnumerable<AvroElement>>
{
    private readonly Field field;
    private readonly ILanguageFeature languageFeature;
    private readonly HashSet<Func<Field, IAvroParser<IEnumerable<AvroElement>>>> handlers;
    public AvroFieldParser(Field field, ILanguageFeature languageFeature)
    {
        this.field = field;
        this.languageFeature = languageFeature;
        handlers = new HashSet<Func<Field, IAvroParser<IEnumerable<AvroElement>>>> {
            x => new AvroUnionParser(x, languageFeature),
            x => new AvroArrayParser(x, languageFeature),
            x => new AvroUnionParser(x, languageFeature),
            x => new AvroEnumParser(x, languageFeature)
        };
    }
    public IEnumerable<AvroElement> Parse()
    {
        bool yielded = false;
        foreach (var handler in handlers)
        {
            IEnumerable<AvroElement> avroElements = handler(field).Parse();
            foreach (var avroElement in avroElements)
            {
                yield return avroElement;
                yielded = true;
            }
        }
        if (!yielded)
        {
            JsonElement type = ShouldOr(field.Type, new ArgumentNullException(MissingFieldType()));
            string name = ShouldOr(field.Name, new ArgumentNullException(MissingFieldName()));
            //string typeName = ShouldOr(type.GetString(), new ArgumentException($"Missing type name for field `{name}`"));
            if (IsFieldType(type))
            {
                yield return new AvroField(name, new string[] { name }, languageFeature);
            }
        }
    }
}