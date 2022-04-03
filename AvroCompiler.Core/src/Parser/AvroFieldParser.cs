using System.Text.Json;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Schema;
using AvroCompiler.Core.Specifications;
using static AvroCompiler.Core.Lexicon;

namespace AvroCompiler.Core.Parser;

public class AvroFieldParser : IAvroParser<AvroElement?>
{
    private readonly Field field;
    private readonly ILanguageFeature languageFeature;
    private readonly HashSet<Func<Field, IAvroParser<AvroElement?>>> handlers;
    public AvroFieldParser(Field field, ILanguageFeature languageFeature)
    {
        this.field = field;
        this.languageFeature = languageFeature;
        handlers = new HashSet<Func<Field, IAvroParser<AvroElement?>>> {
            x => new AvroUnionParser(x, languageFeature)
        };
    }
    public AvroElement? Parse()
    {
        foreach(var handler in handlers) {
            AvroElement? avroElement = handler(field).Parse();
            if(avroElement != null) {
                return avroElement;
            }
        }
        JsonElement type = ShouldOr(field.Type, new ArgumentNullException());
        string name = ShouldOr(field.Name, new ArgumentNullException());
        string typeName = ShouldOr(type.GetString(), new ArgumentException());
        if (IsFieldType(type))
        {
            return new AvroField(name, new string[] { typeName }, languageFeature);
        }
        return null;
    }
}