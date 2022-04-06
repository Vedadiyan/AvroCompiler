using System.Text.Json;
using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Schema;
using AvroCompiler.Core.Specifications;
using static AvroCompiler.Core.Lexicon;
using static AvroCompiler.Core.Exceptions.AvroCompliationErrorLexicon;
using AvroCompiler.Core.Storage;

namespace AvroCompiler.Core.Parser;

public class AvroFixedParser : IAvroParser<IEnumerable<AvroElement>>
{
    private readonly Schema.Type type;
    private readonly ILanguageFeature languageFeature;
    public AvroFixedParser(Schema.Type type, ILanguageFeature languageFeature)
    {
        this.type = type;
        this.languageFeature = languageFeature;
    }
    public IEnumerable<AvroElement> Parse()
    {
        if (type.TypeName == "fixed")
        {
            string name = ShouldOr(type.Name, new ArgumentNullException(MissingFieldName()));
            int size = ShouldOr(type.Size, new ArgumentNullException());
            Types.Current.Value.RegisterType(name, HighOrderType.FIXED);
            yield return new AvroFixed(name, size, languageFeature);
        }
    }
}