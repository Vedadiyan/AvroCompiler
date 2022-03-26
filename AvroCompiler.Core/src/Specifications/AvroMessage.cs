using AvroCompiler.Core.Abstraction;

namespace AvroCompiler.Core.Specifications;

public class AvroMessage : AvroElement
{
    public IReadOnlyDictionary<string, string> Request { get; }
    public string Response { get; }
    public string? Error { get; }
    public AvroMessage(string name, Dictionary<string, string> request, string response, string? error, ILanguageFeature languageFeature) : base(name, null, languageFeature)
    {
        Request = request;
        Response = response;
        Error = error;
    }

    public override string Template()
    {
        return LanguageFeature.GetMessage(Name, Request, Response, new { Error = Error});
    }
}