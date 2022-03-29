using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Contants;

namespace AvroCompiler.Core.Specifications;
public class AvroMap : AvroElement
{
    public AvroTypes ElementType { get; }
    public AvroMap(string name, string elementType, ILanguageFeature languageFeature) : base(name, new string[] { elementType }, languageFeature)
    {
        if (Enum.TryParse<AvroTypes>(elementType.ToUpper(), out AvroTypes type))
        {
            ElementType = type;
        }
        else
        {
            ElementType = AvroTypes.REFERENCE;
        }
    }

    public override string Template()
    {
        if (ElementType != AvroTypes.REFERENCE)
        {
            return LanguageFeature.GetMap(ElementType, Name, new { JsonPropertyName = Name });
        }
        else
        {
            return LanguageFeature.GetMap(TypeNames![0], Name, new { JsonPropertyName = Name });
        }
    }
}