namespace AvroCompiler.Core.Abstraction;
public abstract class AvroElement
{
    protected readonly ILanguageFeature LanguageFeature;
    public string Name { get; }
    public string[]? TypeNames { get; }
    public AvroElement(string name, string[]? typeNames, ILanguageFeature languageFeature)
    {
        Name = name;
        TypeNames = typeNames;
        LanguageFeature = languageFeature;
    }
    public abstract String Template();
}