using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Contants;
using AvroCompiler.Core.Exceptions;
using AvroCompiler.Core.Extensions;

using static AvroCompiler.Core.Lexicon;

namespace AvroCompiler.Core.Specifications;
public class AvroField : AvroElement
{
    public AvroTypes AvroType { get; }
    public string? SelectedType { get; private set; }
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
        bool isNulable = AvroType.IsNullable();
        AvroTypes _type = AvroType;
        if (isNulable)
        {
            _type = _type.ExcludeFlag(AvroTypes.NULL);
        }
        if (_type.IsCompsite())
        {
            AvroType = AvroTypes.UNION;
            if (isNulable)
            {
                AvroType = AvroType.AddFlag(AvroTypes.NULL);
            }
        }
    }

    public override string Template()
    {
        if ((AvroType & AvroTypes.REFERENCE) == AvroTypes.REFERENCE)
        {
            string[] typeNames = MustOr(TypeNames, new AvroTypeException(Name));
            if (typeNames.Length == 1)
            {
                SelectedType = typeNames[0];
                return LanguageFeature.GetField(SelectedType, AvroType, Name, new { JsonPropertyName = Name });
            }
            else if (typeNames.Length == 2)
            {
                SelectedType = ShouldOr(typeNames.FirstOrDefault(x => x != "null"), new AvroTypeException(Name));
                return LanguageFeature.GetField(SelectedType, AvroType, Name, new { JsonPropertyName = Name });
            }
            else
            {
                throw new AvroTooManyTypesException(Name);
            }
        }
        else if (AvroType == AvroTypes.MAP)
        {
            string[] typeNames = MustOr(TypeNames, new AvroTypeException(Name));
            if (typeNames.Length == 2)
            {
                if (Enum.TryParse<AvroTypes>(typeNames[1].ToUpper(), out AvroTypes type))
                {
                    return LanguageFeature.GetMap(type, Name, new { JsonPropertyName = Name });
                }
                else
                {
                    return LanguageFeature.GetMap(typeNames[1], Name, new { JsonPropertyName = Name });
                }
            }
            else
            {
                throw new AvroMapException(MapExceptionMessages.INAVLID_GENRIC_TYPE, Name);
            }
        }
        else
        {
            return LanguageFeature.GetField(AvroType, Name, new { JsonPropertyName = Name, Types = TypeNames });
        }
    }
}