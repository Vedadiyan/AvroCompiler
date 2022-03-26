using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Contants;

namespace AvroCompiler.Core.Specifications;
public class AvroField : AvroElement
{
    public AvroTypes AvroType { get; }
    public AvroField(string name, string[] typeNames, ILanguageFeature languageFeature) : base(name, typeNames, languageFeature)
    {
        foreach (var typeName in typeNames)
        {
            if (Enum.TryParse<AvroTypes>(typeName.ToUpper(), out AvroTypes avroType))
            {
                AvroType |= avroType;
            }
            else
            {
                AvroType |= AvroTypes.REFERENCE;
            }
        }
        if (AvroType != AvroTypes.REFERENCE)
        {
            bool isNulable = (AvroType & AvroTypes.NULL) == AvroTypes.NULL;
            AvroTypes _type = AvroType;
            if (isNulable)
            {
                _type ^= AvroTypes.NULL;
            }
            if ((int)(_type ^ _type) != 0)
            {
                AvroType = AvroTypes.UNION;
                if (isNulable)
                {
                    AvroType |= AvroTypes.NULL;
                }
            }
        }
    }

    public override string Template()
    {
        if ((AvroType & AvroTypes.REFERENCE) == AvroTypes.REFERENCE)
        {
            return LanguageFeature.GetField(TypeNames![0], AvroType,  Name, new { JsonPropertyName = Name });
        }
        else
        {
            return LanguageFeature.GetField(AvroType, Name, new { JsonPropertyName = Name });
        }
    }
}