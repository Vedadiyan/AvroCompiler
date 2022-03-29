using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Contants;

namespace AvroCompiler.Core.Specifications;
public class AvroArray : AvroElement
{
    public AvroTypes ElementType { get; }
    public AvroArray(string name, string elementType, ILanguageFeature languageFeature) : base(name, new string[] { elementType }, languageFeature)
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
            return LanguageFeature.GetArray(ElementType, Name, new { JsonPropertyName = Name });
        }
        else
        {
            return LanguageFeature.GetArray(TypeNames![0], Name, new { JsonPropertyName = Name });
        }
    }
}