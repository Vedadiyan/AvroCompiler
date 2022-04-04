using System.Text;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Deserialization;
using AvroCompiler.Core.Schema;
using AvroCompiler.Core.Specifications;
using static AvroCompiler.Core.Lexicon;

namespace AvroCompiler.Core.Parser;

public class AvroAvprParser : IAvroParser<IEnumerable<AvroElement>>
{
    private readonly ILanguageFeature languageFeature;
    private readonly string avprData;
    private readonly HashSet<Func<Schema.Type, IAvroParser<IEnumerable<AvroElement>>>> handlers;
    public AvroAvprParser(Stream stream, ILanguageFeature languageFeature)
    {
        using (StreamReader streamReader = new StreamReader(stream))
        {
            avprData = streamReader.ReadToEnd();
        }
        this.languageFeature = languageFeature;
        handlers = new HashSet<Func<Schema.Type, IAvroParser<IEnumerable<AvroElement>>>> {
            x => new AvroRecordParser(x, languageFeature),
            x => new AvroEnumParser(x, languageFeature),
            x => new AvroFixedParser(x, languageFeature)
        };
    }
    public IEnumerable<AvroElement> Parse()
    {
        StringBuilder preview = new StringBuilder();
        AvprDeserializer avprDeserializer = new AvprDeserializer(avprData);
        Root root = avprDeserializer.GetRoot();
        AvprDeserializer.FlatenReferences(root);
        if (HasValue(root))
        {
            ThrowIfOtherwise(HasValue(root.Types));
            foreach (var type in MustNeverBeNull(root.Types))
            {
                ThrowIfOtherwise(HasValue(type.Name), HasValue(type.TypeName));
                foreach (var handler in handlers)
                {
                    IReadOnlyList<AvroElement> avroElements = handler(type).Parse().ToList();
                    if (avroElements.Count > 0)
                    {
                        foreach (var avroElement in avroElements)
                        {
                            yield return avroElement;
                        }
                        break;
                    }

                }
            }

            if (HasValue(root.Messages))
            {
                foreach (var message in MustNeverBeNull(root.Messages))
                {
                    yield return
                    new AvroMessage(
                        message.Key,
                        MustNeverBeNull(message.Value.Request).ToDictionary(x => MustNeverBeNull(x.Name), x => MustNeverBeNull(x.Type)),
                        MustNeverBeNull(message.Value.Response),
                        MayBeSafelyNull(message.Value.Errors)?.FirstOrDefault(),
                        languageFeature
                    );
                }
            }
        }
    }
}