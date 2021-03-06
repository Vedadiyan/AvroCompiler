using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Contants;
using AvroCompiler.Core.Exceptions;
using static AvroCompiler.Core.Lexicon;

namespace AvroCompiler.Core.Specifications;
public class AvroMap : AvroElement
{
    public AvroTypes ElementType { get; }
    public List<string>? Validations { get; }
    public AvroMap(string name, string elementType, List<string>? validations, ILanguageFeature languageFeature) : base(name, new string[] { elementType }, languageFeature)
    {
        if (Enum.TryParse<AvroTypes>(elementType.ToUpper(), out AvroTypes type))
        {
            ElementType = type;
        }
        else
        {
            ElementType = AvroTypes.REFERENCE;
        }
        Validations = validations;
    }

    public override string Template()
    {
        if (ElementType != AvroTypes.REFERENCE)
        {
            return LanguageFeature.GetMap(ElementType, Name, new { JsonPropertyName = Name, Validations = Validations });
        }
        else
        {
            //return LanguageFeature.GetMap(MustOr(TypeNames, new AvroTypeException(Name))[0], Name, new { JsonPropertyName = Name });
            throw new Exception("A map item cannot be a complex type");
        }
    }
}