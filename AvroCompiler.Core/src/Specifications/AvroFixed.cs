using AvroCompiler.Core.Abstraction;

namespace AvroCompiler.Core.Specifications;
public class AvroFixed : AvroElement
{
    public int Size { get; }
    public AvroFixed(string name, int size, ILanguageFeature languageFeature) : base(name, null, languageFeature)
    {
        Size = size;
    }
    public override string Template()
    {
        return LanguageFeature.GetFixed(Name, null);
    }
}