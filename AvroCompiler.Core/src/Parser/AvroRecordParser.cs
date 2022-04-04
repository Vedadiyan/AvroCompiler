using System.Text.Json;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Schema;
using AvroCompiler.Core.Specifications;
using static AvroCompiler.Core.Lexicon;

namespace AvroCompiler.Core.Parser;

public class AvroRecordParser : IAvroParser<IEnumerable<AvroElement>>
{
    private readonly Schema.Type type;
    private readonly ILanguageFeature languageFeature;
    private HashSet<Func<Field, IAvroParser<IEnumerable<AvroElement>>>> handlers;
    public AvroRecordParser(Schema.Type type, ILanguageFeature languageFeature)
    {
        this.type = type;
        this.languageFeature = languageFeature;
        handlers = new HashSet<Func<Field, IAvroParser<IEnumerable<AvroElement>>>>() {
            x => new AvroUnionParser(x, languageFeature),
            x => new AvroFieldParser(x, languageFeature),
            x => new AvroArrayParser(x, languageFeature),
            x => new AvroEnumParser(x, languageFeature),
            x => new AvroMapParser(x, languageFeature)
        };
    }

    public IEnumerable<AvroElement> Parse()
    {
        if (type.Fields != null)
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
                    IReadOnlyList<AvroElement> parsedAvroElements = handler(field).Parse().ToList();
                    if (parsedAvroElements.Count > 0)
                    {
                        foreach (var parsedAvroElement in parsedAvroElements)
                        {
                            avroElements.Add(name, MustNeverBeNull(parsedAvroElement));
                        }
                        break;
                    }
                }
                if (len == avroElements.Count)
                {
                    Schema.Type? innerTypes = ShouldOr(field.Type, new ArgumentNullException()).Deserialize<Schema.Type>();
                    AvroRecordParser innerParser = new AvroRecordParser(ShouldOr(innerTypes, new ArgumentException()), languageFeature);
                    foreach (var item in innerParser.Parse())
                    {
                        yield return item;
                    }
                    avroElements.Add(name, new AvroField(name, new string[] { name }, languageFeature));
                }

            }
            yield return new AvroRecord(ShouldOr(type.Name, new ArgumentException()), avroElements, type.RawObject.ValueKind != JsonValueKind.Undefined ? type.RawObject.GetRawText() : "", languageFeature);
        }
        else
        {
            yield return new AvroFixed(ShouldOr(type.Name, new ArgumentException()), ShouldOr(type.Size, new ArgumentException()), languageFeature);
        }

    }
}