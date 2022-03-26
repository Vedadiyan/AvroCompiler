using AvroCompiler.Core.Abstraction;

namespace AvroCompiler.Core.Specifications;
public class AvroEnum : AvroElement
{
    public string[] Symbols { get; }
    public AvroEnum(string name, string[] symbols, ILanguageFeature languageFeature) : base(name, null, languageFeature)
    {
        Symbols = symbols;
    }
    public override string Template()
    {
        return LanguageFeature.GetEnum(Name, Symbols, null);
    }
}