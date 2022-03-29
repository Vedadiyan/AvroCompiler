using AvroCompiler.Core.Abstraction;

namespace AvroCompiler.Core.Specifications;
public class AvroRecord : AvroElement
{
    public IReadOnlyDictionary<string, AvroElement> Fields { get; }
    public string RawJson { get; }
    public AvroRecord(string name, Dictionary<string, AvroElement> fields, string rawJson, ILanguageFeature languageFeature) : base(name, null, languageFeature)
    {
        Fields = fields;
        RawJson = rawJson;
    }
    public override string Template()
    {
        return LanguageFeature.GetRecord(Name, Fields.Select(x => x.Value).ToArray(), null);
    }
}
