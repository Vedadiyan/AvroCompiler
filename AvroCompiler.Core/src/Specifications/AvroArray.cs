using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Contants;
using AvroCompiler.Core.Exceptions;
using static AvroCompiler.Core.Lexicon;

namespace AvroCompiler.Core.Specifications;
public class AvroArray : AvroElement
{
    public int Dimensions { get; }
    public AvroTypes ElementType { get; }
    public string? ElementGenericType { get; }
    public List<string>? Validations {get;}
    public AvroArray(string name, int dimensions, string elementType, string? elementGenericType, List<string>? validations, ILanguageFeature languageFeature) : base(name, new string[] { elementType, MayBeSafelyNull(elementGenericType) }, languageFeature)
    {
        Dimensions = dimensions;
        if (Enum.TryParse<AvroTypes>(elementType.ToUpper(), out AvroTypes type))
        {
            ElementType = type;
        }
        else
        {
            ElementType = AvroTypes.REFERENCE;
        }
        ElementGenericType = elementGenericType;
        Validations = validations;
    }

    public override string Template()
    {
        if (ElementType != AvroTypes.REFERENCE)
        {
            return LanguageFeature.GetArray(Dimensions, ElementType, ElementGenericType, Name, new { JsonPropertyName = Name, Validations = Validations });
        }
        else
        {
            return LanguageFeature.GetArray(Dimensions, MustOr(TypeNames, new AvroTypeException(Name))[0], Name, new { JsonPropertyName = Name, Validations = Validations });
        }
    }
}