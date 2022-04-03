using System.Text.Json;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Schema;

using static AvroCompiler.Core.Lexicon;

namespace AvroCompiler.Core.Parser;

public class AvroRecordParser : IAvroParser<IEnumerable<AvroElement>>
{
    private readonly Schema.Type type;
    private readonly ILanguageFeature languageFeature;
    private HashSet<Func<Field, IAvroParser<AvroElement?>>> handlers;
    public AvroRecordParser(Schema.Type type, ILanguageFeature languageFeature)
    {
        this.type = type;
        this.languageFeature = languageFeature;
        handlers = new HashSet<Func<Field, IAvroParser<AvroElement?>>>() {
            x => new AvroUnionParser(x, languageFeature),
            x => new AvroFieldParser(x, languageFeature),
            x => new AvroArrayParser(x, languageFeature)
        };
    }

    public IEnumerable<AvroElement> Parse()
    {
        IEnumerable<Field> fields = Lexicon.MustOr(type.Fields, new ArgumentException());
        Dictionary<string, AvroElement> avroElements = new Dictionary<string, AvroElement>();
        foreach (var nullableField in fields)
        {
            Field field = Lexicon.ShouldOr(nullableField, new ArgumentNullException());
            string name;
            JsonElement type;
            if (!HasValue(field.Name) || !HasValue(field.Type)) continue;
            name = MustNeverBeNull(field.Name);
            type = MustNeverBeNull(field.Type);
            int len = avroElements.Count;
            foreach (var handler in handlers)
            {
                AvroElement? parsedAvroElement = handler(field).Parse();
                if (HasValue(parsedAvroElement))
                {
                    avroElements.Add(name, MustNeverBeNull(parsedAvroElement));
                    break;
                }
            }
            if (len == avroElements.Count)
            {
                throw new Exception("");
            }

        }
        throw new NotImplementedException();
    }
}