using AvroCompiler.Core.Abstraction;
using AvroCompiler.Core.Contants;

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

        {
            bool isNulable = (AvroType & AvroTypes.NULL) == AvroTypes.NULL;
            AvroTypes _type = AvroType;
            if (isNulable)
            {
                _type ^= AvroTypes.NULL;
            }
            if (isComposit((int)_type))
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
            if (TypeNames!.Length == 1)
            {
                SelectedType = TypeNames![0];
                return LanguageFeature.GetField(SelectedType, AvroType, Name, new { JsonPropertyName = Name });
            }
            else if (TypeNames!.Length == 2)
            {
                SelectedType = TypeNames.FirstOrDefault(x => x != "null") ?? throw new ArgumentNullException();
                return LanguageFeature.GetField(SelectedType, AvroType, Name, new { JsonPropertyName = Name });
            }
            else
            {
                throw new ArgumentException();
            }
        }
        else if (AvroType == AvroTypes.MAP)
        {
            if (Enum.TryParse<AvroTypes>(TypeNames![1].ToUpper(), out AvroTypes type))
            {
                return LanguageFeature.GetMap(type, Name, new { JsonPropertyName = Name });
            }
            else
            {
                return LanguageFeature.GetMap(TypeNames[1]!, Name, new { JsonPropertyName = Name });
            }
        }
        else
        {
            return LanguageFeature.GetField(AvroType, Name, new { JsonPropertyName = Name, Types = TypeNames });
        }
    }
    private bool isComposit(int n)
    {
        double _n = n;
        int maxTraverse = 32;
        int currentTraverse = 1;
        while (currentTraverse++ < maxTraverse)
        {
            _n = _n / 2;
            if (_n % 1 != 0)
            {
                return true;
            }
            else if (_n == 1)
            {
                return false;
            }
        }
        throw new Exception("");
    }
}