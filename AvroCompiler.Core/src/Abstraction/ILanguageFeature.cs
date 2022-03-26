using AvroCompiler.Core.Contants;

namespace AvroCompiler.Core.Abstraction;

public interface ILanguageFeature
{
    string GetType(AvroTypes avroType);
    string GetNamespace(string @namespace);
    string GetImports();
    string GetCodec(string name, string schema, object? options);
    string GetMessage(string name, IReadOnlyDictionary<string, string> request, string response, object? options);
    string GetField(AvroTypes avroType, string name, object? options);
    string GetField(string avroType, AvroTypes actualAvroType, string name, object? options);
    string GetEnum(string name, string[] symbols, object? options);
    string GetFixed(string name, object? options);
    string GetArray(AvroTypes elementType, string name, object? options);
    string GetArray(string elementType, string name, object? options);
    string GetRecord(string name, string[] fields, object? options);
}